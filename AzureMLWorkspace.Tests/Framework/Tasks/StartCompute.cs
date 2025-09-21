using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

/// <summary>
/// Task to start a compute instance
/// </summary>
public class StartCompute : ITask
{
    private readonly string _computeName;
    private readonly ILogger<StartCompute> _logger;

    public string Name => $"Start compute instance '{_computeName}'";

    private StartCompute(string computeName, ILogger<StartCompute> logger)
    {
        _computeName = computeName ?? throw new ArgumentNullException(nameof(computeName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Starting compute instance: {ComputeName}", _computeName);

        if (!actor.HasAbility<UseAzureML>())
        {
            throw new InvalidOperationException($"Actor '{actor.Name}' must have UseAzureML ability to start compute instances");
        }

        var azureML = actor.Using<UseAzureML>();
        await azureML.StartCompute(_computeName);

        _logger.LogInformation("Successfully started compute instance: {ComputeName}", _computeName);
    }

    /// <summary>
    /// Creates a task to start a compute instance with the specified name
    /// </summary>
    public static StartCompute Named(string computeName)
    {
        return new StartCompute(computeName, 
            AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<StartCompute>>());
    }
}