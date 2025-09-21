using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

public class StartComputeIfNotRunning : ITask
{
    private readonly string _computeName;
    private readonly ILogger<StartComputeIfNotRunning> _logger;

    private StartComputeIfNotRunning(string computeName, ILogger<StartComputeIfNotRunning> logger)
    {
        _computeName = computeName ?? throw new ArgumentNullException(nameof(computeName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static StartComputeIfNotRunning Named(string computeName)
    {
        var logger = TestContext.ServiceProvider.GetRequiredService<ILogger<StartComputeIfNotRunning>>();
        return new StartComputeIfNotRunning(computeName, logger);
    }

    public async Task<T> PerformAs<T>(IActor actor) where T : IActor
    {
        _logger.LogInformation("Checking and starting compute instance if not running: {ComputeName}", _computeName);

        try
        {
            // Get the Azure ML ability
            var azureMLAbility = actor.GetAbility<UseAzureML>();
            if (azureMLAbility == null)
            {
                throw new InvalidOperationException("Actor does not have Azure ML ability");
            }

            // Check if compute is running, start if not
            var isRunning = await azureMLAbility.IsComputeRunningAsync(_computeName);
            
            if (!isRunning)
            {
                _logger.LogInformation("Compute instance {ComputeName} is not running, starting it...", _computeName);
                await azureMLAbility.StartComputeInstanceAsync(_computeName);
                _logger.LogInformation("Successfully started compute instance: {ComputeName}", _computeName);
            }
            else
            {
                _logger.LogInformation("Compute instance {ComputeName} is already running", _computeName);
            }

            return (T)actor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start compute instance if not running: {ComputeName}", _computeName);
            throw;
        }
    }
}