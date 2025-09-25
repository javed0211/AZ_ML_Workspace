using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

/// <summary>
/// Task to open an Azure ML workspace
/// </summary>
public class OpenWorkspace : ITask
{
    private readonly string _workspaceName;
    private readonly ILogger<OpenWorkspace> _logger;

    public string Name => $"Open workspace '{_workspaceName}'";

    private OpenWorkspace(string workspaceName, ILogger<OpenWorkspace> logger)
    {
        _workspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Opening workspace: {WorkspaceName}", _workspaceName);

        // If the actor can browse the web, navigate to the workspace
        if (actor.HasAbility<BrowseTheWeb>())
        {
            var browser = actor.Using<BrowseTheWeb>();
            var baseUrl = "https://ml.azure.com";
            var workspaceUrl = $"{baseUrl}/workspaces/{_workspaceName}";
            
            await browser.Page.GotoAsync(workspaceUrl);
            await browser.Page.WaitForLoadStateAsync();
            
            _logger.LogInformation("Navigated to workspace URL: {WorkspaceUrl}", workspaceUrl);
        }

        // If the actor can use Azure ML, verify workspace access
        if (actor.HasAbility<UseAzureML>())
        {
            var azureML = actor.Using<UseAzureML>();
            var workspace = azureML.Workspace;
            
            // Verify we can access the workspace
            var workspaceData = workspace.Data;
            if (workspaceData.Name != _workspaceName)
            {
                throw new InvalidOperationException($"Expected workspace '{_workspaceName}' but got '{workspaceData.Name}'");
            }
            
            _logger.LogInformation("Verified access to workspace: {WorkspaceName}", workspaceData.Name);
        }

        _logger.LogInformation("Successfully opened workspace: {WorkspaceName}", _workspaceName);
    }

    /// <summary>
    /// Creates a task to open a workspace with the specified name
    /// </summary>
    public static OpenWorkspace Named(string workspaceName)
    {
        return new OpenWorkspace(workspaceName, 
            AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<OpenWorkspace>>());
    }
}