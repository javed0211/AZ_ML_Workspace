using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using Bogus;

namespace AzureMLWorkspace.Tests.Framework.Utilities;

/// <summary>
/// Utility class for generating test data
/// </summary>
public static class TestDataGenerator
{
    private static readonly Fixture _fixture = new();
    private static readonly Faker _faker = new();

    /// <summary>
    /// Generates a random string with specified length
    /// </summary>
    public static string GenerateRandomString(int length = 10)
    {
        return _faker.Random.String2(length);
    }

    /// <summary>
    /// Generates a random alphanumeric string
    /// </summary>
    public static string GenerateAlphanumericString(int length = 10)
    {
        return _faker.Random.AlphaNumeric(length);
    }

    /// <summary>
    /// Generates a random email address
    /// </summary>
    public static string GenerateEmail()
    {
        return _faker.Internet.Email();
    }

    /// <summary>
    /// Generates a random workspace name
    /// </summary>
    public static string GenerateWorkspaceName()
    {
        return $"test-workspace-{_faker.Random.AlphaNumeric(8).ToLower()}";
    }

    /// <summary>
    /// Generates a random compute instance name
    /// </summary>
    public static string GenerateComputeName()
    {
        return $"test-compute-{_faker.Random.AlphaNumeric(8).ToLower()}";
    }

    /// <summary>
    /// Generates a random dataset name
    /// </summary>
    public static string GenerateDatasetName()
    {
        return $"test-dataset-{_faker.Random.AlphaNumeric(8).ToLower()}";
    }

    /// <summary>
    /// Generates a random experiment name
    /// </summary>
    public static string GenerateExperimentName()
    {
        return $"test-experiment-{_faker.Random.AlphaNumeric(8).ToLower()}";
    }

    /// <summary>
    /// Generates a random model name
    /// </summary>
    public static string GenerateModelName()
    {
        return $"test-model-{_faker.Random.AlphaNumeric(8).ToLower()}";
    }

    /// <summary>
    /// Generates a random search query
    /// </summary>
    public static string GenerateSearchQuery()
    {
        var topics = new[] { "climate", "weather", "temperature", "precipitation", "data", "analysis", "machine learning" };
        return _faker.PickRandom(topics);
    }

    /// <summary>
    /// Generates test user data
    /// </summary>
    public static TestUser GenerateTestUser()
    {
        return new TestUser
        {
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Role = _faker.PickRandom("Contributor", "Reader", "Owner"),
            Department = _faker.PickRandom("Data Science", "Engineering", "Research", "Analytics")
        };
    }

    /// <summary>
    /// Generates test compute configuration
    /// </summary>
    public static ComputeConfiguration GenerateComputeConfiguration()
    {
        return new ComputeConfiguration
        {
            Name = GenerateComputeName(),
            VmSize = _faker.PickRandom("Standard_DS3_v2", "Standard_DS4_v2", "Standard_NC6"),
            MinNodes = _faker.Random.Int(0, 2),
            MaxNodes = _faker.Random.Int(1, 10),
            IdleTimeBeforeScaleDown = TimeSpan.FromMinutes(_faker.Random.Int(5, 30))
        };
    }

    /// <summary>
    /// Generates test dataset metadata
    /// </summary>
    public static DatasetMetadata GenerateDatasetMetadata()
    {
        return new DatasetMetadata
        {
            Name = GenerateDatasetName(),
            Description = _faker.Lorem.Sentence(),
            Tags = GenerateTags(),
            DataType = _faker.PickRandom("Tabular", "File", "Image", "Text"),
            Size = _faker.Random.Long(1024, 1024 * 1024 * 1024), // 1KB to 1GB
            CreatedBy = _faker.Name.FullName(),
            CreatedDate = _faker.Date.Recent(30)
        };
    }

    /// <summary>
    /// Generates random tags
    /// </summary>
    public static Dictionary<string, string> GenerateTags(int count = 3)
    {
        var tags = new Dictionary<string, string>();
        var tagKeys = new[] { "Environment", "Project", "Owner", "Department", "CostCenter", "Purpose" };
        var tagValues = new[] { "Test", "Development", "Production", "Research", "Analytics", "ML" };

        for (int i = 0; i < count; i++)
        {
            var key = _faker.PickRandom(tagKeys);
            var value = _faker.PickRandom(tagValues);
            tags.TryAdd(key, value);
        }

        return tags;
    }

    /// <summary>
    /// Generates a random file path
    /// </summary>
    public static string GenerateFilePath(string extension = ".csv")
    {
        return $"/test-data/{_faker.System.FileName(extension)}";
    }

    /// <summary>
    /// Generates test configuration for different environments
    /// </summary>
    public static EnvironmentConfiguration GenerateEnvironmentConfiguration(string environment)
    {
        return new EnvironmentConfiguration
        {
            Name = environment,
            SubscriptionId = Guid.NewGuid().ToString(),
            ResourceGroup = $"rg-{environment}-{_faker.Random.AlphaNumeric(6).ToLower()}",
            WorkspaceName = $"ws-{environment}-{_faker.Random.AlphaNumeric(6).ToLower()}",
            Region = _faker.PickRandom("eastus", "westus2", "northeurope", "southeastasia"),
            Tags = GenerateTags()
        };
    }

    /// <summary>
    /// Creates an instance of a type with random data using AutoFixture
    /// </summary>
    public static T Create<T>()
    {
        return _fixture.Create<T>();
    }

    /// <summary>
    /// Creates multiple instances of a type with random data
    /// </summary>
    public static IEnumerable<T> CreateMany<T>(int count = 3)
    {
        return _fixture.CreateMany<T>(count);
    }

    /// <summary>
    /// Customizes the fixture for specific types
    /// </summary>
    public static void CustomizeFixture<T>(Func<ICustomizationComposer<T>, ISpecimenBuilder> customization)
    {
        _fixture.Customize(customization);
    }
}

/// <summary>
/// Test user data model
/// </summary>
public class TestUser
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

/// <summary>
/// Compute configuration data model
/// </summary>
public class ComputeConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string VmSize { get; set; } = string.Empty;
    public int MinNodes { get; set; }
    public int MaxNodes { get; set; }
    public TimeSpan IdleTimeBeforeScaleDown { get; set; }
}

/// <summary>
/// Dataset metadata model
/// </summary>
public class DatasetMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new();
    public string DataType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Environment configuration model
/// </summary>
public class EnvironmentConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string ResourceGroup { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new();
}