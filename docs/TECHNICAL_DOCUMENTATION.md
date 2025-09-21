# Azure ML Workspace Testing Framework - Technical Documentation

## Overview

The Azure ML Workspace Testing Framework is a comprehensive C# test automation solution built using the Screenplay pattern, Reqnroll (BDD), and Playwright for browser automation. It provides a robust foundation for testing Azure Machine Learning workspaces, compute instances, and AI services.

## Architecture

### Core Design Patterns

#### 1. Screenplay Pattern
The framework implements the Screenplay pattern, which provides a more maintainable and readable approach to test automation compared to traditional Page Object Model.

**Key Components:**
- **Actors**: Represent users performing actions (e.g., data scientists, administrators)
- **Abilities**: Define what an actor can do (e.g., use Azure ML, browse the web)
- **Tasks**: High-level business actions (e.g., start compute instance, open workspace)
- **Questions**: Queries about the system state (e.g., is compute running?)

#### 2. Behavior-Driven Development (BDD)
Uses Reqnroll (formerly SpecFlow) for BDD implementation, allowing tests to be written in natural language using Gherkin syntax.

#### 3. Dependency Injection
Leverages Microsoft.Extensions.DependencyInjection for loose coupling and testability.

### Framework Structure

```
AzureMLWorkspace.Tests/
├── Framework/                    # Core framework components
│   ├── Screenplay/              # Screenplay pattern implementation
│   │   ├── Actor.cs            # Main actor implementation
│   │   ├── IActor.cs           # Actor interface
│   │   ├── IAbility.cs         # Ability interface
│   │   ├── ITask.cs            # Task interface
│   │   └── IQuestion.cs        # Question interface
│   ├── Abilities/              # Actor abilities
│   │   ├── UseAzureML.cs       # Azure ML service interaction
│   │   ├── UseAzureAISearch.cs # Azure AI Search interaction
│   │   └── BrowseTheWeb.cs     # Web browser interaction
│   ├── Tasks/                  # High-level business tasks
│   │   ├── OpenWorkspace.cs    # Open ML workspace
│   │   ├── StartCompute.cs     # Start compute instance
│   │   └── StopCompute.cs      # Stop compute instance
│   ├── Questions/              # System state queries
│   │   ├── Validate.cs         # Validation questions
│   │   └── ResultCount.cs      # Count-based questions
│   └── Utilities/              # Helper utilities
│       ├── RetryHelper.cs      # Retry logic
│       └── TestDataGenerator.cs # Test data generation
├── StepDefinitions/            # BDD step definitions
├── Features/                   # Gherkin feature files
├── Configuration/              # Configuration management
├── Tests/                      # Traditional unit/integration tests
└── TestData/                   # Test data files
```

## Core Components

### 1. Actor Implementation

The `Actor` class is the central component that orchestrates test execution:

```csharp
public class Actor : IActor
{
    private readonly ConcurrentDictionary<Type, IAbility> _abilities = new();
    private readonly ConcurrentDictionary<string, object> _memory = new();
    private readonly ILogger<Actor> _logger;

    public string Name { get; }

    // Key methods:
    public IActor Can<T>(T ability) where T : IAbility
    public async Task<IActor> AttemptsTo(ITask task)
    public async Task<T> AsksFor<T>(IQuestion<T> question)
    public async Task<IActor> Should(IQuestion<bool> question)
    public void Remember<T>(string key, T value)
    public T Recall<T>(string key)
}
```

**Key Features:**
- Thread-safe ability and memory management
- Comprehensive logging
- Fluent API for readable test code
- Automatic cleanup and disposal

### 2. Abilities

#### UseAzureML Ability
Provides Azure Machine Learning service integration:

```csharp
public class UseAzureML : IAbility
{
    public ArmClient ArmClient { get; }
    public MachineLearningWorkspaceResource Workspace { get; }
    public TokenCredential Credential { get; }

    // Factory methods:
    public static UseAzureML WithRole(string role)
    public static UseAzureML AsContributor()
    public static UseAzureML AsReader()
    public static UseAzureML AsOwner()

    // Core operations:
    public async Task StartCompute(string computeName)
    public async Task StopCompute(string computeName)
    public async Task<string> GetComputeStatus(string computeName)
}
```

**Features:**
- Role-based access control
- Azure Resource Manager integration
- Compute instance management
- PIM (Privileged Identity Management) support
- Comprehensive error handling and logging

#### UseAzureAISearch Ability
Provides Azure AI Search service integration for document processing and search operations.

#### BrowseTheWeb Ability
Provides Playwright-based web browser automation capabilities.

### 3. Tasks

Tasks represent high-level business operations:

```csharp
public class StartCompute : ITask
{
    public string Name => $"Start compute instance '{_computeName}'";
    
    public async Task PerformAs(IActor actor)
    {
        var azureML = actor.Using<UseAzureML>();
        await azureML.StartCompute(_computeName);
    }
}
```

### 4. Questions

Questions query the system state and return results:

```csharp
public class Validate
{
    public static IQuestion<bool> ComputeStatus(string computeName, string expectedStatus)
    {
        return new ComputeStatusQuestion(computeName, expectedStatus);
    }
}
```

## Configuration Management

### Configuration Sources
The framework supports multiple configuration sources in order of precedence:
1. Environment variables
2. appsettings.test.json (for test-specific overrides)
3. appsettings.json (default configuration)

### Key Configuration Sections

```json
{
  "Azure": {
    "SubscriptionId": "your-subscription-id",
    "ResourceGroup": "your-resource-group",
    "WorkspaceName": "your-workspace-name",
    "TenantId": "your-tenant-id"
  },
  "Playwright": {
    "HeadlessMode": true,
    "BrowserType": "chromium",
    "SlowMo": false,
    "SlowMoDelay": 100,
    "DefaultTimeout": 30000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}
```

## Authentication and Security

### Azure Authentication
The framework uses `DefaultAzureCredential` which supports multiple authentication methods:
- Environment variables
- Managed Identity
- Visual Studio/VS Code
- Azure CLI
- Azure PowerShell
- Interactive browser

### Role-Based Testing
Supports testing with different Azure roles:
- **Owner**: Full access to all resources
- **Contributor**: Can create and manage resources
- **Reader**: Read-only access
- **Custom roles**: Support for custom RBAC roles

### PIM Integration
Planned support for Privileged Identity Management (PIM) role activation for testing elevated permissions.

## Logging and Monitoring

### Logging Framework
Uses Serilog with multiple sinks:
- Console output with structured logging
- File-based logging with daily rotation
- Structured JSON logging for analysis

### Log Levels
- **Trace**: Detailed execution flow
- **Debug**: Development and troubleshooting information
- **Information**: General application flow
- **Warning**: Unexpected but recoverable conditions
- **Error**: Error conditions that don't stop execution
- **Critical**: Critical errors that may cause termination

### Performance Monitoring
- Task execution timing
- Azure API call latency
- Resource utilization tracking

## Error Handling and Resilience

### Retry Mechanisms
Built-in retry logic for:
- Azure API calls
- Network operations
- Transient failures

### Exception Handling
Comprehensive exception handling with:
- Detailed error logging
- Context preservation
- Graceful degradation
- Resource cleanup

### Circuit Breaker Pattern
Implemented for external service calls to prevent cascade failures.

## Testing Strategies

### Test Categories
1. **Unit Tests**: Individual component testing
2. **Integration Tests**: Service integration testing
3. **End-to-End Tests**: Complete workflow testing
4. **Performance Tests**: Load and stress testing
5. **Security Tests**: Authentication and authorization testing

### Test Data Management
- Configurable test data through JSON files
- Dynamic test data generation using Bogus
- Test data isolation and cleanup

### Parallel Execution
- Thread-safe actor implementation
- Parallel test collection execution
- Resource isolation between tests

## Extensibility

### Adding New Abilities
1. Implement `IAbility` interface
2. Add initialization and cleanup logic
3. Register with dependency injection
4. Create factory methods

### Adding New Tasks
1. Implement `ITask` interface
2. Define business logic in `PerformAs` method
3. Add appropriate logging
4. Handle exceptions gracefully

### Adding New Questions
1. Implement `IQuestion<T>` interface
2. Define query logic in `AnsweredBy` method
3. Return appropriate result type
4. Add validation logic

## Performance Considerations

### Resource Management
- Proper disposal of Azure clients
- Connection pooling for HTTP clients
- Memory management for large datasets

### Optimization Strategies
- Lazy loading of expensive resources
- Caching of frequently accessed data
- Asynchronous operations throughout

### Scalability
- Horizontal scaling through parallel execution
- Vertical scaling through resource optimization
- Cloud-native deployment support

## Troubleshooting

### Common Issues
1. **Authentication Failures**: Check Azure credentials and permissions
2. **Timeout Issues**: Adjust timeout configurations
3. **Resource Conflicts**: Ensure proper test isolation
4. **Network Issues**: Implement retry logic and circuit breakers

### Debugging Tools
- Comprehensive logging
- Playwright trace recording
- Screenshot capture on failures
- Performance profiling

### Monitoring and Alerting
- Test execution metrics
- Failure rate monitoring
- Performance degradation alerts
- Resource utilization tracking

## Best Practices

### Code Organization
- Follow SOLID principles
- Use dependency injection
- Implement proper separation of concerns
- Maintain consistent naming conventions

### Test Design
- Write readable and maintainable tests
- Use descriptive test names
- Implement proper test data management
- Ensure test independence

### Error Handling
- Implement comprehensive exception handling
- Use structured logging
- Provide meaningful error messages
- Implement proper cleanup logic

### Performance
- Use asynchronous operations
- Implement proper resource disposal
- Optimize for parallel execution
- Monitor and profile regularly

## Advanced Integrations

### TypeScript Integration
The framework supports TypeScript integration for advanced Playwright features and custom browser automation utilities. TypeScript integration enables:

- **Advanced Browser Capabilities**: Enhanced Playwright features with type safety
- **Custom Utilities**: Type-safe helper functions and utilities
- **Performance Monitoring**: Advanced browser performance metrics
- **Network Traffic Analysis**: Detailed request/response logging
- **Visual Testing**: Screenshot comparison and visual regression testing

For detailed setup and usage instructions, see: [TypeScript Integration Guide](TYPESCRIPT_INTEGRATION.md)

### Python.NET Integration
Python.NET integration allows leveraging Python's rich ML ecosystem within C# test automation:

- **ML Model Validation**: Automated model performance testing
- **Data Quality Assessment**: Comprehensive data analysis and validation
- **Statistical Analysis**: Advanced statistical computations
- **Azure ML Python SDK**: Direct integration with Azure ML services
- **Custom Analytics**: Python-based data processing and analysis

For detailed setup and usage instructions, see: [Python.NET Integration Guide](PYTHON_NET_INTEGRATION.md)

## Future Enhancements

### Planned Features
1. **Enhanced PIM Integration**: Full PIM role activation support
2. **Advanced Reporting**: Rich HTML reports with screenshots and traces
3. **CI/CD Integration**: Enhanced pipeline integration
4. **Mobile Testing**: Support for mobile browser testing
5. **API Testing**: Direct Azure REST API testing capabilities
6. **Performance Testing**: Load testing with NBomber integration
7. **Visual Testing**: Screenshot comparison testing
8. **Database Testing**: Azure SQL and Cosmos DB testing support

### Technology Roadmap
- Migration to .NET 9
- Enhanced Azure SDK integration
- Improved Playwright features
- Advanced monitoring and observability
- Cloud-native deployment options