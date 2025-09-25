using Microsoft.Playwright;

namespace AzureMLWorkspace.Tests.Actions.Core;

/// <summary>
/// Base interface for all actions in the framework
/// </summary>
public interface IAction
{
    /// <summary>
    /// Execute the action
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task ExecuteAsync();
}

/// <summary>
/// Base interface for actions that return a result
/// </summary>
/// <typeparam name="T">The type of result returned</typeparam>
public interface IAction<T>
{
    /// <summary>
    /// Execute the action and return a result
    /// </summary>
    /// <returns>Task containing the result</returns>
    Task<T> ExecuteAsync();
}

/// <summary>
/// Interface for actions that can be chained together
/// </summary>
public interface IChainableAction : IAction
{
    /// <summary>
    /// Chain another action to execute after this one
    /// </summary>
    /// <param name="nextAction">The next action to execute</param>
    /// <returns>A composite action that executes both actions in sequence</returns>
    IChainableAction Then(IAction nextAction);
}

/// <summary>
/// Interface for conditional actions
/// </summary>
public interface IConditionalAction : IAction
{
    /// <summary>
    /// Execute the action only if the condition is met
    /// </summary>
    /// <param name="condition">The condition to check</param>
    /// <returns>The action for method chaining</returns>
    IConditionalAction When(Func<Task<bool>> condition);
    
    /// <summary>
    /// Execute an alternative action if the condition is not met
    /// </summary>
    /// <param name="alternativeAction">The alternative action to execute</param>
    /// <returns>The action for method chaining</returns>
    IConditionalAction Otherwise(IAction alternativeAction);
}