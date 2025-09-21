using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Utilities;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Abilities;

public class UseVSCodeDesktop : IAbility, IAsyncDisposable
{
    private readonly VSCodeDesktopHelper _helper;
    private readonly ILogger<UseVSCodeDesktop> _logger;
    private bool _isLaunched = false;

    private UseVSCodeDesktop(VSCodeDesktopHelper helper, ILogger<UseVSCodeDesktop> logger)
    {
        _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static UseVSCodeDesktop With(VSCodeDesktopHelper helper)
    {
        var logger = TestContext.ServiceProvider.GetRequiredService<ILogger<UseVSCodeDesktop>>();
        return new UseVSCodeDesktop(helper, logger);
    }

    public async Task<VSCodeAutomationResult> LaunchAsync(string? workspacePath = null, int timeoutMs = 0)
    {
        _logger.LogInformation("Launching VS Code Desktop");
        
        var result = await _helper.LaunchVSCodeAsync(workspacePath, timeoutMs);
        
        if (result.Success)
        {
            _isLaunched = true;
            _logger.LogInformation("VS Code Desktop launched successfully");
        }
        else
        {
            _logger.LogError("Failed to launch VS Code Desktop: {Message}", result.Message);
        }

        return result;
    }

    public async Task<VSCodeAutomationResult> CheckInteractivityAsync()
    {
        if (!_isLaunched)
        {
            throw new InvalidOperationException("VS Code is not launched. Call LaunchAsync first.");
        }

        _logger.LogInformation("Checking VS Code Desktop interactivity");
        return await _helper.CheckInteractivityAsync();
    }

    public async Task<VSCodeAutomationResult> OpenWorkspaceAsync(string workspacePath)
    {
        if (!_isLaunched)
        {
            throw new InvalidOperationException("VS Code is not launched. Call LaunchAsync first.");
        }

        _logger.LogInformation("Opening workspace in VS Code: {WorkspacePath}", workspacePath);
        return await _helper.OpenWorkspaceAsync(workspacePath);
    }

    public async Task<VSCodeAutomationResult> ConnectToComputeAsync(string computeName)
    {
        if (!_isLaunched)
        {
            throw new InvalidOperationException("VS Code is not launched. Call LaunchAsync first.");
        }

        _logger.LogInformation("Connecting to compute in VS Code: {ComputeName}", computeName);
        return await _helper.ConnectToComputeAsync(computeName);
    }

    public async Task<VSCodeAutomationResult> CheckApplicationLinksAsync()
    {
        if (!_isLaunched)
        {
            throw new InvalidOperationException("VS Code is not launched. Call LaunchAsync first.");
        }

        _logger.LogInformation("Checking application links in VS Code");
        return await _helper.CheckApplicationLinksAsync();
    }

    public async Task<VSCodeAutomationResult> TakeScreenshotAsync(string? filename = null)
    {
        if (!_isLaunched)
        {
            throw new InvalidOperationException("VS Code is not launched. Call LaunchAsync first.");
        }

        _logger.LogInformation("Taking screenshot of VS Code");
        return await _helper.TakeScreenshotAsync(filename);
    }

    public async Task<VSCodeAutomationResult> CloseAsync()
    {
        if (_isLaunched)
        {
            _logger.LogInformation("Closing VS Code Desktop");
            var result = await _helper.CloseVSCodeAsync();
            _isLaunched = false;
            return result;
        }

        return new VSCodeAutomationResult
        {
            Success = true,
            Message = "VS Code was not launched"
        };
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during VS Code Desktop disposal");
        }
    }
}