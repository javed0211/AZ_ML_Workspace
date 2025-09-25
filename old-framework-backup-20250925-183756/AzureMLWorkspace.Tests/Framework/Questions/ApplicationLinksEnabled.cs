using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Abilities;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Questions;

public class ApplicationLinksEnabled : IQuestion<bool>
{
    private readonly ILogger<ApplicationLinksEnabled> _logger;

    public string Question => "Are application links enabled in the current workspace?";

    private ApplicationLinksEnabled(ILogger<ApplicationLinksEnabled> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static ApplicationLinksEnabled InCurrentWorkspace()
    {
        var logger = Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<ApplicationLinksEnabled>>();
        return new ApplicationLinksEnabled(logger);
    }

    public async Task<bool> AnsweredBy(IActor actor)
    {
        _logger.LogInformation("Checking if application links are enabled");

        try
        {
            // Get the Azure ML ability
            var azureMLAbility = actor.Using<UseAzureML>();
            if (azureMLAbility == null)
            {
                throw new InvalidOperationException("Actor does not have Azure ML ability");
            }

            // Check if application links are enabled
            var linksEnabled = await azureMLAbility.AreApplicationLinksEnabledAsync();

            _logger.LogInformation("Application links enabled: {LinksEnabled}", linksEnabled);
            return linksEnabled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if application links are enabled");
            throw;
        }
    }
}