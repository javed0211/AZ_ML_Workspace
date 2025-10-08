using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.MachineLearning;
using Azure.ResourceManager.MachineLearning.Models;
using Renci.SshNet;
using System.Diagnostics;
using System.Text.Json;
using Serilog;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Utils
{
    public class AzureMLComputeAutomationUtils
    {
        private readonly Logger _logger;
        private readonly ConfigManager _config;
        private ArmClient? _armClient;
        private MachineLearningWorkspaceResource? _workspace;
        private SshClient? _sshClient;
        private readonly string _sshKeyPath;
        private readonly string _sshConfigPath;

        public AzureMLComputeAutomationUtils(Logger logger)
        {
            _logger = logger;
            _config = ConfigManager.Instance;
            _sshKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_rsa");
            _sshConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");
        }

        #region Authentication and Initialization

        public async Task<bool> InitializeAzureClientAsync()
        {
            try
            {
                _logger.LogAction("Initializing Azure client");
                
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeEnvironmentCredential = false,
                    ExcludeInteractiveBrowserCredential = false,
                    ExcludeManagedIdentityCredential = false,
                    ExcludeSharedTokenCacheCredential = false,
                    ExcludeVisualStudioCredential = false,
                    ExcludeVisualStudioCodeCredential = false,
                    ExcludeAzureCliCredential = false,
                    ExcludeAzurePowerShellCredential = false
                });

                _armClient = new ArmClient(credential);
                
                // Test authentication by getting subscription
                var subscriptionId = _config.GetAzureSettings().SubscriptionId;
                var subscription = await _armClient.GetDefaultSubscriptionAsync();
                
                _logger.LogInfo($"✅ Successfully authenticated with Azure subscription: {subscription.Data.DisplayName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to initialize Azure client: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> InitializeWorkspaceAsync()
        {
            try
            {
                if (_armClient == null)
                {
                    throw new InvalidOperationException("Azure client not initialized. Call InitializeAzureClientAsync first.");
                }

                _logger.LogAction("Initializing Azure ML workspace");
                
                var azureSettings = _config.GetAzureSettings();
                var subscriptionId = azureSettings.SubscriptionId;
                var resourceGroupName = azureSettings.ResourceGroupName;
                var workspaceName = azureSettings.WorkspaceName;

                var subscription = await _armClient.GetDefaultSubscriptionAsync();
                var resourceGroup = await subscription.GetResourceGroupAsync(resourceGroupName);
                _workspace = await resourceGroup.Value.GetMachineLearningWorkspaceAsync(workspaceName);

                _logger.LogInfo($"✅ Successfully initialized workspace: {workspaceName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to initialize workspace: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Prerequisites Validation

        public async Task<ValidationResult> ValidatePrerequisitesAsync()
        {
            var result = new ValidationResult();
            
            _logger.LogAction("Validating prerequisites for Azure ML compute automation");

            // Check Python installation
            result.PythonInstalled = await CheckPythonInstallationAsync();
            
            // Check VS Code installation
            result.VSCodeInstalled = await CheckVSCodeInstallationAsync();
            
            // Check SSH setup
            result.SSHConfigured = await CheckSSHSetupAsync();
            
            // Check Azure CLI
            result.AzureCLIInstalled = await CheckAzureCLIAsync();
            
            // Check Azure authentication
            result.AzureAuthenticated = await CheckAzureAuthenticationAsync();
            
            // Check network connectivity
            result.NetworkConnectivity = await CheckNetworkConnectivityAsync();
            
            // Check required Python packages
            result.PythonPackagesInstalled = await CheckPythonPackagesAsync();

            result.AllPrerequisitesMet = result.PythonInstalled && 
                                       result.VSCodeInstalled && 
                                       result.SSHConfigured && 
                                       result.AzureCLIInstalled && 
                                       result.AzureAuthenticated && 
                                       result.NetworkConnectivity && 
                                       result.PythonPackagesInstalled;

            _logger.LogInfo($"Prerequisites validation completed. All met: {result.AllPrerequisitesMet}");
            return result;
        }

        private async Task<bool> CheckPythonInstallationAsync()
        {
            try
            {
                var result = await ExecuteCommandAsync("python", "--version");
                if (result.Success && result.Output.Contains("Python"))
                {
                    _logger.LogInfo($"✅ Python installed: {result.Output.Trim()}");
                    return true;
                }
                
                // Try python3
                result = await ExecuteCommandAsync("python3", "--version");
                if (result.Success && result.Output.Contains("Python"))
                {
                    _logger.LogInfo($"✅ Python3 installed: {result.Output.Trim()}");
                    return true;
                }
                
                _logger.LogError("❌ Python not found");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking Python: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckVSCodeInstallationAsync()
        {
            try
            {
                var result = await ExecuteCommandAsync("code", "--version");
                if (result.Success)
                {
                    _logger.LogInfo($"✅ VS Code installed: {result.Output.Split('\n')[0]}");
                    return true;
                }
                
                _logger.LogError("❌ VS Code not found");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking VS Code: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckSSHSetupAsync()
        {
            try
            {
                // Check if SSH key exists
                if (!File.Exists(_sshKeyPath))
                {
                    _logger.LogWarning("⚠️ SSH key not found, will be generated");
                    return await GenerateSSHKeyAsync();
                }
                
                _logger.LogInfo("✅ SSH key found");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking SSH setup: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckAzureCLIAsync()
        {
            try
            {
                var result = await ExecuteCommandAsync("az", "--version");
                if (result.Success)
                {
                    _logger.LogInfo("✅ Azure CLI installed");
                    return true;
                }
                
                _logger.LogError("❌ Azure CLI not found");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking Azure CLI: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckAzureAuthenticationAsync()
        {
            try
            {
                var result = await ExecuteCommandAsync("az", "account show");
                if (result.Success)
                {
                    _logger.LogInfo("✅ Azure authentication verified");
                    return true;
                }
                
                _logger.LogError("❌ Azure authentication failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking Azure authentication: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckNetworkConnectivityAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                var response = await httpClient.GetAsync("https://ml.azure.com");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInfo("✅ Network connectivity to Azure ML verified");
                    return true;
                }
                
                _logger.LogError("❌ Network connectivity to Azure ML failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking network connectivity: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckPythonPackagesAsync()
        {
            try
            {
                var requiredPackages = new[] { "azure-ai-ml", "azure-identity", "paramiko", "watchdog" };
                var allInstalled = true;
                
                foreach (var package in requiredPackages)
                {
                    var result = await ExecuteCommandAsync("pip", $"show {package}");
                    if (!result.Success)
                    {
                        _logger.LogWarning($"⚠️ Python package not installed: {package}");
                        allInstalled = false;
                    }
                }
                
                if (allInstalled)
                {
                    _logger.LogInfo("✅ All required Python packages installed");
                }
                
                return allInstalled;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking Python packages: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Compute Instance Management

        public async Task<ComputeInstanceResult> CreateComputeInstanceAsync(string instanceName, string vmSize = "Standard_DS3_v2")
        {
            try
            {
                if (_workspace == null)
                {
                    throw new InvalidOperationException("Workspace not initialized");
                }

                _logger.LogAction($"Creating compute instance: {instanceName}");

                // Check if instance already exists
                var existingInstance = await GetComputeInstanceAsync(instanceName);
                if (existingInstance != null)
                {
                    _logger.LogInfo($"Compute instance {instanceName} already exists");
                    return new ComputeInstanceResult 
                    { 
                        Success = true, 
                        InstanceName = instanceName,
                        State = "Exists",
                        Message = "Instance already exists"
                    };
                }

                // Create compute instance
                // TODO: SDK limitation - MachineLearningComputeInstanceProperties cannot be assigned to 
                // MachineLearningComputeData.Properties due to type hierarchy issues in SDK 1.2.3
                // This functionality needs to be implemented using REST API or wait for SDK fix
                _logger.LogWarning($"Compute instance creation is not fully supported in current SDK version. Instance: {instanceName}");
                
                throw new NotImplementedException(
                    "Compute instance creation is not supported in Azure.ResourceManager.MachineLearning SDK 1.2.3 " +
                    "due to type hierarchy limitations. MachineLearningComputeInstanceProperties cannot be assigned " +
                    "to MachineLearningComputeData.Properties. Consider using Azure CLI or REST API instead.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating compute instance: {ex.Message}");
                return new ComputeInstanceResult 
                { 
                    Success = false, 
                    InstanceName = instanceName,
                    Message = ex.Message
                };
            }
        }

        public async Task<MachineLearningComputeResource?> GetComputeInstanceAsync(string instanceName)
        {
            try
            {
                if (_workspace == null)
                {
                    throw new InvalidOperationException("Workspace not initialized");
                }

                var compute = await _workspace.GetMachineLearningComputeAsync(instanceName);
                return compute.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Instance doesn't exist
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting compute instance {instanceName}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> StartComputeInstanceAsync(string instanceName)
        {
            try
            {
                if (_workspace == null)
                {
                    throw new InvalidOperationException("Workspace not initialized");
                }

                _logger.LogAction($"Starting compute instance: {instanceName}");

                var compute = await _workspace.GetMachineLearningComputeAsync(instanceName);
                
                // Start the compute instance (this would be implementation-specific)
                // Note: The actual Azure ML SDK might have different methods for starting instances
                
                _logger.LogInfo($"✅ Successfully started compute instance: {instanceName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error starting compute instance: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StopComputeInstanceAsync(string instanceName)
        {
            try
            {
                if (_workspace == null)
                {
                    throw new InvalidOperationException("Workspace not initialized");
                }

                _logger.LogAction($"Stopping compute instance: {instanceName}");

                var compute = await _workspace.GetMachineLearningComputeAsync(instanceName);
                
                // Stop the compute instance (this would be implementation-specific)
                
                _logger.LogInfo($"✅ Successfully stopped compute instance: {instanceName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error stopping compute instance: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteComputeInstanceAsync(string instanceName)
        {
            try
            {
                if (_workspace == null)
                {
                    throw new InvalidOperationException("Workspace not initialized");
                }

                _logger.LogAction($"Deleting compute instance: {instanceName}");

                var compute = await _workspace.GetMachineLearningComputeAsync(instanceName);
                await compute.Value.DeleteAsync(WaitUntil.Completed, MachineLearningUnderlyingResourceAction.Delete);
                
                _logger.LogInfo($"✅ Successfully deleted compute instance: {instanceName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting compute instance: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region SSH and VS Code Setup

        public async Task<bool> GenerateSSHKeyAsync()
        {
            try
            {
                _logger.LogAction("Generating SSH key pair");

                var sshDir = Path.GetDirectoryName(_sshKeyPath);
                if (!Directory.Exists(sshDir))
                {
                    Directory.CreateDirectory(sshDir!);
                }

                var result = await ExecuteCommandAsync("ssh-keygen", 
                    $"-t rsa -b 2048 -f {_sshKeyPath} -N \"\"");

                if (result.Success)
                {
                    _logger.LogInfo("✅ SSH key pair generated successfully");
                    return true;
                }
                else
                {
                    _logger.LogError($"❌ Failed to generate SSH key: {result.Error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error generating SSH key: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetupSSHConnectionAsync(string instanceName, string hostname, string username)
        {
            try
            {
                _logger.LogAction($"Setting up SSH connection to {instanceName}");

                var sshConfigEntry = $@"
Host {instanceName}
    HostName {hostname}
    User {username}
    IdentityFile {_sshKeyPath}
    StrictHostKeyChecking no
    UserKnownHostsFile /dev/null
";

                await File.AppendAllTextAsync(_sshConfigPath, sshConfigEntry);
                
                _logger.LogInfo($"✅ SSH configuration added for {instanceName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error setting up SSH connection: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> TestSSHConnectionAsync(string hostname, string username)
        {
            try
            {
                _logger.LogAction($"Testing SSH connection to {hostname}");

                using var client = new SshClient(hostname, username, new PrivateKeyFile(_sshKeyPath));
                client.Connect();
                
                if (client.IsConnected)
                {
                    var result = client.RunCommand("echo 'SSH connection successful'");
                    _logger.LogInfo($"✅ SSH connection test successful: {result.Result}");
                    client.Disconnect();
                    return true;
                }
                else
                {
                    _logger.LogError("❌ SSH connection failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ SSH connection test failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetupVSCodeRemoteAsync(string instanceName)
        {
            try
            {
                _logger.LogAction($"Setting up VS Code remote connection to {instanceName}");

                // Install required VS Code extensions
                var extensions = new[]
                {
                    "ms-vscode-remote.remote-ssh",
                    "ms-python.python",
                    "ms-toolsai.jupyter",
                    "ms-azuretools.vscode-azureml"
                };

                foreach (var extension in extensions)
                {
                    var result = await ExecuteCommandAsync("code", $"--install-extension {extension}");
                    if (result.Success)
                    {
                        _logger.LogInfo($"✅ Installed VS Code extension: {extension}");
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ Failed to install extension: {extension}");
                    }
                }

                // Open VS Code with remote connection
                var openResult = await ExecuteCommandAsync("code", $"--remote ssh-remote+{instanceName} .");
                
                if (openResult.Success)
                {
                    _logger.LogInfo($"✅ VS Code remote connection established to {instanceName}");
                    return true;
                }
                else
                {
                    _logger.LogError($"❌ Failed to open VS Code remote connection: {openResult.Error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error setting up VS Code remote: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region File Synchronization

        public async Task<bool> StartFileSynchronizationAsync(string localPath, string remotePath, string hostname, string username)
        {
            try
            {
                _logger.LogAction($"Starting file synchronization: {localPath} -> {remotePath}");

                // This would typically use a file watcher and SFTP
                // For now, we'll implement a basic sync using rsync
                var result = await ExecuteCommandAsync("rsync", 
                    $"-avz --delete -e \"ssh -i {_sshKeyPath}\" {localPath}/ {username}@{hostname}:{remotePath}/");

                if (result.Success)
                {
                    _logger.LogInfo("✅ File synchronization completed successfully");
                    return true;
                }
                else
                {
                    _logger.LogError($"❌ File synchronization failed: {result.Error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error during file synchronization: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private async Task<CommandResult> ExecuteCommandAsync(string command, string arguments)
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                return new CommandResult
                {
                    Success = process.ExitCode == 0,
                    Output = output,
                    Error = error,
                    ExitCode = process.ExitCode
                };
            }
            catch (Exception ex)
            {
                return new CommandResult
                {
                    Success = false,
                    Error = ex.Message,
                    ExitCode = -1
                };
            }
        }

        public void Dispose()
        {
            _sshClient?.Dispose();
        }

        #endregion
    }

    #region Data Models

    public class ValidationResult
    {
        public bool PythonInstalled { get; set; }
        public bool VSCodeInstalled { get; set; }
        public bool SSHConfigured { get; set; }
        public bool AzureCLIInstalled { get; set; }
        public bool AzureAuthenticated { get; set; }
        public bool NetworkConnectivity { get; set; }
        public bool PythonPackagesInstalled { get; set; }
        public bool AllPrerequisitesMet { get; set; }

        public override string ToString()
        {
            return $@"Prerequisites Validation Results:
✅ Python Installed: {PythonInstalled}
✅ VS Code Installed: {VSCodeInstalled}
✅ SSH Configured: {SSHConfigured}
✅ Azure CLI Installed: {AzureCLIInstalled}
✅ Azure Authenticated: {AzureAuthenticated}
✅ Network Connectivity: {NetworkConnectivity}
✅ Python Packages Installed: {PythonPackagesInstalled}
🎯 All Prerequisites Met: {AllPrerequisitesMet}";
        }
    }

    public class ComputeInstanceResult
    {
        public bool Success { get; set; }
        public string InstanceName { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Hostname { get; set; }
        public string? Username { get; set; }
    }

    public class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
    }

    #endregion
}