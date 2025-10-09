# Linux Self-Hosted Agent Requirements

## Overview
This document outlines the requirements for running the Azure ML Test Framework on a Linux-based self-hosted Azure DevOps agent.

---

## 1. System Requirements

### Operating System
- **Ubuntu 20.04 LTS or later** (recommended)
- **RHEL/CentOS 8+** (supported)
- **Debian 11+** (supported)
- **64-bit architecture** (x86_64 or ARM64)

### Hardware Recommendations
- **CPU**: 4+ cores
- **RAM**: 8GB minimum, 16GB recommended
- **Disk Space**: 50GB+ free space
- **Network**: Stable internet connection for Azure services

---

## 2. Required Software & Dependencies

### 2.1 .NET SDK
```bash
# Install .NET 9.0 SDK (as per PlaywrightFramework.csproj)
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

# Add to PATH
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

# Verify installation
dotnet --version  # Should show 9.0.x
```

**Note**: The project uses `net9.0` target framework.

### 2.2 Node.js & npm
```bash
# Install Node.js 18.x (as per azure-pipelines.yml)
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Verify installation
node --version  # Should show v18.x
npm --version   # Should show 9.x or higher
```

### 2.3 Playwright Browsers
```bash
# Install Playwright with system dependencies
npx playwright install --with-deps chromium firefox webkit

# Or install specific browser
npx playwright install --with-deps chromium
```

**Required browsers** (based on azure-pipelines.yml):
- Chromium
- Firefox
- WebKit (optional for Linux)

### 2.4 Azure DevOps Agent
```bash
# Download and configure Azure DevOps agent
mkdir ~/azagent && cd ~/azagent
wget https://vstsagentpackage.azureedge.net/agent/3.236.1/vsts-agent-linux-x64-3.236.1.tar.gz
tar zxvf vsts-agent-linux-x64-3.236.1.tar.gz

# Configure agent
./config.sh

# Install as service (optional)
sudo ./svc.sh install
sudo ./svc.sh start
```

### 2.5 Additional System Dependencies
```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install -y \
    git \
    curl \
    wget \
    unzip \
    libicu-dev \
    libc6-dev \
    libgdiplus \
    libx11-dev \
    libssl-dev \
    ca-certificates

# For Azure Speech Services (Microsoft.CognitiveServices.Speech)
sudo apt-get install -y \
    libasound2 \
    libssl1.1 \
    wget

# RHEL/CentOS
sudo yum install -y \
    git \
    curl \
    wget \
    unzip \
    libicu \
    glibc-devel \
    libgdiplus \
    libX11-devel \
    openssl-devel \
    ca-certificates
```

---

## 3. NuGet Package Dependencies

The following packages will be automatically restored via `dotnet restore`:

### Core Testing Frameworks
- **Microsoft.Playwright** (1.40.0)
- **Microsoft.Playwright.NUnit** (1.40.0)
- **NUnit** (3.13.3)
- **NUnit3TestAdapter** (4.5.0)
- **Microsoft.NET.Test.Sdk** (17.8.0)

### Azure SDK Dependencies
- **Azure.Identity** (1.12.0)
- **Azure.ResourceManager** (1.13.2)
- **Azure.ResourceManager.MachineLearning** (1.2.3)
- **Azure.Search.Documents** (11.5.1)
- **Azure.AI.FormRecognizer** (4.1.0)
- **Microsoft.CognitiveServices.Speech** (1.38.0)

### BDD Framework
- **Reqnroll** (2.0.3)
- **Reqnroll.NUnit** (2.0.3)
- **Reqnroll.Tools.MsBuild.Generation** (2.0.3)

### Utilities
- **Serilog** (3.1.1) + Sinks
- **Newtonsoft.Json** (13.0.3)
- **FluentAssertions** (6.12.0)
- **SSH.NET** (2023.0.0)

### Reporting
- **Allure.NUnit** (2.12.1)
- **Allure.Net.Commons** (2.12.1)

---

## 4. Azure Configuration

### 4.1 Service Principal / Managed Identity
The agent needs Azure credentials to run tests. Configure one of:

#### Option A: Service Principal (Recommended for Self-Hosted)
```bash
# Set environment variables
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_CLIENT_ID="your-client-id"
export AZURE_CLIENT_SECRET="your-client-secret"
export AZURE_SUBSCRIPTION_ID="your-subscription-id"
```

#### Option B: Managed Identity
- Assign Managed Identity to the VM/container running the agent
- Grant appropriate Azure RBAC roles

### 4.2 Required Azure RBAC Roles
The service principal/identity needs:
- **Contributor** on Azure ML Workspace
- **Cognitive Services User** for AI Services
- **Search Service Contributor** for Azure AI Search
- **Reader** on Resource Group (minimum)

### 4.3 Azure Service Endpoints
Ensure the agent can reach:
- `https://management.azure.com` (Azure Resource Manager)
- `https://*.cognitiveservices.azure.com` (AI Services)
- `https://*.search.windows.net` (Azure AI Search)
- `https://*.api.azureml.ms` (Azure ML)

---

## 5. Configuration Files

### 5.1 appsettings.json
Located at: `NewFramework/Config/appsettings.json`

**Required configuration sections:**
```json
{
  "Environment": "dev",
  "Environments": {
    "dev": {
      "AzureML": {
        "SubscriptionId": "your-subscription-id",
        "ResourceGroup": "your-resource-group",
        "WorkspaceName": "your-workspace-name",
        "TenantId": "your-tenant-id"
      },
      "AzureAISearch": {
        "ServiceName": "your-search-service",
        "AdminKey": "your-admin-key",
        "IndexName": "your-index-name"
      },
      "DocumentIntelligence": {
        "Endpoint": "https://your-doc-intel.cognitiveservices.azure.com/",
        "ApiKey": "your-api-key"
      },
      "SpeechServices": {
        "SubscriptionKey": "your-speech-key",
        "Region": "your-region",
        "Endpoint": "https://your-region.api.cognitive.microsoft.com/"
      }
    }
  }
}
```

### 5.2 Environment Variables (Alternative)
Can override appsettings.json:
```bash
export AZURE_SUBSCRIPTION_ID="..."
export AZURE_TENANT_ID="..."
export AZURE_RESOURCE_GROUP="..."
export AZURE_WORKSPACE_NAME="..."
export AZUREAISEARCH_SERVICENAME="..."
export AZUREAISEARCH_INDEXNAME="..."
export AZUREAISEARCH_ADMINKEY="..."
```

---

## 6. Build & Test Commands

### 6.1 Restore Dependencies
```bash
cd /path/to/AZ_ML_Workspace/NewFramework/CSharpTests
dotnet restore PlaywrightFramework.csproj
```

### 6.2 Build Project
```bash
dotnet build PlaywrightFramework.csproj --configuration Release
```

### 6.3 Run All Tests
```bash
dotnet test PlaywrightFramework.csproj \
  --configuration Release \
  --logger "console;verbosity=detailed" \
  --logger "trx;LogFileName=test-results.trx"
```

### 6.4 Run Specific Test Categories
```bash
# Run only Azure Speech Services tests
dotnet test --filter "Category=SpeechServices"

# Run only BDD tests
dotnet test --filter "Category=BDD"

# Run specific feature
dotnet test --filter "FullyQualifiedName~AzureSpeechServices"
```

### 6.5 Generate Allure Reports
```bash
# Install Allure CLI
sudo apt-get install -y default-jre
wget https://github.com/allure-framework/allure2/releases/download/2.24.0/allure-2.24.0.tgz
tar -zxvf allure-2.24.0.tgz -C /opt/
sudo ln -s /opt/allure-2.24.0/bin/allure /usr/bin/allure

# Generate report
cd NewFramework/CSharpTests
./generate-allure-report.sh
```

---

## 7. Playwright-Specific Requirements

### 7.1 Headless Mode (Recommended for CI/CD)
```bash
# Set environment variable
export HEADLESS=true

# Or in test command
dotnet test --environment HEADLESS=true
```

### 7.2 Browser Installation
```bash
# Install all browsers with dependencies
cd NewFramework/CSharpTests
pwsh bin/Debug/net9.0/playwright.ps1 install --with-deps

# Or use npx
npx playwright install --with-deps
```

### 7.3 Display Server (for headed mode)
If running in headed mode on a server:
```bash
# Install Xvfb (virtual framebuffer)
sudo apt-get install -y xvfb

# Run tests with Xvfb
xvfb-run --auto-servernum dotnet test
```

---

## 8. Network & Firewall Requirements

### 8.1 Outbound Connections
Allow outbound HTTPS (443) to:
- `*.azure.com`
- `*.azurewebsites.net`
- `*.cognitiveservices.azure.com`
- `*.search.windows.net`
- `*.api.azureml.ms`
- `github.com` (for package downloads)
- `nuget.org` (for NuGet packages)
- `npmjs.org` (for npm packages)

### 8.2 Proxy Configuration (if applicable)
```bash
# Set proxy for .NET
export HTTP_PROXY=http://proxy.company.com:8080
export HTTPS_PROXY=http://proxy.company.com:8080

# Set proxy for npm
npm config set proxy http://proxy.company.com:8080
npm config set https-proxy http://proxy.company.com:8080

# Set proxy for Playwright
export PLAYWRIGHT_PROXY=http://proxy.company.com:8080
```

---

## 9. Storage & Artifacts

### 9.1 Test Results Location
- **Test Results**: `NewFramework/CSharpTests/TestResults/`
- **Allure Reports**: `NewFramework/CSharpTests/allure-results/`
- **Screenshots**: `NewFramework/CSharpTests/screenshots/`
- **Logs**: `NewFramework/CSharpTests/logs/`

### 9.2 Disk Space Management
```bash
# Clean old test results
find NewFramework/CSharpTests/TestResults -type f -mtime +7 -delete

# Clean build artifacts
dotnet clean
rm -rf NewFramework/CSharpTests/bin
rm -rf NewFramework/CSharpTests/obj
```

---

## 10. Troubleshooting

### 10.1 Common Issues

#### Issue: "Microsoft.CognitiveServices.Speech" native library not found
**Solution**:
```bash
sudo apt-get install -y libasound2 libssl1.1
# Or for newer Ubuntu
sudo apt-get install -y libasound2 libssl3
```

#### Issue: Playwright browsers not found
**Solution**:
```bash
# Reinstall with dependencies
npx playwright install --with-deps chromium firefox
```

#### Issue: Permission denied errors
**Solution**:
```bash
# Ensure agent user has proper permissions
sudo chown -R azureagent:azureagent /path/to/AZ_ML_Workspace
chmod +x NewFramework/CSharpTests/*.sh
```

#### Issue: .NET SDK not found
**Solution**:
```bash
# Verify PATH
echo $PATH | grep dotnet

# Add to ~/.bashrc or ~/.profile
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
```

### 10.2 Diagnostic Commands
```bash
# Check .NET installation
dotnet --info

# Check Node.js installation
node --version
npm --version

# Check Playwright browsers
npx playwright --version
npx playwright install --dry-run

# Check Azure connectivity
curl -I https://management.azure.com

# Test Azure authentication
az login --service-principal \
  --username $AZURE_CLIENT_ID \
  --password $AZURE_CLIENT_SECRET \
  --tenant $AZURE_TENANT_ID
```

---

## 11. Performance Optimization

### 11.1 Parallel Test Execution
```bash
# Run tests in parallel (adjust based on CPU cores)
dotnet test --parallel --max-cpu-count:4
```

### 11.2 Build Caching
```bash
# Use incremental builds
dotnet build --no-restore

# Cache NuGet packages
export NUGET_PACKAGES=$HOME/.nuget/packages
```

### 11.3 Playwright Browser Caching
```bash
# Set Playwright cache directory
export PLAYWRIGHT_BROWSERS_PATH=$HOME/.cache/ms-playwright
```

---

## 12. Security Best Practices

### 12.1 Secrets Management
- **Never commit secrets** to appsettings.json
- Use **Azure Key Vault** for production secrets
- Use **Azure DevOps Variable Groups** for pipeline secrets
- Rotate credentials regularly

### 12.2 Agent Isolation
- Run agent in **dedicated VM/container**
- Use **least privilege** service principal
- Enable **audit logging**
- Regularly update agent software

---

## 13. Quick Setup Script

```bash
#!/bin/bash
# setup-linux-agent.sh

set -e

echo "=== Installing .NET 9.0 SDK ==="
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

echo "=== Installing Node.js 18.x ==="
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

echo "=== Installing System Dependencies ==="
sudo apt-get update
sudo apt-get install -y \
    git curl wget unzip \
    libicu-dev libc6-dev libgdiplus \
    libx11-dev libssl-dev ca-certificates \
    libasound2 libssl1.1

echo "=== Installing Playwright Browsers ==="
npx playwright install --with-deps chromium firefox

echo "=== Configuring Environment ==="
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc

echo "=== Verifying Installation ==="
dotnet --version
node --version
npm --version
npx playwright --version

echo "=== Setup Complete! ==="
echo "Next steps:"
echo "1. Configure Azure credentials in appsettings.json"
echo "2. Run: cd NewFramework/CSharpTests && dotnet restore"
echo "3. Run: dotnet build"
echo "4. Run: dotnet test"
```

---

## 14. Summary Checklist

- [ ] Ubuntu 20.04+ or compatible Linux distribution
- [ ] .NET 9.0 SDK installed
- [ ] Node.js 18.x installed
- [ ] Playwright browsers installed with dependencies
- [ ] Azure DevOps agent configured
- [ ] System dependencies installed (libicu, libasound2, etc.)
- [ ] Azure credentials configured (Service Principal or Managed Identity)
- [ ] appsettings.json configured with Azure endpoints
- [ ] Network access to Azure services verified
- [ ] Test execution successful: `dotnet test`
- [ ] Allure reporting configured (optional)

---

## 15. Support & Resources

- **Project Repository**: `/Users/oldguard/Documents/GitHub/AZ_ML_Workspace`
- **Main Test Project**: `NewFramework/CSharpTests/PlaywrightFramework.csproj`
- **Configuration**: `NewFramework/Config/appsettings.json`
- **Azure Pipelines**: `azure-pipelines.yml`
- **Documentation**: `NewFramework/Documentation/`

### External Resources
- [.NET Installation Guide](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
- [Playwright for .NET](https://playwright.dev/dotnet/)
- [Azure DevOps Self-Hosted Agents](https://learn.microsoft.com/en-us/azure/devops/pipelines/agents/linux-agent)
- [Azure SDK for .NET](https://learn.microsoft.com/en-us/dotnet/azure/)

---

**Document Version**: 1.0  
**Last Updated**: 2025-01-30  
**Target Framework**: .NET 9.0  
**Playwright Version**: 1.40.0