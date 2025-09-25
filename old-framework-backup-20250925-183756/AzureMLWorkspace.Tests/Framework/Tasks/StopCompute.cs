using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

/// <summary>
/// Task to stop a compute instance
/// </summary>
public class StopCompute : ITask
{
    private readonly string _computeName;
    private readonly ILogger<StopCompute> _logger;

    public string Name => $"Stop compute instance '{_computeName}'";

    private StopCompute(string computeName, ILogger<StopCompute> logger)
    {
        _computeName = computeName ?? throw new ArgumentNullException(nameof(computeName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Stopping compute instance: {ComputeName}", _computeName);

        if (!actor.HasAbility<UseAzureML>())
        {
            throw new InvalidOperationException($"Actor '{actor.Name}' must have UseAzureML ability to stop compute instances");
        }

        var azureML = actor.Using<UseAzureML>();
        await azureML.StopCompute(_computeName);

        _logger.LogInformation("Successfully stopped compute instance: {ComputeName}", _computeName);
    }

    /// <summary>
    /// Creates a task to stop a compute instance with the specified name
    /// </summary>
    public static StopCompute Named(string computeName)
    {
        return new StopCompute(computeName, 
            AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<StopCompute>>());
    }
}