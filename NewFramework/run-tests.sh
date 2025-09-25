#!/bin/bash

# Test Automation Framework Runner Script
# Usage: ./run-tests.sh [options]

set -e

# Default values
ENVIRONMENT="dev"
BROWSER="chromium"
HEADED="false"
PARALLEL="false"
LANGUAGE="typescript"
TEST_PATTERN=""
WORKERS=1

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
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

# Function to show usage
show_usage() {
    echo "Test Automation Framework Runner"
    echo ""
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -e, --env ENVIRONMENT     Set test environment (dev, qa, prod) [default: dev]"
    echo "  -b, --browser BROWSER     Set browser (chromium, firefox, webkit) [default: chromium]"
    echo "  -h, --headed              Run tests in headed mode [default: headless]"
    echo "  -p, --parallel            Run tests in parallel [default: sequential]"
    echo "  -w, --workers NUMBER      Number of parallel workers [default: 1]"
    echo "  -l, --language LANG       Test language (typescript, csharp, both) [default: typescript]"
    echo "  -t, --test PATTERN        Test pattern to run [default: all tests]"
    echo "  --electron                Run Electron tests only"
    echo "  --install                 Install dependencies and browsers"
    echo "  --report                  Open test report after execution"
    echo "  --help                    Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 --env qa --browser firefox --headed"
    echo "  $0 --parallel --workers 4 --language csharp"
    echo "  $0 --electron --headed"
    echo "  $0 --test \"login\" --env dev"
}

# Function to install dependencies
install_dependencies() {
    print_info "Installing dependencies..."
    
    # Install Node.js dependencies
    if [ -f "package.json" ]; then
        print_info "Installing Node.js dependencies..."
        npm install
        print_info "Installing Playwright browsers..."
        npx playwright install
        npx playwright install-deps
    fi
    
    # Install .NET dependencies
    if [ -f "CSharpTests/PlaywrightFramework.csproj" ]; then
        print_info "Restoring .NET packages..."
        cd CSharpTests
        dotnet restore
        print_info "Installing Playwright for .NET..."
        pwsh bin/Debug/net8.0/playwright.ps1 install
        cd ..
    fi
    
    print_success "Dependencies installed successfully!"
}

# Function to run TypeScript tests
run_typescript_tests() {
    print_info "Running TypeScript tests..."
    
    export TEST_ENV=$ENVIRONMENT
    
    local cmd="npx playwright test"
    
    if [ "$BROWSER" != "chromium" ]; then
        cmd="$cmd --project=$BROWSER"
    fi
    
    if [ "$HEADED" = "true" ]; then
        cmd="$cmd --headed"
    fi
    
    if [ "$PARALLEL" = "true" ]; then
        cmd="$cmd --workers=$WORKERS"
    fi
    
    if [ "$TEST_PATTERN" != "" ]; then
        cmd="$cmd --grep=\"$TEST_PATTERN\""
    fi
    
    if [ "$ELECTRON_ONLY" = "true" ]; then
        cmd="$cmd --grep=\"@electron\""
    fi
    
    print_info "Executing: $cmd"
    eval $cmd
}

# Function to run C# tests
run_csharp_tests() {
    print_info "Running C# tests..."
    
    export TEST_ENV=$ENVIRONMENT
    
    cd CSharpTests
    
    local cmd="dotnet test"
    
    if [ "$TEST_PATTERN" != "" ]; then
        cmd="$cmd --filter \"Name~$TEST_PATTERN\""
    fi
    
    cmd="$cmd --logger:html --results-directory:../Reports/csharp-results"
    
    print_info "Executing: $cmd"
    eval $cmd
    
    cd ..
}

# Function to open test report
open_report() {
    print_info "Opening test report..."
    
    if [ -f "Reports/html-report/index.html" ]; then
        if command -v open &> /dev/null; then
            open Reports/html-report/index.html
        elif command -v xdg-open &> /dev/null; then
            xdg-open Reports/html-report/index.html
        else
            print_warning "Cannot open report automatically. Please open Reports/html-report/index.html manually."
        fi
    else
        print_warning "No HTML report found. Run tests first."
    fi
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--env)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -b|--browser)
            BROWSER="$2"
            shift 2
            ;;
        -h|--headed)
            HEADED="true"
            shift
            ;;
        -p|--parallel)
            PARALLEL="true"
            shift
            ;;
        -w|--workers)
            WORKERS="$2"
            PARALLEL="true"
            shift 2
            ;;
        -l|--language)
            LANGUAGE="$2"
            shift 2
            ;;
        -t|--test)
            TEST_PATTERN="$2"
            shift 2
            ;;
        --electron)
            ELECTRON_ONLY="true"
            shift
            ;;
        --install)
            install_dependencies
            exit 0
            ;;
        --report)
            open_report
            exit 0
            ;;
        --help)
            show_usage
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

# Validate environment
if [[ ! "$ENVIRONMENT" =~ ^(dev|qa|prod)$ ]]; then
    print_error "Invalid environment: $ENVIRONMENT. Must be dev, qa, or prod."
    exit 1
fi

# Validate browser
if [[ ! "$BROWSER" =~ ^(chromium|firefox|webkit)$ ]]; then
    print_error "Invalid browser: $BROWSER. Must be chromium, firefox, or webkit."
    exit 1
fi

# Validate language
if [[ ! "$LANGUAGE" =~ ^(typescript|csharp|both)$ ]]; then
    print_error "Invalid language: $LANGUAGE. Must be typescript, csharp, or both."
    exit 1
fi

# Create reports directory
mkdir -p Reports/screenshots
mkdir -p Reports/logs

# Print configuration
print_info "Test Configuration:"
echo "  Environment: $ENVIRONMENT"
echo "  Browser: $BROWSER"
echo "  Headed: $HEADED"
echo "  Parallel: $PARALLEL"
if [ "$PARALLEL" = "true" ]; then
    echo "  Workers: $WORKERS"
fi
echo "  Language: $LANGUAGE"
if [ "$TEST_PATTERN" != "" ]; then
    echo "  Test Pattern: $TEST_PATTERN"
fi
if [ "$ELECTRON_ONLY" = "true" ]; then
    echo "  Electron Only: true"
fi
echo ""

# Run tests based on language selection
case $LANGUAGE in
    typescript)
        run_typescript_tests
        ;;
    csharp)
        run_csharp_tests
        ;;
    both)
        print_info "Running both TypeScript and C# tests..."
        run_typescript_tests
        run_csharp_tests
        ;;
esac

print_success "Test execution completed!"
print_info "Reports available in: Reports/"
print_info "To view HTML report, run: $0 --report"