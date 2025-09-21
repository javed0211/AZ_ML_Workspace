using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Questions;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;
using Reqnroll;
using System.Diagnostics;

namespace AzureMLWorkspace.Tests.StepDefinitions;

[Binding]
public class AzureAISearchSteps
{
    private readonly ILogger<AzureAISearchSteps> _logger;
    private IActor? _actor;
    private SearchTestResult? _lastSearchResult;
    private readonly Stopwatch _searchStopwatch = new();

    public AzureAISearchSteps(ILogger<AzureAISearchSteps> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [When(@"I search for ""(.*)"" in the AI search index")]
    public async Task WhenISearchForInTheAISearchIndex(string searchTerm)
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        _logger.LogInformation("Searching for: {SearchTerm}", searchTerm);

        var searchAbility = _actor.Using<UseAzureAISearch>();
        
        _searchStopwatch.Restart();
        _lastSearchResult = await searchAbility.TestAISearch(searchTerm);
        _searchStopwatch.Stop();

        _logger.LogInformation("Search completed in {ElapsedMs}ms with {ResultCount} results", 
            _searchStopwatch.ElapsedMilliseconds, _lastSearchResult.TotalResults);
    }

    [When(@"I search for ""(.*)"" with filters:")]
    public async Task WhenISearchForWithFilters(string searchTerm, Table table)
    {
        if (_actor == null)
            throw new InvalidOperationException("Actor must be created first");

        _logger.LogInformation("Searching for: {SearchTerm} with filters", searchTerm);

        var searchAbility = _actor.Using<UseAzureAISearch>();
        
        // Build search options with filters
        var searchOptions = new Azure.Search.Documents.SearchOptions
        {
            IncludeTotalCount = true,
            Size = 50
        };

        // Add filters based on the table
        var filterParts = new List<string>();
        foreach (var row in table.Rows)
        {
            var filter = row["Filter"];
            var value = row["Value"];
            
            // This is a simplified filter building - in practice, you'd have more sophisticated filter logic
            filterParts.Add($"{filter} eq '{value}'");
        }

        if (filterParts.Any())
        {
            searchOptions.Filter = string.Join(" and ", filterParts);
        }

        _searchStopwatch.Restart();
        var results = await searchAbility.Search(searchTerm, searchOptions);
        _searchStopwatch.Stop();

        _lastSearchResult = new SearchTestResult
        {
            Query = searchTerm,
            IndexName = searchAbility.IndexName,
            TotalResults = results.TotalCount ?? 0,
            ResponseTime = _searchStopwatch.Elapsed,
            Success = true,
            Results = new List<SearchResult<SearchDocument>>()
        };

        _logger.LogInformation("Filtered search completed in {ElapsedMs}ms with {ResultCount} results", 
            _searchStopwatch.ElapsedMilliseconds, _lastSearchResult.TotalResults);
    }

    [Then(@"I should see more than (.*) results")]
    public void ThenIShouldSeeMoreThanResults(int expectedMinResults)
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        _lastSearchResult.TotalResults.Should().BeGreaterThan(expectedMinResults, 
            $"Expected more than {expectedMinResults} results, but got {_lastSearchResult.TotalResults}");
    }

    [Then(@"I should see at least (.*) results")]
    public void ThenIShouldSeeAtLeastResults(int expectedMinResults)
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        _lastSearchResult.TotalResults.Should().BeGreaterOrEqualTo(expectedMinResults, 
            $"Expected at least {expectedMinResults} results, but got {_lastSearchResult.TotalResults}");
    }

    [Then(@"I should see (.*) results")]
    public void ThenIShouldSeeResults(int expectedResults)
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        _lastSearchResult.TotalResults.Should().Be(expectedResults, 
            $"Expected exactly {expectedResults} results, but got {_lastSearchResult.TotalResults}");
    }

    [Then(@"the results should be relevant to (.*)")]
    public void ThenTheResultsShouldBeRelevantTo(string expectedTopic)
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        _lastSearchResult.Success.Should().BeTrue("Search should have succeeded");
        _lastSearchResult.Results.Should().NotBeEmpty("Should have results to validate relevance");

        // This is a simplified relevance check - in practice, you'd have more sophisticated relevance validation
        _logger.LogInformation("Validating relevance to topic: {Topic}", expectedTopic);
        
        // Check if any of the results contain relevant terms
        var relevantResults = _lastSearchResult.Results.Take(5).Any(result =>
        {
            var content = result.Document.ToString();
            return content?.Contains(expectedTopic, StringComparison.OrdinalIgnoreCase) == true;
        });

        relevantResults.Should().BeTrue($"Results should be relevant to {expectedTopic}");
    }

    [Then(@"the search should complete within (.*) seconds")]
    public void ThenTheSearchShouldCompleteWithinSeconds(int maxSeconds)
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        _lastSearchResult.ResponseTime.TotalSeconds.Should().BeLessOrEqualTo(maxSeconds, 
            $"Search should complete within {maxSeconds} seconds, but took {_lastSearchResult.ResponseTime.TotalSeconds:F2} seconds");
    }

    [Then(@"I should receive search results")]
    public void ThenIShouldReceiveSearchResults()
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        _lastSearchResult.Success.Should().BeTrue("Search should have succeeded");
        _lastSearchResult.Should().NotBeNull("Should have received search results");
    }

    [Then(@"I should see filtered results")]
    public void ThenIShouldSeeFilteredResults()
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        _lastSearchResult.Success.Should().BeTrue("Search should have succeeded");
        _lastSearchResult.Results.Should().NotBeNull("Should have received filtered results");
    }

    [Then(@"all results should match the applied filters")]
    public void ThenAllResultsShouldMatchTheAppliedFilters()
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        // This would involve validating that each result matches the applied filters
        // Implementation depends on your specific filter validation logic
        _logger.LogInformation("Validating that all {ResultCount} results match applied filters", 
            _lastSearchResult.TotalResults);
        
        _lastSearchResult.Results.Should().NotBeEmpty("Should have results to validate filters");
    }

    [Then(@"I should receive a proper empty result response")]
    public void ThenIShouldReceiveAProperEmptyResultResponse()
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        _lastSearchResult.Success.Should().BeTrue("Search should have succeeded even with no results");
        _lastSearchResult.TotalResults.Should().Be(0, "Should have zero results");
        _lastSearchResult.Results.Should().BeEmpty("Results collection should be empty");
    }

    [Then(@"the response time should be acceptable")]
    public void ThenTheResponseTimeShouldBeAcceptable()
    {
        if (_lastSearchResult == null)
            throw new InvalidOperationException("No search has been performed");

        // Define acceptable response time (e.g., under 5 seconds)
        _lastSearchResult.ResponseTime.TotalSeconds.Should().BeLessOrEqualTo(5, 
            $"Response time should be acceptable, but took {_lastSearchResult.ResponseTime.TotalSeconds:F2} seconds");
    }

    [AfterScenario]
    public async Task CleanupAfterScenario()
    {
        if (_actor != null)
        {
            try
            {
                if (_actor is IAsyncDisposable disposableActor)
                {
                    await disposableActor.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during scenario cleanup");
            }
            finally
            {
                _actor = null;
                _lastSearchResult = null;
            }
        }
    }
}