using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Questions;

/// <summary>
/// Static class containing validation questions
/// </summary>
public static class Validate
{
    /// <summary>
    /// Creates a question to validate AI Search results
    /// </summary>
    public static AISearchResultsQuestion AISearchResults(string query)
    {
        return new AISearchResultsQuestion(query, 
            AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<AISearchResultsQuestion>>());
    }

    /// <summary>
    /// Creates a question to validate workspace access
    /// </summary>
    public static WorkspaceAccessQuestion WorkspaceAccess(string workspaceName)
    {
        return new WorkspaceAccessQuestion(workspaceName,
            AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<WorkspaceAccessQuestion>>());
    }

    /// <summary>
    /// Creates a question to validate compute status
    /// </summary>
    public static ComputeStatusQuestion ComputeStatus(string computeName, string expectedStatus)
    {
        return new ComputeStatusQuestion(computeName, expectedStatus,
            AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<ComputeStatusQuestion>>());
    }
}

/// <summary>
/// Question to validate AI Search results
/// </summary>
public class AISearchResultsQuestion : IQuestion<ResultCount>
{
    private readonly string _query;
    private readonly ILogger<AISearchResultsQuestion> _logger;

    public string Question => $"AI Search results for query '{_query}'";

    public AISearchResultsQuestion(string query, ILogger<AISearchResultsQuestion> logger)
    {
        _query = query ?? throw new ArgumentNullException(nameof(query));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ResultCount> AnsweredBy(IActor actor)
    {
        _logger.LogInformation("Validating AI Search results for query: {Query}", _query);

        if (!actor.HasAbility<UseAzureAISearch>())
        {
            throw new InvalidOperationException($"Actor '{actor.Name}' must have UseAzureAISearch ability to validate search results");
        }

        var search = actor.Using<UseAzureAISearch>();
        var count = await search.GetSearchResultCount(_query);

        _logger.LogInformation("AI Search returned {Count} results for query: {Query}", count, _query);

        return ResultCount.WithActualCount(count ?? 0);
    }
}

/// <summary>
/// Question to validate workspace access
/// </summary>
public class WorkspaceAccessQuestion : IQuestion<bool>
{
    private readonly string _workspaceName;
    private readonly ILogger<WorkspaceAccessQuestion> _logger;

    public string Question => $"Can access workspace '{_workspaceName}'";

    public WorkspaceAccessQuestion(string workspaceName, ILogger<WorkspaceAccessQuestion> logger)
    {
        _workspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> AnsweredBy(IActor actor)
    {
        _logger.LogInformation("Validating access to workspace: {WorkspaceName}", _workspaceName);

        try
        {
            if (actor.HasAbility<UseAzureML>())
            {
                var azureML = actor.Using<UseAzureML>();
                var workspace = azureML.Workspace;
                var canAccess = workspace.Data.Name == _workspaceName;
                
                _logger.LogInformation("Workspace access validation result: {CanAccess}", canAccess);
                return canAccess;
            }

            if (actor.HasAbility<BrowseTheWeb>())
            {
                var browser = actor.Using<BrowseTheWeb>();
                var currentUrl = browser.Page.Url;
                var canAccess = currentUrl.Contains(_workspaceName, StringComparison.OrdinalIgnoreCase);
                
                _logger.LogInformation("Browser-based workspace access validation result: {CanAccess}", canAccess);
                return canAccess;
            }

            throw new InvalidOperationException($"Actor '{actor.Name}' must have either UseAzureML or BrowseTheWeb ability to validate workspace access");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate workspace access for: {WorkspaceName}", _workspaceName);
            return false;
        }
    }
}

/// <summary>
/// Question to validate compute status
/// </summary>
public class ComputeStatusQuestion : IQuestion<bool>
{
    private readonly string _computeName;
    private readonly string _expectedStatus;
    private readonly ILogger<ComputeStatusQuestion> _logger;

    public string Question => $"Compute '{_computeName}' has status '{_expectedStatus}'";

    public ComputeStatusQuestion(string computeName, string expectedStatus, ILogger<ComputeStatusQuestion> logger)
    {
        _computeName = computeName ?? throw new ArgumentNullException(nameof(computeName));
        _expectedStatus = expectedStatus ?? throw new ArgumentNullException(nameof(expectedStatus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> AnsweredBy(IActor actor)
    {
        _logger.LogInformation("Validating compute status for: {ComputeName}, expected: {ExpectedStatus}", 
            _computeName, _expectedStatus);

        if (!actor.HasAbility<UseAzureML>())
        {
            throw new InvalidOperationException($"Actor '{actor.Name}' must have UseAzureML ability to validate compute status");
        }

        try
        {
            var azureML = actor.Using<UseAzureML>();
            var actualStatus = await azureML.GetComputeStatus(_computeName);
            var statusMatches = string.Equals(actualStatus, _expectedStatus, StringComparison.OrdinalIgnoreCase);
            
            _logger.LogInformation("Compute status validation result: {StatusMatches} (actual: {ActualStatus}, expected: {ExpectedStatus})", 
                statusMatches, actualStatus, _expectedStatus);
            
            return statusMatches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate compute status for: {ComputeName}", _computeName);
            return false;
        }
    }
}