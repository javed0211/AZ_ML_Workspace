using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

public class LoginAsUser : ITask
{
    private readonly string _userName;
    private readonly ILogger<LoginAsUser> _logger;

    private LoginAsUser(string userName, ILogger<LoginAsUser> logger)
    {
        _userName = userName ?? throw new ArgumentNullException(nameof(userName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static LoginAsUser Named(string userName)
    {
        var logger = TestContext.ServiceProvider.GetRequiredService<ILogger<LoginAsUser>>();
        return new LoginAsUser(userName, logger);
    }

    public async Task<T> PerformAs<T>(IActor actor) where T : IActor
    {
        _logger.LogInformation("Attempting to login as user: {UserName}", _userName);

        try
        {
            // Get the Azure ML ability
            var azureMLAbility = actor.GetAbility<UseAzureML>();
            if (azureMLAbility == null)
            {
                throw new InvalidOperationException("Actor does not have Azure ML ability");
            }

            // Perform login if required
            await azureMLAbility.LoginIfRequiredAsync(_userName);

            _logger.LogInformation("Successfully logged in as user: {UserName}", _userName);
            return (T)actor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to login as user: {UserName}", _userName);
            throw;
        }
    }
}