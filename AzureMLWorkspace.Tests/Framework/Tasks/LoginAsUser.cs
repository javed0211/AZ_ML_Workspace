using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Abilities;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

public class LoginAsUser : ITask
{
    private readonly string _userName;
    private readonly ILogger<LoginAsUser> _logger;

    public string Name => $"Login as user '{_userName}'";

    private LoginAsUser(string userName, ILogger<LoginAsUser> logger)
    {
        _userName = userName ?? throw new ArgumentNullException(nameof(userName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static LoginAsUser Named(string userName)
    {
        var logger = Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<LoginAsUser>>();
        return new LoginAsUser(userName, logger);
    }

    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Attempting to login as user: {UserName}", _userName);

        try
        {
            // Get the Azure ML ability
            var azureMLAbility = actor.Using<UseAzureML>();
            if (azureMLAbility == null)
            {
                throw new InvalidOperationException("Actor does not have Azure ML ability");
            }

            // Perform login if required
            await azureMLAbility.LoginIfRequiredAsync(_userName);

            _logger.LogInformation("Successfully logged in as user: {UserName}", _userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to login as user: {UserName}", _userName);
            throw;
        }
    }
}