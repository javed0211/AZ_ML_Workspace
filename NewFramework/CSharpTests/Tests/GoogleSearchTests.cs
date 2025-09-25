using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using PlaywrightFramework.Utils;
using NUnit.Framework;

namespace PlaywrightFramework.Tests
{
    [TestFixture]
    public class GoogleSearchTests : PageTest
    {
        private PlaywrightUtils _utils;
        private Logger _logger;

        [SetUp]
        public async Task Setup()
        {
            _utils = new PlaywrightUtils(Page);
            _logger = Logger.Instance;
            
            _logger.LogTestStart("Google Search Test Setup");
        }

        [TearDown]
        public async Task TearDown()
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Passed ? "PASSED" : "FAILED";
            _logger.LogTestEnd(TestContext.CurrentContext.Test.Name, status);
            
            if (TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Passed)
            {
                await _utils.TakeScreenshotAsync($"failed-{TestContext.CurrentContext.Test.Name.Replace(" ", "-")}");
            }
        }

        [Test]
        public async Task ShouldSearchForMarshallHeadphonesOnGoogle()
        {
            _logger.LogStep("Navigate to Google homepage");
            await _utils.NavigateToAsync("https://www.google.com");
            
            _logger.LogStep("Verify Google homepage loaded");
            await _utils.AssertTitleContainsAsync("Google");
            
            _logger.LogStep("Accept cookies if present");
            // Handle cookie consent if it appears
            try
            {
                await _utils.WaitForElementAsync("[id*=\"accept\"], [id*=\"agree\"], button:has-text(\"Accept all\")", 3000);
                await _utils.ClickAsync("[id*=\"accept\"], [id*=\"agree\"], button:has-text(\"Accept all\")");
                _logger.Info("Cookie consent accepted");
            }
            catch (Exception)
            {
                _logger.Info("No cookie consent dialog found, continuing...");
            }
            
            _logger.LogStep("Find and click on search input");
            // Google search input can have different selectors
            var searchSelectors = new[]
            {
                "input[name=\"q\"]",
                "textarea[name=\"q\"]",
                "[data-testid=\"search-input\"]",
                "#APjFqb"
            };
            
            string? searchInput = null;
            foreach (var selector in searchSelectors)
            {
                try
                {
                    await _utils.WaitForElementAsync(selector, 2000);
                    searchInput = selector;
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            
            if (searchInput == null)
            {
                throw new Exception("Could not find Google search input");
            }
            
            _logger.LogStep("Type search query: marshall headphones");
            await _utils.FillAsync(searchInput, "marshall headphones");
            
            _logger.LogStep("Press Enter to search");
            await _utils.PressKeyOnElementAsync(searchInput, "Enter");
            
            _logger.LogStep("Wait for search results to load");
            await _utils.WaitForUrlAsync(new System.Text.RegularExpressions.Regex("search"));
            
            _logger.LogStep("Verify search results are displayed");
            // Wait for search results container
            await _utils.WaitForElementAsync("#search, #rso, [data-testid=\"search-results\"]");
            
            _logger.LogStep("Verify search results contain Marshall");
            var resultsText = await _utils.GetTextAsync("#search, #rso, [data-testid=\"search-results\"]");
            Assert.That(resultsText.ToLower(), Does.Contain("marshall"));
            
            _logger.LogStep("Take screenshot of search results");
            await _utils.TakeScreenshotAsync("google-search-marshall-headphones");
            
            _logger.LogStep("Verify at least one search result is visible");
            var resultCount = await _utils.GetElementCountAsync("h3, [data-testid=\"result-title\"]");
            Assert.That(resultCount, Is.GreaterThan(0));
            
            _logger.Info("Google search test completed successfully");
        }

        [Test]
        public async Task ShouldSearchAndClickOnFirstMarshallResult()
        {
            _logger.LogStep("Navigate to Google homepage");
            await _utils.NavigateToAsync("https://www.google.com");
            
            _logger.LogStep("Handle cookie consent if present");
            try
            {
                await _utils.WaitForElementAsync("[id*=\"accept\"], [id*=\"agree\"], button:has-text(\"Accept all\")", 3000);
                await _utils.ClickAsync("[id*=\"accept\"], [id*=\"agree\"], button:has-text(\"Accept all\")");
            }
            catch (Exception)
            {
                _logger.Info("No cookie consent dialog found");
            }
            
            _logger.LogStep("Search for Marshall headphones");
            var searchInput = "input[name=\"q\"], textarea[name=\"q\"], #APjFqb";
            await _utils.WaitForElementAsync(searchInput);
            await _utils.FillAsync(searchInput, "marshall headphones");
            await _utils.PressKeyOnElementAsync(searchInput, "Enter");
            
            _logger.LogStep("Wait for search results");
            await _utils.WaitForUrlAsync(new System.Text.RegularExpressions.Regex("search"));
            await _utils.WaitForElementAsync("#search, #rso");
            
            _logger.LogStep("Find and click first Marshall-related result");
            // Look for the first result that contains "Marshall" in the title
            var firstResult = "h3:has-text(\"Marshall\"), [data-testid=\"result-title\"]:has-text(\"Marshall\")";
            
            try
            {
                await _utils.WaitForElementAsync(firstResult, 5000);
                await _utils.TakeScreenshotAsync("before-clicking-result");
                
                // Get the text of the first result for logging
                var resultText = await _utils.GetTextAsync(firstResult);
                _logger.LogAction($"Clicking on result: {resultText}");
                
                await _utils.ClickAsync(firstResult);
                
                _logger.LogStep("Wait for new page to load");
                await _utils.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                _logger.LogStep("Verify we navigated to a Marshall-related page");
                var currentUrl = await _utils.GetCurrentUrlAsync();
                var pageTitle = await _utils.GetTitleAsync();
                
                _logger.Info($"Navigated to: {currentUrl}");
                _logger.Info($"Page title: {pageTitle}");
                
                // Take final screenshot
                await _utils.TakeScreenshotAsync("marshall-page-loaded");
                
                // Verify we're on a relevant page (URL or title should contain Marshall)
                var isMarshallPage = currentUrl.ToLower().Contains("marshall") || 
                                   pageTitle.ToLower().Contains("marshall");
                
                Assert.That(isMarshallPage, Is.True);
            }
            catch (Exception ex)
            {
                _logger.Error("Could not find or click Marshall result", ex);
                await _utils.TakeScreenshotAsync("marshall-result-not-found");
                throw;
            }
            
            _logger.Info("Marshall headphones search and navigation test completed successfully");
        }
    }
}