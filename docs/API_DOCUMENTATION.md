# Azure ML Workspace Testing Framework - API Documentation

## Table of Contents
1. [Core Interfaces](#core-interfaces)
2. [Actor API](#actor-api)
3. [Abilities API](#abilities-api)
4. [Tasks API](#tasks-api)
5. [Questions API](#questions-api)
6. [Step Definitions API](#step-definitions-api)
7. [Configuration API](#configuration-api)
8. [Utilities API](#utilities-api)

## Core Interfaces

### IActor Interface

The main interface for actors in the Screenplay pattern.

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
    Task<IActor> ShouldSee<T>(IQuestion<T> question) where T : IAssertion<T>;
    
    // Memory Management
    void Remember<T>(string key, T value);
    T Recall<T>(string key);
    bool Remembers(string key);
}
```

### IAbility Interface

Base interface for all actor abilities.

```csharp
public interface IAbility
{
    string Name { get; }
    Task InitializeAsync();
    Task CleanupAsync();
}
```

### ITask Interface

Interface for executable tasks.

```csharp
public interface ITask
{
    string Name { get; }
    Task PerformAs(IActor actor);
}
```

### IQuestion Interface

Interface for questions that return typed results.

```csharp
public interface IQuestion<T>
{
    string Question { get; }
    Task<T> AnsweredBy(IActor actor);
}
```

## Actor API

### Actor Class

The main implementation of the IActor interface.

#### Constructor

```csharp
public Actor(string name, ILogger<Actor> logger)
```

**Parameters:**
- `name`: The name of the actor
- `logger`: Logger instance for the actor

#### Static Factory Method

```csharp
public static Actor Named(string name, ILogger<Actor> logger)
```

Creates a new actor with the specified name.

#### Ability Management Methods

```csharp
public IActor Can<T>(T ability) where T : IAbility
```
Grants an ability to the actor.

**Parameters:**
- `ability`: The ability instance to grant

**Returns:** The actor instance for method chaining

**Example:**
```csharp
var actor = Actor.Named("DataScientist", logger)
    .Can(UseAzureML.AsContributor())
    .Can(BrowseTheWeb.Using(browser));
```

```csharp
public T Using<T>() where T : IAbility
```
Retrieves a specific ability from the actor.

**Returns:** The ability instance

**Throws:** `InvalidOperationException` if the ability is not available

**Example:**
```csharp
var azureML = actor.Using<UseAzureML>();
await azureML.StartCompute("my-compute");
```

```csharp
public bool HasAbility<T>() where T : IAbility
```
Checks if the actor has a specific ability.

**Returns:** `true` if the ability is available, `false` otherwise

#### Task Execution Methods

```csharp
public async Task<IActor> AttemptsTo(ITask task)
```
Executes a single task.

**Parameters:**
- `task`: The task to execute

**Returns:** The actor instance for method chaining

**Example:**
```csharp
await actor.AttemptsTo(OpenWorkspace.Named("ml-workspace"));
```

```csharp
public async Task<IActor> AttemptsTo(params ITask[] tasks)
```
Executes multiple tasks in sequence.

**Parameters:**
- `tasks`: Array of tasks to execute

**Returns:** The actor instance for method chaining

#### Question Methods

```csharp
public async Task<T> AsksFor<T>(IQuestion<T> question)
```
Asks a question and returns the result.

**Parameters:**
- `question`: The question to ask

**Returns:** The answer to the question

**Example:**
```csharp
var status = await actor.AsksFor(ComputeStatus.Of("my-compute"));
```

```csharp
public async Task<IActor> Should(IQuestion<bool> question)
```
Asserts that a boolean question returns true.

**Parameters:**
- `question`: The boolean question to evaluate

**Returns:** The actor instance for method chaining

**Throws:** `AssertionException` if the question returns false

#### Memory Methods

```csharp
public void Remember<T>(string key, T value)
```
Stores a value in the actor's memory.

**Parameters:**
- `key`: The key to store the value under
- `value`: The value to store

```csharp
public T Recall<T>(string key)
```
Retrieves a value from the actor's memory.

**Parameters:**
- `key`: The key to retrieve

**Returns:** The stored value

**Throws:** `KeyNotFoundException` if the key doesn't exist

```csharp
public bool Remembers(string key)
```
Checks if the actor remembers a specific key.

**Parameters:**
- `key`: The key to check

**Returns:** `true` if the key exists, `false` otherwise

## Abilities API

### UseAzureML Ability

Provides Azure Machine Learning service integration.

#### Properties

```csharp
public string Name { get; }
public ArmClient ArmClient { get; }
public MachineLearningWorkspaceResource Workspace { get; }
public TokenCredential Credential { get; }
```

#### Factory Methods

```csharp
public static UseAzureML WithRole(string role)
```
Creates an ability with the specified role.

```csharp
public static UseAzureML AsContributor()
```
Creates an ability with Contributor role.

```csharp
public static UseAzureML AsReader()
```
Creates an ability with Reader role.

```csharp
public static UseAzureML AsOwner()
```
Creates an ability with Owner role.

```csharp
public static UseAzureML WithCustomRole(string customRole)
```
Creates an ability with a custom role.

#### Core Methods

```csharp
public async Task InitializeAsync()
```
Initializes the Azure ML client and workspace connection.

```csharp
public async Task CleanupAsync()
```
Cleans up Azure ML resources.

```csharp
public async Task SetPIMRole(string roleName)
```
Activates a Privileged Identity Management role.

**Parameters:**
- `roleName`: The name of the PIM role to activate

```csharp
public async Task StartCompute(string computeName)
```
Starts a compute instance.

**Parameters:**
- `computeName`: The name of the compute instance

```csharp
public async Task StopCompute(string computeName)
```
Stops a compute instance.

**Parameters:**
- `computeName`: The name of the compute instance

```csharp
public async Task<string> GetComputeStatus(string computeName)
```
Gets the status of a compute instance.

**Parameters:**
- `computeName`: The name of the compute instance

**Returns:** The current status of the compute instance

### UseAzureAISearch Ability

Provides Azure AI Search service integration.

#### Factory Methods

```csharp
public static UseAzureAISearch WithDefaultConfiguration()
```
Creates an ability with default configuration.

```csharp
public static UseAzureAISearch WithConfiguration(string endpoint, string apiKey)
```
Creates an ability with custom configuration.

#### Core Methods

```csharp
public async Task<SearchResults<T>> SearchAsync<T>(string query, SearchOptions options = null)
```
Performs a search operation.

```csharp
public async Task<IndexDocumentsResult> IndexDocumentsAsync<T>(IEnumerable<T> documents)
```
Indexes documents in the search service.

### BrowseTheWeb Ability

Provides web browser automation capabilities using Playwright.

#### Factory Methods

```csharp
public static BrowseTheWeb Using(IBrowser browser)
```
Creates an ability using the specified browser.

```csharp
public static BrowseTheWeb UsingChromium()
```
Creates an ability using Chromium browser.

#### Core Methods

```csharp
public async Task<IPage> NewPageAsync()
```
Creates a new browser page.

```csharp
public async Task NavigateToAsync(string url)
```
Navigates to the specified URL.

## Tasks API

### OpenWorkspace Task

Opens an Azure ML workspace.

```csharp
public class OpenWorkspace : ITask
{
    public string Name { get; }
    
    public static OpenWorkspace Named(string workspaceName)
    public async Task PerformAs(IActor actor)
}
```

**Usage:**
```csharp
await actor.AttemptsTo(OpenWorkspace.Named("my-workspace"));
```

### StartCompute Task

Starts a compute instance.

```csharp
public class StartCompute : ITask
{
    public string Name { get; }
    
    public static StartCompute Named(string computeName)
    public async Task PerformAs(IActor actor)
}
```

**Usage:**
```csharp
await actor.AttemptsTo(StartCompute.Named("my-compute"));
```

### StopCompute Task

Stops a compute instance.

```csharp
public class StopCompute : ITask
{
    public string Name { get; }
    
    public static StopCompute Named(string computeName)
    public async Task PerformAs(IActor actor)
}
```

**Usage:**
```csharp
await actor.AttemptsTo(StopCompute.Named("my-compute"));
```

### NavigateTo Task

Navigates to a specific URL.

```csharp
public class NavigateTo : ITask
{
    public string Name { get; }
    
    public static NavigateTo Url(string url)
    public async Task PerformAs(IActor actor)
}
```

**Usage:**
```csharp
await actor.AttemptsTo(NavigateTo.Url("https://ml.azure.com"));
```

## Questions API

### Validate Class

Provides validation questions for system state.

```csharp
public static class Validate
{
    public static IQuestion<bool> ComputeStatus(string computeName, string expectedStatus)
    public static IQuestion<bool> WorkspaceIsAccessible(string workspaceName)
    public static IQuestion<bool> ElementIsVisible(string selector)
    public static IQuestion<bool> TextIsPresent(string text)
}
```

**Usage:**
```csharp
await actor.Should(Validate.ComputeStatus("my-compute", "Running"));
await actor.Should(Validate.WorkspaceIsAccessible("my-workspace"));
```

### ResultCount Class

Provides count-based questions.

```csharp
public static class ResultCount
{
    public static IQuestion<int> Of(string selector)
    public static IQuestion<int> OfSearchResults()
    public static IQuestion<int> OfComputeInstances()
}
```

**Usage:**
```csharp
var count = await actor.AsksFor(ResultCount.OfComputeInstances());
```

## Step Definitions API

### AzureMLWorkspaceSteps Class

Contains BDD step definitions for Azure ML workspace testing.

#### Given Steps

```csharp
[Given(@"I am a data scientist named ""(.*)""")]
public void GivenIAmADataScientistNamed(string name)

[Given(@"I have (.*) access to Azure ML")]
public async Task GivenIHaveAccessToAzureML(string role)

[Given(@"I have access to Azure AI Search")]
public async Task GivenIHaveAccessToAzureAISearch()

[Given(@"I have opened workspace ""(.*)""")]
public async Task GivenIHaveOpenedWorkspace(string workspaceName)

[Given(@"compute instance ""(.*)"" is running")]
public async Task GivenComputeInstanceIsRunning(string computeName)
```

#### When Steps

```csharp
[When(@"I attempt to open workspace ""(.*)""")]
public async Task WhenIAttemptToOpenWorkspace(string workspaceName)

[When(@"I start compute instance ""(.*)""")]
public async Task WhenIStartComputeInstance(string computeName)

[When(@"I stop compute instance ""(.*)""")]
public async Task WhenIStopComputeInstance(string computeName)

[When(@"I start compute instances:")]
public async Task WhenIStartComputeInstances(Table table)

[When(@"I stop all compute instances")]
public async Task WhenIStopAllComputeInstances()
```

#### Then Steps

```csharp
[Then(@"I should be able to access the workspace")]
public async Task ThenIShouldBeAbleToAccessTheWorkspace()

[Then(@"the workspace should be available")]
public async Task ThenTheWorkspaceShouldBeAvailable()

[Then(@"the compute instance should be running")]
public async Task ThenTheComputeInstanceShouldBeRunning()

[Then(@"I should be able to connect to it")]
public async Task ThenIShouldBeAbleToConnectToIt()

[Then(@"the compute instance should be stopped")]
public async Task ThenTheComputeInstanceShouldBeStopped()

[Then(@"all compute instances should be running")]
public async Task ThenAllComputeInstancesShouldBeRunning()

[Then(@"all compute instances should be stopped")]
public async Task ThenAllComputeInstancesShouldBeStopped()
```

#### Hooks

```csharp
[AfterScenario]
public async Task CleanupAfterScenario()
```
Performs cleanup after each scenario execution.

## Configuration API

### TestConfiguration Class

Manages test configuration settings.

```csharp
public class TestConfiguration
{
    public AzureConfiguration Azure { get; set; }
    public PlaywrightConfiguration Playwright { get; set; }
    public LoggingConfiguration Logging { get; set; }
    
    public static TestConfiguration Load()
    public static TestConfiguration LoadFromFile(string filePath)
}
```

### AzureConfiguration Class

```csharp
public class AzureConfiguration
{
    public string SubscriptionId { get; set; }
    public string ResourceGroup { get; set; }
    public string WorkspaceName { get; set; }
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
```

### PlaywrightConfiguration Class

```csharp
public class PlaywrightConfiguration
{
    public bool HeadlessMode { get; set; } = true;
    public string BrowserType { get; set; } = "chromium";
    public bool SlowMo { get; set; } = false;
    public int SlowMoDelay { get; set; } = 100;
    public int DefaultTimeout { get; set; } = 30000;
    public bool CaptureScreenshots { get; set; } = true;
    public bool CaptureVideos { get; set; } = false;
    public bool CaptureTraces { get; set; } = true;
}
```

## Utilities API

### RetryHelper Class

Provides retry logic for operations.

```csharp
public static class RetryHelper
{
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts = 3,
        TimeSpan delay = default,
        Func<Exception, bool> retryCondition = null)
        
    public static async Task RetryAsync(
        Func<Task> operation,
        int maxAttempts = 3,
        TimeSpan delay = default,
        Func<Exception, bool> retryCondition = null)
}
```

**Usage:**
```csharp
var result = await RetryHelper.RetryAsync(
    async () => await someOperation(),
    maxAttempts: 5,
    delay: TimeSpan.FromSeconds(2));
```

### TestDataGenerator Class

Generates test data for testing scenarios.

```csharp
public static class TestDataGenerator
{
    public static string GenerateComputeName()
    public static string GenerateWorkspaceName()
    public static string GenerateResourceGroupName()
    public static AzureConfiguration GenerateAzureConfiguration()
}
```

**Usage:**
```csharp
var computeName = TestDataGenerator.GenerateComputeName();
var config = TestDataGenerator.GenerateAzureConfiguration();
```

## Exception Types

### Custom Exceptions

```csharp
public class AssertionException : Exception
public class AbilityNotFoundException : Exception
public class TaskExecutionException : Exception
public class QuestionAnsweringException : Exception
public class ConfigurationException : Exception
```

## Extension Methods

### ScreenplayExtensions Class

Provides extension methods for enhanced fluent API.

```csharp
public static class ScreenplayExtensions
{
    public static async Task<IActor> WaitsFor(this IActor actor, TimeSpan duration)
    public static async Task<IActor> WaitsUntil(this IActor actor, IQuestion<bool> condition, TimeSpan timeout = default)
    public static async Task<IActor> Eventually(this IActor actor, ITask task, TimeSpan timeout = default)
}
```

**Usage:**
```csharp
await actor
    .AttemptsTo(StartCompute.Named("my-compute"))
    .WaitsUntil(Validate.ComputeStatus("my-compute", "Running"))
    .Eventually(ConnectToCompute.Named("my-compute"));
```

## Error Handling

### Common Error Scenarios

1. **Authentication Failures**
   - Invalid credentials
   - Expired tokens
   - Insufficient permissions

2. **Resource Not Found**
   - Workspace doesn't exist
   - Compute instance not found
   - Resource group not accessible

3. **Timeout Errors**
   - Operation timeout
   - Network timeout
   - Service unavailable

4. **Configuration Errors**
   - Missing configuration values
   - Invalid configuration format
   - Environment variable not set

### Error Response Format

All exceptions include structured error information:

```csharp
public class FrameworkException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, object> Context { get; }
    public DateTime Timestamp { get; }
    public string ActorName { get; }
    public string Operation { get; }
}
```

## Performance Considerations

### Async/Await Best Practices
- All operations are asynchronous
- Proper ConfigureAwait usage
- Cancellation token support

### Resource Management
- Automatic disposal of resources
- Connection pooling
- Memory optimization

### Parallel Execution
- Thread-safe implementations
- Concurrent collections
- Isolation between test runs