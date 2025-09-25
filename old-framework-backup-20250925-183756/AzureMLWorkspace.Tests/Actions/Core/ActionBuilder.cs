using Microsoft.Playwright;
using AzureMLWorkspace.Tests.Helpers;
using AzureMLWorkspace.Tests.Configuration;

namespace AzureMLWorkspace.Tests.Actions.Core;

/// <summary>
/// Fluent builder for creating and chaining actions
/// </summary>
public class ActionBuilder
{
    private readonly IPage _page;
    private readonly TestLogger _logger;
    private readonly TestConfiguration _config;
    private readonly List<IAction> _actions = new();

    public ActionBuilder(IPage page, TestLogger logger, TestConfiguration config)
    {
        _page = page;
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Add an action to the chain
    /// </summary>
    public ActionBuilder Add(IAction action)
    {
        _actions.Add(action);
        return this;
    }

    /// <summary>
    /// Add a custom action using a lambda
    /// </summary>
    public ActionBuilder Add(Func<Task> actionFunc, string? description = null)
    {
        _actions.Add(new LambdaAction(_page, _logger, _config, actionFunc, description));
        return this;
    }

    /// <summary>
    /// Add a conditional action
    /// </summary>
    public ActionBuilder AddIf(Func<Task<bool>> condition, IAction action)
    {
        var conditionalAction = new ConditionalAction(_page, _logger, _config, condition, action);
        _actions.Add(conditionalAction);
        return this;
    }

    /// <summary>
    /// Add a retry action
    /// </summary>
    public ActionBuilder AddWithRetry(IAction action, int maxRetries = 3, TimeSpan? delay = null)
    {
        var retryAction = new RetryAction(_page, _logger, _config, action, maxRetries, delay ?? TimeSpan.FromSeconds(1));
        _actions.Add(retryAction);
        return this;
    }

    /// <summary>
    /// Add a parallel action group
    /// </summary>
    public ActionBuilder AddParallel(params IAction[] actions)
    {
        var parallelAction = new ParallelAction(_page, _logger, _config, actions);
        _actions.Add(parallelAction);
        return this;
    }

    /// <summary>
    /// Execute all actions in the chain
    /// </summary>
    public async Task ExecuteAsync()
    {
        _logger.LogStep($"Executing action chain with {_actions.Count} actions");
        
        foreach (var action in _actions)
        {
            await action.ExecuteAsync();
        }
        
        _logger.LogStep("Action chain completed successfully");
    }

    /// <summary>
    /// Execute all actions and return results
    /// </summary>
    public async Task<List<ActionResult>> ExecuteWithResultsAsync()
    {
        var results = new List<ActionResult>();
        _logger.LogStep($"Executing action chain with {_actions.Count} actions");
        
        foreach (var action in _actions)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                await action.ExecuteAsync();
                var executionTime = DateTime.UtcNow - startTime;
                results.Add(ActionResult.Success(executionTime));
            }
            catch (Exception ex)
            {
                var executionTime = DateTime.UtcNow - startTime;
                results.Add(ActionResult.Failure(ex.Message, ex, executionTime));
                throw; // Re-throw to maintain original behavior
            }
        }
        
        _logger.LogStep("Action chain completed successfully");
        return results;
    }
}

/// <summary>
/// Action that executes a lambda function
/// </summary>
internal class LambdaAction : BaseAction
{
    private readonly Func<Task> _actionFunc;
    private readonly string _description;

    public LambdaAction(IPage page, TestLogger logger, TestConfiguration config, Func<Task> actionFunc, string? description = null)
        : base(page, logger, config)
    {
        _actionFunc = actionFunc;
        _description = description ?? "Lambda Action";
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep(_description);
        await _actionFunc();
    }
}

/// <summary>
/// Action that executes conditionally
/// </summary>
internal class ConditionalAction : BaseAction
{
    private readonly Func<Task<bool>> _condition;
    private readonly IAction _action;

    public ConditionalAction(IPage page, TestLogger logger, TestConfiguration config, Func<Task<bool>> condition, IAction action)
        : base(page, logger, config)
    {
        _condition = condition;
        _action = action;
    }

    protected override async Task ExecuteActionAsync()
    {
        if (await _condition())
        {
            await _action.ExecuteAsync();
        }
        else
        {
            Logger.LogStep("Condition not met, skipping action");
        }
    }
}

/// <summary>
/// Action that retries on failure
/// </summary>
internal class RetryAction : BaseAction
{
    private readonly IAction _action;
    private readonly int _maxRetries;
    private readonly TimeSpan _delay;

    public RetryAction(IPage page, TestLogger logger, TestConfiguration config, IAction action, int maxRetries, TimeSpan delay)
        : base(page, logger, config)
    {
        _action = action;
        _maxRetries = maxRetries;
        _delay = delay;
    }

    protected override async Task ExecuteActionAsync()
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt < _maxRetries)
        {
            try
            {
                attempt++;
                Logger.LogStep($"Retry attempt {attempt}/{_maxRetries}");
                await _action.ExecuteAsync();
                return; // Success, exit retry loop
            }
            catch (Exception ex)
            {
                lastException = ex;
                Logger.Warning($"Attempt {attempt} failed: {ex.Message}");
                
                if (attempt < _maxRetries)
                {
                    Logger.LogStep($"Waiting {_delay.TotalSeconds} seconds before retry");
                    await Task.Delay(_delay);
                }
            }
        }

        throw new Exception($"Action failed after {_maxRetries} attempts", lastException);
    }
}

/// <summary>
/// Action that executes multiple actions in parallel
/// </summary>
internal class ParallelAction : BaseAction
{
    private readonly IAction[] _actions;

    public ParallelAction(IPage page, TestLogger logger, TestConfiguration config, params IAction[] actions)
        : base(page, logger, config)
    {
        _actions = actions;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Executing {_actions.Length} actions in parallel");
        var tasks = _actions.Select(action => action.ExecuteAsync()).ToArray();
        await Task.WhenAll(tasks);
        Logger.LogStep("Parallel execution completed");
    }
}