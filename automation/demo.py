#!/usr/bin/env python3
"""
Demo script for Azure ML Compute Instance Automation
Shows how to use the automation programmatically and demonstrates key features.
"""

import os
import sys
import time
import json
import logging
from pathlib import Path

# Add current directory to path to import our automation module
sys.path.append('.')

# For demo purposes, we'll create a mock automation class if the real one isn't available
try:
    from azure_ml_compute_automation import AzureMLComputeAutomation
    MOCK_MODE = False
except ImportError:
    print("ℹ️  Running in demo mode (automation module not imported)")
    MOCK_MODE = True
    
    class AzureMLComputeAutomation:
        def __init__(self, config_path=None):
            self.config_path = config_path or "config/azure-ml-automation-config.json"
            self.config = {
                "azure": {
                    "subscription_id": "demo-subscription-id",
                    "resource_group": "demo-resource-group",
                    "workspace_name": "demo-workspace",
                    "location": "eastus"
                },
                "compute_instance": {
                    "name": "demo-compute-instance",
                    "vm_size": "Standard_DS3_v2",
                    "admin_username": "azureuser",
                    "idle_time_before_shutdown": 30,
                    "enable_node_public_ip": True
                },
                "file_sync": {
                    "sync_patterns": ["*.py", "*.ipynb", "*.json", "*.md", "*.txt"],
                    "exclude_patterns": [".git/*", "__pycache__/*", "*.pyc", ".vscode/*"]
                }
            }
        
        def generate_ssh_key(self):
            return "/home/user/.ssh/id_rsa", "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQ... (demo key)"
        
        def _create_vscode_remote_settings(self):
            pass

# Configure logging for demo
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

def demo_configuration_setup():
    """Demonstrate configuration setup and validation."""
    print("\n🔧 DEMO: Configuration Setup")
    print("=" * 50)
    
    config_path = "config/azure-ml-automation-config.json"
    
    # Create automation instance
    automation = AzureMLComputeAutomation(config_path)
    
    print(f"✓ Configuration loaded from: {config_path}")
    print(f"✓ Workspace: {automation.config['azure']['workspace_name']}")
    print(f"✓ Resource Group: {automation.config['azure']['resource_group']}")
    print(f"✓ Compute Instance: {automation.config['compute_instance']['name']}")
    print(f"✓ VM Size: {automation.config['compute_instance']['vm_size']}")
    
    return automation

def demo_ssh_key_generation():
    """Demonstrate SSH key generation."""
    print("\n🔑 DEMO: SSH Key Generation")
    print("=" * 50)
    
    automation = AzureMLComputeAutomation()
    
    try:
        private_key_path, public_key = automation.generate_ssh_key()
        print(f"✓ SSH key pair ready")
        print(f"✓ Private key: {private_key_path}")
        print(f"✓ Public key length: {len(public_key)} characters")
        
        # Show first few characters of public key
        print(f"✓ Public key preview: {public_key[:50]}...")
        
    except Exception as e:
        print(f"❌ SSH key generation failed: {e}")

def demo_vscode_config():
    """Demonstrate VS Code configuration creation."""
    print("\n💻 DEMO: VS Code Configuration")
    print("=" * 50)
    
    automation = AzureMLComputeAutomation()
    
    # Simulate SSH connection info
    hostname = "20.123.45.67"  # Example IP
    port = 22
    username = "azureuser"
    
    print(f"✓ Simulating SSH connection to: {username}@{hostname}:{port}")
    
    # Create SSH config entry (demo mode - don't actually write)
    config_entry = f"""
# Azure ML Compute Instance (DEMO)
Host azureml-compute-demo
    HostName {hostname}
    Port {port}
    User {username}
    IdentityFile ~/.ssh/id_rsa
    StrictHostKeyChecking no
    UserKnownHostsFile /dev/null
    ServerAliveInterval 60
    ServerAliveCountMax 3
"""
    
    print("✓ SSH config entry would be:")
    print(config_entry)
    
    # Show VS Code remote settings
    automation._create_vscode_remote_settings()
    print("✓ VS Code remote settings configured")

def demo_file_sync_patterns():
    """Demonstrate file synchronization patterns."""
    print("\n📁 DEMO: File Synchronization Patterns")
    print("=" * 50)
    
    automation = AzureMLComputeAutomation()
    config = automation.config
    
    print("✓ Files that WILL be synchronized:")
    for pattern in config['file_sync']['sync_patterns']:
        print(f"   • {pattern}")
    
    print("\n✓ Files that will be EXCLUDED:")
    for pattern in config['file_sync']['exclude_patterns']:
        print(f"   • {pattern}")
    
    # Test some example files
    test_files = [
        "notebook.ipynb",
        "script.py",
        "data.csv",
        "config.json",
        "README.md",
        "__pycache__/module.pyc",
        ".git/config",
        "temp/file.tmp"
    ]
    
    print("\n✓ Example file sync decisions:")
    
    # Simulate file sync handler logic
    if not MOCK_MODE:
        from azure_ml_compute_automation import FileSyncHandler
        
        # Create a mock handler for testing
        class MockSSHClient:
            pass
        
        handler = FileSyncHandler(
            MockSSHClient(),
            "/local/path",
            "/remote/path",
            config['file_sync']['sync_patterns'],
            config['file_sync']['exclude_patterns']
        )
    else:
        # Mock handler for demo mode
        class MockFileSyncHandler:
            def __init__(self, ssh_client, local_path, remote_path, sync_patterns, exclude_patterns):
                self.sync_patterns = sync_patterns
                self.exclude_patterns = exclude_patterns
            
            def should_sync_file(self, file_path):
                from pathlib import Path
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
        
        handler = MockFileSyncHandler(
            None,
            "/local/path",
            "/remote/path",
            config['file_sync']['sync_patterns'],
            config['file_sync']['exclude_patterns']
        )
    
    for file_path in test_files:
        should_sync = handler.should_sync_file(file_path)
        status = "✓ SYNC" if should_sync else "✗ SKIP"
        print(f"   {status}: {file_path}")

def demo_integration_with_existing_notebook():
    """Demonstrate integration with existing notebook from your project."""
    print("\n📓 DEMO: Integration with Existing Notebook")
    print("=" * 50)
    
    # Check for existing notebook
    notebook_path = Path("../NewNotebook/azure_ml_project.ipynb")
    
    if notebook_path.exists():
        print(f"✓ Found existing notebook: {notebook_path}")
        
        try:
            with open(notebook_path, 'r') as f:
                notebook_content = json.load(f)
            
            cell_count = len(notebook_content.get('cells', []))
            print(f"✓ Notebook has {cell_count} cells")
            
            # Show cell types
            cell_types = {}
            for cell in notebook_content.get('cells', []):
                cell_type = cell.get('cell_type', 'unknown')
                cell_types[cell_type] = cell_types.get(cell_type, 0) + 1
            
            print("✓ Cell types:")
            for cell_type, count in cell_types.items():
                print(f"   • {cell_type}: {count}")
            
            print("\n✓ This notebook would be automatically synced to the remote compute instance")
            print("✓ You could edit it locally and changes would sync in real-time")
            
        except Exception as e:
            print(f"❌ Error reading notebook: {e}")
    else:
        print(f"ℹ️  Notebook not found at: {notebook_path}")
        print("ℹ️  The automation would work with any .ipynb files in your workspace")

def demo_monitoring_and_health_checks():
    """Demonstrate monitoring and health check capabilities."""
    print("\n📊 DEMO: Monitoring and Health Checks")
    print("=" * 50)
    
    automation = AzureMLComputeAutomation()
    
    # Simulate health check results
    health_checks = [
        ("Azure Authentication", "✅ HEALTHY", "Successfully authenticated with Azure"),
        ("Compute Instance", "✅ RUNNING", "Instance is active and responsive"),
        ("SSH Connection", "✅ CONNECTED", "SSH tunnel established"),
        ("File Sync", "✅ ACTIVE", "Real-time sync operational"),
        ("VS Code Remote", "✅ CONNECTED", "Remote development session active")
    ]
    
    print("✓ System Health Status:")
    for check_name, status, details in health_checks:
        print(f"   {status} {check_name}: {details}")
    
    # Show metrics that would be collected
    print("\n✓ Metrics that would be collected:")
    metrics = [
        "Compute instance uptime",
        "File sync operations per minute", 
        "Network latency to remote instance",
        "VS Code extension responsiveness",
        "Storage usage on remote instance"
    ]
    
    for metric in metrics:
        print(f"   • {metric}")

def demo_cost_optimization():
    """Demonstrate cost optimization features."""
    print("\n💰 DEMO: Cost Optimization Features")
    print("=" * 50)
    
    automation = AzureMLComputeAutomation()
    config = automation.config
    
    print("✓ Cost optimization settings:")
    print(f"   • Auto-shutdown after {config['compute_instance']['idle_time_before_shutdown']} minutes of inactivity")
    print(f"   • VM Size: {config['compute_instance']['vm_size']} (optimized for development)")
    print(f"   • Public IP: {'Enabled' if config['compute_instance']['enable_node_public_ip'] else 'Disabled'}")
    
    # Estimate costs (example)
    vm_size = config['compute_instance']['vm_size']
    estimated_hourly_cost = {
        'Standard_DS3_v2': 0.192,
        'Standard_DS4_v2': 0.384,
        'Standard_NC6': 0.90
    }.get(vm_size, 0.20)
    
    print(f"\n✓ Estimated costs for {vm_size}:")
    print(f"   • Per hour: ~${estimated_hourly_cost:.3f}")
    print(f"   • Per 8-hour workday: ~${estimated_hourly_cost * 8:.2f}")
    print(f"   • Per month (22 workdays): ~${estimated_hourly_cost * 8 * 22:.2f}")
    
    print("\n✓ Cost savings with auto-shutdown:")
    print(f"   • Without auto-shutdown (24/7): ~${estimated_hourly_cost * 24 * 30:.2f}/month")
    print(f"   • With auto-shutdown (8h/day): ~${estimated_hourly_cost * 8 * 22:.2f}/month")
    savings = (estimated_hourly_cost * 24 * 30) - (estimated_hourly_cost * 8 * 22)
    print(f"   • Monthly savings: ~${savings:.2f}")

def main():
    """Run all demo functions."""
    print("🚀 Azure ML Compute Instance Automation - DEMO")
    print("=" * 60)
    print("This demo shows the key features and capabilities of the automation system.")
    print("No actual Azure resources will be created during this demo.")
    print("=" * 60)
    
    try:
        # Run all demo functions
        demo_configuration_setup()
        demo_ssh_key_generation()
        demo_vscode_config()
        demo_file_sync_patterns()
        demo_integration_with_existing_notebook()
        demo_monitoring_and_health_checks()
        demo_cost_optimization()
        
        print("\n🎉 DEMO COMPLETED SUCCESSFULLY!")
        print("=" * 60)
        print("Ready to run the actual automation? Use one of these commands:")
        print("")
        print("🔧 Full automation:")
        print("   ./azure-ml-automation.sh")
        print("   python azure-ml-compute-automation.py")
        print("")
        print("🧪 Test prerequisites first:")
        print("   python test-automation.py")
        print("")
        print("📖 Read the documentation:")
        print("   cat README.md")
        print("")
        print("⚙️  Update configuration:")
        print("   nano config/azure-ml-automation-config.json")
        print("=" * 60)
        
    except Exception as e:
        print(f"\n❌ Demo failed: {e}")
        logger.exception("Demo error details:")
        sys.exit(1)

if __name__ == "__main__":
    main()