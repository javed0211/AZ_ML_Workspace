# 🚀 Quick Start Guide

Get up and running with the Azure ML Test Automation Framework in minutes!

## Prerequisites Checklist

- [ ] **.NET 8.0 SDK** installed ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- [ ] **Git** installed
- [ ] **Azure CLI** (optional, for authentication)
- [ ] **PowerShell 7+** (Windows) or **Bash** (macOS/Linux)

## 1. Clone and Setup

```bash
# Clone the repository
git clone <your-repo-url>
cd AZ_ML_Workspace

# Restore dependencies
dotnet restore AzureMLWorkspace.Tests/

# Build the solution
dotnet build AzureMLWorkspace.Tests/
```

## 2. Configure Environment

### Option A: Environment Variables (Recommended for CI/CD)
```bash
# Set Azure configuration
export AZURE_SUBSCRIPTION_ID="your-subscription-id"
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_RESOURCE_GROUP="your-resource-group"
export AZURE_WORKSPACE_NAME="your-workspace-name"

# Set Azure AI Search configuration
export AZUREAISEARCH_SERVICENAME="your-search-service"
export AZUREAISEARCH_INDEXNAME="your-search-index"
```

### Option B: Configuration Files (Recommended for local development)
Edit `AzureMLWorkspace.Tests/appsettings.json`:

```json
{
  "Azure": {
    "SubscriptionId": "your-subscription-id",
    "TenantId": "your-tenant-id",
    "ResourceGroup": "your-resource-group",
    "WorkspaceName": "your-workspace-name"
  },
  "AzureAISearch": {
    "ServiceName": "your-search-service",
    "IndexName": "your-search-index"
  }
}
```

## 3. Run Your First Tests

### Windows (PowerShell)
```powershell
# Run all tests
.\run-tests.ps1

# Run UI tests with visible browser
.\run-tests.ps1 -Category "UI" -Browser "chromium" -Headless:$false

# Run API tests with coverage
.\run-tests.ps1 -Category "API" -Coverage

# Get help
.\run-tests.ps1 -Help
```

### macOS/Linux (Bash)
```bash
# Make script executable (first time only)
chmod +x run-tests.sh

# Run all tests
./run-tests.sh

# Run UI tests with visible browser
./run-tests.sh --category "UI" --browser "chromium" --headed

# Run API tests with coverage
./run-tests.sh --category "API" --coverage

# Get help
./run-tests.sh --help
```

### Direct .NET Commands
```bash
# Run all tests
dotnet test AzureMLWorkspace.Tests/

# Run specific category
dotnet test AzureMLWorkspace.Tests/ --filter Category=UI

# Run with coverage
dotnet test AzureMLWorkspace.Tests/ --collect:"XPlat Code Coverage"
```

## 4. View Results

After test execution, check these locations:

```
TestResults/
├── Screenshots/           # Test screenshots
├── Videos/               # Test recordings (if enabled)
├── Traces/               # Playwright traces
├── Logs/                 # Execution logs
├── CoverageReport/       # Coverage reports
├── *.trx                 # Test result files
└── *.html                # HTML reports
```

### Open Playwright Traces (for debugging)
```bash
# Install Playwright CLI (first time only)
dotnet tool install --global Microsoft.Playwright.CLI

# View trace
playwright show-trace TestResults/Traces/trace.zip
```

## 5. Write Your First Test

Create a new test file in `AzureMLWorkspace.Tests/Tests/`:

```csharp
using AzureMLWorkspace.Tests.Framework;
using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Tasks;
using AzureMLWorkspace.Tests.Framework.Questions;

namespace AzureMLWorkspace.Tests.Tests;

[TestFixture]
[Category("UI")]
public class MyFirstTests : TestBase
{
    [Test]
    public async Task Should_Access_Workspace_Successfully()
    {
        // Arrange
        var user = CreateActor("TestUser")
            .Can(BrowseTheWeb.Headlessly(GetLogger<BrowseTheWeb>()))
            .Can(UseAzureML.AsContributor());

        // Act & Assert
        await user
            .AttemptsTo(NavigateTo.AzureMLPortal())
            .And(OpenWorkspace.Named("my-workspace"))
            .Should(Validate.WorkspaceAccess("my-workspace"));
    }
}
```

## 6. Common Commands Reference

### Test Categories
- `UI` - Browser-based tests
- `API` - Azure SDK/REST API tests
- `BDD` - Behavior-driven scenarios
- `Integration` - End-to-end tests
- `Performance` - Performance tests
- `Security` - Security tests

### Browser Options
- `chromium` - Google Chrome/Chromium
- `firefox` - Mozilla Firefox
- `webkit` - Safari WebKit
- `all` - Run on all browsers

### Environment Options
- `test` - Test environment
- `staging` - Staging environment
- `production` - Production environment

## 7. Troubleshooting

### Common Issues

**Issue: "Playwright browsers not found"**
```bash
# Solution: Install browsers
dotnet run --project AzureMLWorkspace.Tests -- playwright install --with-deps
```

**Issue: "Azure authentication failed"**
```bash
# Solution: Login with Azure CLI
az login
```

**Issue: "Tests fail with timeout"**
```bash
# Solution: Increase timeout in appsettings.json
{
  "TestExecution": {
    "DefaultTimeoutSeconds": 60
  }
}
```

**Issue: "Permission denied on run-tests.sh"**
```bash
# Solution: Make script executable
chmod +x run-tests.sh
```

### Debug Mode
Run tests in debug mode to see what's happening:

```bash
# Windows
.\run-tests.ps1 -Category "UI" -Headless:$false -Browser "chromium"

# macOS/Linux
./run-tests.sh --category "UI" --headed --browser "chromium"
```

### Verbose Logging
Enable debug logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "AzureMLWorkspace.Tests": "Debug"
    }
  }
}
```

## 8. Next Steps

1. **Explore Sample Tests** - Check `Tests/` directory for examples
2. **Read BDD Features** - Look at `Features/` for business scenarios
3. **Customize Configuration** - Modify `appsettings.json` for your needs
4. **Add Custom Tasks** - Create new tasks in `Framework/Tasks/`
5. **Set up CI/CD** - Use provided GitHub Actions or Azure DevOps pipelines

## 🆘 Need Help?

- **Documentation**: Check the main [README.md](AzureMLWorkspace.Tests/README.md)
- **Examples**: Browse the `Tests/` and `Features/` directories
- **Issues**: Create an issue in the repository
- **Debugging**: Use Playwright trace viewer for detailed analysis

## 🎉 You're Ready!

You now have a modern, cross-platform test automation framework running. Start writing tests using the Screenplay pattern and enjoy the power of Azure SDK integration!

---

**Happy Testing! 🧪✨**