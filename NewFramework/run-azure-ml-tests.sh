#!/bin/bash

# Azure ML Workspace Test Runner
# This script runs the Azure ML Workspace automation tests using the new Playwright framework

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
LANGUAGE="typescript"
BROWSER="chromium"
HEADED="false"
ENVIRONMENT="dev"
TEST_FILTER=""
PARALLEL="false"
WORKERS=1
REPORT_OPEN="true"
BDD_MODE="false"

# Function to display usage
show_usage() {
    echo -e "${BLUE}Azure ML Workspace Test Runner${NC}"
    echo ""
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --language, -l      Test language (typescript|csharp) [default: typescript]"
    echo "  --browser, -b       Browser to use (chromium|firefox|webkit) [default: chromium]"
    echo "  --headed, -h        Run in headed mode (visible browser)"
    echo "  --environment, -e   Environment to test against (dev|qa|prod) [default: dev]"
    echo "  --test, -t          Specific test to run (partial name match)"
    echo "  --parallel, -p      Run tests in parallel"
    echo "  --workers, -w       Number of parallel workers [default: 1]"
    echo "  --bdd               Run BDD tests using Reqnroll (C# only)"
    echo "  --no-report         Don't open HTML report after execution"
    echo "  --install           Install dependencies only"
    echo "  --help              Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 --headed                                    # Run all Azure ML tests in headed mode"
    echo "  $0 --test \"workspace access\"                   # Run workspace access test only"
    echo "  $0 --language csharp --browser firefox         # Run C# tests in Firefox"
    echo "  $0 --bdd --language csharp                     # Run BDD tests using Reqnroll"
    echo "  $0 --environment prod --parallel --workers 2   # Run in production with 2 parallel workers"
    echo "  $0 --test \"compute\" --headed                   # Run compute-related tests in headed mode"
}

# Function to log messages
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to install dependencies
install_dependencies() {
    log_info "Installing dependencies..."
    
    if [ "$LANGUAGE" = "typescript" ]; then
        log_info "Installing Node.js dependencies..."
        npm install
        
        log_info "Installing Playwright browsers..."
        npx playwright install
    else
        log_info "Restoring .NET packages..."
        cd CSharpTests
        dotnet restore
        
        log_info "Building C# project..."
        dotnet build
        
        log_info "Installing Playwright browsers for .NET..."
        pwsh bin/Debug/net8.0/playwright.ps1 install
        cd ..
    fi
    
    log_info "Dependencies installed successfully!"
}

# Function to run TypeScript tests
run_typescript_tests() {
    log_info "🚀 Running Azure ML Workspace Tests (TypeScript)"
    log_info "Language: TypeScript"
    log_info "Browser: $BROWSER"
    log_info "Environment: $ENVIRONMENT"
    log_info "Mode: $([ "$HEADED" = "true" ] && echo "Headed (visible)" || echo "Headless")"
    
    # Set environment variable
    export TEST_ENV="$ENVIRONMENT"
    
    # Build test command
    local cmd="npx playwright test azure-ml-workspace.spec.ts"
    
    # Add browser project
    cmd="$cmd --project=$BROWSER"
    
    # Add headed mode if requested
    if [ "$HEADED" = "true" ]; then
        cmd="$cmd --headed"
    fi
    
    # Add test filter if specified
    if [ -n "$TEST_FILTER" ]; then
        cmd="$cmd --grep \"$TEST_FILTER\""
    fi
    
    # Add parallel execution if requested
    if [ "$PARALLEL" = "true" ]; then
        cmd="$cmd --workers=$WORKERS"
    else
        cmd="$cmd --workers=1"
    fi
    
    # Add reporter
    cmd="$cmd --reporter=html,line"
    
    log_info "Executing: $cmd"
    eval $cmd
    
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        log_info "✅ All Azure ML tests passed!"
    else
        log_error "❌ Some Azure ML tests failed!"
    fi
    
    # Open report if requested
    if [ "$REPORT_OPEN" = "true" ] && [ -f "playwright-report/index.html" ]; then
        log_info "📊 Opening test report..."
        if command -v open >/dev/null 2>&1; then
            open playwright-report/index.html
        elif command -v xdg-open >/dev/null 2>&1; then
            xdg-open playwright-report/index.html
        else
            log_info "Report available at: $(pwd)/playwright-report/index.html"
        fi
    fi
    
    return $exit_code
}

# Function to run C# tests
run_csharp_tests() {
    log_info "🚀 Running Azure ML Workspace Tests (C#)"
    log_info "Language: C#"
    log_info "Browser: $BROWSER"
    log_info "Environment: $ENVIRONMENT"
    log_info "Mode: $([ "$HEADED" = "true" ] && echo "Headed (visible)" || echo "Headless")"
    
    cd CSharpTests
    
    # Set environment variables
    export TEST_ENV="$ENVIRONMENT"
    export BROWSER_TYPE="$BROWSER"
    export HEADED_MODE="$HEADED"
    
    # Build test command
    local cmd="dotnet test"
    
    # Add test filter if specified
    if [ -n "$TEST_FILTER" ]; then
        cmd="$cmd --filter \"DisplayName~$TEST_FILTER\""
    else
        cmd="$cmd --filter \"Category=AzureML\""
    fi
    
    # Add logger for better output
    cmd="$cmd --logger \"console;verbosity=detailed\""
    
    # Add parallel execution if requested
    if [ "$PARALLEL" = "true" ]; then
        cmd="$cmd --parallel --maxcpucount:$WORKERS"
    fi
    
    log_info "Executing: $cmd"
    eval $cmd
    
    local exit_code=$?
    
    cd ..
    
    if [ $exit_code -eq 0 ]; then
        log_info "✅ All Azure ML tests passed!"
    else
        log_error "❌ Some Azure ML tests failed!"
    fi
    
    return $exit_code
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --language|-l)
            LANGUAGE="$2"
            shift 2
            ;;
        --browser|-b)
            BROWSER="$2"
            shift 2
            ;;
        --headed|-h)
            HEADED="true"
            shift
            ;;
        --environment|-e)
            ENVIRONMENT="$2"
            shift 2
            ;;
        --test|-t)
            TEST_FILTER="$2"
            shift 2
            ;;
        --parallel|-p)
            PARALLEL="true"
            shift
            ;;
        --workers|-w)
            WORKERS="$2"
            shift 2
            ;;
        --bdd)
            BDD_MODE="true"
            LANGUAGE="csharp"  # BDD only supported in C#
            shift
            ;;
        --no-report)
            REPORT_OPEN="false"
            shift
            ;;
        --install)
            install_dependencies
            exit 0
            ;;
        --help)
            show_usage
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

# Validate inputs
if [ "$LANGUAGE" != "typescript" ] && [ "$LANGUAGE" != "csharp" ]; then
    log_error "Invalid language: $LANGUAGE. Must be 'typescript' or 'csharp'"
    exit 1
fi

if [ "$BROWSER" != "chromium" ] && [ "$BROWSER" != "firefox" ] && [ "$BROWSER" != "webkit" ]; then
    log_error "Invalid browser: $BROWSER. Must be 'chromium', 'firefox', or 'webkit'"
    exit 1
fi

if [ "$ENVIRONMENT" != "dev" ] && [ "$ENVIRONMENT" != "qa" ] && [ "$ENVIRONMENT" != "prod" ]; then
    log_error "Invalid environment: $ENVIRONMENT. Must be 'dev', 'qa', or 'prod'"
    exit 1
fi

# Check if dependencies are installed
if [ "$LANGUAGE" = "typescript" ]; then
    if [ ! -d "node_modules" ]; then
        log_warning "Node modules not found. Installing dependencies..."
        install_dependencies
    fi
else
    if [ ! -f "CSharpTests/bin/Debug/net8.0/PlaywrightFramework.dll" ]; then
        log_warning "C# project not built. Installing dependencies..."
        install_dependencies
    fi
fi

# Create reports directory
mkdir -p Reports/screenshots
mkdir -p Reports/videos
mkdir -p Reports/logs

# Run the tests
log_info "🎯 Starting Azure ML Workspace Test Automation"
log_info "================================================"

if [ "$LANGUAGE" = "typescript" ]; then
    run_typescript_tests
else
    run_csharp_tests
fi

exit_code=$?

log_info "================================================"
if [ $exit_code -eq 0 ]; then
    log_info "🎉 Azure ML Workspace test execution completed successfully!"
else
    log_error "💥 Azure ML Workspace test execution completed with failures!"
fi

exit $exit_code