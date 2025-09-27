# VS Code Notebook Automation Test Runner (PowerShell)
# This script runs the VS Code notebook automation tests using Cucumber and Playwright

Write-Host "🚀 Starting VS Code Notebook Automation Tests" -ForegroundColor Green
Write-Host "=============================================="

# Set the working directory to the NewFramework directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$frameworkPath = Split-Path -Parent $scriptPath
Set-Location $frameworkPath

# Check if VS Code is installed
$vsCodePath = Get-Command code -ErrorAction SilentlyContinue
if (-not $vsCodePath) {
    Write-Host "⚠️  Warning: VS Code command 'code' not found in PATH" -ForegroundColor Yellow
    Write-Host "   VS Code might still work if installed in default location"
}

# Check if required dependencies are installed
Write-Host "📦 Checking dependencies..."
$cucumberInstalled = npm list @cucumber/cucumber 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ @cucumber/cucumber not found. Installing dependencies..." -ForegroundColor Red
    npm install
}

$playwrightInstalled = npm list @playwright/test 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ @playwright/test not found. Installing dependencies..." -ForegroundColor Red
    npm install
}

# Create necessary directories
Write-Host "📁 Creating required directories..."
New-Item -ItemType Directory -Force -Path "Reports\screenshots" | Out-Null
New-Item -ItemType Directory -Force -Path "Reports\logs" | Out-Null
New-Item -ItemType Directory -Force -Path "TestData" | Out-Null

# Set environment variables
$env:NODE_ENV = "test"
$env:HEADLESS = "false"  # Set to "true" for headless mode

Write-Host "🧪 Running VS Code Notebook Tests..." -ForegroundColor Cyan
Write-Host ""

# Function to run a test scenario
function Run-TestScenario {
    param(
        [string]$Description,
        [string]$ScenarioLine,
        [string]$OutputFile
    )
    
    Write-Host "Running: $Description" -ForegroundColor Yellow
    
    $command = "npx cucumber-js TypeScriptTests/Features/VSCodeNotebookAutomation.feature:$ScenarioLine --require TypeScriptTests/StepDefinitions/VSCodeNotebookSteps.ts --require-module ts-node/register --format progress --format json:Reports/$OutputFile"
    
    Invoke-Expression $command
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ $Description completed successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ $Description failed" -ForegroundColor Red
    }
    Write-Host ""
}

# Run individual test scenarios
Run-TestScenario "Create and execute new notebook test" "8" "vscode-notebook-results.json"
Run-TestScenario "Open and execute existing notebook test" "18" "vscode-existing-notebook-results.json"
Run-TestScenario "Comprehensive notebook workflow test" "28" "vscode-comprehensive-results.json"
Run-TestScenario "Error handling test" "45" "vscode-error-handling-results.json"

Write-Host "✅ VS Code Notebook Automation Tests Completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Test Results:" -ForegroundColor Cyan
Write-Host "   - Screenshots: Reports\screenshots\"
Write-Host "   - Logs: Reports\logs\"
Write-Host "   - JSON Results: Reports\vscode-*-results.json"
Write-Host ""
Write-Host "🔍 To run all tests at once:" -ForegroundColor Yellow
Write-Host "   npx cucumber-js TypeScriptTests/Features/VSCodeNotebookAutomation.feature"
Write-Host ""
Write-Host "🎯 To run with specific tags:" -ForegroundColor Yellow
Write-Host "   npx cucumber-js --tags '@vscode and @notebook'"
Write-Host "   npx cucumber-js --tags '@electron'"
Write-Host "   npx cucumber-js --tags '@comprehensive'"
Write-Host ""
Write-Host "⚙️  Configuration:" -ForegroundColor Cyan
Write-Host "   - Headless mode: $($env:HEADLESS)"
Write-Host "   - Environment: $($env:NODE_ENV)"
Write-Host "   - VS Code path configured in: Config\appsettings.json"

# Pause to keep window open
Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")