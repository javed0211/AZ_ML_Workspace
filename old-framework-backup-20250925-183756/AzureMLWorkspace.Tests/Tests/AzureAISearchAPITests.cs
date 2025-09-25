using AzureMLWorkspace.Tests.Framework;
using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Questions;

namespace AzureMLWorkspace.Tests.Tests;

/// <summary>
/// API tests for Azure AI Search functionality
/// </summary>
[TestFixture]
[Category("API")]
[Category("AzureAISearch")]
public class AzureAISearchAPITests : TestBase
{
    [Test]
    [Description("Verify AI Search returns results for climate data queries")]
    public async Task Should_Return_Climate_Data_Search_Results()
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(UseAzureAISearch.WithDefaultConfiguration());

        await javed.Using<UseAzureAISearch>().InitializeAsync();

        // Act & Assert
        var result = await javed.AsksFor(Validate.AISearchResults("climate-data"));
        result.IsValid.Should().BeTrue("Search should return valid results");
        result.Count.Should().BeGreaterThan(0, "Search should return at least one result");
    }

    [Test]
    [Description("Verify search performance meets requirements")]
    public async Task Should_Complete_Search_Within_Performance_Threshold()
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(UseAzureAISearch.WithDefaultConfiguration());

        await javed.Using<UseAzureAISearch>().InitializeAsync();

        var searchAbility = javed.Using<UseAzureAISearch>();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var searchResult = await searchAbility.TestAISearch("machine learning");
        stopwatch.Stop();

        // Assert
        searchResult.Success.Should().BeTrue("Search should complete successfully");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "Search should complete within 2 seconds");
        searchResult.ResponseTime.TotalSeconds.Should().BeLessThan(2, "Response time should be under 2 seconds");
        
        Logger.LogInformation("Search completed in {ElapsedMs}ms with {ResultCount} results", 
            stopwatch.ElapsedMilliseconds, searchResult.TotalResults);
    }

    [Test]
    [Description("Verify search handles empty results gracefully")]
    public async Task Should_Handle_Empty_Search_Results_Gracefully()
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(UseAzureAISearch.WithDefaultConfiguration());

        await javed.Using<UseAzureAISearch>().InitializeAsync();

        var searchAbility = javed.Using<UseAzureAISearch>();

        // Act
        var searchResult = await searchAbility.TestAISearch("nonexistent-data-xyz123");

        // Assert
        searchResult.Success.Should().BeTrue("Search should complete successfully even with no results");
        searchResult.TotalResults.Should().Be(0, "Should return zero results for non-existent data");
        searchResult.Results.Should().BeEmpty("Results collection should be empty");
        searchResult.Error.Should().BeNullOrEmpty("Should not have any errors");
    }

    [Test]
    [Description("Verify search index statistics are accessible")]
    public async Task Should_Retrieve_Search_Index_Statistics()
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(UseAzureAISearch.WithDefaultConfiguration());

        await javed.Using<UseAzureAISearch>().InitializeAsync();

        var searchAbility = javed.Using<UseAzureAISearch>();

        // Act
        var stats = await searchAbility.GetIndexStatistics();

        // Assert
        stats.Should().NotBeNull("Index statistics should be available");
        stats.DocumentCount.Should().BeGreaterOrEqualTo(0, "Document count should be non-negative");
        stats.StorageSize.Should().BeGreaterOrEqualTo(0, "Storage size should be non-negative");
        
        Logger.LogInformation("Index statistics - Documents: {DocumentCount}, Storage: {StorageSize} bytes", 
            stats.DocumentCount, stats.StorageSize);
    }

    [Test]
    [Description("Verify search with different query types")]
    [TestCase("weather", 5, "Simple term search")]
    [TestCase("temperature AND precipitation", 3, "Boolean AND search")]
    [TestCase("climate OR weather", 10, "Boolean OR search")]
    [TestCase("\"climate change\"", 2, "Phrase search")]
    public async Task Should_Handle_Different_Query_Types(string query, int minExpectedResults, string queryType)
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(UseAzureAISearch.WithDefaultConfiguration());

        await javed.Using<UseAzureAISearch>().InitializeAsync();

        var searchAbility = javed.Using<UseAzureAISearch>();

        // Act
        var searchResult = await searchAbility.TestAISearch(query);

        // Assert
        searchResult.Success.Should().BeTrue($"{queryType} should complete successfully");
        searchResult.TotalResults.Should().BeGreaterOrEqualTo(minExpectedResults, 
            $"{queryType} should return at least {minExpectedResults} results");
        
        Logger.LogInformation("{QueryType} '{Query}' returned {ResultCount} results", 
            queryType, query, searchResult.TotalResults);
    }

    [Test]
    [Description("Verify concurrent search operations")]
    public async Task Should_Handle_Concurrent_Search_Operations()
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(UseAzureAISearch.WithDefaultConfiguration());

        await javed.Using<UseAzureAISearch>().InitializeAsync();

        var searchAbility = javed.Using<UseAzureAISearch>();
        var queries = new[] { "climate", "weather", "temperature", "precipitation", "data" };

        // Act
        var searchTasks = queries.Select(query => searchAbility.TestAISearch(query)).ToArray();
        var results = await Task.WhenAll(searchTasks);

        // Assert
        results.Should().HaveCount(queries.Length, "All search operations should complete");
        results.Should().OnlyContain(r => r.Success, "All searches should be successful");
        
        var totalResults = results.Sum(r => r.TotalResults);
        totalResults.Should().BeGreaterThan(0, "Concurrent searches should return results");
        
        Logger.LogInformation("Concurrent searches completed - Total results across all queries: {TotalResults}", totalResults);
    }

    [Test]
    [Description("Verify search error handling")]
    public async Task Should_Handle_Search_Errors_Gracefully()
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(UseAzureAISearch.WithDefaultConfiguration());

        await javed.Using<UseAzureAISearch>().InitializeAsync();

        var searchAbility = javed.Using<UseAzureAISearch>();

        // Act & Assert - Test with malformed query
        var malformedQuery = "field:value AND (unclosed parenthesis";
        var searchResult = await searchAbility.TestAISearch(malformedQuery);

        // The search might succeed or fail depending on the search service's query parser
        // We just verify that it handles the response appropriately
        searchResult.Should().NotBeNull("Should return a result object even for malformed queries");
        
        if (!searchResult.Success)
        {
            searchResult.Error.Should().NotBeNullOrEmpty("Error message should be provided for failed searches");
            Logger.LogInformation("Malformed query handled gracefully with error: {Error}", searchResult.Error);
        }
        else
        {
            Logger.LogInformation("Search service handled malformed query and returned {ResultCount} results", 
                searchResult.TotalResults);
        }
    }

    [Test]
    [Description("Verify search result relevance and ranking")]
    public async Task Should_Return_Relevant_And_Ranked_Results()
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(UseAzureAISearch.WithDefaultConfiguration());

        await javed.Using<UseAzureAISearch>().InitializeAsync();

        var searchAbility = javed.Using<UseAzureAISearch>();

        // Act
        var searchResult = await searchAbility.TestAISearch("climate change impact");

        // Assert
        searchResult.Success.Should().BeTrue("Search should complete successfully");
        
        if (searchResult.TotalResults > 0)
        {
            searchResult.Results.Should().NotBeEmpty("Should have search results to validate");
            
            // Verify results are ordered by relevance (score should be descending)
            var scores = searchResult.Results.Take(5).Select(r => r.Score).Where(s => s.HasValue).Select(s => s!.Value).ToList();
            if (scores.Count > 1)
            {
                for (int i = 1; i < scores.Count; i++)
                {
                    scores[i].Should().BeLessOrEqualTo(scores[i - 1], 
                        "Results should be ordered by relevance score (descending)");
                }
            }
            
            Logger.LogInformation("Search returned {ResultCount} results with scores ranging from {MaxScore} to {MinScore}", 
                searchResult.TotalResults, scores.FirstOrDefault(), scores.LastOrDefault());
        }
    }
}