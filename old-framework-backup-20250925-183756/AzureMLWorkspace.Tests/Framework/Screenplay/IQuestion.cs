namespace AzureMLWorkspace.Tests.Framework.Screenplay;

/// <summary>
/// Represents a question that an actor can ask
/// </summary>
/// <typeparam name="T">The type of answer expected</typeparam>
public interface IQuestion<T>
{
    /// <summary>
    /// The question being asked
    /// </summary>
    string Question { get; }

    /// <summary>
    /// Gets the answer to the question
    /// </summary>
    /// <param name="actor">The actor asking the question</param>
    /// <returns>The answer</returns>
    Task<T> AnsweredBy(IActor actor);
}

/// <summary>
/// Represents an assertion that can be made about a value
/// </summary>
/// <typeparam name="T">The type of value being asserted</typeparam>
public interface IAssertion<T>
{
    /// <summary>
    /// Performs the assertion
    /// </summary>
    /// <param name="actual">The actual value</param>
    void Assert(T actual);
}