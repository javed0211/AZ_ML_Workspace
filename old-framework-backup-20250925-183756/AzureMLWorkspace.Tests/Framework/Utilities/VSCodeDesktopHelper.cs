using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Utilities;

public class VSCodeDesktopHelper
{
    private readonly ILogger<VSCodeDesktopHelper> _logger;
    private readonly string _nodeScriptPath;
    private readonly int _defaultTimeoutMs = 60000; // 60 seconds

    public VSCodeDesktopHelper(ILogger<VSCodeDesktopHelper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Path to the compiled TypeScript automation script
        _nodeScriptPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "VSCodeDesktop",
            "dist",
            "vscode-automation.js"
        );
    }

    public async Task<VSCodeAutomationResult> LaunchVSCodeAsync(string? workspacePath = null, int timeoutMs = 0)
    {
        var parameters = new VSCodeAutomationParams
        {
            Action = "launch",
            WorkspacePath = workspacePath,
            Timeout = timeoutMs > 0 ? timeoutMs : _defaultTimeoutMs
        };

        return await ExecuteNodeScriptAsync(parameters);
    }

    public async Task<VSCodeAutomationResult> CheckInteractivityAsync()
    {
        var parameters = new VSCodeAutomationParams
        {
            Action = "checkInteractivity"
        };

        return await ExecuteNodeScriptAsync(parameters);
    }

    public async Task<VSCodeAutomationResult> OpenWorkspaceAsync(string workspacePath)
    {
        var parameters = new VSCodeAutomationParams
        {
            Action = "openWorkspace",
            WorkspacePath = workspacePath
        };

        return await ExecuteNodeScriptAsync(parameters);
    }

    public async Task<VSCodeAutomationResult> ConnectToComputeAsync(string computeName)
    {
        var parameters = new VSCodeAutomationParams
        {
            Action = "connectToCompute",
            ComputeName = computeName
        };

        return await ExecuteNodeScriptAsync(parameters);
    }

    public async Task<VSCodeAutomationResult> CheckApplicationLinksAsync()
    {
        var parameters = new VSCodeAutomationParams
        {
            Action = "checkApplicationLinks"
        };

        return await ExecuteNodeScriptAsync(parameters);
    }

    public async Task<VSCodeAutomationResult> TakeScreenshotAsync(string? filename = null)
    {
        var parameters = new VSCodeAutomationParams
        {
            Action = "takeScreenshot",
            Filename = filename
        };

        return await ExecuteNodeScriptAsync(parameters);
    }

    public async Task<VSCodeAutomationResult> CloseVSCodeAsync()
    {
        var parameters = new VSCodeAutomationParams
        {
            Action = "close"
        };

        return await ExecuteNodeScriptAsync(parameters);
    }

    private async Task<VSCodeAutomationResult> ExecuteNodeScriptAsync(VSCodeAutomationParams parameters)
    {
        try
        {
            // Ensure the TypeScript is compiled
            await EnsureTypeScriptCompiledAsync();

            var parametersJson = JsonSerializer.Serialize(parameters, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogInformation("Executing VS Code automation: {Action}", parameters.Action);
            _logger.LogDebug("Parameters: {Parameters}", parametersJson);

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = $"\"{_nodeScriptPath}\" \"{parametersJson.Replace("\"", "\\\"")}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var timeoutTask = Task.Delay(_defaultTimeoutMs);
            var processTask = Task.Run(() => process.WaitForExit());

            var completedTask = await Task.WhenAny(processTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                _logger.LogWarning("VS Code automation timed out for action: {Action}", parameters.Action);
                
                try
                {
                    process.Kill();
                }
                catch (Exception killEx)
                {
                    _logger.LogWarning(killEx, "Failed to kill timed out process");
                }

                return new VSCodeAutomationResult
                {
                    Success = false,
                    Message = $"VS Code automation timed out after {_defaultTimeoutMs}ms",
                    Error = "Timeout"
                };
            }

            var output = outputBuilder.ToString().Trim();
            var error = errorBuilder.ToString().Trim();

            _logger.LogDebug("Node script output: {Output}", output);
            
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Node script error: {Error}", error);
            }

            // Try to parse the JSON result
            if (!string.IsNullOrEmpty(output))
            {
                try
                {
                    var result = JsonSerializer.Deserialize<VSCodeAutomationResult>(output, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (result != null)
                    {
                        _logger.LogInformation("VS Code automation completed: {Action} - Success: {Success}", 
                            parameters.Action, result.Success);
                        return result;
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogWarning(jsonEx, "Failed to parse JSON result from Node script");
                }
            }

            // Fallback result based on exit code
            var success = process.ExitCode == 0;
            return new VSCodeAutomationResult
            {
                Success = success,
                Message = success ? "VS Code automation completed" : "VS Code automation failed",
                Error = !success ? error : null,
                Data = !string.IsNullOrEmpty(output) ? output : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute VS Code automation for action: {Action}", parameters.Action);
            
            return new VSCodeAutomationResult
            {
                Success = false,
                Message = "Failed to execute VS Code automation",
                Error = ex.Message
            };
        }
    }

    private async Task EnsureTypeScriptCompiledAsync()
    {
        var vsCodeDesktopDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VSCodeDesktop");
        var distDir = Path.Combine(vsCodeDesktopDir, "dist");
        var compiledScript = Path.Combine(distDir, "vscode-automation.js");

        // Check if compiled script exists and is newer than source
        var sourceScript = Path.Combine(vsCodeDesktopDir, "src", "vscode-automation.ts");
        
        if (File.Exists(compiledScript) && File.Exists(sourceScript))
        {
            var compiledTime = File.GetLastWriteTime(compiledScript);
            var sourceTime = File.GetLastWriteTime(sourceScript);
            
            if (compiledTime >= sourceTime)
            {
                return; // Already compiled and up to date
            }
        }

        _logger.LogInformation("Compiling TypeScript automation script...");

        try
        {
            // Install dependencies if needed
            var packageJsonPath = Path.Combine(vsCodeDesktopDir, "package.json");
            var nodeModulesPath = Path.Combine(vsCodeDesktopDir, "node_modules");
            
            if (File.Exists(packageJsonPath) && !Directory.Exists(nodeModulesPath))
            {
                await RunCommandAsync("npm", "install", vsCodeDesktopDir);
            }

            // Compile TypeScript
            await RunCommandAsync("npm", "run build", vsCodeDesktopDir);
            
            _logger.LogInformation("TypeScript compilation completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile TypeScript automation script");
            throw new InvalidOperationException("Failed to compile VS Code automation script", ex);
        }
    }

    private async Task RunCommandAsync(string command, string arguments, string workingDirectory)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Command failed: {command} {arguments}\nError: {error}\nOutput: {output}");
        }
    }
}

public class VSCodeAutomationParams
{
    public string Action { get; set; } = string.Empty;
    public string? WorkspacePath { get; set; }
    public string? ComputeName { get; set; }
    public string? Filename { get; set; }
    public int? Timeout { get; set; }
    public bool? Headless { get; set; }
}

public class VSCodeAutomationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public string? Error { get; set; }
}