using Microsoft.Extensions.Configuration;

namespace AzureMLWorkspace.Tests.Framework.Configuration;

/// <summary>
/// Configuration settings for the test framework
/// </summary>
public class TestConfiguration
{
    public AzureConfiguration Azure { get; set; } = new();
    public AzureAISearchConfiguration AzureAISearch { get; set; } = new();
    public BrowserConfiguration Browser { get; set; } = new();
    public TestExecutionConfiguration TestExecution { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
    public ReportingConfiguration Reporting { get; set; } = new();
    public RetryConfiguration Retry { get; set; } = new();

    public static TestConfiguration LoadFromConfiguration(IConfiguration configuration)
    {
        var testConfig = new TestConfiguration();
        configuration.Bind(testConfig);
        return testConfig;
    }
}

public class AzureConfiguration
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ResourceGroup { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public string Region { get; set; } = "eastus";
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class AzureAISearchConfiguration
{
    public string ServiceName { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2023-11-01";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}

public class BrowserConfiguration
{
    public string DefaultBrowser { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public int DefaultTimeout { get; set; } = 30000;
    public ViewportConfiguration Viewport { get; set; } = new();
    public bool CaptureScreenshots { get; set; } = true;
    public bool CaptureVideos { get; set; } = false;
    public bool CaptureTraces { get; set; } = true;
    public int SlowMo { get; set; } = 0;
    public List<string> Args { get; set; } = new();
}

public class ViewportConfiguration
{
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
}

public class TestExecutionConfiguration
{
    public bool ParallelExecution { get; set; } = true;
    public int MaxDegreeOfParallelism { get; set; } = 4;
    public int DefaultTimeoutSeconds { get; set; } = 300;
    public bool ContinueOnFailure { get; set; } = false;
    public List<string> IncludedCategories { get; set; } = new();
    public List<string> ExcludedCategories { get; set; } = new();
}

public class LoggingConfiguration
{
    public string LogLevel { get; set; } = "Information";
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = true;
    public string LogFilePath { get; set; } = "logs/test-execution.log";
    public bool EnableStructuredLogging { get; set; } = true;
    public int MaxLogFileSizeMB { get; set; } = 100;
    public int MaxLogFiles { get; set; } = 10;
}

public class ReportingConfiguration
{
    public bool GenerateHtmlReport { get; set; } = true;
    public bool GenerateJsonReport { get; set; } = true;
    public bool GenerateAllureReport { get; set; } = false;
    public string ReportOutputPath { get; set; } = "TestResults";
    public bool IncludeScreenshots { get; set; } = true;
    public bool IncludeLogs { get; set; } = true;
    public bool OpenReportAfterExecution { get; set; } = false;
}

public class RetryConfiguration
{
    public int MaxRetries { get; set; } = 3;
    public int DelayBetweenRetriesMs { get; set; } = 1000;
    public double BackoffMultiplier { get; set; } = 2.0;
    public int MaxDelayMs { get; set; } = 10000;
    public List<string> RetryableExceptions { get; set; } = new()
    {
        "TimeoutException",
        "HttpRequestException",
        "SocketException",
        "TaskCanceledException"
    };
}