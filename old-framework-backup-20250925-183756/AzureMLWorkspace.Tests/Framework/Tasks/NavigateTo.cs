using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

/// <summary>
/// Task to navigate to a specific URL
/// </summary>
public class NavigateTo : ITask
{
    private readonly string _url;
    private readonly ILogger<NavigateTo> _logger;

    public string Name => $"Navigate to '{_url}'";

    private NavigateTo(string url, ILogger<NavigateTo> logger)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Navigating to: {Url}", _url);

        if (!actor.HasAbility<BrowseTheWeb>())
        {
            throw new InvalidOperationException($"Actor '{actor.Name}' must have BrowseTheWeb ability to navigate to URLs");
        }

        var browser = actor.Using<BrowseTheWeb>();
        await browser.Page.GotoAsync(_url);
        await browser.Page.WaitForLoadStateAsync();

        _logger.LogInformation("Successfully navigated to: {Url}", _url);
    }

    /// <summary>
    /// Creates a task to navigate to the specified URL
    /// </summary>
    public static NavigateTo Url(string url)
    {
        return new NavigateTo(url, 
            AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<NavigateTo>>());
    }

    /// <summary>
    /// Creates a task to navigate to the Azure ML portal
    /// </summary>
    public static NavigateTo AzureMLPortal()
    {
        return Url("https://ml.azure.com");
    }

    /// <summary>
    /// Creates a task to navigate to a specific workspace
    /// </summary>
    public static NavigateTo Workspace(string workspaceName)
    {
        return Url($"https://ml.azure.com/workspaces/{workspaceName}");
    }
}