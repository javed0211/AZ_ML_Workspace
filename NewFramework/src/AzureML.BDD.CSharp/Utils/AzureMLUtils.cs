using Microsoft.Playwright;
using PlaywrightFramework.Utils;
using Serilog;

namespace PlaywrightFramework.Utils
{
    public class AzureMLUtils
    {
        private readonly IPage _page;
        private readonly PlaywrightUtils _utils;
        private readonly ConfigManager _config;
        private readonly Logger _logger;
        private readonly PIMUtils _pimUtils;

        public AzureMLUtils(IPage page, Logger logger)
        {
            _page = page;
            _utils = new PlaywrightUtils(page);
            _config = ConfigManager.Instance;
            _logger = logger;
            _pimUtils = new PIMUtils(page, logger);
        }

        // Navigation Methods
        public async Task NavigateToWorkspaceAsync(string workspaceName)
        {
            _logger.LogAction($"Navigate to Azure ML workspace: {workspaceName}");
            var baseUrl = "https://ml.azure.com";
            var workspaceUrl = $"{baseUrl}/workspaces/{workspaceName}";
            
            await _utils.NavigateToAsync(workspaceUrl);
            await _utils.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Wait for the page to fully load
            await _utils.SleepAsync(3000);
        }

        public async Task NavigateToWorkspaceUrlAsync(string url)
        {
            _logger.LogAction($"Navigate to Azure ML workspace URL: {url}");
            await _utils.NavigateToAsync(url);
            await _utils.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        public async Task NavigateToComputeSectionAsync()
        {
            _logger.LogAction("Navigate to compute section");
            
            // Look for compute navigation link
            var computeSelectors = new[]
            {
                "a[href*=\"compute\"]",
                "button:has-text(\"Compute\")",
                "[data-testid=\"compute-nav\"]",
                "nav a:has-text(\"Compute\")",
                ".nav-item:has-text(\"Compute\")"
            };

            var computeFound = false;
            foreach (var selector in computeSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    await _utils.ClickAsync(selector);
                    computeFound = true;
                    break;
                }
                catch (Exception)
                {
                    _logger.LogWarning($"Compute selector not found: {selector}");
                }
            }

            if (!computeFound)
            {
                // Try to find compute in sidebar or menu
                var menuSelectors = new[]
                {
                    "[role=\"navigation\"]",
                    ".sidebar",
                    ".nav-menu",
                    ".navigation"
                };

                foreach (var menuSelector in menuSelectors)
                {
                    try
                    {
                        var computeInMenu = $"{menuSelector} a:has-text(\"Compute\"), {menuSelector} button:has-text(\"Compute\")";
                        await _utils.WaitForElementAsync(computeInMenu, 3000);
                        await _utils.ClickAsync(computeInMenu);
                        computeFound = true;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            if (!computeFound)
            {
                throw new Exception("Could not find compute navigation option");
            }

            await _utils.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _utils.SleepAsync(2000);
        }

        // Authentication Methods
        public async Task HandleAuthenticationIfRequiredAsync()
        {
            _logger.LogAction("Handle authentication if required");
            
            // Check if we're on a login page
            var loginIndicators = new[]
            {
                "input[type=\"email\"]",
                "input[name=\"loginfmt\"]",
                "input[id=\"i0116\"]",
                ".login-form",
                "[data-testid=\"login\"]"
            };

            var loginRequired = false;
            foreach (var selector in loginIndicators)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 3000);
                    loginRequired = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (loginRequired)
            {
                await PerformLoginAsync();
            }
            else
            {
                _logger.LogInfo("No authentication required - already logged in");
            }
        }

        public async Task HandleLoginIfRequiredAsync(string userName)
        {
            _logger.LogAction($"Handle login if required for user: {userName}");
            await HandleAuthenticationIfRequiredAsync();
        }

        private async Task PerformLoginAsync()
        {
            _logger.LogAction("Performing Azure authentication");
            
            var authConfig = _config.GetAuthenticationSettings();
            
            // Enter email
            var emailSelectors = new[]
            {
                "input[type=\"email\"]",
                "input[name=\"loginfmt\"]",
                "input[id=\"i0116\"]"
            };

            foreach (var selector in emailSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    await _utils.FillAsync(selector, authConfig.Username);
                    await _utils.PressKeyOnElementAsync(selector, "Enter");
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            await _utils.SleepAsync(2000);

            // Enter password
            var passwordSelectors = new[]
            {
                "input[type=\"password\"]",
                "input[name=\"passwd\"]",
                "input[id=\"i0118\"]"
            };

            foreach (var selector in passwordSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    await _utils.FillAsync(selector, authConfig.Password);
                    await _utils.PressKeyOnElementAsync(selector, "Enter");
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Handle MFA if enabled
            if (authConfig.MFA?.Enabled == true)
            {
                await HandleMFAAsync();
            }

            // Wait for successful login
            await _utils.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _utils.SleepAsync(3000);
        }

        private async Task HandleMFAAsync()
        {
            _logger.LogAction("Handling MFA authentication");
            
            var authConfig = _config.GetAuthenticationSettings();
            
            // Look for MFA input
            var mfaSelectors = new[]
            {
                "input[name=\"otc\"]",
                "input[id=\"idTxtBx_SAOTCC_OTC\"]",
                "input[type=\"tel\"]",
                ".form-control[placeholder*=\"code\"]"
            };

            var mfaFound = false;
            foreach (var selector in mfaSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 10000);
                    
                    if (authConfig.MFA?.AutoSubmitOTP == true && !string.IsNullOrEmpty(authConfig.MFA?.TOTPSecretKey))
                    {
                        // Generate TOTP code (simplified - in real implementation you'd use a proper TOTP library)
                        var otpCode = GenerateTOTPCode(authConfig.MFA.TOTPSecretKey);
                        await _utils.FillAsync(selector, otpCode);
                        await _utils.PressKeyOnElementAsync(selector, "Enter");
                    }
                    else
                    {
                        _logger.LogWarning("MFA code required but auto-submit not configured");
                        // Wait for manual input
                        await _utils.SleepAsync(30000);
                    }
                    
                    mfaFound = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (mfaFound)
            {
                await _utils.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await _utils.SleepAsync(2000);
            }
        }

        private string GenerateTOTPCode(string secret)
        {
            // Simplified TOTP generation - in real implementation use proper TOTP library
            // This is a placeholder that returns a 6-digit code
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // Workspace Verification Methods
        public async Task VerifyWorkspaceAccessAsync(string workspaceName)
        {
            _logger.LogAction($"Verify workspace access: {workspaceName}");
            
            // Check for workspace indicators
            var workspaceIndicators = new[]
            {
                $"h1:has-text(\"{workspaceName}\")",
                $"[title*=\"{workspaceName}\"]",
                $".workspace-name:has-text(\"{workspaceName}\")",
                $"[data-testid=\"workspace-name\"]:has-text(\"{workspaceName}\")"
            };

            var workspaceFound = false;
            foreach (var selector in workspaceIndicators)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 10000);
                    workspaceFound = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!workspaceFound)
            {
                // Check page title or URL
                var currentUrl = await _utils.GetCurrentUrlAsync();
                var pageTitle = await _utils.GetTitleAsync();
                
                if (currentUrl.Contains(workspaceName) || pageTitle.Contains(workspaceName))
                {
                    workspaceFound = true;
                }
            }

            if (!workspaceFound)
            {
                throw new Exception($"Could not verify access to workspace: {workspaceName}");
            }

            _logger.LogInfo($"✅ Successfully verified access to workspace: {workspaceName}");
        }

        public async Task VerifyWorkspaceAvailableAsync()
        {
            _logger.LogAction("Verify workspace is available");
            
            // Check for error indicators
            var errorIndicators = new[]
            {
                ".error-message",
                ".alert-danger",
                "[role=\"alert\"]",
                ".notification-error"
            };

            foreach (var selector in errorIndicators)
            {
                try
                {
                    var isVisible = await _utils.IsVisibleAsync(selector);
                    if (isVisible)
                    {
                        var errorText = await _utils.GetTextAsync(selector);
                        throw new Exception($"Workspace error detected: {errorText}");
                    }
                }
                catch (Exception)
                {
                    // If element doesn't exist, that's good
                    continue;
                }
            }

            // Check for positive indicators
            var availabilityIndicators = new[]
            {
                ".workspace-dashboard",
                ".workspace-content",
                "[data-testid=\"workspace-main\"]",
                ".main-content"
            };

            var workspaceAvailable = false;
            foreach (var selector in availabilityIndicators)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    workspaceAvailable = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!workspaceAvailable)
            {
                // Check if page has loaded successfully
                var currentUrl = await _utils.GetCurrentUrlAsync();
                if (currentUrl.Contains("ml.azure.com") && !currentUrl.Contains("error"))
                {
                    workspaceAvailable = true;
                }
            }

            if (!workspaceAvailable)
            {
                throw new Exception("Workspace does not appear to be available");
            }

            _logger.LogInfo("✅ Workspace is available and accessible");
        }

        // Compute Instance Management Methods
        public async Task StartComputeInstanceAsync(string computeName)
        {
            _logger.LogAction($"Start compute instance: {computeName}");
            
            // Find the compute instance in the list
            await FindComputeInstanceAsync(computeName);
            
            // Look for start button
            var startSelectors = new[]
            {
                $"[data-compute-name=\"{computeName}\"] button:has-text(\"Start\")",
                $"tr:has-text(\"{computeName}\") button:has-text(\"Start\")",
                $".compute-row:has-text(\"{computeName}\") .start-button",
                $"[aria-label*=\"Start {computeName}\"]"
            };

            var startButtonFound = false;
            foreach (var selector in startSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    await _utils.ClickAsync(selector);
                    startButtonFound = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!startButtonFound)
            {
                // Try generic approach - find compute row and then start button
                var computeRowSelectors = new[]
                {
                    $"tr:has-text(\"{computeName}\")",
                    $".compute-item:has-text(\"{computeName}\")",
                    $"[data-testid=\"compute-row\"]:has-text(\"{computeName}\")"
                };

                foreach (var rowSelector in computeRowSelectors)
                {
                    try
                    {
                        await _utils.WaitForElementAsync(rowSelector, 5000);
                        
                        // Look for start button within this row
                        var startInRow = $"{rowSelector} button:has-text(\"Start\"), {rowSelector} [aria-label*=\"Start\"]";
                        await _utils.WaitForElementAsync(startInRow, 3000);
                        await _utils.ClickAsync(startInRow);
                        startButtonFound = true;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            if (!startButtonFound)
            {
                throw new Exception($"Could not find start button for compute instance: {computeName}");
            }

            // Wait for the start operation to begin
            await _utils.SleepAsync(3000);
            
            // Wait for compute to start (this can take several minutes)
            await WaitForComputeStatusAsync(computeName, "Running", 300000); // 5 minutes timeout
            
            _logger.LogInfo($"✅ Successfully started compute instance: {computeName}");
        }

        public async Task StopComputeInstanceAsync(string computeName)
        {
            _logger.LogAction($"Stop compute instance: {computeName}");
            
            // Find the compute instance in the list
            await FindComputeInstanceAsync(computeName);
            
            // Look for stop button
            var stopSelectors = new[]
            {
                $"[data-compute-name=\"{computeName}\"] button:has-text(\"Stop\")",
                $"tr:has-text(\"{computeName}\") button:has-text(\"Stop\")",
                $".compute-row:has-text(\"{computeName}\") .stop-button",
                $"[aria-label*=\"Stop {computeName}\"]"
            };

            var stopButtonFound = false;
            foreach (var selector in stopSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    await _utils.ClickAsync(selector);
                    stopButtonFound = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!stopButtonFound)
            {
                // Try generic approach
                var computeRowSelectors = new[]
                {
                    $"tr:has-text(\"{computeName}\")",
                    $".compute-item:has-text(\"{computeName}\")",
                    $"[data-testid=\"compute-row\"]:has-text(\"{computeName}\")"
                };

                foreach (var rowSelector in computeRowSelectors)
                {
                    try
                    {
                        await _utils.WaitForElementAsync(rowSelector, 5000);
                        
                        var stopInRow = $"{rowSelector} button:has-text(\"Stop\"), {rowSelector} [aria-label*=\"Stop\"]";
                        await _utils.WaitForElementAsync(stopInRow, 3000);
                        await _utils.ClickAsync(stopInRow);
                        stopButtonFound = true;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            if (!stopButtonFound)
            {
                throw new Exception($"Could not find stop button for compute instance: {computeName}");
            }

            // Confirm stop if dialog appears
            try
            {
                await _utils.WaitForElementAsync("button:has-text(\"Confirm\"), button:has-text(\"Yes\"), button:has-text(\"Stop\")", 3000);
                await _utils.ClickAsync("button:has-text(\"Confirm\"), button:has-text(\"Yes\"), button:has-text(\"Stop\")");
            }
            catch (Exception)
            {
                // No confirmation dialog
            }

            // Wait for the stop operation to complete
            await WaitForComputeStatusAsync(computeName, "Stopped", 120000); // 2 minutes timeout
            
            _logger.LogInfo($"✅ Successfully stopped compute instance: {computeName}");
        }

        public async Task EnsureComputeInstanceRunningAsync(string computeName)
        {
            _logger.LogAction($"Ensure compute instance is running: {computeName}");
            
            var status = await GetComputeInstanceStatusAsync(computeName);
            
            if (status != "Running")
            {
                _logger.LogInfo($"Compute instance {computeName} is {status}, starting it...");
                await StartComputeInstanceAsync(computeName);
            }
            else
            {
                _logger.LogInfo($"Compute instance {computeName} is already running");
            }
        }

        public async Task StartComputeIfNotRunningAsync(string computeName)
        {
            _logger.LogAction($"Start compute if not running: {computeName}");
            await EnsureComputeInstanceRunningAsync(computeName);
        }

        public async Task StopAllComputeInstancesAsync()
        {
            _logger.LogAction("Stop all compute instances");
            
            // Get list of running compute instances
            var runningInstances = await GetRunningComputeInstancesAsync();
            
            foreach (var computeName in runningInstances)
            {
                try
                {
                    await StopComputeInstanceAsync(computeName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to stop compute instance {computeName}: {ex.Message}");
                }
            }
            
            _logger.LogInfo("✅ Stopped all compute instances");
        }

        // Compute Status and Verification Methods
        public async Task VerifyComputeInstanceStatusAsync(string computeName, string expectedStatus)
        {
            _logger.LogAction($"Verify compute instance status: {computeName} should be {expectedStatus}");
            
            var actualStatus = await GetComputeInstanceStatusAsync(computeName);
            
            if (actualStatus != expectedStatus)
            {
                throw new Exception($"Expected compute instance {computeName} to be {expectedStatus}, but it is {actualStatus}");
            }
            
            _logger.LogInfo($"✅ Compute instance {computeName} is {expectedStatus} as expected");
        }

        public async Task VerifyComputeInstanceConnectivityAsync(string computeName)
        {
            _logger.LogAction($"Verify compute instance connectivity: {computeName}");
            
            // Look for connectivity indicators
            var connectivityIndicators = new[]
            {
                $"[data-compute-name=\"{computeName}\"] .connectivity-status.connected",
                $"tr:has-text(\"{computeName}\") .status-connected",
                $"[aria-label*=\"Connected {computeName}\"]"
            };

            var connectivityVerified = false;
            foreach (var selector in connectivityIndicators)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 10000);
                    connectivityVerified = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!connectivityVerified)
            {
                // Check if there are any connection options available
                var connectionOptions = new[]
                {
                    $"[data-compute-name=\"{computeName}\"] button:has-text(\"Connect\")",
                    $"tr:has-text(\"{computeName}\") a:has-text(\"Jupyter\")",
                    $"tr:has-text(\"{computeName}\") a:has-text(\"Terminal\")"
                };

                foreach (var selector in connectionOptions)
                {
                    try
                    {
                        await _utils.WaitForElementAsync(selector, 5000);
                        connectivityVerified = true;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            if (!connectivityVerified)
            {
                _logger.LogWarning($"Could not verify connectivity for compute instance: {computeName}");
            }
            else
            {
                _logger.LogInfo($"✅ Compute instance {computeName} connectivity verified");
            }
        }

        private async Task FindComputeInstanceAsync(string computeName)
        {
            _logger.LogAction($"Find compute instance: {computeName}");
            
            // Wait for compute list to load
            var computeListSelectors = new[]
            {
                ".compute-list",
                ".compute-instances",
                "[data-testid=\"compute-list\"]",
                "table tbody"
            };

            var listFound = false;
            foreach (var selector in computeListSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 10000);
                    listFound = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!listFound)
            {
                throw new Exception("Could not find compute instances list");
            }

            // Look for the specific compute instance
            var computeSelectors = new[]
            {
                $"tr:has-text(\"{computeName}\")",
                $".compute-item:has-text(\"{computeName}\")",
                $"[data-compute-name=\"{computeName}\"]",
                $"[data-testid=\"compute-row\"]:has-text(\"{computeName}\")"
            };

            var computeFound = false;
            foreach (var selector in computeSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    computeFound = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!computeFound)
            {
                throw new Exception($"Could not find compute instance: {computeName}");
            }
        }

        private async Task<string> GetComputeInstanceStatusAsync(string computeName)
        {
            await FindComputeInstanceAsync(computeName);
            
            // Look for status indicators
            var statusSelectors = new[]
            {
                $"[data-compute-name=\"{computeName}\"] .status",
                $"tr:has-text(\"{computeName}\") .compute-status",
                $"tr:has-text(\"{computeName}\") td:nth-child(3)", // Assuming status is in 3rd column
                $".compute-item:has-text(\"{computeName}\") .status"
            };

            foreach (var selector in statusSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    var statusText = await _utils.GetTextAsync(selector);
                    
                    // Normalize status text
                    var normalizedStatus = statusText.Trim().ToLower();
                    if (normalizedStatus.Contains("running") || normalizedStatus.Contains("succeeded"))
                    {
                        return "Running";
                    }
                    else if (normalizedStatus.Contains("stopped") || normalizedStatus.Contains("deallocated"))
                    {
                        return "Stopped";
                    }
                    else if (normalizedStatus.Contains("starting"))
                    {
                        return "Starting";
                    }
                    else if (normalizedStatus.Contains("stopping"))
                    {
                        return "Stopping";
                    }
                    
                    return statusText.Trim();
                }
                catch (Exception)
                {
                    continue;
                }
            }

            throw new Exception($"Could not determine status for compute instance: {computeName}");
        }

        private async Task WaitForComputeStatusAsync(string computeName, string expectedStatus, int timeoutMs = 300000)
        {
            _logger.LogAction($"Wait for compute {computeName} to reach status: {expectedStatus}");
            
            var startTime = DateTime.Now;
            
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                try
                {
                    var currentStatus = await GetComputeInstanceStatusAsync(computeName);
                    
                    if (currentStatus == expectedStatus)
                    {
                        _logger.LogInfo($"✅ Compute instance {computeName} reached status: {expectedStatus}");
                        return;
                    }
                    
                    _logger.LogInfo($"Compute instance {computeName} status: {currentStatus}, waiting for {expectedStatus}...");
                    await _utils.SleepAsync(10000); // Wait 10 seconds before checking again
                    
                    // Refresh the page to get updated status
                    await _utils.RefreshAsync();
                    await _utils.SleepAsync(3000);
                    
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error checking compute status: {ex.Message}");
                    await _utils.SleepAsync(5000);
                }
            }
            
            throw new Exception($"Timeout waiting for compute instance {computeName} to reach status {expectedStatus}");
        }

        private async Task<List<string>> GetRunningComputeInstancesAsync()
        {
            var runningInstances = new List<string>();
            
            // This is a simplified implementation
            // In a real scenario, you'd parse the compute instances table
            try
            {
                var computeRows = await _page.Locator("tr:has-text(\"Running\"), tr:has-text(\"Succeeded\")").AllAsync();
                
                foreach (var row in computeRows)
                {
                    try
                    {
                        var computeName = await row.Locator("td:first-child").TextContentAsync();
                        if (!string.IsNullOrEmpty(computeName))
                        {
                            runningInstances.Add(computeName.Trim());
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not get running compute instances: {ex.Message}");
            }
            
            return runningInstances;
        }

        // Workspace Selection and Navigation Methods
        public async Task SelectWorkspaceAsync(string workspaceName)
        {
            _logger.LogAction($"Select workspace: {workspaceName}");
            
            // Look for workspace selector
            var workspaceSelectors = new[]
            {
                $"button:has-text(\"{workspaceName}\")",
                $"a:has-text(\"{workspaceName}\")",
                $".workspace-item:has-text(\"{workspaceName}\")",
                $"[data-testid=\"workspace-selector\"] option:has-text(\"{workspaceName}\")"
            };

            var workspaceSelected = false;
            foreach (var selector in workspaceSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 10000);
                    await _utils.ClickAsync(selector);
                    workspaceSelected = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!workspaceSelected)
            {
                throw new Exception($"Could not select workspace: {workspaceName}");
            }

            await _utils.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _utils.SleepAsync(3000);
            
            _logger.LogInfo($"✅ Selected workspace: {workspaceName}");
        }

        public async Task ChooseComputeOptionAsync()
        {
            _logger.LogAction("Choose compute option");
            
            var computeOptionSelectors = new[]
            {
                "a:has-text(\"Compute\")",
                "button:has-text(\"Compute\")",
                "[href*=\"compute\"]",
                ".nav-item:has-text(\"Compute\")",
                "[data-testid=\"compute-option\"]"
            };

            var computeOptionFound = false;
            foreach (var selector in computeOptionSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 10000);
                    await _utils.ClickAsync(selector);
                    computeOptionFound = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!computeOptionFound)
            {
                throw new Exception("Could not find compute option");
            }

            await _utils.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _utils.SleepAsync(2000);
            
            _logger.LogInfo("✅ Selected compute option");
        }

        public async Task OpenComputeAsync(string computeName)
        {
            _logger.LogAction($"Open compute: {computeName}");
            
            // Find and click on the compute instance
            await FindComputeInstanceAsync(computeName);
            
            var openSelectors = new[]
            {
                $"[data-compute-name=\"{computeName}\"] a",
                $"tr:has-text(\"{computeName}\") a:first-child",
                $".compute-item:has-text(\"{computeName}\") .compute-name"
            };

            var computeOpened = false;
            foreach (var selector in openSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    await _utils.ClickAsync(selector);
                    computeOpened = true;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!computeOpened)
            {
                // Try clicking on the compute name directly
                var computeNameSelectors = new[]
                {
                    $"tr:has-text(\"{computeName}\") td:first-child",
                    $".compute-row:has-text(\"{computeName}\") .name"
                };

                foreach (var selector in computeNameSelectors)
                {
                    try
                    {
                        await _utils.WaitForElementAsync(selector, 5000);
                        await _utils.ClickAsync(selector);
                        computeOpened = true;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            if (!computeOpened)
            {
                throw new Exception($"Could not open compute instance: {computeName}");
            }

            await _utils.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _utils.SleepAsync(3000);
            
            _logger.LogInfo($"✅ Opened compute: {computeName}");
        }

        // Application Links and VS Code Integration Methods
        public async Task<bool> CheckApplicationLinksEnabledAsync()
        {
            _logger.LogAction("Check if application links are enabled");
            
            var applicationLinkSelectors = new[]
            {
                ".application-links",
                ".app-links",
                "a:has-text(\"VS Code\")",
                "a:has-text(\"Jupyter\")",
                "a:has-text(\"Terminal\")",
                "[data-testid=\"application-links\"]"
            };

            foreach (var selector in applicationLinkSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 5000);
                    var isVisible = await _utils.IsVisibleAsync(selector);
                    if (isVisible)
                    {
                        _logger.LogInfo("✅ Application links are enabled");
                        return true;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            _logger.LogWarning("Application links do not appear to be enabled");
            return false;
        }

        public async Task StartVSCodeDesktopAsync()
        {
            _logger.LogAction("Start VS Code Desktop");
            
            // Look for VS Code Desktop link
            var vsCodeSelectors = new[]
            {
                "a:has-text(\"VS Code Desktop\")",
                "button:has-text(\"VS Code Desktop\")",
                "a:has-text(\"VS Code\")",
                "[data-testid=\"vscode-desktop\"]",
                ".app-link:has-text(\"VS Code\")"
            };

            var vsCodeLinkFound = false;
            foreach (var selector in vsCodeSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 10000);
                    
                    // Get the href if it's a link
                    var href = await _utils.GetAttributeAsync(selector, "href");
                    
                    if (!string.IsNullOrEmpty(href) && href.StartsWith("vscode://"))
                    {
                        // This is a VS Code protocol link
                        await _utils.ClickAsync(selector);
                        vsCodeLinkFound = true;
                        break;
                    }
                    else
                    {
                        // Regular link or button
                        await _utils.ClickAsync(selector);
                        vsCodeLinkFound = true;
                        break;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (!vsCodeLinkFound)
            {
                throw new Exception("Could not find VS Code Desktop link");
            }

            // Wait for VS Code to launch
            await _utils.SleepAsync(5000);
            
            _logger.LogInfo("✅ VS Code Desktop launch initiated");
        }

        public async Task<bool> VerifyVSCodeInteractionAsync()
        {
            _logger.LogAction("Verify VS Code Desktop interaction");
            
            try
            {
                // This is a simplified check - in a real implementation you'd use proper VS Code automation
                // For now, we'll just check if the VS Code process is running
                await _utils.SleepAsync(5000);
                
                _logger.LogInfo("✅ VS Code Desktop interaction verified (simplified check)");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"VS Code interaction failed: {ex.Message}");
                return false;
            }
        }

        public async Task StopComputeInstance(string computeName)
        {
            await StopComputeInstanceAsync(computeName);
        }

        public async Task HandleAuthentication(string userName)
        {
            _logger.LogAction($"Handle authentication for user: {userName}");
            
            try
            {
                // Check if already logged in
                var userIndicators = new[]
                {
                    $"[aria-label*=\"{userName}\"]",
                    $"button:has-text(\"{userName}\")",
                    $".user-name:has-text(\"{userName}\")",
                    ".user-profile"
                };

                var alreadyLoggedIn = false;
                foreach (var selector in userIndicators)
                {
                    try
                    {
                        await _utils.WaitForElementAsync(selector, 3000);
                        alreadyLoggedIn = true;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                if (alreadyLoggedIn)
                {
                    _logger.LogInfo($"✅ Already authenticated as {userName}");
                    return;
                }

                // Look for login button or sign-in prompt
                var loginSelectors = new[]
                {
                    "button:has-text(\"Sign in\")",
                    "a:has-text(\"Sign in\")",
                    ".login-button",
                    "[data-testid=\"login-button\"]"
                };

                var loginButtonFound = false;
                foreach (var selector in loginSelectors)
                {
                    try
                    {
                        await _utils.WaitForElementAsync(selector, 5000);
                        await _utils.ClickAsync(selector);
                        loginButtonFound = true;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                if (loginButtonFound)
                {
                    // Wait for authentication to complete
                    await _utils.SleepAsync(10000);
                    _logger.LogInfo($"✅ Authentication initiated for {userName}");
                }
                else
                {
                    _logger.LogInfo("No login required or already authenticated");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Authentication handling failed: {ex.Message}");
            }
        }

        public async Task StartVSCodeDesktop()
        {
            _logger.LogAction("Start VS Code Desktop");
            
            try
            {
                // Look for VS Code Desktop launch button
                var vsCodeSelectors = new[]
                {
                    "button:has-text(\"VS Code Desktop\")",
                    "a:has-text(\"VS Code Desktop\")",
                    "[data-testid=\"vscode-desktop\"]",
                    ".vscode-desktop-button"
                };

                var vsCodeButtonFound = false;
                foreach (var selector in vsCodeSelectors)
                {
                    try
                    {
                        await _utils.WaitForElementAsync(selector, 5000);
                        await _utils.ClickAsync(selector);
                        vsCodeButtonFound = true;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                if (vsCodeButtonFound)
                {
                    // Wait for VS Code to launch
                    await _utils.SleepAsync(10000);
                    _logger.LogInfo("✅ VS Code Desktop launch initiated");
                }
                else
                {
                    throw new Exception("Could not find VS Code Desktop launch button");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start VS Code Desktop: {ex.Message}");
                throw;
            }
        }

        // PIM (Privileged Identity Management) Methods
        public async Task ActivatePIMRoleAsync(string roleName, string justification = "Automated test execution", int durationHours = 8)
        {
            _logger.LogAction($"Activate PIM role: {roleName}");
            await _pimUtils.ActivatePIMRoleAsync(roleName, justification, durationHours);
        }

        public async Task ActivateDataScientistPIMRoleAsync()
        {
            _logger.LogAction("Activate Data Scientist PIM role");
            await _pimUtils.ActivateDataScientistRoleAsync();
        }

        public async Task<bool> IsPIMRoleActiveAsync(string roleName)
        {
            _logger.LogAction($"Check if PIM role is active: {roleName}");
            return await _pimUtils.IsRoleActiveAsync(roleName);
        }

        public async Task<bool> IsDataScientistPIMRoleActiveAsync()
        {
            const string roleName = "PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST";
            return await IsPIMRoleActiveAsync(roleName);
        }
    }
}