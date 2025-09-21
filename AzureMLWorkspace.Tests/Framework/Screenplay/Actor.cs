using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AzureMLWorkspace.Tests.Framework.Screenplay;

/// <summary>
/// Implementation of the Actor in the Screenplay pattern
/// </summary>
public class Actor : IActor
{
    private readonly ConcurrentDictionary<Type, IAbility> _abilities = new();
    private readonly ConcurrentDictionary<string, object> _memory = new();
    private readonly ILogger<Actor> _logger;

    public string Name { get; }

    public Actor(string name, ILogger<Actor> logger)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("Actor '{ActorName}' created", Name);
    }

    public IActor Can<T>(T ability) where T : IAbility
    {
        if (ability == null) throw new ArgumentNullException(nameof(ability));
        
        _abilities.TryAdd(typeof(T), ability);
        _logger.LogDebug("Actor '{ActorName}' gained ability '{AbilityName}'", Name, ability.Name);
        return this;
    }

    public T Using<T>() where T : IAbility
    {
        if (!_abilities.TryGetValue(typeof(T), out var ability))
        {
            throw new InvalidOperationException($"Actor '{Name}' does not have the ability '{typeof(T).Name}'");
        }
        return (T)ability;
    }

    public bool HasAbility<T>() where T : IAbility
    {
        return _abilities.ContainsKey(typeof(T));
    }

    public async Task<IActor> AttemptsTo(ITask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        
        _logger.LogInformation("Actor '{ActorName}' attempts to {TaskName}", Name, task.Name);
        
        try
        {
            await task.PerformAs(this);
            _logger.LogDebug("Actor '{ActorName}' successfully completed {TaskName}", Name, task.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Actor '{ActorName}' failed to complete {TaskName}", Name, task.Name);
            throw;
        }
        
        return this;
    }

    public async Task<IActor> AttemptsTo(params ITask[] tasks)
    {
        foreach (var task in tasks)
        {
            await AttemptsTo(task);
        }
        return this;
    }

    public async Task<IActor> And(ITask task)
    {
        return await AttemptsTo(task);
    }

    public async Task<T> AsksFor<T>(IQuestion<T> question)
    {
        if (question == null) throw new ArgumentNullException(nameof(question));
        
        _logger.LogDebug("Actor '{ActorName}' asks: {Question}", Name, question.Question);
        
        try
        {
            var answer = await question.AnsweredBy(this);
            _logger.LogDebug("Actor '{ActorName}' received answer: {Answer}", Name, answer);
            return answer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Actor '{ActorName}' failed to get answer for: {Question}", Name, question.Question);
            throw;
        }
    }

    public async Task<IActor> Should(IQuestion<bool> question)
    {
        var result = await AsksFor(question);
        if (!result)
        {
            throw new AssertionException($"Actor '{Name}' expected '{question.Question}' to be true, but it was false");
        }
        return this;
    }

    public async Task<IActor> ShouldSee<T>(IQuestion<T> question, Action<T> assertion)
    {
        var result = await AsksFor(question);
        
        try
        {
            assertion(result);
            _logger.LogDebug("Actor '{ActorName}' assertion passed for: {Question}", Name, question.Question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Actor '{ActorName}' assertion failed for: {Question}", Name, question.Question);
            throw;
        }
        
        return this;
    }

    public async Task<IActor> ShouldSee<T>(IQuestion<T> question) where T : IAssertion<T>
    {
        var result = await AsksFor(question);
        result.Assert(result);
        return this;
    }

    public void Remember<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        _memory.AddOrUpdate(key, value!, (k, v) => value!);
        _logger.LogDebug("Actor '{ActorName}' remembered '{Key}' = '{Value}'", Name, key, value);
    }

    public T Recall<T>(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        if (!_memory.TryGetValue(key, out var value))
        {
            throw new KeyNotFoundException($"Actor '{Name}' does not remember '{key}'");
        }
        
        if (value is not T typedValue)
        {
            throw new InvalidCastException($"Actor '{Name}' remembered '{key}' as {value?.GetType().Name}, but expected {typeof(T).Name}");
        }
        
        _logger.LogDebug("Actor '{ActorName}' recalled '{Key}' = '{Value}'", Name, key, typedValue);
        return typedValue;
    }

    public bool Remembers(string key)
    {
        return !string.IsNullOrEmpty(key) && _memory.ContainsKey(key);
    }

    /// <summary>
    /// Creates a new actor with the given name
    /// </summary>
    /// <param name="name">The name of the actor</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>A new actor instance</returns>
    public static Actor Named(string name, ILogger<Actor> logger)
    {
        return new Actor(name, logger);
    }

    /// <summary>
    /// Cleans up all abilities when the actor is disposed
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing actor '{ActorName}'", Name);
        
        foreach (var ability in _abilities.Values)
        {
            try
            {
                await ability.CleanupAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup ability '{AbilityName}' for actor '{ActorName}'", 
                    ability.Name, Name);
            }
        }
        
        _abilities.Clear();
        _memory.Clear();
    }
}