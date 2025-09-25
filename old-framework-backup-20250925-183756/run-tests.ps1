# PowerShell script for running Azure ML tests on Windows
param(
    [string]$Category = "all",
    [string]$Browser = "chromium",
    [switch]$Headless = $true,
    [switch]$Parallel = $false,
    [switch]$Coverage = $false,
    [string]$Environment = "test",
    [string]$OutputPath = "TestResults",
    [int]$MaxRetries = 3,
    [switch]$GenerateReport = $true,
    [switch]$OpenReport = $false,
    [switch]$Help
)

# Display help information
if ($Help) {
    Write-Host "Azure ML Test Automation Runner (PowerShell)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\run-tests.ps1 [OPTIONS]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -Category <string>     Test category to run (all, UI, API, BDD, Smoke, Performance)" -ForegroundColor White
    Write-Host "  -Browser <string>      Browser for UI tests (chromium, firefox, webkit, all)" -ForegroundColor White
    Write-Host "  -Headless              Run browser in headless mode (default: true)" -ForegroundColor White
    Write-Host "  -Parallel              Enable parallel test execution" -ForegroundColor White
    Write-Host "  -Coverage              Generate code coverage report" -ForegroundColor White
    Write-Host "  -Environment <string>  Target environment (test, staging, production)" -ForegroundColor White
    Write-Host "  -OutputPath <string>   Output directory for test results" -ForegroundColor White
    Write-Host "  -MaxRetries <int>      Maximum retry attempts for failed tests" -ForegroundColor White
    Write-Host "  -GenerateReport        Generate HTML test report (default: true)" -ForegroundColor White
    Write-Host "  -OpenReport            Open test report after execution" -ForegroundColor White
    Write-Host "  -Help                  Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\run-tests.ps1 -Category UI -Browser firefox -Headless:$false" -ForegroundColor Gray
    Write-Host "  .\run-tests.ps1 -Category API -Coverage -Parallel" -ForegroundColor Gray
    Write-Host "  .\run-tests.ps1 -Category BDD -Environment staging" -ForegroundColor Gray
    exit 0
}

# Set error action preference
$ErrorActionPreference = "Stop"

# Colors for output
$Green = "Green"
$Yellow = "Yellow"
$Red = "Red"
$Blue = "Blue"
$Gray = "Gray"

Write-Host "🚀 Azure ML Test Automation Framework" -ForegroundColor $Green
Write-Host "=====================================" -ForegroundColor $Green
Write-Host ""

# Validate parameters
$validCategories = @("all", "UI", "API", "BDD", "Smoke", "Performance", "Security")
$validBrowsers = @("chromium", "firefox", "webkit", "all")
$validEnvironments = @("test", "staging", "production")

if ($validCategories -notcontains $Category) {
    Write-Host "❌ Invalid category: $Category" -ForegroundColor $Red
    Write-Host "Valid categories: $($validCategories -join ', ')" -ForegroundColor $Gray
    exit 1
}

if ($validBrowsers -notcontains $Browser) {
    Write-Host "❌ Invalid browser: $Browser" -ForegroundColor $Red
    Write-Host "Valid browsers: $($validBrowsers -join ', ')" -ForegroundColor $Gray
    exit 1
}

if ($validEnvironments -notcontains $Environment) {
    Write-Host "❌ Invalid environment: $Environment" -ForegroundColor $Red
    Write-Host "Valid environments: $($validEnvironments -join ', ')" -ForegroundColor $Gray
    exit 1
}

# Display configuration
Write-Host "Configuration:" -ForegroundColor $Yellow
Write-Host "  Category: $Category" -ForegroundColor $Gray
Write-Host "  Browser: $Browser" -ForegroundColor $Gray
Write-Host "  Headless: $Headless" -ForegroundColor $Gray
Write-Host "  Parallel: $Parallel" -ForegroundColor $Gray
Write-Host "  Coverage: $Coverage" -ForegroundColor $Gray
Write-Host "  Environment: $Environment" -ForegroundColor $Gray
Write-Host "  Output Path: $OutputPath" -ForegroundColor $Gray
Write-Host ""

# Check prerequisites
Write-Host "🔍 Checking prerequisites..." -ForegroundColor $Blue

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK: $dotnetVersion" -ForegroundColor $Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8.0 SDK." -ForegroundColor $Red
    exit 1
}

# Check if project exists
$projectPath = "AzureMLWorkspace.Tests\AzureMLWorkspace.Tests.csproj"
if (-not (Test-Path $projectPath)) {
    Write-Host "❌ Project file not found: $projectPath" -ForegroundColor $Red
    exit 1
}

Write-Host "✅ Project file found" -ForegroundColor $Green

# Create output directory
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Write-Host "✅ Created output directory: $OutputPath" -ForegroundColor $Green
}

# Restore packages
Write-Host ""
Write-Host "📦 Restoring packages..." -ForegroundColor $Blue
try {
    dotnet restore $projectPath
    Write-Host "✅ Packages restored successfully" -ForegroundColor $Green
} catch {
    Write-Host "❌ Failed to restore packages" -ForegroundColor $Red
    exit 1
}

# Build solution
Write-Host ""
Write-Host "🔨 Building solution..." -ForegroundColor $Blue
try {
    dotnet build $projectPath --configuration Release --no-restore
    Write-Host "✅ Build completed successfully" -ForegroundColor $Green
} catch {
    Write-Host "❌ Build failed" -ForegroundColor $Red
    exit 1
}

# Install Playwright browsers
Write-Host ""
Write-Host "🌐 Installing Playwright browsers..." -ForegroundColor $Blue
try {
    if ($Browser -eq "all") {
        dotnet run --project $projectPath -- playwright install --with-deps
    } else {
        dotnet run --project $projectPath -- playwright install --with-deps $Browser
    }
    Write-Host "✅ Playwright browsers installed" -ForegroundColor $Green
} catch {
    Write-Host "❌ Failed to install Playwright browsers" -ForegroundColor $Red
    exit 1
}

# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:BROWSER_TYPE = $Browser
$env:HEADLESS_MODE = $Headless.ToString().ToLower()

# Build test filter
$testFilter = ""
switch ($Category) {
    "all" { $testFilter = "" }
    "UI" { $testFilter = "Category=UI" }
    "API" { $testFilter = "Category=API" }
    "BDD" { $testFilter = "Category=BDD" }
    "Smoke" { $testFilter = "Category=Smoke" }
    "Performance" { $testFilter = "Category=Performance" }
    "Security" { $testFilter = "Category=Security" }
}

# Build test arguments
$testArgs = @(
    "--configuration", "Release",
    "--no-build",
    "--logger", "trx",
    "--logger", "console;verbosity=detailed",
    "--results-directory", $OutputPath
)

if ($Coverage) {
    $testArgs += "--collect:XPlat Code Coverage"
}

if ($testFilter) {
    $testArgs += "--filter", $testFilter
}

if ($Parallel) {
    $testArgs += "--parallel"
}

# Run tests
Write-Host ""
Write-Host "🧪 Running tests..." -ForegroundColor $Blue
Write-Host "Filter: $($testFilter -or 'All tests')" -ForegroundColor $Gray

$testStartTime = Get-Date

try {
    if ($Browser -eq "all" -and ($Category -eq "UI" -or $Category -eq "all")) {
        # Run tests for each browser
        $browsers = @("chromium", "firefox", "webkit")
        foreach ($browserType in $browsers) {
            Write-Host ""
            Write-Host "🌐 Running tests with $browserType..." -ForegroundColor $Yellow
            $env:BROWSER_TYPE = $browserType
            
            $browserOutputPath = Join-Path $OutputPath $browserType
            if (-not (Test-Path $browserOutputPath)) {
                New-Item -ItemType Directory -Path $browserOutputPath -Force | Out-Null
            }
            
            $browserTestArgs = $testArgs.Clone()
            $browserTestArgs[($browserTestArgs.IndexOf("--results-directory") + 1)] = $browserOutputPath
            
            dotnet test $projectPath @browserTestArgs
        }
    } else {
        dotnet test $projectPath @testArgs
    }
    
    $testEndTime = Get-Date
    $testDuration = $testEndTime - $testStartTime
    
    Write-Host ""
    Write-Host "✅ Tests completed successfully in $($testDuration.ToString('mm\:ss'))" -ForegroundColor $Green
} catch {
    Write-Host ""
    Write-Host "❌ Tests failed" -ForegroundColor $Red
    exit 1
}

# Generate coverage report
if ($Coverage) {
    Write-Host ""
    Write-Host "📊 Generating coverage report..." -ForegroundColor $Blue
    
    try {
        # Install ReportGenerator if not already installed
        dotnet tool install -g dotnet-reportgenerator-globaltool 2>$null
        
        $coverageReportPath = Join-Path $OutputPath "CoverageReport"
        reportgenerator `
            -reports:"$OutputPath/**/coverage.cobertura.xml" `
            -targetdir:$coverageReportPath `
            -reporttypes:"Html;JsonSummary"
            
        Write-Host "✅ Coverage report generated: $coverageReportPath" -ForegroundColor $Green
        
        if ($OpenReport) {
            $reportFile = Join-Path $coverageReportPath "index.html"
            if (Test-Path $reportFile) {
                Start-Process $reportFile
            }
        }
    } catch {
        Write-Host "⚠️  Failed to generate coverage report" -ForegroundColor $Yellow
    }
}

# Generate HTML test report
if ($GenerateReport) {
    Write-Host ""
    Write-Host "📋 Generating test report..." -ForegroundColor $Blue
    
    try {
        # Install dotnet-trx2html if not already installed
        dotnet tool install -g dotnet-trx2html 2>$null
        
        $trxFiles = Get-ChildItem -Path $OutputPath -Filter "*.trx" -Recurse
        foreach ($trxFile in $trxFiles) {
            $htmlReportPath = $trxFile.FullName -replace "\.trx$", ".html"
            trx2html $trxFile.FullName $htmlReportPath
        }
        
        Write-Host "✅ Test reports generated" -ForegroundColor $Green
        
        if ($OpenReport -and $trxFiles.Count -gt 0) {
            $firstHtmlReport = $trxFiles[0].FullName -replace "\.trx$", ".html"
            if (Test-Path $firstHtmlReport) {
                Start-Process $firstHtmlReport
            }
        }
    } catch {
        Write-Host "⚠️  Failed to generate HTML test report" -ForegroundColor $Yellow
    }
}

# Summary
Write-Host ""
Write-Host "🎉 Test execution completed!" -ForegroundColor $Green
Write-Host "Results available in: $OutputPath" -ForegroundColor $Gray

# List generated artifacts
$artifacts = Get-ChildItem -Path $OutputPath -Recurse | Where-Object { -not $_.PSIsContainer }
if ($artifacts.Count -gt 0) {
    Write-Host ""
    Write-Host "Generated artifacts:" -ForegroundColor $Yellow
    foreach ($artifact in $artifacts | Select-Object -First 10) {
        Write-Host "  $($artifact.FullName)" -ForegroundColor $Gray
    }
    if ($artifacts.Count -gt 10) {
        Write-Host "  ... and $($artifacts.Count - 10) more files" -ForegroundColor $Gray
    }
}