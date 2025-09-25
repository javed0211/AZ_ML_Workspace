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

        // For maximized windows, disable viewport to use full browser window size
        var contextOptions = _contextOptions;
        if (_launchOptions.Args?.Contains("--start-maximized") == true)
        {
            // Create new context options with viewport disabled for maximized windows
            contextOptions = new BrowserNewContextOptions
            {
                ViewportSize = null, // This is key - null viewport uses the full browser window
                UserAgent = _contextOptions.UserAgent,
                IsMobile = _contextOptions.IsMobile,
                HasTouch = _contextOptions.HasTouch,
                DeviceScaleFactor = _contextOptions.DeviceScaleFactor,
                Locale = _contextOptions.Locale,
                TimezoneId = _contextOptions.TimezoneId,
                Geolocation = _contextOptions.Geolocation,
                Permissions = _contextOptions.Permissions,
                ExtraHTTPHeaders = _contextOptions.ExtraHTTPHeaders,
                HttpCredentials = _contextOptions.HttpCredentials,
                ColorScheme = _contextOptions.ColorScheme,
                ReducedMotion = _contextOptions.ReducedMotion,
                ForcedColors = _contextOptions.ForcedColors,
                AcceptDownloads = _contextOptions.AcceptDownloads,
                BypassCSP = _contextOptions.BypassCSP,
                IgnoreHTTPSErrors = _contextOptions.IgnoreHTTPSErrors,
                JavaScriptEnabled = _contextOptions.JavaScriptEnabled,
                Offline = _contextOptions.Offline,
                RecordHarPath = _contextOptions.RecordHarPath,
                RecordVideoDir = _contextOptions.RecordVideoDir,
                RecordVideoSize = _contextOptions.RecordVideoSize,
                StorageState = _contextOptions.StorageState,
                StorageStatePath = _contextOptions.StorageStatePath,
                BaseURL = _contextOptions.BaseURL,
                StrictSelectors = _contextOptions.StrictSelectors,
                ServiceWorkers = _contextOptions.ServiceWorkers
            };
        }

        _context = await _browser.NewContextAsync(contextOptions);
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
        return new BrowseTheWeb(logger,
            new BrowserTypeLaunchOptions { Args = new[] { "--start-maximized" } },
            new BrowserNewContextOptions { ViewportSize = null });
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
        return new BrowseTheWeb(logger,
            new BrowserTypeLaunchOptions { Headless = true, Args = new[] { "--start-maximized" } },
            new BrowserNewContextOptions { ViewportSize = null });
    }

    /// <summary>
    /// Creates a headed browser ability for debugging
    /// </summary>
    public static BrowseTheWeb Visibly(ILogger<BrowseTheWeb> logger)
    {
        return new BrowseTheWeb(logger,
            new BrowserTypeLaunchOptions { Headless = false, SlowMo = 100, Args = new[] { "--start-maximized" } },
            new BrowserNewContextOptions { ViewportSize = null });
    }

    /// <summary>
    /// Creates a maximized browser ability with optimized startup
    /// </summary>
    public static BrowseTheWeb Maximized(ILogger<BrowseTheWeb> logger)
    {
        return new BrowseTheWeb(logger,
            new BrowserTypeLaunchOptions 
            { 
                Headless = false, 
                Args = new[] 
                { 
                    "--start-maximized",
                    "--no-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-extensions",
                    "--disable-gpu",
                    "--disable-background-timer-throttling",
                    "--disable-backgrounding-occluded-windows",
                    "--disable-renderer-backgrounding"
                } 
            },
            new BrowserNewContextOptions { ViewportSize = null });
    }

    /// <summary>
    /// Creates a browser ability with specific viewport size
    /// </summary>
    public static BrowseTheWeb WithViewport(ILogger<BrowseTheWeb> logger, int width, int height)
    {
        return new BrowseTheWeb(logger,
            new BrowserTypeLaunchOptions { Args = new[] { "--start-maximized" } },
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
        return new BrowseTheWeb(logger, new BrowserTypeLaunchOptions { Args = new[] { "--start-maximized" } }, mobileOptions);
    }

    /// <summary>
    /// Creates a browser ability using test configuration settings
    /// </summary>
    public static BrowseTheWeb UsingTestConfig(ILogger<BrowseTheWeb> logger, Configuration.TestConfiguration testConfig)
    {
        var args = new List<string>();
        
        // Always include maximized for better test visibility
        if (!args.Contains("--start-maximized"))
        {
            args.Add("--start-maximized");
        }
        
        // Add configured args
        if (testConfig.Browser.Args?.Any() == true)
        {
            args.AddRange(testConfig.Browser.Args);
        }

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = testConfig.Browser.Headless,
            SlowMo = testConfig.Browser.SlowMo,
            Args = args.Distinct().ToArray() // Remove duplicates
        };

        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = testConfig.Browser.Headless ? 
                new ViewportSize { Width = testConfig.Browser.Viewport.Width, Height = testConfig.Browser.Viewport.Height } : 
                null // null viewport for maximized windows
        };

        return new BrowseTheWeb(logger, launchOptions, contextOptions);
    }
}