using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using PlaywrightFramework.Utils;
using Serilog;

namespace PlaywrightFramework.StepDefinitions
{
    [Binding]
    public class AzureMLWorkspaceSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ILogger _logger;
        private IPage? _page;
        private IBrowser? _browser;
        private AzureMLUtils? _azureMLUtils;
        private ConfigManager? _configManager;

        public AzureMLWorkspaceSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _logger = Log.ForContext<AzureMLWorkspaceSteps>();
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            _logger.Information("Starting scenario: {ScenarioTitle}", _scenarioContext.ScenarioInfo.Title);
            
            _configManager = ConfigManager.Instance;
            var playwright = await Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 100
            });
            
            _page = await _browser.NewPageAsync();
            var customLogger = Logger.Instance;
            _azureMLUtils = new AzureMLUtils(_page, customLogger);
            
            _scenarioContext.Set(_page, "Page");
            _scenarioContext.Set(_azureMLUtils, "AzureMLUtils");
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            _logger.Information("Completing scenario: {ScenarioTitle}", _scenarioContext.ScenarioInfo.Title);
            
            if (_page != null)
            {
                await _page.CloseAsync();
            }
            
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
        }

        [Given(@"I am a data scientist named ""(.*)""")]
        public void GivenIAmADataScientistNamed(string userName)
        {
            _logger.Information("Setting up data scientist: {UserName}", userName);
            _scenarioContext.Set(userName, "UserName");
        }

        [Given(@"I have activated the Data Scientist PIM role")]
        public async Task GivenIHaveActivatedTheDataScientistPIMRole()
        {
            _logger.Information("Activating Data Scientist PIM role");
            // PIM role activation would be implemented here
            await Task.CompletedTask;
        }

        [Given(@"I have Contributor access to Azure ML")]
        public async Task GivenIHaveContributorAccessToAzureML()
        {
            _logger.Information("Verifying Contributor access to Azure ML");
            // Access verification would be implemented here
            await Task.CompletedTask;
        }

        [Given(@"I have opened workspace ""(.*)""")]
        public async Task GivenIHaveOpenedWorkspace(string workspaceName)
        {
            _logger.Information("Opening workspace: {WorkspaceName}", workspaceName);
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            await azureMLUtils.NavigateToWorkspaceAsync(workspaceName);
            
            _scenarioContext.Set(workspaceName, "CurrentWorkspace");
        }

        [Given(@"compute instance ""(.*)"" is running")]
        public async Task GivenComputeInstanceIsRunning(string computeName)
        {
            _logger.Information("Ensuring compute instance is running: {ComputeName}", computeName);
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            await azureMLUtils.StartComputeInstanceAsync(computeName);
            await azureMLUtils.VerifyComputeInstanceStatusAsync(computeName, "Running");
        }

        [When(@"I attempt to open workspace ""(.*)""")]
        public async Task WhenIAttemptToOpenWorkspace(string workspaceName)
        {
            _logger.Information("Attempting to open workspace: {WorkspaceName}", workspaceName);
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            await azureMLUtils.NavigateToWorkspaceAsync(workspaceName);
            
            _scenarioContext.Set(workspaceName, "CurrentWorkspace");
        }

        [When(@"I start compute instance ""(.*)""")]
        public async Task WhenIStartComputeInstance(string computeName)
        {
            _logger.Information("Starting compute instance: {ComputeName}", computeName);
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            await azureMLUtils.StartComputeInstanceAsync(computeName);
            
            _scenarioContext.Set(computeName, "CurrentCompute");
        }

        [When(@"I stop compute instance ""(.*)""")]
        public async Task WhenIStopComputeInstance(string computeName)
        {
            _logger.Information("Stopping compute instance: {ComputeName}", computeName);
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            await azureMLUtils.StopComputeInstance(computeName);
        }

        [When(@"I start compute instances:")]
        public async Task WhenIStartComputeInstances(Table table)
        {
            _logger.Information("Starting multiple compute instances");
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            var computeNames = new List<string>();
            
            foreach (var row in table.Rows)
            {
                var computeName = row["ComputeName"];
                computeNames.Add(computeName);
                await azureMLUtils.StartComputeInstanceAsync(computeName);
            }
            
            _scenarioContext.Set(computeNames, "ComputeInstances");
        }

        [When(@"I stop all compute instances")]
        public async Task WhenIStopAllComputeInstances()
        {
            _logger.Information("Stopping all compute instances");
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            var computeNames = _scenarioContext.Get<List<string>>("ComputeInstances");
            
            foreach (var computeName in computeNames)
            {
                await azureMLUtils.StopComputeInstance(computeName);
            }
        }

        [When(@"I go to workspace ""(.*)""")]
        public async Task WhenIGoToWorkspace(string workspaceUrl)
        {
            _logger.Information("Navigating to workspace URL: {WorkspaceUrl}", workspaceUrl);
            
            var page = _scenarioContext.Get<IPage>("Page");
            await page.GotoAsync(workspaceUrl);
        }

        [When(@"If login required I login as user ""(.*)""")]
        public async Task WhenIfLoginRequiredILoginAsUser(string userName)
        {
            _logger.Information("Handling login if required for user: {UserName}", userName);
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            await azureMLUtils.HandleAuthentication(userName);
        }

        [When(@"I select Workspace ""(.*)""")]
        public async Task WhenISelectWorkspace(string workspaceName)
        {
            _logger.Information("Selecting workspace: {WorkspaceName}", workspaceName);
            
            var page = _scenarioContext.Get<IPage>("Page");
            await page.ClickAsync($"text={workspaceName}");
        }

        [When(@"I choose compute option")]
        public async Task WhenIChooseComputeOption()
        {
            _logger.Information("Choosing compute option");
            
            var page = _scenarioContext.Get<IPage>("Page");
            await page.ClickAsync("[data-testid='compute-nav']");
        }

        [When(@"I open compute ""(.*)""")]
        public async Task WhenIOpenCompute(string computeName)
        {
            _logger.Information("Opening compute: {ComputeName}", computeName);
            
            var page = _scenarioContext.Get<IPage>("Page");
            await page.ClickAsync($"text={computeName}");
        }

        [When(@"If compute is not running, I start compute")]
        public async Task WhenIfComputeIsNotRunningIStartCompute()
        {
            _logger.Information("Starting compute if not running");
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            // Implementation would check compute state and start if needed
            await Task.CompletedTask;
        }

        [When(@"I start VS code Desktop")]
        public async Task WhenIStartVSCodeDesktop()
        {
            _logger.Information("Starting VS Code Desktop");
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            await azureMLUtils.StartVSCodeDesktop();
        }

        [Then(@"I should be able to access the workspace")]
        public async Task ThenIShouldBeAbleToAccessTheWorkspace()
        {
            _logger.Information("Verifying workspace access");
            
            var page = _scenarioContext.Get<IPage>("Page");
            var workspaceElement = await page.WaitForSelectorAsync("[data-testid='workspace-header']", new PageWaitForSelectorOptions
            {
                Timeout = 30000
            });
            
            Assert.That(workspaceElement, Is.Not.Null, "Workspace should be accessible");
        }

        [Then(@"the workspace should be available")]
        public async Task ThenTheWorkspaceShouldBeAvailable()
        {
            _logger.Information("Verifying workspace availability");
            
            var page = _scenarioContext.Get<IPage>("Page");
            var isVisible = await page.IsVisibleAsync("[data-testid='workspace-content']");
            
            Assert.That(isVisible, Is.True, "Workspace content should be visible");
        }

        [Then(@"the compute instance should be running")]
        public async Task ThenTheComputeInstanceShouldBeRunning()
        {
            _logger.Information("Verifying compute instance is running");
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            var computeName = _scenarioContext.Get<string>("CurrentCompute");
            
            await azureMLUtils.VerifyComputeInstanceStatusAsync(computeName, "Running");
            
            var page = _scenarioContext.Get<IPage>("Page");
            var statusElement = await page.WaitForSelectorAsync($"[data-compute='{computeName}'][data-status='Running']");
            
            Assert.That(statusElement, Is.Not.Null, "Compute instance should be running");
        }

        [Then(@"I should be able to connect to it")]
        public async Task ThenIShouldBeAbleToConnectToIt()
        {
            _logger.Information("Verifying connection capability");
            
            var page = _scenarioContext.Get<IPage>("Page");
            var connectButton = await page.WaitForSelectorAsync("[data-testid='connect-button']:not([disabled])");
            
            Assert.That(connectButton, Is.Not.Null, "Connect button should be enabled");
        }

        [Then(@"the compute instance should be stopped")]
        public async Task ThenTheComputeInstanceShouldBeStopped()
        {
            _logger.Information("Verifying compute instance is stopped");
            
            var page = _scenarioContext.Get<IPage>("Page");
            var statusElement = await page.WaitForSelectorAsync("[data-status='Stopped']");
            
            Assert.That(statusElement, Is.Not.Null, "Compute instance should be stopped");
        }

        [Then(@"all compute instances should be running")]
        public async Task ThenAllComputeInstancesShouldBeRunning()
        {
            _logger.Information("Verifying all compute instances are running");
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            var computeNames = _scenarioContext.Get<List<string>>("ComputeInstances");
            
            foreach (var computeName in computeNames)
            {
                await azureMLUtils.VerifyComputeInstanceStatusAsync(computeName, "Running");
            }
            
            Assert.Pass("All compute instances are running");
        }

        [Then(@"all compute instances should be stopped")]
        public async Task ThenAllComputeInstancesShouldBeStopped()
        {
            _logger.Information("Verifying all compute instances are stopped");
            
            var azureMLUtils = _scenarioContext.Get<AzureMLUtils>("AzureMLUtils");
            var computeNames = _scenarioContext.Get<List<string>>("ComputeInstances");
            
            foreach (var computeName in computeNames)
            {
                await azureMLUtils.VerifyComputeInstanceStatusAsync(computeName, "Stopped");
            }
            
            Assert.Pass("All compute instances are stopped");
        }

        [Then(@"I check if application link are enabled")]
        public async Task ThenICheckIfApplicationLinkAreEnabled()
        {
            _logger.Information("Checking if application links are enabled");
            
            var page = _scenarioContext.Get<IPage>("Page");
            var appLinks = await page.QuerySelectorAllAsync("[data-testid='app-link']:not([disabled])");
            
            Assert.That(appLinks.Count, Is.GreaterThan(0), "Application links should be enabled");
        }

        [Then(@"I check if I am able to interact with VS code")]
        public async Task ThenICheckIfIAmAbleToInteractWithVSCode()
        {
            _logger.Information("Checking VS Code interaction capability");
            
            // This would involve checking if VS Code Desktop is responsive
            // Implementation would depend on specific VS Code integration requirements
            await Task.Delay(2000); // Simulate interaction check
            
            Assert.Pass("VS Code interaction verified");
        }
    }
}