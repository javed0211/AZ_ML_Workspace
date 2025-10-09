# Setup Summary - Workspace Consolidation & Linux Agent Requirements

## ✅ Completed Tasks

### 1. Workspace File Consolidation

#### Actions Taken
- ✅ Compared two workspace files:
  - `AzureML-BDD-Framework.code-workspace` (outdated)
  - `AzureMLTestFramework.code-workspace` (current)
- ✅ Deleted outdated workspace file
- ✅ Kept current, accurate workspace file

#### Key Differences Found

| Aspect | Old (Deleted) | New (Kept) |
|--------|---------------|------------|
| **Paths** | ❌ Points to non-existent `src/AzureML.BDD.*` | ✅ Points to actual `CSharpTests/` |
| **Extensions** | ❌ Deprecated `alexkrechik.cucumberautocomplete` | ✅ Modern `reqnroll.reqnroll-official` |
| **Settings** | ⚠️ Basic only | ✅ Comprehensive (Playwright, formatting, etc.) |
| **Tasks** | ⚠️ Generic | ✅ Specific (validation, Azure ML runner) |
| **Debug** | ⚠️ Single config | ✅ Multiple configs |

#### Result
- **Single workspace file**: `AzureMLTestFramework.code-workspace`
- **All paths verified**: Point to actual directories
- **Modern tooling**: Uses Reqnroll, official extensions
- **Better organization**: 6 logical folders including Features

---

### 2. Linux Self-Hosted Agent Documentation

#### Created Documents

1. **LINUX_SELF_HOSTED_AGENT_REQUIREMENTS.md** (Comprehensive)
   - Complete system requirements
   - All software dependencies
   - Azure configuration
   - Build & test commands
   - Troubleshooting guide
   - 15 sections, ~500 lines

2. **LINUX_QUICK_START.md** (Quick Reference)
   - One-command setup
   - Quick commands
   - Common test filters
   - Troubleshooting tips
   - Pro tips for developers

3. **WORKSPACE_CONSOLIDATION_SUMMARY.md**
   - Detailed comparison of workspace files
   - Migration notes
   - Recommendations

4. **setup-linux-agent.sh** (Automated Setup)
   - Detects OS (Ubuntu/Debian/RHEL/CentOS)
   - Installs all dependencies
   - Configures environment
   - Verifies installation
   - Executable script

5. **health-check.sh** (Diagnostic Tool)
   - Checks all dependencies
   - Verifies Azure connectivity
   - Tests project build
   - Color-coded output
   - Exit codes for CI/CD

---

## 📋 Linux Agent Requirements Summary

### Core Requirements

#### Software
- ✅ **.NET 9.0 SDK** (as per `net9.0` target framework)
- ✅ **Node.js 18.x** (as per azure-pipelines.yml)
- ✅ **Playwright 1.40.0** with browsers (Chromium, Firefox)
- ✅ **Git, curl, wget** (standard tools)

#### System Libraries
- ✅ **libicu-dev** - Internationalization
- ✅ **libasound2** - Audio support (Speech SDK)
- ✅ **libssl1.1/libssl3** - SSL/TLS
- ✅ **libgdiplus** - Graphics (optional)
- ✅ **libx11-dev** - X11 support

#### Azure Configuration
- ✅ **Service Principal** or **Managed Identity**
- ✅ **RBAC Roles**: Contributor, Cognitive Services User, Search Service Contributor
- ✅ **Network Access**: Azure Management API, AI Services, Search, ML endpoints

### NuGet Packages (Auto-Restored)

#### Testing Frameworks
- Microsoft.Playwright (1.40.0)
- NUnit (3.13.3)
- Reqnroll (2.0.3)

#### Azure SDKs
- Azure.Identity (1.12.0)
- Azure.ResourceManager.MachineLearning (1.2.3)
- Azure.Search.Documents (11.5.1)
- Azure.AI.FormRecognizer (4.1.0)
- Microsoft.CognitiveServices.Speech (1.38.0)

#### Utilities
- Serilog (3.1.1)
- FluentAssertions (6.12.0)
- SSH.NET (2023.0.0)
- Allure.NUnit (2.12.1)

---

## 🚀 Quick Start Commands

### Setup (One-Time)
```bash
# Automated setup
./setup-linux-agent.sh

# Or manual setup
cd NewFramework/CSharpTests
dotnet restore
npx playwright install --with-deps chromium firefox
```

### Health Check
```bash
./health-check.sh
```

### Build & Test
```bash
cd NewFramework/CSharpTests

# Build
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release

# Run specific category
dotnet test --filter "Category=SpeechServices"
```

---

## 📁 Project Structure

```
AZ_ML_Workspace/
├── AzureMLTestFramework.code-workspace    ← Active workspace
├── setup-linux-agent.sh                   ← NEW: Automated setup
├── health-check.sh                        ← NEW: Health check
├── LINUX_SELF_HOSTED_AGENT_REQUIREMENTS.md ← NEW: Full requirements
├── LINUX_QUICK_START.md                   ← NEW: Quick reference
├── WORKSPACE_CONSOLIDATION_SUMMARY.md     ← NEW: Workspace comparison
├── azure-pipelines.yml                    ← CI/CD configuration
└── NewFramework/
    ├── CSharpTests/
    │   ├── PlaywrightFramework.csproj     ← Main project file
    │   ├── Features/                      ← BDD feature files
    │   ├── StepDefinitions/               ← Step implementations
    │   ├── Utils/                         ← Helper classes
    │   └── Tests/                         ← Unit tests
    ├── Config/
    │   └── appsettings.json               ← Azure configuration
    └── Documentation/
```

---

## 🔧 Configuration Required

### 1. Azure Credentials (Choose One)

#### Option A: Environment Variables
```bash
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_CLIENT_ID="your-client-id"
export AZURE_CLIENT_SECRET="your-client-secret"
export AZURE_SUBSCRIPTION_ID="your-subscription-id"
```

#### Option B: appsettings.json
```json
{
  "Environment": "dev",
  "Environments": {
    "dev": {
      "AzureML": {
        "SubscriptionId": "...",
        "TenantId": "...",
        "ResourceGroup": "...",
        "WorkspaceName": "..."
      },
      "SpeechServices": {
        "SubscriptionKey": "...",
        "Region": "...",
        "Endpoint": "..."
      }
    }
  }
}
```

### 2. Azure RBAC Roles
Assign to Service Principal/Managed Identity:
- **Contributor** on Azure ML Workspace
- **Cognitive Services User** for AI Services
- **Search Service Contributor** for Azure AI Search

### 3. Network Access
Allow outbound HTTPS (443) to:
- `*.azure.com`
- `*.cognitiveservices.azure.com`
- `*.search.windows.net`
- `*.api.azureml.ms`

---

## 🧪 Test Suites Available

| Test Suite | Category Filter | Scenarios |
|------------|----------------|-----------|
| **Azure Speech Services** | `Category=SpeechServices` | 46+ (4 retired) |
| **Azure AI Search** | `Category=AISearch` | 20+ |
| **Document Intelligence** | `Category=DocumentIntelligence` | 15+ |
| **Azure ML Workspace** | `Category=AzureML` | 10+ |
| **Azure ML Compute** | `Category=Compute` | 15+ |
| **Integration Tests** | `Category=Integration` | 10+ |

### Run Specific Tests
```bash
# Speech Services only
dotnet test --filter "Category=SpeechServices"

# Exclude retired tests
dotnet test --filter "Category!=retired"

# Specific feature
dotnet test --filter "FullyQualifiedName~AzureSpeechServices"

# Parallel execution
dotnet test --parallel --max-cpu-count:4
```

---

## 🐛 Troubleshooting

### Common Issues & Solutions

#### Issue: .NET SDK not found
```bash
# Install .NET 9.0
./setup-linux-agent.sh
# Or manually:
wget https://dot.net/v1/dotnet-install.sh
./dotnet-install.sh --channel 9.0
```

#### Issue: Playwright browsers not found
```bash
npx playwright install --with-deps chromium firefox
```

#### Issue: Speech SDK native library error
```bash
# Ubuntu/Debian
sudo apt-get install -y libasound2 libssl1.1
# Or for Ubuntu 22.04+
sudo apt-get install -y libasound2 libssl3
```

#### Issue: Azure authentication fails
```bash
# Test connectivity
curl -I https://management.azure.com

# Verify credentials
az login --service-principal \
  --username $AZURE_CLIENT_ID \
  --password $AZURE_CLIENT_SECRET \
  --tenant $AZURE_TENANT_ID
```

### Diagnostic Commands
```bash
# Run health check
./health-check.sh

# Check .NET
dotnet --info

# Check Playwright
npx playwright --version
npx playwright install --dry-run

# Check project build
cd NewFramework/CSharpTests
dotnet build --no-restore
```

---

## 📊 CI/CD Integration

### Azure DevOps Pipeline
The project includes `azure-pipelines.yml` configured for:
- ✅ Multi-platform testing (Linux, Windows, macOS)
- ✅ Multiple browsers (Chromium, Firefox, Edge, Safari)
- ✅ Parallel execution
- ✅ Test result publishing
- ✅ Code coverage reports
- ✅ Allure reporting

### Required Pipeline Variables
Set in Azure DevOps Variable Groups:
- `AZURE_SUBSCRIPTION_ID`
- `AZURE_TENANT_ID`
- `AZURE_CLIENT_ID`
- `AZURE_CLIENT_SECRET`
- `AZURE_RESOURCE_GROUP`
- `AZURE_WORKSPACE_NAME`
- `AZUREAISEARCH_SERVICENAME`
- `AZUREAISEARCH_INDEXNAME`
- `AZUREAISEARCH_ADMINKEY`

---

## 📈 Performance Optimization

### Build Caching
```bash
# Cache NuGet packages
export NUGET_PACKAGES=$HOME/.nuget/packages

# Incremental builds
dotnet build --no-restore
```

### Parallel Testing
```bash
# Use multiple CPU cores
dotnet test --parallel --max-cpu-count:4
```

### Playwright Browser Caching
```bash
# Set cache directory
export PLAYWRIGHT_BROWSERS_PATH=$HOME/.cache/ms-playwright
```

---

## 🔒 Security Best Practices

1. **Never commit secrets** to appsettings.json
2. Use **Azure Key Vault** for production secrets
3. Use **Azure DevOps Variable Groups** for pipeline secrets
4. Rotate credentials regularly
5. Use **least privilege** service principal
6. Enable **audit logging**
7. Run agent in **dedicated VM/container**

---

## 📚 Documentation Index

| Document | Purpose | Audience |
|----------|---------|----------|
| **LINUX_SELF_HOSTED_AGENT_REQUIREMENTS.md** | Complete requirements & setup | DevOps Engineers |
| **LINUX_QUICK_START.md** | Quick reference & commands | Developers |
| **WORKSPACE_CONSOLIDATION_SUMMARY.md** | Workspace file comparison | Team Leads |
| **setup-linux-agent.sh** | Automated setup script | DevOps Engineers |
| **health-check.sh** | Diagnostic tool | Everyone |
| **azure-pipelines.yml** | CI/CD configuration | DevOps Engineers |
| **README_AZURE_SPEECH_SERVICES.md** | Speech Services tests | QA Engineers |

---

## ✅ Verification Checklist

### Before Running Tests
- [ ] .NET 9.0 SDK installed
- [ ] Node.js 18.x installed
- [ ] Playwright browsers installed
- [ ] System libraries installed
- [ ] Azure credentials configured
- [ ] Network access verified
- [ ] Project builds successfully
- [ ] Health check passes

### Run Verification
```bash
# 1. Health check
./health-check.sh

# 2. Build project
cd NewFramework/CSharpTests
dotnet build

# 3. Run smoke tests
dotnet test --filter "TestCategory=Smoke"

# 4. Run full test suite
dotnet test
```

---

## 🎯 Success Criteria

### Setup Complete When:
- ✅ Health check script passes all critical checks
- ✅ Project builds without errors
- ✅ At least one test runs successfully
- ✅ Azure connectivity verified
- ✅ Test results generated

### Expected Output:
```
=== Health Check Summary ===
Passed:  25
Warnings: 3
Failed:  0

✓ All critical checks passed!
```

---

## 📞 Support & Resources

### Internal Documentation
- `/NewFramework/Documentation/` - Project documentation
- `/NewFramework/CSharpTests/README*.md` - Test suite documentation

### External Resources
- [.NET Installation](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
- [Playwright for .NET](https://playwright.dev/dotnet/)
- [Azure DevOps Agents](https://learn.microsoft.com/en-us/azure/devops/pipelines/agents/linux-agent)
- [Azure SDK for .NET](https://learn.microsoft.com/en-us/dotnet/azure/)

---

## 📝 Change Log

| Date | Change | Impact |
|------|--------|--------|
| 2025-01-30 | Workspace consolidation | Removed outdated workspace file |
| 2025-01-30 | Linux requirements documented | Complete setup guide created |
| 2025-01-30 | Automated setup script | One-command installation |
| 2025-01-30 | Health check script | Automated verification |

---

**Status**: ✅ Complete  
**Last Updated**: 2025-01-30  
**Version**: 1.0  
**Target Platform**: Linux (Ubuntu 20.04+, RHEL 8+, Debian 11+)