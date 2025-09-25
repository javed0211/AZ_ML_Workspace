namespace AzureMLWorkspace.Tests.Framework.Screenplay;

/// <summary>
/// Represents a task that an actor can perform
/// </summary>
public interface ITask
{
    /// <summary>
    /// The name of the task
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the task
    /// </summary>
    /// <param name="actor">The actor performing the task</param>
    Task PerformAs(IActor actor);
}

/// <summary>
/// Represents a task that returns a value
/// </summary>
/// <typeparam name="T">The type of value returned</typeparam>
public interface ITask<T> : ITask
{
    /// <summary>
    /// Executes the task and returns a value
    /// </summary>
    /// <param name="actor">The actor performing the task</param>
    /// <returns>The result of the task</returns>
    new Task<T> PerformAs(IActor actor);
}