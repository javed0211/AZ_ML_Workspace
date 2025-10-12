# C# Playwright Test Framework

A comprehensive test automation framework built with .NET 9.0, Playwright, NUnit, and Reqnroll (BDD) for testing Azure ML and Azure AI services.

## 🚀 Quick Start

### Prerequisites

- .NET 9.0 SDK
- Playwright browsers installed
- Azure credentials configured

### Installation

```bash
# Restore dependencies
dotnet restore

# Install Playwright browsers
pwsh bin/Debug/net9.0/playwright.ps1 install

# Or on Unix/macOS
./bin/Debug/net9.0/playwright.sh install
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with custom settings
dotnet test --settings .runsettings

# Run specific test category
dotnet test --filter "Category=Smoke"

# Run in headless mode
dotnet test -- RunConfiguration.EnvironmentVariables.HEADLESS=true

# Run on specific browser
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=firefox
```

## 📋 Features

### Multi-Browser Support
- ✅ Chromium (Chrome, Edge)
- ✅ Firefox
- ✅ WebKit (Safari)
- ✅ Configurable via environment variables

### Test Frameworks
- **NUnit** - Primary test framework
- **Reqnroll** - BDD/Gherkin support
- **Playwright** - Browser automation
- **FluentAssertions** - Readable assertions

### Reporting
- **Allure** - Rich HTML reports with screenshots and traces
- **TRX** - Visual Studio test results
- **HTML** - Built-in HTML reports
- **Console** - Real-time test output

### Azure Services Integration
- Azure ML Workspace automation
- Azure AI Search
- Azure Document Intelligence
- Azure Speech Services
- Compute instance management

## 🎯 Configuration

### Environment Variables

| Variable | Options | Default | Description |
|----------|---------|---------|-------------|
| `HEADLESS` | `true`/`false` | `false` | Run browsers in headless mode |
| `BROWSER` | `chromium`/`firefox`/`webkit`/`all` | `chromium` | Browser selection |
| `SLOW_MO` | `0-5000` | `0` | Slow motion delay (ms) |
| `WORKERS` | `1-N` | `3` | Parallel test workers |
| `RETRIES` | `0-N` | `0` | Test retry count |
| `TIMEOUT` | milliseconds | `30000` | Test timeout |
| `TRACE` | `on`/`off`/`retain-on-failure` | `retain-on-failure` | Playwright trace |
| `VIDEO` | `on`/`off`/`retain-on-failure` | `off` | Video recording |
| `SCREENSHOT` | `on`/`off`/`only-on-failure` | `only-on-failure` | Screenshot capture |

### Configuration Files

- **`.runsettings`** - Test execution configuration
- **`reqnroll.json`** - BDD/Gherkin configuration
- **`allureConfig.json`** - Allure reporting configuration
- **`.env.example`** - Environment variable template

## 📝 Common Commands

### Test Execution

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~AzureMLWorkspaceTests"

# Run tests by category
dotnet test --filter "Category=Integration"

# Run tests by priority
dotnet test --filter "Priority=1"
```

### Browser Selection

```bash
# Chromium only
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=chromium

# Firefox only
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=firefox

# WebKit only
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=webkit

# All browsers
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=all
```

### Debugging

```bash
# Headed mode with slow motion
dotnet test -- RunConfiguration.EnvironmentVariables.HEADLESS=false RunConfiguration.EnvironmentVariables.SLOW_MO=500

# Single worker (no parallelization)
dotnet test -- RunConfiguration.EnvironmentVariables.WORKERS=1

# Enable all traces and videos
dotnet test -- RunConfiguration.EnvironmentVariables.TRACE=on RunConfiguration.EnvironmentVariables.VIDEO=on
```

### Reporting

```bash
# Generate Allure report (PowerShell)
.\generate-allure-report.ps1

# Generate Allure report (Bash)
./generate-allure-report.sh

# View Allure report
allure serve allure-results
```

## 🏗️ Project Structure

```
CSharpTests/
├── Features/                    # BDD feature files
│   ├── AzureMLWorkspace.feature
│   ├── AzureAISearch.feature
│   ├── AzureMLComputeAutomation.feature
│   └── ...
├── StepDefinitions/            # BDD step implementations
│   ├── AzureMLWorkspaceSteps.cs
│   ├── AzureAISearchSteps.cs
│   └── ...
├── Tests/                      # NUnit test classes
│   ├── ApiIntegrationTest.cs
│   ├── AzureMLComputeAutomationTests.cs
│   └── ...
├── Utils/                      # Helper utilities
│   ├── PlaywrightUtils.cs
│   ├── AzureMLUtils.cs
│   ├── ConfigManager.cs
│   └── Logger.cs
├── Hooks/                      # Test lifecycle hooks
│   └── TestHooks.cs
├── TestData/                   # Test data files
├── .runsettings               # Test configuration
├── reqnroll.json              # BDD configuration
├── allureConfig.json          # Allure configuration
└── PlaywrightFramework.csproj # Project file
```

## 🧪 Test Categories

Tests are organized by categories for selective execution:

- **`Smoke`** - Quick smoke tests
- **`Integration`** - Integration tests
- **`E2E`** - End-to-end tests
- **`API`** - API tests
- **`UI`** - UI tests
- **`Azure`** - Azure-specific tests

```bash
# Run smoke tests only
dotnet test --filter "Category=Smoke"

# Run integration tests
dotnet test --filter "Category=Integration"

# Exclude UI tests
dotnet test --filter "Category!=UI"
```

## 🔧 Advanced Usage

### Parallel Execution

```bash
# Run with 5 parallel workers
dotnet test -- NUnit.NumberOfTestWorkers=5

# Or via environment variable
dotnet test -- RunConfiguration.EnvironmentVariables.WORKERS=5
```

### Test Filtering

```bash
# By namespace
dotnet test --filter "FullyQualifiedName~PlaywrightFramework.Tests"

# By test name pattern
dotnet test --filter "Name~Azure"

# By multiple criteria
dotnet test --filter "(Category=Smoke|Category=Integration)&Priority=1"
```

### Custom Test Parameters

```bash
# Pass custom parameters
dotnet test -- NUnit.TestParameters.baseUrl=https://example.com

# Access in tests via TestContext.Parameters["baseUrl"]
```

## 📊 Reporting

### Allure Reports

1. Run tests to generate results:
   ```bash
   dotnet test
   ```

2. Generate and view report:
   ```bash
   # PowerShell
   .\generate-allure-report.ps1
   
   # Bash
   ./generate-allure-report.sh
   ```

3. Or manually:
   ```bash
   allure generate allure-results --clean -o allure-report
   allure open allure-report
   ```

### TRX Reports

TRX files are automatically generated in `TestResults/` directory and can be viewed in Visual Studio or Azure DevOps.

## 🐛 Troubleshooting

### Playwright Browsers Not Found

```bash
# Install browsers
pwsh bin/Debug/net9.0/playwright.ps1 install

# Or with dependencies
pwsh bin/Debug/net9.0/playwright.ps1 install --with-deps
```

### Tests Timing Out

```bash
# Increase timeout
dotnet test -- RunConfiguration.EnvironmentVariables.TIMEOUT=60000
```

### Parallel Execution Issues

```bash
# Reduce workers or disable parallelization
dotnet test -- RunConfiguration.EnvironmentVariables.WORKERS=1
```

### Azure Authentication Issues

```bash
# Login with Azure CLI
az login

# Set subscription
az account set --subscription "your-subscription-id"
```

## 📚 Additional Documentation

- **[Configuration Guide](./CONFIGURATION_GUIDE.md)** - Detailed configuration options
- **[Quick Reference](./QUICK_REFERENCE.md)** - Command cheat sheet
- **[BDD Guide](./BDD_GUIDE.md)** - Writing BDD tests with Reqnroll
- **[Azure Setup](./AZURE_SETUP.md)** - Azure services configuration

## 🤝 Contributing

1. Follow existing code structure
2. Add tests for new features
3. Update documentation
4. Use meaningful commit messages

## 📄 License

[Your License Here]

## 🔗 Links

- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [NUnit Documentation](https://docs.nunit.org/)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [Allure Documentation](https://docs.qameta.io/allure/)