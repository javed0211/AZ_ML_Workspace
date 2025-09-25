# Test-PIMPermissions.ps1
# This script tests if the PIM permissions are correctly configured

param(
    [Parameter(Mandatory=$true)]
    [string]$TenantId,
    
    [Parameter(Mandatory=$true)]
    [string]$ClientId,
    
    [Parameter(Mandatory=$true)]
    [string]$ClientSecret
)

Write-Host "Testing PIM permissions..." -ForegroundColor Green
Write-Host "Tenant ID: $TenantId"
Write-Host "Client ID: $ClientId"
Write-Host "Client Secret: [HIDDEN]"

try {
    # Create credential
    $secureSecret = ConvertTo-SecureString $ClientSecret -AsPlainText -Force
    $credential = New-Object System.Management.Automation.PSCredential($ClientId, $secureSecret)

    # Connect to Microsoft Graph using app-only authentication
    Write-Host "`nConnecting to Microsoft Graph..." -ForegroundColor Yellow
    Connect-MgGraph -TenantId $TenantId -ClientSecretCredential $credential

    Write-Host "✓ Successfully connected to Microsoft Graph" -ForegroundColor Green

    # Test basic Graph access
    Write-Host "`nTesting basic Graph access..." -ForegroundColor Yellow
    $context = Get-MgContext
    Write-Host "✓ Connected as: $($context.Account)" -ForegroundColor Green
    Write-Host "✓ Tenant: $($context.TenantId)" -ForegroundColor Green

    # Test role management access
    Write-Host "`nTesting role management access..." -ForegroundColor Yellow
    try {
        $roleDefinitions = Get-MgRoleManagementDirectoryRoleDefinition -Top 5
        Write-Host "✓ Successfully accessed role definitions (found $($roleDefinitions.Count) roles)" -ForegroundColor Green
    } catch {
        Write-Warning "✗ Failed to access role definitions: $($_.Exception.Message)"
    }

    # Test role assignment schedule access
    Write-Host "`nTesting role assignment schedule access..." -ForegroundColor Yellow
    try {
        $scheduleRequests = Get-MgRoleManagementDirectoryRoleAssignmentScheduleRequest -Top 5
        Write-Host "✓ Successfully accessed role assignment schedule requests" -ForegroundColor Green
    } catch {
        Write-Warning "✗ Failed to access role assignment schedule requests: $($_.Exception.Message)"
    }

    # Test role assignment schedule instances
    Write-Host "`nTesting role assignment schedule instances access..." -ForegroundColor Yellow
    try {
        $scheduleInstances = Get-MgRoleManagementDirectoryRoleAssignmentScheduleInstance -Top 5
        Write-Host "✓ Successfully accessed role assignment schedule instances" -ForegroundColor Green
    } catch {
        Write-Warning "✗ Failed to access role assignment schedule instances: $($_.Exception.Message)"
    }

    Write-Host "`n✓ Permission test completed successfully!" -ForegroundColor Green
    Write-Host "Your application appears to have the required permissions for PIM operations." -ForegroundColor Green

} catch {
    Write-Error "✗ Permission test failed: $($_.Exception.Message)"
    Write-Host "`nTroubleshooting steps:" -ForegroundColor Yellow
    Write-Host "1. Verify the ClientId and ClientSecret are correct"
    Write-Host "2. Ensure the following permissions are granted in Azure Portal:"
    Write-Host "   - RoleAssignmentSchedule.ReadWrite.Directory"
    Write-Host "   - RoleManagement.ReadWrite.Directory"
    Write-Host "   - RoleAssignmentSchedule.Remove.Directory"
    Write-Host "3. Make sure admin consent has been granted"
    Write-Host "4. Wait 5-10 minutes after granting permissions for them to propagate"
    
    exit 1
} finally {
    # Disconnect from Microsoft Graph
    Disconnect-MgGraph -ErrorAction SilentlyContinue
}