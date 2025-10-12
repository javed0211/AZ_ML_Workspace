# C# Playwright Test Framework - Quick Reference

Quick command reference for common testing operations.

## 🚀 Basic Commands

```bash
# Run all tests
dotnet test

# Run with settings file
dotnet test --settings .runsettings

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Build and test
dotnet build && dotnet test
```

## 🎯 Test Filtering

```bash
# By category
dotnet test --filter "Category=Smoke"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category!=Slow"

# By test name
dotnet test --filter "FullyQualifiedName~AzureML"
dotnet test --filter "Name~Authentication"

# By priority
dotnet test --filter "Priority=1"

# Complex filters
dotnet test --filter "(Category=Smoke|Category=Integration)&Priority=1"
dotnet test --filter "Category=Smoke&Priority=1"
```

## 🌐 Browser Selection

```bash
# Chromium (default)
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=chromium

# Firefox
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=firefox

# WebKit (Safari)
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=webkit

# All browsers
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=all
```

## 👁️ Headless/Headed Mode

```bash
# Headed mode (visible browser)
dotnet test -- RunConfiguration.EnvironmentVariables.HEADLESS=false

# Headless mode (no UI)
dotnet test -- RunConfiguration.EnvironmentVariables.HEADLESS=true
```

## 🐛 Debugging

```bash
# Slow motion (500ms delay)
dotnet test -- RunConfiguration.EnvironmentVariables.SLOW_MO=500

# Single worker (no parallelization)
dotnet test -- RunConfiguration.EnvironmentVariables.WORKERS=1

# Enable traces
dotnet test -- RunConfiguration.EnvironmentVariables.TRACE=on

# Enable video recording
dotnet test -- RunConfiguration.EnvironmentVariables.VIDEO=on

# Enable screenshots
dotnet test -- RunConfiguration.EnvironmentVariables.SCREENSHOT=on

# Full debug mode
dotnet test -- RunConfiguration.EnvironmentVariables.HEADLESS=false RunConfiguration.EnvironmentVariables.SLOW_MO=500 RunConfiguration.EnvironmentVariables.WORKERS=1 RunConfiguration.EnvironmentVariables.TRACE=on
```

## ⚡ Parallel Execution

```bash
# Single worker (sequential)
dotnet test -- NUnit.NumberOfTestWorkers=1

# Multiple workers (parallel)
dotnet test -- NUnit.NumberOfTestWorkers=3
dotnet test -- NUnit.NumberOfTestWorkers=5

# Or via environment variable
dotnet test -- RunConfiguration.EnvironmentVariables.WORKERS=3
```

## 🔄 Retries

```bash
# No retries
dotnet test -- RunConfiguration.EnvironmentVariables.RETRIES=0

# Retry once
dotnet test -- RunConfiguration.EnvironmentVariables.RETRIES=1

# Retry twice
dotnet test -- RunConfiguration.EnvironmentVariables.RETRIES=2
```

## ⏱️ Timeouts

```bash
# 30 seconds (default)
dotnet test -- RunConfiguration.EnvironmentVariables.TIMEOUT=30000

# 1 minute
dotnet test -- RunConfiguration.EnvironmentVariables.TIMEOUT=60000

# 5 minutes
dotnet test -- RunConfiguration.EnvironmentVariables.TIMEOUT=300000
```

## 📊 Reporting

```bash
# Generate TRX report
dotnet test --logger trx

# Generate HTML report
dotnet test --logger html

# Generate Allure report (PowerShell)
.\generate-allure-report.ps1

# Generate Allure report (Bash)
./generate-allure-report.sh

# View Allure report
allure serve allure-results

# Generate and open Allure report manually
allure generate allure-results --clean -o allure-report
allure open allure-report
```

## 🏷️ Test Categories

```bash
# Smoke tests
dotnet test --filter "Category=Smoke"

# Integration tests
dotnet test --filter "Category=Integration"

# E2E tests
dotnet test --filter "Category=E2E"

# API tests
dotnet test --filter "Category=API"

# UI tests
dotnet test --filter "Category=UI"

# Azure tests
dotnet test --filter "Category=Azure"
```

## 🎭 Common Scenarios

### Local Development
```bash
dotnet test --settings .runsettings
```

### Debugging a Specific Test
```bash
dotnet test --filter "FullyQualifiedName~YourTestName" -- RunConfiguration.EnvironmentVariables.HEADLESS=false RunConfiguration.EnvironmentVariables.SLOW_MO=500 RunConfiguration.EnvironmentVariables.WORKERS=1
```

### Quick Smoke Test
```bash
dotnet test --filter "Category=Smoke" -- RunConfiguration.EnvironmentVariables.HEADLESS=true
```

### Cross-Browser Testing
```bash
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=all RunConfiguration.EnvironmentVariables.HEADLESS=false
```

### CI/CD Mode
```bash
dotnet test --settings .runsettings --logger trx -- RunConfiguration.EnvironmentVariables.HEADLESS=true RunConfiguration.EnvironmentVariables.WORKERS=1 RunConfiguration.EnvironmentVariables.RETRIES=2
```

### Performance Testing
```bash
dotnet test --filter "Category=Performance" -- RunConfiguration.EnvironmentVariables.HEADLESS=true RunConfiguration.EnvironmentVariables.WORKERS=5 RunConfiguration.EnvironmentVariables.TRACE=off RunConfiguration.EnvironmentVariables.VIDEO=off
```

## 🔧 Playwright Commands

```bash
# Install browsers
pwsh bin/Debug/net9.0/playwright.ps1 install

# Install with dependencies
pwsh bin/Debug/net9.0/playwright.ps1 install --with-deps

# Show trace
pwsh bin/Debug/net9.0/playwright.ps1 show-trace test-results/trace.zip

# Codegen (record actions)
pwsh bin/Debug/net9.0/playwright.ps1 codegen https://example.com
```

## 🏗️ Build Commands

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Clean build
dotnet clean && dotnet build

# Build in Release mode
dotnet build -c Release

# Rebuild
dotnet build --no-incremental
```

## 🧹 Cleanup Commands

```bash
# Clean build artifacts
dotnet clean

# Remove test results
rm -rf TestResults
rm -rf allure-results
rm -rf allure-report

# Remove bin and obj folders
rm -rf bin obj
```

## 📦 Package Management

```bash
# Restore packages
dotnet restore

# Update packages
dotnet list package --outdated
dotnet add package PackageName --version x.x.x

# Remove package
dotnet remove package PackageName
```

## 🔍 Test Discovery

```bash
# List all tests
dotnet test --list-tests

# List tests in specific category
dotnet test --list-tests --filter "Category=Smoke"

# Count tests
dotnet test --list-tests | wc -l
```

## 📝 Custom Parameters

```bash
# Pass custom parameters
dotnet test -- NUnit.TestParameters.baseUrl=https://example.com
dotnet test -- NUnit.TestParameters.apiKey=your-key
dotnet test -- NUnit.TestParameters.environment=staging
```

## 🌍 Environment Variables

```bash
# Set environment variables (PowerShell)
$env:HEADLESS="true"
$env:BROWSER="firefox"
dotnet test

# Set environment variables (Bash)
export HEADLESS=true
export BROWSER=firefox
dotnet test

# Or inline
HEADLESS=true BROWSER=firefox dotnet test
```

## 🔐 Azure Authentication

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "your-subscription-id"

# Show current account
az account show

# List subscriptions
az account list --output table
```

## 📊 Code Coverage

```bash
# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## 🎯 Useful Aliases

Add to your shell profile:

```bash
# Bash/Zsh
alias dt='dotnet test'
alias dts='dotnet test --settings .runsettings'
alias dtv='dotnet test --logger "console;verbosity=detailed"'
alias dtd='dotnet test -- RunConfiguration.EnvironmentVariables.HEADLESS=false RunConfiguration.EnvironmentVariables.SLOW_MO=500'
alias dth='dotnet test -- RunConfiguration.EnvironmentVariables.HEADLESS=true'
alias dtsmoke='dotnet test --filter "Category=Smoke"'
```

```powershell
# PowerShell
function dt { dotnet test }
function dts { dotnet test --settings .runsettings }
function dtv { dotnet test --logger "console;verbosity=detailed" }
function dtd { dotnet test -- RunConfiguration.EnvironmentVariables.HEADLESS=false RunConfiguration.EnvironmentVariables.SLOW_MO=500 }
function dth { dotnet test -- RunConfiguration.EnvironmentVariables.HEADLESS=true }
function dtsmoke { dotnet test --filter "Category=Smoke" }
```

## 📚 Help Commands

```bash
# dotnet test help
dotnet test --help

# NUnit help
dotnet test -- --help

# List available loggers
dotnet test --list-loggers

# Show test settings
dotnet test --settings .runsettings -- --help
```

## 🔗 Quick Links

- **Playwright .NET**: https://playwright.dev/dotnet/
- **NUnit**: https://docs.nunit.org/
- **Reqnroll**: https://docs.reqnroll.net/
- **Allure**: https://docs.qameta.io/allure/

---

**Tip**: Bookmark this page for quick access to common commands!