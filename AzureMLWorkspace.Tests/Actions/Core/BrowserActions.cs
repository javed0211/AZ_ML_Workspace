using Microsoft.Playwright;
using AzureMLWorkspace.Tests.Helpers;
using AzureMLWorkspace.Tests.Configuration;

namespace AzureMLWorkspace.Tests.Actions.Core;

/// <summary>
/// Common browser actions for navigation, interaction, and verification
/// </summary>
public static class BrowserActions
{
    /// <summary>
    /// Navigate to a URL
    /// </summary>
    public static NavigateAction NavigateTo(IPage page, TestLogger logger, TestConfiguration config, string url)
        => new(page, logger, config, url);

    /// <summary>
    /// Click on an element
    /// </summary>
    public static ClickAction Click(IPage page, TestLogger logger, TestConfiguration config, string selector)
        => new(page, logger, config, selector);

    /// <summary>
    /// Type text into an element
    /// </summary>
    public static TypeAction Type(IPage page, TestLogger logger, TestConfiguration config, string selector, string text)
        => new(page, logger, config, selector, text);

    /// <summary>
    /// Wait for an element to be visible
    /// </summary>
    public static WaitForElementAction WaitForElement(IPage page, TestLogger logger, TestConfiguration config, string selector)
        => new(page, logger, config, selector);

    /// <summary>
    /// Verify element is visible
    /// </summary>
    public static VerifyElementVisibleAction VerifyElementVisible(IPage page, TestLogger logger, TestConfiguration config, string selector)
        => new(page, logger, config, selector);

    /// <summary>
    /// Verify text content
    /// </summary>
    public static VerifyTextAction VerifyText(IPage page, TestLogger logger, TestConfiguration config, string selector, string expectedText)
        => new(page, logger, config, selector, expectedText);

    /// <summary>
    /// Take a screenshot
    /// </summary>
    public static ScreenshotAction TakeScreenshot(IPage page, TestLogger logger, TestConfiguration config, string? fileName = null)
        => new(page, logger, config, fileName);

    /// <summary>
    /// Wait for page to load
    /// </summary>
    public static WaitForPageLoadAction WaitForPageLoad(IPage page, TestLogger logger, TestConfiguration config)
        => new(page, logger, config);

    /// <summary>
    /// Scroll to element
    /// </summary>
    public static ScrollToElementAction ScrollToElement(IPage page, TestLogger logger, TestConfiguration config, string selector)
        => new(page, logger, config, selector);

    /// <summary>
    /// Select option from dropdown
    /// </summary>
    public static SelectOptionAction SelectOption(IPage page, TestLogger logger, TestConfiguration config, string selector, string value)
        => new(page, logger, config, selector, value);

    /// <summary>
    /// Upload file
    /// </summary>
    public static UploadFileAction UploadFile(IPage page, TestLogger logger, TestConfiguration config, string selector, string filePath)
        => new(page, logger, config, selector, filePath);
}

/// <summary>
/// Navigate to a URL action
/// </summary>
public class NavigateAction : BaseAction
{
    private readonly string _url;

    public NavigateAction(IPage page, TestLogger logger, TestConfiguration config, string url)
        : base(page, logger, config)
    {
        _url = url;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Navigating to: {_url}");
        await Page.GotoAsync(_url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }
}

/// <summary>
/// Click element action
/// </summary>
public class ClickAction : BaseAction
{
    private readonly string _selector;

    public ClickAction(IPage page, TestLogger logger, TestConfiguration config, string selector)
        : base(page, logger, config)
    {
        _selector = selector;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Clicking element: {_selector}");
        await WaitForElementAsync(_selector);
        await ScrollIntoViewAsync(_selector);
        await Page.ClickAsync(_selector);
    }
}

/// <summary>
/// Type text action
/// </summary>
public class TypeAction : BaseAction
{
    private readonly string _selector;
    private readonly string _text;

    public TypeAction(IPage page, TestLogger logger, TestConfiguration config, string selector, string text)
        : base(page, logger, config)
    {
        _selector = selector;
        _text = text;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Typing text into element: {_selector}");
        await WaitForElementAsync(_selector);
        await ScrollIntoViewAsync(_selector);
        await Page.FillAsync(_selector, _text);
    }
}

/// <summary>
/// Wait for element action
/// </summary>
public class WaitForElementAction : BaseAction
{
    private readonly string _selector;

    public WaitForElementAction(IPage page, TestLogger logger, TestConfiguration config, string selector)
        : base(page, logger, config)
    {
        _selector = selector;
    }

    protected override async Task ExecuteActionAsync()
    {
        await WaitForElementAsync(_selector);
    }
}

/// <summary>
/// Verify element visible action
/// </summary>
public class VerifyElementVisibleAction : BaseAction
{
    private readonly string _selector;

    public VerifyElementVisibleAction(IPage page, TestLogger logger, TestConfiguration config, string selector)
        : base(page, logger, config)
    {
        _selector = selector;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Verifying element is visible: {_selector}");
        var isVisible = await IsElementVisibleAsync(_selector);
        if (!isVisible)
        {
            throw new AssertionException($"Element is not visible: {_selector}");
        }
    }
}

/// <summary>
/// Verify text content action
/// </summary>
public class VerifyTextAction : BaseAction
{
    private readonly string _selector;
    private readonly string _expectedText;

    public VerifyTextAction(IPage page, TestLogger logger, TestConfiguration config, string selector, string expectedText)
        : base(page, logger, config)
    {
        _selector = selector;
        _expectedText = expectedText;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Verifying text content: {_selector} should contain '{_expectedText}'");
        var actualText = await GetTextAsync(_selector);
        if (!actualText.Contains(_expectedText))
        {
            throw new AssertionException($"Text verification failed. Expected: '{_expectedText}', Actual: '{actualText}'");
        }
    }
}

/// <summary>
/// Take screenshot action
/// </summary>
public class ScreenshotAction : BaseAction
{
    private readonly string? _fileName;

    public ScreenshotAction(IPage page, TestLogger logger, TestConfiguration config, string? fileName = null)
        : base(page, logger, config)
    {
        _fileName = fileName;
    }

    protected override async Task ExecuteActionAsync()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = _fileName ?? $"screenshot_{timestamp}.png";
        var screenshotPath = Path.Combine(Config.ScreenshotsPath, fileName);
        
        Logger.LogStep($"Taking screenshot: {screenshotPath}");
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = screenshotPath,
            FullPage = true
        });
    }
}

/// <summary>
/// Wait for page load action
/// </summary>
public class WaitForPageLoadAction : BaseAction
{
    public WaitForPageLoadAction(IPage page, TestLogger logger, TestConfiguration config)
        : base(page, logger, config)
    {
    }

    protected override async Task ExecuteActionAsync()
    {
        await WaitForPageLoadAsync();
    }
}

/// <summary>
/// Scroll to element action
/// </summary>
public class ScrollToElementAction : BaseAction
{
    private readonly string _selector;

    public ScrollToElementAction(IPage page, TestLogger logger, TestConfiguration config, string selector)
        : base(page, logger, config)
    {
        _selector = selector;
    }

    protected override async Task ExecuteActionAsync()
    {
        await ScrollIntoViewAsync(_selector);
    }
}

/// <summary>
/// Select option from dropdown action
/// </summary>
public class SelectOptionAction : BaseAction
{
    private readonly string _selector;
    private readonly string _value;

    public SelectOptionAction(IPage page, TestLogger logger, TestConfiguration config, string selector, string value)
        : base(page, logger, config)
    {
        _selector = selector;
        _value = value;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Selecting option '{_value}' from dropdown: {_selector}");
        await WaitForElementAsync(_selector);
        await Page.SelectOptionAsync(_selector, _value);
    }
}

/// <summary>
/// Upload file action
/// </summary>
public class UploadFileAction : BaseAction
{
    private readonly string _selector;
    private readonly string _filePath;

    public UploadFileAction(IPage page, TestLogger logger, TestConfiguration config, string selector, string filePath)
        : base(page, logger, config)
    {
        _selector = selector;
        _filePath = filePath;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Uploading file '{_filePath}' to element: {_selector}");
        
        // Ensure file exists and use cross-platform path
        var normalizedPath = Path.GetFullPath(_filePath);
        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException($"File not found: {normalizedPath}");
        }
        
        await WaitForElementAsync(_selector);
        await Page.SetInputFilesAsync(_selector, normalizedPath);
    }
}

/// <summary>
/// Custom assertion exception for action failures
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
    public AssertionException(string message, Exception innerException) : base(message, innerException) { }
}