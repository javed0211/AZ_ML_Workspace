# Setup-PIMPermissions.ps1
# This script helps set up the required Microsoft Graph API permissions for PIM role management

param(
    [Parameter(Mandatory=$true)]
    [string]$AppId,
    
    [Parameter(Mandatory=$false)]
    [string]$TenantId,
    
    [Parameter(Mandatory=$false)]
    [switch]$WhatIf
)

# Required permissions for PIM operations
$RequiredPermissions = @(
    "RoleAssignmentSchedule.ReadWrite.Directory",
    "RoleManagement.ReadWrite.Directory", 
    "RoleAssignmentSchedule.Remove.Directory"
)

Write-Host "Setting up PIM permissions for application: $AppId" -ForegroundColor Green

try {
    # Connect to Microsoft Graph
    if ($TenantId) {
        Write-Host "Connecting to Microsoft Graph with tenant: $TenantId"
        Connect-MgGraph -TenantId $TenantId -Scopes "Application.ReadWrite.All", "AppRoleAssignment.ReadWrite.All"
    } else {
        Write-Host "Connecting to Microsoft Graph..."
        Connect-MgGraph -Scopes "Application.ReadWrite.All", "AppRoleAssignment.ReadWrite.All"
    }

    # Get the application
    Write-Host "Looking up application: $AppId"
    $app = Get-MgApplication -Filter "AppId eq '$AppId'"
    
    if (-not $app) {
        Write-Error "Application with AppId '$AppId' not found"
        exit 1
    }

    Write-Host "Found application: $($app.DisplayName)" -ForegroundColor Green

    # Get Microsoft Graph service principal
    $graphServicePrincipal = Get-MgServicePrincipal -Filter "AppId eq '00000003-0000-0000-c000-000000000000'"
    
    if (-not $graphServicePrincipal) {
        Write-Error "Microsoft Graph service principal not found"
        exit 1
    }

    # Get the service principal for our app
    $appServicePrincipal = Get-MgServicePrincipal -Filter "AppId eq '$AppId'"
    
    if (-not $appServicePrincipal) {
        Write-Error "Service principal for application '$AppId' not found. Please create one first."
        exit 1
    }

    Write-Host "Checking required permissions..." -ForegroundColor Yellow

    foreach ($permission in $RequiredPermissions) {
        Write-Host "  Checking permission: $permission"
        
        # Find the app role in Microsoft Graph
        $appRole = $graphServicePrincipal.AppRoles | Where-Object { $_.Value -eq $permission }
        
        if (-not $appRole) {
            Write-Warning "  Permission '$permission' not found in Microsoft Graph app roles"
            continue
        }

        # Check if permission is already granted
        $existingAssignment = Get-MgServicePrincipalAppRoleAssignment -ServicePrincipalId $appServicePrincipal.Id | 
            Where-Object { $_.AppRoleId -eq $appRole.Id -and $_.ResourceId -eq $graphServicePrincipal.Id }

        if ($existingAssignment) {
            Write-Host "  ✓ Permission '$permission' already granted" -ForegroundColor Green
        } else {
            if ($WhatIf) {
                Write-Host "  [WHAT-IF] Would grant permission: $permission" -ForegroundColor Cyan
            } else {
                Write-Host "  Granting permission: $permission" -ForegroundColor Yellow
                
                try {
                    New-MgServicePrincipalAppRoleAssignment -ServicePrincipalId $appServicePrincipal.Id -Body @{
                        PrincipalId = $appServicePrincipal.Id
                        ResourceId = $graphServicePrincipal.Id
                        AppRoleId = $appRole.Id
                    } | Out-Null
                    
                    Write-Host "  ✓ Successfully granted permission: $permission" -ForegroundColor Green
                } catch {
                    Write-Error "  ✗ Failed to grant permission '$permission': $($_.Exception.Message)"
                }
            }
        }
    }

    if ($WhatIf) {
        Write-Host "`nWHAT-IF mode completed. No changes were made." -ForegroundColor Cyan
        Write-Host "Run the script without -WhatIf to apply the changes." -ForegroundColor Cyan
    } else {
        Write-Host "`nPermission setup completed!" -ForegroundColor Green
        Write-Host "Note: It may take 5-10 minutes for permissions to propagate." -ForegroundColor Yellow
    }

} catch {
    Write-Error "An error occurred: $($_.Exception.Message)"
    exit 1
} finally {
    # Disconnect from Microsoft Graph
    Disconnect-MgGraph -ErrorAction SilentlyContinue
}

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Verify the permissions in Azure Portal → App registrations → Your App → API permissions"
Write-Host "2. Ensure admin consent has been granted"
Write-Host "3. Update your appsettings.json with the correct ClientId and ClientSecret"
Write-Host "4. Test the PIM role activation functionality"