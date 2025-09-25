using Microsoft.Extensions.Configuration;

namespace AzureMLWorkspace.Tests.Framework.Configuration;

/// <summary>
/// Helper class to provide consistent configuration loading from a single appsettings.json file
/// </summary>
public static class ConfigurationHelper
{
    /// <summary>
    /// Creates a configuration from the single appsettings.json file
    /// </summary>
    /// <param name="environment">The environment name (Development, Test, Demo, PIM, etc.)</param>
    /// <returns>Built IConfiguration instance</returns>
    public static IConfiguration BuildConfiguration(string? environment = null)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Add environment variables and command line args for overrides
        builder.AddEnvironmentVariables();

        return builder.Build();
    }

    /// <summary>
    /// Creates a TestConfiguration instance with the specified environment
    /// </summary>
    /// <param name="environment">The environment name (Development, Test, Demo, PIM, etc.)</param>
    /// <returns>TestConfiguration instance</returns>
    public static TestConfiguration LoadTestConfiguration(string? environment = null)
    {
        var configuration = BuildConfiguration();
        
        // Determine which environment to use
        var targetEnvironment = environment ?? GetCurrentEnvironment();
        
        // Create a merged configuration with environment-specific values
        var mergedConfig = CreateEnvironmentSpecificConfiguration(configuration, targetEnvironment);
        
        return TestConfiguration.LoadFromConfiguration(mergedConfig);
    }

    /// <summary>
    /// Gets the current environment name from various sources
    /// </summary>
    /// <returns>Environment name</returns>
    public static string GetCurrentEnvironment()
    {
        // Check environment variable first
        var envVar = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!string.IsNullOrEmpty(envVar))
            return envVar;

        // Check configuration file
        var config = BuildConfiguration();
        var configEnv = config["CurrentEnvironment"];
        if (!string.IsNullOrEmpty(configEnv))
            return configEnv;

        // Default to Development
        return "Development";
    }

    /// <summary>
    /// Sets the environment for the current process
    /// </summary>
    /// <param name="environment">Environment name to set</param>
    public static void SetEnvironment(string environment)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
    }

    /// <summary>
    /// Gets available environments from the configuration
    /// </summary>
    /// <returns>List of available environment names</returns>
    public static List<string> GetAvailableEnvironments()
    {
        var config = BuildConfiguration();
        var environments = config.GetSection("Environments").GetChildren();
        return environments.Select(env => env.Key).ToList();
    }

    /// <summary>
    /// Creates a configuration that merges base settings with environment-specific overrides
    /// </summary>
    /// <param name="baseConfig">The base configuration</param>
    /// <param name="environment">The target environment</param>
    /// <returns>Merged configuration</returns>
    private static IConfiguration CreateEnvironmentSpecificConfiguration(IConfiguration baseConfig, string environment)
    {
        var builder = new ConfigurationBuilder();
        
        // Add base configuration as in-memory collection
        var baseSettings = new Dictionary<string, string?>();
        
        // Copy all base settings (non-environment specific)
        CopyConfigurationSection(baseConfig, baseSettings, "", new[] { "Environments", "CurrentEnvironment" });
        
        // Add environment-specific overrides
        var envSection = baseConfig.GetSection($"Environments:{environment}");
        if (envSection.Exists())
        {
            CopyConfigurationSection(envSection, baseSettings, "");
        }
        
        builder.AddInMemoryCollection(baseSettings);
        
        // Add environment variables for final overrides
        builder.AddEnvironmentVariables();
        
        return builder.Build();
    }

    /// <summary>
    /// Recursively copies configuration sections to a dictionary
    /// </summary>
    private static void CopyConfigurationSection(IConfiguration config, Dictionary<string, string?> target, string prefix, string[]? excludeSections = null)
    {
        foreach (var child in config.GetChildren())
        {
            var key = string.IsNullOrEmpty(prefix) ? child.Key : $"{prefix}:{child.Key}";
            
            // Skip excluded sections
            if (excludeSections?.Contains(child.Key) == true && string.IsNullOrEmpty(prefix))
                continue;
            
            if (child.Value != null)
            {
                target[key] = child.Value;
            }
            else
            {
                CopyConfigurationSection(child, target, key);
            }
        }
    }
}