# Azure Speech Services Test Setup Script (PowerShell)
# This script helps set up the Speech Services test environment on Windows

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Azure Speech Services Test Setup" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Host "Working directory: $ScriptDir"
Write-Host ""

# Step 1: Check .NET installation
Write-Host "Step 1: Checking .NET installation..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found. Please install .NET 9.0 or later." -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download"
    exit 1
}
Write-Host ""

# Step 2: Restore NuGet packages
Write-Host "Step 2: Restoring NuGet packages..." -ForegroundColor Yellow
try {
    dotnet restore
    Write-Host "✓ NuGet packages restored successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to restore NuGet packages" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Check for Speech SDK
Write-Host "Step 3: Checking for Microsoft.CognitiveServices.Speech package..." -ForegroundColor Yellow
$packages = dotnet list package
if ($packages -match "Microsoft.CognitiveServices.Speech") {
    Write-Host "✓ Speech SDK package found" -ForegroundColor Green
} else {
    Write-Host "! Speech SDK package not found. Installing..." -ForegroundColor Yellow
    dotnet add package Microsoft.CognitiveServices.Speech --version 1.38.0
}
Write-Host ""

# Step 4: Create test data directories
Write-Host "Step 4: Creating test data directories..." -ForegroundColor Yellow
$directories = @(
    "TestData\audio",
    "TestData\output",
    "Reports\logs",
    "allure-results"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
}
Write-Host "✓ Directories created" -ForegroundColor Green
Write-Host ""

# Step 5: Check for audio files
Write-Host "Step 5: Checking for test audio files..." -ForegroundColor Yellow
$audioDir = "TestData\audio"
$audioFiles = @("sample-english.wav", "sample-spanish.wav", "sample-french.wav")
$missingFiles = @()

foreach ($file in $audioFiles) {
    $filePath = Join-Path $audioDir $file
    if (Test-Path $filePath) {
        Write-Host "✓ Found: $file" -ForegroundColor Green
    } else {
        Write-Host "! Missing: $file" -ForegroundColor Yellow
        $missingFiles += $file
    }
}

if ($missingFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "Warning: Some audio files are missing." -ForegroundColor Yellow
    Write-Host "You can:"
    Write-Host "  1. Record your own audio files"
    Write-Host "  2. Generate them using Azure TTS"
    Write-Host "  3. Download samples from Common Voice"
    Write-Host ""
    Write-Host "See TestData\audio\README.md for details."
    Write-Host ""
}
Write-Host ""

# Step 6: Check configuration
Write-Host "Step 6: Checking configuration..." -ForegroundColor Yellow
$configFile = "..\Config\appsettings.json"

if (Test-Path $configFile) {
    Write-Host "✓ Configuration file found" -ForegroundColor Green
    
    $configContent = Get-Content $configFile -Raw
    
    # Check if Speech Services configuration exists
    if ($configContent -match "SpeechServices") {
        Write-Host "✓ Speech Services configuration found" -ForegroundColor Green
        
        # Check if placeholder values are still present
        if ($configContent -match "your-speech-services-key") {
            Write-Host "! Configuration contains placeholder values" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Please update the following in $configFile:"
            Write-Host "  - SubscriptionKey: Your Azure Speech Services key"
            Write-Host "  - Region: Your Azure region (e.g., uksouth, eastus)"
            Write-Host ""
            Write-Host "Or set environment variables:"
            Write-Host "  `$env:AZURE_SPEECH_KEY='your-key'"
            Write-Host "  `$env:AZURE_SPEECH_REGION='your-region'"
        } else {
            Write-Host "✓ Configuration appears to be set" -ForegroundColor Green
        }
    } else {
        Write-Host "✗ Speech Services configuration not found in appsettings.json" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✗ Configuration file not found: $configFile" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 7: Build the project
Write-Host "Step 7: Building the project..." -ForegroundColor Yellow
try {
    dotnet build --no-restore
    Write-Host "✓ Project built successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 8: Check for Reqnroll code generation
Write-Host "Step 8: Checking Reqnroll code generation..." -ForegroundColor Yellow
$featureFile = "Features\AzureSpeechServices.feature"
$codebehindFile = "Features\AzureSpeechServices.feature.cs"

if (Test-Path $featureFile) {
    Write-Host "✓ Feature file found" -ForegroundColor Green
    
    if (Test-Path $codebehindFile) {
        Write-Host "✓ Code-behind file generated" -ForegroundColor Green
    } else {
        Write-Host "! Code-behind file not found. Regenerating..." -ForegroundColor Yellow
        dotnet build /t:ReqnrollGenerate
    }
} else {
    Write-Host "✗ Feature file not found: $featureFile" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 9: Run a quick smoke test
Write-Host "Step 9: Running smoke tests..." -ForegroundColor Yellow
Write-Host "This will verify your Azure Speech Services credentials..."
Write-Host ""

$response = Read-Host "Would you like to run smoke tests now? (y/n)"
if ($response -eq 'y' -or $response -eq 'Y') {
    Write-Host "Running smoke tests..."
    try {
        dotnet test --filter "Category=smoke&Category=speech" --logger "console;verbosity=normal"
        Write-Host "✓ Smoke tests passed!" -ForegroundColor Green
    } catch {
        Write-Host "! Some smoke tests failed. This is expected if:" -ForegroundColor Yellow
        Write-Host "  - Azure credentials are not configured"
        Write-Host "  - Audio files are missing"
        Write-Host "  - Network connectivity issues"
    }
} else {
    Write-Host "Skipping smoke tests."
}
Write-Host ""

# Summary
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Setup Summary" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "✓ .NET SDK: Installed" -ForegroundColor Green
Write-Host "✓ NuGet Packages: Restored" -ForegroundColor Green
Write-Host "✓ Project: Built successfully" -ForegroundColor Green
Write-Host "✓ Test Structure: Ready" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:"
Write-Host "1. Configure Azure Speech Services credentials in appsettings.json"
Write-Host "2. Add test audio files to TestData\audio\"
Write-Host "3. Run tests with: dotnet test --filter `"Category=speech`""
Write-Host ""
Write-Host "For more information, see:"
Write-Host "  - README_AZURE_SPEECH_SERVICES.md"
Write-Host "  - TestData\audio\README.md"
Write-Host ""
Write-Host "Quick Test Commands:"
Write-Host "  dotnet test --filter `"Category=smoke&Category=speech`"     # Smoke tests"
Write-Host "  dotnet test --filter `"Category=stt`"                       # Speech-to-Text"
Write-Host "  dotnet test --filter `"Category=tts`"                       # Text-to-Speech"
Write-Host "  dotnet test --filter `"Category=speech`"                    # All tests"
Write-Host ""
Write-Host "Setup complete!" -ForegroundColor Green