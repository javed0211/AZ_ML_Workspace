using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Tests
{
    [TestFixture]
    public class AzureMLWorkspaceTests
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private IPage? _page;
        private PlaywrightUtils? _utils;
        private AzureMLUtils? _azureMLUtils;
        private ConfigManager? _config;
        private Logger? _logger;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _playwright = await Playwright.CreateAsync();
            _config = ConfigManager.Instance;
            _logger = Logger.Instance;
            
            var browserSettings = _config.GetBrowserSettings();
            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = browserSettings.Headless,
                SlowMo = browserSettings.SlowMo,
                Args = browserSettings.Args?.ToArray()
            };

            _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
        }

        [SetUp]
        public async Task SetUp()
        {
            if (_browser == null) throw new InvalidOperationException("Browser not initialized");
            
            _page = await _browser.NewPageAsync();
            _utils = new PlaywrightUtils(_page);
            _azureMLUtils = new AzureMLUtils(_page, _logger);
            
            _logger?.LogInfo("🚀 Starting Azure ML Workspace test setup");
            
            // Setup data scientist context and activate PIM role
            _logger?.LogStep("Setup data scientist context and activate PIM role");
            _logger?.LogInfo("Data scientist: Javed");
            
            try
            {
                // Check if PIM role is already active
                var isActive = await _azureMLUtils.IsDataScientistPIMRoleActiveAsync();
                
                if (!isActive)
                {
                    _logger?.LogInfo("🔐 Activating Data Scientist PIM role...");
                    await _azureMLUtils.ActivateDataScientistPIMRoleAsync();
                    _logger?.LogInfo("✅ Data Scientist PIM role activated successfully");
                }
                else
                {
                    _logger?.LogInfo("✅ Data Scientist PIM role is already active");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"⚠️ PIM role activation failed, continuing with tests: {ex.Message}");
                // Continue with tests even if PIM activation fails
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_azureMLUtils != null && _logger != null)
            {
                _logger.LogInfo("🧹 Cleaning up after test");
                
                // Take final screenshot
                await _utils?.TakeScreenshotAsync("test-cleanup")!;
                
                // Stop any running compute instances
                try
                {
                    await _azureMLUtils.StopAllComputeInstancesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to stop compute instances during cleanup: {ex.Message}");
                }
                
                _logger.LogInfo("✅ Test cleanup completed");
            }

            if (_page != null)
            {
                await _page.CloseAsync();
                _page = null;
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }
            
            _playwright?.Dispose();
        }

        [Test]
        [Category("AzureML")]
        [Description("Should access Azure ML Workspace")]
        public async Task ShouldAccessAzureMLWorkspace()
        {
            _logger?.LogInfo("🎯 Test: Access Azure ML Workspace");
            
            // Step 1: Navigate to Azure ML workspace
            _logger?.LogStep("Navigate to Azure ML workspace");
            var workspaceName = _config?.GetAzureSettings().WorkspaceName ?? "ml-workspace";
            await _azureMLUtils!.NavigateToWorkspaceAsync(workspaceName);
            
            // Step 2: Handle authentication if required
            _logger?.LogStep("Handle authentication if required");
            await _azureMLUtils.HandleAuthenticationIfRequiredAsync();
            
            // Step 3: Verify workspace access
            _logger?.LogStep("Verify workspace access");
            await _azureMLUtils.VerifyWorkspaceAccessAsync(workspaceName);
            
            // Step 4: Verify workspace is available
            _logger?.LogStep("Verify workspace is available");
            await _azureMLUtils.VerifyWorkspaceAvailableAsync();
            
            // Take screenshot of successful access
            await _utils!.TakeScreenshotAsync("workspace-access-success");
            
            _logger?.LogInfo("✅ Successfully accessed Azure ML workspace");
        }

        [Test]
        [Category("AzureML")]
        [Description("Should start compute instance")]
        public async Task ShouldStartComputeInstance()
        {
            _logger?.LogInfo("🎯 Test: Start Compute Instance");
            
            // Step 1: Open workspace
            _logger?.LogStep("Open workspace");
            var workspaceName = _config?.GetAzureSettings().WorkspaceName ?? "ml-workspace";
            await _azureMLUtils!.NavigateToWorkspaceAsync(workspaceName);
            await _azureMLUtils.HandleAuthenticationIfRequiredAsync();
            
            // Step 2: Navigate to compute section
            _logger?.LogStep("Navigate to compute section");
            await _azureMLUtils.NavigateToComputeSectionAsync();
            
            // Step 3: Start compute instance
            _logger?.LogStep("Start compute instance");
            const string computeName = "test-compute";
            await _azureMLUtils.StartComputeInstanceAsync(computeName);
            
            // Step 4: Verify compute instance is running
            _logger?.LogStep("Verify compute instance is running");
            await _azureMLUtils.VerifyComputeInstanceStatusAsync(computeName, "Running");
            
            // Step 5: Verify connectivity
            _logger?.LogStep("Verify compute instance connectivity");
            await _azureMLUtils.VerifyComputeInstanceConnectivityAsync(computeName);
            
            // Take screenshot of running compute
            await _utils!.TakeScreenshotAsync("compute-instance-running");
            
            _logger?.LogInfo("✅ Successfully started and verified compute instance");
        }

        [Test]
        [Category("AzureML")]
        [Description("Should stop compute instance")]
        public async Task ShouldStopComputeInstance()
        {
            _logger?.LogInfo("🎯 Test: Stop Compute Instance");
            
            // Step 1: Open workspace and ensure compute is running
            _logger?.LogStep("Setup: Ensure compute instance is running");
            var workspaceName = _config?.GetAzureSettings().WorkspaceName ?? "ml-workspace";
            const string computeName = "test-compute";
            
            await _azureMLUtils!.NavigateToWorkspaceAsync(workspaceName);
            await _azureMLUtils.HandleAuthenticationIfRequiredAsync();
            await _azureMLUtils.NavigateToComputeSectionAsync();
            
            // Ensure compute is running first
            await _azureMLUtils.EnsureComputeInstanceRunningAsync(computeName);
            
            // Step 2: Stop compute instance
            _logger?.LogStep("Stop compute instance");
            await _azureMLUtils.StopComputeInstanceAsync(computeName);
            
            // Step 3: Verify compute instance is stopped
            _logger?.LogStep("Verify compute instance is stopped");
            await _azureMLUtils.VerifyComputeInstanceStatusAsync(computeName, "Stopped");
            
            // Take screenshot of stopped compute
            await _utils!.TakeScreenshotAsync("compute-instance-stopped");
            
            _logger?.LogInfo("✅ Successfully stopped compute instance");
        }

        [Test]
        [Category("AzureML")]
        [Description("Should manage multiple compute instances")]
        public async Task ShouldManageMultipleComputeInstances()
        {
            _logger?.LogInfo("🎯 Test: Manage Multiple Compute Instances");
            
            // Step 1: Open workspace
            _logger?.LogStep("Open workspace");
            var workspaceName = _config?.GetAzureSettings().WorkspaceName ?? "ml-workspace";
            await _azureMLUtils!.NavigateToWorkspaceAsync(workspaceName);
            await _azureMLUtils.HandleAuthenticationIfRequiredAsync();
            await _azureMLUtils.NavigateToComputeSectionAsync();
            
            // Step 2: Start multiple compute instances
            _logger?.LogStep("Start multiple compute instances");
            var computeInstances = new[] { "test-compute-1", "test-compute-2" };
            
            foreach (var computeName in computeInstances)
            {
                _logger?.LogAction($"Starting compute instance: {computeName}");
                await _azureMLUtils.StartComputeInstanceAsync(computeName);
            }
            
            // Step 3: Verify all instances are running
            _logger?.LogStep("Verify all compute instances are running");
            foreach (var computeName in computeInstances)
            {
                await _azureMLUtils.VerifyComputeInstanceStatusAsync(computeName, "Running");
            }
            
            // Take screenshot of multiple running instances
            await _utils!.TakeScreenshotAsync("multiple-compute-instances-running");
            
            // Step 4: Stop all compute instances
            _logger?.LogStep("Stop all compute instances");
            foreach (var computeName in computeInstances)
            {
                _logger?.LogAction($"Stopping compute instance: {computeName}");
                await _azureMLUtils.StopComputeInstanceAsync(computeName);
            }
            
            // Step 5: Verify all instances are stopped
            _logger?.LogStep("Verify all compute instances are stopped");
            foreach (var computeName in computeInstances)
            {
                await _azureMLUtils.VerifyComputeInstanceStatusAsync(computeName, "Stopped");
            }
            
            // Take screenshot of all stopped instances
            await _utils!.TakeScreenshotAsync("multiple-compute-instances-stopped");
            
            _logger?.LogInfo("✅ Successfully managed multiple compute instances");
        }

        [Test]
        [Category("AzureML")]
        [Description("Should integrate with VS Code Desktop")]
        public async Task ShouldIntegrateWithVSCodeDesktop()
        {
            _logger?.LogInfo("🎯 Test: Azure ML Workspace with VS Code Desktop Integration");
            
            // Step 1: Verify Contributor access and navigate to workspace
            _logger?.LogStep("Navigate to Azure ML workspace with Contributor access");
            await _azureMLUtils!.NavigateToWorkspaceUrlAsync("https://ml.azure.com/workspaces");
            
            // Step 2: Handle login if required
            _logger?.LogStep("Handle login if required");
            await _azureMLUtils.HandleLoginIfRequiredAsync("Javed Khan");
            
            // Step 3: Select workspace
            _logger?.LogStep("Select workspace");
            await _azureMLUtils.SelectWorkspaceAsync("CTO-workspace");
            
            // Step 4: Choose compute option
            _logger?.LogStep("Choose compute option");
            await _azureMLUtils.ChooseComputeOptionAsync();
            
            // Step 5: Open specific compute
            _logger?.LogStep("Open compute instance");
            const string computeName = "com-jk";
            await _azureMLUtils.OpenComputeAsync(computeName);
            
            // Step 6: Start compute if not running
            _logger?.LogStep("Start compute if not running");
            await _azureMLUtils.StartComputeIfNotRunningAsync(computeName);
            
            // Step 7: Check if application links are enabled
            _logger?.LogStep("Check if application links are enabled");
            var linksEnabled = await _azureMLUtils.CheckApplicationLinksEnabledAsync();
            _logger?.LogInfo($"Application links enabled: {linksEnabled}");
            
            // Take screenshot of workspace with application links
            await _utils!.TakeScreenshotAsync("workspace-application-links");
            
            // Step 8: Start VS Code Desktop
            _logger?.LogStep("Start VS Code Desktop");
            await _azureMLUtils.StartVSCodeDesktopAsync();
            
            // Step 9: Verify VS Code interaction
            _logger?.LogStep("Verify VS Code Desktop interaction");
            var vsCodeInteractive = await _azureMLUtils.VerifyVSCodeInteractionAsync();
            
            if (!vsCodeInteractive)
            {
                throw new Exception("VS Code Desktop is not interactive or not responding properly");
            }
            
            // Take screenshot of VS Code Desktop integration
            await _utils!.TakeScreenshotAsync("vscode-desktop-integration");
            
            _logger?.LogInfo("✅ Successfully integrated with VS Code Desktop");
        }
    }
}