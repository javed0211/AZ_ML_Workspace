using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AzureMLWorkspace.Tests.Framework.Configuration;
using System.Text;

namespace AzureMLWorkspace.Tests.Framework.AI;

/// <summary>
/// Service for writing generated test files to disk
/// </summary>
public class TestFileGenerationService : ITestFileGenerationService
{
    private readonly AITestGenerationConfiguration _config;
    private readonly ILogger<TestFileGenerationService> _logger;

    public TestFileGenerationService(
        IOptions<AITestGenerationConfiguration> config,
        ILogger<TestFileGenerationService> logger)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<GeneratedFile>> WriteTestSuiteAsync(GeneratedTestSuite testSuite, string baseOutputPath)
    {
        if (testSuite == null)
            throw new ArgumentNullException(nameof(testSuite));

        if (string.IsNullOrWhiteSpace(baseOutputPath))
            throw new ArgumentException("Base output path cannot be empty", nameof(baseOutputPath));

        var generatedFiles = new List<GeneratedFile>();

        try
        {
            await EnsureDirectoryStructureAsync(baseOutputPath);

            // Write feature file
            if (!string.IsNullOrWhiteSpace(testSuite.FeatureContent))
            {
                var featureFile = await WriteFeatureFileAsync(testSuite, baseOutputPath);
                generatedFiles.Add(featureFile);
            }

            // Write step definitions
            foreach (var stepDef in testSuite.StepDefinitions)
            {
                var stepDefFile = await WriteStepDefinitionFileAsync(stepDef, baseOutputPath);
                generatedFiles.Add(stepDefFile);
            }

            // Write tasks
            foreach (var task in testSuite.Tasks)
            {
                var taskFile = await WriteTaskFileAsync(task, baseOutputPath);
                generatedFiles.Add(taskFile);
            }

            // Write questions
            foreach (var question in testSuite.Questions)
            {
                var questionFile = await WriteQuestionFileAsync(question, baseOutputPath);
                generatedFiles.Add(questionFile);
            }

            // Write TypeScript files
            foreach (var tsFile in testSuite.TypeScriptFiles)
            {
                var typeScriptFile = await WriteTypeScriptFileAsync(tsFile, baseOutputPath);
                generatedFiles.Add(typeScriptFile);
            }

            _logger.LogInformation("Successfully wrote {FileCount} files to {OutputPath}", 
                generatedFiles.Count, baseOutputPath);

            return generatedFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write test suite files to {OutputPath}", baseOutputPath);
            throw;
        }
    }

    public async Task EnsureDirectoryStructureAsync(string baseOutputPath)
    {
        var directories = new[]
        {
            Path.Combine(baseOutputPath, _config.OutputPaths.FeaturesDirectory),
            Path.Combine(baseOutputPath, _config.OutputPaths.StepDefinitionsDirectory),
            Path.Combine(baseOutputPath, _config.OutputPaths.TasksDirectory),
            Path.Combine(baseOutputPath, _config.OutputPaths.QuestionsDirectory),
            Path.Combine(baseOutputPath, _config.OutputPaths.TypeScriptDirectory)
        };

        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Created directory: {Directory}", directory);
            }
        }

        await Task.CompletedTask;
    }

    private async Task<GeneratedFile> WriteFeatureFileAsync(GeneratedTestSuite testSuite, string baseOutputPath)
    {
        var featuresDir = Path.Combine(baseOutputPath, _config.OutputPaths.FeaturesDirectory);
        var filePath = Path.Combine(featuresDir, testSuite.FeatureFileName);

        // Add generation metadata as comments
        var contentWithMetadata = AddGenerationMetadata(testSuite.FeatureContent, "feature");

        await File.WriteAllTextAsync(filePath, contentWithMetadata, Encoding.UTF8);

        var fileInfo = new FileInfo(filePath);
        return new GeneratedFile
        {
            FullPath = filePath,
            RelativePath = Path.GetRelativePath(baseOutputPath, filePath),
            FileType = "Feature",
            IsPlaceholder = false,
            SizeBytes = fileInfo.Length
        };
    }

    private async Task<GeneratedFile> WriteStepDefinitionFileAsync(GeneratedStepDefinition stepDef, string baseOutputPath)
    {
        var stepDefsDir = Path.Combine(baseOutputPath, _config.OutputPaths.StepDefinitionsDirectory);
        var filePath = Path.Combine(stepDefsDir, stepDef.FileName);

        // Add generation metadata as comments
        var contentWithMetadata = AddGenerationMetadata(stepDef.Content, "csharp");

        await File.WriteAllTextAsync(filePath, contentWithMetadata, Encoding.UTF8);

        var fileInfo = new FileInfo(filePath);
        return new GeneratedFile
        {
            FullPath = filePath,
            RelativePath = Path.GetRelativePath(baseOutputPath, filePath),
            FileType = "StepDefinition",
            IsPlaceholder = stepDef.HasPlaceholders,
            SizeBytes = fileInfo.Length
        };
    }

    private async Task<GeneratedFile> WriteTaskFileAsync(GeneratedTask task, string baseOutputPath)
    {
        var tasksDir = Path.Combine(baseOutputPath, _config.OutputPaths.TasksDirectory);
        var filePath = Path.Combine(tasksDir, task.FileName);

        // Add generation metadata as comments
        var contentWithMetadata = AddGenerationMetadata(task.Content, "csharp");

        await File.WriteAllTextAsync(filePath, contentWithMetadata, Encoding.UTF8);

        var fileInfo = new FileInfo(filePath);
        return new GeneratedFile
        {
            FullPath = filePath,
            RelativePath = Path.GetRelativePath(baseOutputPath, filePath),
            FileType = "Task",
            IsPlaceholder = task.IsPlaceholder,
            SizeBytes = fileInfo.Length
        };
    }

    private async Task<GeneratedFile> WriteQuestionFileAsync(GeneratedQuestion question, string baseOutputPath)
    {
        var questionsDir = Path.Combine(baseOutputPath, _config.OutputPaths.QuestionsDirectory);
        var filePath = Path.Combine(questionsDir, question.FileName);

        // Add generation metadata as comments
        var contentWithMetadata = AddGenerationMetadata(question.Content, "csharp");

        await File.WriteAllTextAsync(filePath, contentWithMetadata, Encoding.UTF8);

        var fileInfo = new FileInfo(filePath);
        return new GeneratedFile
        {
            FullPath = filePath,
            RelativePath = Path.GetRelativePath(baseOutputPath, filePath),
            FileType = "Question",
            IsPlaceholder = question.IsPlaceholder,
            SizeBytes = fileInfo.Length
        };
    }

    private async Task<GeneratedFile> WriteTypeScriptFileAsync(GeneratedTypeScriptFile tsFile, string baseOutputPath)
    {
        var tsDir = Path.Combine(baseOutputPath, _config.OutputPaths.TypeScriptDirectory);
        var filePath = Path.Combine(tsDir, tsFile.FileName);

        // Add generation metadata as comments
        var contentWithMetadata = AddGenerationMetadata(tsFile.Content, "typescript");

        await File.WriteAllTextAsync(filePath, contentWithMetadata, Encoding.UTF8);

        var fileInfo = new FileInfo(filePath);
        return new GeneratedFile
        {
            FullPath = filePath,
            RelativePath = Path.GetRelativePath(baseOutputPath, filePath),
            FileType = $"TypeScript-{tsFile.FileType}",
            IsPlaceholder = tsFile.HasPlaceholders,
            SizeBytes = fileInfo.Length
        };
    }

    private string AddGenerationMetadata(string content, string fileType)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        var metadata = fileType.ToLowerInvariant() switch
        {
            "feature" => $@"# This file was auto-generated by AI Test Generation
# Generated on: {timestamp}
# Framework: Azure ML Workspace Test Automation
# 
# IMPORTANT: Review and modify as needed before running tests
# 

{content}",
            "csharp" => $@"// This file was auto-generated by AI Test Generation
// Generated on: {timestamp}
// Framework: Azure ML Workspace Test Automation
// 
// IMPORTANT: Review and implement any NotImplementedException methods before running tests
// 

{content}",
            "typescript" => $@"// This file was auto-generated by AI Test Generation
// Generated on: {timestamp}
// Framework: Azure ML Workspace Test Automation
// 
// IMPORTANT: Review and implement any TODO comments before running tests
// 

{content}",
            _ => content
        };

        return metadata;
    }
}