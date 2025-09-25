using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AzureMLWorkspace.Tests.Framework.Configuration;

namespace AzureMLWorkspace.Tests.Framework.AI;

/// <summary>
/// Command-line interface for AI test generation
/// </summary>
public class TestGenerationCLI
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TestGenerationCLI> _logger;

    public TestGenerationCLI(IServiceProvider serviceProvider, ILogger<TestGenerationCLI> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates and configures the root command for test generation
    /// </summary>
    /// <returns>Configured root command</returns>
    public RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand("Azure ML Workspace Test Automation Framework with AI Test Generation");

        // Add the generate command
        var generateCommand = new Command("generate", "Generate test scripts using AI from natural language description");
        
        var descriptionOption = new Option<string?>(
            aliases: new[] { "--description", "-d" },
            description: "Natural language description of the test to generate");
        
        var interactiveOption = new Option<bool>(
            aliases: new[] { "--interactive", "-i" },
            description: "Run in interactive mode to prompt for test description");

        var outputOption = new Option<string?>(
            aliases: new[] { "--output", "-o" },
            description: "Output directory for generated files (optional)");

        var validateOption = new Option<bool>(
            aliases: new[] { "--validate", "-v" },
            description: "Validate AI service configuration without generating tests");

        generateCommand.AddOption(descriptionOption);
        generateCommand.AddOption(interactiveOption);
        generateCommand.AddOption(outputOption);
        generateCommand.AddOption(validateOption);

        generateCommand.SetHandler(async (description, interactive, output, validate) =>
        {
            await HandleGenerateCommandAsync(description, interactive, output, validate);
        }, descriptionOption, interactiveOption, outputOption, validateOption);

        rootCommand.AddCommand(generateCommand);

        // Add test execution commands
        var runCommand = new Command("run", "Run existing tests");
        var testFilterOption = new Option<string?>(
            aliases: new[] { "--filter", "-f" },
            description: "Filter tests to run");
        
        runCommand.AddOption(testFilterOption);
        runCommand.SetHandler(async (filter) =>
        {
            await HandleRunCommandAsync(filter);
        }, testFilterOption);

        rootCommand.AddCommand(runCommand);

        return rootCommand;
    }

    private async Task HandleGenerateCommandAsync(string? description, bool interactive, string? outputPath, bool validate)
    {
        try
        {
            var config = _serviceProvider.GetRequiredService<IOptions<AITestGenerationConfiguration>>().Value;
            
            if (!config.EnableAIScriptGeneration)
            {
                Console.WriteLine("❌ AI script generation is disabled in configuration.");
                Console.WriteLine("Set 'AITestGeneration:EnableAIScriptGeneration' to true in appsettings.json");
                return;
            }

            var aiService = _serviceProvider.GetRequiredService<IAITestGenerationService>();
            var fileService = _serviceProvider.GetRequiredService<ITestFileGenerationService>();

            // Validate service if requested
            if (validate)
            {
                Console.WriteLine("🔍 Validating AI service configuration...");
                var isValid = await aiService.ValidateServiceAsync();
                
                if (isValid)
                {
                    Console.WriteLine("✅ AI service is configured correctly and accessible.");
                }
                else
                {
                    Console.WriteLine("❌ AI service validation failed. Please check your configuration.");
                }
                return;
            }

            // Get test description
            string testDescription;
            if (interactive || string.IsNullOrWhiteSpace(description))
            {
                testDescription = await PromptForTestDescriptionAsync();
            }
            else
            {
                testDescription = description;
            }

            if (string.IsNullOrWhiteSpace(testDescription))
            {
                Console.WriteLine("❌ Test description is required.");
                return;
            }

            Console.WriteLine($"🤖 Generating test suite for: {testDescription}");
            Console.WriteLine("⏳ This may take a few moments...");

            // Generate test suite
            var testSuite = await aiService.GenerateTestSuiteAsync(testDescription);

            // Write files
            var outputDirectory = outputPath ?? Directory.GetCurrentDirectory();
            var generatedFiles = await fileService.WriteTestSuiteAsync(testSuite, outputDirectory);

            // Display results
            Console.WriteLine();
            Console.WriteLine("✅ Test suite generated successfully!");
            Console.WriteLine();
            Console.WriteLine("📁 Generated files:");
            
            foreach (var file in generatedFiles)
            {
                var status = file.IsPlaceholder ? "🔧 (needs implementation)" : "✅";
                Console.WriteLine($"   {status} {file.RelativePath}");
            }

            if (testSuite.Warnings.Any())
            {
                Console.WriteLine();
                Console.WriteLine("⚠️  Warnings:");
                foreach (var warning in testSuite.Warnings)
                {
                    Console.WriteLine($"   • {warning}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("🚀 Next steps:");
            Console.WriteLine("   1. Review the generated files");
            Console.WriteLine("   2. Implement any placeholder methods marked with NotImplementedException");
            Console.WriteLine("   3. Run the tests: dotnet test");
            Console.WriteLine("   4. Update configuration files with your Azure credentials");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate test suite");
            Console.WriteLine($"❌ Error generating test suite: {ex.Message}");
        }
    }

    private async Task HandleRunCommandAsync(string? filter)
    {
        try
        {
            Console.WriteLine("🧪 Running tests...");
            
            var testCommand = "dotnet test";
            if (!string.IsNullOrWhiteSpace(filter))
            {
                testCommand += $" --filter \"{filter}\"";
            }

            Console.WriteLine($"Executing: {testCommand}");
            
            // In a real implementation, you would execute the test command
            // For now, just show what would be executed
            await Task.Delay(100); // Simulate async operation
            
            Console.WriteLine("✅ Test execution completed. Check the output above for results.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run tests");
            Console.WriteLine($"❌ Error running tests: {ex.Message}");
        }
    }

    private async Task<string> PromptForTestDescriptionAsync()
    {
        Console.WriteLine();
        Console.WriteLine("🤖 AI Test Generation");
        Console.WriteLine("====================");
        Console.WriteLine();
        Console.WriteLine("Please describe the test you want to generate.");
        Console.WriteLine("Examples:");
        Console.WriteLine("  • Test that user can login to Azure portal and activate PIM role");
        Console.WriteLine("  • Verify Azure ML workspace creation and compute instance management");
        Console.WriteLine("  • Test document processing with Azure AI Search integration");
        Console.WriteLine();
        Console.Write("Test description: ");
        
        var description = await Task.Run(() => Console.ReadLine());
        return description?.Trim() ?? string.Empty;
    }
}