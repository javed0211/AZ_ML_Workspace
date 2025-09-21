using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AzureMLWorkspace.Tests.Framework.Configuration;
using AzureMLWorkspace.Tests.Framework.AI;
using Serilog;
using Serilog.Extensions.Hosting;

namespace AzureMLWorkspace.Tests;

/// <summary>
/// Main program entry point for the test automation framework
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Configure Serilog early for startup logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/startup.log")
            .CreateLogger();

        try
        {
            var host = CreateHostBuilder(args).Build();
            
            // Check if this is a CLI command
            if (args.Length > 0 && (args.Contains("generate") || args.Contains("run") || args.Contains("--help") || args.Contains("-h")))
            {
                return await RunCLIAsync(host, args);
            }

            // Default behavior - run as test host
            await host.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task<int> RunCLIAsync(IHost host, string[] args)
    {
        try
        {
            using var scope = host.Services.CreateScope();
            var cli = scope.ServiceProvider.GetRequiredService<TestGenerationCLI>();
            var rootCommand = cli.CreateRootCommand();
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CLI Error: {ex.Message}");
            Log.Error(ex, "CLI execution failed");
            return 1;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.Console()
                    .WriteTo.File("logs/application.log", rollingInterval: RollingInterval.Day);
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                
                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.Configure<AITestGenerationConfiguration>(
                    context.Configuration.GetSection("AITestGeneration"));

                // HTTP Client
                services.AddHttpClient<IAITestGenerationService, AITestGenerationService>();

                // AI Services
                services.AddScoped<IAITestGenerationService, AITestGenerationService>();
                services.AddScoped<ITestFileGenerationService, TestFileGenerationService>();
                services.AddScoped<TestGenerationCLI>();

                // Logging
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog();
                });
            });
}