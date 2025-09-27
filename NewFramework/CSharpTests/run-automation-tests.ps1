# Azure ML Compute Automation Tests Runner (PowerShell)
# This script runs the C# test suite for Azure ML compute automation

param(
    [string]$Category = "",
    [string]$Test = "",
    [switch]$BDD = $false,
    [switch]$Report = $false,
    [switch]$PrerequisitesOnly = $false,
    [switch]$BuildOnly = $false,
    [switch]$Help = $false
)

# Function to print colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Function to show usage
function Show-Usage {
    Write-Host "Usage: .\run-automation-tests.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Help                   Show this help message"
    Write-Host "  -Category CATEGORY      Run tests in specific category"
    Write-Host "  -Test TEST_NAME         Run specific test by name"
    Write-Host "  -BDD                    Run BDD tests only"
    Write-Host "  -Report                 Generate test report"
    Write-Host "  -PrerequisitesOnly      Check prerequisites only"
    Write-Host "  -BuildOnly              Build project only"
    Write-Host ""
    Write-Host "Categories:"
    Write-Host "  Prerequisites           Validate prerequisites"
    Write-Host "  Authentication          Azure authentication tests"
    Write-Host "  ComputeInstance         Compute instance management tests"
    Write-Host "  SSH                     SSH setup tests"
    Write-Host "  VSCode                  VS Code remote setup tests"
    Write-Host "  FileSync                File synchronization tests"
    Write-Host "  Integration             End-to-end integration tests"
    Write-Host "  Performance             Performance tests"
    Write-Host "  ErrorHandling           Error handling tests"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\run-automation-tests.ps1                           # Run all automation tests"
    Write-Host "  .\run-automation-tests.ps1 -Category Prerequisites   # Run prerequisites tests only"
    Write-Host "  .\run-automation-tests.ps1 -Test ValidatePrerequisites # Run specific test"
    Write-Host "  .\run-automation-tests.ps1 -BDD                      # Run BDD tests only"
    Write-Host "  .\run-automation-tests.ps1 -Report                   # Run tests and generate report"
    Write-Host "  .\run-automation-tests.ps1 -PrerequisitesOnly        # Check prerequisites only"
}

# Function to check prerequisites
function Test-Prerequisites {
    Write-Status "Checking prerequisites..."
    
    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-Success ".NET SDK version: $dotnetVersion"
    }
    catch {
        Write-Error ".NET SDK is not installed. Please install .NET 9.0 SDK."
        exit 1
    }
    
    # Check Azure CLI
    try {
        $azVersion = az --version | Select-Object -First 1
        Write-Success "Azure CLI: $azVersion"
    }
    catch {
        Write-Warning "Azure CLI is not installed. Some tests may fail."
        Write-Status "Install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    }
    
    # Check VS Code
    try {
        $null = Get-Command code -ErrorAction Stop
        Write-Success "VS Code is installed"
    }
    catch {
        Write-Warning "VS Code is not installed. VS Code-related tests may fail."
    }
    
    # Check Python
    try {
        $pythonVersion = python --version 2>$null
        if ($pythonVersion) {
            Write-Success "Python: $pythonVersion"
        } else {
            $python3Version = python3 --version 2>$null
            if ($python3Version) {
                Write-Success "Python: $python3Version"
            } else {
                Write-Warning "Python is not installed. Some automation features may not work."
            }
        }
    }
    catch {
        Write-Warning "Python is not installed. Some automation features may not work."
    }
}

# Function to restore NuGet packages
function Restore-Packages {
    Write-Status "Restoring NuGet packages..."
    $result = dotnet restore
    if ($LASTEXITCODE -eq 0) {
        Write-Success "NuGet packages restored successfully"
    } else {
        Write-Error "Failed to restore NuGet packages"
        exit 1
    }
}

# Function to build the project
function Build-Project {
    Write-Status "Building the project..."
    $result = dotnet build --configuration Release --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Project built successfully"
    } else {
        Write-Error "Failed to build project"
        exit 1
    }
}

# Function to run specific test categories
function Invoke-Tests {
    param(
        [string]$Category,
        [string]$TestName
    )
    
    if ($Category) {
        Write-Status "Running tests in category: $Category"
        dotnet test --configuration Release --no-build --logger "console;verbosity=detailed" --filter "Category=$Category"
    } elseif ($TestName) {
        Write-Status "Running specific test: $TestName"
        dotnet test --configuration Release --no-build --logger "console;verbosity=detailed" --filter "Name~$TestName"
    } else {
        Write-Status "Running all Azure ML Compute Automation tests..."
        dotnet test --configuration Release --no-build --logger "console;verbosity=detailed" --filter "Category=AzureMLComputeAutomation"
    }
}

# Function to run BDD tests
function Invoke-BDDTests {
    Write-Status "Running BDD tests for Azure ML Compute Automation..."
    dotnet test --configuration Release --no-build --logger "console;verbosity=detailed" --filter "TestCategory=Reqnroll"
}

# Function to generate test report
function New-TestReport {
    Write-Status "Generating test report..."
    
    # Create TestResults directory if it doesn't exist
    if (!(Test-Path "./TestResults")) {
        New-Item -ItemType Directory -Path "./TestResults" -Force | Out-Null
    }
    
    dotnet test --configuration Release --no-build --logger "trx;LogFileName=azure-ml-automation-tests.trx" --results-directory "./TestResults"
    
    if (Test-Path "./TestResults/azure-ml-automation-tests.trx") {
        Write-Success "Test report generated: ./TestResults/azure-ml-automation-tests.trx"
    }
}

# Main execution
function Main {
    # Show help if requested
    if ($Help) {
        Show-Usage
        exit 0
    }
    
    # Print header
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host "🚀 Azure ML Compute Automation Tests" -ForegroundColor Cyan
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Check prerequisites
    Test-Prerequisites
    
    if ($PrerequisitesOnly) {
        Write-Success "Prerequisites check completed"
        exit 0
    }
    
    Write-Host ""
    
    # Restore packages and build
    Restore-Packages
    Write-Host ""
    
    Build-Project
    
    if ($BuildOnly) {
        Write-Success "Build completed"
        exit 0
    }
    
    Write-Host ""
    
    # Create logs directory
    if (!(Test-Path "./logs")) {
        New-Item -ItemType Directory -Path "./logs" -Force | Out-Null
    }
    
    # Run tests
    if ($BDD) {
        Invoke-BDDTests
    } else {
        Invoke-Tests -Category $Category -TestName $Test
    }
    
    # Generate report if requested
    if ($Report) {
        Write-Host ""
        New-TestReport
    }
    
    Write-Host ""
    Write-Success "Test execution completed!"
    Write-Host ""
    Write-Host "📋 Test Results:" -ForegroundColor Cyan
    Write-Host "   - Console output above shows detailed results"
    Write-Host "   - Logs are available in: ./logs/"
    if ($Report) {
        Write-Host "   - Test report: ./TestResults/azure-ml-automation-tests.trx"
    }
    Write-Host ""
    Write-Host "🔧 Troubleshooting:" -ForegroundColor Cyan
    Write-Host "   - Ensure Azure CLI is authenticated: az login"
    Write-Host "   - Check configuration in appsettings.json"
    Write-Host "   - Verify network connectivity to Azure services"
    Write-Host "   - Review logs for detailed error information"
}

# Execute main function
Main