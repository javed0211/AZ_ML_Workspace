# Test Azure CLI Integration for PIM Role Management
# This script demonstrates the Azure CLI commands used by the PIM implementation

param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$ClientId,
    
    [Parameter(Mandatory=$false)]
    [string]$ManagedIdentityName,
    
    [Parameter(Mandatory=$false)]
    [string]$UserPrincipalName
)

Write-Host "=== Testing Azure CLI Integration for PIM Role Management ===" -ForegroundColor Green

# Check if Azure CLI is installed
Write-Host "`n1. Checking Azure CLI installation..." -ForegroundColor Yellow
try {
    $azVersion = az --version 2>$null
    if ($azVersion) {
        Write-Host "✅ Azure CLI is installed" -ForegroundColor Green
        Write-Host ($azVersion | Select-Object -First 1) -ForegroundColor Gray
    } else {
        throw "Azure CLI not found"
    }
} catch {
    Write-Host "❌ Azure CLI is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Check if logged in
Write-Host "`n2. Checking Azure CLI authentication..." -ForegroundColor Yellow
try {
    $account = az account show --query "{subscriptionId:id, tenantId:tenantId, user:user.name}" -o json 2>$null | ConvertFrom-Json
    if ($account) {
        Write-Host "✅ Logged in to Azure" -ForegroundColor Green
        Write-Host "   Subscription: $($account.subscriptionId)" -ForegroundColor Gray
        Write-Host "   Tenant: $($account.tenantId)" -ForegroundColor Gray
        Write-Host "   User: $($account.user)" -ForegroundColor Gray
    } else {
        throw "Not logged in"
    }
} catch {
    Write-Host "❌ Not logged in to Azure CLI" -ForegroundColor Red
    Write-Host "Please run: az login" -ForegroundColor Yellow
    exit 1
}

# Set the correct subscription
Write-Host "`n3. Setting subscription context..." -ForegroundColor Yellow
try {
    az account set --subscription $SubscriptionId
    Write-Host "✅ Subscription set to: $SubscriptionId" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to set subscription: $SubscriptionId" -ForegroundColor Red
    exit 1
}

# Test principal ID resolution
Write-Host "`n4. Testing Principal ID Resolution..." -ForegroundColor Yellow

if ($ClientId) {
    Write-Host "   Testing Service Principal lookup..." -ForegroundColor Cyan
    try {
        $spId = az ad sp show --id $ClientId --query id -o tsv 2>$null
        if ($spId) {
            Write-Host "   ✅ Service Principal ID: $spId" -ForegroundColor Green
        } else {
            Write-Host "   ❌ Service Principal not found for ClientId: $ClientId" -ForegroundColor Red
        }
    } catch {
        Write-Host "   ❌ Error looking up Service Principal: $_" -ForegroundColor Red
    }
}

if ($ManagedIdentityName) {
    Write-Host "   Testing Managed Identity lookup..." -ForegroundColor Cyan
    try {
        $miId = az ad sp list --display-name $ManagedIdentityName --query "[0].id" -o tsv 2>$null
        if ($miId) {
            Write-Host "   ✅ Managed Identity ID: $miId" -ForegroundColor Green
        } else {
            Write-Host "   ❌ Managed Identity not found: $ManagedIdentityName" -ForegroundColor Red
        }
    } catch {
        Write-Host "   ❌ Error looking up Managed Identity: $_" -ForegroundColor Red
    }
}

if ($UserPrincipalName) {
    Write-Host "   Testing User lookup..." -ForegroundColor Cyan
    try {
        $userId = az ad user show --id $UserPrincipalName --query id -o tsv 2>$null
        if ($userId) {
            Write-Host "   ✅ User ID: $userId" -ForegroundColor Green
        } else {
            Write-Host "   ❌ User not found: $UserPrincipalName" -ForegroundColor Red
        }
    } catch {
        Write-Host "   ❌ Error looking up User: $_" -ForegroundColor Red
    }
}

# Test current user lookup
Write-Host "   Testing current signed-in user..." -ForegroundColor Cyan
try {
    $currentUserId = az ad signed-in-user show --query id -o tsv 2>$null
    if ($currentUserId) {
        Write-Host "   ✅ Current User ID: $currentUserId" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Could not get current user ID" -ForegroundColor Red
    }
} catch {
    Write-Host "   ❌ Error getting current user: $_" -ForegroundColor Red
}

# Test role definition listing
Write-Host "`n5. Testing Role Definition Resolution..." -ForegroundColor Yellow

Write-Host "   Getting all role definitions..." -ForegroundColor Cyan
try {
    $roles = az role definition list --query "[].{Name:roleName, Id:id}" -o json 2>$null | ConvertFrom-Json
    if ($roles -and $roles.Count -gt 0) {
        Write-Host "   ✅ Found $($roles.Count) role definitions" -ForegroundColor Green
        
        # Show first 5 roles as examples
        Write-Host "   Sample roles:" -ForegroundColor Gray
        $roles | Select-Object -First 5 | ForEach-Object {
            Write-Host "     - $($_.Name)" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ❌ No role definitions found" -ForegroundColor Red
    }
} catch {
    Write-Host "   ❌ Error getting role definitions: $_" -ForegroundColor Red
}

# Test specific role lookups
$testRoles = @("Owner", "Contributor", "Reader", "Data Scientist")
foreach ($roleName in $testRoles) {
    Write-Host "   Testing role lookup: '$roleName'..." -ForegroundColor Cyan
    try {
        $roleId = az role definition list --name $roleName --query "[0].id" -o tsv 2>$null
        if ($roleId) {
            Write-Host "   ✅ '$roleName' -> $roleId" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  Role '$roleName' not found (may not exist in this subscription)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "   ❌ Error looking up role '$roleName': $_" -ForegroundColor Red
    }
}

# Test pattern matching
Write-Host "   Testing pattern matching for 'Data' roles..." -ForegroundColor Cyan
try {
    $dataRoles = az role definition list --query "[?contains(roleName, 'Data')].{Name:roleName, Id:id}" -o json 2>$null | ConvertFrom-Json
    if ($dataRoles -and $dataRoles.Count -gt 0) {
        Write-Host "   ✅ Found $($dataRoles.Count) roles containing 'Data'" -ForegroundColor Green
        $dataRoles | ForEach-Object {
            Write-Host "     - $($_.Name)" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ⚠️  No roles found containing 'Data'" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ❌ Error searching for Data roles: $_" -ForegroundColor Red
}

Write-Host "`n=== Azure CLI Integration Test Complete ===" -ForegroundColor Green
Write-Host "The PIM Role Management implementation will use these Azure CLI commands for:" -ForegroundColor White
Write-Host "- Principal ID resolution (with Graph API fallback)" -ForegroundColor White
Write-Host "- Role definition resolution (with GUID fallback)" -ForegroundColor White
Write-Host "- Better error handling and debugging" -ForegroundColor White

# Example usage
Write-Host "`n=== Example Usage in Code ===" -ForegroundColor Cyan
Write-Host @"
// Now you can use role names directly:
var result = await pimAbility.ActivateRoleAsync(
    roleDefinitionId: "Data Scientist",  // Role name
    justification: "Test execution",
    durationHours: 8
);

// Or still use GUIDs:
var result = await pimAbility.ActivateRoleAsync(
    roleDefinitionId: "c8d4ff99-41c3-41a8-9f60-21dfdad59608",
    justification: "Test execution", 
    durationHours: 8
);

// Get all available roles:
var roles = await pimAbility.GetAvailableRoleDefinitionsAsync();
"@ -ForegroundColor Gray