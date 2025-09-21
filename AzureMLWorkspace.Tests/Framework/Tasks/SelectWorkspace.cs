using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

public class SelectWorkspace : ITask
{
    private readonly string _workspaceName;
    private readonly ILogger<SelectWorkspace> _logger;

    private SelectWorkspace(string workspaceName, ILogger<SelectWorkspace> logger)
    {
        _workspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static SelectWorkspace Named(string workspaceName)
    {
        var logger = TestContext.ServiceProvider.GetRequiredService<ILogger<SelectWorkspace>>();
        return new SelectWorkspace(workspaceName, logger);
    }

    public async Task<T> PerformAs<T>(IActor actor) where T : IActor
    {
        _logger.LogInformation("Selecting workspace: {WorkspaceName}", _workspaceName);

        try
        {
            // Get the Azure ML ability
            var azureMLAbility = actor.GetAbility<UseAzureML>();
            if (azureMLAbility == null)
            {
                throw new InvalidOperationException("Actor does not have Azure ML ability");
            }

            // Select the specific workspace
            await azureMLAbility.SelectWorkspaceAsync(_workspaceName);

            _logger.LogInformation("Successfully selected workspace: {WorkspaceName}", _workspaceName);
            return (T)actor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to select workspace: {WorkspaceName}", _workspaceName);
            throw;
        }
    }
}