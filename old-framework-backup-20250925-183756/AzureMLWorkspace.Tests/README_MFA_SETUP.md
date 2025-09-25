# Microsoft MFA Automation Setup Guide

This guide explains how to set up automatic TOTP (Time-based One-Time Password) generation for Microsoft MFA authentication.

## Overview

The system now supports automatic OTP generation using the OTP.NET library, eliminating the need for manual MFA code entry during test automation.

## Configuration

### 1. Enable MFA Automation

In your `appsettings.json`, ensure MFA is configured:

```json
{
  "Authentication": {
    "MFA": {
      "Enabled": true,
      "AutoSubmitOTP": true,
      "OTPTimeoutSeconds": 120,
      "TOTPSecretKey": "YOUR_SECRET_KEY_HERE"
    }
  }
}
```

### 2. Obtain Your TOTP Secret Key

To get your TOTP secret key from Microsoft Authenticator:

#### Method 1: During Initial Setup
1. When setting up MFA in Azure/Microsoft 365, choose "Authenticator app"
2. Instead of scanning the QR code, click "Can't scan image?"
3. Copy the **secret key** (Base32 encoded string)
4. Add this key to your `appsettings.json` as `TOTPSecretKey`

#### Method 2: From Existing Setup
1. Open Microsoft Authenticator app
2. Find your Microsoft account
3. Tap the three dots (⋯) next to the account
4. Select "Export" or "Show QR code"
5. The secret key should be visible or you can decode it from the QR code

### 3. Secret Key Format

The secret key should be a Base32-encoded string, typically looking like:
```
JBSWY3DPEHPK3PXP
```

**Important**: Keep this secret key secure and never commit it to version control!

## How It Works

1. **MFA Detection**: System detects Microsoft MFA challenge page
2. **OTP Generation**: Generates current TOTP code using your secret key
3. **Automatic Entry**: Enters the code into the MFA input field
4. **Auto-Submit**: Clicks the verify/submit button
5. **Completion Detection**: Waits for successful authentication

## Supported MFA Input Fields

The system recognizes various Microsoft MFA input selectors:
- `input[name='otc']` - Standard OTP input
- `input[id='idTxtBx_SAOTCC_OTC']` - Microsoft-specific MFA input
- `input[placeholder*='code']` - Generic code inputs
- `input[type='tel'][maxlength='6']` - 6-digit telephone inputs
- `input[type='text'][maxlength='6']` - 6-digit text inputs

## Fallback Behavior

If automatic OTP submission fails:
1. System logs the error
2. Falls back to manual MFA completion
3. Provides clear instructions for manual entry
4. Waits for user to complete MFA manually

## Testing Your Setup

Run the OTP service tests to verify your configuration:

```bash
dotnet test --filter "FullyQualifiedName~OTPServiceTests"
```

This will:
- Validate your secret key format
- Generate a test OTP code
- Show remaining time for the current code

## Security Considerations

1. **Secret Key Storage**: Store the secret key securely
2. **Environment Variables**: Consider using environment variables for production
3. **Access Control**: Limit access to configuration files containing the secret key
4. **Key Rotation**: Regularly rotate your MFA secret keys

## Troubleshooting

### Invalid Secret Key
- Error: "Failed to generate TOTP code. Please verify the secret key format"
- Solution: Ensure the secret key is valid Base32 format

### MFA Field Not Found
- Error: "Could not find OTP input field to enter the generated code"
- Solution: Microsoft may have changed their MFA UI. Check logs for detected selectors

### Time Synchronization
- Issue: Generated codes don't work
- Solution: Ensure system time is synchronized (TOTP is time-sensitive)

### Code Timing
- Issue: Code expires before submission
- Solution: The system shows remaining time - codes refresh every 30 seconds

## Example Usage

```csharp
// The system automatically handles MFA when configured
await actor.AttemptsTo(LoginToAzurePortal.FromConfiguration());

// Manual OTP generation (for testing)
var otpService = serviceProvider.GetRequiredService<OTPService>();
var currentCode = otpService.GenerateOneTimeCode();
Console.WriteLine($"Current OTP: {currentCode}");
Console.WriteLine($"Expires in: {OTPService.GetRemainingTimeSeconds()} seconds");
```

## Configuration Examples

### Development Environment
```json
{
  "Authentication": {
    "MFA": {
      "Enabled": true,
      "AutoSubmitOTP": true,
      "OTPTimeoutSeconds": 120,
      "TOTPSecretKey": "JBSWY3DPEHPK3PXP"
    }
  }
}
```

### Production Environment (using environment variables)
```json
{
  "Authentication": {
    "MFA": {
      "Enabled": true,
      "AutoSubmitOTP": true,
      "OTPTimeoutSeconds": 120,
      "TOTPSecretKey": "${MFA_TOTP_SECRET_KEY}"
    }
  }
}
```

Set the environment variable:
```bash
set MFA_TOTP_SECRET_KEY=YOUR_ACTUAL_SECRET_KEY
```