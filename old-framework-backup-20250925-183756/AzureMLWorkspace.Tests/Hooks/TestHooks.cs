using AzureMLWorkspace.Tests.Framework;
using AzureMLWorkspace.Tests.Framework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Serilog;
using Serilog.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Hooks;

/// <summary>
/// Global hooks for Reqnroll BDD tests
/// </summary>
[Binding]
public class TestHooks
{
    private static IServiceProvider? _serviceProvider;
    private static IConfiguration? _configuration;
    private static TestConfiguration? _testConfig;

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        // Build configuration using unified helper
        _configuration = ConfigurationHelper.BuildConfiguration();
        _testConfig = TestConfiguration.LoadFromConfiguration(_configuration);

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(_testConfig.Logging.LogLevel))
            .WriteTo.Console()
            .WriteTo.File(
                _testConfig.Logging.LogFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: _testConfig.Logging.MaxLogFiles,
                fileSizeLimitBytes: _testConfig.Logging.MaxLogFileSizeMB * 1024 * 1024)
            .CreateLogger();

        // Build service collection
        var services = new ServiceCollection();
        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
        AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider = _serviceProvider;

        var logger = _serviceProvider.GetRequiredService<ILogger<TestHooks>>();
        logger.LogInformation("BDD Test run started - Framework initialized");
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        var logger = _serviceProvider?.GetService<ILogger<TestHooks>>();
        logger?.LogInformation("BDD Test run completed - Cleaning up resources");

        if (_serviceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Log.CloseAndFlush();
    }

    [BeforeScenario]
    public void BeforeScenario(ScenarioContext scenarioContext)
    {
        var logger = _serviceProvider?.GetService<ILogger<TestHooks>>();
        logger?.LogInformation("Starting scenario: {ScenarioTitle}", scenarioContext.ScenarioInfo.Title);

        // Store scenario context for access in step definitions
        scenarioContext.Set(_serviceProvider!, "ServiceProvider");
        scenarioContext.Set(_configuration!, "Configuration");
        scenarioContext.Set(_testConfig!, "TestConfig");
    }

    [AfterScenario]
    public async Task AfterScenario(ScenarioContext scenarioContext)
    {
        var logger = _serviceProvider?.GetService<ILogger<TestHooks>>();
        logger?.LogInformation("Scenario completed: {ScenarioTitle} - {Status}", 
            scenarioContext.ScenarioInfo.Title, 
            scenarioContext.ScenarioExecutionStatus);

        // Capture screenshot on failure
        if (scenarioContext.TestError != null && _testConfig?.Browser.CaptureScreenshots == true)
        {
            await CaptureScreenshotOnFailure(scenarioContext);
        }

        // Clean up any resources stored in scenario context
        await CleanupScenarioResources(scenarioContext);
    }

    [BeforeStep]
    public void BeforeStep(ScenarioContext scenarioContext)
    {
        var logger = _serviceProvider?.GetService<ILogger<TestHooks>>();
        logger?.LogDebug("Executing step: {StepText}", scenarioContext.StepContext.StepInfo.Text);
    }

    [AfterStep]
    public void AfterStep(ScenarioContext scenarioContext)
    {
        var logger = _serviceProvider?.GetService<ILogger<TestHooks>>();
        var stepResult = scenarioContext.StepContext.StepInfo.StepDefinitionType;
        logger?.LogDebug("Step completed: {StepText} - {StepType}", 
            scenarioContext.StepContext.StepInfo.Text, stepResult);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register configuration
        services.AddSingleton(_configuration!);
        services.AddSingleton(_testConfig!);

        // Register logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger);
        });

        // Register framework services
        services.AddTransient<Framework.Screenplay.Actor>();
        services.AddSingleton<ILoggerFactory, SerilogLoggerFactory>();

        // Register step definition classes
        services.AddTransient<StepDefinitions.AzureMLWorkspaceSteps>();
        services.AddTransient<StepDefinitions.AzureAISearchSteps>();
        
        // Register framework utilities
        services.AddTransient<Framework.Utilities.VSCodeDesktopHelper>();
        
        // Register OTP service for MFA automation
        services.AddTransient<Framework.Services.OTPService>();
    }

    private async Task CaptureScreenshotOnFailure(ScenarioContext scenarioContext)
    {
        try
        {
            var logger = _serviceProvider?.GetService<ILogger<TestHooks>>();
            
            // Try to get the current page from scenario context
            if (scenarioContext.TryGetValue("CurrentPage", out Microsoft.Playwright.IPage page))
            {
                var screenshotPath = Path.Combine(
                    _testConfig!.Reporting.ReportOutputPath,
                    "Screenshots",
                    $"{scenarioContext.ScenarioInfo.Title.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

                Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
                await page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                
                logger?.LogInformation("Screenshot captured for failed scenario: {ScreenshotPath}", screenshotPath);
            }
        }
        catch (Exception ex)
        {
            var logger = _serviceProvider?.GetService<ILogger<TestHooks>>();
            logger?.LogWarning(ex, "Failed to capture screenshot for scenario: {ScenarioTitle}", 
                scenarioContext.ScenarioInfo.Title);
        }
    }

    private async Task CleanupScenarioResources(ScenarioContext scenarioContext)
    {
        var logger = _serviceProvider?.GetService<ILogger<TestHooks>>();
        
        try
        {
            // Clean up any actors stored in scenario context
            if (scenarioContext.TryGetValue("CurrentActor", out Framework.Screenplay.IActor actor))
            {
                if (actor is IAsyncDisposable asyncDisposableActor)
                {
                    await asyncDisposableActor.DisposeAsync();
                }
            }

            // Clean up any pages stored in scenario context
            if (scenarioContext.TryGetValue("CurrentPage", out Microsoft.Playwright.IPage page))
            {
                await page.CloseAsync();
            }

            // Clean up any browser contexts
            if (scenarioContext.TryGetValue("CurrentBrowserContext", out Microsoft.Playwright.IBrowserContext context))
            {
                await context.CloseAsync();
            }

            // Clean up any browsers
            if (scenarioContext.TryGetValue("CurrentBrowser", out Microsoft.Playwright.IBrowser browser))
            {
                await browser.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Error during scenario resource cleanup: {ScenarioTitle}", 
                scenarioContext.ScenarioInfo.Title);
        }
    }
}