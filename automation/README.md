# Azure ML Compute Instance Automation

This automation solution provides a comprehensive way to launch Linux-based compute instances from an Azure ML workspace and establish VS Code remote connections with real-time file synchronization.

## 🚀 Features

- **Automated Compute Provisioning**: Creates and manages Azure ML compute instances
- **VS Code Remote Setup**: Automatically configures VS Code for remote development
- **Real-time File Synchronization**: Bidirectional sync between local and remote environments
- **Cross-Platform Support**: Works on Windows, macOS, and Linux
- **Secure Authentication**: Uses Azure managed identity and SSH key authentication
- **Network Configuration**: Handles firewall rules and network security
- **File Transfer Protocols**: Secure SFTP-based file synchronization

## 📋 Prerequisites

### Required Software
- **Python 3.7+** with pip
- **VS Code** (latest version recommended)
- **Azure CLI** (optional but recommended)
- **SSH client** (OpenSSH or equivalent)

### Azure Requirements
- Azure subscription with ML workspace
- Appropriate permissions to create compute instances
- Resource group with sufficient quota

## 🛠 Installation

### 1. Clone and Setup
```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/automation
```

### 2. Install Dependencies

#### Option A: Using the automation script
```bash
# macOS/Linux
./azure-ml-automation.sh --install-deps

# Windows PowerShell
.\azure-ml-automation.ps1 -InstallDependencies
```

#### Option B: Manual installation
```bash
pip install -r requirements.txt
```

### 3. Configure Azure Credentials

#### Option A: Azure CLI (Recommended)
```bash
az login
az account set --subscription "your-subscription-id"
```

#### Option B: Environment Variables
```bash
export AZURE_SUBSCRIPTION_ID="your-subscription-id"
export AZURE_CLIENT_ID="your-client-id"
export AZURE_CLIENT_SECRET="your-client-secret"
export AZURE_TENANT_ID="your-tenant-id"
```

### 4. Update Configuration
Edit `config/azure-ml-automation-config.json` with your Azure details:

```json
{
  "azure": {
    "subscription_id": "your-actual-subscription-id",
    "resource_group": "your-actual-resource-group",
    "workspace_name": "your-actual-workspace-name",
    "location": "eastus"
  }
}
```

## 🎯 Usage

### Quick Start - Full Automation
```bash
# macOS/Linux
./azure-ml-automation.sh

# Windows PowerShell
.\azure-ml-automation.ps1

# Python directly
python azure-ml-compute-automation.py
```

### Advanced Usage Options

#### Create Compute Instance Only
```bash
# macOS/Linux
./azure-ml-automation.sh --create-only

# Windows PowerShell
.\azure-ml-automation.ps1 -CreateOnly

# Python
python azure-ml-compute-automation.py --create-only
```

#### Setup VS Code Connection Only (assumes compute exists)
```bash
# macOS/Linux
./azure-ml-automation.sh --vscode-only

# Windows PowerShell
.\azure-ml-automation.ps1 -VSCodeOnly

# Python
python azure-ml-compute-automation.py --vscode-only
```

#### Start File Synchronization Only
```bash
# macOS/Linux
./azure-ml-automation.sh --sync-only

# Windows PowerShell
.\azure-ml-automation.ps1 -SyncOnly

# Python
python azure-ml-compute-automation.py --sync-only
```

#### Cleanup (Delete Compute Instance)
```bash
# macOS/Linux
./azure-ml-automation.sh --cleanup

# Windows PowerShell
.\azure-ml-automation.ps1 -Cleanup

# Python
python azure-ml-compute-automation.py --cleanup
```

## 📁 File Structure

```
automation/
├── azure-ml-compute-automation.py    # Main Python automation script
├── azure-ml-automation.sh            # Bash script for Unix/Linux/macOS
├── azure-ml-automation.ps1           # PowerShell script for Windows
├── requirements.txt                  # Python dependencies
├── README.md                         # This documentation
├── config/
│   └── azure-ml-automation-config.json  # Configuration file
└── logs/
    └── azure_ml_automation.log       # Automation logs
```

## ⚙️ Configuration Options

### Compute Instance Settings
```json
{
  "compute_instance": {
    "name": "ml-compute-instance",
    "vm_size": "Standard_DS3_v2",
    "admin_username": "azureuser",
    "enable_ssh": true,
    "idle_time_before_shutdown": 30,
    "enable_node_public_ip": true
  }
}
```

### File Synchronization Settings
```json
{
  "file_sync": {
    "enabled": true,
    "sync_patterns": ["*.py", "*.ipynb", "*.json", "*.md"],
    "exclude_patterns": [".git/*", "__pycache__/*", "*.pyc"],
    "bidirectional": true,
    "real_time": true
  }
}
```

### VS Code Settings
```json
{
  "vscode": {
    "remote_extensions": [
      "ms-python.python",
      "ms-toolsai.jupyter",
      "ms-vscode-remote.remote-ssh",
      "ms-azuretools.vscode-azureml"
    ],
    "local_workspace_path": "/Users/oldguard/Documents/GitHub/AZ_ML_Workspace",
    "remote_workspace_path": "/home/azureuser/workspace"
  }
}
```

## 🔐 Security Features

### Authentication Methods
- **Azure Managed Identity**: Preferred for production environments
- **Service Principal**: For automated deployments
- **Interactive Browser**: For development environments
- **Azure CLI**: Uses existing CLI authentication

### Network Security
- **SSH Key Authentication**: RSA 2048-bit keys generated automatically
- **IP Restrictions**: Configurable allowed IP ranges
- **Firewall Rules**: Automatic security group configuration
- **Auto-shutdown**: Configurable idle timeout

### File Transfer Security
- **SFTP Protocol**: Secure file transfer over SSH
- **Key-based Authentication**: No password authentication
- **Encrypted Connections**: All data transfer encrypted

## 🔧 Troubleshooting

### Common Issues

#### 1. Authentication Failures
```bash
# Check Azure CLI authentication
az account show

# Re-authenticate if needed
az login --use-device-code
```

#### 2. SSH Connection Issues
```bash
# Test SSH connection manually
ssh azureml-compute

# Check SSH key permissions
chmod 600 ~/.ssh/id_rsa
chmod 644 ~/.ssh/id_rsa.pub
```

#### 3. VS Code Remote Connection Problems
```bash
# Check VS Code SSH extension
code --list-extensions | grep remote-ssh

# Install if missing
code --install-extension ms-vscode-remote.remote-ssh
```

#### 4. File Sync Issues
```bash
# Check file permissions
ls -la /path/to/local/files

# Verify remote directory exists
ssh azureml-compute "ls -la /home/azureuser/workspace"
```

### Debug Mode
Enable detailed logging by setting environment variable:
```bash
export AZURE_ML_DEBUG=true
python azure-ml-compute-automation.py
```

### Log Files
Check automation logs:
```bash
tail -f logs/azure_ml_automation.log
```

## 🚀 Advanced Features

### Custom VM Sizes
Supported VM sizes for different workloads:

| VM Size | vCPUs | RAM | Use Case |
|---------|-------|-----|----------|
| Standard_DS3_v2 | 4 | 14 GB | General development |
| Standard_DS4_v2 | 8 | 28 GB | Data processing |
| Standard_NC6 | 6 | 56 GB | GPU workloads |
| Standard_NC12 | 12 | 112 GB | Heavy ML training |

### Environment Variables
Customize behavior with environment variables:

```bash
# Compute instance configuration
export AZURE_ML_COMPUTE_NAME="my-compute"
export AZURE_ML_VM_SIZE="Standard_DS4_v2"
export AZURE_ML_IDLE_TIMEOUT="60"

# File sync configuration
export AZURE_ML_SYNC_ENABLED="true"
export AZURE_ML_SYNC_REALTIME="true"

# VS Code configuration
export AZURE_ML_VSCODE_EXTENSIONS="ms-python.python,ms-toolsai.jupyter"
```

### Integration with CI/CD
Example GitHub Actions workflow:

```yaml
name: Setup Azure ML Environment
on: [push]

jobs:
  setup:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup Azure ML Compute
      run: |
        cd automation
        python azure-ml-compute-automation.py --create-only
      env:
        AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
        AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
        AZURE_CLIENT_SECRET: ${{ secrets.AZURE_CLIENT_SECRET }}
        AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
```

## 📊 Monitoring and Metrics

### Health Checks
The automation includes built-in health monitoring:
- Compute instance status
- SSH connection health
- File sync status
- VS Code remote connection

### Metrics Collection
Optional metrics collection for:
- Compute instance utilization
- File sync performance
- Connection reliability
- Cost tracking

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For issues and questions:
1. Check the troubleshooting section above
2. Review the logs in `logs/azure_ml_automation.log`
3. Create an issue in the repository
4. Contact the development team

## 🔄 Updates and Maintenance

### Regular Updates
- Keep Azure ML SDK updated: `pip install --upgrade azure-ai-ml`
- Update VS Code extensions regularly
- Monitor Azure service updates

### Backup and Recovery
- SSH keys are automatically backed up
- Configuration files should be version controlled
- Compute instances can be recreated from configuration

---

**Happy Coding with Azure ML! 🎉**