namespace AzureMLWorkspace.Tests.Framework.AI;

/// <summary>
/// Service for AI-driven test script generation
/// </summary>
public interface IAITestGenerationService
{
    /// <summary>
    /// Generates a complete test suite from natural language description
    /// </summary>
    /// <param name="description">Natural language description of the test</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated test suite</returns>
    Task<GeneratedTestSuite> GenerateTestSuiteAsync(string description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the AI service configuration and connectivity
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the service is available and configured correctly</returns>
    Task<bool> ValidateServiceAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a generated test suite
/// </summary>
public class GeneratedTestSuite
{
    /// <summary>
    /// The generated feature file content
    /// </summary>
    public string FeatureContent { get; set; } = string.Empty;

    /// <summary>
    /// The feature file name
    /// </summary>
    public string FeatureFileName { get; set; } = string.Empty;

    /// <summary>
    /// Generated step definitions
    /// </summary>
    public List<GeneratedStepDefinition> StepDefinitions { get; set; } = new();

    /// <summary>
    /// Generated tasks
    /// </summary>
    public List<GeneratedTask> Tasks { get; set; } = new();

    /// <summary>
    /// Generated questions
    /// </summary>
    public List<GeneratedQuestion> Questions { get; set; } = new();

    /// <summary>
    /// Generated TypeScript utilities
    /// </summary>
    public List<GeneratedTypeScriptFile> TypeScriptFiles { get; set; } = new();

    /// <summary>
    /// Any warnings or notes about the generation
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Represents a generated step definition
/// </summary>
public class GeneratedStepDefinition
{
    /// <summary>
    /// The step definition class name
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// The step definition file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The step definition content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether this step definition contains placeholder methods
    /// </summary>
    public bool HasPlaceholders { get; set; }
}

/// <summary>
/// Represents a generated task
/// </summary>
public class GeneratedTask
{
    /// <summary>
    /// The task class name
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// The task file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The task content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether this task is a placeholder
    /// </summary>
    public bool IsPlaceholder { get; set; }
}

/// <summary>
/// Represents a generated question
/// </summary>
public class GeneratedQuestion
{
    /// <summary>
    /// The question class name
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// The question file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The question content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether this question is a placeholder
    /// </summary>
    public bool IsPlaceholder { get; set; }
}

/// <summary>
/// Represents a generated TypeScript file
/// </summary>
public class GeneratedTypeScriptFile
{
    /// <summary>
    /// The TypeScript file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The TypeScript content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The file type (task, utility, helper, etc.)
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this file contains placeholder implementations
    /// </summary>
    public bool HasPlaceholders { get; set; }
}