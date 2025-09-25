using AzureMLWorkspace.Tests.Framework.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace AzureMLWorkspace.Tests.Tests;

[TestFixture]
public class OTPServiceTests
{
    private ILogger<OTPService> _logger = null!;
    private IConfiguration _configuration = null!;
    private OTPService _otpService = null!;

    [SetUp]
    public void Setup()
    {
        // Create a mock logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<OTPService>();

        // Create a mock configuration with a test secret key
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:MFA:TOTPSecretKey"] = "JBSWY3DPEHPK3PXP" // Test key: "Hello!"
        });
        _configuration = configBuilder.Build();

        _otpService = new OTPService(_logger, _configuration);
    }

    [Test]
    public void GenerateOneTimeCode_WithValidSecretKey_ShouldReturnSixDigitCode()
    {
        // Act
        var otpCode = _otpService.GenerateOneTimeCode();

        // Assert
        Assert.That(otpCode, Is.Not.Null);
        Assert.That(otpCode, Has.Length.EqualTo(6));
        Assert.That(otpCode, Does.Match(@"^\d{6}$"), "OTP should be 6 digits");
        
        Console.WriteLine($"Generated OTP: {otpCode}");
        Console.WriteLine($"Remaining time: {OTPService.GetRemainingTimeSeconds()} seconds");
    }

    [Test]
    public void GenerateOneTimeCode_WithoutSecretKey_ShouldThrowException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:MFA:TOTPSecretKey"] = ""
        });
        var emptyConfig = configBuilder.Build();
        var otpServiceWithoutKey = new OTPService(_logger, emptyConfig);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => otpServiceWithoutKey.GenerateOneTimeCode());
        Assert.That(ex.Message, Does.Contain("TOTP secret key is not configured"));
    }

    [Test]
    public void IsValidSecretKey_WithValidKey_ShouldReturnTrue()
    {
        // Act & Assert
        Assert.That(OTPService.IsValidSecretKey("JBSWY3DPEHPK3PXP"), Is.True);
    }

    [Test]
    public void IsValidSecretKey_WithInvalidKey_ShouldReturnFalse()
    {
        // Act & Assert
        Assert.That(OTPService.IsValidSecretKey("invalid-key"), Is.False);
        Assert.That(OTPService.IsValidSecretKey(""), Is.False);
        Assert.That(OTPService.IsValidSecretKey(null!), Is.False);
    }

    [Test]
    public void GetRemainingTimeSeconds_ShouldReturnValidRange()
    {
        // Act
        var remainingTime = OTPService.GetRemainingTimeSeconds();

        // Assert
        Assert.That(remainingTime, Is.GreaterThan(0));
        Assert.That(remainingTime, Is.LessThanOrEqualTo(30));
        
        Console.WriteLine($"Remaining time: {remainingTime} seconds");
    }
}