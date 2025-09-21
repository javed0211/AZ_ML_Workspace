using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Utilities;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

public class StartVSCodeDesktop : ITask
{
    private readonly string? _workspacePath;
    private readonly ILogger<StartVSCodeDesktop> _logger;

    private StartVSCodeDesktop(string? workspacePath, ILogger<StartVSCodeDesktop> logger)
    {
        _workspacePath = workspacePath;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static StartVSCodeDesktop Now()
    {
        var logger = TestContext.ServiceProvider.GetRequiredService<ILogger<StartVSCodeDesktop>>();
        return new StartVSCodeDesktop(null, logger);
    }

    public static StartVSCodeDesktop WithWorkspace(string workspacePath)
    {
        var logger = TestContext.ServiceProvider.GetRequiredService<ILogger<StartVSCodeDesktop>>();
        return new StartVSCodeDesktop(workspacePath, logger);
    }

    public async Task<T> PerformAs<T>(IActor actor) where T : IActor
    {
        _logger.LogInformation("Starting VS Code Desktop{WorkspacePath}", 
            !string.IsNullOrEmpty(_workspacePath) ? $" with workspace: {_workspacePath}" : "");

        try
        {
            // Get or create VS Code Desktop ability
            var vsCodeAbility = actor.GetAbility<UseVSCodeDesktop>();
            if (vsCodeAbility == null)
            {
                var vsCodeHelper = TestContext.ServiceProvider.GetRequiredService<VSCodeDesktopHelper>();
                vsCodeAbility = UseVSCodeDesktop.With(vsCodeHelper);
                actor.Can(vsCodeAbility);
            }

            // Launch VS Code
            var result = await vsCodeAbility.LaunchAsync(_workspacePath);
            
            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed to launch VS Code: {result.Message}");
            }

            _logger.LogInformation("Successfully started VS Code Desktop");
            return (T)actor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start VS Code Desktop");
            throw;
        }
    }
}