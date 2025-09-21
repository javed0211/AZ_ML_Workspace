using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

public class OpenCompute : ITask
{
    private readonly string _computeName;
    private readonly ILogger<OpenCompute> _logger;

    private OpenCompute(string computeName, ILogger<OpenCompute> logger)
    {
        _computeName = computeName ?? throw new ArgumentNullException(nameof(computeName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static OpenCompute Named(string computeName)
    {
        var logger = TestContext.ServiceProvider.GetRequiredService<ILogger<OpenCompute>>();
        return new OpenCompute(computeName, logger);
    }

    public async Task<T> PerformAs<T>(IActor actor) where T : IActor
    {
        _logger.LogInformation("Opening compute instance: {ComputeName}", _computeName);

        try
        {
            // Get the Azure ML ability
            var azureMLAbility = actor.GetAbility<UseAzureML>();
            if (azureMLAbility == null)
            {
                throw new InvalidOperationException("Actor does not have Azure ML ability");
            }

            // Open the specific compute instance
            await azureMLAbility.OpenComputeInstanceAsync(_computeName);

            _logger.LogInformation("Successfully opened compute instance: {ComputeName}", _computeName);
            return (T)actor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open compute instance: {ComputeName}", _computeName);
            throw;
        }
    }
}