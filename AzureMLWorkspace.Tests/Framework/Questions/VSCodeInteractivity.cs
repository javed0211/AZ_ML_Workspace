using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Abilities;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Questions;

public class VSCodeInteractivity : IQuestion<bool>
{
    private readonly ILogger<VSCodeInteractivity> _logger;

    public string Question => "Is VS Code Desktop interactive and working?";

    private VSCodeInteractivity(ILogger<VSCodeInteractivity> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static VSCodeInteractivity IsWorking()
    {
        var logger = Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<VSCodeInteractivity>>();
        return new VSCodeInteractivity(logger);
    }

    public async Task<bool> AnsweredBy(IActor actor)
    {
        _logger.LogInformation("Checking VS Code Desktop interactivity");

        try
        {
            // Get the VS Code Desktop ability
            var vsCodeAbility = actor.Using<UseVSCodeDesktop>();
            if (vsCodeAbility == null)
            {
                throw new InvalidOperationException("Actor does not have VS Code Desktop ability");
            }

            // Check interactivity
            var result = await vsCodeAbility.CheckInteractivityAsync();

            _logger.LogInformation("VS Code interactivity check result: {Success} - {Message}", 
                result.Success, result.Message);

            return result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check VS Code Desktop interactivity");
            return false;
        }
    }
}