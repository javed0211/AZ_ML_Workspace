using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Abilities;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

public class SelectWorkspace : ITask
{
    private readonly string _workspaceName;
    private readonly ILogger<SelectWorkspace> _logger;

    public string Name => $"Select workspace '{_workspaceName}'";

    private SelectWorkspace(string workspaceName, ILogger<SelectWorkspace> logger)
    {
        _workspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static SelectWorkspace Named(string workspaceName)
    {
        var logger = Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<SelectWorkspace>>();
        return new SelectWorkspace(workspaceName, logger);
    }

    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Selecting workspace: {WorkspaceName}", _workspaceName);

        try
        {
            // Get the Azure ML ability
            var azureMLAbility = actor.Using<UseAzureML>();
            if (azureMLAbility == null)
            {
                throw new InvalidOperationException("Actor does not have Azure ML ability");
            }

            // Select the specific workspace
            await azureMLAbility.SelectWorkspaceAsync(_workspaceName);

            _logger.LogInformation("Successfully selected workspace: {WorkspaceName}", _workspaceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to select workspace: {WorkspaceName}", _workspaceName);
            throw;
        }
    }
}