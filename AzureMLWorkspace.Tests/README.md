# Azure ML Workspace Test Automation Framework

A comprehensive, modern, cross-platform C# test automation framework for Azure Machine Learning workspaces using **Screenplay Pattern** with Playwright, Azure SDK integration, and BDD support built on .NET 8.0.

## 🚀 Key Features

### Modern Architecture
- **Screenplay Pattern** - Actor/Ability/Task design for maintainable tests
- **No Page Object Model (POM)** - Modern, flexible approach
- **Fluent Interface** - Chainable, readable test actions
- **Azure SDK Integration** - Direct API access with tight coupling
- **Cross-platform Compatibility** - Windows, macOS, and Linux support

### Advanced Capabilities
- **Multi-browser Support** - Chromium, Firefox, WebKit
- **BDD Support** - Reqnroll (SpecFlow replacement) integration
- **Parallel Test Execution** - Per feature, scenario, and browser
- **Retry Logic** - Configurable retry mechanisms for flaky tests
- **Azure Services Integration** - ML, AI Search, Cognitive Services
- **Rich Reporting** - HTML, JSON, Allure reports with screenshots
- **Comprehensive Logging** - Structured logging with Serilog

## 🏗️ Architecture Overview

### Screenplay Pattern
The framework uses the **Screenplay Pattern** with Actors, Abilities, Tasks, and Questions for maintainable and readable tests:

```csharp
// Screenplay Pattern example
var javed = Actor.Named("Javed")
    .Can(UseAzureML.WithRole("Contributor"))
    .Can(BrowseTheWeb.Headlessly())
    .Can(UseAzureAISearch.WithDefaultConfiguration());

await javed
    .AttemptsTo(OpenWorkspace.Named("ml-workspace"))
    .And(StartCompute.Named("test-compute"))
    .Should(Validate.ComputeStatus("test-compute", "Running"))
    .ShouldSee(Validate.AISearchResults("climate-data"), 
        result => result.Should().BeGreaterThan(10));
```

### Azure SDK Integration
Direct integration with Azure services provides robust API testing capabilities:

```csharp
// Azure SDK integration
await javed
    .AttemptsTo(SetPIMRole("Azure ML Data Scientist"))
    .And(StartCompute("ml-workspace", "gpu-compute"))
    .And(TestAISearch("climate change data", "research-index"))
    .Should(Validate.WorkspaceAccess("ml-workspace"));
```

### Core Components

```
AzureMLWorkspace.Tests/
├── Framework/                     # Core framework components
│   ├── Screenplay/               # Screenplay pattern implementation
│   │   ├── IActor.cs            # Actor interface
│   │   ├── Actor.cs             # Actor implementation
│   │   ├── IAbility.cs          # Ability interface
│   │   ├── ITask.cs             # Task interface
│   │   └── IQuestion.cs         # Question interface
│   ├── Abilities/               # Actor abilities
│   │   ├── BrowseTheWeb.cs      # Web browsing with Playwright
│   │   ├── UseAzureML.cs        # Azure ML SDK integration
│   │   └── UseAzureAISearch.cs  # Azure AI Search integration
│   ├── Tasks/                   # Reusable tasks
│   │   ├── OpenWorkspace.cs     # Open ML workspace
│   │   ├── StartCompute.cs      # Start compute instance
│   │   ├── StopCompute.cs       # Stop compute instance
│   │   └── NavigateTo.cs        # Navigate to URL
│   ├── Questions/               # Validation questions
│   │   ├── Validate.cs          # Common validations
│   │   └── ResultCount.cs       # Result count assertions
│   ├── Configuration/           # Configuration management
│   │   └── TestConfiguration.cs # Centralized config
│   ├── Utilities/               # Helper utilities
│   │   ├── RetryHelper.cs       # Retry logic
│   │   └── TestDataGenerator.cs # Test data generation
│   └── TestBase.cs              # Base test class
├── Features/                    # BDD feature files
│   ├── AzureMLWorkspace.feature # Workspace management scenarios
│   └── AzureAISearch.feature    # AI Search scenarios
├── StepDefinitions/             # BDD step definitions
│   ├── AzureMLWorkspaceSteps.cs # Workspace step definitions
│   └── AzureAISearchSteps.cs    # AI Search step definitions
├── Tests/                       # Test classes by category
│   ├── AzureMLWorkspaceUITests.cs    # UI automation tests
│   ├── AzureAISearchAPITests.cs      # API integration tests
│   └── IntegrationTests.cs           # End-to-end tests
├── Hooks/                       # BDD hooks and setup
│   └── TestHooks.cs            # Global test hooks
└── TestData/                    # Test data and fixtures
```

## 🔧 Cross-Platform Setup

### Prerequisites
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** - For version control
- **Azure CLI** - For authentication (optional)
- **PowerShell 7+** - For Windows script execution (Windows)
- **Bash** - For macOS/Linux script execution

### Installation

#### Windows (PowerShell)
```powershell
# Clone repository
git clone <repository-url>
cd AZ_ML_Workspace

# Install dependencies and browsers
dotnet restore AzureMLWorkspace.Tests/
dotnet run --project AzureMLWorkspace.Tests -- playwright install --with-deps

# Run tests
.\run-tests.ps1
```

#### macOS/Linux (Bash)
```bash
# Clone repository
git clone <repository-url>
cd AZ_ML_Workspace

# Install dependencies and browsers
dotnet restore AzureMLWorkspace.Tests/
dotnet run --project AzureMLWorkspace.Tests -- playwright install --with-deps

# Run tests
./run-tests.sh
```

## 🎯 Running Tests

### Cross-Platform Test Runners

#### Windows (PowerShell)
```powershell
# Run all tests
.\run-tests.ps1

# Run specific category
.\run-tests.ps1 -Category "UI" -Browser "firefox" -Headless:$false

# Run API tests with coverage
.\run-tests.ps1 -Category "API" -Coverage -Parallel

# Run BDD tests
.\run-tests.ps1 -Category "BDD" -Environment "staging"

# Run with custom configuration
.\run-tests.ps1 -Category "all" -Browser "all" -GenerateReport -OpenReport
```

#### macOS/Linux (Bash)
```bash
# Run all tests
./run-tests.sh

# Run specific category
./run-tests.sh --category "UI" --browser "webkit" --headed

# Run API tests with coverage
./run-tests.sh --category "API" --coverage --parallel

# Run BDD tests
./run-tests.sh --category "BDD" --environment "staging"

# Run with custom configuration
./run-tests.sh --category "all" --browser "all" --generate-report --open-report
```

### Direct .NET Commands
```bash
# Run all tests
dotnet test

# Run specific category
dotnet test --filter Category=UI

# Run specific test
dotnet test --filter Name~Should_Access_AzureML_Workspace_Successfully

# Run BDD tests
dotnet test --filter Category=BDD

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 Writing Tests with Screenplay Pattern

### Basic Screenplay Usage
```csharp
[Test]
public async Task Should_Access_AzureML_Workspace_Successfully()
{
    // Arrange
    var javed = CreateActor("Javed")
        .Can(BrowseTheWeb.Headlessly(GetLogger<BrowseTheWeb>()))
        .Can(UseAzureML.AsContributor());

    // Act & Assert
    await javed
        .AttemptsTo(NavigateTo.AzureMLPortal())
        .And(OpenWorkspace.Named("ml-workspace"))
        .Should(Validate.WorkspaceAccess("ml-workspace"));
}
```

### Advanced Screenplay Patterns
```csharp
[Test]
public async Task Should_Manage_Complex_ML_Workflow()
{
    // Arrange
    var javed = CreateActor("Javed")
        .Can(BrowseTheWeb.WithViewport(GetLogger<BrowseTheWeb>(), 1920, 1080))
        .Can(UseAzureML.AsContributor())
        .Can(UseAzureAISearch.WithDefaultConfiguration());

    // Act & Assert - Complex workflow
    await javed
        .AttemptsTo(NavigateTo.Workspace("ml-workspace"))
        .And(StartCompute.Named("gpu-compute"))
        .Should(Validate.ComputeStatus("gpu-compute", "Running"))
        .ShouldSee(Validate.AISearchResults("climate-data"), 
            results => results.Should().BeGreaterThan(10))
        .AttemptsTo(StopCompute.Named("gpu-compute"));
}
```

### Azure SDK Integration Examples
```csharp
[Test]
public async Task Should_Test_Azure_Services_Integration()
{
    var javed = CreateActor("Javed")
        .Can(UseAzureML.WithRole("Contributor"))
        .Can(UseAzureAISearch.WithDefaultConfiguration());

    // Test PIM role activation
    await javed.Using<UseAzureML>().SetPIMRole("Azure ML Data Scientist");
    
    // Test compute management
    await javed.AttemptsTo(StartCompute.Named("test-compute"));
    
    // Test AI Search
    var searchResult = await javed.Using<UseAzureAISearch>()
        .TestAISearch("machine learning", "research-index");
    
    searchResult.Success.Should().BeTrue();
    searchResult.TotalResults.Should().BeGreaterThan(0);
}
```

### Creating Custom Tasks
```csharp
public class CreateNotebook : ITask
{
    private readonly string _notebookName;
    private readonly ILogger<CreateNotebook> _logger;

    public string Name => $"Create notebook '{_notebookName}'";

    public CreateNotebook(string notebookName, ILogger<CreateNotebook> logger)
    {
        _notebookName = notebookName;
        _logger = logger;
    }

    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Creating notebook: {NotebookName}", _notebookName);

        if (actor.HasAbility<BrowseTheWeb>())
        {
            var browser = actor.Using<BrowseTheWeb>();
            await browser.Page.ClickAsync("[data-testid='create-notebook']");
            await browser.Page.FillAsync("[data-testid='notebook-name']", _notebookName);
            await browser.Page.ClickAsync("[data-testid='create-button']");
        }

        _logger.LogInformation("Successfully created notebook: {NotebookName}", _notebookName);
    }

    public static CreateNotebook Named(string notebookName)
    {
        return new CreateNotebook(notebookName, 
            TestContext.ServiceProvider.GetRequiredService<ILogger<CreateNotebook>>());
    }
}
```

### Creating Custom Abilities
```csharp
public class UseAzureCognitiveServices : IAbility
{
    private readonly ILogger<UseAzureCognitiveServices> _logger;
    private readonly IConfiguration _configuration;
    
    public string Name => "Use Azure Cognitive Services";

    public UseAzureCognitiveServices(ILogger<UseAzureCognitiveServices> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Azure Cognitive Services client");
        // Initialize your cognitive services client here
        await Task.CompletedTask;
    }

    public async Task CleanupAsync()
    {
        _logger.LogInformation("Cleaning up Azure Cognitive Services resources");
        await Task.CompletedTask;
    }

    public static UseAzureCognitiveServices WithDefaultConfiguration()
    {
        return new UseAzureCognitiveServices(
            TestContext.ServiceProvider.GetRequiredService<ILogger<UseAzureCognitiveServices>>(),
            TestContext.ServiceProvider.GetRequiredService<IConfiguration>());
    }
}
```

## 🧪 Test Categories

### UI Tests (Category=UI)
- Azure ML workspace navigation
- Compute instance management through UI
- Cross-browser compatibility testing
- Responsive design validation
- User interface interactions

### API Tests (Category=API)
- Azure AI Search integration
- Azure ML SDK operations
- REST API validations
- Performance testing
- Error handling verification

### BDD Tests (Category=BDD)
- Behavior-driven scenarios using Reqnroll
- Business-readable test specifications
- Stakeholder collaboration
- End-to-end workflow validation

### Integration Tests (Category=Integration)
- Complete ML pipeline workflows
- Multi-service integrations
- Cross-platform compatibility
- Environment-specific testing

### Performance Tests (Category=Performance)
- Search response time validation
- Compute startup performance
- Concurrent operation testing
- Load testing scenarios

### Security Tests (Category=Security)
- Authentication flows
- Role-based access control (RBAC)
- Privileged Identity Management (PIM)
- Resource access permissions
- Audit logging verification

## 🎭 BDD with Reqnroll

### Feature Files
The framework includes comprehensive BDD support with Reqnroll (SpecFlow replacement):

```gherkin
Feature: Azure ML Workspace Management
    As a data scientist
    I want to manage Azure ML workspaces
    So that I can perform machine learning tasks

Background:
    Given I am a data scientist named "Javed"
    And I have Contributor access to Azure ML

Scenario: Access Azure ML Workspace
    When I attempt to open workspace "ml-workspace"
    Then I should be able to access the workspace
    And the workspace should be available

Scenario: Start Compute Instance
    Given I have opened workspace "ml-workspace"
    When I start compute instance "test-compute"
    Then the compute instance should be running
```

### Step Definitions
Step definitions integrate seamlessly with the Screenplay pattern:

```csharp
[Given(@"I am a data scientist named ""(.*)""")]
public void GivenIAmADataScientistNamed(string name)
{
    _actor = Actor.Named(name, GetLogger<Actor>());
}

[When(@"I attempt to open workspace ""(.*)""")]
public async Task WhenIAttemptToOpenWorkspace(string workspaceName)
{
    await _actor.AttemptsTo(OpenWorkspace.Named(workspaceName));
}

[Then(@"I should be able to access the workspace")]
public async Task ThenIShouldBeAbleToAccessTheWorkspace()
{
    await _actor.Should(Validate.WorkspaceAccess(workspaceName));
}
```

## ⚙️ Configuration

### Environment Configuration
The framework supports multiple environments through configuration files:

### appsettings.json
```json
{
  "Azure": {
    "SubscriptionId": "your-subscription-id",
    "TenantId": "your-tenant-id",
    "ResourceGroup": "your-resource-group",
    "WorkspaceName": "your-workspace-name",
    "Region": "eastus"
  },
  "AzureAISearch": {
    "ServiceName": "your-search-service",
    "IndexName": "your-search-index"
  },
  "Browser": {
    "Headless": true,
    "CaptureScreenshots": true,
    "CaptureVideos": false,
    "CaptureTraces": true,
    "Viewport": {
      "Width": 1920,
      "Height": 1080
    }
  },
  "TestExecution": {
    "ParallelExecution": true,
    "MaxDegreeOfParallelism": 4,
    "DefaultTimeoutSeconds": 30,
    "RetryAttempts": 3
  },
  "Logging": {
    "LogLevel": "Information",
    "EnableFileLogging": true,
    "LogFilePath": "logs/test-execution.log"
  },
  "Reporting": {
    "GenerateHtmlReport": true,
    "GenerateJsonReport": true,
    "ReportOutputPath": "TestResults",
    "IncludeScreenshots": true,
    "IncludeLogs": true
  }
}
```

### Environment Variables
Set these environment variables for CI/CD or local testing:

```bash
# Azure Configuration
export AZURE_SUBSCRIPTION_ID="your-subscription-id"
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_RESOURCE_GROUP="your-resource-group"
export AZURE_WORKSPACE_NAME="your-workspace-name"

# Azure AI Search
export AZUREAISEARCH_SERVICENAME="your-search-service"
export AZUREAISEARCH_INDEXNAME="your-search-index"

# Browser Configuration
export BROWSER_TYPE="chromium"  # chromium, firefox, webkit
export HEADLESS_MODE="true"

# Test Execution
export ASPNETCORE_ENVIRONMENT="test"  # test, staging, production
```

## 📊 Test Artifacts & Reporting

### Generated Artifacts
- **Screenshots** - Captured on failures and key steps
- **Videos** - Full test execution recordings (optional)
- **Traces** - Playwright traces for debugging
- **Logs** - Structured logs with Serilog
- **Test Reports** - HTML, TRX, and JSON formats
- **Coverage Reports** - Code coverage analysis

### Artifact Locations
```
TestResults/
├── Screenshots/           # PNG screenshots by test
├── Videos/               # MP4 recordings (if enabled)
├── Traces/               # Playwright trace files
├── Logs/                 # Structured log files
├── CoverageReport/       # HTML coverage reports
├── *.trx                 # MSTest result files
├── *.html                # HTML test reports
└── *.json                # JSON reports for dashboards
```

### Report Integration
The framework generates reports compatible with:
- **Azure DevOps** - TRX and coverage reports
- **GitHub Actions** - Test summaries and artifacts
- **Power BI** - JSON exports for dashboard integration
- **Allure** - Rich HTML reports with history

## 🔍 Debugging & Troubleshooting

### Playwright Trace Viewer
View detailed execution traces for failed tests:

```bash
# Install Playwright CLI
dotnet tool install --global Microsoft.Playwright.CLI

# Open trace viewer
playwright show-trace TestResults/Traces/trace.zip
```

### Debug Mode
Run tests in debug mode with visible browser:

```bash
# Windows
.\run-tests.ps1 -Category "UI" -Headless:$false -Browser "chromium"

# macOS/Linux
./run-tests.sh --category "UI" --headed --browser "chromium"
```

### Logging Levels
Adjust logging verbosity in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.Playwright": "Information",
      "AzureMLWorkspace.Tests": "Debug"
    }
  }
}
```

## 🚀 CI/CD Integration

### GitHub Actions
The framework includes a comprehensive GitHub Actions workflow:

- **Multi-platform testing** (Windows, macOS, Linux)
- **Multi-browser support** (Chromium, Firefox, WebKit)
- **Parallel execution** with matrix strategy
- **Automatic retry** on transient failures
- **Rich reporting** with artifacts
- **Security scanning** with Trivy
- **Performance testing** on schedule

### Azure DevOps
Azure Pipelines configuration supports:

- **Cross-platform agents** (Windows, Linux, macOS)
- **Browser matrix testing**
- **Test result publishing**
- **Code coverage reports**
- **Security scanning** with WhiteSource
- **Deployment to test environments**

### Manual Triggers
Both pipelines support manual execution with parameters:

- **Test Category** - Choose specific test types
- **Browser** - Select browser for UI tests
- **Environment** - Target specific environments

## 🔧 Extensibility

### Adding New Abilities
Create custom abilities for new Azure services:

```csharp
public class UseAzureKeyVault : IAbility
{
    public string Name => "Use Azure Key Vault";
    
    public async Task InitializeAsync()
    {
        // Initialize Key Vault client
    }
    
    public async Task CleanupAsync()
    {
        // Cleanup resources
    }
}
```

### TypeScript Integration
For advanced Playwright features, create TypeScript utilities:

```typescript
// utils/advanced-playwright.ts
export async function captureNetworkTraffic(page: Page): Promise<NetworkLog[]> {
    const logs: NetworkLog[] = [];
    
    page.on('request', request => {
        logs.push({
            url: request.url(),
            method: request.method(),
            timestamp: Date.now()
        });
    });
    
    return logs;
}
```

### Python.NET Integration
For ML-specific validations:

```csharp
// Optional Python.NET integration
public class MLValidationHelper
{
    public static bool ValidateModelAccuracy(string modelPath, double threshold)
    {
        using (Py.GIL())
        {
            dynamic sklearn = Py.Import("sklearn.metrics");
            // Python ML validation logic
            return true;
        }
    }
}
```

## 📚 Best Practices

### Test Organization
- **Group related tests** in the same test class
- **Use descriptive test names** that explain the scenario
- **Implement proper setup and teardown** in test hooks
- **Use data-driven tests** for multiple scenarios

### Screenplay Pattern Guidelines
- **Keep tasks focused** - One task should do one thing
- **Make abilities stateless** - Avoid storing state in abilities
- **Use meaningful names** - Tasks and questions should read like English
- **Compose complex scenarios** - Combine simple tasks for complex workflows

### Performance Optimization
- **Use parallel execution** for independent tests
- **Implement proper waits** - Avoid Thread.Sleep
- **Reuse browser contexts** when possible
- **Clean up resources** in teardown methods

### Error Handling
- **Implement retry logic** for flaky operations
- **Capture artifacts** on failures
- **Use structured logging** for better debugging
- **Provide meaningful error messages**

## 🤝 Contributing

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Install dependencies: `dotnet restore`
4. Run tests: `dotnet test`
5. Submit a pull request

### Code Standards
- Follow C# coding conventions
- Write unit tests for new features
- Update documentation for changes
- Use meaningful commit messages

### Testing Guidelines
- Test on all supported platforms
- Verify cross-browser compatibility
- Include both positive and negative test cases
- Maintain test data independence

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

### Documentation
- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [Azure SDK for .NET](https://docs.microsoft.com/en-us/dotnet/azure/)

### Issues
For bug reports and feature requests, please use the [GitHub Issues](https://github.com/your-repo/issues) page.

### Community
Join our community discussions for questions and support.

---

**Built with ❤️ for modern test automation**