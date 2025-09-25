# PIM (Privileged Identity Management) Setup Guide

This guide explains how to set up and configure PIM role activation for the Azure ML test automation framework.

## Overview

The framework supports automatic PIM role activation through the Azure Portal UI, eliminating the need for manual role activation before running tests. This ensures that tests have the necessary permissions to access Azure ML resources.

## Prerequisites

1. **Azure AD Premium P2 License**: Required for PIM functionality
2. **PIM Role Assignment**: User must be assigned to eligible PIM roles
3. **Azure Portal Access**: Framework uses UI automation to activate roles
4. **Proper Authentication**: Valid Azure credentials configured

## Supported PIM Roles

The framework currently supports activation of the following roles:

- **Data Scientist Role**: `PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST`
- **Custom Roles**: Any PIM role can be activated using the generic methods

## Configuration

### 1. Authentication Settings

Ensure your `appsettings.json` includes proper authentication configuration:

```json
{
  "Authentication": {
    "Username": "your-email@domain.com",
    "Password": "your-password",
    "MFA": {
      "Enabled": true,
      "AutoSubmitOTP": true,
      "TOTPSecretKey": "YOUR_TOTP_SECRET_KEY"
    }
  },
  "Azure": {
    "TenantId": "your-tenant-id",
    "SubscriptionId": "your-subscription-id",
    "ResourceGroup": "your-resource-group",
    "WorkspaceName": "your-workspace-name"
  }
}
```

### 2. PIM Role Configuration

The framework automatically handles the following PIM roles:

- **Default Role**: `PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST`
- **Default Duration**: 8 hours
- **Default Justification**: "Automated test setup - activating Data Scientist role for test execution"

## Usage

### 1. Automatic PIM Activation in Tests

The framework automatically activates PIM roles during test setup:

```csharp
[SetUp]
public async Task SetUp()
{
    // PIM role is automatically activated
    // Check if role is already active
    var isActive = await _azureMLUtils.IsDataScientistPIMRoleActiveAsync();
    
    if (!isActive)
    {
        // Activate the role
        await _azureMLUtils.ActivateDataScientistPIMRoleAsync();
    }
}
```

### 2. BDD Step Definitions

Use the following Gherkin step in your feature files:

```gherkin
Given I have activated the Data Scientist PIM role
```

This step will:
- Check if the role is already active
- Activate the role if needed
- Handle any errors gracefully

### 3. Manual PIM Activation

You can also manually activate PIM roles:

```csharp
// Activate specific role
await azureMLUtils.ActivatePIMRoleAsync(
    "PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST",
    "Manual test execution",
    8 // hours
);

// Activate Data Scientist role with defaults
await azureMLUtils.ActivateDataScientistPIMRoleAsync();

// Check if role is active
var isActive = await azureMLUtils.IsDataScientistPIMRoleActiveAsync();
```

## PIM Activation Process

The framework performs the following steps to activate a PIM role:

1. **Navigate to PIM Portal**: Opens Azure Portal PIM activation page
2. **Check Active Roles**: Verifies if role is already active (optimization)
3. **Find Eligible Role**: Locates the role in eligible assignments
4. **Click Activate**: Initiates the activation process
5. **Fill Justification**: Provides reason for activation
6. **Set Duration**: Configures activation duration
7. **Submit Request**: Submits the activation request
8. **Wait for Confirmation**: Waits for successful activation
9. **Verify Activation**: Confirms role is now active

## Error Handling

The framework includes robust error handling:

- **Timeout Errors**: If PIM pages take too long to load
- **Role Not Found**: If the specified role is not available
- **Already Active**: Skips activation if role is already active
- **Authentication Issues**: Handles login and MFA challenges
- **Graceful Degradation**: Tests continue even if PIM activation fails

## Troubleshooting

### Common Issues

1. **Role Not Found**
   - Verify the role name is correct
   - Ensure user is assigned to the eligible role
   - Check PIM configuration in Azure Portal

2. **Timeout During Activation**
   - Azure Portal may be slow
   - Check network connectivity
   - Verify authentication is working

3. **Permission Denied**
   - User may not have PIM activation rights
   - Check role assignments in Azure AD
   - Verify PIM policies allow activation

4. **MFA Issues**
   - Ensure TOTP secret key is configured
   - Check MFA settings in authentication config
   - Verify time synchronization for TOTP

### Debug Mode

Enable debug logging to troubleshoot PIM issues:

```csharp
// Enable detailed logging
_logger.LogInfo("🔍 Debug: Checking PIM role status");
var isActive = await _azureMLUtils.IsDataScientistPIMRoleActiveAsync();
_logger.LogInfo($"🔍 Debug: Role active status: {isActive}");
```

### Manual Verification

To manually verify PIM setup:

1. Open Azure Portal
2. Navigate to Azure AD → Privileged Identity Management
3. Check "My roles" → "Eligible assignments"
4. Verify the Data Scientist role is listed
5. Try manual activation to test the process

## Best Practices

1. **Role Duration**: Use appropriate activation duration (default: 8 hours)
2. **Justification**: Provide clear justification for audit purposes
3. **Error Handling**: Always handle PIM activation failures gracefully
4. **Optimization**: Check if role is already active before activation
5. **Security**: Keep authentication credentials secure
6. **Monitoring**: Monitor PIM activation logs for security

## Integration with CI/CD

For automated testing in CI/CD pipelines:

1. **Service Principal**: Consider using service principal authentication
2. **Role Assignment**: Ensure CI/CD service principal has necessary roles
3. **Secrets Management**: Use secure secret management for credentials
4. **Conditional Activation**: Only activate PIM roles when needed

## API Reference

### PIMUtils Class

```csharp
public class PIMUtils
{
    // Activate any PIM role
    Task ActivatePIMRoleAsync(string roleName, string justification, int durationHours);
    
    // Activate Data Scientist role with defaults
    Task ActivateDataScientistRoleAsync();
    
    // Check if role is active
    Task<bool> IsRoleActiveAsync(string roleName);
}
```

### AzureMLUtils PIM Methods

```csharp
public class AzureMLUtils
{
    // PIM role activation
    Task ActivatePIMRoleAsync(string roleName, string justification, int durationHours);
    Task ActivateDataScientistPIMRoleAsync();
    
    // PIM role status
    Task<bool> IsPIMRoleActiveAsync(string roleName);
    Task<bool> IsDataScientistPIMRoleActiveAsync();
}
```

## Security Considerations

1. **Credential Security**: Never commit credentials to version control
2. **Role Scope**: Use least privilege principle for role assignments
3. **Audit Trail**: PIM activations are logged for compliance
4. **Time Limits**: Use appropriate activation durations
5. **Justification**: Provide meaningful justifications for audit purposes

## Support

For issues with PIM setup or activation:

1. Check Azure AD PIM configuration
2. Verify role assignments and policies
3. Test manual activation in Azure Portal
4. Review framework logs for detailed error information
5. Contact Azure support for PIM-specific issues