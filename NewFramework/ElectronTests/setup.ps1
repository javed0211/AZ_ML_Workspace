# VS Code Electron Testing Framework Setup Script (PowerShell)
# This script sets up the testing environment for VS Code automation on Windows

param(
    [switch]$SkipVSCodeCheck,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Setting up VS Code Electron Testing Framework..." -ForegroundColor Green

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

# Check if Node.js is installed
try {
    $nodeVersion = node --version
    $nodeVersionNumber = [int]($nodeVersion -replace 'v(\d+)\..*', '$1')
    
    if ($nodeVersionNumber -lt 16) {
        Write-ColorOutput Red "❌ Node.js version 16 or higher is required. Current version: $nodeVersion"
        Write-Host "   Visit: https://nodejs.org/" -ForegroundColor Yellow
        exit 1
    }
    
    Write-ColorOutput Green "✅ Node.js version: $nodeVersion"
} catch {
    Write-ColorOutput Red "❌ Node.js is not installed. Please install Node.js 16 or higher."
    Write-Host "   Visit: https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

# Check if VS Code is installed (unless skipped)
if (-not $SkipVSCodeCheck) {
    $vscodePaths = @(
        "${env:ProgramFiles}\Microsoft VS Code\Code.exe",
        "${env:ProgramFiles(x86)}\Microsoft VS Code\Code.exe",
        "$env:LOCALAPPDATA\Programs\Microsoft VS Code\Code.exe"
    )
    
    $vscodeFound = $false
    $vscodePath = ""
    
    foreach ($path in $vscodePaths) {
        if (Test-Path $path) {
            $vscodeFound = $true
            $vscodePath = $path
            break
        }
    }
    
    if (-not $vscodeFound) {
        Write-ColorOutput Red "❌ VS Code not found in standard locations."
        Write-Host "   Please install VS Code from: https://code.visualstudio.com/" -ForegroundColor Yellow
        Write-Host "   Or use -SkipVSCodeCheck to skip this check" -ForegroundColor Yellow
        exit 1
    }
    
    Write-ColorOutput Green "✅ VS Code found at: $vscodePath"
}

# Install npm dependencies
Write-Host "📦 Installing npm dependencies..." -ForegroundColor Cyan
try {
    npm install
    Write-ColorOutput Green "✅ npm dependencies installed"
} catch {
    Write-ColorOutput Red "❌ Failed to install npm dependencies"
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Install Playwright browsers
Write-Host "🌐 Installing Playwright browsers..." -ForegroundColor Cyan
try {
    npm run install-browsers
    Write-ColorOutput Green "✅ Playwright browsers installed"
} catch {
    Write-ColorOutput Red "❌ Failed to install Playwright browsers"
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Build TypeScript
Write-Host "🔨 Building TypeScript..." -ForegroundColor Cyan
try {
    npm run build
    Write-ColorOutput Green "✅ TypeScript built successfully"
} catch {
    Write-ColorOutput Red "❌ Failed to build TypeScript"
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Create temp directory
if (-not (Test-Path "temp")) {
    New-Item -ItemType Directory -Path "temp" | Out-Null
}
Write-ColorOutput Green "✅ Temporary directory created"

# Create reports directories
$reportsPath = "..\Reports\electron-test-results"
$screenshotsPath = "..\Reports\screenshots"

if (-not (Test-Path $reportsPath)) {
    New-Item -ItemType Directory -Path $reportsPath -Force | Out-Null
}
if (-not (Test-Path $screenshotsPath)) {
    New-Item -ItemType Directory -Path $screenshotsPath -Force | Out-Null
}
Write-ColorOutput Green "✅ Reports directories created"

# Run a quick test to verify setup
Write-Host "🧪 Running setup verification test..." -ForegroundColor Cyan
try {
    npx playwright test --grep "should launch VS Code successfully" --reporter=line
    
    Write-Host ""
    Write-ColorOutput Green "🎉 Setup completed successfully!"
    Write-Host ""
    Write-Host "📋 Next steps:" -ForegroundColor Yellow
    Write-Host "   • Run all tests: npm test"
    Write-Host "   • Run tests in headed mode: npm run test:headed"
    Write-Host "   • Run tests with UI: npm run test:ui"
    Write-Host "   • View test reports in: ..\Reports\electron-test-results\"
    Write-Host ""
    Write-Host "📖 Documentation: See README.md for detailed usage instructions" -ForegroundColor Yellow
    
} catch {
    Write-Host ""
    Write-ColorOutput Yellow "⚠️  Setup completed but verification test failed."
    Write-Host "   This might be due to VS Code configuration or system-specific issues."
    Write-Host "   Try running tests manually: npm test"
    Write-Host ""
    Write-Host "🔧 Troubleshooting:" -ForegroundColor Yellow
    Write-Host "   • Check VS Code installation path in config\vscode-config.ts"
    Write-Host "   • Ensure VS Code can be launched manually"
    Write-Host "   • Check system permissions"
    Write-Host "   • Run as Administrator if needed"
}

# Display system information
Write-Host ""
Write-Host "🔍 System Information:" -ForegroundColor Cyan
Write-Host "   OS: $($PSVersionTable.PSVersion)"
Write-Host "   Node.js: $(node --version)"
Write-Host "   npm: $(npm --version)"
if ($vscodePath) {
    Write-Host "   VS Code: $vscodePath"
}
Write-Host ""

if ($Verbose) {
    Write-Host "📁 Directory Structure:" -ForegroundColor Cyan
    Get-ChildItem -Recurse -Depth 2 | Where-Object { $_.PSIsContainer } | ForEach-Object {
        Write-Host "   $($_.FullName.Replace($PWD.Path, '.'))"
    }
}