using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Abilities;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

public class ChooseComputeOption : ITask
{
    private readonly ILogger<ChooseComputeOption> _logger;

    public string Name => "Choose compute option";

    private ChooseComputeOption(ILogger<ChooseComputeOption> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static ChooseComputeOption Now()
    {
        var logger = Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<ChooseComputeOption>>();
        return new ChooseComputeOption(logger);
    }

    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Choosing compute option in Azure ML workspace");

        try
        {
            // Get the Azure ML ability
            var azureMLAbility = actor.Using<UseAzureML>();
            if (azureMLAbility == null)
            {
                throw new InvalidOperationException("Actor does not have Azure ML ability");
            }

            // Navigate to compute section
            await azureMLAbility.NavigateToComputeAsync();

            _logger.LogInformation("Successfully navigated to compute options");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to choose compute option");
            throw;
        }
    }
}