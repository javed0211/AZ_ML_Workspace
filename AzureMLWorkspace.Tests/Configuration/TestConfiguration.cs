using Microsoft.Extensions.Configuration;

namespace AzureMLWorkspace.Tests.Configuration;

public class TestConfiguration
{
    private readonly IConfiguration _configuration;

    public TestConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.test.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();
    }

    public string BaseUrl => _configuration["BaseUrl"] ?? "https://ml.azure.com";
    public string AzureSubscriptionId => _configuration["Azure:SubscriptionId"] ?? "";
    public string AzureResourceGroup => _configuration["Azure:ResourceGroup"] ?? "";
    public string AzureWorkspaceName => _configuration["Azure:WorkspaceName"] ?? "";
    public string AzureTenantId => _configuration["Azure:TenantId"] ?? "";
    public string AzureClientId => _configuration["Azure:ClientId"] ?? "";
    public string AzureClientSecret => _configuration["Azure:ClientSecret"] ?? "";
    
    public int DefaultTimeout => int.Parse(_configuration["DefaultTimeout"] ?? "30000");
    public bool HeadlessMode => bool.Parse(_configuration["HeadlessMode"] ?? "true");
    public string BrowserType => _configuration["BrowserType"] ?? "chromium";
    public bool SlowMo => bool.Parse(_configuration["SlowMo"] ?? "false");
    public int SlowMoDelay => int.Parse(_configuration["SlowMoDelay"] ?? "100");
    
    public string TestDataPath => _configuration["TestDataPath"] ?? GetCrossPlatformPath("TestData");
    public string ScreenshotsPath => _configuration["ScreenshotsPath"] ?? GetCrossPlatformPath("Screenshots");
    public string VideosPath => _configuration["VideosPath"] ?? GetCrossPlatformPath("Videos");
    public string TracesPath => _configuration["TracesPath"] ?? GetCrossPlatformPath("Traces");
    
    public bool CaptureScreenshots => bool.Parse(_configuration["CaptureScreenshots"] ?? "true");
    public bool CaptureVideos => bool.Parse(_configuration["CaptureVideos"] ?? "false");
    public bool CaptureTraces => bool.Parse(_configuration["CaptureTraces"] ?? "true");
    
    public string LogLevel => _configuration["LogLevel"] ?? "Information";
    public string LogPath => _configuration["LogPath"] ?? GetCrossPlatformPath("Logs");

    private static string GetCrossPlatformPath(string folderName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), folderName);
    }
}