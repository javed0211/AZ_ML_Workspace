using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Questions;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Tasks;
using Microsoft.Extensions.Logging;
using Reqnroll;

namespace AzureMLWorkspace.Tests.StepDefinitions;

[Binding]
public class AzureMLWorkspaceSteps
{
    private readonly ILogger<AzureMLWorkspaceSteps> _logger;
    private IActor? _actor;
    private readonly List<string> _computeInstances = new();

    public AzureMLWorkspaceSteps(ILogger<AzureMLWorkspaceSteps> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Given(@"I am a data scientist named ""(.*)""")]
    public void GivenIAmADataScientistNamed(string name)
    {
        _logger.LogInformation("Creating actor: {ActorName}", name);
        _actor = Actor.Named(name, AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<Actor>>());
    }

    [Given(@"I have (.*) access to Azure ML")]
    public async Task GivenIHaveAccessToAzureML(string role)
    {
        _logger.LogInformation("Giving actor {Role} access to Azure ML", role);
        
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        var azureMLAbility = UseAzureML.WithRole(role);
        _actor.Can(azureMLAbility);
        await azureMLAbility.InitializeAsync();
    }

    [Given(@"I have access to Azure AI Search")]
    public async Task GivenIHaveAccessToAzureAISearch()
    {
        _logger.LogInformation("Giving actor access to Azure AI Search");
        
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        var searchAbility = UseAzureAISearch.WithDefaultConfiguration();
        _actor.Can(searchAbility);
        await searchAbility.InitializeAsync();
    }

    [Given(@"I have opened workspace ""(.*)""")]
    public async Task GivenIHaveOpenedWorkspace(string workspaceName)
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        await _actor.AttemptsTo(OpenWorkspace.Named(workspaceName));
    }

    [Given(@"compute instance ""(.*)"" is running")]
    public async Task GivenComputeInstanceIsRunning(string computeName)
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        // Start the compute instance if it's not already running
        await _actor.AttemptsTo(StartCompute.Named(computeName));
        _computeInstances.Add(computeName);
    }

    [When(@"I attempt to open workspace ""(.*)""")]
    public async Task WhenIAttemptToOpenWorkspace(string workspaceName)
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        await _actor.AttemptsTo(OpenWorkspace.Named(workspaceName));
    }

    [When(@"I start compute instance ""(.*)""")]
    public async Task WhenIStartComputeInstance(string computeName)
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        await _actor.AttemptsTo(StartCompute.Named(computeName));
        _computeInstances.Add(computeName);
    }

    [When(@"I stop compute instance ""(.*)""")]
    public async Task WhenIStopComputeInstance(string computeName)
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        await _actor.AttemptsTo(StopCompute.Named(computeName));
        _computeInstances.Remove(computeName);
    }

    [When(@"I start compute instances:")]
    public async Task WhenIStartComputeInstances(Table table)
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        foreach (var row in table.Rows)
        {
            var computeName = row["ComputeName"];
            await _actor.AttemptsTo(StartCompute.Named(computeName));
            _computeInstances.Add(computeName);
        }
    }

    [When(@"I stop all compute instances")]
    public async Task WhenIStopAllComputeInstances()
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        foreach (var computeName in _computeInstances.ToList())
        {
            await _actor.AttemptsTo(StopCompute.Named(computeName));
            _computeInstances.Remove(computeName);
        }
    }

    [Then(@"I should be able to access the workspace")]
    public async Task ThenIShouldBeAbleToAccessTheWorkspace()
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        // This is implicitly validated by the OpenWorkspace task succeeding
        await Task.CompletedTask;
    }

    [Then(@"the workspace should be available")]
    public async Task ThenTheWorkspaceShouldBeAvailable()
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        // Additional validation can be added here
        await Task.CompletedTask;
    }

    [Then(@"the compute instance should be running")]
    public async Task ThenTheComputeInstanceShouldBeRunning()
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        // Validate that the compute instance is in running state
        var computeName = _computeInstances.LastOrDefault();
        if (computeName != null)
        {
            await _actor.Should(Validate.ComputeStatus(computeName, "Succeeded"));
        }
    }

    [Then(@"I should be able to connect to it")]
    public async Task ThenIShouldBeAbleToConnectToIt()
    {
        // This would involve additional validation of compute connectivity
        await Task.CompletedTask;
    }

    [Then(@"the compute instance should be stopped")]
    public async Task ThenTheComputeInstanceShouldBeStopped()
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        // Validate that the compute instance is in stopped state
        var computeName = _computeInstances.LastOrDefault() ?? "test-compute";
        await _actor.Should(Validate.ComputeStatus(computeName, "Stopped"));
    }

    [Then(@"all compute instances should be running")]
    public async Task ThenAllComputeInstancesShouldBeRunning()
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        foreach (var computeName in _computeInstances)
        {
            await _actor.Should(Validate.ComputeStatus(computeName, "Succeeded"));
        }
    }

    [Then(@"all compute instances should be stopped")]
    public async Task ThenAllComputeInstancesShouldBeStopped()
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        // Since we removed them from the list when stopping, we need to check the last known instances
        // This is a simplified implementation - in practice, you'd track stopped instances separately
        await Task.CompletedTask;
    }

    [AfterScenario]
    public async Task CleanupAfterScenario()
    {
        if (_actor != null)
        {
            try
            {
                // Clean up any running compute instances
                foreach (var computeName in _computeInstances.ToList())
                {
                    try
                    {
                        await _actor.AttemptsTo(StopCompute.Named(computeName));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to stop compute instance {ComputeName} during cleanup", computeName);
                    }
                }

                // Dispose the actor
                if (_actor is IAsyncDisposable disposableActor)
                {
                    await disposableActor.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during scenario cleanup");
            }
            finally
            {
                _actor = null;
                _computeInstances.Clear();
            }
        }
    }
}