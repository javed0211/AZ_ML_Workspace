using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AzureMLWorkspace.Tests.Framework.Abilities;

/// <summary>
/// Ability to browse the web using Playwright
/// </summary>
public class BrowseTheWeb : IAbility, IAsyncDisposable
{
    private readonly ILogger<BrowseTheWeb> _logger;
    private readonly BrowserTypeLaunchOptions _launchOptions;
    private readonly BrowserNewContextOptions _contextOptions;
    
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    public string Name => "Browse the Web";
    public IPage Page => _page ?? throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");
    public IBrowserContext Context => _context ?? throw new InvalidOperationException("Browser context not initialized.");
    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized.");

    private BrowseTheWeb(ILogger<BrowseTheWeb> logger, BrowserTypeLaunchOptions launchOptions, BrowserNewContextOptions contextOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _launchOptions = launchOptions ?? new BrowserTypeLaunchOptions();
        _contextOptions = contextOptions ?? new BrowserNewContextOptions();
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing browser for web browsing");
        
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(_launchOptions);
        _context = await _browser.NewContextAsync(_contextOptions);
        _page = await _context.NewPageAsync();
        
        // Set up page event handlers
        _page.Console += (_, e) => _logger.LogDebug("Browser console: {Message}", e.Text);
        _page.PageError += (_, e) => _logger.LogWarning("Browser error: {Error}", e);
        _page.RequestFailed += (_, e) => _logger.LogWarning("Request failed: {Url} - {Failure}", e.Url, e.Failure);
        
        _logger.LogInformation("Browser initialized successfully");
    }

    public async Task CleanupAsync()
    {
        _logger.LogInformation("Cleaning up browser resources");
        
        if (_page != null)
        {
            await _page.CloseAsync();
            _page = null;
        }
        
        if (_context != null)
        {
            await _context.CloseAsync();
            _context = null;
        }
        
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }
        
        _logger.LogInformation("Browser cleanup completed");
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupAsync();
    }

    /// <summary>
    /// Creates a new BrowseTheWeb ability with default options
    /// </summary>
    public static BrowseTheWeb With(ILogger<BrowseTheWeb> logger)
    {
        return new BrowseTheWeb(logger, new BrowserTypeLaunchOptions(), new BrowserNewContextOptions());
    }

    /// <summary>
    /// Creates a new BrowseTheWeb ability with custom launch options
    /// </summary>
    public static BrowseTheWeb With(ILogger<BrowseTheWeb> logger, BrowserTypeLaunchOptions launchOptions)
    {
        return new BrowseTheWeb(logger, launchOptions, new BrowserNewContextOptions());
    }

    /// <summary>
    /// Creates a new BrowseTheWeb ability with custom launch and context options
    /// </summary>
    public static BrowseTheWeb With(ILogger<BrowseTheWeb> logger, BrowserTypeLaunchOptions launchOptions, BrowserNewContextOptions contextOptions)
    {
        return new BrowseTheWeb(logger, launchOptions, contextOptions);
    }

    /// <summary>
    /// Creates a headless browser ability
    /// </summary>
    public static BrowseTheWeb Headlessly(ILogger<BrowseTheWeb> logger)
    {
        return new BrowseTheWeb(logger, new BrowserTypeLaunchOptions { Headless = true }, new BrowserNewContextOptions());
    }

    /// <summary>
    /// Creates a headed browser ability for debugging
    /// </summary>
    public static BrowseTheWeb Visibly(ILogger<BrowseTheWeb> logger)
    {
        return new BrowseTheWeb(logger, 
            new BrowserTypeLaunchOptions { Headless = false, SlowMo = 100 }, 
            new BrowserNewContextOptions());
    }

    /// <summary>
    /// Creates a browser ability with specific viewport size
    /// </summary>
    public static BrowseTheWeb WithViewport(ILogger<BrowseTheWeb> logger, int width, int height)
    {
        return new BrowseTheWeb(logger, 
            new BrowserTypeLaunchOptions(), 
            new BrowserNewContextOptions 
            { 
                ViewportSize = new ViewportSize { Width = width, Height = height } 
            });
    }

    /// <summary>
    /// Creates a mobile browser ability
    /// </summary>
    public static BrowseTheWeb OnMobile(ILogger<BrowseTheWeb> logger, string deviceName = "iPhone 13")
    {
        // Create mobile viewport settings manually since Playwright.Devices is not available
        var mobileOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 390, Height = 844 }, // iPhone 13 dimensions
            UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1",
            IsMobile = true,
            HasTouch = true
        };
        return new BrowseTheWeb(logger, new BrowserTypeLaunchOptions(), mobileOptions);
    }
}