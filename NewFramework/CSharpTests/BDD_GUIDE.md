# BDD Testing Guide with Reqnroll

Complete guide to writing and running BDD tests using Reqnroll (SpecFlow successor) in the C# Playwright framework.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Writing Feature Files](#writing-feature-files)
- [Step Definitions](#step-definitions)
- [Hooks](#hooks)
- [Data Tables](#data-tables)
- [Scenario Outline](#scenario-outline)
- [Tags](#tags)
- [Context Injection](#context-injection)
- [Best Practices](#best-practices)
- [Examples](#examples)

## Overview

Reqnroll is a BDD (Behavior-Driven Development) framework for .NET that allows you to write tests in Gherkin syntax (Given-When-Then).

### Benefits

- **Readable**: Tests are written in plain English
- **Collaborative**: Business stakeholders can understand tests
- **Reusable**: Step definitions can be reused across scenarios
- **Living Documentation**: Feature files serve as documentation

## Getting Started

### Project Structure

```
CSharpTests/
├── Features/                    # Gherkin feature files
│   ├── AzureMLWorkspace.feature
│   └── AzureAISearch.feature
├── StepDefinitions/            # Step implementations
│   ├── AzureMLWorkspaceSteps.cs
│   └── AzureAISearchSteps.cs
└── Hooks/                      # Test lifecycle hooks
    └── TestHooks.cs
```

### Running BDD Tests

```bash
# Run all BDD tests
dotnet test

# Run specific feature
dotnet test --filter "FullyQualifiedName~AzureMLWorkspace"

# Run by tag
dotnet test --filter "Category=Smoke"
```

## Writing Feature Files

### Basic Structure

```gherkin
Feature: Feature Name
  Brief description of the feature
  
  Background:
    Given common preconditions
    
  Scenario: Scenario Name
    Given initial context
    When action is performed
    Then expected outcome
```

### Example

```gherkin
Feature: Azure ML Workspace Management
  As a data scientist
  I want to manage Azure ML workspaces
  So that I can organize my machine learning projects

  Background:
    Given I have valid Azure credentials
    And I am authenticated with Azure ML

  Scenario: Create a new workspace
    Given I have a unique workspace name "test-workspace"
    When I create the workspace in resource group "test-rg"
    Then the workspace should be created successfully
    And the workspace should be accessible
```

### Gherkin Keywords

- **Feature**: High-level description of a feature
- **Background**: Common steps for all scenarios
- **Scenario**: Individual test case
- **Given**: Initial context/preconditions
- **When**: Action/event
- **Then**: Expected outcome/assertion
- **And**: Additional steps
- **But**: Negative assertion

## Step Definitions

### Creating Step Definitions

Step definitions are C# methods that implement Gherkin steps.

```csharp
using Reqnroll;
using NUnit.Framework;
using FluentAssertions;

namespace PlaywrightFramework.StepDefinitions
{
    [Binding]
    public class AzureMLWorkspaceSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private string _workspaceName;
        
        public AzureMLWorkspaceSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }
        
        [Given(@"I have a unique workspace name ""(.*)""")]
        public void GivenIHaveAUniqueWorkspaceName(string workspaceName)
        {
            _workspaceName = workspaceName;
        }
        
        [When(@"I create the workspace in resource group ""(.*)""")]
        public async Task WhenICreateTheWorkspaceInResourceGroup(string resourceGroup)
        {
            // Implementation
            var result = await CreateWorkspace(_workspaceName, resourceGroup);
            _scenarioContext["WorkspaceResult"] = result;
        }
        
        [Then(@"the workspace should be created successfully")]
        public void ThenTheWorkspaceShouldBeCreatedSuccessfully()
        {
            var result = (bool)_scenarioContext["WorkspaceResult"];
            result.Should().BeTrue();
        }
    }
}
```

### Step Patterns

#### Simple String Match

```csharp
[Given(@"I am on the home page")]
public void GivenIAmOnTheHomePage()
{
    // Implementation
}
```

#### String Parameter

```csharp
[Given(@"I have a workspace named ""(.*)""")]
public void GivenIHaveAWorkspaceNamed(string workspaceName)
{
    // Implementation
}
```

#### Integer Parameter

```csharp
[Given(@"I have (\d+) compute instances")]
public void GivenIHaveComputeInstances(int count)
{
    // Implementation
}
```

#### Multiple Parameters

```csharp
[When(@"I create a compute instance ""(.*)"" with VM size ""(.*)""")]
public void WhenICreateAComputeInstance(string name, string vmSize)
{
    // Implementation
}
```

#### Async Steps

```csharp
[When(@"I start the compute instance")]
public async Task WhenIStartTheComputeInstance()
{
    await StartComputeInstance();
}
```

## Hooks

Hooks allow you to run code at specific points in the test lifecycle.

### Hook Types

```csharp
using Reqnroll;

namespace PlaywrightFramework.Hooks
{
    [Binding]
    public class TestHooks
    {
        // Before entire test run
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Setup code
        }
        
        // After entire test run
        [AfterTestRun]
        public static void AfterTestRun()
        {
            // Cleanup code
        }
        
        // Before each feature
        [BeforeFeature]
        public static void BeforeFeature()
        {
            // Feature setup
        }
        
        // After each feature
        [AfterFeature]
        public static void AfterFeature()
        {
            // Feature cleanup
        }
        
        // Before each scenario
        [BeforeScenario]
        public void BeforeScenario()
        {
            // Scenario setup
        }
        
        // After each scenario
        [AfterScenario]
        public void AfterScenario()
        {
            // Scenario cleanup
        }
        
        // Before each step
        [BeforeStep]
        public void BeforeStep()
        {
            // Step setup
        }
        
        // After each step
        [AfterStep]
        public void AfterStep()
        {
            // Step cleanup
        }
    }
}
```

### Tagged Hooks

Run hooks only for specific tags:

```csharp
[BeforeScenario("ui")]
public void BeforeUIScenario()
{
    // Setup for UI tests only
}

[AfterScenario("database")]
public void AfterDatabaseScenario()
{
    // Cleanup for database tests
}
```

### Hook Order

```csharp
[BeforeScenario(Order = 1)]
public void FirstHook()
{
    // Runs first
}

[BeforeScenario(Order = 2)]
public void SecondHook()
{
    // Runs second
}
```

## Data Tables

### Simple Data Table

Feature file:

```gherkin
Scenario: Create multiple workspaces
  Given I have the following workspaces:
    | Name           | ResourceGroup | Region  |
    | workspace-1    | rg-1          | eastus  |
    | workspace-2    | rg-2          | westus  |
    | workspace-3    | rg-3          | northeu |
  When I create all workspaces
  Then all workspaces should be created successfully
```

Step definition:

```csharp
[Given(@"I have the following workspaces:")]
public void GivenIHaveTheFollowingWorkspaces(Table table)
{
    foreach (var row in table.Rows)
    {
        var name = row["Name"];
        var resourceGroup = row["ResourceGroup"];
        var region = row["Region"];
        
        // Store or process data
    }
}
```

### Vertical Data Table

Feature file:

```gherkin
Scenario: Create workspace with configuration
  Given I have a workspace with the following configuration:
    | Property      | Value         |
    | Name          | test-ws       |
    | ResourceGroup | test-rg       |
    | Region        | eastus        |
    | SKU           | Basic         |
  When I create the workspace
  Then the workspace should be created successfully
```

Step definition:

```csharp
[Given(@"I have a workspace with the following configuration:")]
public void GivenIHaveAWorkspaceWithConfiguration(Table table)
{
    var config = table.CreateInstance<WorkspaceConfig>();
    // Or manually:
    var config = new WorkspaceConfig
    {
        Name = table.Rows[0]["Value"],
        ResourceGroup = table.Rows[1]["Value"],
        Region = table.Rows[2]["Value"],
        SKU = table.Rows[3]["Value"]
    };
}
```

## Scenario Outline

Run the same scenario with different data sets.

### Example

```gherkin
Scenario Outline: Create compute instances with different VM sizes
  Given I am authenticated with Azure ML
  When I create a compute instance "<InstanceName>" with VM size "<VMSize>"
  Then the compute instance should be created successfully
  And the instance should have VM size "<VMSize>"

  Examples:
    | InstanceName  | VMSize           |
    | small-vm      | Standard_DS1_v2  |
    | medium-vm     | Standard_DS2_v2  |
    | large-vm      | Standard_DS3_v2  |
```

### Multiple Example Sets

```gherkin
Scenario Outline: Test different environments
  Given I am in "<Environment>" environment
  When I perform action "<Action>"
  Then I should see result "<Result>"

  Examples: Development
    | Environment | Action | Result  |
    | dev         | create | success |
    | dev         | update | success |

  Examples: Production
    | Environment | Action | Result  |
    | prod        | create | success |
    | prod        | update | success |
```

## Tags

Tags allow you to organize and filter scenarios.

### Using Tags

```gherkin
@smoke @critical
Feature: Critical Smoke Tests

  @ui @slow
  Scenario: UI test that takes time
    Given I am on the home page
    When I perform complex actions
    Then I should see results

  @api @fast
  Scenario: Fast API test
    Given I have API credentials
    When I call the API
    Then I should get a response
```

### Running Tagged Tests

```bash
# Run smoke tests
dotnet test --filter "Category=smoke"

# Run UI tests
dotnet test --filter "Category=ui"

# Exclude slow tests
dotnet test --filter "Category!=slow"

# Multiple tags
dotnet test --filter "Category=smoke|Category=critical"
```

### Common Tags

- `@smoke` - Smoke tests
- `@integration` - Integration tests
- `@e2e` - End-to-end tests
- `@ui` - UI tests
- `@api` - API tests
- `@slow` - Slow tests
- `@fast` - Fast tests
- `@critical` - Critical tests
- `@ignore` - Ignored tests
- `@wip` - Work in progress
- `@serial` - Non-parallelizable tests

### Ignoring Scenarios

```gherkin
@ignore
Scenario: Test under development
  Given this test is not ready
  When I run it
  Then it should be skipped
```

## Context Injection

Share data between step definitions using ScenarioContext.

### ScenarioContext

```csharp
public class StepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    
    public StepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }
    
    [When(@"I create a workspace")]
    public void WhenICreateAWorkspace()
    {
        var workspace = CreateWorkspace();
        _scenarioContext["Workspace"] = workspace;
    }
    
    [Then(@"the workspace should be accessible")]
    public void ThenTheWorkspaceShouldBeAccessible()
    {
        var workspace = (Workspace)_scenarioContext["Workspace"];
        // Assertions
    }
}
```

### Dependency Injection

Share objects between step definition classes:

```csharp
public class SharedContext
{
    public string WorkspaceName { get; set; }
    public IPlaywright Playwright { get; set; }
    public IBrowser Browser { get; set; }
}

[Binding]
public class StepDefinitions1
{
    private readonly SharedContext _context;
    
    public StepDefinitions1(SharedContext context)
    {
        _context = context;
    }
    
    [Given(@"I have a workspace")]
    public void GivenIHaveAWorkspace()
    {
        _context.WorkspaceName = "test-workspace";
    }
}

[Binding]
public class StepDefinitions2
{
    private readonly SharedContext _context;
    
    public StepDefinitions2(SharedContext context)
    {
        _context = context;
    }
    
    [When(@"I access the workspace")]
    public void WhenIAccessTheWorkspace()
    {
        var name = _context.WorkspaceName; // Shared data
    }
}
```

## Best Practices

### 1. Write Declarative Steps

**Good**:
```gherkin
Given I am logged in as an admin
When I create a new workspace
Then the workspace should be created
```

**Bad**:
```gherkin
Given I navigate to the login page
And I enter username "admin"
And I enter password "password"
And I click the login button
And I wait for the dashboard to load
And I click on workspaces
And I click on create new
```

### 2. Keep Steps Reusable

```csharp
// Reusable step
[Given(@"I am authenticated with Azure ML")]
public async Task GivenIAmAuthenticatedWithAzureML()
{
    await AuthenticateWithAzureML();
}

// Use in multiple scenarios
```

### 3. Use Background for Common Steps

```gherkin
Feature: Azure ML Workspace

  Background:
    Given I have valid Azure credentials
    And I am authenticated with Azure ML

  Scenario: Create workspace
    # No need to repeat authentication steps
    When I create a workspace
    Then it should succeed

  Scenario: Delete workspace
    # Authentication already done in Background
    When I delete a workspace
    Then it should succeed
```

### 4. One Assertion Per Then

**Good**:
```gherkin
Then the workspace should be created
And the workspace should be accessible
And the workspace should have correct configuration
```

**Bad**:
```gherkin
Then the workspace should be created and accessible with correct configuration
```

### 5. Use Scenario Outline for Similar Tests

Instead of:
```gherkin
Scenario: Test VM size DS1
  When I create instance with "Standard_DS1_v2"
  Then it should succeed

Scenario: Test VM size DS2
  When I create instance with "Standard_DS2_v2"
  Then it should succeed
```

Use:
```gherkin
Scenario Outline: Test different VM sizes
  When I create instance with "<VMSize>"
  Then it should succeed

  Examples:
    | VMSize           |
    | Standard_DS1_v2  |
    | Standard_DS2_v2  |
```

### 6. Tag Appropriately

```gherkin
@smoke @critical @fast
Scenario: Critical smoke test

@integration @slow @database
Scenario: Database integration test
```

### 7. Use Meaningful Names

**Good**:
```gherkin
Scenario: Admin can create workspace in any resource group
```

**Bad**:
```gherkin
Scenario: Test 1
```

## Examples

### Complete Feature File

```gherkin
@azure @ml @workspace
Feature: Azure ML Workspace Management
  As a data scientist
  I want to manage Azure ML workspaces
  So that I can organize my machine learning projects

  Background:
    Given I have valid Azure credentials
    And I am authenticated with Azure ML

  @smoke @critical
  Scenario: Create a new workspace
    Given I have a unique workspace name "test-workspace"
    When I create the workspace in resource group "test-rg"
    Then the workspace should be created successfully
    And the workspace should be accessible
    And the workspace should be in "eastus" region

  @integration
  Scenario: Create workspace with custom configuration
    Given I have a workspace with the following configuration:
      | Property      | Value         |
      | Name          | custom-ws     |
      | ResourceGroup | custom-rg     |
      | Region        | westus        |
      | SKU           | Premium       |
    When I create the workspace
    Then the workspace should be created successfully
    And the workspace should have SKU "Premium"

  @smoke
  Scenario Outline: Create workspaces in different regions
    Given I have a unique workspace name "<WorkspaceName>"
    When I create the workspace in region "<Region>"
    Then the workspace should be created successfully
    And the workspace should be in "<Region>" region

    Examples:
      | WorkspaceName | Region    |
      | ws-east       | eastus    |
      | ws-west       | westus    |
      | ws-europe     | northeu   |

  @cleanup
  Scenario: Delete a workspace
    Given I have an existing workspace "test-workspace"
    When I delete the workspace
    Then the workspace should be deleted successfully
    And the workspace should not be accessible
```

### Complete Step Definition

```csharp
using Reqnroll;
using NUnit.Framework;
using FluentAssertions;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.StepDefinitions
{
    [Binding]
    public class AzureMLWorkspaceSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly AzureMLUtils _azureMLUtils;
        private string _workspaceName;
        private string _resourceGroup;
        private WorkspaceConfig _config;
        
        public AzureMLWorkspaceSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _azureMLUtils = new AzureMLUtils();
        }
        
        [Given(@"I have valid Azure credentials")]
        public void GivenIHaveValidAzureCredentials()
        {
            // Assume credentials are configured
        }
        
        [Given(@"I am authenticated with Azure ML")]
        public async Task GivenIAmAuthenticatedWithAzureML()
        {
            var authenticated = await _azureMLUtils.AuthenticateAsync();
            authenticated.Should().BeTrue();
        }
        
        [Given(@"I have a unique workspace name ""(.*)""")]
        public void GivenIHaveAUniqueWorkspaceName(string workspaceName)
        {
            _workspaceName = $"{workspaceName}-{DateTime.Now:yyyyMMddHHmmss}";
        }
        
        [Given(@"I have a workspace with the following configuration:")]
        public void GivenIHaveAWorkspaceWithConfiguration(Table table)
        {
            _config = new WorkspaceConfig
            {
                Name = table.Rows[0]["Value"],
                ResourceGroup = table.Rows[1]["Value"],
                Region = table.Rows[2]["Value"],
                SKU = table.Rows[3]["Value"]
            };
        }
        
        [When(@"I create the workspace in resource group ""(.*)""")]
        public async Task WhenICreateTheWorkspaceInResourceGroup(string resourceGroup)
        {
            _resourceGroup = resourceGroup;
            var result = await _azureMLUtils.CreateWorkspaceAsync(_workspaceName, _resourceGroup);
            _scenarioContext["WorkspaceResult"] = result;
        }
        
        [When(@"I create the workspace")]
        public async Task WhenICreateTheWorkspace()
        {
            var result = await _azureMLUtils.CreateWorkspaceAsync(_config);
            _scenarioContext["WorkspaceResult"] = result;
        }
        
        [Then(@"the workspace should be created successfully")]
        public void ThenTheWorkspaceShouldBeCreatedSuccessfully()
        {
            var result = (bool)_scenarioContext["WorkspaceResult"];
            result.Should().BeTrue();
        }
        
        [Then(@"the workspace should be accessible")]
        public async Task ThenTheWorkspaceShouldBeAccessible()
        {
            var accessible = await _azureMLUtils.IsWorkspaceAccessibleAsync(_workspaceName);
            accessible.Should().BeTrue();
        }
    }
}
```

## Additional Resources

- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [Gherkin Reference](https://cucumber.io/docs/gherkin/reference/)
- [BDD Best Practices](https://cucumber.io/docs/bdd/)
- [Reqnroll GitHub](https://github.com/reqnroll/Reqnroll)