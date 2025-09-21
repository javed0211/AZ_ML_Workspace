namespace AzureMLWorkspace.Tests.Framework.Screenplay;

/// <summary>
/// Represents an ability that an actor can have
/// </summary>
public interface IAbility
{
    /// <summary>
    /// The name of the ability
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Initializes the ability
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Cleans up the ability
    /// </summary>
    Task CleanupAsync();
}