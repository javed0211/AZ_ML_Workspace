using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Tests
{
    [TestFixture]
    public class ExampleWebTests : PageTest
    {
        private PlaywrightUtils? _utils;
        private Logger? _logger;
        private ConfigManager? _config;

        [SetUp]
        public async Task Setup()
        {
            _utils = new PlaywrightUtils(Page);
            _logger = Logger.Instance;
            _config = ConfigManager.Instance;
            
            _logger.LogTestStart($"Web Test Setup - {TestContext.CurrentContext.Test.Name}");
        }

        [TearDown]
        public async Task TearDown()
        {
            var testResult = TestContext.CurrentContext.Result.Outcome.Status;
            var status = testResult == NUnit.Framework.Interfaces.TestStatus.Passed ? "PASSED" : "FAILED";
            
            _logger?.LogTestEnd(TestContext.CurrentContext.Test.Name, status);
            
            if (testResult == NUnit.Framework.Interfaces.TestStatus.Failed && _utils != null)
            {
                await _utils.TakeScreenshotAsync($"failed-{TestContext.CurrentContext.Test.Name.Replace(" ", "-")}");
            }
        }

        [Test]
        public async Task ShouldNavigateToHomepageAndVerifyTitle()
        {
            _logger!.LogStep("Navigate to homepage");
            await _utils!.NavigateToAsync(_config!.GetCurrentEnvironment().BaseUrl);
            
            _logger.LogStep("Verify page title");
            var title = await _utils.GetTitleAsync();
            Assert.That(title, Is.Not.Empty, "Page title should not be empty");
            
            _logger.LogStep("Take screenshot of homepage");
            await _utils.TakeScreenshotAsync("homepage");
        }

        [Test]
        public async Task ShouldPerformLoginFlow()
        {
            var env = _config!.GetCurrentEnvironment();
            
            _logger!.LogStep("Navigate to login page");
            await _utils!.NavigateToAsync($"{env.BaseUrl}/login");
            
            _logger.LogStep("Fill login credentials");
            await _utils.FillAsync("#username", env.Username);
            await _utils.FillAsync("#password", env.Password);
            
            _logger.LogStep("Click login button");
            await _utils.ClickAsync("#login-button");
            
            _logger.LogStep("Wait for dashboard to load");
            await _utils.WaitForUrlAsync(new System.Text.RegularExpressions.Regex("dashboard"));
            
            _logger.LogStep("Verify successful login");
            await _utils.AssertElementVisibleAsync(".dashboard-header");
        }

        [Test]
        public async Task ShouldTestFormInteractions()
        {
            _logger!.LogStep("Navigate to form page");
            await _utils!.NavigateToAsync($"{_config!.GetCurrentEnvironment().BaseUrl}/form");
            
            _logger.LogStep("Fill text input");
            await _utils.TypeAsync("#name", "John Doe");
            
            _logger.LogStep("Select dropdown option");
            await _utils.SelectByTextAsync("#country", "United States");
            
            _logger.LogStep("Check checkbox");
            await _utils.CheckAsync("#agree-terms");
            
            _logger.LogStep("Upload file");
            await _utils.UploadFileAsync("#file-upload", "./TestData/sample-file.txt");
            
            _logger.LogStep("Submit form");
            await _utils.ClickAsync("#submit-button");
            
            _logger.LogStep("Verify form submission");
            await _utils.AssertTextContainsAsync(".success-message", "Form submitted successfully");
        }

        [Test]
        public async Task ShouldTestElementInteractionsAndAssertions()
        {
            await _utils!.NavigateToAsync($"{_config!.GetCurrentEnvironment().BaseUrl}/elements");
            
            // Test visibility assertions
            await _utils.AssertElementVisibleAsync("#visible-element");
            await _utils.AssertElementHiddenAsync("#hidden-element");
            
            // Test text assertions
            await _utils.AssertTextAsync("#welcome-message", "Welcome to our site!");
            await _utils.AssertTextContainsAsync("#description", "This is a test");
            
            // Test element states
            await _utils.AssertElementEnabledAsync("#enabled-button");
            await _utils.AssertElementDisabledAsync("#disabled-button");
            
            // Test element count
            await _utils.AssertElementCountAsync(".list-item", 5);
            
            // Test hover and focus
            await _utils.HoverAsync("#hover-element");
            await _utils.FocusAsync("#focus-element");
            
            // Test scroll
            await _utils.ScrollToElementAsync("#bottom-element");
            await _utils.ScrollToTopAsync();
        }

        [Test]
        public async Task ShouldTestAdvancedInteractions()
        {
            await _utils!.NavigateToAsync($"{_config!.GetCurrentEnvironment().BaseUrl}/advanced");
            
            // Test drag and drop
            await _utils.DragAndDropAsync("#draggable", "#droppable");
            
            // Test double click
            await _utils.DoubleClickAsync("#double-click-element");
            
            // Test right click
            await _utils.RightClickAsync("#context-menu-element");
            
            // Test keyboard interactions
            await _utils.PressKeyAsync("Escape");
            await _utils.PressKeyOnElementAsync("#input-field", "Enter");
        }

        [Test]
        public async Task ShouldTestResponsiveDesign()
        {
            await _utils!.NavigateToAsync(_config!.GetCurrentEnvironment().BaseUrl);
            
            // Test mobile viewport
            await Page.SetViewportSizeAsync(375, 667);
            await _utils.AssertElementVisibleAsync(".mobile-menu");
            
            // Test tablet viewport
            await Page.SetViewportSizeAsync(768, 1024);
            await _utils.AssertElementVisibleAsync(".tablet-layout");
            
            // Test desktop viewport
            await Page.SetViewportSizeAsync(1920, 1080);
            await _utils.AssertElementVisibleAsync(".desktop-layout");
        }

        [Test]
        [TestCase("dev")]
        [TestCase("qa")]
        public async Task ShouldTestMultipleEnvironments(string environment)
        {
            _config!.SetEnvironment(environment);
            var env = _config.GetCurrentEnvironment();
            
            _logger!.LogStep($"Testing environment: {environment}");
            await _utils!.NavigateToAsync(env.BaseUrl);
            
            var title = await _utils.GetTitleAsync();
            Assert.That(title, Is.Not.Empty, $"Page title should not be empty for {environment} environment");
        }
    }
}