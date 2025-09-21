# Azure ML Workspace Testing Framework - User Guide

## Table of Contents
1. [Getting Started](#getting-started)
2. [Writing BDD Tests](#writing-bdd-tests)
3. [Using the Screenplay Pattern](#using-the-screenplay-pattern)
4. [Configuration Guide](#configuration-guide)
5. [Running Tests](#running-tests)
6. [Test Data Management](#test-data-management)
7. [Debugging and Troubleshooting](#debugging-and-troubleshooting)
8. [Best Practices](#best-practices)
9. [Examples and Tutorials](#examples-and-tutorials)

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** or later
- **Visual Studio 2022** or **VS Code** with C# extension
- **Azure subscription** with Machine Learning workspace
- **Git** for version control

### Installation

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd AZ_ML_Workspace
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Install Playwright browsers:**
   ```bash
   pwsh bin/Debug/net8.0/playwright.ps1 install
   ```
   
   Or on Linux/macOS:
   ```bash
   ./bin/Debug/net8.0/playwright.sh install
   ```

### Initial Configuration

1. **Copy the example configuration:**
   ```bash
   cp .env.example .env.test
   ```

2. **Update configuration with your Azure details:**
   ```json
   {
     "Azure": {
       "SubscriptionId": "your-subscription-id",
       "ResourceGroup": "your-resource-group",
       "WorkspaceName": "your-workspace-name",
       "TenantId": "your-tenant-id"
     }
   }
   ```

3. **Verify installation:**
   ```bash
   dotnet test --filter "Category=Smoke"
   ```

## Writing BDD Tests

### Understanding Gherkin Syntax

The framework uses Gherkin syntax for writing behavior-driven tests. Here's the basic structure:

```gherkin
Feature: Feature Name
    As a [role]
    I want [goal]
    So that [benefit]

Background:
    Given [common setup steps]

Scenario: Scenario Name
    Given [initial context]
    When [action is performed]
    Then [expected outcome]
    And [additional verification]
```

### Creating Your First Feature File

1. **Create a new feature file** in the `Features` directory:

```gherkin
# Features/MyWorkspace.feature
Feature: My Azure ML Workspace Testing
    As a data scientist
    I want to manage my ML workspace
    So that I can perform machine learning tasks efficiently

Background:
    Given I am a data scientist named "Alice"
    And I have Contributor access to Azure ML

Scenario: Access my workspace
    When I attempt to open workspace "my-ml-workspace"
    Then I should be able to access the workspace
    And the workspace should be available

Scenario: Manage compute resources
    Given I have opened workspace "my-ml-workspace"
    When I start compute instance "my-compute-instance"
    Then the compute instance should be running
    And I should be able to connect to it
```

2. **Generate step definitions** (if using Visual Studio):
   - Right-click on the feature file
   - Select "Generate Step Definitions"
   - Choose the appropriate step definition class

### Available Step Definitions

#### Given Steps (Setup)

```gherkin
Given I am a data scientist named "John"
Given I have Contributor access to Azure ML
Given I have Reader access to Azure ML
Given I have Owner access to Azure ML
Given I have access to Azure AI Search
Given I have opened workspace "workspace-name"
Given compute instance "compute-name" is running
```

#### When Steps (Actions)

```gherkin
When I attempt to open workspace "workspace-name"
When I start compute instance "compute-name"
When I stop compute instance "compute-name"
When I start compute instances:
    | ComputeName    |
    | compute-1      |
    | compute-2      |
When I stop all compute instances
```

#### Then Steps (Assertions)

```gherkin
Then I should be able to access the workspace
Then the workspace should be available
Then the compute instance should be running
Then I should be able to connect to it
Then the compute instance should be stopped
Then all compute instances should be running
Then all compute instances should be stopped
```

### Advanced Gherkin Features

#### Using Data Tables

```gherkin
Scenario: Start multiple compute instances
    Given I have opened workspace "ml-workspace"
    When I start compute instances:
        | ComputeName    | Size      | Priority |
        | gpu-compute-1  | Standard  | High     |
        | cpu-compute-1  | Basic     | Low      |
        | gpu-compute-2  | Premium   | Medium   |
    Then all compute instances should be running
```

#### Using Scenario Outlines

```gherkin
Scenario Outline: Test different user roles
    Given I am a data scientist named "<UserName>"
    And I have <Role> access to Azure ML
    When I attempt to open workspace "<WorkspaceName>"
    Then I should <ExpectedResult>

Examples:
    | UserName | Role        | WorkspaceName | ExpectedResult              |
    | Alice    | Owner       | ml-workspace  | be able to access the workspace |
    | Bob      | Contributor | ml-workspace  | be able to access the workspace |
    | Charlie  | Reader      | ml-workspace  | be able to access the workspace |
```

#### Using Background for Common Setup

```gherkin
Feature: Compute Instance Management

Background:
    Given I am a data scientist named "DataScientist"
    And I have Contributor access to Azure ML
    And I have opened workspace "test-workspace"

Scenario: Start compute instance
    When I start compute instance "test-compute"
    Then the compute instance should be running

Scenario: Stop compute instance
    Given compute instance "test-compute" is running
    When I stop compute instance "test-compute"
    Then the compute instance should be stopped
```

## Using the Screenplay Pattern

### Understanding the Screenplay Pattern

The Screenplay pattern organizes test code around:
- **Actors**: Who performs the actions (users, systems)
- **Abilities**: What an actor can do (use Azure ML, browse web)
- **Tasks**: High-level business actions (start compute, open workspace)
- **Questions**: Queries about system state (is compute running?)

### Writing Custom Tasks

1. **Create a new task class:**

```csharp
using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Abilities;

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
        
        // Click create new notebook
        await page.ClickAsync("[data-testid='create-notebook-btn']");
        
        // Fill in notebook details
        await page.FillAsync("[data-testid='notebook-name']", _notebookName);
        await page.SelectOptionAsync("[data-testid='kernel-select']", _kernelType);
        
        // Create the notebook
        await page.ClickAsync("[data-testid='create-btn']");
        
        // Wait for notebook to be created
        await page.WaitForSelectorAsync($"[data-testid='notebook-{_notebookName}']");
    }
}
```

2. **Use the task in step definitions:**

```csharp
[When(@"I create a notebook named ""(.*)""")]
public async Task WhenICreateANotebookNamed(string notebookName)
{
    if (_actor == null)
        throw new InvalidOperationException("Actor must be created first");

    await _actor.AttemptsTo(CreateNotebook.Named(notebookName));
}
```

### Writing Custom Questions

1. **Create a question class:**

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

2. **Use the question in assertions:**

```csharp
[Then(@"the notebook ""(.*)"" should exist")]
public async Task ThenTheNotebookShouldExist(string notebookName)
{
    if (_actor == null)
        throw new InvalidOperationException("Actor must be created first");

    await _actor.Should(NotebookExists.Named(notebookName));
}
```

### Writing Custom Abilities

1. **Create an ability class:**

```csharp
public class UseJupyterNotebook : IAbility
{
    private readonly ILogger<UseJupyterNotebook> _logger;
    private IPage? _notebookPage;

    public string Name => "Use Jupyter Notebook";

    private UseJupyterNotebook(ILogger<UseJupyterNotebook> logger)
    {
        _logger = logger;
    }

    public static UseJupyterNotebook WithDefaultSettings()
    {
        var logger = TestContext.ServiceProvider.GetRequiredService<ILogger<UseJupyterNotebook>>();
        return new UseJupyterNotebook(logger);
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Jupyter Notebook ability");
        // Initialize notebook connection
        await Task.CompletedTask;
    }

    public async Task CleanupAsync()
    {
        _logger.LogInformation("Cleaning up Jupyter Notebook ability");
        if (_notebookPage != null)
        {
            await _notebookPage.CloseAsync();
        }
    }

    public async Task ExecuteCell(string code)
    {
        if (_notebookPage == null)
            throw new InvalidOperationException("Notebook page not initialized");

        // Add code to cell
        await _notebookPage.FillAsync(".CodeMirror textarea", code);
        
        // Execute cell
        await _notebookPage.Keyboard.PressAsync("Shift+Enter");
        
        // Wait for execution to complete
        await _notebookPage.WaitForSelectorAsync(".output_area", 
            new PageWaitForSelectorOptions { Timeout = 30000 });
    }
}
```

2. **Grant the ability to an actor:**

```csharp
[Given(@"I have access to Jupyter notebooks")]
public async Task GivenIHaveAccessToJupyterNotebooks()
{
    if (_actor == null)
        throw new InvalidOperationException("Actor must be created first");

    var notebookAbility = UseJupyterNotebook.WithDefaultSettings();
    _actor.Can(notebookAbility);
    await notebookAbility.InitializeAsync();
}
```

## Configuration Guide

### Configuration Files

The framework uses a hierarchical configuration system:

1. **appsettings.json** - Base configuration
2. **appsettings.test.json** - Test-specific overrides
3. **Environment variables** - Runtime overrides

### Azure Configuration

```json
{
  "Azure": {
    "SubscriptionId": "your-subscription-id",
    "ResourceGroup": "your-resource-group",
    "WorkspaceName": "your-workspace-name",
    "TenantId": "your-tenant-id",
    "ClientId": "optional-service-principal-id",
    "ClientSecret": "optional-service-principal-secret",
    "Environment": "AzureCloud"
  }
}
```

### Playwright Configuration

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

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning",
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

### Environment Variables

You can override any configuration using environment variables:

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

### Role-Based Configuration

Configure different roles for testing:

```json
{
  "Roles": {
    "DataScientist": {
      "Permissions": ["ReadWorkspace", "CreateCompute", "RunExperiments"],
      "DefaultWorkspace": "ds-workspace"
    },
    "MLEngineer": {
      "Permissions": ["ReadWorkspace", "ManageCompute", "DeployModels"],
      "DefaultWorkspace": "ml-workspace"
    },
    "Administrator": {
      "Permissions": ["*"],
      "DefaultWorkspace": "admin-workspace"
    }
  }
}
```

## Running Tests

### Command Line Options

#### Run All Tests
```bash
dotnet test
```

#### Run Specific Categories
```bash
# Run only BDD scenarios
dotnet test --filter "Category=BDD"

# Run only unit tests
dotnet test --filter "Category=Unit"

# Run integration tests
dotnet test --filter "Category=Integration"

# Run smoke tests
dotnet test --filter "Category=Smoke"
```

#### Run Specific Features
```bash
# Run specific feature file
dotnet test --filter "FullyQualifiedName~AzureMLWorkspace"

# Run specific scenario
dotnet test --filter "DisplayName~'Access Azure ML Workspace'"
```

#### Browser-Specific Testing
```bash
# Run with Chromium
dotnet test -- Playwright.BrowserName=chromium

# Run with Firefox
dotnet test -- Playwright.BrowserName=firefox

# Run with WebKit (Safari)
dotnet test -- Playwright.BrowserName=webkit
```

#### Debug Mode
```bash
# Run in headed mode (visible browser)
dotnet test -- Playwright.HeadlessMode=false

# Run with slow motion
dotnet test -- Playwright.SlowMo=true

# Capture videos
dotnet test -- Playwright.CaptureVideos=true
```

### Using Test Scripts

#### Windows (PowerShell)
```powershell
# Run all tests
.\run-tests.ps1

# Run with specific browser
.\run-tests.ps1 -Browser firefox

# Run in debug mode
.\run-tests.ps1 -Debug

# Run specific category
.\run-tests.ps1 -Category Integration
```

#### Linux/macOS (Bash)
```bash
# Run all tests
./run-tests.sh

# Run with specific browser
./run-tests.sh --browser firefox

# Run in debug mode
./run-tests.sh --debug

# Run specific category
./run-tests.sh --category Integration
```

### IDE Integration

#### Visual Studio 2022
1. Open Test Explorer (Test → Test Explorer)
2. Build the solution to discover tests
3. Right-click on tests to run/debug
4. Use filters to organize tests by category

#### VS Code
1. Install C# extension
2. Install .NET Core Test Explorer extension
3. Use Command Palette (Ctrl+Shift+P) → ".NET: Run Tests"
4. View results in Test Explorer panel

### Continuous Integration

#### Azure DevOps Pipeline
```yaml
# azure-pipelines.yml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- script: dotnet restore
  displayName: 'Restore packages'

- script: |
    pwsh bin/Debug/net8.0/playwright.ps1 install --with-deps
  displayName: 'Install Playwright browsers'

- script: dotnet test --logger trx --collect:"XPlat Code Coverage"
  displayName: 'Run tests'
  env:
    Azure__SubscriptionId: $(AZURE_SUBSCRIPTION_ID)
    Azure__ResourceGroup: $(AZURE_RESOURCE_GROUP)
    Azure__WorkspaceName: $(AZURE_WORKSPACE_NAME)

- task: PublishTestResults@2
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
```

#### GitHub Actions
```yaml
# .github/workflows/test.yml
name: Test

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Install Playwright
      run: pwsh bin/Debug/net8.0/playwright.ps1 install --with-deps
    
    - name: Run tests
      run: dotnet test --verbosity normal
      env:
        Azure__SubscriptionId: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
        Azure__ResourceGroup: ${{ secrets.AZURE_RESOURCE_GROUP }}
        Azure__WorkspaceName: ${{ secrets.AZURE_WORKSPACE_NAME }}
```

## Test Data Management

### Static Test Data

Store static test data in the `TestData` directory:

```
TestData/
├── azure-ml-config.json
├── sample-notebook.ipynb
├── sample-dataset.csv
├── requirements.txt
└── test-users.json
```

#### Example test-users.json
```json
{
  "users": [
    {
      "name": "Alice",
      "role": "DataScientist",
      "email": "alice@company.com",
      "permissions": ["ReadWorkspace", "CreateCompute"]
    },
    {
      "name": "Bob",
      "role": "MLEngineer",
      "email": "bob@company.com",
      "permissions": ["ReadWorkspace", "ManageCompute", "DeployModels"]
    }
  ]
}
```

### Dynamic Test Data

Use the `TestDataGenerator` class for dynamic data:

```csharp
// In step definitions
[Given(@"I have a randomly generated compute instance")]
public async Task GivenIHaveARandomlyGeneratedComputeInstance()
{
    var computeName = TestDataGenerator.GenerateComputeName();
    _actor.Remember("ComputeName", computeName);
    
    await _actor.AttemptsTo(StartCompute.Named(computeName));
}

[When(@"I use the remembered compute instance")]
public async Task WhenIUseTheRememberedComputeInstance()
{
    var computeName = _actor.Recall<string>("ComputeName");
    await _actor.AttemptsTo(ConnectToCompute.Named(computeName));
}
```

### Data-Driven Tests

Use scenario outlines for data-driven testing:

```gherkin
Scenario Outline: Test different compute sizes
    Given I have opened workspace "test-workspace"
    When I create compute instance "<ComputeName>" with size "<Size>"
    Then the compute instance should be created with size "<Size>"
    And the compute instance should be running

Examples:
    | ComputeName | Size           |
    | small-cpu   | Standard_DS2_v2|
    | medium-cpu  | Standard_DS3_v2|
    | large-cpu   | Standard_DS4_v2|
    | gpu-basic   | Standard_NC6   |
```

### Test Data Cleanup

Implement cleanup in hooks:

```csharp
[AfterScenario("@cleanup")]
public async Task CleanupTestData()
{
    if (_actor != null && _actor.Remembers("CreatedResources"))
    {
        var resources = _actor.Recall<List<string>>("CreatedResources");
        
        foreach (var resource in resources)
        {
            try
            {
                await _actor.AttemptsTo(DeleteResource.Named(resource));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup resource: {Resource}", resource);
            }
        }
    }
}
```

## Debugging and Troubleshooting

### Debug Mode

Run tests in debug mode to see browser interactions:

```bash
# Command line
dotnet test -- Playwright.HeadlessMode=false Playwright.SlowMo=true

# Or set in appsettings.test.json
{
  "Playwright": {
    "HeadlessMode": false,
    "SlowMo": true,
    "SlowMoDelay": 1000
  }
}
```

### Screenshots and Videos

Enable automatic capture on failures:

```json
{
  "Playwright": {
    "CaptureScreenshots": true,
    "CaptureVideos": true,
    "CaptureTraces": true,
    "ScreenshotMode": "only-on-failure",
    "VideoMode": "retain-on-failure"
  }
}
```

### Logging

Increase logging verbosity for debugging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "AzureMLWorkspace.Tests": "Trace"
    }
  }
}
```

### Common Issues and Solutions

#### Authentication Issues
```
Error: Unable to authenticate with Azure
```

**Solutions:**
1. Ensure you're logged in with Azure CLI: `az login`
2. Check your subscription access: `az account show`
3. Verify configuration values in appsettings.json
4. Check environment variables are set correctly

#### Timeout Issues
```
Error: Timeout waiting for element
```

**Solutions:**
1. Increase timeout values in configuration
2. Add explicit waits in custom tasks
3. Check network connectivity
4. Verify element selectors are correct

#### Resource Not Found
```
Error: Workspace 'test-workspace' not found
```

**Solutions:**
1. Verify workspace name in configuration
2. Check resource group and subscription
3. Ensure you have access permissions
4. Create the workspace if it doesn't exist

#### Browser Issues
```
Error: Browser not found
```

**Solutions:**
1. Reinstall Playwright browsers: `pwsh playwright.ps1 install`
2. Check browser type in configuration
3. Try different browser (chromium, firefox, webkit)
4. Update Playwright to latest version

### Debugging Step Definitions

Add breakpoints and logging in step definitions:

```csharp
[When(@"I start compute instance ""(.*)""")]
public async Task WhenIStartComputeInstance(string computeName)
{
    _logger.LogDebug("Starting compute instance: {ComputeName}", computeName);
    
    if (_actor == null)
        throw new InvalidOperationException("Actor must be created first");

    // Add breakpoint here for debugging
    System.Diagnostics.Debugger.Break();
    
    await _actor.AttemptsTo(StartCompute.Named(computeName));
    _computeInstances.Add(computeName);
    
    _logger.LogDebug("Compute instance started successfully: {ComputeName}", computeName);
}
```

### Using Playwright Inspector

Enable Playwright Inspector for step-by-step debugging:

```bash
# Set environment variable
export PWDEBUG=1

# Run specific test
dotnet test --filter "DisplayName~'Start Compute Instance'"
```

## Best Practices

### Test Organization

1. **Group related scenarios** in the same feature file
2. **Use descriptive scenario names** that explain the business value
3. **Keep scenarios focused** on a single business rule
4. **Use Background** for common setup steps

### Step Definition Guidelines

1. **Keep steps simple** and focused on a single action
2. **Use meaningful parameter names** in regex patterns
3. **Implement proper error handling** and logging
4. **Clean up resources** in AfterScenario hooks

### Code Quality

1. **Follow SOLID principles** in custom tasks and abilities
2. **Use dependency injection** for loose coupling
3. **Implement proper async/await** patterns
4. **Add comprehensive logging** for debugging

### Performance

1. **Reuse browser instances** when possible
2. **Implement proper cleanup** to avoid resource leaks
3. **Use parallel execution** for independent tests
4. **Optimize selectors** for faster element location

### Security

1. **Never commit secrets** to version control
2. **Use environment variables** for sensitive data
3. **Implement proper authentication** handling
4. **Follow least privilege principle** for test accounts

## Examples and Tutorials

### Example 1: Basic Workspace Access

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

### Example 2: Compute Instance Management

```gherkin
Feature: Compute Instance Management
    As a data scientist
    I want to manage compute instances
    So that I can run my ML experiments

Background:
    Given I am a data scientist named "Bob"
    And I have Contributor access to Azure ML
    And I have opened workspace "ml-workspace"

Scenario: Start and stop compute instance
    When I start compute instance "test-compute"
    Then the compute instance should be running
    And I should be able to connect to it
    When I stop compute instance "test-compute"
    Then the compute instance should be stopped

Scenario: Manage multiple compute instances
    When I start compute instances:
        | ComputeName    |
        | compute-cpu-1  |
        | compute-cpu-2  |
        | compute-gpu-1  |
    Then all compute instances should be running
    When I stop all compute instances
    Then all compute instances should be stopped
```

### Example 3: Role-Based Testing

```gherkin
Feature: Role-Based Access Control
    As an administrator
    I want to ensure proper access control
    So that users can only perform authorized actions

Scenario Outline: Different user roles
    Given I am a user named "<UserName>"
    And I have <Role> access to Azure ML
    When I attempt to open workspace "secure-workspace"
    Then I should <ExpectedResult>

Examples:
    | UserName | Role        | ExpectedResult                    |
    | Admin    | Owner       | be able to access the workspace   |
    | Dev      | Contributor | be able to access the workspace   |
    | Viewer   | Reader      | be able to access the workspace   |
    | Guest    | None        | receive an access denied error    |
```

### Example 4: Custom Task Implementation

```csharp
// Custom task for uploading datasets
public class UploadDataset : ITask
{
    private readonly string _datasetName;
    private readonly string _filePath;

    public string Name => $"Upload dataset '{_datasetName}' from '{_filePath}'";

    private UploadDataset(string datasetName, string filePath)
    {
        _datasetName = datasetName;
        _filePath = filePath;
    }

    public static UploadDataset Named(string datasetName, string filePath)
    {
        return new UploadDataset(datasetName, filePath);
    }

    public async Task PerformAs(IActor actor)
    {
        var azureML = actor.Using<UseAzureML>();
        var browser = actor.Using<BrowseTheWeb>();
        
        // Navigate to datasets section
        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://ml.azure.com/datasets");
        
        // Click upload dataset
        await page.ClickAsync("[data-testid='upload-dataset-btn']");
        
        // Fill dataset details
        await page.FillAsync("[data-testid='dataset-name']", _datasetName);
        
        // Upload file
        await page.SetInputFilesAsync("[data-testid='file-input']", _filePath);
        
        // Submit upload
        await page.ClickAsync("[data-testid='upload-submit-btn']");
        
        // Wait for upload completion
        await page.WaitForSelectorAsync($"[data-testid='dataset-{_datasetName}']");
        
        actor.Remember("UploadedDataset", _datasetName);
    }
}
```

### Example 5: Integration with External Services

```csharp
// Custom ability for Azure Storage integration
public class UseAzureStorage : IAbility
{
    private readonly BlobServiceClient _blobClient;
    private readonly ILogger<UseAzureStorage> _logger;

    public string Name => "Use Azure Storage";

    private UseAzureStorage(BlobServiceClient blobClient, ILogger<UseAzureStorage> logger)
    {
        _blobClient = blobClient;
        _logger = logger;
    }

    public static UseAzureStorage WithConnectionString(string connectionString)
    {
        var blobClient = new BlobServiceClient(connectionString);
        var logger = TestContext.ServiceProvider.GetRequiredService<ILogger<UseAzureStorage>>();
        return new UseAzureStorage(blobClient, logger);
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Azure Storage ability");
        // Verify connection
        await _blobClient.GetPropertiesAsync();
    }

    public async Task CleanupAsync()
    {
        _logger.LogInformation("Cleaning up Azure Storage ability");
        // Cleanup logic here
    }

    public async Task UploadBlob(string containerName, string blobName, Stream content)
    {
        var containerClient = _blobClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, overwrite: true);
        
        _logger.LogInformation("Uploaded blob {BlobName} to container {ContainerName}", 
            blobName, containerName);
    }
}
```

This comprehensive user guide provides everything needed to effectively use the Azure ML Workspace Testing Framework, from basic setup to advanced customization scenarios.