namespace AzureMLWorkspace.Tests.Framework.AI;

/// <summary>
/// Service for writing generated test files to disk
/// </summary>
public interface ITestFileGenerationService
{
    /// <summary>
    /// Writes a complete test suite to the file system
    /// </summary>
    /// <param name="testSuite">The generated test suite</param>
    /// <param name="baseOutputPath">Base output directory</param>
    /// <returns>List of generated files with their paths</returns>
    Task<List<GeneratedFile>> WriteTestSuiteAsync(GeneratedTestSuite testSuite, string baseOutputPath);

    /// <summary>
    /// Ensures the required directory structure exists
    /// </summary>
    /// <param name="baseOutputPath">Base output directory</param>
    /// <returns>Task</returns>
    Task EnsureDirectoryStructureAsync(string baseOutputPath);
}

/// <summary>
/// Represents a generated file
/// </summary>
public class GeneratedFile
{
    /// <summary>
    /// Full path to the generated file
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// Relative path from the base output directory
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Type of file (Feature, StepDefinition, Task, Question)
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this file contains placeholder implementations
    /// </summary>
    public bool IsPlaceholder { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeBytes { get; set; }
}