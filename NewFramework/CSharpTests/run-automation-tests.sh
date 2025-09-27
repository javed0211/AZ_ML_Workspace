#!/bin/bash

# Azure ML Compute Automation Tests Runner
# This script runs the C# test suite for Azure ML compute automation

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed. Please install .NET 9.0 SDK."
        exit 1
    fi
    
    local dotnet_version=$(dotnet --version)
    print_success ".NET SDK version: $dotnet_version"
    
    # Check Azure CLI
    if ! command -v az &> /dev/null; then
        print_warning "Azure CLI is not installed. Some tests may fail."
        print_status "Install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    else
        local az_version=$(az --version | head -n 1)
        print_success "Azure CLI: $az_version"
    fi
    
    # Check VS Code
    if ! command -v code &> /dev/null; then
        print_warning "VS Code is not installed. VS Code-related tests may fail."
    else
        print_success "VS Code is installed"
    fi
    
    # Check Python
    if command -v python3 &> /dev/null; then
        local python_version=$(python3 --version)
        print_success "Python: $python_version"
    elif command -v python &> /dev/null; then
        local python_version=$(python --version)
        print_success "Python: $python_version"
    else
        print_warning "Python is not installed. Some automation features may not work."
    fi
}

# Function to restore NuGet packages
restore_packages() {
    print_status "Restoring NuGet packages..."
    dotnet restore
    if [ $? -eq 0 ]; then
        print_success "NuGet packages restored successfully"
    else
        print_error "Failed to restore NuGet packages"
        exit 1
    fi
}

# Function to build the project
build_project() {
    print_status "Building the project..."
    dotnet build --configuration Release --no-restore
    if [ $? -eq 0 ]; then
        print_success "Project built successfully"
    else
        print_error "Failed to build project"
        exit 1
    fi
}

# Function to run specific test categories
run_tests() {
    local category=$1
    local test_name=$2
    
    if [ -n "$category" ]; then
        print_status "Running tests in category: $category"
        dotnet test --configuration Release --no-build --logger "console;verbosity=detailed" --filter "Category=$category"
    elif [ -n "$test_name" ]; then
        print_status "Running specific test: $test_name"
        dotnet test --configuration Release --no-build --logger "console;verbosity=detailed" --filter "Name~$test_name"
    else
        print_status "Running all Azure ML Compute Automation tests..."
        dotnet test --configuration Release --no-build --logger "console;verbosity=detailed" --filter "Category=AzureMLComputeAutomation"
    fi
}

# Function to run BDD tests
run_bdd_tests() {
    print_status "Running BDD tests for Azure ML Compute Automation..."
    dotnet test --configuration Release --no-build --logger "console;verbosity=detailed" --filter "TestCategory=Reqnroll"
}

# Function to generate test report
generate_report() {
    print_status "Generating test report..."
    dotnet test --configuration Release --no-build --logger "trx;LogFileName=azure-ml-automation-tests.trx" --results-directory "./TestResults"
    
    if [ -f "./TestResults/azure-ml-automation-tests.trx" ]; then
        print_success "Test report generated: ./TestResults/azure-ml-automation-tests.trx"
    fi
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -h, --help              Show this help message"
    echo "  -c, --category CATEGORY Run tests in specific category"
    echo "  -t, --test TEST_NAME    Run specific test by name"
    echo "  -b, --bdd               Run BDD tests only"
    echo "  -r, --report            Generate test report"
    echo "  --prerequisites-only    Check prerequisites only"
    echo "  --build-only            Build project only"
    echo ""
    echo "Categories:"
    echo "  Prerequisites           Validate prerequisites"
    echo "  Authentication          Azure authentication tests"
    echo "  ComputeInstance         Compute instance management tests"
    echo "  SSH                     SSH setup tests"
    echo "  VSCode                  VS Code remote setup tests"
    echo "  FileSync                File synchronization tests"
    echo "  Integration             End-to-end integration tests"
    echo "  Performance             Performance tests"
    echo "  ErrorHandling           Error handling tests"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Run all automation tests"
    echo "  $0 -c Prerequisites                  # Run prerequisites tests only"
    echo "  $0 -t ValidatePrerequisites          # Run specific test"
    echo "  $0 -b                                # Run BDD tests only"
    echo "  $0 -r                                # Run tests and generate report"
    echo "  $0 --prerequisites-only              # Check prerequisites only"
}

# Main execution
main() {
    local category=""
    local test_name=""
    local run_bdd=false
    local generate_report_flag=false
    local prerequisites_only=false
    local build_only=false
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--help)
                show_usage
                exit 0
                ;;
            -c|--category)
                category="$2"
                shift 2
                ;;
            -t|--test)
                test_name="$2"
                shift 2
                ;;
            -b|--bdd)
                run_bdd=true
                shift
                ;;
            -r|--report)
                generate_report_flag=true
                shift
                ;;
            --prerequisites-only)
                prerequisites_only=true
                shift
                ;;
            --build-only)
                build_only=true
                shift
                ;;
            *)
                print_error "Unknown option: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Print header
    echo "=================================================="
    echo "🚀 Azure ML Compute Automation Tests"
    echo "=================================================="
    echo ""
    
    # Check prerequisites
    check_prerequisites
    
    if [ "$prerequisites_only" = true ]; then
        print_success "Prerequisites check completed"
        exit 0
    fi
    
    echo ""
    
    # Restore packages and build
    restore_packages
    echo ""
    
    build_project
    
    if [ "$build_only" = true ]; then
        print_success "Build completed"
        exit 0
    fi
    
    echo ""
    
    # Create logs directory
    mkdir -p logs
    
    # Run tests
    if [ "$run_bdd" = true ]; then
        run_bdd_tests
    else
        run_tests "$category" "$test_name"
    fi
    
    # Generate report if requested
    if [ "$generate_report_flag" = true ]; then
        echo ""
        generate_report
    fi
    
    echo ""
    print_success "Test execution completed!"
    echo ""
    echo "📋 Test Results:"
    echo "   - Console output above shows detailed results"
    echo "   - Logs are available in: ./logs/"
    if [ "$generate_report_flag" = true ]; then
        echo "   - Test report: ./TestResults/azure-ml-automation-tests.trx"
    fi
    echo ""
    echo "🔧 Troubleshooting:"
    echo "   - Ensure Azure CLI is authenticated: az login"
    echo "   - Check configuration in appsettings.json"
    echo "   - Verify network connectivity to Azure services"
    echo "   - Review logs for detailed error information"
}

# Run main function with all arguments
main "$@"