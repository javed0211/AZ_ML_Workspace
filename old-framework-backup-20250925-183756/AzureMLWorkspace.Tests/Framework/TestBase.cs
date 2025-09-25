using AzureMLWorkspace.Tests.Framework.Configuration;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright.NUnit;
using Serilog;
using Serilog.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework;

/// <summary>
/// Base class for all test classes providing common setup and teardown
/// </summary>
[TestFixture]
public abstract class TestBase : PageTest
{
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected IConfiguration Configuration { get; private set; } = null!;
    protected TestConfiguration TestConfig { get; private set; } = null!;
    protected Microsoft.Extensions.Logging.ILogger Logger { get; private set; } = null!;

    [OneTimeSetUp]
    public virtual async Task OneTimeSetUp()
    {
        // Build configuration using unified helper
        Configuration = ConfigurationHelper.BuildConfiguration();
        TestConfig = TestConfiguration.LoadFromConfiguration(Configuration);

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(TestConfig.Logging.LogLevel))
            .WriteTo.Console()
            .WriteTo.File(
                TestConfig.Logging.LogFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: TestConfig.Logging.MaxLogFiles,
                fileSizeLimitBytes: TestConfig.Logging.MaxLogFileSizeMB * 1024 * 1024)
            .CreateLogger();

        // Build service collection
        var services = new ServiceCollection();
        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
        AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider = ServiceProvider;

        Logger = ServiceProvider.GetRequiredService<ILogger<TestBase>>();
        Logger.LogInformation("Test framework initialized");

        await Task.CompletedTask;
    }

    [OneTimeTearDown]
    public virtual async Task OneTimeTearDown()
    {
        Logger?.LogInformation("Test framework cleanup started");

        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Log.CloseAndFlush();
    }

    [SetUp]
    public virtual async Task SetUp()
    {
        Logger.LogInformation("Starting test: {TestName}", TestContext.CurrentContext.Test.Name);
        await Task.CompletedTask;
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        var testResult = TestContext.CurrentContext.Result.Outcome.Status;
        Logger.LogInformation("Test completed: {TestName} - {Result}", 
            TestContext.CurrentContext.Test.Name, testResult);

        // Capture screenshot on failure
        if (testResult == NUnit.Framework.Interfaces.TestStatus.Failed && TestConfig.Browser.CaptureScreenshots)
        {
            await CaptureScreenshotOnFailure();
        }
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Register configuration
        services.AddSingleton(Configuration);
        services.AddSingleton(TestConfig);

        // Register logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger);
        });

        // Register framework services
        services.AddTransient<Actor>();
        services.AddSingleton<ILoggerFactory, SerilogLoggerFactory>();

        // Register abilities and other services as needed
        // These will be created per test as needed
    }

    protected virtual async Task CaptureScreenshotOnFailure()
    {
        try
        {
            if (Page != null)
            {
                var screenshotPath = Path.Combine(
                    TestConfig.Reporting.ReportOutputPath,
                    "Screenshots",
                    $"{TestContext.CurrentContext.Test.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

                Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
                await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                
                Logger.LogInformation("Screenshot captured: {ScreenshotPath}", screenshotPath);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to capture screenshot");
        }
    }

    /// <summary>
    /// Creates a new actor with the specified name
    /// </summary>
    protected Actor CreateActor(string name)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<Actor>>();
        return Actor.Named(name, logger);
    }

    /// <summary>
    /// Gets a service from the dependency injection container
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a logger for the specified type
    /// </summary>
    protected ILogger<T> GetLogger<T>()
    {
        return ServiceProvider.GetRequiredService<ILogger<T>>();
    }
}