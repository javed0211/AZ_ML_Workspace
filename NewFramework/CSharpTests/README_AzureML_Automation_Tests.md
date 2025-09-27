# 🧪 Azure ML Compute Automation Tests (C#)

## 📋 Overview

This comprehensive C# test suite validates the Azure ML compute automation functionality using **NUnit** and **Reqnroll (BDD)** frameworks. The tests cover the complete workflow from prerequisites validation to end-to-end automation scenarios.

## 🏗️ Architecture

### **Test Framework Stack**
- **NUnit 3.13.3** - Core testing framework
- **Reqnroll 2.0.3** - BDD testing with Gherkin syntax
- **FluentAssertions 6.12.0** - Expressive assertions
- **Azure SDK** - Azure Resource Manager and ML services
- **SSH.NET** - SSH connectivity testing
- **Serilog** - Structured logging

### **Test Categories**
- ✅ **Prerequisites** - Validate system requirements
- 🔐 **Authentication** - Azure authentication and workspace access
- 💻 **ComputeInstance** - Compute instance lifecycle management
- 🔑 **SSH** - SSH key generation and connection setup
- 🖥️ **VSCode** - VS Code remote development setup
- 📁 **FileSync** - File synchronization testing
- 🔄 **Integration** - End-to-end workflow validation
- ⚡ **Performance** - Performance and timing tests
- ❌ **ErrorHandling** - Error scenarios and recovery

## 🚀 Quick Start

### **1. Prerequisites**
```bash
# Install .NET 9.0 SDK
# Install Azure CLI
# Install VS Code (optional for some tests)
# Install Python 3.x (optional for some tests)

# Authenticate with Azure
az login
```

### **2. Run All Tests**
```bash
# Unix/Linux/macOS
./run-automation-tests.sh

# Windows PowerShell
.\run-automation-tests.ps1
```

### **3. Run Specific Categories**
```bash
# Prerequisites validation only
./run-automation-tests.sh -c Prerequisites

# BDD tests only
./run-automation-tests.sh -b

# Integration tests with report
./run-automation-tests.sh -c Integration -r
```

## 📁 Project Structure

```
CSharpTests/
├── Features/                           # BDD Feature files
│   ├── AzureMLComputeAutomation.feature   # Main BDD scenarios
│   ├── AzureMLWorkspace.feature           # Existing workspace tests
│   └── AzureAISearch.feature              # Existing search tests
├── StepDefinitions/                    # BDD Step implementations
│   ├── AzureMLComputeAutomationSteps.cs   # Automation step definitions
│   ├── AzureMLWorkspaceSteps.cs           # Existing workspace steps
│   └── AzureAISearchSteps.cs              # Existing search steps
├── Tests/                              # NUnit test classes
│   ├── AzureMLComputeAutomationTests.cs   # Main automation tests
│   ├── ApiIntegrationTest.cs              # Existing API tests
│   └── ApiTestExample.cs                  # Existing examples
├── Utils/                              # Utility classes
│   ├── AzureMLComputeAutomationUtils.cs   # Core automation utilities
│   ├── AzureMLUtils.cs                    # Existing ML utilities
│   ├── ConfigManager.cs                   # Configuration management
│   └── Logger.cs                          # Logging utilities
├── run-automation-tests.sh            # Unix test runner
├── run-automation-tests.ps1           # Windows test runner
├── PlaywrightFramework.csproj         # Project file with dependencies
└── README_AzureML_Automation_Tests.md # This documentation
```

## 🔧 Configuration

### **appsettings.json Structure**
```json
{
  "Environment": "development",
  "Environments": {
    "development": {
      "Azure": {
        "SubscriptionId": "your-subscription-id",
        "TenantId": "your-tenant-id",
        "ResourceGroup": "your-resource-group",
        "WorkspaceName": "your-workspace-name",
        "Region": "eastus"
      },
      "Authentication": {
        "UseDefaultCredentials": true,
        "TimeoutSeconds": 300
      }
    }
  }
}
```

## 🧪 Test Categories Explained

### **Prerequisites Tests**
Validates system requirements and dependencies:
- ✅ Python installation and version
- ✅ VS Code installation and accessibility
- ✅ Azure CLI installation and authentication
- ✅ Network connectivity to Azure services
- ✅ Required Python packages availability

### **Authentication Tests**
Tests Azure authentication and workspace access:
- 🔐 Azure client initialization with DefaultAzureCredential
- 🔐 ML workspace connection and validation
- 🔐 Subscription and resource group access verification

### **Compute Instance Tests**
Validates compute instance lifecycle management:
- 💻 Create compute instances with specified VM sizes
- 💻 Start and stop compute instances
- 💻 Retrieve instance details and status
- 💻 Delete compute instances (cleanup)

### **SSH Setup Tests**
Tests SSH key generation and connection setup:
- 🔑 Generate RSA SSH key pairs
- 🔑 Configure SSH connection settings
- 🔑 Test SSH connectivity (when possible)
- 🔑 Validate SSH key permissions and format

### **VS Code Remote Tests**
Validates VS Code remote development setup:
- 🖥️ Install required VS Code extensions
- 🖥️ Configure remote SSH connections
- 🖥️ Open remote workspaces (GUI-dependent)

### **File Synchronization Tests**
Tests file sync between local and remote environments:
- 📁 Setup bidirectional file synchronization
- 📁 Monitor real-time file changes
- 📁 Validate secure file transfer protocols

### **Integration Tests**
End-to-end workflow validation:
- 🔄 Complete automation workflow execution
- 🔄 Multi-step process coordination
- 🔄 Resource lifecycle management
- 🔄 Error recovery and cleanup

### **Performance Tests**
Validates automation performance and timing:
- ⚡ Azure client initialization timing
- ⚡ Workspace connection performance
- ⚡ Compute instance creation duration
- ⚡ Overall workflow completion time

### **Error Handling Tests**
Tests error scenarios and recovery:
- ❌ Invalid credentials handling
- ❌ Network connectivity issues
- ❌ Insufficient permissions scenarios
- ❌ Resource cleanup on failures

## 🎯 BDD Scenarios

The BDD feature file includes comprehensive scenarios covering:

### **Core Scenarios**
- Prerequisites validation
- Azure authentication
- Compute instance management
- SSH connection setup
- VS Code remote configuration
- File synchronization
- End-to-end automation workflow

### **Error Handling Scenarios**
- Authentication failures
- Network connectivity issues
- Permission errors
- Resource cleanup

### **Security Scenarios**
- SSH key management
- Secure file transfer
- Credential handling

### **Performance Scenarios**
- Timing requirements
- Resource optimization

## 📊 Test Execution Options

### **Command Line Options**

#### **Unix/Linux/macOS (Bash)**
```bash
./run-automation-tests.sh [OPTIONS]

Options:
  -h, --help              Show help message
  -c, --category CATEGORY Run tests in specific category
  -t, --test TEST_NAME    Run specific test by name
  -b, --bdd               Run BDD tests only
  -r, --report            Generate test report
  --prerequisites-only    Check prerequisites only
  --build-only            Build project only
```

#### **Windows (PowerShell)**
```powershell
.\run-automation-tests.ps1 [OPTIONS]

Options:
  -Help                   Show help message
  -Category CATEGORY      Run tests in specific category
  -Test TEST_NAME         Run specific test by name
  -BDD                    Run BDD tests only
  -Report                 Generate test report
  -PrerequisitesOnly      Check prerequisites only
  -BuildOnly              Build project only
```

### **Example Usage**

#### **Basic Test Execution**
```bash
# Run all automation tests
./run-automation-tests.sh

# Run prerequisites validation only
./run-automation-tests.sh -c Prerequisites

# Run specific test
./run-automation-tests.sh -t "Test_ValidatePrerequisites_ShouldPassAllChecks"

# Run BDD scenarios
./run-automation-tests.sh -b
```

#### **Advanced Options**
```bash
# Run integration tests with detailed report
./run-automation-tests.sh -c Integration -r

# Check prerequisites without running tests
./run-automation-tests.sh --prerequisites-only

# Build project without running tests
./run-automation-tests.sh --build-only

# Run performance tests
./run-automation-tests.sh -c Performance
```

#### **Windows Examples**
```powershell
# Run all tests with report generation
.\run-automation-tests.ps1 -Report

# Run specific category
.\run-automation-tests.ps1 -Category "ComputeInstance"

# Run BDD tests only
.\run-automation-tests.ps1 -BDD
```

## 📈 Test Reports and Logging

### **Console Output**
- Real-time test execution status
- Detailed assertion results
- Performance metrics
- Error messages and stack traces

### **Log Files**
- `logs/azure-ml-automation-tests.log` - NUnit test logs
- `logs/azure-ml-automation-bdd.log` - BDD test logs
- Structured logging with Serilog

### **Test Reports**
- `TestResults/azure-ml-automation-tests.trx` - Visual Studio test results
- Compatible with Azure DevOps and other CI/CD systems

## 🔍 Troubleshooting

### **Common Issues**

#### **Authentication Failures**
```bash
# Ensure Azure CLI is authenticated
az login

# Verify subscription access
az account show

# Check resource group permissions
az group show --name "your-resource-group"
```

#### **Prerequisites Missing**
```bash
# Install .NET SDK
# Download from: https://dotnet.microsoft.com/download

# Install Azure CLI
# Instructions: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli

# Install VS Code (optional)
# Download from: https://code.visualstudio.com/
```

#### **Network Connectivity**
```bash
# Test Azure ML connectivity
curl -I https://ml.azure.com

# Check DNS resolution
nslookup ml.azure.com

# Verify firewall settings
```

#### **Configuration Issues**
```bash
# Validate appsettings.json syntax
# Ensure all required fields are populated
# Check subscription ID and resource group names
```

### **Debug Mode**
```bash
# Enable verbose logging
export AZURE_ML_DEBUG=true

# Run with detailed output
./run-automation-tests.sh -c Prerequisites --verbose
```

### **Test Environment Setup**
```bash
# Create test resource group (if needed)
az group create --name "test-ml-automation" --location "eastus"

# Create test ML workspace (if needed)
az ml workspace create --name "test-workspace" --resource-group "test-ml-automation"
```

## 🚀 CI/CD Integration

### **Azure DevOps Pipeline**
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Azure ML Automation Tests'
  inputs:
    command: 'test'
    projects: '**/PlaywrightFramework.csproj'
    arguments: '--configuration Release --logger trx --filter "Category=AzureMLComputeAutomation"'
    publishTestResults: true
```

### **GitHub Actions**
```yaml
- name: Run Azure ML Automation Tests
  run: |
    dotnet test --configuration Release --logger "console;verbosity=detailed" --filter "Category=AzureMLComputeAutomation"
```

## 📚 Additional Resources

### **Documentation Links**
- [Azure ML SDK Documentation](https://docs.microsoft.com/en-us/azure/machine-learning/)
- [NUnit Documentation](https://docs.nunit.org/)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)

### **Related Files**
- `../automation/azure-ml-compute-automation.py` - Python automation script
- `../automation/README.md` - Python automation documentation
- `../automation/config/azure-ml-automation-config.json` - Automation configuration

## 🤝 Contributing

### **Adding New Tests**
1. Create test methods in `Tests/AzureMLComputeAutomationTests.cs`
2. Add BDD scenarios to `Features/AzureMLComputeAutomation.feature`
3. Implement step definitions in `StepDefinitions/AzureMLComputeAutomationSteps.cs`
4. Update test categories and documentation

### **Test Naming Conventions**
- NUnit tests: `Test_MethodName_ShouldExpectedBehavior`
- BDD scenarios: Descriptive scenario names
- Categories: Use existing category names for consistency

### **Code Quality**
- Follow C# coding standards
- Add comprehensive assertions
- Include proper error handling
- Document complex test logic

## 📄 License

This test suite is part of the Azure ML Workspace automation project and follows the same licensing terms as the parent project.

---

**Happy Testing! 🧪✨**

*For support or questions, please refer to the main project documentation or create an issue in the repository.*