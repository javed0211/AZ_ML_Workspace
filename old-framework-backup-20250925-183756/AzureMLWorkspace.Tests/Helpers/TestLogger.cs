using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using AzureMLWorkspace.Tests.Configuration;

namespace AzureMLWorkspace.Tests.Helpers;

public class TestLogger
{
    private readonly ILogger<TestLogger> _logger;
    private readonly TestConfiguration _config;

    public TestLogger(TestConfiguration config)
    {
        _config = config;
        
        // Ensure log directory exists
        Directory.CreateDirectory(_config.LogPath);
        
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(GetSerilogLevel(_config.LogLevel))
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(_config.LogPath, "test-log-.txt"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        var loggerFactory = new SerilogLoggerFactory(Log.Logger);
        _logger = loggerFactory.CreateLogger<TestLogger>();
    }

    public void Info(string message) => _logger.LogInformation(message);
    public void Debug(string message) => _logger.LogDebug(message);
    public void Warning(string message) => _logger.LogWarning(message);
    public void Error(string message) => _logger.LogError(message);
    public void Error(Exception exception, string message) => _logger.LogError(exception, message);

    public void LogTestStart(string testName)
    {
        _logger.LogInformation("=== Starting Test: {TestName} ===", testName);
    }

    public void LogTestEnd(string testName, bool passed)
    {
        var status = passed ? "PASSED" : "FAILED";
        _logger.LogInformation("=== Test {Status}: {TestName} ===", status, testName);
    }

    public void LogStep(string stepDescription)
    {
        _logger.LogInformation("Step: {StepDescription}", stepDescription);
    }

    public void LogScreenshot(string screenshotPath)
    {
        _logger.LogInformation("Screenshot captured: {ScreenshotPath}", screenshotPath);
    }

    private static Serilog.Events.LogEventLevel GetSerilogLevel(string level)
    {
        return level.ToLower() switch
        {
            "debug" => Serilog.Events.LogEventLevel.Debug,
            "information" => Serilog.Events.LogEventLevel.Information,
            "warning" => Serilog.Events.LogEventLevel.Warning,
            "error" => Serilog.Events.LogEventLevel.Error,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }
}