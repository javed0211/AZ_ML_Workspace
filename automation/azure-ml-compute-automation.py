#!/usr/bin/env python3
"""
Azure ML Compute Instance Automation Script
Automates the process of launching Linux compute instances and setting up VS Code remote connections.
"""

import os
import sys
import json
import time
import subprocess
import logging
import argparse
from pathlib import Path
from typing import Dict, List, Optional, Tuple
from datetime import datetime, timedelta

try:
    from azure.ai.ml import MLClient
    from azure.ai.ml.entities import ComputeInstance
    from azure.identity import DefaultAzureCredential, InteractiveBrowserCredential
    from azure.core.exceptions import ResourceNotFoundError, ResourceExistsError
    import paramiko
    import watchdog
    from watchdog.observers import Observer
    from watchdog.events import FileSystemEventHandler
except ImportError as e:
    print(f"Missing required packages. Please install: {e}")
    print("Run: pip install azure-ai-ml paramiko watchdog")
    sys.exit(1)

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('azure_ml_automation.log'),
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger(__name__)

class AzureMLComputeAutomation:
    """Main automation class for Azure ML compute instance management."""
    
    def __init__(self, config_path: str = None):
        """Initialize the automation with configuration."""
        self.config_path = config_path or "config/azure-ml-automation-config.json"
        self.config = self._load_config()
        self.ml_client = None
        self.compute_instance = None
        self.ssh_client = None
        self.file_sync_observer = None
        
    def _load_config(self) -> Dict:
        """Load configuration from JSON file."""
        try:
            with open(self.config_path, 'r') as f:
                config = json.load(f)
            logger.info(f"Configuration loaded from {self.config_path}")
            return config
        except FileNotFoundError:
            logger.error(f"Configuration file not found: {self.config_path}")
            self._create_default_config()
            return self._load_config()
    
    def _create_default_config(self):
        """Create a default configuration file."""
        default_config = {
            "azure": {
                "subscription_id": "your-subscription-id",
                "resource_group": "your-resource-group",
                "workspace_name": "your-workspace-name",
                "location": "eastus"
            },
            "compute_instance": {
                "name": "ml-compute-instance",
                "vm_size": "Standard_DS3_v2",
                "admin_username": "azureuser",
                "enable_ssh": True,
                "ssh_public_key": "~/.ssh/id_rsa.pub",
                "idle_time_before_shutdown": 30,
                "enable_node_public_ip": True
            },
            "vscode": {
                "remote_extensions": [
                    "ms-python.python",
                    "ms-toolsai.jupyter",
                    "ms-vscode-remote.remote-ssh",
                    "ms-azuretools.vscode-azureml"
                ],
                "settings_sync": True,
                "local_workspace_path": "./",
                "remote_workspace_path": "/home/azureuser/workspace"
            },
            "file_sync": {
                "enabled": True,
                "sync_patterns": ["*.py", "*.ipynb", "*.json", "*.md", "*.txt"],
                "exclude_patterns": [".git/*", "__pycache__/*", "*.pyc", ".vscode/*"],
                "bidirectional": True,
                "real_time": True
            },
            "security": {
                "allowed_ip_ranges": ["0.0.0.0/0"],  # Restrict this in production
                "ssh_key_type": "rsa",
                "ssh_key_bits": 2048
            }
        }
        
        # Create config directory if it doesn't exist
        os.makedirs(os.path.dirname(self.config_path), exist_ok=True)
        
        with open(self.config_path, 'w') as f:
            json.dump(default_config, f, indent=2)
        
        logger.info(f"Default configuration created at {self.config_path}")
        logger.warning("Please update the configuration with your Azure details before running again.")
    
    def authenticate(self) -> bool:
        """Authenticate with Azure and create ML client."""
        try:
            # Try default credential first (managed identity, environment variables, etc.)
            credential = DefaultAzureCredential()
            
            self.ml_client = MLClient(
                credential=credential,
                subscription_id=self.config["azure"]["subscription_id"],
                resource_group_name=self.config["azure"]["resource_group"],
                workspace_name=self.config["azure"]["workspace_name"]
            )
            
            # Test the connection
            workspace = self.ml_client.workspaces.get(self.config["azure"]["workspace_name"])
            logger.info(f"Successfully authenticated with workspace: {workspace.name}")
            return True
            
        except Exception as e:
            logger.warning(f"Default authentication failed: {e}")
            
            # Fallback to interactive browser authentication
            try:
                credential = InteractiveBrowserCredential()
                self.ml_client = MLClient(
                    credential=credential,
                    subscription_id=self.config["azure"]["subscription_id"],
                    resource_group_name=self.config["azure"]["resource_group"],
                    workspace_name=self.config["azure"]["workspace_name"]
                )
                
                workspace = self.ml_client.workspaces.get(self.config["azure"]["workspace_name"])
                logger.info(f"Successfully authenticated with workspace: {workspace.name}")
                return True
                
            except Exception as e:
                logger.error(f"Authentication failed: {e}")
                return False
    
    def generate_ssh_key(self) -> Tuple[str, str]:
        """Generate SSH key pair if not exists."""
        ssh_dir = Path.home() / ".ssh"
        ssh_dir.mkdir(exist_ok=True)
        
        private_key_path = ssh_dir / "id_rsa"
        public_key_path = ssh_dir / "id_rsa.pub"
        
        if not private_key_path.exists():
            logger.info("Generating SSH key pair...")
            subprocess.run([
                "ssh-keygen", "-t", "rsa", "-b", "2048",
                "-f", str(private_key_path),
                "-N", "",  # No passphrase
                "-C", f"azureml-automation-{datetime.now().strftime('%Y%m%d')}"
            ], check=True)
            logger.info(f"SSH key pair generated: {private_key_path}")
        
        with open(public_key_path, 'r') as f:
            public_key = f.read().strip()
        
        return str(private_key_path), public_key
    
    def create_compute_instance(self) -> bool:
        """Create or get existing compute instance."""
        try:
            compute_name = self.config["compute_instance"]["name"]
            
            # Check if compute instance already exists
            try:
                self.compute_instance = self.ml_client.compute.get(compute_name)
                logger.info(f"Compute instance '{compute_name}' already exists")
                
                # Check if it's running
                if self.compute_instance.state.lower() == "running":
                    logger.info("Compute instance is already running")
                    return True
                elif self.compute_instance.state.lower() in ["stopped", "stopping"]:
                    logger.info("Starting existing compute instance...")
                    self.ml_client.compute.begin_start(compute_name).wait()
                    return True
                    
            except ResourceNotFoundError:
                logger.info(f"Creating new compute instance: {compute_name}")
                
                # Generate SSH key if needed
                private_key_path, public_key = self.generate_ssh_key()
                
                # Create compute instance configuration
                compute_config = ComputeInstance(
                    name=compute_name,
                    size=self.config["compute_instance"]["vm_size"],
                    ssh_public_access_enabled=self.config["compute_instance"]["enable_ssh"],
                    ssh_settings={
                        "admin_username": self.config["compute_instance"]["admin_username"],
                        "ssh_public_key": public_key
                    } if self.config["compute_instance"]["enable_ssh"] else None,
                    idle_time_before_shutdown=self.config["compute_instance"]["idle_time_before_shutdown"],
                    enable_node_public_ip=self.config["compute_instance"]["enable_node_public_ip"]
                )
                
                # Create the compute instance
                operation = self.ml_client.compute.begin_create_or_update(compute_config)
                self.compute_instance = operation.result()
                
                logger.info(f"Compute instance '{compute_name}' created successfully")
                return True
                
        except Exception as e:
            logger.error(f"Failed to create compute instance: {e}")
            return False
    
    def wait_for_compute_ready(self, timeout_minutes: int = 15) -> bool:
        """Wait for compute instance to be ready."""
        compute_name = self.config["compute_instance"]["name"]
        start_time = datetime.now()
        timeout = timedelta(minutes=timeout_minutes)
        
        logger.info(f"Waiting for compute instance to be ready (timeout: {timeout_minutes} minutes)...")
        
        while datetime.now() - start_time < timeout:
            try:
                self.compute_instance = self.ml_client.compute.get(compute_name)
                state = self.compute_instance.state.lower()
                
                logger.info(f"Compute instance state: {state}")
                
                if state == "running":
                    logger.info("Compute instance is ready!")
                    return True
                elif state in ["failed", "error"]:
                    logger.error("Compute instance failed to start")
                    return False
                
                time.sleep(30)  # Wait 30 seconds before checking again
                
            except Exception as e:
                logger.error(f"Error checking compute instance state: {e}")
                time.sleep(30)
        
        logger.error("Timeout waiting for compute instance to be ready")
        return False
    
    def get_ssh_connection_info(self) -> Tuple[str, int, str]:
        """Get SSH connection information for the compute instance."""
        try:
            compute_name = self.config["compute_instance"]["name"]
            self.compute_instance = self.ml_client.compute.get(compute_name)
            
            # Get connection info
            ssh_info = self.compute_instance.ssh_settings
            if not ssh_info:
                raise ValueError("SSH is not enabled for this compute instance")
            
            hostname = ssh_info.public_ip_address or ssh_info.private_ip_address
            port = ssh_info.ssh_port or 22
            username = ssh_info.admin_username
            
            logger.info(f"SSH connection info - Host: {hostname}, Port: {port}, User: {username}")
            return hostname, port, username
            
        except Exception as e:
            logger.error(f"Failed to get SSH connection info: {e}")
            return None, None, None
    
    def setup_ssh_connection(self) -> bool:
        """Setup SSH connection to the compute instance."""
        try:
            hostname, port, username = self.get_ssh_connection_info()
            if not all([hostname, port, username]):
                return False
            
            # Setup SSH client
            self.ssh_client = paramiko.SSHClient()
            self.ssh_client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
            
            # Get private key path
            private_key_path = Path.home() / ".ssh" / "id_rsa"
            private_key = paramiko.RSAKey.from_private_key_file(str(private_key_path))
            
            # Connect
            logger.info(f"Connecting to {hostname}:{port} as {username}...")
            self.ssh_client.connect(
                hostname=hostname,
                port=port,
                username=username,
                pkey=private_key,
                timeout=30
            )
            
            logger.info("SSH connection established successfully")
            return True
            
        except Exception as e:
            logger.error(f"Failed to setup SSH connection: {e}")
            return False
    
    def setup_remote_environment(self) -> bool:
        """Setup the remote environment for development."""
        try:
            if not self.ssh_client:
                logger.error("SSH connection not established")
                return False
            
            commands = [
                # Update system
                "sudo apt-get update -y",
                
                # Install essential tools
                "sudo apt-get install -y git curl wget unzip",
                
                # Create workspace directory
                f"mkdir -p {self.config['vscode']['remote_workspace_path']}",
                
                # Install Python packages
                "pip install --upgrade pip",
                "pip install jupyter notebook jupyterlab",
                "pip install pandas numpy matplotlib seaborn scikit-learn",
                "pip install azureml-sdk azureml-core",
                
                # Setup Git (if not already configured)
                "git config --global init.defaultBranch main || true",
                
                # Create a simple test script
                f"echo 'print(\"Remote environment setup complete!\")' > {self.config['vscode']['remote_workspace_path']}/test.py"
            ]
            
            for cmd in commands:
                logger.info(f"Executing: {cmd}")
                stdin, stdout, stderr = self.ssh_client.exec_command(cmd)
                
                # Wait for command to complete
                exit_status = stdout.channel.recv_exit_status()
                if exit_status != 0:
                    error_output = stderr.read().decode()
                    logger.warning(f"Command failed with exit status {exit_status}: {error_output}")
                else:
                    output = stdout.read().decode()
                    if output.strip():
                        logger.info(f"Output: {output.strip()}")
            
            logger.info("Remote environment setup completed")
            return True
            
        except Exception as e:
            logger.error(f"Failed to setup remote environment: {e}")
            return False
    
    def create_vscode_ssh_config(self) -> bool:
        """Create VS Code SSH configuration."""
        try:
            hostname, port, username = self.get_ssh_connection_info()
            if not all([hostname, port, username]):
                return False
            
            ssh_config_dir = Path.home() / ".ssh"
            ssh_config_file = ssh_config_dir / "config"
            
            # Create SSH config entry
            config_entry = f"""
# Azure ML Compute Instance
Host azureml-compute
    HostName {hostname}
    Port {port}
    User {username}
    IdentityFile ~/.ssh/id_rsa
    StrictHostKeyChecking no
    UserKnownHostsFile /dev/null
    ServerAliveInterval 60
    ServerAliveCountMax 3
"""
            
            # Read existing config
            existing_config = ""
            if ssh_config_file.exists():
                with open(ssh_config_file, 'r') as f:
                    existing_config = f.read()
            
            # Remove existing azureml-compute entry if present
            lines = existing_config.split('\n')
            filtered_lines = []
            skip_section = False
            
            for line in lines:
                if line.strip().startswith('Host azureml-compute'):
                    skip_section = True
                    continue
                elif line.strip().startswith('Host ') and skip_section:
                    skip_section = False
                
                if not skip_section:
                    filtered_lines.append(line)
            
            # Add new config entry
            new_config = '\n'.join(filtered_lines) + config_entry
            
            with open(ssh_config_file, 'w') as f:
                f.write(new_config)
            
            # Set proper permissions
            os.chmod(ssh_config_file, 0o600)
            
            logger.info(f"SSH config updated: {ssh_config_file}")
            logger.info("You can now connect using: ssh azureml-compute")
            return True
            
        except Exception as e:
            logger.error(f"Failed to create SSH config: {e}")
            return False
    
    def launch_vscode_remote(self) -> bool:
        """Launch VS Code with remote SSH connection."""
        try:
            # Create VS Code settings for remote
            self._create_vscode_remote_settings()
            
            # Launch VS Code with remote SSH
            remote_path = self.config['vscode']['remote_workspace_path']
            cmd = [
                "code",
                "--new-window",
                "--remote", f"ssh-remote+azureml-compute",
                remote_path
            ]
            
            logger.info("Launching VS Code with remote SSH connection...")
            logger.info(f"Command: {' '.join(cmd)}")
            
            process = subprocess.Popen(
                cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True
            )
            
            # Wait a bit to see if VS Code starts successfully
            time.sleep(5)
            
            if process.poll() is None:
                logger.info("VS Code launched successfully with remote connection")
                return True
            else:
                stdout, stderr = process.communicate()
                logger.error(f"VS Code failed to launch: {stderr}")
                return False
                
        except Exception as e:
            logger.error(f"Failed to launch VS Code remote: {e}")
            return False
    
    def _create_vscode_remote_settings(self):
        """Create VS Code settings for remote development."""
        try:
            # VS Code settings for remote
            remote_settings = {
                "python.defaultInterpreterPath": "/opt/miniconda/envs/azureml_py38/bin/python",
                "python.terminal.activateEnvironment": True,
                "jupyter.askForKernelRestart": False,
                "jupyter.alwaysTrustNotebooks": True,
                "terminal.integrated.defaultProfile.linux": "bash",
                "files.watcherExclude": {
                    "**/.git/objects/**": True,
                    "**/.git/subtree-cache/**": True,
                    "**/node_modules/*/**": True,
                    "**/__pycache__/**": True
                }
            }
            
            # Create remote settings directory
            remote_settings_dir = Path.home() / ".vscode-server" / "data" / "Machine"
            remote_settings_dir.mkdir(parents=True, exist_ok=True)
            
            settings_file = remote_settings_dir / "settings.json"
            with open(settings_file, 'w') as f:
                json.dump(remote_settings, f, indent=2)
            
            logger.info("VS Code remote settings created")
            
        except Exception as e:
            logger.warning(f"Failed to create VS Code remote settings: {e}")

class FileSyncHandler(FileSystemEventHandler):
    """File system event handler for real-time file synchronization."""
    
    def __init__(self, ssh_client, local_path: str, remote_path: str, sync_patterns: List[str], exclude_patterns: List[str]):
        self.ssh_client = ssh_client
        self.local_path = Path(local_path)
        self.remote_path = remote_path
        self.sync_patterns = sync_patterns
        self.exclude_patterns = exclude_patterns
        self.logger = logging.getLogger(f"{__name__}.FileSyncHandler")
    
    def should_sync_file(self, file_path: str) -> bool:
        """Check if file should be synchronized."""
        file_path = Path(file_path)
        
        # Check exclude patterns
        for pattern in self.exclude_patterns:
            if file_path.match(pattern):
                return False
        
        # Check include patterns
        for pattern in self.sync_patterns:
            if file_path.match(pattern):
                return True
        
        return False
    
    def sync_file_to_remote(self, local_file_path: str):
        """Sync a file to remote."""
        try:
            if not self.should_sync_file(local_file_path):
                return
            
            local_path = Path(local_file_path)
            relative_path = local_path.relative_to(self.local_path)
            remote_file_path = f"{self.remote_path}/{relative_path}"
            
            # Create remote directory if needed
            remote_dir = os.path.dirname(remote_file_path)
            self.ssh_client.exec_command(f"mkdir -p {remote_dir}")
            
            # Transfer file
            sftp = self.ssh_client.open_sftp()
            sftp.put(str(local_path), remote_file_path)
            sftp.close()
            
            self.logger.info(f"Synced to remote: {relative_path}")
            
        except Exception as e:
            self.logger.error(f"Failed to sync file to remote: {e}")
    
    def on_modified(self, event):
        if not event.is_directory:
            self.sync_file_to_remote(event.src_path)
    
    def on_created(self, event):
        if not event.is_directory:
            self.sync_file_to_remote(event.src_path)

def main():
    """Main function to run the automation."""
    parser = argparse.ArgumentParser(description="Azure ML Compute Instance Automation")
    parser.add_argument("--config", help="Path to configuration file")
    parser.add_argument("--create-only", action="store_true", help="Only create compute instance, don't setup VS Code")
    parser.add_argument("--vscode-only", action="store_true", help="Only setup VS Code connection (assume compute exists)")
    parser.add_argument("--sync-only", action="store_true", help="Only start file synchronization")
    parser.add_argument("--cleanup", action="store_true", help="Cleanup and delete compute instance")
    
    args = parser.parse_args()
    
    # Initialize automation
    automation = AzureMLComputeAutomation(args.config)
    
    try:
        if args.cleanup:
            logger.info("Cleaning up compute instance...")
            if automation.authenticate():
                compute_name = automation.config["compute_instance"]["name"]
                automation.ml_client.compute.begin_delete(compute_name).wait()
                logger.info("Compute instance deleted successfully")
            return
        
        # Authenticate with Azure
        if not automation.authenticate():
            logger.error("Authentication failed. Exiting.")
            return
        
        if not args.vscode_only and not args.sync_only:
            # Create and start compute instance
            if not automation.create_compute_instance():
                logger.error("Failed to create compute instance. Exiting.")
                return
            
            # Wait for compute to be ready
            if not automation.wait_for_compute_ready():
                logger.error("Compute instance not ready. Exiting.")
                return
        
        if args.create_only:
            logger.info("Compute instance creation completed.")
            return
        
        # Setup SSH connection
        if not automation.setup_ssh_connection():
            logger.error("Failed to setup SSH connection. Exiting.")
            return
        
        if not args.sync_only:
            # Setup remote environment
            if not automation.setup_remote_environment():
                logger.warning("Remote environment setup had issues, but continuing...")
            
            # Create SSH config for VS Code
            if not automation.create_vscode_ssh_config():
                logger.error("Failed to create SSH config. Exiting.")
                return
            
            # Launch VS Code
            if not automation.launch_vscode_remote():
                logger.error("Failed to launch VS Code remote. You can manually connect using 'ssh azureml-compute'")
        
        # Setup file synchronization if enabled
        if automation.config["file_sync"]["enabled"] and automation.config["file_sync"]["real_time"]:
            logger.info("Starting real-time file synchronization...")
            
            sync_handler = FileSyncHandler(
                automation.ssh_client,
                automation.config["vscode"]["local_workspace_path"],
                automation.config["vscode"]["remote_workspace_path"],
                automation.config["file_sync"]["sync_patterns"],
                automation.config["file_sync"]["exclude_patterns"]
            )
            
            observer = Observer()
            observer.schedule(
                sync_handler,
                automation.config["vscode"]["local_workspace_path"],
                recursive=True
            )
            observer.start()
            
            logger.info("File synchronization started. Press Ctrl+C to stop.")
            
            try:
                while True:
                    time.sleep(1)
            except KeyboardInterrupt:
                observer.stop()
                logger.info("File synchronization stopped.")
            
            observer.join()
        
        logger.info("Automation completed successfully!")
        logger.info("VS Code should now be connected to your Azure ML compute instance.")
        logger.info("You can also manually connect using: ssh azureml-compute")
        
    except KeyboardInterrupt:
        logger.info("Automation interrupted by user.")
    except Exception as e:
        logger.error(f"Automation failed: {e}")
    finally:
        # Cleanup
        if automation.ssh_client:
            automation.ssh_client.close()

if __name__ == "__main__":
    main()