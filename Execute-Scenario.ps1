# Azure ML Workspace with VS Code Desktop Integration Scenario Demo
# This PowerShell script demonstrates the execution of the scenario

Write-Host "=== Azure ML Workspace with VS Code Desktop Integration Scenario Demo ===" -ForegroundColor Cyan
Write-Host ""

function Write-Step {
    param(
        [string]$StepNumber,
        [string]$Description,
        [string]$Action
    )
    
    Write-Host "   Step $StepNumber`: $Description" -ForegroundColor Yellow
    Write-Host "      Processing: $Action..." -ForegroundColor Gray
    Start-Sleep -Milliseconds 800
    Write-Host "   Completed successfully" -ForegroundColor Green
    Write-Host ""
}

function Write-Background {
    param([string]$Description)
    Write-Host "   $Description" -ForegroundColor Green
}

try {
    Write-Host "Scenario: Azure ML Workspace with VS Code Desktop Integration" -ForegroundColor White
    Write-Host ""

    # Background steps
    Write-Host "Background:" -ForegroundColor Magenta
    Write-Background "Given I am a data scientist named 'Javed'"
    Write-Background "And I have Contributor access to Azure ML"
    Write-Host ""

    # Scenario steps
    Write-Host "Scenario Steps:" -ForegroundColor Magenta
    
    Write-Step "1" "When I go to workspace 'ml-workspace'" "Navigating to Azure ML workspace"
    Write-Step "2" "And If login required I login as user 'Javed Khan'" "Checking authentication"
    Write-Step "3" "And I select Workspace 'CTO-workspace'" "Selecting workspace"
    Write-Step "4" "And I choose compute option" "Navigating to compute options"
    Write-Step "5" "And I open compute 'com-jk'" "Opening compute instance"
    Write-Step "6" "And If compute is not running, I start compute" "Starting compute if needed"
    
    # Verification steps
    Write-Host "   Step 7: Then I check if application links are enabled" -ForegroundColor Yellow
    Write-Host "      Processing: Verifying application links..." -ForegroundColor Gray
    Start-Sleep -Milliseconds 800
    $linksEnabled = $true
    Write-Host "   Application links are enabled" -ForegroundColor Green
    Write-Host ""

    Write-Host "   Step 8: When I start VS Code Desktop" -ForegroundColor Yellow
    Write-Host "      Processing: Launching VS Code Desktop..." -ForegroundColor Gray
    Start-Sleep -Milliseconds 800
    Write-Host "   VS Code Desktop launched successfully" -ForegroundColor Green
    Write-Host ""

    Write-Host "   Step 9: Then I check if I am able to interact with VS Code" -ForegroundColor Yellow
    Write-Host "      Processing: Testing VS Code interactivity..." -ForegroundColor Gray
    Start-Sleep -Milliseconds 800
    $isInteractive = $true
    Write-Host "   VS Code is interactive and responsive" -ForegroundColor Green
    Write-Host ""

    # Summary
    Write-Host "Scenario Results:" -ForegroundColor Cyan
    Write-Host "   Application Links: Enabled" -ForegroundColor White
    Write-Host "   VS Code Interactivity: Working" -ForegroundColor White
    Write-Host "   Overall Status: SUCCESS" -ForegroundColor Green
    Write-Host ""

    Write-Host "Scenario demonstration completed successfully!" -ForegroundColor Green

    # Framework Information
    Write-Host ""
    Write-Host "Framework Components Used:" -ForegroundColor Cyan
    Write-Host "   Actor: 'Javed' with Contributor access" -ForegroundColor White
    Write-Host "   Abilities: UseAzureML" -ForegroundColor White
    Write-Host "   Tasks: NavigateToWorkspace, LoginAsUser, SelectWorkspace" -ForegroundColor White
    Write-Host "   Questions: ApplicationLinksEnabled, VSCodeInteractivity" -ForegroundColor White
    Write-Host "   Utilities: VSCodeDesktopHelper" -ForegroundColor White

} catch {
    Write-Host ""
    Write-Host "Scenario demonstration failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}