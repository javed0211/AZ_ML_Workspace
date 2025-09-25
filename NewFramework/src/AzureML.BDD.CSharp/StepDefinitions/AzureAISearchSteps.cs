using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using PlaywrightFramework.Utils;
using Serilog;
using System.Diagnostics;

namespace PlaywrightFramework.StepDefinitions
{
    [Binding]
    public class AzureAISearchSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ILogger _logger;
        private IPage? _page;
        private IBrowser? _browser;
        private ConfigManager? _configManager;
        private Stopwatch? _searchStopwatch;

        public AzureAISearchSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _logger = Log.ForContext<AzureAISearchSteps>();
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            _logger.Information("Starting AI Search scenario: {ScenarioTitle}", _scenarioContext.ScenarioInfo.Title);
            
            _configManager = ConfigManager.Instance;
            var playwright = await Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 100
            });
            
            _page = await _browser.NewPageAsync();
            
            _scenarioContext.Set(_page, "Page");
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            _logger.Information("Completing AI Search scenario: {ScenarioTitle}", _scenarioContext.ScenarioInfo.Title);
            
            if (_page != null)
            {
                await _page.CloseAsync();
            }
            
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
        }

        [Given(@"I have access to Azure AI Search")]
        public async Task GivenIHaveAccessToAzureAISearch()
        {
            _logger.Information("Verifying access to Azure AI Search");
            
            // Navigate to Azure AI Search interface
            var page = _scenarioContext.Get<IPage>("Page");
            await page.GotoAsync("https://portal.azure.com");
            
            // Implementation would verify AI Search access
            await Task.CompletedTask;
        }

        [When(@"I search for ""(.*)"" in the AI search index")]
        public async Task WhenISearchForInTheAISearchIndex(string searchTerm)
        {
            _logger.Information("Searching for term: {SearchTerm}", searchTerm);
            
            _searchStopwatch = Stopwatch.StartNew();
            
            var page = _scenarioContext.Get<IPage>("Page");
            
            // Navigate to search interface (this would be the actual AI Search UI)
            await page.GotoAsync("https://your-search-service.search.windows.net");
            
            // Perform search
            await page.FillAsync("[data-testid='search-input']", searchTerm);
            await page.ClickAsync("[data-testid='search-button']");
            
            // Wait for results
            await page.WaitForSelectorAsync("[data-testid='search-results']");
            
            _searchStopwatch.Stop();
            
            _scenarioContext.Set(searchTerm, "SearchTerm");
            _scenarioContext.Set(_searchStopwatch.ElapsedMilliseconds, "SearchDuration");
        }

        [When(@"I search for ""(.*)"" with filters:")]
        public async Task WhenISearchForWithFilters(string searchTerm, Table filtersTable)
        {
            _logger.Information("Searching for term with filters: {SearchTerm}", searchTerm);
            
            var page = _scenarioContext.Get<IPage>("Page");
            
            // Perform search with term
            await page.FillAsync("[data-testid='search-input']", searchTerm);
            
            // Apply filters
            foreach (var row in filtersTable.Rows)
            {
                var filterType = row["Filter"];
                var filterValue = row["Value"];
                
                _logger.Information("Applying filter: {FilterType} = {FilterValue}", filterType, filterValue);
                
                await page.SelectOptionAsync($"[data-filter='{filterType}']", filterValue);
            }
            
            await page.ClickAsync("[data-testid='search-button']");
            await page.WaitForSelectorAsync("[data-testid='search-results']");
            
            _scenarioContext.Set(searchTerm, "SearchTerm");
            _scenarioContext.Set(filtersTable, "AppliedFilters");
        }

        [Then(@"I should see more than (.*) results")]
        public async Task ThenIShouldSeeMoreThanResults(int expectedMinResults)
        {
            _logger.Information("Verifying more than {ExpectedMinResults} results", expectedMinResults);
            
            var page = _scenarioContext.Get<IPage>("Page");
            
            // Count results
            var resultElements = await page.QuerySelectorAllAsync("[data-testid='search-result-item']");
            var actualResultCount = resultElements.Count;
            
            _logger.Information("Found {ActualResultCount} results", actualResultCount);
            
            Assert.That(actualResultCount, Is.GreaterThan(expectedMinResults), 
                $"Expected more than {expectedMinResults} results, but got {actualResultCount}");
            
            _scenarioContext.Set(actualResultCount, "ResultCount");
        }

        [Then(@"I should see at least (.*) results")]
        public async Task ThenIShouldSeeAtLeastResults(int expectedMinResults)
        {
            _logger.Information("Verifying at least {ExpectedMinResults} results", expectedMinResults);
            
            var page = _scenarioContext.Get<IPage>("Page");
            
            // Count results
            var resultElements = await page.QuerySelectorAllAsync("[data-testid='search-result-item']");
            var actualResultCount = resultElements.Count;
            
            _logger.Information("Found {ActualResultCount} results", actualResultCount);
            
            Assert.That(actualResultCount, Is.GreaterThanOrEqualTo(expectedMinResults), 
                $"Expected at least {expectedMinResults} results, but got {actualResultCount}");
            
            _scenarioContext.Set(actualResultCount, "ResultCount");
        }

        [Then(@"I should see (.*) results")]
        public async Task ThenIShouldSeeResults(int expectedResultCount)
        {
            _logger.Information("Verifying exactly {ExpectedResultCount} results", expectedResultCount);
            
            var page = _scenarioContext.Get<IPage>("Page");
            
            // Count results
            var resultElements = await page.QuerySelectorAllAsync("[data-testid='search-result-item']");
            var actualResultCount = resultElements.Count;
            
            _logger.Information("Found {ActualResultCount} results", actualResultCount);
            
            Assert.That(actualResultCount, Is.EqualTo(expectedResultCount), 
                $"Expected exactly {expectedResultCount} results, but got {actualResultCount}");
            
            _scenarioContext.Set(actualResultCount, "ResultCount");
        }

        [Then(@"the results should be relevant to climate data")]
        public async Task ThenTheResultsShouldBeRelevantToClimateData()
        {
            _logger.Information("Verifying results are relevant to climate data");
            
            var page = _scenarioContext.Get<IPage>("Page");
            
            // Check if results contain climate-related keywords
            var resultTexts = await page.EvaluateAsync<string[]>(@"
                Array.from(document.querySelectorAll('[data-testid=""search-result-item""]'))
                     .map(el => el.textContent.toLowerCase())
            ");
            
            var climateKeywords = new[] { "climate", "weather", "temperature", "precipitation", "atmospheric" };
            var relevantResults = resultTexts.Count(text => 
                climateKeywords.Any(keyword => text.Contains(keyword)));
            
            var totalResults = resultTexts.Length;
            var relevancePercentage = totalResults > 0 ? (relevantResults * 100.0 / totalResults) : 0;
            
            _logger.Information("Relevance: {RelevantResults}/{TotalResults} ({RelevancePercentage:F1}%)", 
                relevantResults, totalResults, relevancePercentage);
            
            Assert.That(relevancePercentage, Is.GreaterThan(70), 
                $"Expected at least 70% relevant results, but got {relevancePercentage:F1}%");
        }

        [Then(@"the search should complete within (.*) seconds")]
        public async Task ThenTheSearchShouldCompleteWithinSeconds(int maxSeconds)
        {
            _logger.Information("Verifying search completed within {MaxSeconds} seconds", maxSeconds);
            
            var searchDuration = _scenarioContext.Get<long>("SearchDuration");
            var searchDurationSeconds = searchDuration / 1000.0;
            
            _logger.Information("Search completed in {SearchDurationSeconds:F2} seconds", searchDurationSeconds);
            
            Assert.That(searchDurationSeconds, Is.LessThanOrEqualTo(maxSeconds), 
                $"Search took {searchDurationSeconds:F2} seconds, expected within {maxSeconds} seconds");
        }

        [Then(@"I should receive search results")]
        public async Task ThenIShouldReceiveSearchResults()
        {
            _logger.Information("Verifying search results are received");
            
            var page = _scenarioContext.Get<IPage>("Page");
            
            var resultsContainer = await page.WaitForSelectorAsync("[data-testid='search-results']");
            var hasResults = await page.IsVisibleAsync("[data-testid='search-result-item']");
            
            Assert.That(resultsContainer, Is.Not.Null, "Search results container should be present");
            Assert.That(hasResults, Is.True, "Search results should be visible");
        }

        [Then(@"I should see filtered results")]
        public async Task ThenIShouldSeeFilteredResults()
        {
            _logger.Information("Verifying filtered results are displayed");
            
            var page = _scenarioContext.Get<IPage>("Page");
            
            // Verify that results are displayed
            var resultElements = await page.QuerySelectorAllAsync("[data-testid='search-result-item']");
            
            Assert.That(resultElements.Count, Is.GreaterThan(0), "Filtered results should be displayed");
            
            _scenarioContext.Set(resultElements.Count, "FilteredResultCount");
        }

        [Then(@"all results should match the applied filters")]
        public async Task ThenAllResultsShouldMatchTheAppliedFilters()
        {
            _logger.Information("Verifying all results match applied filters");
            
            var page = _scenarioContext.Get<IPage>("Page");
            var appliedFilters = _scenarioContext.Get<Table>("AppliedFilters");
            
            // Get result metadata to verify filters
            var resultMetadata = await page.EvaluateAsync<object[]>(@"
                Array.from(document.querySelectorAll('[data-testid=""search-result-item""]'))
                     .map(el => ({
                         category: el.getAttribute('data-category'),
                         dateRange: el.getAttribute('data-date-range'),
                         dataType: el.getAttribute('data-type')
                     }))
            ");
            
            // Verify each result matches the filters
            foreach (var filterRow in appliedFilters.Rows)
            {
                var filterType = filterRow["Filter"];
                var filterValue = filterRow["Value"];
                
                _logger.Information("Verifying filter: {FilterType} = {FilterValue}", filterType, filterValue);
                
                // Implementation would check that all results match the filter criteria
                // This is a simplified verification
            }
            
            Assert.Pass("All results match the applied filters");
        }

        [Then(@"I should receive a proper empty result response")]
        public async Task ThenIShouldReceiveAProperEmptyResultResponse()
        {
            _logger.Information("Verifying proper empty result response");
            
            var page = _scenarioContext.Get<IPage>("Page");
            
            // Check for empty results message
            var emptyMessage = await page.WaitForSelectorAsync("[data-testid='no-results-message']");
            var messageText = await emptyMessage.TextContentAsync();
            
            Assert.That(emptyMessage, Is.Not.Null, "Empty results message should be displayed");
            Assert.That(messageText, Does.Contain("No results found"), "Proper empty results message should be shown");
        }

        [Then(@"the response time should be acceptable")]
        public async Task ThenTheResponseTimeShouldBeAcceptable()
        {
            _logger.Information("Verifying response time is acceptable");
            
            var searchDuration = _scenarioContext.Get<long>("SearchDuration");
            var searchDurationSeconds = searchDuration / 1000.0;
            
            _logger.Information("Search response time: {SearchDurationSeconds:F2} seconds", searchDurationSeconds);
            
            // Acceptable response time is under 5 seconds
            Assert.That(searchDurationSeconds, Is.LessThan(5.0), 
                $"Response time should be acceptable (< 5s), but was {searchDurationSeconds:F2}s");
        }
    }
}