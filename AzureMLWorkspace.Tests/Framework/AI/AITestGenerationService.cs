using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AzureMLWorkspace.Tests.Framework.Configuration;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AzureMLWorkspace.Tests.Framework.AI;

/// <summary>
/// AI-driven test script generation service
/// </summary>
public class AITestGenerationService : IAITestGenerationService
{
    private readonly AITestGenerationConfiguration _config;
    private readonly ILogger<AITestGenerationService> _logger;
    private readonly HttpClient _httpClient;

    public AITestGenerationService(
        IOptions<AITestGenerationConfiguration> config,
        ILogger<AITestGenerationService> logger,
        HttpClient httpClient)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<GeneratedTestSuite> GenerateTestSuiteAsync(string description, CancellationToken cancellationToken = default)
    {
        if (!_config.EnableAIScriptGeneration)
        {
            throw new InvalidOperationException("AI script generation is disabled in configuration");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Test description cannot be empty", nameof(description));
        }

        _logger.LogInformation("Generating test suite for description: {Description}", description);

        try
        {
            var prompt = BuildGenerationPrompt(description);
            var response = await CallAIServiceAsync(prompt, cancellationToken);
            var testSuite = ParseAIResponse(response, description);

            _logger.LogInformation("Successfully generated test suite with {FeatureCount} feature(s), {StepDefCount} step definition(s), {TaskCount} task(s), {QuestionCount} question(s)",
                1, testSuite.StepDefinitions.Count, testSuite.Tasks.Count, testSuite.Questions.Count);

            return testSuite;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate test suite for description: {Description}", description);
            throw;
        }
    }

    public async Task<bool> ValidateServiceAsync(CancellationToken cancellationToken = default)
    {
        if (!_config.EnableAIScriptGeneration)
        {
            _logger.LogWarning("AI script generation is disabled in configuration");
            return false;
        }

        try
        {
            var testPrompt = "Generate a simple test validation response.";
            var response = await CallAIServiceAsync(testPrompt, cancellationToken);
            
            var isValid = !string.IsNullOrWhiteSpace(response);
            _logger.LogInformation("AI service validation result: {IsValid}", isValid);
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI service validation failed");
            return false;
        }
    }

    private string BuildGenerationPrompt(string description)
    {
        return $@"
You are an expert test automation engineer specializing in C# Playwright automation with Reqnroll BDD and the Screenplay pattern.

Generate a complete test suite for the following requirement:
""{description}""

The framework uses:
- C# with .NET 8
- Playwright for UI automation (cross-platform: Windows, macOS, Linux)
- Reqnroll for BDD (Gherkin syntax)
- Screenplay pattern with Actor, Task, Question, and Ability concepts
- Azure SDK for Azure services integration
- TypeScript integration for advanced Playwright features
- Python.NET integration for ML model validation

IMPORTANT REQUIREMENTS:
1. Generate ONLY the code content, no explanations or markdown formatting
2. Use modern task/ability-based design (no Page Object Model)
3. Follow the existing framework patterns shown in the examples
4. Generate placeholder methods with NotImplementedException for missing implementations
5. Ensure all generated code compiles and doesn't break the build
6. Use proper C# naming conventions and async/await patterns
7. Include proper error handling and logging
8. Support Azure portal interactions, PIM role activation, and Azure ML workspace operations
9. Include TypeScript task generation for complex browser interactions when needed
10. Support Electron app testing for VS Code Desktop scenarios

EXISTING FRAMEWORK PATTERNS:

Step Definition Example:
```csharp
[Given(@""I am a data scientist named ""(.*)"""")]
public void GivenIAmADataScientistNamed(string name)
{{
    _actor = Actor.Named(name, _serviceProvider.GetRequiredService<ILogger<Actor>>());
}}

[When(@""I attempt to login to Azure portal"")]
public async Task WhenIAttemptToLoginToAzurePortal()
{{
    await _actor.AttemptsTo(LoginToAzurePortal.WithCredentials());
}}
```

Task Example:
```csharp
public class LoginToAzurePortal : ITask
{{
    public static LoginToAzurePortal WithCredentials() => new();
    
    public async Task<IActor> PerformAs(IActor actor)
    {{
        var browser = actor.Using<BrowseTheWeb>();
        await browser.Page.GotoAsync(""https://portal.azure.com"");
        // Implementation or NotImplementedException
        return actor;
    }}
}}
```

Question Example:
```csharp
public class IsUserLoggedIn : IQuestion<bool>
{{
    public static IsUserLoggedIn ToAzurePortal() => new();
    
    public async Task<bool> AnsweredBy(IActor actor)
    {{
        var browser = actor.Using<BrowseTheWeb>();
        // Implementation or throw new NotImplementedException()
        return false;
    }}
}}
```

Generate the response in this exact JSON format:
{{
    ""feature"": {{
        ""fileName"": ""GeneratedFeatureName.feature"",
        ""content"": ""Feature file content here""
    }},
    ""stepDefinitions"": [
        {{
            ""className"": ""GeneratedStepsClassName"",
            ""fileName"": ""GeneratedStepsClassName.cs"",
            ""content"": ""Complete C# step definition class content"",
            ""hasPlaceholders"": true
        }}
    ],
    ""tasks"": [
        {{
            ""className"": ""TaskClassName"",
            ""fileName"": ""TaskClassName.cs"",
            ""content"": ""Complete C# task class content"",
            ""isPlaceholder"": false
        }}
    ],
    ""questions"": [
        {{
            ""className"": ""QuestionClassName"",
            ""fileName"": ""QuestionClassName.cs"",
            ""content"": ""Complete C# question class content"",
            ""isPlaceholder"": false
        }}
    ],
    ""typeScriptFiles"": [
        {{
            ""fileName"": ""utilityName.ts"",
            ""content"": ""Complete TypeScript utility content"",
            ""fileType"": ""utility"",
            ""hasPlaceholders"": false
        }}
    ],
    ""warnings"": [""Any warnings about generated placeholders or missing implementations""]
}}";
    }

    private async Task<string> CallAIServiceAsync(string prompt, CancellationToken cancellationToken)
    {
        return _config.Provider.ToLowerInvariant() switch
        {
            "azureopenai" => await CallAzureOpenAIAsync(prompt, cancellationToken),
            "openai" => await CallOpenAIAsync(prompt, cancellationToken),
            "localllm" => await CallLocalLLMAsync(prompt, cancellationToken),
            _ => throw new NotSupportedException($"AI provider '{_config.Provider}' is not supported")
        };
    }

    private async Task<string> CallAzureOpenAIAsync(string prompt, CancellationToken cancellationToken)
    {
        var client = new OpenAIClient(
            new Uri(_config.AzureOpenAI.Endpoint),
            new AzureKeyCredential(_config.AzureOpenAI.ApiKey));

        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = _config.AzureOpenAI.DeploymentName,
            Messages = { new ChatRequestUserMessage(prompt) },
            MaxTokens = _config.GenerationSettings.MaxTokens,
            Temperature = (float)_config.GenerationSettings.Temperature,
            NucleusSamplingFactor = (float)_config.GenerationSettings.TopP,
            FrequencyPenalty = (float)_config.GenerationSettings.FrequencyPenalty,
            PresencePenalty = (float)_config.GenerationSettings.PresencePenalty
        };

        var response = await client.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
        return response.Value.Choices[0].Message.Content;
    }

    private async Task<string> CallOpenAIAsync(string prompt, CancellationToken cancellationToken)
    {
        // Use HTTP client for OpenAI API calls to avoid version conflicts
        var requestBody = new
        {
            model = _config.OpenAI.Model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = _config.GenerationSettings.MaxTokens,
            temperature = _config.GenerationSettings.Temperature,
            top_p = _config.GenerationSettings.TopP,
            frequency_penalty = _config.GenerationSettings.FrequencyPenalty,
            presence_penalty = _config.GenerationSettings.PresencePenalty
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.OpenAI.ApiKey}");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
        
        return responseObj.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }

    private async Task<string> CallLocalLLMAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _config.LocalLLM.Model,
            prompt = prompt,
            max_tokens = _config.GenerationSettings.MaxTokens,
            temperature = _config.GenerationSettings.Temperature,
            top_p = _config.GenerationSettings.TopP,
            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        if (!string.IsNullOrEmpty(_config.LocalLLM.ApiKey))
        {
            content.Headers.Add("Authorization", $"Bearer {_config.LocalLLM.ApiKey}");
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.GenerationSettings.TimeoutSeconds));
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

        var response = await _httpClient.PostAsync($"{_config.LocalLLM.BaseUrl}/v1/completions", content, combinedCts.Token);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(combinedCts.Token);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        return responseObj.GetProperty("choices")[0].GetProperty("text").GetString() ?? string.Empty;
    }

    private GeneratedTestSuite ParseAIResponse(string response, string originalDescription)
    {
        try
        {
            // Clean the response - remove any markdown formatting or extra text
            var cleanedResponse = CleanAIResponse(response);
            
            var jsonDoc = JsonSerializer.Deserialize<JsonElement>(cleanedResponse);

            var testSuite = new GeneratedTestSuite();

            // Parse feature
            if (jsonDoc.TryGetProperty("feature", out var featureElement))
            {
                testSuite.FeatureFileName = featureElement.GetProperty("fileName").GetString() ?? "Generated.feature";
                testSuite.FeatureContent = featureElement.GetProperty("content").GetString() ?? string.Empty;
            }

            // Parse step definitions
            if (jsonDoc.TryGetProperty("stepDefinitions", out var stepDefsElement))
            {
                foreach (var stepDef in stepDefsElement.EnumerateArray())
                {
                    testSuite.StepDefinitions.Add(new GeneratedStepDefinition
                    {
                        ClassName = stepDef.GetProperty("className").GetString() ?? string.Empty,
                        FileName = stepDef.GetProperty("fileName").GetString() ?? string.Empty,
                        Content = stepDef.GetProperty("content").GetString() ?? string.Empty,
                        HasPlaceholders = stepDef.TryGetProperty("hasPlaceholders", out var hasPlaceholders) && hasPlaceholders.GetBoolean()
                    });
                }
            }

            // Parse tasks
            if (jsonDoc.TryGetProperty("tasks", out var tasksElement))
            {
                foreach (var task in tasksElement.EnumerateArray())
                {
                    testSuite.Tasks.Add(new GeneratedTask
                    {
                        ClassName = task.GetProperty("className").GetString() ?? string.Empty,
                        FileName = task.GetProperty("fileName").GetString() ?? string.Empty,
                        Content = task.GetProperty("content").GetString() ?? string.Empty,
                        IsPlaceholder = task.TryGetProperty("isPlaceholder", out var isPlaceholder) && isPlaceholder.GetBoolean()
                    });
                }
            }

            // Parse questions
            if (jsonDoc.TryGetProperty("questions", out var questionsElement))
            {
                foreach (var question in questionsElement.EnumerateArray())
                {
                    testSuite.Questions.Add(new GeneratedQuestion
                    {
                        ClassName = question.GetProperty("className").GetString() ?? string.Empty,
                        FileName = question.GetProperty("fileName").GetString() ?? string.Empty,
                        Content = question.GetProperty("content").GetString() ?? string.Empty,
                        IsPlaceholder = question.TryGetProperty("isPlaceholder", out var isPlaceholder) && isPlaceholder.GetBoolean()
                    });
                }
            }

            // Parse warnings
            if (jsonDoc.TryGetProperty("warnings", out var warningsElement))
            {
                foreach (var warning in warningsElement.EnumerateArray())
                {
                    testSuite.Warnings.Add(warning.GetString() ?? string.Empty);
                }
            }

            return testSuite;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI response as JSON. Response: {Response}", response);
            
            // Fallback: create a basic test suite with placeholder content
            return CreateFallbackTestSuite(originalDescription, response);
        }
    }

    private string CleanAIResponse(string response)
    {
        // Remove markdown code blocks
        response = Regex.Replace(response, @"```json\s*", "", RegexOptions.IgnoreCase);
        response = Regex.Replace(response, @"```\s*$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        
        // Find the JSON content between curly braces
        var match = Regex.Match(response, @"\{.*\}", RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Value;
        }

        return response.Trim();
    }

    private GeneratedTestSuite CreateFallbackTestSuite(string description, string aiResponse)
    {
        var sanitizedName = Regex.Replace(description, @"[^\w\s]", "").Replace(" ", "");
        var featureName = $"Generated{sanitizedName}";

        return new GeneratedTestSuite
        {
            FeatureFileName = $"{featureName}.feature",
            FeatureContent = $@"Feature: {featureName}
    As a user
    I want to {description.ToLowerInvariant()}
    So that I can achieve my goals

Scenario: {featureName} Test
    Given I am a user
    When I perform the required action
    Then I should see the expected result",
            StepDefinitions = new List<GeneratedStepDefinition>
            {
                new GeneratedStepDefinition
                {
                    ClassName = $"{featureName}Steps",
                    FileName = $"{featureName}Steps.cs",
                    Content = $@"using Reqnroll;
using AzureMLWorkspace.Tests.Framework.Screenplay;

namespace AzureMLWorkspace.Tests.StepDefinitions.Generated;

[Binding]
public class {featureName}Steps
{{
    private IActor? _actor;

    [Given(@""I am a user"")]
    public void GivenIAmAUser()
    {{
        throw new NotImplementedException(""This step definition was auto-generated and needs implementation"");
    }}

    [When(@""I perform the required action"")]
    public async Task WhenIPerformTheRequiredAction()
    {{
        throw new NotImplementedException(""This step definition was auto-generated and needs implementation"");
    }}

    [Then(@""I should see the expected result"")]
    public async Task ThenIShouldSeeTheExpectedResult()
    {{
        throw new NotImplementedException(""This step definition was auto-generated and needs implementation"");
    }}
}}",
                    HasPlaceholders = true
                }
            },
            Warnings = new List<string>
            {
                "AI response could not be parsed properly. Generated fallback test suite.",
                "All step definitions contain NotImplementedException and need manual implementation.",
                $"Original AI response: {aiResponse.Substring(0, Math.Min(200, aiResponse.Length))}..."
            }
        };
    }
}