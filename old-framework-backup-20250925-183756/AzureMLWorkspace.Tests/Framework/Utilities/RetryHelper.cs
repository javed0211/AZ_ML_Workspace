using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System.Net.Sockets;

namespace AzureMLWorkspace.Tests.Framework.Utilities;

/// <summary>
/// Helper class for implementing retry logic in tests
/// </summary>
public static class RetryHelper
{
    /// <summary>
    /// Creates a retry policy for async operations
    /// </summary>
    public static IAsyncPolicy CreateRetryPolicy(int maxRetries = 3, TimeSpan? delay = null, ILogger? logger = null)
    {
        var baseDelay = delay ?? TimeSpan.FromSeconds(1);
        
        return Policy
            .Handle<Exception>(ex => IsRetryableException(ex))
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(
                    baseDelay.TotalMilliseconds * Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger?.LogWarning("Retry attempt {RetryCount} after {Delay}ms due to: {Exception}",
                        retryCount, timespan.TotalMilliseconds, outcome?.Message);
                });
    }

    /// <summary>
    /// Creates a retry policy for async operations with result
    /// </summary>
    public static IAsyncPolicy<T> CreateRetryPolicy<T>(int maxRetries = 3, TimeSpan? delay = null, ILogger? logger = null)
    {
        var baseDelay = delay ?? TimeSpan.FromSeconds(1);
        
        return Policy<T>
            .Handle<Exception>(ex => IsRetryableException(ex))
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(
                    baseDelay.TotalMilliseconds * Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger?.LogWarning("Retry attempt {RetryCount} after {Delay}ms due to: {Exception}",
                        retryCount, timespan.TotalMilliseconds, outcome?.Exception?.Message);
                });
    }

    /// <summary>
    /// Creates an HTTP retry policy
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> CreateHttpRetryPolicy(int maxRetries = 3, ILogger? logger = null)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger?.LogWarning("HTTP retry attempt {RetryCount} after {Delay}s due to: {Reason}",
                        retryCount, timespan.TotalSeconds, 
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });
    }

    /// <summary>
    /// Executes an action with retry logic
    /// </summary>
    public static async Task ExecuteWithRetryAsync(
        Func<Task> action, 
        int maxRetries = 3, 
        TimeSpan? delay = null, 
        ILogger? logger = null)
    {
        var policy = CreateRetryPolicy(maxRetries, delay, logger);
        await policy.ExecuteAsync(action);
    }

    /// <summary>
    /// Executes a function with retry logic
    /// </summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> func, 
        int maxRetries = 3, 
        TimeSpan? delay = null, 
        ILogger? logger = null)
    {
        var policy = CreateRetryPolicy<T>(maxRetries, delay, logger);
        return await policy.ExecuteAsync(func);
    }

    /// <summary>
    /// Determines if an exception is retryable
    /// </summary>
    private static bool IsRetryableException(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            HttpRequestException => true,
            TaskCanceledException => true,
            SocketException => true,
            InvalidOperationException ex when ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) => true,
            InvalidOperationException ex when ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
            _ => false
        };
    }

    /// <summary>
    /// Waits for a condition to be true with timeout and retry
    /// </summary>
    public static async Task<bool> WaitForConditionAsync(
        Func<Task<bool>> condition,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        ILogger? logger = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(500);
        var endTime = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < endTime)
        {
            try
            {
                if (await condition())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger?.LogDebug("Condition check failed: {Exception}", ex.Message);
            }

            await Task.Delay(interval);
        }

        logger?.LogWarning("Condition was not met within timeout of {Timeout}", timeout);
        return false;
    }

    /// <summary>
    /// Waits for a condition to be true with timeout and retry, returning the result
    /// </summary>
    public static async Task<T?> WaitForConditionAsync<T>(
        Func<Task<T?>> condition,
        Func<T?, bool> predicate,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        ILogger? logger = null) where T : class
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(500);
        var endTime = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < endTime)
        {
            try
            {
                var result = await condition();
                if (result != null && predicate(result))
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger?.LogDebug("Condition check failed: {Exception}", ex.Message);
            }

            await Task.Delay(interval);
        }

        logger?.LogWarning("Condition was not met within timeout of {Timeout}", timeout);
        return null;
    }
}