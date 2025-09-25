using Microsoft.Extensions.Configuration;
using AzureMLWorkspace.Tests.Framework.Configuration;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing Configuration Loading...");
        
        try
        {
            // Test 1: Basic configuration loading
            var baseConfig = ConfigurationHelper.BuildConfiguration();
            Console.WriteLine($"Current Environment: {baseConfig["CurrentEnvironment"]}");
            
            // Test 2: Environment-specific configuration
            var environment = ConfigurationHelper.GetCurrentEnvironment();
            Console.WriteLine($"Detected Environment: {environment}");
            
            // Test 3: Load test configuration
            var testConfig = ConfigurationHelper.LoadTestConfiguration(environment);
            Console.WriteLine($"Azure SubscriptionId from TestConfig: {testConfig?.Azure?.SubscriptionId ?? "NULL"}");
            
            // Test 4: Create merged configuration (like in TestHooks)
            var mergedConfig = CreateEnvironmentSpecificConfiguration(baseConfig, environment);
            Console.WriteLine($"Azure:SubscriptionId from merged config: {mergedConfig["Azure:SubscriptionId"] ?? "NULL"}");
            Console.WriteLine($"Azure:ResourceGroup from merged config: {mergedConfig["Azure:ResourceGroup"] ?? "NULL"}");
            Console.WriteLine($"Azure:WorkspaceName from merged config: {mergedConfig["Azure:WorkspaceName"] ?? "NULL"}");
            Console.WriteLine($"Azure:TenantId from merged config: {mergedConfig["Azure:TenantId"] ?? "NULL"}");
            Console.WriteLine($"Authentication:Password from merged config: {(string.IsNullOrEmpty(mergedConfig["Authentication:Password"]) ? "NULL" : "***HIDDEN***")}");
            
            // Test 5: Show all keys in merged config
            Console.WriteLine("\nAll configuration keys:");
            ShowAllKeys(mergedConfig, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    private static void ShowAllKeys(IConfiguration config, string prefix)
    {
        foreach (var child in config.GetChildren())
        {
            var key = string.IsNullOrEmpty(prefix) ? child.Key : $"{prefix}:{child.Key}";
            
            if (child.Value != null)
            {
                var value = key.Contains("Password") ? "***HIDDEN***" : child.Value;
                Console.WriteLine($"  {key} = {value}");
            }
            else
            {
                ShowAllKeys(child, key);
            }
        }
    }
    
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
            Console.WriteLine($"Found environment section: Environments:{environment}");
            CopyConfigurationSection(envSection, baseSettings, "");
        }
        else
        {
            Console.WriteLine($"Environment section NOT found: Environments:{environment}");
        }
        
        builder.AddInMemoryCollection(baseSettings);
        
        // Add environment variables for final overrides
        builder.AddEnvironmentVariables();
        
        return builder.Build();
    }

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