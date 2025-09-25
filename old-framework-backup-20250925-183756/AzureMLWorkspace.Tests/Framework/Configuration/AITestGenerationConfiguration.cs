using System.ComponentModel.DataAnnotations;

namespace AzureMLWorkspace.Tests.Framework.Configuration;

/// <summary>
/// Configuration for AI-driven test script generation
/// </summary>
public class AITestGenerationConfiguration
{
    /// <summary>
    /// Whether AI script generation is enabled
    /// </summary>
    public bool EnableAIScriptGeneration { get; set; } = false;

    /// <summary>
    /// AI provider to use (AzureOpenAI, OpenAI, LocalLLM)
    /// </summary>
    [Required]
    public string Provider { get; set; } = "AzureOpenAI";

    /// <summary>
    /// Azure OpenAI configuration
    /// </summary>
    public AzureOpenAIConfiguration AzureOpenAI { get; set; } = new();

    /// <summary>
    /// OpenAI configuration
    /// </summary>
    public OpenAIConfiguration OpenAI { get; set; } = new();

    /// <summary>
    /// Local LLM configuration
    /// </summary>
    public LocalLLMConfiguration LocalLLM { get; set; } = new();

    /// <summary>
    /// Generation settings
    /// </summary>
    public GenerationSettings GenerationSettings { get; set; } = new();

    /// <summary>
    /// Output paths for generated files
    /// </summary>
    public OutputPaths OutputPaths { get; set; } = new();
}

/// <summary>
/// Azure OpenAI configuration
/// </summary>
public class AzureOpenAIConfiguration
{
    /// <summary>
    /// Azure OpenAI endpoint
    /// </summary>
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// API key for authentication
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Deployment name
    /// </summary>
    [Required]
    public string DeploymentName { get; set; } = "gpt-4";

    /// <summary>
    /// API version
    /// </summary>
    public string ApiVersion { get; set; } = "2024-02-15-preview";
}

/// <summary>
/// OpenAI configuration
/// </summary>
public class OpenAIConfiguration
{
    /// <summary>
    /// API key for authentication
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model to use
    /// </summary>
    public string Model { get; set; } = "gpt-4";

    /// <summary>
    /// Base URL for the API
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}

/// <summary>
/// Local LLM configuration
/// </summary>
public class LocalLLMConfiguration
{
    /// <summary>
    /// Base URL for the local LLM
    /// </summary>
    [Required]
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Model to use
    /// </summary>
    public string Model { get; set; } = "llama2";

    /// <summary>
    /// API key (if required)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Generation settings
/// </summary>
public class GenerationSettings
{
    /// <summary>
    /// Maximum tokens to generate
    /// </summary>
    public int MaxTokens { get; set; } = 4000;

    /// <summary>
    /// Temperature for generation
    /// </summary>
    public double Temperature { get; set; } = 0.1;

    /// <summary>
    /// Top P for generation
    /// </summary>
    public double TopP { get; set; } = 0.9;

    /// <summary>
    /// Frequency penalty
    /// </summary>
    public double FrequencyPenalty { get; set; } = 0.0;

    /// <summary>
    /// Presence penalty
    /// </summary>
    public double PresencePenalty { get; set; } = 0.0;

    /// <summary>
    /// Timeout for generation requests
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
}

/// <summary>
/// Output paths for generated files
/// </summary>
public class OutputPaths
{
    /// <summary>
    /// Directory for generated feature files
    /// </summary>
    public string FeaturesDirectory { get; set; } = "Features/Generated";

    /// <summary>
    /// Directory for generated step definitions
    /// </summary>
    public string StepDefinitionsDirectory { get; set; } = "StepDefinitions/Generated";

    /// <summary>
    /// Directory for generated tasks
    /// </summary>
    public string TasksDirectory { get; set; } = "Framework/Tasks/Generated";

    /// <summary>
    /// Directory for generated questions
    /// </summary>
    public string QuestionsDirectory { get; set; } = "Framework/Questions/Generated";

    /// <summary>
    /// Directory for generated TypeScript files
    /// </summary>
    public string TypeScriptDirectory { get; set; } = "typescript-utils/src/generated";
}