# C# Playwright Test Framework - Configuration Guide

Complete guide to configuring and customizing the C# Playwright test framework.

## Table of Contents

- [Overview](#overview)
- [Configuration Files](#configuration-files)
- [Environment Variables](#environment-variables)
- [Test Execution Settings](#test-execution-settings)
- [Browser Configuration](#browser-configuration)
- [Parallel Execution](#parallel-execution)
- [Reporting Configuration](#reporting-configuration)
- [BDD Configuration](#bdd-configuration)
- [Azure Configuration](#azure-configuration)
- [CI/CD Configuration](#cicd-configuration)
- [Advanced Scenarios](#advanced-scenarios)

## Overview

The framework uses multiple configuration layers:

1. **`.runsettings`** - Test execution configuration
2. **`reqnroll.json`** - BDD/Gherkin configuration
3. **`allureConfig.json`** - Allure reporting
4. **Environment Variables** - Runtime overrides
5. **`appsettings.json`** - Application settings

## Configuration Files

### .runsettings

The `.runsettings` file is the primary configuration for test execution.

**Location**: `/NewFramework/CSharpTests/.runsettings`

**Key Sections**:

```xml
<RunSettings>
  <RunConfiguration>
    <!-- Test execution settings -->
  </RunConfiguration>
  
  <NUnit>
    <!-- NUnit-specific settings -->
  </NUnit>
  
  <DataCollectionRunSettings>
    <!-- Code coverage and diagnostics -->
  </DataCollectionRunSettings>
  
  <LoggerRunSettings>
    <!-- Logging configuration -->
  </LoggerRunSettings>
</RunSettings>
```

**Usage**:

```bash
# Use default .runsettings
dotnet test --settings .runsettings

# Override specific values
dotnet test --settings .runsettings -- RunConfiguration.EnvironmentVariables.HEADLESS=true
```

### reqnroll.json

Configures BDD/Gherkin behavior.

**Location**: `/NewFramework/CSharpTests/reqnroll.json`

**Key Settings**:

```json
{
  "generator": {
    "allowRowTests": true,
    "generateAsyncTests": true,
    "markFeaturesParallelizable": true
  },
  "trace": {
    "traceSuccessfulSteps": true,
    "traceTimings": true,
    "coloredOutput": true
  }
}
```

### allureConfig.json

Configures Allure reporting.

**Location**: `/NewFramework/CSharpTests/allureConfig.json`

```json
{
  "allure": {
    "directory": "allure-results",
    "links": [
      "https://github.com/your-repo/issues/{issue}",
      "https://github.com/your-repo/issues/{tms}"
    ]
  }
}
```

## Environment Variables

### Playwright Configuration

#### HEADLESS

Controls browser visibility.

```bash
# Headed mode (visible browser)
HEADLESS=false

# Headless mode (no UI)
HEADLESS=true
```

**Use Cases**:
- `false` - Local development, debugging
- `true` - CI/CD, performance testing

#### BROWSER

Selects which browser(s) to use.

```bash
# Single browser
BROWSER=chromium  # Chrome, Edge
BROWSER=firefox   # Firefox
BROWSER=webkit    # Safari

# All browsers
BROWSER=all
```

**Examples**:

```bash
# Test on Firefox only
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=firefox

# Test on all browsers
dotnet test -- RunConfiguration.EnvironmentVariables.BROWSER=all
```

#### SLOW_MO

Adds delay between Playwright actions (milliseconds).

```bash
# No delay (default)
SLOW_MO=0

# 500ms delay (debugging)
SLOW_MO=500

# 1 second delay (demos)
SLOW_MO=1000
```

**Use Cases**:
- `0` - Normal test execution
- `100-500` - Debugging, watching test execution
- `1000+` - Demos, presentations

#### WORKERS

Number of parallel test workers.

```bash
# Single worker (sequential)
WORKERS=1

# Multiple workers (parallel)
WORKERS=3
WORKERS=5
```

**Recommendations**:
- **Local**: 3-5 workers (based on CPU cores)
- **CI**: 1-2 workers (resource constraints)
- **Debugging**: 1 worker (easier to follow)

#### RETRIES

Number of times to retry failed tests.

```bash
# No retries (default for local)
RETRIES=0

# Retry twice (CI/CD)
RETRIES=2

# Retry once
RETRIES=1
```

**Best Practices**:
- **Local**: 0 retries (fix flaky tests)
- **CI**: 1-2 retries (handle transient failures)

#### TIMEOUT

Test timeout in milliseconds.

```bash
# 30 seconds (default)
TIMEOUT=30000

# 1 minute
TIMEOUT=60000

# 5 minutes
TIMEOUT=300000
```

#### TRACE

Playwright trace recording.

```bash
# Only on failure (default)
TRACE=retain-on-failure

# Always record
TRACE=on

# Never record
TRACE=off
```

**Trace Files**: Saved to `test-results/` directory

**Viewing Traces**:
```bash
pwsh bin/Debug/net9.0/playwright.ps1 show-trace test-results/trace.zip
```

#### VIDEO

Video recording of test execution.

```bash
# No video (default)
VIDEO=off

# Record on failure
VIDEO=retain-on-failure

# Always record
VIDEO=on
```

**Video Files**: Saved to `test-results/` directory

#### SCREENSHOT

Screenshot capture strategy.

```bash
# Only on failure (default)
SCREENSHOT=only-on-failure

# Always capture
SCREENSHOT=on

# Never capture
SCREENSHOT=off
```

### Azure Configuration

#### Azure ML Workspace

```bash
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_WORKSPACE_NAME=your-workspace-name
AZURE_REGION=eastus
```

#### Azure AI Search

```bash
AZURE_SEARCH_ENDPOINT=https://your-search.search.windows.net
AZURE_SEARCH_API_KEY=your-api-key
AZURE_SEARCH_INDEX_NAME=your-index
```

#### Azure Document Intelligence

```bash
AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT=https://your-doc-intel.cognitiveservices.azure.com/
AZURE_DOCUMENT_INTELLIGENCE_KEY=your-key
```

#### Azure Speech Services

```bash
AZURE_SPEECH_KEY=your-speech-key
AZURE_SPEECH_REGION=eastus
```

### Reporting Configuration

#### ALLURE_RESULTS_DIR

Directory for Allure results.

```bash
# Default
ALLURE_RESULTS_DIR=allure-results

# Custom location
ALLURE_RESULTS_DIR=test-output/allure
```

#### TEST_RESULTS_DIR

Directory for test results.

```bash
# Default
TEST_RESULTS_DIR=TestResults

# Custom location
TEST_RESULTS_DIR=output/test-results
```

### Test Environment

#### TEST_ENV

Current test environment.

```bash
TEST_ENV=local       # Local development
TEST_ENV=dev         # Development environment
TEST_ENV=staging     # Staging environment
TEST_ENV=production  # Production environment
```

#### LOG_LEVEL

Logging verbosity.

```bash
LOG_LEVEL=Verbose      # Most detailed
LOG_LEVEL=Debug        # Debug information
LOG_LEVEL=Information  # Default
LOG_LEVEL=Warning      # Warnings only
LOG_LEVEL=Error        # Errors only
LOG_LEVEL=Fatal        # Fatal errors only
```

#### CI

Indicates CI/CD environment.

```bash
# Local development
CI=false

# CI/CD pipeline
CI=true
```

**Auto-adjustments when CI=true**:
- Workers: 1 (instead of 3)
- Retries: 2 (instead of 0)
- Headless: true (forced)

## Test Execution Settings

### NUnit Configuration

#### Number of Workers

```xml
<NUnit>
  <NumberOfTestWorkers>3</NumberOfTestWorkers>
</NUnit>
```

Or via command line:

```bash
dotnet test -- NUnit.NumberOfTestWorkers=5
```

#### Default Timeout

```xml
<NUnit>
  <DefaultTimeout>30000</DefaultTimeout>
</NUnit>
```

#### Test Filtering

```xml
<NUnit>
  <Include>Category=Smoke</Include>
  <Exclude>Category=Slow</Exclude>
</NUnit>
```

Or via command line:

```bash
# Include specific category
dotnet test --filter "Category=Smoke"

# Exclude category
dotnet test --filter "Category!=Slow"

# Complex filter
dotnet test --filter "(Category=Smoke|Category=Integration)&Priority=1"
```

#### Verbosity

```xml
<NUnit>
  <Verbosity>Normal</Verbosity>
</NUnit>
```

Options: `Quiet`, `Minimal`, `Normal`, `Detailed`

### Test Parameters

Pass custom parameters to tests:

```bash
dotnet test -- NUnit.TestParameters.baseUrl=https://example.com
dotnet test -- NUnit.TestParameters.apiKey=your-key
```

Access in tests:

```csharp
var baseUrl = TestContext.Parameters["baseUrl"];
var apiKey = TestContext.Parameters["apiKey"];
```

## Browser Configuration

### Playwright Browser Options

Configure in test code:

```csharp
var options = new BrowserTypeLaunchOptions
{
    Headless = bool.Parse(Environment.GetEnvironmentVariable("HEADLESS") ?? "false"),
    SlowMo = int.Parse(Environment.GetEnvironmentVariable("SLOW_MO") ?? "0"),
    Timeout = int.Parse(Environment.GetEnvironmentVariable("TIMEOUT") ?? "30000"),
    Args = new[] { "--start-maximized" }
};

var browser = await playwright.Chromium.LaunchAsync(options);
```

### Browser Context Options

```csharp
var contextOptions = new BrowserNewContextOptions
{
    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
    ScreenSize = new ScreenSize { Width = 1920, Height = 1080 },
    RecordVideoDir = "test-results/videos",
    RecordVideoSize = new RecordVideoSize { Width = 1920, Height = 1080 },
    IgnoreHTTPSErrors = true,
    AcceptDownloads = true
};

var context = await browser.NewContextAsync(contextOptions);
```

## Parallel Execution

### Feature-Level Parallelization

Reqnroll features are parallelizable by default:

```json
{
  "generator": {
    "markFeaturesParallelizable": true
  }
}
```

### Disable Parallelization for Specific Tests

Use tags in feature files:

```gherkin
@serial
@no-parallel
Feature: Sequential Test
  This feature must run sequentially
```

Or in C# tests:

```csharp
[Test, NonParallelizable]
public void SequentialTest()
{
    // This test runs alone
}
```

### Control Parallelization

```bash
# Sequential execution
dotnet test -- NUnit.NumberOfTestWorkers=1

# Parallel execution
dotnet test -- NUnit.NumberOfTestWorkers=5
```

## Reporting Configuration

### Console Logger

```xml
<Logger friendlyName="console" enabled="True">
  <Configuration>
    <Verbosity>normal</Verbosity>
  </Configuration>
</Logger>
```

### TRX Logger

```xml
<Logger friendlyName="trx" enabled="True">
  <Configuration>
    <LogFileName>test-results.trx</LogFileName>
  </Configuration>
</Logger>
```

### HTML Logger

```xml
<Logger friendlyName="html" enabled="True">
  <Configuration>
    <LogFileName>test-results.html</LogFileName>
  </Configuration>
</Logger>
```

### Allure Configuration

Generate Allure reports:

```bash
# PowerShell
.\generate-allure-report.ps1

# Bash
./generate-allure-report.sh

# Manual
allure generate allure-results --clean -o allure-report
allure open allure-report
```

## BDD Configuration

### Reqnroll Settings

#### Async Tests

```json
{
  "generator": {
    "generateAsyncTests": true
  }
}
```

#### Row Tests

```json
{
  "generator": {
    "allowRowTests": true
  }
}
```

#### Trace Settings

```json
{
  "trace": {
    "traceSuccessfulSteps": true,
    "traceTimings": true,
    "minTracedDuration": "0:0:0.1",
    "coloredOutput": true
  }
}
```

## CI/CD Configuration

### Azure DevOps

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Tests'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: '--settings .runsettings --logger trx --logger "console;verbosity=detailed"'
  env:
    HEADLESS: 'true'
    BROWSER: 'chromium'
    WORKERS: '1'
    RETRIES: '2'
    CI: 'true'
```

### GitHub Actions

```yaml
- name: Run Tests
  run: dotnet test --settings .runsettings --logger trx
  env:
    HEADLESS: 'true'
    BROWSER: 'chromium'
    WORKERS: '1'
    RETRIES: '2'
    CI: 'true'
```

### GitLab CI

```yaml
test:
  script:
    - dotnet test --settings .runsettings --logger trx
  variables:
    HEADLESS: 'true'
    BROWSER: 'chromium'
    WORKERS: '1'
    RETRIES: '2'
    CI: 'true'
```

## Advanced Scenarios

### Scenario 1: Local Development

```bash
# .env or environment
HEADLESS=false
BROWSER=chromium
SLOW_MO=0
WORKERS=3
RETRIES=0
TRACE=retain-on-failure
VIDEO=off
SCREENSHOT=only-on-failure
LOG_LEVEL=Information
```

### Scenario 2: Debugging

```bash
HEADLESS=false
BROWSER=chromium
SLOW_MO=500
WORKERS=1
RETRIES=0
TRACE=on
VIDEO=on
SCREENSHOT=on
LOG_LEVEL=Debug
```

### Scenario 3: CI/CD Pipeline

```bash
HEADLESS=true
BROWSER=all
SLOW_MO=0
WORKERS=1
RETRIES=2
TRACE=retain-on-failure
VIDEO=retain-on-failure
SCREENSHOT=only-on-failure
CI=true
LOG_LEVEL=Information
```

### Scenario 4: Cross-Browser Testing

```bash
HEADLESS=false
BROWSER=all
SLOW_MO=0
WORKERS=3
RETRIES=0
TRACE=retain-on-failure
VIDEO=off
SCREENSHOT=only-on-failure
```

### Scenario 5: Performance Testing

```bash
HEADLESS=true
BROWSER=chromium
SLOW_MO=0
WORKERS=5
RETRIES=0
TRACE=off
VIDEO=off
SCREENSHOT=off
TIMEOUT=60000
```

### Scenario 6: Smoke Testing

```bash
HEADLESS=true
BROWSER=chromium
WORKERS=3
RETRIES=1
TRACE=off
VIDEO=off
SCREENSHOT=only-on-failure

# Run command
dotnet test --filter "Category=Smoke"
```

## Best Practices

### 1. Environment-Specific Configuration

Use different configurations for different environments:

```bash
# Local
dotnet test --settings .runsettings.local

# CI
dotnet test --settings .runsettings.ci

# Production
dotnet test --settings .runsettings.prod
```

### 2. Sensitive Data

Never commit sensitive data. Use:
- Environment variables
- Azure Key Vault
- Secret management tools

### 3. Test Isolation

Ensure tests are independent:
- Don't share state between tests
- Clean up resources after tests
- Use unique test data

### 4. Parallel Execution

- Use parallelization for faster execution
- Mark UI tests as non-parallelizable if needed
- Be careful with shared resources

### 5. Reporting

- Always generate reports in CI/CD
- Keep historical reports for trend analysis
- Use Allure for rich reporting

## Troubleshooting

### Issue: Tests timing out

**Solution**: Increase timeout

```bash
dotnet test -- RunConfiguration.EnvironmentVariables.TIMEOUT=60000
```

### Issue: Parallel execution failures

**Solution**: Reduce workers or disable parallelization

```bash
dotnet test -- RunConfiguration.EnvironmentVariables.WORKERS=1
```

### Issue: Browser not found

**Solution**: Install Playwright browsers

```bash
pwsh bin/Debug/net9.0/playwright.ps1 install
```

### Issue: Azure authentication failures

**Solution**: Login with Azure CLI

```bash
az login
az account set --subscription "your-subscription-id"
```

## Additional Resources

- [.NET Test Documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [NUnit Documentation](https://docs.nunit.org/)
- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [Allure Documentation](https://docs.qameta.io/allure/)