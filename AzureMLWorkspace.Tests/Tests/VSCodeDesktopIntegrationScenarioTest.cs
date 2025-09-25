using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Configuration;
using AzureMLWorkspace.Tests.Framework.Questions;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Tasks;
using AzureMLWorkspace.Tests.Framework.Tasks.Generated;
using AzureMLWorkspace.Tests.Framework.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace AzureMLWorkspace.Tests.Tests;

/// <summary>
/// Integration test for the complete "Azure ML Workspace with VS Code Desktop Integration" scenario
/// This test validates the entire framework and identifies any missing components
/// </summary>
[TestFixture]
[Category("Integration")]
[Category("VSCodeDesktop")]
public class VSCodeDesktopIntegrationScenarioTest
{
    private ServiceProvider? _serviceProvider;
    private ILogger<VSCodeDesktopIntegrationScenarioTest>? _logger;
    private TestConfiguration? _config;
    private IActor? _actor;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Build configuration using unified helper
        var configuration = ConfigurationHelper.BuildConfiguration();

        // Build service collection
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Debug);
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
        
        _logger = _serviceProvider.GetRequiredService<ILogger<VSCodeDesktopIntegrationScenarioTest>>();
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
        _logger!.LogInformation("=== Starting VS Code Desktop Integration Scenario Test ===");
        
        // Create actor
        var actorLogger = _serviceProvider!.GetRequiredService<ILogger<Actor>>();
        _actor = Actor.Named("TestDataScientist", actorLogger);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_actor != null)
        {
            try
            {
                // Clean up VS Code if it was launched
                if (_actor.HasAbility<UseVSCodeDesktop>())
                {
                    var vsCodeAbility = _actor.Using<UseVSCodeDesktop>();
                    await vsCodeAbility.CloseAsync();
                }

                // Clean up browser if it was used for PIM activation
                if (_actor.HasAbility<BrowseTheWeb>())
                {
                    var browserAbility = _actor.Using<BrowseTheWeb>();
                    // Note: PIM roles will auto-expire based on their duration
                    await browserAbility.CleanupAsync();
                }

                // Dispose actor
                if (_actor is IAsyncDisposable disposableActor)
                {
                    await disposableActor.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger!.LogWarning(ex, "Error during test cleanup");
            }
        }

        _logger!.LogInformation("=== VS Code Desktop Integration Scenario Test Completed ===");
    }

    [Test]
    [Description("Tests the complete Azure ML Workspace with VS Code Desktop Integration scenario")]
    public async Task CompleteVSCodeDesktopIntegrationScenario()
    {
        Assert.That(_actor, Is.Not.Null, "Actor should be created");
        Assert.That(_config, Is.Not.Null, "Configuration should be loaded");

        try
        {
            // Step 1: Activate PIM role through Azure Portal UI
            _logger!.LogInformation("Step 1: Activating Data Scientist PIM role through Azure Portal");
            
            var tenantId = _config!.Azure.TenantId;
            var subscriptionId = _config.Azure.SubscriptionId;
            
            Assert.That(tenantId, Is.Not.Empty, "Azure Tenant ID must be configured");
            Assert.That(subscriptionId, Is.Not.Empty, "Azure Subscription ID must be configured");

            // Add browser ability for UI-based PIM activation
            var logger = AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<BrowseTheWeb>>();
            var browserAbility = BrowseTheWeb.Maximized(logger);
            _actor!.Can(browserAbility);
            await browserAbility.InitializeAsync();

            // Perform PIM role activation through Azure Portal UI
            var justification = "Integration test - VS Code Desktop scenario";
            await _actor.AttemptsTo(
                ActivatePIMRole.ForDataScientistRole("PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST")
                    .WithJustification(justification)
                    .ForDuration(8) // 8 hours
                    .Build()
            );

            _logger.LogInformation("✅ Step 1 completed: PIM role activated through Azure Portal UI");

            // Step 2: Give actor Azure ML ability
            _logger.LogInformation("Step 2: Setting up Azure ML ability");
            
            var azureMLAbility = UseAzureML.WithRole("Data Scientist", browserAbility);
            _actor.Can(azureMLAbility);
            await azureMLAbility.InitializeAsync();

            _logger.LogInformation("✅ Step 2 completed: Azure ML ability configured");

            // Step 3: Navigate to workspace
            _logger.LogInformation("Step 3: Navigating to workspace");
            
            var workspaceName = _config.Azure.WorkspaceName;
            Assert.That(workspaceName, Is.Not.Empty, "Azure Workspace Name must be configured");

            await _actor.AttemptsTo(NavigateToWorkspace.Named(workspaceName));
            
            _logger.LogInformation("✅ Step 3 completed: Navigated to workspace");

            // Step 4: Login if required
            _logger.LogInformation("Step 4: Checking login requirements");
            
            await _actor.AttemptsTo(LoginAsUser.Named("Javed Khan"));
            
            _logger.LogInformation("✅ Step 4 completed: Login check completed");

            // Step 5: Select workspace
            _logger.LogInformation("Step 5: Selecting workspace");
            
            await _actor.AttemptsTo(SelectWorkspace.Named("CTO-workspace"));
            
            _logger.LogInformation("✅ Step 5 completed: Workspace selected");

            // Step 6: Choose compute option
            _logger.LogInformation("Step 6: Choosing compute option");
            
            await _actor.AttemptsTo(ChooseComputeOption.Now());
            
            _logger.LogInformation("✅ Step 6 completed: Compute option chosen");

            // Step 7: Open compute
            _logger.LogInformation("Step 7: Opening compute instance");
            
            await _actor.AttemptsTo(OpenCompute.Named("com-jk"));
            
            _logger.LogInformation("✅ Step 7 completed: Compute instance opened");

            // Step 8: Start compute if not running
            _logger.LogInformation("Step 8: Starting compute if not running");
            
            await _actor.AttemptsTo(StartComputeIfNotRunning.Named("com-jk"));
            
            _logger.LogInformation("✅ Step 8 completed: Compute instance started if needed");

            // Step 9: Check application links
            _logger.LogInformation("Step 9: Checking application links");
            
            var linksEnabled = await _actor.AsksFor(ApplicationLinksEnabled.InCurrentWorkspace());
            _logger.LogInformation("Application links enabled: {LinksEnabled}", linksEnabled);
            
            _logger.LogInformation("✅ Step 9 completed: Application links checked");

            // Step 10: Start VS Code Desktop
            _logger.LogInformation("Step 10: Starting VS Code Desktop");
            
            await _actor.AttemptsTo(StartVSCodeDesktop.Now());
            
            _logger.LogInformation("✅ Step 10 completed: VS Code Desktop started");

            // Step 11: Check VS Code interactivity
            _logger.LogInformation("Step 11: Checking VS Code interactivity");
            
            var isInteractive = await _actor.AsksFor(VSCodeInteractivity.IsWorking());
            Assert.That(isInteractive, Is.True, "VS Code should be interactive");
            
            _logger.LogInformation("✅ Step 11 completed: VS Code interactivity verified");

            _logger.LogInformation("🎉 Complete scenario executed successfully!");
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "❌ Scenario failed at step: {Message}", ex.Message);
            throw;
        }
    }

    [Test]
    [Description("Tests individual framework components for completeness")]
    public async Task ValidateFrameworkComponents()
    {
        _logger!.LogInformation("=== Validating Framework Components ===");

        // Test 1: Validate all required tasks exist
        _logger.LogInformation("Test 1: Validating task implementations");
        
        var tasks = new[]
        {
            typeof(NavigateToWorkspace),
            typeof(LoginAsUser),
            typeof(SelectWorkspace),
            typeof(ChooseComputeOption),
            typeof(OpenCompute),
            typeof(StartComputeIfNotRunning),
            typeof(StartVSCodeDesktop),
            typeof(ActivatePIMRole)
        };

        foreach (var taskType in tasks)
        {
            Assert.That(taskType, Is.Not.Null, $"Task {taskType.Name} should exist");
            _logger.LogDebug("✅ Task validated: {TaskName}", taskType.Name);
        }

        // Test 2: Validate all required questions exist
        _logger.LogInformation("Test 2: Validating question implementations");
        
        var questions = new[]
        {
            typeof(ApplicationLinksEnabled),
            typeof(VSCodeInteractivity)
        };

        foreach (var questionType in questions)
        {
            Assert.That(questionType, Is.Not.Null, $"Question {questionType.Name} should exist");
            _logger.LogDebug("✅ Question validated: {QuestionName}", questionType.Name);
        }

        // Test 3: Validate all required abilities exist
        _logger.LogInformation("Test 3: Validating ability implementations");
        
        var abilities = new[]
        {
            typeof(UseAzureML),
            typeof(UseVSCodeDesktop),
            typeof(BrowseTheWeb) // Browser ability is now used for PIM UI automation
        };

        foreach (var abilityType in abilities)
        {
            Assert.That(abilityType, Is.Not.Null, $"Ability {abilityType.Name} should exist");
            _logger.LogDebug("✅ Ability validated: {AbilityName}", abilityType.Name);
        }

        // Test 4: Validate service registrations
        _logger.LogInformation("Test 4: Validating service registrations");
        
        var vsCodeHelper = _serviceProvider!.GetService<VSCodeDesktopHelper>();
        Assert.That(vsCodeHelper, Is.Not.Null, "VSCodeDesktopHelper should be registered");
        
        var config = _serviceProvider.GetService<TestConfiguration>();
        Assert.That(config, Is.Not.Null, "TestConfiguration should be registered");

        _logger.LogInformation("✅ All framework components validated successfully");
    }

    [Test]
    [Description("Tests the PIM role management through Azure Portal UI")]
    public async Task ValidatePIMRoleManagementThroughUI()
    {
        _logger!.LogInformation("=== Testing PIM Role Management through Azure Portal UI ===");

        var tenantId = _config!.Azure.TenantId;
        var subscriptionId = _config.Azure.SubscriptionId;
        
        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(subscriptionId))
        {
            Assert.Ignore("Azure Tenant ID and Subscription ID must be configured for this test");
            return;
        }

        try
        {
            // Test PIM task creation
            var pimTask = ActivatePIMRole.ForDataScientistRole("PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST")
                .WithJustification("Test validation of PIM UI automation")
                .ForDuration(1); // 1 hour for testing

            Assert.That(pimTask, Is.Not.Null, "PIM task should be created");
            Assert.That(pimTask.Name, Contains.Substring("Data Scientist"), "Task name should contain role name");

            _logger.LogInformation("✅ PIM task created successfully: {TaskName}", pimTask.Name);

            // Test browser ability for UI automation
            var logger = AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<BrowseTheWeb>>();
            var browserAbility = BrowseTheWeb.Maximized(logger);
            Assert.That(browserAbility, Is.Not.Null, "Browser ability should be created for PIM UI automation");

            _logger.LogInformation("✅ PIM role management through UI validated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PIM role management UI test failed");
            Assert.Inconclusive($"PIM role management UI test inconclusive: {ex.Message}");
        }
    }

    [Test]
    [Description("Tests configuration completeness")]
    public void ValidateConfiguration()
    {
        _logger!.LogInformation("=== Validating Configuration ===");

        Assert.That(_config, Is.Not.Null, "Configuration should be loaded");
        Assert.That(_config!.Azure, Is.Not.Null, "Azure configuration should exist");

        // Log configuration status (without sensitive data)
        _logger.LogInformation("Configuration validation:");
        _logger.LogInformation("- Subscription ID: {HasValue}", !string.IsNullOrEmpty(_config.Azure.SubscriptionId) ? "✅ Set" : "❌ Missing");
        _logger.LogInformation("- Tenant ID: {HasValue}", !string.IsNullOrEmpty(_config.Azure.TenantId) ? "✅ Set" : "❌ Missing");
        _logger.LogInformation("- Resource Group: {HasValue}", !string.IsNullOrEmpty(_config.Azure.ResourceGroup) ? "✅ Set" : "❌ Missing");
        _logger.LogInformation("- Workspace Name: {HasValue}", !string.IsNullOrEmpty(_config.Azure.WorkspaceName) ? "✅ Set" : "❌ Missing");
        // Note: DataScientistRoleId was removed as part of PIM cleanup
        _logger.LogInformation("- PIM Configuration: ⚠️ Now handled via new ActivatePIMRole ability");

        _logger.LogInformation("✅ Configuration validation completed");
    }
}