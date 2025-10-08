# 📊 Generate Beautiful Allure HTML Report
# This script runs tests and generates an Allure report

param(
    [string]$Category = "",
    [switch]$Smoke,
    [switch]$Static
)

Write-Host "🚀 Starting test execution and report generation..." -ForegroundColor Cyan
Write-Host ""

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

# Clean previous results
Write-Host "🧹 Cleaning previous test results..." -ForegroundColor Blue
Remove-Item -Path "allure-results\*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "allure-report\*" -Recurse -Force -ErrorAction SilentlyContinue

# Run tests
Write-Host "🧪 Running tests..." -ForegroundColor Blue
Write-Host ""

if ($Smoke) {
    Write-Host "Running smoke tests only..."
    dotnet test --filter "Category=smoke" --logger "console;verbosity=normal"
}
elseif ($Category) {
    Write-Host "Running tests with category: $Category"
    dotnet test --filter "Category=$Category" --logger "console;verbosity=normal"
}
else {
    Write-Host "Running all tests..."
    dotnet test --logger "console;verbosity=normal"
}

Write-Host ""
Write-Host "✅ Tests completed!" -ForegroundColor Green
Write-Host ""

# Check if allure is installed
$allureInstalled = Get-Command allure -ErrorAction SilentlyContinue
if (-not $allureInstalled) {
    Write-Host "⚠️  Allure CLI is not installed!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please install Allure:"
    Write-Host "  Windows: scoop install allure"
    Write-Host "  Or download from: https://github.com/allure-framework/allure2/releases"
    Write-Host ""
    exit 1
}

# Check if results exist
if (-not (Test-Path "allure-results") -or (Get-ChildItem "allure-results" | Measure-Object).Count -eq 0) {
    Write-Host "⚠️  No test results found in allure-results directory" -ForegroundColor Yellow
    Write-Host "Tests may not have generated Allure results."
    exit 1
}

# Generate and serve report
Write-Host "📊 Generating Allure report..." -ForegroundColor Blue
Write-Host ""

if ($Static) {
    # Generate static report
    allure generate allure-results -o allure-report --clean
    Write-Host ""
    Write-Host "✅ Static report generated!" -ForegroundColor Green
    Write-Host "📂 Report location: $ScriptDir\allure-report\index.html" -ForegroundColor Blue
    Write-Host ""
    Write-Host "To view the report, run:"
    Write-Host "  start allure-report\index.html"
}
else {
    # Serve report (opens in browser)
    Write-Host "🌐 Opening report in browser..." -ForegroundColor Green
    Write-Host ""
    allure serve allure-results
}