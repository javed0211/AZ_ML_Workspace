# Azure ML Workspace Testing Framework - Comprehensive README

## 🚀 Overview

The Azure ML Workspace Testing Framework is a state-of-the-art C# test automation solution that combines the power of the Screenplay pattern, Behavior-Driven Development (BDD), and modern browser automation to provide comprehensive testing capabilities for Azure Machine Learning workspaces and AI services.

## ✨ Key Features

### 🎭 Screenplay Pattern Implementation
- **Actor-based testing**: Natural, readable test code that mirrors real user interactions
- **Ability system**: Modular capabilities that actors can use (Azure ML, AI Search, Web browsing)
- **Task-oriented actions**: High-level business operations that are reusable and maintainable
- **Question-based assertions**: Intelligent queries about system state

### 🥒 Behavior-Driven Development (BDD)
- **Gherkin syntax**: Write tests in natural language using Given-When-Then
- **Reqnroll integration**: Modern BDD framework with excellent .NET support
- **Living documentation**: Tests serve as executable specifications
- **Stakeholder collaboration**: Business-readable test scenarios

### 🌐 Modern Browser Automation
- **Playwright integration**: Fast, reliable, and cross-browser testing
- **Multi-browser support**: Chromium, Firefox, and WebKit (Safari)
- **Advanced debugging**: Screenshots, videos, and execution traces
- **Mobile testing**: Responsive design and mobile browser testing

### ☁️ Azure Services Integration
- **Azure ML Workspace**: Complete workspace management and testing
- **Azure AI Search**: Document processing and search functionality
- **Azure Storage**: Blob storage and data management
- **Role-based testing**: Test with different Azure RBAC roles
- **PIM support**: Privileged Identity Management integration

### 🔧 Enterprise-Ready Features
- **Comprehensive logging**: Structured logging with Serilog
- **Configuration management**: Hierarchical configuration with environment support
- **Parallel execution**: Thread-safe, scalable test execution
- **CI/CD integration**: Azure DevOps and GitHub Actions support
- **Rich reporting**: ExtentReports and Allure integration

## 📋 Table of Contents

1. [Quick Start](#quick-start)
2. [Installation](#installation)
3. [Configuration](#configuration)
4. [Writing Tests](#writing-tests)
5. [Running Tests](#running-tests)
6. [Architecture](#architecture)
7. [API Reference](#api-reference)
8. [Examples](#examples)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)
11. [Contributing](#contributing)
12. [License](#license)

## 🚀 Quick Start

### Prerequisites

- **.NET 8.0 SDK** or later
- **Visual Studio 2022** or **VS Code** with C# extension
- **Azure subscription** with ML workspace access
- **Git** for version control

### 5-Minute Setup

1. **Clone and setup:**
   ```bash
   git clone <repository-url>
   cd AZ_ML_Workspace
   dotnet restore
   pwsh bin/Debug/net8.0/playwright.ps1 install
   ```

2. **Configure Azure settings:**
   ```json
   // appsettings.test.json
   {
     "Azure": {
       "SubscriptionId": "your-subscription-id",
       "ResourceGroup": "your-resource-group",
       "WorkspaceName": "your-workspace-name"
     }
   }
   ```

3. **Run your first test:**
   ```bash
   dotnet test --filter "Category=Smoke"
   ```

### Your First BDD Test

Create a simple feature file:

```gherkin
Feature: My First Azure ML Test
    As a data scientist
    I want to access my ML workspace
    So that I can start my ML projects

Scenario: Access workspace successfully
    Given I am a data scientist named "Alice"
    And I have Contributor access to Azure ML
    When I attempt to open workspace "my-workspace"
    Then I should be able to access the workspace
```

## 📦 Installation

### System Requirements

| Component | Minimum Version | Recommended |
|-----------|----------------|-------------|
| .NET SDK | 8.0 | Latest LTS |
| Visual Studio | 2022 17.0 | Latest |
| Memory | 8 GB RAM | 16 GB RAM |
| Storage | 2 GB free | 5 GB free |

### Step-by-Step Installation

1. **Install .NET SDK:**
   ```bash
   # Windows (using winget)
   winget install Microsoft.DotNet.SDK.8
   
   # macOS (using Homebrew)
   brew install dotnet
   
   # Linux (Ubuntu/Debian)
   sudo apt-get update
   sudo apt-get install -y dotnet-sdk-8.0
   ```

2. **Clone the repository:**
   ```bash
   git clone https://github.com/your-org/AZ_ML_Workspace.git
   cd AZ_ML_Workspace
   ```

3. **Restore packages:**
   ```bash
   dotnet restore
   ```

4. **Install Playwright browsers:**
   ```bash
   # Windows PowerShell
   pwsh bin/Debug/net8.0/playwright.ps1 install --with-deps
   
   # Linux/macOS
   ./bin/Debug/net8.0/playwright.sh install --with-deps
   ```

5. **Verify installation:**
   ```bash
   dotnet build
   dotnet test --filter "Category=Installation"
   ```

### IDE Setup

#### Visual Studio 2022
1. Install the **Reqnroll for Visual Studio** extension
2. Install the **NUnit 3 Test Adapter** extension
3. Configure test discovery in Test Explorer

#### VS Code
1. Install the **C#** extension
2. Install the **.NET Core Test Explorer** extension
3. Install the **Cucumber (Gherkin) Full Support** extension

## ⚙️ Configuration

### Configuration Hierarchy

The framework uses a hierarchical configuration system:

```
Environment Variables (Highest Priority)
    ↓
appsettings.test.json (Test Overrides)
    ↓
appsettings.json (Base Configuration)
```

### Core Configuration Sections

#### Azure Configuration
```json
{
  "Azure": {
    "SubscriptionId": "12345678-1234-1234-1234-123456789012",
    "ResourceGroup": "ml-resources-rg",
    "WorkspaceName": "my-ml-workspace",
    "TenantId": "87654321-4321-4321-4321-210987654321",
    "Environment": "AzureCloud",
    "Region": "East US"
  }
}
```

#### Playwright Configuration
```json
{
  "Playwright": {
    "HeadlessMode": false,
    "BrowserType": "chromium",
    "SlowMo": true,
    "SlowMoDelay": 500,
    "DefaultTimeout": 30000,
    "CaptureScreenshots": true,
    "CaptureVideos": true,
    "CaptureTraces": true,
    "ViewportSize": {
      "Width": 1920,
      "Height": 1080
    }
  }
}
```

#### Logging Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "AzureMLWorkspace.Tests": "Debug"
    },
    "Console": {
      "IncludeScopes": true,
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
    },
    "File": {
      "Path": "Logs/test-{Date}.log",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 7
    }
  }
}
```

### Environment-Specific Configuration

#### Development Environment
```json
// appsettings.Development.json
{
  "Playwright": {
    "HeadlessMode": false,
    "SlowMo": true,
    "CaptureVideos": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

#### CI/CD Environment
```json
// appsettings.Production.json
{
  "Playwright": {
    "HeadlessMode": true,
    "SlowMo": false,
    "CaptureVideos": false,
    "CaptureScreenshots": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Environment Variables

Override any configuration using environment variables:

```bash
# Azure Configuration
export Azure__SubscriptionId="your-subscription-id"
export Azure__ResourceGroup="your-resource-group"
export Azure__WorkspaceName="your-workspace-name"

# Playwright Configuration
export Playwright__HeadlessMode="false"
export Playwright__BrowserType="firefox"

# Logging Configuration
export Logging__LogLevel__Default="Debug"
```

## ✍️ Writing Tests

### BDD Feature Files

Feature files use Gherkin syntax to describe test scenarios:

```gherkin
Feature: Compute Instance Management
    As a data scientist
    I want to manage compute instances
    So that I can run my ML experiments efficiently

Background:
    Given I am a data scientist named "DataScientist"
    And I have Contributor access to Azure ML
    And I have opened workspace "test-workspace"

Scenario: Start and monitor compute instance
    When I start compute instance "test-compute"
    Then the compute instance should be running
    And I should be able to connect to it
    And the compute status should be "Succeeded"

Scenario: Handle multiple compute instances
    When I start compute instances:
        | ComputeName    | Size           | Priority |
        | cpu-compute-1  | Standard_DS2_v2| High     |
        | gpu-compute-1  | Standard_NC6   | Medium   |
        | cpu-compute-2  | Standard_DS3_v2| Low      |
    Then all compute instances should be running
    When I stop all compute instances
    Then all compute instances should be stopped

Scenario Outline: Test different user roles
    Given I am a user named "<UserName>"
    And I have <Role> access to Azure ML
    When I attempt to open workspace "secure-workspace"
    Then I should <ExpectedResult>

Examples:
    | UserName | Role        | ExpectedResult                    |
    | Admin    | Owner       | be able to access the workspace   |
    | Dev      | Contributor | be able to access the workspace   |
    | Viewer   | Reader      | be able to access the workspace   |
```

### Step Definitions

Step definitions connect Gherkin steps to C# code:

```csharp
[Binding]
public class ComputeManagementSteps
{
    private readonly ILogger<ComputeManagementSteps> _logger;
    private IActor? _actor;
    private readonly List<string> _computeInstances = new();

    public ComputeManagementSteps(ILogger<ComputeManagementSteps> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Given(@"I am a data scientist named ""(.*)""")]
    public void GivenIAmADataScientistNamed(string name)
    {
        _logger.LogInformation("Creating actor: {ActorName}", name);
        _actor = Actor.Named(name, GetService<ILogger<Actor>>());
    }

    [Given(@"I have (.*) access to Azure ML")]
    public async Task GivenIHaveAccessToAzureML(string role)
    {
        _logger.LogInformation("Granting {Role} access to Azure ML", role);
        
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        var azureMLAbility = UseAzureML.WithRole(role);
        _actor.Can(azureMLAbility);
        await azureMLAbility.InitializeAsync();
    }

    [When(@"I start compute instance ""(.*)""")]
    public async Task WhenIStartComputeInstance(string computeName)
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        await _actor.AttemptsTo(StartCompute.Named(computeName));
        _computeInstances.Add(computeName);
    }

    [Then(@"the compute instance should be running")]
    public async Task ThenTheComputeInstanceShouldBeRunning()
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        var computeName = _computeInstances.LastOrDefault();
        if (computeName != null)
        {
            await _actor.Should(Validate.ComputeStatus(computeName, "Succeeded"));
        }
    }
}
```

### Custom Tasks

Create reusable business actions:

```csharp
public class CreateNotebook : ITask
{
    private readonly string _notebookName;
    private readonly string _kernelType;

    public string Name => $"Create notebook '{_notebookName}' with {_kernelType} kernel";

    private CreateNotebook(string notebookName, string kernelType)
    {
        _notebookName = notebookName;
        _kernelType = kernelType;
    }

    public static CreateNotebook Named(string notebookName)
    {
        return new CreateNotebook(notebookName, "Python 3.8");
    }

    public static CreateNotebook WithKernel(string notebookName, string kernelType)
    {
        return new CreateNotebook(notebookName, kernelType);
    }

    public async Task PerformAs(IActor actor)
    {
        var browser = actor.Using<BrowseTheWeb>();
        var page = await browser.NewPageAsync();
        
        // Navigate to notebooks section
        await page.ClickAsync("[data-testid='notebooks-tab']");
        
        // Create new notebook
        await page.ClickAsync("[data-testid='create-notebook-btn']");
        await page.FillAsync("[data-testid='notebook-name']", _notebookName);
        await page.SelectOptionAsync("[data-testid='kernel-select']", _kernelType);
        await page.ClickAsync("[data-testid='create-btn']");
        
        // Wait for creation
        await page.WaitForSelectorAsync($"[data-testid='notebook-{_notebookName}']");
        
        actor.Remember("CreatedNotebook", _notebookName);
    }
}
```

### Custom Questions

Create intelligent system queries:

```csharp
public class NotebookExists : IQuestion<bool>
{
    private readonly string _notebookName;

    public string Question => $"Does notebook '{_notebookName}' exist?";

    private NotebookExists(string notebookName)
    {
        _notebookName = notebookName;
    }

    public static NotebookExists Named(string notebookName)
    {
        return new NotebookExists(notebookName);
    }

    public async Task<bool> AnsweredBy(IActor actor)
    {
        var browser = actor.Using<BrowseTheWeb>();
        var page = await browser.CurrentPageAsync();
        
        try
        {
            await page.WaitForSelectorAsync($"[data-testid='notebook-{_notebookName}']", 
                new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
```

## 🏃‍♂️ Running Tests

### Command Line Execution

#### Basic Test Execution
```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run with detailed logging
dotnet test --logger "console;verbosity=detailed"
```

#### Category-Based Execution
```bash
# Run BDD scenarios only
dotnet test --filter "Category=BDD"

# Run unit tests only
dotnet test --filter "Category=Unit"

# Run integration tests
dotnet test --filter "Category=Integration"

# Run smoke tests
dotnet test --filter "Category=Smoke"

# Run performance tests
dotnet test --filter "Category=Performance"
```

#### Feature-Specific Execution
```bash
# Run specific feature
dotnet test --filter "FullyQualifiedName~AzureMLWorkspace"

# Run specific scenario
dotnet test --filter "DisplayName~'Start Compute Instance'"

# Run tests matching pattern
dotnet test --filter "TestCategory=ComputeManagement"
```

#### Browser-Specific Testing
```bash
# Run with Chromium (default)
dotnet test -- Playwright.BrowserName=chromium

# Run with Firefox
dotnet test -- Playwright.BrowserName=firefox

# Run with WebKit (Safari)
dotnet test -- Playwright.BrowserName=webkit

# Run with all browsers
dotnet test -- Playwright.BrowserName=chromium,firefox,webkit
```

#### Debug and Development Mode
```bash
# Run in headed mode (visible browser)
dotnet test -- Playwright.HeadlessMode=false

# Run with slow motion
dotnet test -- Playwright.SlowMo=true Playwright.SlowMoDelay=1000

# Capture videos and screenshots
dotnet test -- Playwright.CaptureVideos=true Playwright.CaptureScreenshots=true

# Enable trace recording
dotnet test -- Playwright.CaptureTraces=true
```

### Using Test Scripts

#### Windows PowerShell Script
```powershell
# run-tests.ps1
param(
    [string]$Browser = "chromium",
    [string]$Category = "",
    [switch]$Debug = $false,
    [switch]$Parallel = $true
)

$env:Playwright__BrowserName = $Browser

if ($Debug) {
    $env:Playwright__HeadlessMode = "false"
    $env:Playwright__SlowMo = "true"
    $env:Playwright__CaptureVideos = "true"
}

$filterArgs = ""
if ($Category) {
    $filterArgs = "--filter Category=$Category"
}

$parallelArgs = ""
if (-not $Parallel) {
    $parallelArgs = "--parallel 1"
}

Write-Host "Running tests with browser: $Browser" -ForegroundColor Green

dotnet test $filterArgs $parallelArgs --logger "console;verbosity=normal"
```

#### Linux/macOS Bash Script
```bash
#!/bin/bash
# run-tests.sh

BROWSER="chromium"
CATEGORY=""
DEBUG=false
PARALLEL=true

while [[ $# -gt 0 ]]; do
    case $1 in
        --browser)
            BROWSER="$2"
            shift 2
            ;;
        --category)
            CATEGORY="$2"
            shift 2
            ;;
        --debug)
            DEBUG=true
            shift
            ;;
        --no-parallel)
            PARALLEL=false
            shift
            ;;
        *)
            echo "Unknown option $1"
            exit 1
            ;;
    esac
done

export Playwright__BrowserName=$BROWSER

if [ "$DEBUG" = true ]; then
    export Playwright__HeadlessMode=false
    export Playwright__SlowMo=true
    export Playwright__CaptureVideos=true
fi

FILTER_ARGS=""
if [ -n "$CATEGORY" ]; then
    FILTER_ARGS="--filter Category=$CATEGORY"
fi

PARALLEL_ARGS=""
if [ "$PARALLEL" = false ]; then
    PARALLEL_ARGS="--parallel 1"
fi

echo "Running tests with browser: $BROWSER"

dotnet test $FILTER_ARGS $PARALLEL_ARGS --logger "console;verbosity=normal"
```

### IDE Integration

#### Visual Studio 2022
1. **Test Explorer**: View → Test Explorer
2. **Run/Debug**: Right-click tests to run or debug
3. **Live Unit Testing**: Enable for real-time feedback
4. **Code Coverage**: Analyze → Code Coverage

#### VS Code
1. **Test Explorer**: Install .NET Core Test Explorer extension
2. **Command Palette**: Ctrl+Shift+P → ".NET: Run Tests"
3. **Debug Tests**: Set breakpoints and debug individual tests
4. **Terminal Integration**: Use integrated terminal for command-line execution

### Continuous Integration

#### Azure DevOps Pipeline
```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
    - main
    - develop
  paths:
    exclude:
    - docs/*
    - README.md

variables:
  buildConfiguration: 'Release'
  vmImage: 'ubuntu-latest'

stages:
- stage: Build
  displayName: 'Build and Test'
  jobs:
  - job: BuildAndTest
    displayName: 'Build and Test Job'
    pool:
      vmImage: $(vmImage)
    
    steps:
    - task: UseDotNet@2
      displayName: 'Use .NET 8.0'
      inputs:
        version: '8.0.x'
        includePreviewVersions: false

    - script: dotnet restore
      displayName: 'Restore NuGet packages'

    - script: dotnet build --configuration $(buildConfiguration) --no-restore
      displayName: 'Build solution'

    - script: |
        pwsh bin/$(buildConfiguration)/net8.0/playwright.ps1 install --with-deps
      displayName: 'Install Playwright browsers'

    - script: |
        dotnet test --configuration $(buildConfiguration) --no-build \
          --logger trx --collect:"XPlat Code Coverage" \
          --results-directory $(Agent.TempDirectory)/TestResults
      displayName: 'Run tests'
      env:
        Azure__SubscriptionId: $(AZURE_SUBSCRIPTION_ID)
        Azure__ResourceGroup: $(AZURE_RESOURCE_GROUP)
        Azure__WorkspaceName: $(AZURE_WORKSPACE_NAME)
        Azure__TenantId: $(AZURE_TENANT_ID)

    - task: PublishTestResults@2
      displayName: 'Publish test results'
      inputs:
        testResultsFormat: 'VSTest'
        testResultsFiles: '**/*.trx'
        searchFolder: '$(Agent.TempDirectory)/TestResults'
        mergeTestResults: true
        failTaskOnFailedTests: true

    - task: PublishCodeCoverageResults@1
      displayName: 'Publish code coverage'
      inputs:
        codeCoverageTool: 'Cobertura'
        summaryFileLocation: '$(Agent.TempDirectory)/TestResults/**/coverage.cobertura.xml'

    - task: PublishBuildArtifacts@1
      displayName: 'Publish test artifacts'
      condition: always()
      inputs:
        pathToPublish: '$(Build.SourcesDirectory)/test-results'
        artifactName: 'test-results'
```

#### GitHub Actions Workflow
```yaml
# .github/workflows/test.yml
name: Test Automation

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

env:
  DOTNET_VERSION: '8.0.x'

jobs:
  test:
    runs-on: ubuntu-latest
    
    strategy:
      matrix:
        browser: [chromium, firefox, webkit]
        
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Cache dependencies
      uses: actions/cache@v3
      with:
        path: ~/.nuget/packages
        key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
        restore-keys: |
          ${{ runner.os }}-nuget-

    - name: Restore dependencies
      run: dotnet restore

    - name: Build solution
      run: dotnet build --configuration Release --no-restore

    - name: Install Playwright browsers
      run: pwsh bin/Release/net8.0/playwright.ps1 install --with-deps

    - name: Run tests
      run: |
        dotnet test --configuration Release --no-build \
          --logger "trx;LogFileName=test-results-${{ matrix.browser }}.trx" \
          --collect:"XPlat Code Coverage" \
          -- Playwright.BrowserName=${{ matrix.browser }}
      env:
        Azure__SubscriptionId: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
        Azure__ResourceGroup: ${{ secrets.AZURE_RESOURCE_GROUP }}
        Azure__WorkspaceName: ${{ secrets.AZURE_WORKSPACE_NAME }}
        Azure__TenantId: ${{ secrets.AZURE_TENANT_ID }}

    - name: Upload test results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: test-results-${{ matrix.browser }}
        path: |
          TestResults/
          test-results/
          **/*.trx

    - name: Upload screenshots
      uses: actions/upload-artifact@v3
      if: failure()
      with:
        name: screenshots-${{ matrix.browser }}
        path: test-results/screenshots/
```

## 🏗️ Architecture

### High-Level Architecture

The framework follows a layered architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   BDD Features  │  │  Step Definitions│  │  Test Runner│ │
│  │   (Gherkin)     │  │   (Reqnroll)    │  │   (NUnit)   │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                   Business Logic Layer                     │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │     Actors      │  │      Tasks      │  │  Questions  │ │
│  │  (Screenplay)   │  │   (Actions)     │  │ (Assertions)│ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                   Service Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   Abilities     │  │   Utilities     │  │Configuration│ │
│  │ (Capabilities)  │  │   (Helpers)     │  │ Management  │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   Azure SDK     │  │   Playwright    │  │   Logging   │ │
│  │  Integration    │  │   Browser       │  │  Framework  │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Core Design Patterns

1. **Screenplay Pattern**: Actor-based testing with abilities, tasks, and questions
2. **Factory Pattern**: Creation of abilities, tasks, and questions
3. **Builder Pattern**: Complex configuration and test data creation
4. **Strategy Pattern**: Different authentication and execution strategies
5. **Observer Pattern**: Test execution monitoring and reporting
6. **Command Pattern**: Encapsulating actions as tasks

### Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Testing Framework | NUnit | 4.2.2 |
| BDD Framework | Reqnroll | 2.1.0 |
| Browser Automation | Playwright | 1.49.0 |
| .NET Runtime | .NET | 8.0 |
| Dependency Injection | Microsoft.Extensions.DI | 9.0.0 |
| Configuration | Microsoft.Extensions.Configuration | 9.0.3 |
| Logging | Serilog | 4.2.0 |
| Azure Integration | Azure SDK for .NET | Latest |
| Assertions | FluentAssertions | 6.12.2 |
| Test Data | Bogus, AutoFixture | Latest |

## 📚 API Reference

### Core Interfaces

#### IActor Interface
```csharp
public interface IActor : IAsyncDisposable
{
    string Name { get; }
    
    // Ability Management
    IActor Can<T>(T ability) where T : IAbility;
    T Using<T>() where T : IAbility;
    bool HasAbility<T>() where T : IAbility;
    
    // Task Execution
    Task<IActor> AttemptsTo(ITask task);
    Task<IActor> AttemptsTo(params ITask[] tasks);
    Task<IActor> And(ITask task);
    
    // Questions and Assertions
    Task<T> AsksFor<T>(IQuestion<T> question);
    Task<IActor> Should(IQuestion<bool> question);
    Task<IActor> ShouldSee<T>(IQuestion<T> question, Action<T> assertion);
    
    // Memory Management
    void Remember<T>(string key, T value);
    T Recall<T>(string key);
    bool Remembers(string key);
}
```

#### IAbility Interface
```csharp
public interface IAbility
{
    string Name { get; }
    Task InitializeAsync();
    Task CleanupAsync();
}
```

#### ITask Interface
```csharp
public interface ITask
{
    string Name { get; }
    Task PerformAs(IActor actor);
}
```

#### IQuestion Interface
```csharp
public interface IQuestion<T>
{
    string Question { get; }
    Task<T> AnsweredBy(IActor actor);
}
```

### Key Classes

#### Actor Class
```csharp
public class Actor : IActor
{
    public string Name { get; }
    
    public static Actor Named(string name, ILogger<Actor> logger);
    public IActor Can<T>(T ability) where T : IAbility;
    public T Using<T>() where T : IAbility;
    public async Task<IActor> AttemptsTo(ITask task);
    public async Task<T> AsksFor<T>(IQuestion<T> question);
    public async Task<IActor> Should(IQuestion<bool> question);
    public void Remember<T>(string key, T value);
    public T Recall<T>(string key);
}
```

#### UseAzureML Ability
```csharp
public class UseAzureML : IAbility
{
    public string Name { get; }
    public ArmClient ArmClient { get; }
    public MachineLearningWorkspaceResource Workspace { get; }
    
    public static UseAzureML WithRole(string role);
    public static UseAzureML AsContributor();
    public static UseAzureML AsReader();
    public static UseAzureML AsOwner();
    
    public async Task StartCompute(string computeName);
    public async Task StopCompute(string computeName);
    public async Task<string> GetComputeStatus(string computeName);
}
```

## 💡 Examples

### Example 1: Basic Workspace Access Test

```gherkin
Feature: Basic Workspace Access
    As a data scientist
    I want to access my Azure ML workspace
    So that I can start working on ML projects

Scenario: Successful workspace access
    Given I am a data scientist named "Alice"
    And I have Contributor access to Azure ML
    When I attempt to open workspace "my-workspace"
    Then I should be able to access the workspace
    And the workspace should be available
```

### Example 2: Complex Compute Management

```gherkin
Feature: Advanced Compute Management
    As an ML engineer
    I want to manage multiple compute instances
    So that I can optimize resource usage

Background:
    Given I am an ML engineer named "Bob"
    And I have Owner access to Azure ML
    And I have opened workspace "production-workspace"

Scenario: Orchestrate compute resources for training
    When I start compute instances:
        | ComputeName     | Size            | Priority | AutoShutdown |
        | training-gpu-1  | Standard_NC12   | High     | 60 minutes   |
        | training-gpu-2  | Standard_NC12   | High     | 60 minutes   |
        | preprocessing   | Standard_DS4_v2 | Medium   | 30 minutes   |
        | monitoring      | Standard_DS2_v2 | Low      | 120 minutes  |
    Then all compute instances should be running
    And I should be able to monitor resource utilization
    When the training job completes
    Then high priority compute instances should auto-shutdown
    And monitoring compute should remain running
```

### Example 3: Role-Based Security Testing

```gherkin
Feature: Role-Based Access Control
    As a security administrator
    I want to ensure proper access control
    So that users can only perform authorized actions

Scenario Outline: Test user permissions
    Given I am a user named "<UserName>" with role "<Role>"
    And I have <Role> access to Azure ML
    When I attempt to <Action>
    Then I should <ExpectedResult>

Examples:
    | UserName | Role        | Action                    | ExpectedResult           |
    | Admin    | Owner       | create compute instance   | succeed                  |
    | Dev      | Contributor | create compute instance   | succeed                  |
    | Analyst  | Reader      | create compute instance   | receive access denied    |
    | Guest    | None        | access workspace          | receive authentication error |
    | Admin    | Owner       | delete workspace          | succeed                  |
    | Dev      | Contributor | delete workspace          | receive insufficient permissions |
```

### Example 4: End-to-End ML Workflow

```gherkin
Feature: Complete ML Workflow
    As a data scientist
    I want to execute a complete ML workflow
    So that I can train and deploy a model

Scenario: Train and deploy a classification model
    Given I am a data scientist named "DataScientist"
    And I have Contributor access to Azure ML
    And I have access to Azure AI Search
    And I have opened workspace "ml-workspace"
    
    When I upload dataset "customer-data.csv"
    And I create a new experiment "customer-classification"
    And I start compute instance "training-compute"
    And I create a training script with the following parameters:
        | Parameter     | Value                    |
        | Algorithm     | RandomForest            |
        | MaxDepth      | 10                      |
        | Estimators    | 100                     |
        | TestSize      | 0.2                     |
    And I submit the training job
    
    Then the training job should complete successfully
    And the model accuracy should be greater than 0.85
    And the model should be registered in the model registry
    
    When I create a deployment endpoint "customer-classifier-endpoint"
    And I deploy the model to the endpoint
    
    Then the endpoint should be healthy
    And I should be able to make predictions
    And the response time should be less than 2 seconds
```

### Example 5: Custom Task Implementation

```csharp
public class TrainModel : ITask
{
    private readonly string _experimentName;
    private readonly string _datasetName;
    private readonly Dictionary<string, object> _parameters;

    public string Name => $"Train model for experiment '{_experimentName}'";

    private TrainModel(string experimentName, string datasetName, Dictionary<string, object> parameters)
    {
        _experimentName = experimentName;
        _datasetName = datasetName;
        _parameters = parameters;
    }

    public static TrainModel ForExperiment(string experimentName)
    {
        return new TrainModel(experimentName, "", new Dictionary<string, object>());
    }

    public TrainModel WithDataset(string datasetName)
    {
        return new TrainModel(_experimentName, datasetName, _parameters);
    }

    public TrainModel WithParameter(string key, object value)
    {
        var newParams = new Dictionary<string, object>(_parameters) { [key] = value };
        return new TrainModel(_experimentName, _datasetName, newParams);
    }

    public async Task PerformAs(IActor actor)
    {
        var azureML = actor.Using<UseAzureML>();
        var browser = actor.Using<BrowseTheWeb>();
        
        // Navigate to experiments
        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://ml.azure.com/experiments");
        
        // Create or select experiment
        await page.ClickAsync($"[data-testid='experiment-{_experimentName}']");
        
        // Configure training job
        await page.ClickAsync("[data-testid='new-run-btn']");
        await page.SelectOptionAsync("[data-testid='dataset-select']", _datasetName);
        
        // Set parameters
        foreach (var param in _parameters)
        {
            await page.FillAsync($"[data-testid='param-{param.Key}']", param.Value.ToString());
        }
        
        // Submit job
        await page.ClickAsync("[data-testid='submit-job-btn']");
        
        // Wait for job to start
        await page.WaitForSelectorAsync("[data-testid='job-status'][data-status='Running']");
        
        // Store job ID for later reference
        var jobId = await page.GetAttributeAsync("[data-testid='job-id']", "data-job-id");
        actor.Remember("TrainingJobId", jobId);
        
        // Wait for completion (with timeout)
        await page.WaitForSelectorAsync(
            "[data-testid='job-status'][data-status='Completed']", 
            new PageWaitForSelectorOptions { Timeout = 1800000 }); // 30 minutes
    }
}
```

## 🎯 Best Practices

### Test Design Principles

#### 1. Write Readable Tests
```gherkin
# Good: Clear, business-focused scenario
Scenario: Data scientist creates and runs experiment
    Given I am a data scientist named "Alice"
    And I have access to the ML workspace
    When I create an experiment with customer data
    And I train a classification model
    Then the model should achieve acceptable accuracy
    And the results should be saved to the model registry

# Avoid: Technical, implementation-focused scenario
Scenario: HTTP POST to /api/experiments endpoint
    Given I have valid authentication token
    When I send POST request with JSON payload
    Then I should receive 201 status code
    And response should contain experiment ID
```

#### 2. Keep Scenarios Focused
```gherkin
# Good: Single responsibility
Scenario: Start compute instance
    Given I have opened the ML workspace
    When I start compute instance "test-compute"
    Then the compute instance should be running

# Avoid: Multiple responsibilities
Scenario: Complete ML workflow
    Given I have opened the ML workspace
    When I start compute instance "test-compute"
    And I upload dataset "data.csv"
    And I create experiment "test-exp"
    And I train a model
    And I deploy the model
    Then everything should work correctly
```

#### 3. Use Meaningful Test Data
```csharp
// Good: Descriptive test data
public static class TestData
{
    public static readonly string DataScientistUser = "alice.smith";
    public static readonly string MLWorkspaceName = "customer-analytics-workspace";
    public static readonly string TrainingComputeName = "gpu-training-cluster";
    public static readonly string InferenceComputeName = "cpu-inference-cluster";
}

// Avoid: Generic test data
public static class TestData
{
    public static readonly string User1 = "user1";
    public static readonly string Workspace1 = "ws1";
    public static readonly string Compute1 = "comp1";
}
```

### Code Organization

#### 1. Logical File Structure
```
AzureMLWorkspace.Tests/
├── Features/                    # BDD feature files
│   ├── WorkspaceManagement/
│   ├── ComputeManagement/
│   ├── ExperimentManagement/
│   └── ModelDeployment/
├── StepDefinitions/            # Step definition classes
│   ├── WorkspaceSteps.cs
│   ├── ComputeSteps.cs
│   └── ExperimentSteps.cs
├── Framework/                  # Core framework
│   ├── Abilities/
│   ├── Tasks/
│   ├── Questions/
│   └── Screenplay/
├── TestData/                   # Test data files
└── Configuration/              # Configuration classes
```

#### 2. Consistent Naming Conventions
```csharp
// Abilities: Use[Service/System]
public class UseAzureML : IAbility { }
public class UseAzureAISearch : IAbility { }
public class BrowseTheWeb : IAbility { }

// Tasks: [Verb][Object]
public class StartCompute : ITask { }
public class CreateExperiment : ITask { }
public class DeployModel : ITask { }

// Questions: [Object][Property/State]
public class ComputeStatus : IQuestion<string> { }
public class ModelAccuracy : IQuestion<double> { }
public class WorkspaceExists : IQuestion<bool> { }
```

#### 3. Proper Error Handling
```csharp
public async Task PerformAs(IActor actor)
{
    try
    {
        var azureML = actor.Using<UseAzureML>();
        await azureML.StartCompute(_computeName);
        
        // Wait for compute to be ready
        await WaitForComputeReady(actor, _computeName);
    }
    catch (ComputeNotFoundException ex)
    {
        throw new TaskExecutionException(
            $"Compute instance '{_computeName}' not found", ex);
    }
    catch (InsufficientPermissionsException ex)
    {
        throw new TaskExecutionException(
            $"Insufficient permissions to start compute '{_computeName}'", ex);
    }
    catch (Exception ex)
    {
        throw new TaskExecutionException(
            $"Failed to start compute instance '{_computeName}'", ex);
    }
}
```

### Performance Optimization

#### 1. Resource Management
```csharp
public class UseAzureML : IAbility, IAsyncDisposable
{
    private ArmClient? _armClient;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task InitializeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_armClient == null)
            {
                _armClient = new ArmClient(_credential);
                // Initialize workspace connection
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _armClient?.Dispose();
            _armClient = null;
        }
        finally
        {
            _semaphore.Release();
            _semaphore.Dispose();
        }
    }
}
```

#### 2. Parallel Execution
```csharp
[Test, Parallelizable(ParallelScope.Self)]
public async Task MultipleComputeInstancesCanBeStartedInParallel()
{
    var tasks = new[]
    {
        _actor.AttemptsTo(StartCompute.Named("compute-1")),
        _actor.AttemptsTo(StartCompute.Named("compute-2")),
        _actor.AttemptsTo(StartCompute.Named("compute-3"))
    };

    await Task.WhenAll(tasks);

    // Verify all instances are running
    await _actor.Should(Validate.ComputeStatus("compute-1", "Running"));
    await _actor.Should(Validate.ComputeStatus("compute-2", "Running"));
    await _actor.Should(Validate.ComputeStatus("compute-3", "Running"));
}
```

#### 3. Caching and Reuse
```csharp
public class BrowserPool
{
    private static readonly ConcurrentQueue<IBrowser> _availableBrowsers = new();
    private static readonly SemaphoreSlim _semaphore = new(Environment.ProcessorCount);

    public static async Task<IBrowser> GetBrowserAsync()
    {
        await _semaphore.WaitAsync();
        
        if (_availableBrowsers.TryDequeue(out var browser))
        {
            return browser;
        }

        var playwright = await Playwright.CreateAsync();
        return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public static void ReturnBrowser(IBrowser browser)
    {
        _availableBrowsers.Enqueue(browser);
        _semaphore.Release();
    }
}
```

### Security Best Practices

#### 1. Secure Configuration Management
```csharp
public class SecureConfigurationProvider
{
    public static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
            .AddEnvironmentVariables()
            .AddAzureKeyVault(vaultUri, credential); // For production secrets

        return builder.Build();
    }
}
```

#### 2. Credential Management
```csharp
public class SecureCredentialProvider
{
    public static TokenCredential GetCredential()
    {
        // Try different credential sources in order of preference
        var credentialOptions = new DefaultAzureCredentialOptions
        {
            ExcludeEnvironmentCredential = false,
            ExcludeInteractiveBrowserCredential = !IsInteractiveEnvironment(),
            ExcludeManagedIdentityCredential = !IsManagedIdentityEnvironment(),
            ExcludeSharedTokenCacheCredential = false,
            ExcludeVisualStudioCredential = !IsDevelopmentEnvironment(),
            ExcludeAzureCliCredential = false
        };

        return new DefaultAzureCredential(credentialOptions);
    }
}
```

#### 3. Data Protection
```csharp
public class TestDataProtection
{
    public static void SanitizeTestData(object testData)
    {
        // Remove or mask sensitive information from test data
        var json = JsonSerializer.Serialize(testData);
        var sanitized = Regex.Replace(json, @"""password""\s*:\s*""[^""]*""", @"""password"":""***""");
        sanitized = Regex.Replace(sanitized, @"""apiKey""\s*:\s*""[^""]*""", @"""apiKey"":""***""");
        
        // Log sanitized version only
        Logger.LogDebug("Test data: {SanitizedData}", sanitized);
    }
}
```

## 🔧 Troubleshooting

### Common Issues and Solutions

#### 1. Authentication Problems

**Issue**: `Unable to authenticate with Azure`
```
Azure.Identity.AuthenticationFailedException: DefaultAzureCredential failed to retrieve a token from the included credentials.
```

**Solutions**:
```bash
# Check Azure CLI login
az login
az account show

# Verify subscription access
az account list --output table

# Set correct subscription
az account set --subscription "your-subscription-id"

# Check environment variables
echo $AZURE_SUBSCRIPTION_ID
echo $AZURE_TENANT_ID
```

**Code Solution**:
```csharp
public static class AuthenticationTroubleshooter
{
    public static async Task DiagnoseAuthenticationAsync()
    {
        var credential = new DefaultAzureCredential();
        
        try
        {
            var tokenRequest = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
            var token = await credential.GetTokenAsync(tokenRequest);
            Console.WriteLine($"Authentication successful. Token expires: {token.ExpiresOn}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
            
            // Try individual credential types
            await TryCredentialType(new EnvironmentCredential(), "Environment");
            await TryCredentialType(new ManagedIdentityCredential(), "Managed Identity");
            await TryCredentialType(new AzureCliCredential(), "Azure CLI");
        }
    }
}
```

#### 2. Browser and Playwright Issues

**Issue**: `Browser executable not found`
```
Microsoft.Playwright.PlaywrightException: Executable doesn't exist at /path/to/browser
```

**Solutions**:
```bash
# Reinstall Playwright browsers
pwsh bin/Debug/net8.0/playwright.ps1 install --with-deps

# Check installed browsers
pwsh bin/Debug/net8.0/playwright.ps1 install --dry-run

# Install specific browser
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

**Issue**: `Page timeout waiting for element`
```
Microsoft.Playwright.TimeoutException: Timeout 30000ms exceeded waiting for selector
```

**Solutions**:
```csharp
// Increase timeout
await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
{ 
    Timeout = 60000 
});

// Use more specific selectors
await page.WaitForSelectorAsync("[data-testid='specific-element']");

// Wait for network idle
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// Add explicit waits
await page.WaitForTimeoutAsync(2000);
```

#### 3. Configuration Issues

**Issue**: `Configuration value not found`
```
System.InvalidOperationException: Azure:SubscriptionId not configured
```

**Solutions**:
```csharp
public static class ConfigurationValidator
{
    public static void ValidateConfiguration(IConfiguration configuration)
    {
        var requiredSettings = new[]
        {
            "Azure:SubscriptionId",
            "Azure:ResourceGroup",
            "Azure:WorkspaceName",
            "Azure:TenantId"
        };

        var missingSettings = requiredSettings
            .Where(setting => string.IsNullOrEmpty(configuration[setting]))
            .ToList();

        if (missingSettings.Any())
        {
            throw new InvalidOperationException(
                $"Missing required configuration settings: {string.Join(", ", missingSettings)}");
        }
    }
}
```

#### 4. Resource Access Issues

**Issue**: `Resource not found or access denied`
```
Azure.RequestFailedException: The Resource 'Microsoft.MachineLearningServices/workspaces/my-workspace' under resource group 'my-rg' was not found.
```

**Solutions**:
```csharp
public static class ResourceValidator
{
    public static async Task ValidateResourceAccessAsync(UseAzureML azureML)
    {
        try
        {
            // Test workspace access
            var workspace = azureML.Workspace;
            var workspaceData = workspace.Data;
            Console.WriteLine($"Workspace found: {workspaceData.Name}");
            
            // Test compute access
            var computes = workspace.GetMachineLearningComputes();
            await foreach (var compute in computes)
            {
                Console.WriteLine($"Compute found: {compute.Data.Name}");
            }
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            Console.WriteLine("Resource not found. Check resource names and permissions.");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 403)
        {
            Console.WriteLine("Access denied. Check RBAC permissions.");
        }
    }
}
```

#### 5. Test Execution Issues

**Issue**: `Tests fail intermittently`

**Solutions**:
```csharp
// Add retry logic
public static class RetryHelper
{
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts = 3,
        TimeSpan delay = default)
    {
        if (delay == default) delay = TimeSpan.FromSeconds(1);
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxAttempts && IsRetryableException(ex))
            {
                await Task.Delay(delay * attempt);
            }
        }
        
        return await operation(); // Final attempt without catch
    }
    
    private static bool IsRetryableException(Exception ex)
    {
        return ex is TimeoutException ||
               ex is HttpRequestException ||
               (ex is Azure.RequestFailedException azEx && azEx.Status >= 500);
    }
}
```

### Debugging Tools and Techniques

#### 1. Enable Detailed Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.Playwright": "Information",
      "Azure": "Information",
      "AzureMLWorkspace.Tests": "Trace"
    }
  }
}
```

#### 2. Use Playwright Inspector
```bash
# Enable Playwright Inspector
export PWDEBUG=1
dotnet test --filter "DisplayName~'Your Test Name'"
```

#### 3. Capture Test Artifacts
```csharp
[TearDown]
public async Task TearDown()
{
    if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
    {
        // Capture screenshot
        var screenshot = await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = $"screenshots/{TestContext.CurrentContext.Test.Name}.png",
            FullPage = true
        });
        
        // Capture page content
        var content = await _page.ContentAsync();
        await File.WriteAllTextAsync($"page-content/{TestContext.CurrentContext.Test.Name}.html", content);
        
        // Capture console logs
        var logs = _page.ConsoleMessages.Select(msg => $"{msg.Type}: {msg.Text}");
        await File.WriteAllLinesAsync($"console-logs/{TestContext.CurrentContext.Test.Name}.log", logs);
    }
}
```

#### 4. Performance Profiling
```csharp
public class PerformanceProfiler
{
    private readonly Stopwatch _stopwatch = new();
    private readonly List<(string Operation, TimeSpan Duration)> _measurements = new();

    public void StartMeasurement(string operation)
    {
        _stopwatch.Restart();
    }

    public void EndMeasurement(string operation)
    {
        _stopwatch.Stop();
        _measurements.Add((operation, _stopwatch.Elapsed));
    }

    public void LogResults()
    {
        foreach (var (operation, duration) in _measurements)
        {
            Console.WriteLine($"{operation}: {duration.TotalMilliseconds:F2}ms");
        }
    }
}
```

### Getting Help

#### 1. Documentation Resources
- [Framework Documentation](docs/)
- [API Reference](docs/API_DOCUMENTATION.md)
- [Architecture Guide](docs/ARCHITECTURE_DOCUMENTATION.md)
- [User Guide](docs/USER_GUIDE.md)

#### 2. Community Support
- GitHub Issues: Report bugs and request features
- Discussions: Ask questions and share experiences
- Wiki: Community-contributed guides and examples

#### 3. Professional Support
- Enterprise support available for production deployments
- Training and consulting services
- Custom development and integration

## 🤝 Contributing

We welcome contributions from the community! Here's how you can help:

### Getting Started

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Make your changes**
4. **Add tests** for new functionality
5. **Run the test suite**: `dotnet test`
6. **Commit your changes**: `git commit -m 'Add amazing feature'`
7. **Push to the branch**: `git push origin feature/amazing-feature`
8. **Open a Pull Request**

### Contribution Guidelines

#### Code Standards
- Follow C# coding conventions
- Use meaningful names for classes, methods, and variables
- Add XML documentation for public APIs
- Include unit tests for new functionality
- Ensure all tests pass before submitting

#### Documentation
- Update README.md for new features
- Add or update API documentation
- Include examples for new functionality
- Update configuration documentation if needed

#### Pull Request Process
1. Ensure your PR has a clear description
2. Link to any related issues
3. Include screenshots for UI changes
4. Ensure CI/CD pipeline passes
5. Request review from maintainers

### Development Setup

```bash
# Clone your fork
git clone https://github.com/your-username/AZ_ML_Workspace.git
cd AZ_ML_Workspace

# Add upstream remote
git remote add upstream https://github.com/original-repo/AZ_ML_Workspace.git

# Create development branch
git checkout -b develop

# Install dependencies
dotnet restore
pwsh bin/Debug/net8.0/playwright.ps1 install

# Run tests
dotnet test
```

### Types of Contributions

- **Bug fixes**: Help us squash bugs
- **New features**: Add new testing capabilities
- **Documentation**: Improve guides and examples
- **Performance**: Optimize test execution
- **Security**: Enhance security features
- **Examples**: Add more test scenarios

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### MIT License Summary

- ✅ Commercial use
- ✅ Modification
- ✅ Distribution
- ✅ Private use
- ❌ Liability
- ❌ Warranty

## 🙏 Acknowledgments

- **Microsoft Azure Team** for the excellent Azure SDKs
- **Playwright Team** for the amazing browser automation framework
- **Reqnroll Team** for the modern BDD framework
- **NUnit Team** for the robust testing framework
- **Serilog Team** for the flexible logging framework
- **Community Contributors** for their valuable feedback and contributions

## 📞 Support

### Community Support
- **GitHub Issues**: [Report bugs and request features](https://github.com/your-org/AZ_ML_Workspace/issues)
- **Discussions**: [Ask questions and share experiences](https://github.com/your-org/AZ_ML_Workspace/discussions)
- **Wiki**: [Community guides and examples](https://github.com/your-org/AZ_ML_Workspace/wiki)

### Professional Support
- **Enterprise Support**: Available for production deployments
- **Training Services**: Comprehensive training programs
- **Consulting**: Custom development and integration services
- **Priority Support**: Dedicated support channels

### Contact Information
- **Email**: support@your-org.com
- **Slack**: [Join our community](https://your-org.slack.com)
- **Twitter**: [@YourOrgHandle](https://twitter.com/YourOrgHandle)

---

**Made with ❤️ by the Azure ML Testing Framework Team**

*Empowering teams to build reliable, maintainable, and scalable test automation for Azure Machine Learning workspaces.*