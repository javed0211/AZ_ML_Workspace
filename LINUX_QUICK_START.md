# Linux Self-Hosted Agent - Quick Start Guide

## 🚀 One-Command Setup

```bash
curl -fsSL https://raw.githubusercontent.com/your-repo/setup-linux-agent.sh | bash
```

---

## 📋 Prerequisites Checklist

```bash
# Check all prerequisites at once
./check-prerequisites.sh
```

Or manually:

```bash
# 1. Check .NET 9.0
dotnet --version  # Should be 9.0.x

# 2. Check Node.js 18+
node --version    # Should be v18.x

# 3. Check Playwright
npx playwright --version

# 4. Check Azure CLI (optional)
az --version
```

---

## ⚡ Quick Commands

### Setup (First Time Only)
```bash
# 1. Clone repository
git clone https://github.com/your-org/AZ_ML_Workspace.git
cd AZ_ML_Workspace

# 2. Install dependencies
cd NewFramework/CSharpTests
dotnet restore

# 3. Install Playwright browsers
npx playwright install --with-deps chromium firefox

# 4. Configure Azure credentials
cp ../Config/appsettings.example.json ../Config/appsettings.json
nano ../Config/appsettings.json  # Edit with your Azure credentials
```

### Build & Test
```bash
# Build
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release

# Run specific feature
dotnet test --filter "FullyQualifiedName~AzureSpeechServices"

# Run with detailed logging
dotnet test --logger "console;verbosity=detailed"
```

### Common Test Filters
```bash
# Azure Speech Services only
dotnet test --filter "Category=SpeechServices"

# Azure AI Search only
dotnet test --filter "Category=AISearch"

# Document Intelligence only
dotnet test --filter "Category=DocumentIntelligence"

# Exclude retired tests
dotnet test --filter "Category!=retired"

# BDD tests only
dotnet test --filter "TestCategory=BDD"
```

---

## 🔧 Environment Variables

### Required for Azure Authentication
```bash
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_CLIENT_ID="your-client-id"
export AZURE_CLIENT_SECRET="your-client-secret"
export AZURE_SUBSCRIPTION_ID="your-subscription-id"
```

### Optional Overrides
```bash
export AZURE_RESOURCE_GROUP="your-rg"
export AZURE_WORKSPACE_NAME="your-workspace"
export AZUREAISEARCH_SERVICENAME="your-search-service"
export AZUREAISEARCH_INDEXNAME="your-index"
export HEADLESS=true  # For headless browser mode
```

### Save to Profile (Persistent)
```bash
# Add to ~/.bashrc or ~/.profile
cat >> ~/.bashrc << 'EOF'
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_CLIENT_ID="your-client-id"
export AZURE_CLIENT_SECRET="your-client-secret"
export AZURE_SUBSCRIPTION_ID="your-subscription-id"
EOF

source ~/.bashrc
```

---

## 🐛 Troubleshooting

### Issue: .NET not found
```bash
# Install .NET 9.0
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

# Add to PATH
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT
```

### Issue: Playwright browsers not found
```bash
# Reinstall with system dependencies
npx playwright install --with-deps chromium firefox
```

### Issue: Speech SDK native library error
```bash
# Ubuntu/Debian
sudo apt-get install -y libasound2 libssl1.1

# If libssl1.1 not available (Ubuntu 22.04+)
sudo apt-get install -y libasound2 libssl3
```

### Issue: Permission denied
```bash
# Fix ownership
sudo chown -R $USER:$USER ~/AZ_ML_Workspace

# Make scripts executable
chmod +x NewFramework/CSharpTests/*.sh
```

### Issue: Azure authentication fails
```bash
# Test Azure connectivity
curl -I https://management.azure.com

# Test service principal login
az login --service-principal \
  --username $AZURE_CLIENT_ID \
  --password $AZURE_CLIENT_SECRET \
  --tenant $AZURE_TENANT_ID

# Verify access to resources
az ml workspace show \
  --name $AZURE_WORKSPACE_NAME \
  --resource-group $AZURE_RESOURCE_GROUP
```

---

## 📊 Generate Reports

### Allure Reports
```bash
# Install Allure (one-time)
sudo apt-get install -y default-jre
wget https://github.com/allure-framework/allure2/releases/download/2.24.0/allure-2.24.0.tgz
tar -zxvf allure-2.24.0.tgz -C /opt/
sudo ln -s /opt/allure-2.24.0/bin/allure /usr/bin/allure

# Generate report
cd NewFramework/CSharpTests
./generate-allure-report.sh

# View report
allure serve allure-results
```

### TRX Reports (Azure DevOps)
```bash
dotnet test --logger "trx;LogFileName=test-results.trx"
# Results in: TestResults/test-results.trx
```

---

## 🔄 CI/CD Integration

### Azure DevOps Pipeline
The project includes `azure-pipelines.yml` configured for Linux agents.

**Key pipeline variables to set:**
- `AZURE_SUBSCRIPTION_ID`
- `AZURE_TENANT_ID`
- `AZURE_CLIENT_ID`
- `AZURE_CLIENT_SECRET`
- `AZURE_RESOURCE_GROUP`
- `AZURE_WORKSPACE_NAME`
- `AZUREAISEARCH_SERVICENAME`
- `AZUREAISEARCH_INDEXNAME`
- `AZUREAISEARCH_ADMINKEY`

### GitHub Actions (Alternative)
```yaml
# .github/workflows/test.yml
name: Run Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install Playwright
        run: npx playwright install --with-deps chromium
      
      - name: Restore dependencies
        run: dotnet restore NewFramework/CSharpTests/PlaywrightFramework.csproj
      
      - name: Build
        run: dotnet build NewFramework/CSharpTests/PlaywrightFramework.csproj --no-restore
      
      - name: Test
        run: dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj --no-build
        env:
          AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
          AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
          AZURE_CLIENT_SECRET: ${{ secrets.AZURE_CLIENT_SECRET }}
          AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

---

## 📁 Important Paths

| Item | Path |
|------|------|
| **Project Root** | `/path/to/AZ_ML_Workspace` |
| **C# Tests** | `NewFramework/CSharpTests` |
| **Project File** | `NewFramework/CSharpTests/PlaywrightFramework.csproj` |
| **Configuration** | `NewFramework/Config/appsettings.json` |
| **Features** | `NewFramework/CSharpTests/Features/*.feature` |
| **Test Results** | `NewFramework/CSharpTests/TestResults/` |
| **Logs** | `NewFramework/CSharpTests/logs/` |
| **Screenshots** | `NewFramework/CSharpTests/screenshots/` |

---

## 🎯 Test Coverage

### Available Test Suites
- ✅ **Azure ML Workspace** - Workspace operations, compute management
- ✅ **Azure AI Search** - Index operations, search queries
- ✅ **Document Intelligence** - Document analysis, form recognition
- ✅ **Speech Services** - Speech-to-Text, Text-to-Speech, Translation
- ✅ **Azure ML Compute Automation** - VM operations, SSH connectivity
- ✅ **Integration Tests** - Cross-service scenarios

### Test Statistics
- **Total Scenarios**: 50+
- **Active Scenarios**: 46+
- **Retired Scenarios**: 4 (Speaker Recognition API)
- **Feature Files**: 7
- **Step Definitions**: 600+ steps

---

## 💡 Pro Tips

### 1. Parallel Execution
```bash
# Run tests in parallel (faster)
dotnet test --parallel --max-cpu-count:4
```

### 2. Watch Mode (Development)
```bash
# Auto-run tests on file changes
dotnet watch test
```

### 3. Selective Test Execution
```bash
# Run only failed tests from last run
dotnet test --filter "TestCategory=Failed"

# Run tests matching pattern
dotnet test --filter "DisplayName~Speech"
```

### 4. Clean Build
```bash
# Clean and rebuild
dotnet clean
dotnet build --no-incremental
```

### 5. Verbose Logging
```bash
# Maximum verbosity
dotnet test -v diag --logger "console;verbosity=detailed"
```

### 6. Headless vs Headed Mode
```bash
# Headless (default for CI/CD)
export HEADLESS=true
dotnet test

# Headed (for debugging, requires X server)
export HEADLESS=false
xvfb-run dotnet test
```

---

## 📞 Support

### Documentation
- **Full Requirements**: `LINUX_SELF_HOSTED_AGENT_REQUIREMENTS.md`
- **Workspace Info**: `WORKSPACE_CONSOLIDATION_SUMMARY.md`
- **Azure Speech Services**: `NewFramework/CSharpTests/README_AZURE_SPEECH_SERVICES.md`
- **Azure AI Services**: `NewFramework/CSharpTests/README_AZURE_AI_SERVICES_TESTS.md`

### Logs Location
```bash
# View recent test logs
tail -f NewFramework/CSharpTests/logs/test-execution.log

# View Playwright traces
ls -la NewFramework/CSharpTests/test-results/
```

### Health Check Script
```bash
#!/bin/bash
# health-check.sh

echo "=== System Health Check ==="

echo -n ".NET SDK: "
dotnet --version 2>/dev/null && echo "✅" || echo "❌"

echo -n "Node.js: "
node --version 2>/dev/null && echo "✅" || echo "❌"

echo -n "Playwright: "
npx playwright --version 2>/dev/null && echo "✅" || echo "❌"

echo -n "Azure Credentials: "
[[ -n "$AZURE_TENANT_ID" ]] && echo "✅" || echo "❌"

echo -n "Project Build: "
cd NewFramework/CSharpTests && dotnet build --no-restore > /dev/null 2>&1 && echo "✅" || echo "❌"

echo "=== Health Check Complete ==="
```

---

## 🚦 Status Indicators

### Build Status
```bash
# Check if project builds
dotnet build --no-restore && echo "✅ Build OK" || echo "❌ Build Failed"
```

### Test Status
```bash
# Quick smoke test
dotnet test --filter "TestCategory=Smoke" && echo "✅ Tests OK" || echo "❌ Tests Failed"
```

### Azure Connectivity
```bash
# Check Azure endpoints
curl -s -o /dev/null -w "%{http_code}" https://management.azure.com | grep -q 401 && echo "✅ Azure Reachable" || echo "❌ Azure Unreachable"
```

---

**Last Updated**: 2025-01-30  
**Version**: 1.0  
**Target Platform**: Linux (Ubuntu 20.04+)