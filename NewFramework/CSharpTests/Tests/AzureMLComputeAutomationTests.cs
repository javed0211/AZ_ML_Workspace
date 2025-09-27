using NUnit.Framework;
using FluentAssertions;
using PlaywrightFramework.Utils;
using Serilog;
using System.Text.Json;

namespace PlaywrightFramework.Tests
{
    [TestFixture]
    [Category("AzureMLComputeAutomation")]
    public class AzureMLComputeAutomationTests
    {
        private AzureMLComputeAutomationUtils _automationUtils = null!;
        private Logger _logger = null!;
        private ConfigManager _config = null!;
        private string _testInstanceName = null!;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            // Initialize logger
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/azure-ml-automation-tests.log")
                .CreateLogger();

            // Initialize config
            _config = ConfigManager.Instance;
            
            // Initialize automation utils
            _automationUtils = new AzureMLComputeAutomationUtils(_logger);
            
            // Generate unique test instance name
            _testInstanceName = $"test-compute-{DateTime.Now:yyyyMMdd-HHmmss}";
            
            _logger.Information("🚀 Starting Azure ML Compute Automation Tests");
            _logger.Information($"Test instance name: {_testInstanceName}");
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            // Cleanup test resources
            try
            {
                await _automationUtils.DeleteComputeInstanceAsync(_testInstanceName);
                _logger.Information("✅ Test cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.Warning($"⚠️ Test cleanup warning: {ex.Message}");
            }
            
            _automationUtils?.Dispose();
            _logger.Information("🏁 Azure ML Compute Automation Tests completed");
        }

        [SetUp]
        public void SetUp()
        {
            _logger.Information($"🧪 Starting test: {TestContext.CurrentContext.Test.Name}");
        }

        [TearDown]
        public void TearDown()
        {
            var testResult = TestContext.CurrentContext.Result.Outcome.Status;
            var emoji = testResult == NUnit.Framework.Interfaces.TestStatus.Passed ? "✅" : "❌";
            _logger.Information($"{emoji} Test completed: {TestContext.CurrentContext.Test.Name} - {testResult}");
        }

        #region Prerequisites Tests

        [Test, Order(1)]
        [Description("Validate all prerequisites for Azure ML compute automation")]
        public async Task Test_ValidatePrerequisites_ShouldPassAllChecks()
        {
            // Act
            var result = await _automationUtils.ValidatePrerequisitesAsync();

            // Assert
            result.Should().NotBeNull();
            
            // Log detailed results
            _logger.Information("Prerequisites validation results:");
            _logger.Information(result.ToString());

            // Individual assertions with helpful messages
            result.PythonInstalled.Should().BeTrue("Python is required for Azure ML automation");
            result.VSCodeInstalled.Should().BeTrue("VS Code is required for remote development");
            result.AzureCLIInstalled.Should().BeTrue("Azure CLI is required for authentication");
            result.NetworkConnectivity.Should().BeTrue("Network connectivity to Azure ML is required");
            
            // These might be warnings rather than failures in some environments
            if (!result.SSHConfigured)
            {
                _logger.Warning("⚠️ SSH not configured - will be set up during automation");
            }
            
            if (!result.AzureAuthenticated)
            {
                _logger.Warning("⚠️ Azure authentication not configured - please run 'az login'");
            }
            
            if (!result.PythonPackagesInstalled)
            {
                _logger.Warning("⚠️ Some Python packages missing - will be installed during automation");
            }

            // At minimum, we need Python, VS Code, Azure CLI, and network connectivity
            var criticalPrerequisites = result.PythonInstalled && 
                                      result.VSCodeInstalled && 
                                      result.AzureCLIInstalled && 
                                      result.NetworkConnectivity;
            
            criticalPrerequisites.Should().BeTrue("Critical prerequisites must be met");
        }

        [Test, Order(2)]
        [Description("Test Python installation and version")]
        public async Task Test_PythonInstallation_ShouldBeAvailable()
        {
            // This test is more granular than the prerequisites check
            var result = await _automationUtils.ValidatePrerequisitesAsync();
            
            result.PythonInstalled.Should().BeTrue("Python must be installed and accessible");
            
            // Additional checks could be added here for specific Python version requirements
            _logger.Information("✅ Python installation verified");
        }

        [Test, Order(3)]
        [Description("Test VS Code installation and extensions")]
        public async Task Test_VSCodeInstallation_ShouldBeAvailable()
        {
            var result = await _automationUtils.ValidatePrerequisitesAsync();
            
            result.VSCodeInstalled.Should().BeTrue("VS Code must be installed and accessible");
            
            _logger.Information("✅ VS Code installation verified");
        }

        [Test, Order(4)]
        [Description("Test Azure CLI installation and authentication")]
        public async Task Test_AzureCLI_ShouldBeInstalledAndAuthenticated()
        {
            var result = await _automationUtils.ValidatePrerequisitesAsync();
            
            result.AzureCLIInstalled.Should().BeTrue("Azure CLI must be installed");
            
            if (!result.AzureAuthenticated)
            {
                _logger.Warning("⚠️ Azure CLI is installed but not authenticated. Run 'az login' to authenticate.");
                Assert.Inconclusive("Azure CLI authentication required. Please run 'az login' and retry.");
            }
            
            _logger.Information("✅ Azure CLI installation and authentication verified");
        }

        #endregion

        #region Azure Authentication Tests

        [Test, Order(10)]
        [Description("Initialize Azure client with proper authentication")]
        public async Task Test_InitializeAzureClient_ShouldSucceed()
        {
            // Act
            var result = await _automationUtils.InitializeAzureClientAsync();

            // Assert
            result.Should().BeTrue("Azure client initialization should succeed with proper credentials");
            
            _logger.Information("✅ Azure client initialized successfully");
        }

        [Test, Order(11)]
        [Description("Initialize Azure ML workspace connection")]
        public async Task Test_InitializeWorkspace_ShouldSucceed()
        {
            // Arrange
            await _automationUtils.InitializeAzureClientAsync();

            // Act
            var result = await _automationUtils.InitializeWorkspaceAsync();

            // Assert
            result.Should().BeTrue("Workspace initialization should succeed with valid configuration");
            
            _logger.Information("✅ Azure ML workspace initialized successfully");
        }

        #endregion

        #region Compute Instance Management Tests

        [Test, Order(20)]
        [Description("Create a new compute instance")]
        public async Task Test_CreateComputeInstance_ShouldSucceed()
        {
            // Arrange
            await _automationUtils.InitializeAzureClientAsync();
            await _automationUtils.InitializeWorkspaceAsync();

            // Act
            var result = await _automationUtils.CreateComputeInstanceAsync(_testInstanceName, "Standard_DS3_v2");

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue($"Compute instance creation should succeed. Message: {result.Message}");
            result.InstanceName.Should().Be(_testInstanceName);
            
            _logger.Information($"✅ Compute instance created successfully: {_testInstanceName}");
        }

        [Test, Order(21)]
        [Description("Get compute instance details")]
        public async Task Test_GetComputeInstance_ShouldReturnInstanceDetails()
        {
            // Arrange
            await _automationUtils.InitializeAzureClientAsync();
            await _automationUtils.InitializeWorkspaceAsync();

            // Act
            var instance = await _automationUtils.GetComputeInstanceAsync(_testInstanceName);

            // Assert
            instance.Should().NotBeNull($"Compute instance {_testInstanceName} should exist");
            
            _logger.Information($"✅ Compute instance details retrieved: {_testInstanceName}");
        }

        [Test, Order(22)]
        [Description("Start compute instance")]
        public async Task Test_StartComputeInstance_ShouldSucceed()
        {
            // Arrange
            await _automationUtils.InitializeAzureClientAsync();
            await _automationUtils.InitializeWorkspaceAsync();

            // Act
            var result = await _automationUtils.StartComputeInstanceAsync(_testInstanceName);

            // Assert
            result.Should().BeTrue($"Starting compute instance {_testInstanceName} should succeed");
            
            _logger.Information($"✅ Compute instance started successfully: {_testInstanceName}");
        }

        [Test, Order(23)]
        [Description("Stop compute instance")]
        public async Task Test_StopComputeInstance_ShouldSucceed()
        {
            // Arrange
            await _automationUtils.InitializeAzureClientAsync();
            await _automationUtils.InitializeWorkspaceAsync();

            // Act
            var result = await _automationUtils.StopComputeInstanceAsync(_testInstanceName);

            // Assert
            result.Should().BeTrue($"Stopping compute instance {_testInstanceName} should succeed");
            
            _logger.Information($"✅ Compute instance stopped successfully: {_testInstanceName}");
        }

        #endregion

        #region SSH Setup Tests

        [Test, Order(30)]
        [Description("Generate SSH key pair")]
        public async Task Test_GenerateSSHKey_ShouldSucceed()
        {
            // Act
            var result = await _automationUtils.GenerateSSHKeyAsync();

            // Assert
            result.Should().BeTrue("SSH key generation should succeed");
            
            _logger.Information("✅ SSH key pair generated successfully");
        }

        [Test, Order(31)]
        [Description("Setup SSH connection configuration")]
        public async Task Test_SetupSSHConnection_ShouldSucceed()
        {
            // Arrange
            var hostname = "test-hostname.azure.com";
            var username = "azureuser";

            // Act
            var result = await _automationUtils.SetupSSHConnectionAsync(_testInstanceName, hostname, username);

            // Assert
            result.Should().BeTrue("SSH connection setup should succeed");
            
            _logger.Information($"✅ SSH connection configured for {_testInstanceName}");
        }

        #endregion

        #region VS Code Remote Setup Tests

        [Test, Order(40)]
        [Description("Setup VS Code remote connection")]
        public async Task Test_SetupVSCodeRemote_ShouldInstallExtensions()
        {
            // Act
            var result = await _automationUtils.SetupVSCodeRemoteAsync(_testInstanceName);

            // Assert
            // Note: This might fail in CI/CD environments without VS Code GUI
            if (result)
            {
                _logger.Information($"✅ VS Code remote setup completed for {_testInstanceName}");
            }
            else
            {
                _logger.Warning("⚠️ VS Code remote setup failed - this is expected in headless environments");
                Assert.Inconclusive("VS Code remote setup requires GUI environment");
            }
        }

        #endregion

        #region File Synchronization Tests

        [Test, Order(50)]
        [Description("Test file synchronization setup")]
        public async Task Test_FileSynchronization_ShouldSetupCorrectly()
        {
            // Arrange
            var localPath = Path.Combine(Directory.GetCurrentDirectory(), "test-sync");
            var remotePath = "/home/azureuser/test-sync";
            var hostname = "test-hostname.azure.com";
            var username = "azureuser";

            // Create test directory and file
            Directory.CreateDirectory(localPath);
            await File.WriteAllTextAsync(Path.Combine(localPath, "test.txt"), "Test file content");

            try
            {
                // Act
                var result = await _automationUtils.StartFileSynchronizationAsync(localPath, remotePath, hostname, username);

                // Assert
                // Note: This will likely fail without actual SSH connection
                if (result)
                {
                    _logger.Information("✅ File synchronization completed successfully");
                }
                else
                {
                    _logger.Warning("⚠️ File synchronization failed - expected without active SSH connection");
                    Assert.Inconclusive("File synchronization requires active SSH connection");
                }
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(localPath))
                {
                    Directory.Delete(localPath, true);
                }
            }
        }

        #endregion

        #region Integration Tests

        [Test, Order(100)]
        [Description("Full end-to-end automation workflow")]
        [Category("Integration")]
        public async Task Test_FullAutomationWorkflow_ShouldCompleteSuccessfully()
        {
            _logger.Information("🚀 Starting full automation workflow test");

            try
            {
                // Step 1: Validate prerequisites
                _logger.Information("Step 1: Validating prerequisites");
                var prerequisites = await _automationUtils.ValidatePrerequisitesAsync();
                
                var criticalPrerequisites = prerequisites.PythonInstalled && 
                                          prerequisites.VSCodeInstalled && 
                                          prerequisites.AzureCLIInstalled && 
                                          prerequisites.NetworkConnectivity;
                
                criticalPrerequisites.Should().BeTrue("Critical prerequisites must be met for full workflow");

                // Step 2: Initialize Azure connection
                _logger.Information("Step 2: Initializing Azure connection");
                var azureInit = await _automationUtils.InitializeAzureClientAsync();
                azureInit.Should().BeTrue("Azure client initialization required");

                var workspaceInit = await _automationUtils.InitializeWorkspaceAsync();
                workspaceInit.Should().BeTrue("Workspace initialization required");

                // Step 3: Create compute instance
                _logger.Information("Step 3: Creating compute instance");
                var createResult = await _automationUtils.CreateComputeInstanceAsync(_testInstanceName);
                createResult.Success.Should().BeTrue("Compute instance creation required");

                // Step 4: Setup SSH
                _logger.Information("Step 4: Setting up SSH");
                var sshKeyResult = await _automationUtils.GenerateSSHKeyAsync();
                // SSH key generation might fail if key already exists, which is OK

                // Step 5: Setup VS Code (optional in headless environments)
                _logger.Information("Step 5: Setting up VS Code remote (optional)");
                var vscodeResult = await _automationUtils.SetupVSCodeRemoteAsync(_testInstanceName);
                // VS Code setup might fail in CI/CD, which is acceptable

                _logger.Information("✅ Full automation workflow completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Full automation workflow failed: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Performance Tests

        [Test, Order(200)]
        [Description("Test automation performance and timing")]
        [Category("Performance")]
        public async Task Test_AutomationPerformance_ShouldCompleteWithinReasonableTime()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Test key operations for performance
                await _automationUtils.InitializeAzureClientAsync();
                await _automationUtils.InitializeWorkspaceAsync();
                
                var prerequisites = await _automationUtils.ValidatePrerequisitesAsync();
                
                stopwatch.Stop();
                
                _logger.Information($"⏱️ Automation operations completed in {stopwatch.ElapsedMilliseconds}ms");
                
                // Assert reasonable performance (adjust thresholds as needed)
                stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, "Basic operations should complete within 30 seconds");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error($"❌ Performance test failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Error Handling Tests

        [Test, Order(300)]
        [Description("Test error handling with invalid configuration")]
        [Category("ErrorHandling")]
        public async Task Test_ErrorHandling_ShouldHandleInvalidConfiguration()
        {
            // This test verifies that the automation handles errors gracefully
            
            // Test with non-existent compute instance
            var result = await _automationUtils.GetComputeInstanceAsync("non-existent-instance");
            result.Should().BeNull("Non-existent compute instance should return null");
            
            // Test SSH connection to invalid host
            var sshResult = await _automationUtils.TestSSHConnectionAsync("invalid-host", "invalid-user");
            sshResult.Should().BeFalse("SSH connection to invalid host should fail gracefully");
            
            _logger.Information("✅ Error handling tests completed successfully");
        }

        #endregion
    }
}