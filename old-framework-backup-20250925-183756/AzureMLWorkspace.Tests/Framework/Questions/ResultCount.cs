using AzureMLWorkspace.Tests.Framework.Screenplay;

namespace AzureMLWorkspace.Tests.Framework.Questions;

/// <summary>
/// Question about result counts with assertion capabilities
/// </summary>
public class ResultCount : IQuestion<ResultCount>, IAssertion<ResultCount>
{
    private readonly long _actualCount;
    private readonly Func<long, bool> _predicate;
    private readonly string _description;

    public string Question => $"Result count {_description}";
    public long Count => _actualCount;
    public bool IsValid => _predicate(_actualCount);
    public string ErrorMessage => IsValid ? string.Empty : $"Expected result count {_description}, but got {_actualCount}";

    private ResultCount(long actualCount, Func<long, bool> predicate, string description)
    {
        _actualCount = actualCount;
        _predicate = predicate;
        _description = description;
    }

    public async Task<ResultCount> AnsweredBy(IActor actor)
    {
        // The count is already known, so just return this instance
        await Task.CompletedTask;
        return this;
    }

    public void Assert(ResultCount actual)
    {
        if (!_predicate(_actualCount))
        {
            throw new AssertionException($"Expected result count {_description}, but got {_actualCount}");
        }
    }

    /// <summary>
    /// Creates a question that checks if the result count is greater than the specified value
    /// </summary>
    public static ResultCount GreaterThan(long expectedCount)
    {
        return new ResultCount(0, count => count > expectedCount, $"greater than {expectedCount}");
    }

    /// <summary>
    /// Creates a question that checks if the result count is less than the specified value
    /// </summary>
    public static ResultCount LessThan(long expectedCount)
    {
        return new ResultCount(0, count => count < expectedCount, $"less than {expectedCount}");
    }

    /// <summary>
    /// Creates a question that checks if the result count equals the specified value
    /// </summary>
    public static ResultCount EqualTo(long expectedCount)
    {
        return new ResultCount(0, count => count == expectedCount, $"equal to {expectedCount}");
    }

    /// <summary>
    /// Creates a question that checks if the result count is greater than or equal to the specified value
    /// </summary>
    public static ResultCount GreaterThanOrEqualTo(long expectedCount)
    {
        return new ResultCount(0, count => count >= expectedCount, $"greater than or equal to {expectedCount}");
    }

    /// <summary>
    /// Creates a question that checks if the result count is less than or equal to the specified value
    /// </summary>
    public static ResultCount LessThanOrEqualTo(long expectedCount)
    {
        return new ResultCount(0, count => count <= expectedCount, $"less than or equal to {expectedCount}");
    }

    /// <summary>
    /// Creates a question that checks if the result count is between two values (inclusive)
    /// </summary>
    public static ResultCount Between(long minCount, long maxCount)
    {
        return new ResultCount(0, count => count >= minCount && count <= maxCount, $"between {minCount} and {maxCount}");
    }

    /// <summary>
    /// Creates a question with an actual count value for assertion
    /// </summary>
    public static ResultCount WithActualCount(long actualCount)
    {
        return new ResultCount(actualCount, _ => true, $"is {actualCount}");
    }

    /// <summary>
    /// Sets the actual count for this result count question
    /// </summary>
    public ResultCount WithCount(long actualCount)
    {
        return new ResultCount(actualCount, _predicate, _description);
    }
}