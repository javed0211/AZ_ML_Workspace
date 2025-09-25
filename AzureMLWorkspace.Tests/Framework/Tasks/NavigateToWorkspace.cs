using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Abilities;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

public class NavigateToWorkspace : ITask
{
    private readonly string _workspaceName;
    private readonly ILogger<NavigateToWorkspace> _logger;

    public string Name => $"Navigate to workspace '{_workspaceName}'";

    private NavigateToWorkspace(string workspaceName, ILogger<NavigateToWorkspace> logger)
    {
        _workspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static NavigateToWorkspace Named(string workspaceName)
    {
        var logger = Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<NavigateToWorkspace>>();
        return new NavigateToWorkspace(workspaceName, logger);
    }

    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Navigating to workspace: {WorkspaceName}", _workspaceName);

        try
        {
            // Get the Azure ML ability
            var azureMLAbility = actor.Using<UseAzureML>();
            if (azureMLAbility == null)
            {
                throw new InvalidOperationException("Actor does not have Azure ML ability");
            }

            // Navigate to the workspace in the Azure portal
            await azureMLAbility.NavigateToWorkspaceAsync(_workspaceName);

            _logger.LogInformation("Successfully navigated to workspace: {WorkspaceName}", _workspaceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to workspace: {WorkspaceName}", _workspaceName);
            throw;
        }
    }
}