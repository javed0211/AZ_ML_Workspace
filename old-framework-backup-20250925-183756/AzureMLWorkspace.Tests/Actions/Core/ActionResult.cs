namespace AzureMLWorkspace.Tests.Actions.Core;

/// <summary>
/// Represents the result of an action execution
/// </summary>
/// <typeparam name="T">The type of data returned by the action</typeparam>
public class ActionResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
    public TimeSpan ExecutionTime { get; init; }

    /// <summary>
    /// Create a successful result
    /// </summary>
    public static ActionResult<T> Success(T data, TimeSpan executionTime) => new()
    {
        IsSuccess = true,
        Data = data,
        ExecutionTime = executionTime
    };

    /// <summary>
    /// Create a failed result
    /// </summary>
    public static ActionResult<T> Failure(string errorMessage, Exception? exception = null, TimeSpan executionTime = default) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Exception = exception,
        ExecutionTime = executionTime
    };
}

/// <summary>
/// Represents the result of an action execution without return data
/// </summary>
public class ActionResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
    public TimeSpan ExecutionTime { get; init; }

    /// <summary>
    /// Create a successful result
    /// </summary>
    public static ActionResult Success(TimeSpan executionTime) => new()
    {
        IsSuccess = true,
        ExecutionTime = executionTime
    };

    /// <summary>
    /// Create a failed result
    /// </summary>
    public static ActionResult Failure(string errorMessage, Exception? exception = null, TimeSpan executionTime = default) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Exception = exception,
        ExecutionTime = executionTime
    };
}