#!/usr/bin/env python3
"""
Test script for Azure ML Compute Instance Automation
Validates configuration and prerequisites before running the main automation.
"""

import os
import sys
import json
import subprocess
import logging
from pathlib import Path
from typing import Dict, List, Tuple

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class AutomationTester:
    """Test class for validating automation prerequisites."""
    
    def __init__(self, config_path: str = "config/azure-ml-automation-config.json"):
        self.config_path = config_path
        self.config = None
        self.test_results = []
    
    def run_all_tests(self) -> bool:
        """Run all validation tests."""
        logger.info("Starting Azure ML Automation validation tests...")
        
        tests = [
            ("Configuration File", self.test_config_file),
            ("Python Environment", self.test_python_environment),
            ("Required Packages", self.test_required_packages),
            ("VS Code Installation", self.test_vscode_installation),
            ("SSH Setup", self.test_ssh_setup),
            ("Azure CLI", self.test_azure_cli),
            ("Network Connectivity", self.test_network_connectivity),
            ("File Permissions", self.test_file_permissions)
        ]
        
        all_passed = True
        
        for test_name, test_func in tests:
            logger.info(f"Running test: {test_name}")
            try:
                result = test_func()
                status = "PASS" if result else "FAIL"
                self.test_results.append((test_name, status, ""))
                
                if result:
                    logger.info(f"✓ {test_name}: PASSED")
                else:
                    logger.error(f"✗ {test_name}: FAILED")
                    all_passed = False
                    
            except Exception as e:
                logger.error(f"✗ {test_name}: ERROR - {e}")
                self.test_results.append((test_name, "ERROR", str(e)))
                all_passed = False
        
        self.print_summary()
        return all_passed
    
    def test_config_file(self) -> bool:
        """Test if configuration file exists and is valid."""
        try:
            if not os.path.exists(self.config_path):
                logger.error(f"Configuration file not found: {self.config_path}")
                return False
            
            with open(self.config_path, 'r') as f:
                self.config = json.load(f)
            
            # Validate required sections
            required_sections = ['azure', 'compute_instance', 'vscode', 'file_sync']
            for section in required_sections:
                if section not in self.config:
                    logger.error(f"Missing configuration section: {section}")
                    return False
            
            # Validate Azure configuration
            azure_config = self.config['azure']
            required_azure_fields = ['subscription_id', 'resource_group', 'workspace_name']
            for field in required_azure_fields:
                if not azure_config.get(field) or azure_config[field] == f"your-{field.replace('_', '-')}":
                    logger.error(f"Azure configuration field '{field}' not set properly")
                    return False
            
            logger.info("Configuration file is valid")
            return True
            
        except json.JSONDecodeError as e:
            logger.error(f"Invalid JSON in configuration file: {e}")
            return False
        except Exception as e:
            logger.error(f"Error reading configuration file: {e}")
            return False
    
    def test_python_environment(self) -> bool:
        """Test Python version and environment."""
        try:
            # Check Python version
            version_info = sys.version_info
            if version_info.major < 3 or (version_info.major == 3 and version_info.minor < 7):
                logger.error(f"Python 3.7+ required, found {version_info.major}.{version_info.minor}")
                return False
            
            logger.info(f"Python {version_info.major}.{version_info.minor}.{version_info.micro} - OK")
            
            # Check pip
            try:
                import pip
                logger.info("pip is available")
            except ImportError:
                logger.error("pip is not available")
                return False
            
            return True
            
        except Exception as e:
            logger.error(f"Error checking Python environment: {e}")
            return False
    
    def test_required_packages(self) -> bool:
        """Test if required Python packages are installed."""
        required_packages = [
            'azure.ai.ml',
            'azure.identity',
            'paramiko',
            'watchdog'
        ]
        
        missing_packages = []
        
        for package in required_packages:
            try:
                __import__(package)
                logger.info(f"✓ {package} is installed")
            except ImportError:
                missing_packages.append(package)
                logger.error(f"✗ {package} is missing")
        
        if missing_packages:
            logger.error(f"Missing packages: {', '.join(missing_packages)}")
            logger.info("Install with: pip install -r requirements.txt")
            return False
        
        return True
    
    def test_vscode_installation(self) -> bool:
        """Test VS Code installation and extensions."""
        try:
            # Check if VS Code is installed
            result = subprocess.run(['code', '--version'], 
                                  capture_output=True, text=True, timeout=10)
            
            if result.returncode != 0:
                logger.error("VS Code is not installed or not in PATH")
                return False
            
            version_lines = result.stdout.strip().split('\n')
            if version_lines:
                logger.info(f"VS Code version: {version_lines[0]}")
            
            # Check required extensions
            result = subprocess.run(['code', '--list-extensions'], 
                                  capture_output=True, text=True, timeout=10)
            
            if result.returncode == 0:
                installed_extensions = result.stdout.strip().split('\n')
                required_extensions = [
                    'ms-vscode-remote.remote-ssh',
                    'ms-python.python',
                    'ms-toolsai.jupyter'
                ]
                
                missing_extensions = []
                for ext in required_extensions:
                    if ext not in installed_extensions:
                        missing_extensions.append(ext)
                
                if missing_extensions:
                    logger.warning(f"Missing VS Code extensions: {', '.join(missing_extensions)}")
                    logger.info("Extensions will be installed automatically")
                else:
                    logger.info("All required VS Code extensions are installed")
            
            return True
            
        except subprocess.TimeoutExpired:
            logger.error("VS Code command timed out")
            return False
        except FileNotFoundError:
            logger.error("VS Code command not found")
            return False
        except Exception as e:
            logger.error(f"Error checking VS Code: {e}")
            return False
    
    def test_ssh_setup(self) -> bool:
        """Test SSH client and key setup."""
        try:
            # Check SSH client
            result = subprocess.run(['ssh', '-V'], 
                                  capture_output=True, text=True, timeout=5)
            
            if result.returncode != 0:
                logger.error("SSH client is not installed")
                return False
            
            # SSH version is typically printed to stderr
            ssh_version = result.stderr.strip().split('\n')[0] if result.stderr else "Unknown"
            logger.info(f"SSH client: {ssh_version}")
            
            # Check SSH directory
            ssh_dir = Path.home() / '.ssh'
            if not ssh_dir.exists():
                logger.info("SSH directory will be created")
                return True
            
            # Check for existing SSH keys
            private_key = ssh_dir / 'id_rsa'
            public_key = ssh_dir / 'id_rsa.pub'
            
            if private_key.exists() and public_key.exists():
                logger.info("SSH key pair already exists")
            else:
                logger.info("SSH key pair will be generated")
            
            return True
            
        except subprocess.TimeoutExpired:
            logger.error("SSH command timed out")
            return False
        except FileNotFoundError:
            logger.error("SSH command not found")
            return False
        except Exception as e:
            logger.error(f"Error checking SSH setup: {e}")
            return False
    
    def test_azure_cli(self) -> bool:
        """Test Azure CLI installation and authentication."""
        try:
            # Check Azure CLI installation
            result = subprocess.run(['az', '--version'], 
                                  capture_output=True, text=True, timeout=10)
            
            if result.returncode != 0:
                logger.warning("Azure CLI is not installed (optional but recommended)")
                return True  # Not required, so return True
            
            # Parse version info
            version_lines = result.stdout.strip().split('\n')
            if version_lines:
                logger.info(f"Azure CLI: {version_lines[0]}")
            
            # Check authentication status
            result = subprocess.run(['az', 'account', 'show'], 
                                  capture_output=True, text=True, timeout=10)
            
            if result.returncode == 0:
                account_info = json.loads(result.stdout)
                logger.info(f"Azure CLI authenticated as: {account_info.get('user', {}).get('name', 'Unknown')}")
            else:
                logger.warning("Azure CLI is not authenticated (you may need to run 'az login')")
            
            return True
            
        except subprocess.TimeoutExpired:
            logger.warning("Azure CLI command timed out")
            return True
        except FileNotFoundError:
            logger.warning("Azure CLI not found (optional)")
            return True
        except Exception as e:
            logger.warning(f"Error checking Azure CLI: {e}")
            return True
    
    def test_network_connectivity(self) -> bool:
        """Test network connectivity to Azure services."""
        try:
            import requests
            
            # Test connectivity to Azure management endpoint
            test_urls = [
                'https://management.azure.com/',
                'https://login.microsoftonline.com/',
                'https://ml.azure.com/'
            ]
            
            for url in test_urls:
                try:
                    response = requests.get(url, timeout=10)
                    if response.status_code in [200, 401, 403]:  # 401/403 are OK, means we reached the service
                        logger.info(f"✓ Connectivity to {url} - OK")
                    else:
                        logger.warning(f"⚠ Connectivity to {url} - Status: {response.status_code}")
                except requests.RequestException as e:
                    logger.warning(f"⚠ Connectivity to {url} - Error: {e}")
            
            return True
            
        except ImportError:
            logger.warning("requests package not available for network test")
            return True
        except Exception as e:
            logger.warning(f"Error testing network connectivity: {e}")
            return True
    
    def test_file_permissions(self) -> bool:
        """Test file system permissions."""
        try:
            # Test write permissions in current directory
            test_file = Path('test_permissions.tmp')
            
            try:
                with open(test_file, 'w') as f:
                    f.write('test')
                
                test_file.unlink()  # Delete test file
                logger.info("File system write permissions - OK")
                
            except PermissionError:
                logger.error("No write permissions in current directory")
                return False
            
            # Test SSH directory permissions
            ssh_dir = Path.home() / '.ssh'
            if ssh_dir.exists():
                stat_info = ssh_dir.stat()
                permissions = oct(stat_info.st_mode)[-3:]
                
                if permissions == '700':
                    logger.info("SSH directory permissions - OK")
                else:
                    logger.warning(f"SSH directory permissions: {permissions} (should be 700)")
            
            return True
            
        except Exception as e:
            logger.error(f"Error testing file permissions: {e}")
            return False
    
    def print_summary(self):
        """Print test results summary."""
        logger.info("\n" + "="*60)
        logger.info("TEST RESULTS SUMMARY")
        logger.info("="*60)
        
        passed = sum(1 for _, status, _ in self.test_results if status == "PASS")
        failed = sum(1 for _, status, _ in self.test_results if status == "FAIL")
        errors = sum(1 for _, status, _ in self.test_results if status == "ERROR")
        
        for test_name, status, error_msg in self.test_results:
            status_symbol = "✓" if status == "PASS" else "✗"
            logger.info(f"{status_symbol} {test_name}: {status}")
            if error_msg:
                logger.info(f"    Error: {error_msg}")
        
        logger.info("-"*60)
        logger.info(f"Total Tests: {len(self.test_results)}")
        logger.info(f"Passed: {passed}")
        logger.info(f"Failed: {failed}")
        logger.info(f"Errors: {errors}")
        
        if failed == 0 and errors == 0:
            logger.info("🎉 All tests passed! Ready to run automation.")
        else:
            logger.info("⚠️  Some tests failed. Please fix issues before running automation.")
        
        logger.info("="*60)

def main():
    """Main function to run tests."""
    import argparse
    
    parser = argparse.ArgumentParser(description="Test Azure ML Automation Prerequisites")
    parser.add_argument("--config", help="Path to configuration file", 
                       default="config/azure-ml-automation-config.json")
    
    args = parser.parse_args()
    
    tester = AutomationTester(args.config)
    success = tester.run_all_tests()
    
    sys.exit(0 if success else 1)

if __name__ == "__main__":
    main()