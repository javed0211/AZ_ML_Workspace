using AzureMLWorkspace.Tests.Framework.Questions;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Tasks;

namespace AzureMLWorkspace.Tests.Framework.Extensions;

/// <summary>
/// Extension methods for the Screenplay pattern to provide fluent API
/// </summary>
public static class ScreenplayExtensions
{
    /// <summary>
    /// Allows chaining of tasks after an async actor operation
    /// </summary>
    public static async Task<IActor> And<T>(this Task<IActor> actorTask, T task) where T : ITask
    {
        var actor = await actorTask;
        await actor.AttemptsTo(task);
        return actor;
    }

    /// <summary>
    /// Allows chaining of tasks after a synchronous actor operation
    /// </summary>
    public static async Task<IActor> And<T>(this IActor actor, T task) where T : ITask
    {
        await actor.AttemptsTo(task);
        return actor;
    }

    /// <summary>
    /// Allows validation after an async actor operation for ResultCount
    /// </summary>
    public static async Task Should(this Task<IActor> actorTask, IQuestion<ResultCount> question)
    {
        var actor = await actorTask;
        await actor.Should(question);
    }

    /// <summary>
    /// Allows validation after an async actor operation for boolean results
    /// </summary>
    public static async Task Should(this Task<IActor> actorTask, IQuestion<bool> question)
    {
        var actor = await actorTask;
        await actor.Should(question);
    }

    /// <summary>
    /// Allows validation after an async actor operation for string results
    /// </summary>
    public static async Task Should(this Task<IActor> actorTask, IQuestion<string> question)
    {
        var actor = await actorTask;
        await actor.Should(question);
    }

    /// <summary>
    /// Allows validation after a synchronous actor operation for ResultCount
    /// </summary>
    public static async Task Should(this IActor actor, IQuestion<ResultCount> question)
    {
        var result = await actor.AsksFor(question);
        if (!result.IsValid)
        {
            throw new InvalidOperationException($"Validation failed: {result.ErrorMessage}");
        }
    }

    /// <summary>
    /// Allows validation after a synchronous actor operation for boolean results
    /// </summary>
    public static async Task Should(this IActor actor, IQuestion<bool> question)
    {
        var result = await actor.AsksFor(question);
        if (!result)
        {
            throw new InvalidOperationException($"Validation failed for question: {question.Question}");
        }
    }

    /// <summary>
    /// Allows validation after a synchronous actor operation for string results
    /// </summary>
    public static async Task Should(this IActor actor, IQuestion<string> question)
    {
        var result = await actor.AsksFor(question);
        if (string.IsNullOrEmpty(result))
        {
            throw new InvalidOperationException($"Validation failed for question: {question.Question}");
        }
    }
}