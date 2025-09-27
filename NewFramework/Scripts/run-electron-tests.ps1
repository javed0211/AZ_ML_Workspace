# VS Code Electron Tests Runner Script (PowerShell)
# This script runs the Electron-based VS Code automation tests

param(
    [switch]$Headed,
    [switch]$Debug,
    [switch]$UI,
    [string]$Grep = "",
    [switch]$Help
)

$ErrorActionPreference = "Stop"

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ElectronTestsDir = Join-Path $ScriptDir "..\ElectronTests"
$ReportsDir = Join-Path $ScriptDir "..\Reports"

# Function to write colored output
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    } else {
        $input | Write-Output
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

# Show help if requested
if ($Help) {
    Write-Host "VS Code Electron Tests Runner" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\run-electron-tests.ps1 [OPTIONS]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -Headed      Run tests in headed mode (visible browser)"
    Write-Host "  -Debug       Run tests in debug mode"
    Write-Host "  -UI          Run tests in UI mode"
    Write-Host "  -Grep TEXT   Run only tests matching TEXT"
    Write-Host "  -Help        Show this help message"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\run-electron-tests.ps1                           # Run all tests"
    Write-Host "  .\run-electron-tests.ps1 -Headed                   # Run with visible browser"
    Write-Host "  .\run-electron-tests.ps1 -Grep 'basic functionality' # Run specific tests"
    Write-Host "  .\run-electron-tests.ps1 -Debug                    # Run in debug mode"
    exit 0
}

Write-ColorOutput Green "🚀 Running VS Code Electron Tests..."
Write-Host "📁 Test Directory: $ElectronTestsDir"
Write-Host "📊 Reports Directory: $ReportsDir"

# Check if Electron tests directory exists
if (-not (Test-Path $ElectronTestsDir)) {
    Write-ColorOutput Red "❌ Electron tests directory not found: $ElectronTestsDir"
    exit 1
}

# Navigate to Electron tests directory
Push-Location $ElectronTestsDir

try {
    # Check if dependencies are installed
    if (-not (Test-Path "node_modules")) {
        Write-Host "📦 Installing dependencies..." -ForegroundColor Cyan
        npm install
    }

    # Check if Playwright browsers are installed
    if (-not (Test-Path "node_modules\@playwright")) {
        Write-Host "🌐 Installing Playwright browsers..." -ForegroundColor Cyan
        npm run install-browsers
    }

    # Build TypeScript
    Write-Host "🔨 Building TypeScript..." -ForegroundColor Cyan
    npm run build

    # Create reports directory if it doesn't exist
    $ElectronReportsDir = Join-Path $ReportsDir "electron-test-results"
    $ScreenshotsDir = Join-Path $ReportsDir "screenshots"
    
    if (-not (Test-Path $ElectronReportsDir)) {
        New-Item -ItemType Directory -Path $ElectronReportsDir -Force | Out-Null
    }
    if (-not (Test-Path $ScreenshotsDir)) {
        New-Item -ItemType Directory -Path $ScreenshotsDir -Force | Out-Null
    }

    # Build the test command
    $TestCmd = "npx playwright test"

    if ($Headed) {
        $TestCmd += " --headed"
    }

    if ($Debug) {
        $TestCmd += " --debug"
    }

    if ($UI) {
        $TestCmd += " --ui"
    }

    if ($Grep) {
        $TestCmd += " --grep `"$Grep`""
    }

    # Run the tests
    Write-Host "🧪 Executing: $TestCmd" -ForegroundColor Cyan
    Write-Host ""

    $TestResult = Invoke-Expression $TestCmd
    $ExitCode = $LASTEXITCODE

    if ($ExitCode -eq 0) {
        Write-Host ""
        Write-ColorOutput Green "✅ Electron tests completed successfully!"
        Write-Host ""
        Write-Host "📊 Test Reports:" -ForegroundColor Yellow
        Write-Host "   HTML Report: $ElectronReportsDir\index.html"
        Write-Host "   JSON Report: $ReportsDir\electron-test-results.json"
        Write-Host "   Screenshots: $ScreenshotsDir\"
        Write-Host ""
        Write-Host "🌐 Open HTML report:" -ForegroundColor Yellow
        Write-Host "   Start-Process `"$ElectronReportsDir\index.html`""
    } else {
        Write-Host ""
        Write-ColorOutput Red "❌ Electron tests failed!"
        Write-Host ""
        Write-Host "🔍 Check the following for troubleshooting:" -ForegroundColor Yellow
        Write-Host "   • Screenshots: $ScreenshotsDir\"
        Write-Host "   • Test logs in the output above"
        Write-Host "   • VS Code installation and permissions"
        Write-Host ""
        Write-Host "💡 Try running with -Headed to see what's happening:" -ForegroundColor Yellow
        Write-Host "   .\run-electron-tests.ps1 -Headed"
        exit 1
    }

} catch {
    Write-ColorOutput Red "❌ Error running Electron tests: $($_.Exception.Message)"
    exit 1
} finally {
    # Return to original directory
    Pop-Location
}