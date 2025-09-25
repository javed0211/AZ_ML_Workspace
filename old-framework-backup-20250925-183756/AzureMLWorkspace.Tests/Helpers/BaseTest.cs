using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using AzureMLWorkspace.Tests.Configuration;
using AzureMLWorkspace.Tests.Helpers;
using AzureMLWorkspace.Tests.Actions.Core;

namespace AzureMLWorkspace.Tests.Helpers;

[TestFixture]
public abstract class BaseTest : PageTest
{
    protected TestConfiguration Config { get; private set; } = null!;
    protected TestLogger TestLogger { get; private set; } = null!;
    protected string TestName { get; private set; } = string.Empty;

    [OneTimeSetUp]
    public virtual Task OneTimeSetUp()
    {
        Config = new TestConfiguration();
        TestLogger = new TestLogger(Config);
        
        // Ensure directories exist
        Directory.CreateDirectory(Config.ScreenshotsPath);
        Directory.CreateDirectory(Config.VideosPath);
        Directory.CreateDirectory(Config.TracesPath);
        Directory.CreateDirectory(Config.TestDataPath);
        
        return Task.CompletedTask;
    }

    [SetUp]
    public virtual async Task SetUp()
    {
        TestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogTestStart(TestName);

        // Configure browser context options
        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            RecordVideoDir = Config.CaptureVideos ? Config.VideosPath : null,
            RecordVideoSize = Config.CaptureVideos ? new RecordVideoSize { Width = 1920, Height = 1080 } : null
        };

        // Set up tracing if enabled
        if (Config.CaptureTraces)
        {
            await Context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        var testPassed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Passed;
        
        try
        {
            // Capture screenshot on failure
            if (!testPassed && Config.CaptureScreenshots)
            {
                await CaptureScreenshot($"{TestName}_failure");
            }

            // Stop tracing and save if enabled
            if (Config.CaptureTraces)
            {
                var tracePath = Path.Combine(Config.TracesPath, $"{TestName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
                await Context.Tracing.StopAsync(new TracingStopOptions
                {
                    Path = tracePath
                });
                TestLogger.Info($"Trace saved: {tracePath}");
            }
        }
        catch (Exception ex)
        {
            TestLogger.Error(ex, "Error during test teardown");
        }
        finally
        {
            TestLogger.LogTestEnd(TestName, testPassed);
        }
    }

    [OneTimeTearDown]
    public virtual Task OneTimeTearDown()
    {
        TestLogger?.Dispose();
        return Task.CompletedTask;
    }

    protected async Task CaptureScreenshot(string name = "")
    {
        try
        {
            var fileName = string.IsNullOrEmpty(name) 
                ? $"{TestName}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                : $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            
            var screenshotPath = Path.Combine(Config.ScreenshotsPath, fileName);
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
            TestLogger.LogScreenshot(screenshotPath);
        }
        catch (Exception ex)
        {
            TestLogger.Error(ex, "Failed to capture screenshot");
        }
    }

    /// <summary>
    /// Create an action builder for fluent action chaining
    /// </summary>
    protected ActionBuilder Actions => new(Page, TestLogger, Config);

    /// <summary>
    /// Navigate to Azure ML workspace
    /// </summary>
    protected async Task NavigateToAzureML()
    {
        await BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl).ExecuteAsync();
    }

    /// <summary>
    /// Wait for an element to be visible
    /// </summary>
    protected async Task WaitForElement(string selector, int timeoutMs = 0)
    {
        await BrowserActions.WaitForElement(Page, TestLogger, Config, selector).ExecuteAsync();
    }

    /// <summary>
    /// Click on an element
    /// </summary>
    protected async Task ClickElement(string selector)
    {
        await BrowserActions.Click(Page, TestLogger, Config, selector).ExecuteAsync();
    }

    /// <summary>
    /// Type text into an element
    /// </summary>
    protected async Task TypeText(string selector, string text)
    {
        await BrowserActions.Type(Page, TestLogger, Config, selector, text).ExecuteAsync();
    }
}