using Microsoft.Playwright;

namespace AzureMLWorkspace.Tests.Framework.Screenplay;

/// <summary>
/// Represents an actor in the Screenplay pattern who can perform tasks and ask questions
/// </summary>
public interface IActor
{
    /// <summary>
    /// The name of the actor
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gives the actor an ability to perform certain actions
    /// </summary>
    /// <typeparam name="T">The type of ability</typeparam>
    /// <param name="ability">The ability instance</param>
    /// <returns>The actor with the new ability</returns>
    IActor Can<T>(T ability) where T : IAbility;

    /// <summary>
    /// Gets an ability that the actor has
    /// </summary>
    /// <typeparam name="T">The type of ability</typeparam>
    /// <returns>The ability instance</returns>
    T Using<T>() where T : IAbility;

    /// <summary>
    /// Checks if the actor has a specific ability
    /// </summary>
    /// <typeparam name="T">The type of ability</typeparam>
    /// <returns>True if the actor has the ability</returns>
    bool HasAbility<T>() where T : IAbility;

    /// <summary>
    /// Makes the actor attempt to perform a task
    /// </summary>
    /// <param name="task">The task to perform</param>
    /// <returns>The actor for method chaining</returns>
    Task<IActor> AttemptsTo(ITask task);

    /// <summary>
    /// Makes the actor attempt to perform multiple tasks
    /// </summary>
    /// <param name="tasks">The tasks to perform</param>
    /// <returns>The actor for method chaining</returns>
    Task<IActor> AttemptsTo(params ITask[] tasks);

    /// <summary>
    /// Allows method chaining with "And"
    /// </summary>
    /// <param name="task">The next task to perform</param>
    /// <returns>The actor for method chaining</returns>
    Task<IActor> And(ITask task);

    /// <summary>
    /// Makes the actor ask a question
    /// </summary>
    /// <typeparam name="T">The type of answer expected</typeparam>
    /// <param name="question">The question to ask</param>
    /// <returns>The answer to the question</returns>
    Task<T> AsksFor<T>(IQuestion<T> question);

    /// <summary>
    /// Makes the actor check that a condition is met
    /// </summary>
    /// <param name="question">The question that should return true</param>
    /// <returns>The actor for method chaining</returns>
    Task<IActor> Should(IQuestion<bool> question);

    /// <summary>
    /// Makes the actor check that a condition is met with a custom assertion
    /// </summary>
    /// <typeparam name="T">The type of value to check</typeparam>
    /// <param name="question">The question to ask</param>
    /// <param name="assertion">The assertion to perform</param>
    /// <returns>The actor for method chaining</returns>
    Task<IActor> ShouldSee<T>(IQuestion<T> question, Action<T> assertion);

    /// <summary>
    /// Makes the actor check that a condition is met
    /// </summary>
    /// <typeparam name="T">The type of value to check</typeparam>
    /// <param name="question">The question to ask</param>
    /// <returns>The actor for method chaining</returns>
    Task<IActor> ShouldSee<T>(IQuestion<T> question) where T : IAssertion<T>;

    /// <summary>
    /// Remembers a value for later use
    /// </summary>
    /// <typeparam name="T">The type of value to remember</typeparam>
    /// <param name="key">The key to store the value under</param>
    /// <param name="value">The value to remember</param>
    void Remember<T>(string key, T value);

    /// <summary>
    /// Recalls a previously remembered value
    /// </summary>
    /// <typeparam name="T">The type of value to recall</typeparam>
    /// <param name="key">The key the value was stored under</param>
    /// <returns>The remembered value</returns>
    T Recall<T>(string key);

    /// <summary>
    /// Checks if a value is remembered
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the value is remembered</returns>
    bool Remembers(string key);
}