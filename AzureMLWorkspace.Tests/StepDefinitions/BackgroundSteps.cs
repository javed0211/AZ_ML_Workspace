using Reqnroll;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.StepDefinitions;

/// <summary>
/// Step definitions for background setup tasks that run before scenarios
/// </summary>
[Binding]
public class BackgroundSteps
{
    private ILogger<BackgroundSteps> _logger => 
        AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<BackgroundSteps>>();

    [Given(@"I have activated the Data Scientist PIM role")]
    public async Task GivenIHaveActivatedTheDataScientistPIMRole()
    {
        _logger.LogInformation("Setting up Data Scientist PIM role activation in background");

        try
        {
            // Create a background actor for role activation
            var actorLogger = AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<Actor>>();
            var backgroundActor = Actor.Named("BackgroundSetup", actorLogger);

            // Add browser ability for UI-based PIM activation
            var logger = AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<BrowseTheWeb>>();
            var browserAbility = BrowseTheWeb.Maximized(logger);
            backgroundActor.Can(browserAbility);
            await browserAbility.InitializeAsync();

            // Perform PIM role activation through UI
            var justification = "Automated test setup - activating Data Scientist role for test execution";
            await backgroundActor.AttemptsTo(
                ActivatePIMRole.ForDataScientistRole("PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST")
                    .WithJustification(justification)
                    .ForDuration(8) // 8 hours
                    .Build()
            );

            _logger.LogInformation("Data Scientist PIM role activated successfully through Azure Portal UI");

            // Store the actor in scenario context for cleanup
            ScenarioContext.Current["BackgroundActor"] = backgroundActor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate Data Scientist PIM role in background");
            throw new InvalidOperationException($"Background PIM role activation failed: {ex.Message}", ex);
        }
    }

    [Given(@"I have the required Azure permissions")]
    public async Task GivenIHaveTheRequiredAzurePermissions()
    {
        _logger.LogInformation("Verifying Azure permissions are available");

        try
        {
            // This step can be used to verify that the necessary permissions are active
            // For now, we'll assume the PIM role activation provides the required permissions
            await GivenIHaveActivatedTheDataScientistPIMRole();
            
            _logger.LogInformation("Azure permissions verified and activated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify or activate Azure permissions");
            throw new InvalidOperationException($"Azure permissions setup failed: {ex.Message}", ex);
        }
    }

    [AfterScenario]
    public async Task CleanupBackgroundSetup()
    {
        if (ScenarioContext.Current.ContainsKey("BackgroundActor"))
        {
            var backgroundActor = ScenarioContext.Current["BackgroundActor"] as IActor;
            if (backgroundActor != null)
            {
                try
                {
                    _logger.LogInformation("Cleaning up background PIM role activation");

                    // TODO: Implement PIM role deactivation if needed
                    // For now, PIM roles will auto-expire based on their duration

                    // Dispose the background actor
                    if (backgroundActor is IAsyncDisposable disposableActor)
                    {
                        await disposableActor.DisposeAsync();
                    }

                    _logger.LogInformation("Background setup cleanup completed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during background setup cleanup");
                }
                finally
                {
                    ScenarioContext.Current.Remove("BackgroundActor");
                }
            }
        }
    }
}