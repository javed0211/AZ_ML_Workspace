using OtpNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Services;

/// <summary>
/// Service for generating Time-based One-Time Passwords (TOTP) for MFA authentication
/// </summary>
public class OTPService
{
    private readonly ILogger<OTPService> _logger;
    private readonly IConfiguration _configuration;

    public OTPService(ILogger<OTPService> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Generates a TOTP code using the configured secret key
    /// </summary>
    /// <returns>6-digit TOTP code</returns>
    public string GenerateOneTimeCode()
    {
        // Use environment-aware configuration path
        var currentEnvironment = _configuration.GetValue<string>("CurrentEnvironment", "Development");
        var authPath = $"Environments:{currentEnvironment}:Authentication";
        var secretKey = _configuration.GetValue<string>($"{authPath}:MFA:TOTPSecretKey");

        _logger.LogDebug("Reading TOTP secret key from path: {AuthPath}:MFA:TOTPSecretKey", authPath);

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException($"TOTP secret key is not configured. Please set {authPath}:MFA:TOTPSecretKey in appsettings.json");
        }

        return GenerateOneTimeCode(secretKey);
    }

    /// <summary>
    /// Generates a TOTP code using the provided secret key
    /// </summary>
    /// <param name="key">Base32-encoded secret key</param>
    /// <returns>6-digit TOTP code</returns>
    private static string GenerateOneTimeCode(string key)
    {
        try
        {
            byte[] base32Bytes = Base32Encoding.ToBytes(key);
            var totp = new Totp(base32Bytes);
            var result = totp.ComputeTotp();
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate TOTP code. Please verify the secret key format: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates if the provided secret key is in correct Base32 format
    /// </summary>
    /// <param name="secretKey">Secret key to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidSecretKey(string secretKey)
    {
        if (string.IsNullOrEmpty(secretKey))
            return false;

        try
        {
            Base32Encoding.ToBytes(secretKey);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the remaining time in seconds until the current TOTP code expires
    /// </summary>
    /// <returns>Remaining seconds</returns>
    public static int GetRemainingTimeSeconds()
    {
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var remainingTime = 30 - (unixTime % 30);
        return (int)remainingTime;
    }
}