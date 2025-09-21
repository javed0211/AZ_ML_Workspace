using Microsoft.Playwright;
using AzureMLWorkspace.Tests.Helpers;
using AzureMLWorkspace.Tests.Configuration;

namespace AzureMLWorkspace.Tests.Actions.Core;

/// <summary>
/// Base class for all actions providing common functionality
/// </summary>
public abstract class BaseAction : IChainableAction, IConditionalAction
{
    protected readonly IPage Page;
    protected readonly TestLogger Logger;
    protected readonly TestConfiguration Config;
    
    private readonly List<IAction> _chainedActions = new();
    private Func<Task<bool>>? _condition;
    private IAction? _alternativeAction;

    protected BaseAction(IPage page, TestLogger logger, TestConfiguration config)
    {
        Page = page;
        Logger = logger;
        Config = config;
    }

    /// <summary>
    /// Abstract method to be implemented by concrete actions
    /// </summary>
    protected abstract Task ExecuteActionAsync();

    /// <summary>
    /// Execute the action with error handling and logging
    /// </summary>
    public virtual async Task ExecuteAsync()
    {
        try
        {
            Logger.LogStep($"Executing action: {GetType().Name}");
            
            // Check condition if specified
            if (_condition != null)
            {
                var conditionMet = await _condition();
                if (!conditionMet)
                {
                    if (_alternativeAction != null)
                    {
                        Logger.LogStep($"Condition not met, executing alternative action");
                        await _alternativeAction.ExecuteAsync();
                    }
                    else
                    {
                        Logger.LogStep($"Condition not met, skipping action");
                    }
                    return;
                }
            }

            // Execute the main action
            await ExecuteActionAsync();
            
            // Execute chained actions
            foreach (var chainedAction in _chainedActions)
            {
                await chainedAction.ExecuteAsync();
            }
            
            Logger.LogStep($"Action completed successfully: {GetType().Name}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Action failed: {GetType().Name}");
            
            // Capture screenshot on failure
            await CaptureScreenshotOnFailure();
            throw;
        }
    }

    /// <summary>
    /// Chain another action to execute after this one
    /// </summary>
    public IChainableAction Then(IAction nextAction)
    {
        _chainedActions.Add(nextAction);
        return this;
    }

    /// <summary>
    /// Execute the action only if the condition is met
    /// </summary>
    public IConditionalAction When(Func<Task<bool>> condition)
    {
        _condition = condition;
        return this;
    }

    /// <summary>
    /// Execute an alternative action if the condition is not met
    /// </summary>
    public IConditionalAction Otherwise(IAction alternativeAction)
    {
        _alternativeAction = alternativeAction;
        return this;
    }

    /// <summary>
    /// Wait for an element to be visible
    /// </summary>
    protected async Task WaitForElementAsync(string selector, int timeoutMs = 30000)
    {
        Logger.LogStep($"Waiting for element: {selector}");
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
        { 
            Timeout = timeoutMs,
            State = WaitForSelectorState.Visible 
        });
    }

    /// <summary>
    /// Wait for an element to be hidden
    /// </summary>
    protected async Task WaitForElementToHideAsync(string selector, int timeoutMs = 30000)
    {
        Logger.LogStep($"Waiting for element to hide: {selector}");
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
        { 
            Timeout = timeoutMs,
            State = WaitForSelectorState.Hidden 
        });
    }

    /// <summary>
    /// Check if an element is visible
    /// </summary>
    protected async Task<bool> IsElementVisibleAsync(string selector)
    {
        try
        {
            return await Page.IsVisibleAsync(selector);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get text content from an element
    /// </summary>
    protected async Task<string> GetTextAsync(string selector)
    {
        Logger.LogStep($"Getting text from element: {selector}");
        return await Page.TextContentAsync(selector) ?? string.Empty;
    }

    /// <summary>
    /// Scroll element into view if needed
    /// </summary>
    protected async Task ScrollIntoViewAsync(string selector)
    {
        Logger.LogStep($"Scrolling to element: {selector}");
        await Page.Locator(selector).ScrollIntoViewIfNeededAsync();
    }

    /// <summary>
    /// Wait for page to load completely
    /// </summary>
    protected async Task WaitForPageLoadAsync()
    {
        Logger.LogStep("Waiting for page to load");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Capture screenshot on failure
    /// </summary>
    private async Task CaptureScreenshotOnFailure()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var screenshotPath = Path.Combine(Config.ScreenshotsPath, $"failure_{GetType().Name}_{timestamp}.png");
            
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });
            
            Logger.LogStep($"Screenshot captured: {screenshotPath}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to capture screenshot");
        }
    }
}