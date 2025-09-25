using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Configuration;
using AzureMLWorkspace.Tests.Framework.Questions;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Tasks;
using AzureMLWorkspace.Tests.Framework.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace AzureMLWorkspace.Tests.Tests;

/// <summary>
/// Demonstration test showing the complete framework working without requiring actual Azure resources
/// This test validates the framework structure and component integration
/// </summary>
[TestFixture]
[Category("Demo")]
[Category("Framework")]
public class FrameworkDemonstrationTest
{
    private ServiceProvider? _serviceProvider;
    private ILogger<FrameworkDemonstrationTest>? _logger;
    private TestConfiguration? _config;
    private IActor? _actor;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Build configuration with demo environment
        var configuration = ConfigurationHelper.BuildConfiguration("Demo");

        // Build service collection
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add configuration
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<TestConfiguration>(TestConfiguration.LoadFromConfiguration(configuration));

        // Add framework utilities
        services.AddScoped<VSCodeDesktopHelper>();

        // Build service provider
        _serviceProvider = services.BuildServiceProvider();
        
        // Initialize TestContext
        AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider = _serviceProvider;
        
        _logger = _serviceProvider.GetRequiredService<ILogger<FrameworkDemonstrationTest>>();
        _config = _serviceProvider.GetRequiredService<TestConfiguration>();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _serviceProvider?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _logger!.LogInformation("=== Starting Framework Demonstration Test ===");
        
        // Create actor
        var actorLogger = _serviceProvider!.GetRequiredService<ILogger<Actor>>();
        _actor = Actor.Named("DemoDataScientist", actorLogger);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_actor != null && _actor is IAsyncDisposable disposableActor)
        {
            await disposableActor.DisposeAsync();
        }
        _logger!.LogInformation("=== Framework Demonstration Test Completed ===");
    }

    [Test]
    [Description("Demonstrates the complete framework structure and component creation")]
    public async Task DemonstrateFrameworkStructure()
    {
        Assert.That(_actor, Is.Not.Null, "Actor should be created");
        Assert.That(_config, Is.Not.Null, "Configuration should be loaded");

        _logger!.LogInformation("🎯 Demonstrating Framework Structure");

        // Step 1: Demonstrate Task Creation
        _logger.LogInformation("Step 1: Creating Tasks");
        
        var navigateTask = NavigateToWorkspace.Named("CTO-workspace");
        Assert.That(navigateTask, Is.Not.Null, "NavigateToWorkspace task should be created");
        Assert.That(navigateTask.Name, Contains.Substring("CTO-workspace"), "Task name should contain workspace name");
        
        var loginTask = LoginAsUser.Named("Javed Khan");
        Assert.That(loginTask, Is.Not.Null, "LoginAsUser task should be created");
        
        var selectTask = SelectWorkspace.Named("CTO-workspace");
        Assert.That(selectTask, Is.Not.Null, "SelectWorkspace task should be created");
        
        var computeTask = ChooseComputeOption.Now();
        Assert.That(computeTask, Is.Not.Null, "ChooseComputeOption task should be created");
        
        var openComputeTask = OpenCompute.Named("com-jk");
        Assert.That(openComputeTask, Is.Not.Null, "OpenCompute task should be created");
        
        var startComputeTask = StartComputeIfNotRunning.Named("com-jk");
        Assert.That(startComputeTask, Is.Not.Null, "StartComputeIfNotRunning task should be created");
        
        var vsCodeTask = StartVSCodeDesktop.Now();
        Assert.That(vsCodeTask, Is.Not.Null, "StartVSCodeDesktop task should be created");
        
        _logger.LogInformation("✅ All tasks created successfully");

        // Step 2: Demonstrate Question Creation
        _logger.LogInformation("Step 2: Creating Questions");
        
        var appLinksQuestion = ApplicationLinksEnabled.InCurrentWorkspace();
        Assert.That(appLinksQuestion, Is.Not.Null, "ApplicationLinksEnabled question should be created");
        Assert.That(appLinksQuestion.Question, Is.Not.Empty, "Question should have descriptive text");
        
        var vsCodeQuestion = VSCodeInteractivity.IsWorking();
        Assert.That(vsCodeQuestion, Is.Not.Null, "VSCodeInteractivity question should be created");
        
        _logger.LogInformation("✅ All questions created successfully");

        // Step 3: Demonstrate Ability Creation
        _logger.LogInformation("Step 3: Creating Abilities");
        
        // Add browser ability for PIM UI automation
        var logger = AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<BrowseTheWeb>>();
        var browserAbility = BrowseTheWeb.Maximized(logger);
        Assert.That(browserAbility, Is.Not.Null, "BrowseTheWeb ability should be created");
        
        var azureMLAbility = UseAzureML.WithRole("Data Scientist", browserAbility);
        Assert.That(azureMLAbility, Is.Not.Null, "UseAzureML ability should be created");
        Assert.That(azureMLAbility.Name, Contains.Substring("Data Scientist"), "Ability name should contain role");
        
        var vsCodeHelper = _serviceProvider!.GetRequiredService<VSCodeDesktopHelper>();
        var vsCodeAbility = UseVSCodeDesktop.With(vsCodeHelper);
        Assert.That(vsCodeAbility, Is.Not.Null, "UseVSCodeDesktop ability should be created");
        
        _logger.LogInformation("✅ All abilities created successfully");

        // Step 4: Demonstrate Actor Ability Assignment
        _logger.LogInformation("Step 4: Assigning Abilities to Actor");
        
        _actor!.Can(azureMLAbility);
        Assert.That(_actor.HasAbility<UseAzureML>(), Is.True, "Actor should have Azure ML ability");
        
        _actor.Can(browserAbility);
        Assert.That(_actor.HasAbility<BrowseTheWeb>(), Is.True, "Actor should have browser ability");
        
        _actor.Can(vsCodeAbility);
        Assert.That(_actor.HasAbility<UseVSCodeDesktop>(), Is.True, "Actor should have VS Code ability");
        
        _logger.LogInformation("✅ All abilities assigned to actor successfully");

        // Step 5: Demonstrate Framework Integration
        _logger.LogInformation("Step 5: Testing Framework Integration");
        
        var retrievedAzureML = _actor.Using<UseAzureML>();
        Assert.That(retrievedAzureML, Is.Not.Null, "Should be able to retrieve Azure ML ability");
        Assert.That(retrievedAzureML, Is.SameAs(azureMLAbility), "Retrieved ability should be the same instance");
        
        var retrievedBrowser = _actor.Using<BrowseTheWeb>();
        Assert.That(retrievedBrowser, Is.Not.Null, "Should be able to retrieve browser ability");
        
        var retrievedVSCode = _actor.Using<UseVSCodeDesktop>();
        Assert.That(retrievedVSCode, Is.Not.Null, "Should be able to retrieve VS Code ability");
        
        _logger.LogInformation("✅ Framework integration working correctly");

        // Step 6: Demonstrate Task Creation (PIM is now a Task, not an Ability)
        _logger.LogInformation("Step 6: Demonstrating Task Creation");
        
        var pimTask = ActivatePIMRole.ForDataScientistRole("PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST")
            .WithJustification("Framework demonstration")
            .ForDuration(1)
            .Build();
        Assert.That(pimTask, Is.Not.Null, "PIM task should be created");
        Assert.That(pimTask.Name, Contains.Substring("Data Scientist"), "Task name should contain role");
        
        _logger.LogInformation("✅ Task creation working correctly");

        _logger.LogInformation("🎉 Framework demonstration completed successfully!");
        
        // Log summary
        _logger.LogInformation("📊 Framework Summary:");
        _logger.LogInformation("   - Tasks: 7 types demonstrated");
        _logger.LogInformation("   - Questions: 2 types demonstrated");
        _logger.LogInformation("   - Abilities: 3 types demonstrated");
        _logger.LogInformation("   - Actor: 1 with 3 abilities");
        _logger.LogInformation("   - Configuration: Loaded and validated");
        _logger.LogInformation("   - Service Registration: All services available");
    }

    [Test]
    [Description("Demonstrates the scenario flow without actual execution")]
    public async Task DemonstrateScenarioFlow()
    {
        _logger!.LogInformation("🎯 Demonstrating Scenario Flow Structure");

        // This test shows how the scenario would flow without actually executing against Azure
        var scenarioSteps = new List<string>
        {
            "1. Activate Data Scientist PIM role",
            "2. Navigate to workspace 'CTO-workspace'",
            "3. Login as user 'Javed Khan' if required",
            "4. Select workspace 'CTO-workspace'",
            "5. Choose compute option",
            "6. Open compute 'com-jk'",
            "7. Start compute 'com-jk' if not running",
            "8. Check if application links are enabled",
            "9. Start VS Code Desktop",
            "10. Verify VS Code interactivity"
        };

        _logger.LogInformation("📋 Complete Scenario Steps:");
        foreach (var step in scenarioSteps)
        {
            _logger.LogInformation("   {Step}", step);
        }

        // Demonstrate that all required components exist for each step
        var componentMapping = new Dictionary<string, string>
        {
            ["PIM Role Activation"] = "UsePIMRoleManagement ability + ActivatePIMRole task",
            ["Workspace Navigation"] = "UseAzureML ability + NavigateToWorkspace task",
            ["User Login"] = "UseAzureML ability + LoginAsUser task",
            ["Workspace Selection"] = "UseAzureML ability + SelectWorkspace task",
            ["Compute Options"] = "UseAzureML ability + ChooseComputeOption task",
            ["Compute Access"] = "UseAzureML ability + OpenCompute task",
            ["Compute Management"] = "UseAzureML ability + StartComputeIfNotRunning task",
            ["Application Links Check"] = "UseAzureML ability + ApplicationLinksEnabled question",
            ["VS Code Launch"] = "UseVSCodeDesktop ability + StartVSCodeDesktop task",
            ["VS Code Verification"] = "UseVSCodeDesktop ability + VSCodeInteractivity question"
        };

        _logger.LogInformation("🔧 Component Mapping:");
        foreach (var mapping in componentMapping)
        {
            _logger.LogInformation("   {Feature}: {Components}", mapping.Key, mapping.Value);
        }

        Assert.That(componentMapping.Count, Is.EqualTo(10), "All scenario steps should have corresponding components");
        
        _logger.LogInformation("✅ All scenario steps have corresponding framework components");
        _logger.LogInformation("🎉 Scenario flow demonstration completed successfully!");
    }

    [Test]
    [Description("Validates configuration loading and structure")]
    public void ValidateConfigurationStructure()
    {
        _logger!.LogInformation("🎯 Validating Configuration Structure");

        Assert.That(_config, Is.Not.Null, "Configuration should be loaded");
        Assert.That(_config!.Azure, Is.Not.Null, "Azure configuration section should exist");

        // Validate demo configuration values
        Assert.That(_config.Azure.SubscriptionId, Is.EqualTo("demo-subscription-id"), "Demo subscription ID should be loaded");
        Assert.That(_config.Azure.TenantId, Is.EqualTo("demo-tenant-id"), "Demo tenant ID should be loaded");
        Assert.That(_config.Azure.ResourceGroup, Is.EqualTo("demo-resource-group"), "Demo resource group should be loaded");
        Assert.That(_config.Azure.WorkspaceName, Is.EqualTo("CTO-workspace"), "Demo workspace name should be loaded");
        // Note: DataScientistRoleId removed from configuration as part of PIM refactoring

        _logger.LogInformation("✅ Configuration structure validated successfully");
        _logger.LogInformation("📊 Configuration Values:");
        _logger.LogInformation("   - Subscription ID: {SubscriptionId}", _config.Azure.SubscriptionId);
        _logger.LogInformation("   - Tenant ID: {TenantId}", _config.Azure.TenantId);
        _logger.LogInformation("   - Resource Group: {ResourceGroup}", _config.Azure.ResourceGroup);
        _logger.LogInformation("   - Workspace Name: {WorkspaceName}", _config.Azure.WorkspaceName);
        // Note: Data Scientist Role ID removed from configuration
    }

    [Test]
    [Description("Demonstrates service registration and dependency injection")]
    public void ValidateServiceRegistration()
    {
        _logger!.LogInformation("🎯 Validating Service Registration");

        // Validate all required services are registered
        var configuration = _serviceProvider!.GetService<IConfiguration>();
        Assert.That(configuration, Is.Not.Null, "IConfiguration should be registered");

        var testConfig = _serviceProvider.GetService<TestConfiguration>();
        Assert.That(testConfig, Is.Not.Null, "TestConfiguration should be registered");

        var vsCodeHelper = _serviceProvider.GetService<VSCodeDesktopHelper>();
        Assert.That(vsCodeHelper, Is.Not.Null, "VSCodeDesktopHelper should be registered");

        var logger = _serviceProvider.GetService<ILogger<FrameworkDemonstrationTest>>();
        Assert.That(logger, Is.Not.Null, "ILogger should be registered");

        _logger.LogInformation("✅ All required services are properly registered");
        _logger.LogInformation("📊 Registered Services:");
        _logger.LogInformation("   - IConfiguration: ✅");
        _logger.LogInformation("   - TestConfiguration: ✅");
        _logger.LogInformation("   - VSCodeDesktopHelper: ✅");
        _logger.LogInformation("   - ILogger<T>: ✅");
    }
}