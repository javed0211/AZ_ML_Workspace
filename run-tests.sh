#!/bin/bash

# Bash script for running Azure ML tests on macOS/Linux

# Default values
CATEGORY="all"
BROWSER="chromium"
HEADLESS=true
PARALLEL=false
COVERAGE=false
ENVIRONMENT="test"
OUTPUT_PATH="TestResults"
MAX_RETRIES=3
GENERATE_REPORT=true
OPEN_REPORT=false

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Function to display help
show_help() {
    echo -e "${GREEN}Azure ML Test Automation Runner (Bash)${NC}"
    echo ""
    echo -e "${YELLOW}Usage: ./run-tests.sh [OPTIONS]${NC}"
    echo ""
    echo -e "${YELLOW}Options:${NC}"
    echo -e "  --category <string>     Test category to run (all, UI, API, BDD, Smoke, Performance)"
    echo -e "  --browser <string>      Browser for UI tests (chromium, firefox, webkit, all)"
    echo -e "  --headless              Run browser in headless mode (default: true)"
    echo -e "  --headed                Run browser in headed mode"
    echo -e "  --parallel              Enable parallel test execution"
    echo -e "  --coverage              Generate code coverage report"
    echo -e "  --environment <string>  Target environment (test, staging, production)"
    echo -e "  --output <string>       Output directory for test results"
    echo -e "  --max-retries <int>     Maximum retry attempts for failed tests"
    echo -e "  --generate-report       Generate HTML test report (default: true)"
    echo -e "  --no-report             Skip HTML test report generation"
    echo -e "  --open-report           Open test report after execution"
    echo -e "  --help                  Show this help message"
    echo ""
    echo -e "${YELLOW}Examples:${NC}"
    echo -e "${GRAY}  ./run-tests.sh --category UI --browser firefox --headed${NC}"
    echo -e "${GRAY}  ./run-tests.sh --category API --coverage --parallel${NC}"
    echo -e "${GRAY}  ./run-tests.sh --category BDD --environment staging${NC}"
    exit 0
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --category)
            CATEGORY="$2"
            shift 2
            ;;
        --browser)
            BROWSER="$2"
            shift 2
            ;;
        --headless)
            HEADLESS=true
            shift
            ;;
        --headed)
            HEADLESS=false
            shift
            ;;
        --parallel)
            PARALLEL=true
            shift
            ;;
        --coverage)
            COVERAGE=true
            shift
            ;;
        --environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        --output)
            OUTPUT_PATH="$2"
            shift 2
            ;;
        --max-retries)
            MAX_RETRIES="$2"
            shift 2
            ;;
        --generate-report)
            GENERATE_REPORT=true
            shift
            ;;
        --no-report)
            GENERATE_REPORT=false
            shift
            ;;
        --open-report)
            OPEN_REPORT=true
            shift
            ;;
        --help)
            show_help
            ;;
        *)
            echo -e "${RED}❌ Unknown option: $1${NC}"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

echo -e "${GREEN}🚀 Azure ML Test Automation Framework${NC}"
echo -e "${GREEN}=====================================${NC}"
echo ""

# Validate parameters
valid_categories=("all" "UI" "API" "BDD" "Smoke" "Performance" "Security")
valid_browsers=("chromium" "firefox" "webkit" "all")
valid_environments=("test" "staging" "production")

if [[ ! " ${valid_categories[@]} " =~ " ${CATEGORY} " ]]; then
    echo -e "${RED}❌ Invalid category: $CATEGORY${NC}"
    echo -e "${GRAY}Valid categories: ${valid_categories[*]}${NC}"
    exit 1
fi

if [[ ! " ${valid_browsers[@]} " =~ " ${BROWSER} " ]]; then
    echo -e "${RED}❌ Invalid browser: $BROWSER${NC}"
    echo -e "${GRAY}Valid browsers: ${valid_browsers[*]}${NC}"
    exit 1
fi

if [[ ! " ${valid_environments[@]} " =~ " ${ENVIRONMENT} " ]]; then
    echo -e "${RED}❌ Invalid environment: $ENVIRONMENT${NC}"
    echo -e "${GRAY}Valid environments: ${valid_environments[*]}${NC}"
    exit 1
fi

# Display configuration
echo -e "${YELLOW}Configuration:${NC}"
echo -e "${GRAY}  Category: $CATEGORY${NC}"
echo -e "${GRAY}  Browser: $BROWSER${NC}"
echo -e "${GRAY}  Headless: $HEADLESS${NC}"
echo -e "${GRAY}  Parallel: $PARALLEL${NC}"
echo -e "${GRAY}  Coverage: $COVERAGE${NC}"
echo -e "${GRAY}  Environment: $ENVIRONMENT${NC}"
echo -e "${GRAY}  Output Path: $OUTPUT_PATH${NC}"
echo ""

# Check prerequisites
echo -e "${BLUE}🔍 Checking prerequisites...${NC}"

# Check .NET SDK
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "${GREEN}✅ .NET SDK: $DOTNET_VERSION${NC}"
else
    echo -e "${RED}❌ .NET SDK not found. Please install .NET 8.0 SDK.${NC}"
    exit 1
fi

# Check if project exists
PROJECT_PATH="AzureMLWorkspace.Tests/AzureMLWorkspace.Tests.csproj"
if [[ -f "$PROJECT_PATH" ]]; then
    echo -e "${GREEN}✅ Project file found${NC}"
else
    echo -e "${RED}❌ Project file not found: $PROJECT_PATH${NC}"
    exit 1
fi

# Create output directory
if [[ ! -d "$OUTPUT_PATH" ]]; then
    mkdir -p "$OUTPUT_PATH"
    echo -e "${GREEN}✅ Created output directory: $OUTPUT_PATH${NC}"
fi

# Restore packages
echo ""
echo -e "${BLUE}📦 Restoring packages...${NC}"
if dotnet restore "$PROJECT_PATH"; then
    echo -e "${GREEN}✅ Packages restored successfully${NC}"
else
    echo -e "${RED}❌ Failed to restore packages${NC}"
    exit 1
fi

# Build solution
echo ""
echo -e "${BLUE}🔨 Building solution...${NC}"
if dotnet build "$PROJECT_PATH" --configuration Release --no-restore; then
    echo -e "${GREEN}✅ Build completed successfully${NC}"
else
    echo -e "${RED}❌ Build failed${NC}"
    exit 1
fi

# Install Playwright browsers
echo ""
echo -e "${BLUE}🌐 Installing Playwright browsers...${NC}"
if [[ "$BROWSER" == "all" ]]; then
    if dotnet run --project "$PROJECT_PATH" -- playwright install --with-deps; then
        echo -e "${GREEN}✅ Playwright browsers installed${NC}"
    else
        echo -e "${RED}❌ Failed to install Playwright browsers${NC}"
        exit 1
    fi
else
    if dotnet run --project "$PROJECT_PATH" -- playwright install --with-deps "$BROWSER"; then
        echo -e "${GREEN}✅ Playwright browsers installed${NC}"
    else
        echo -e "${RED}❌ Failed to install Playwright browsers${NC}"
        exit 1
    fi
fi

# Set environment variables
export ASPNETCORE_ENVIRONMENT="$ENVIRONMENT"
export BROWSER_TYPE="$BROWSER"
export HEADLESS_MODE="$HEADLESS"

# Build test filter
TEST_FILTER=""
case $CATEGORY in
    "all") TEST_FILTER="" ;;
    "UI") TEST_FILTER="Category=UI" ;;
    "API") TEST_FILTER="Category=API" ;;
    "BDD") TEST_FILTER="Category=BDD" ;;
    "Smoke") TEST_FILTER="Category=Smoke" ;;
    "Performance") TEST_FILTER="Category=Performance" ;;
    "Security") TEST_FILTER="Category=Security" ;;
esac

# Build test arguments
TEST_ARGS=(
    "--configuration" "Release"
    "--no-build"
    "--logger" "trx"
    "--logger" "console;verbosity=detailed"
    "--results-directory" "$OUTPUT_PATH"
)

if [[ "$COVERAGE" == true ]]; then
    TEST_ARGS+=("--collect:XPlat Code Coverage")
fi

if [[ -n "$TEST_FILTER" ]]; then
    TEST_ARGS+=("--filter" "$TEST_FILTER")
fi

if [[ "$PARALLEL" == true ]]; then
    TEST_ARGS+=("--parallel")
fi

# Run tests
echo ""
echo -e "${BLUE}🧪 Running tests...${NC}"
echo -e "${GRAY}Filter: ${TEST_FILTER:-All tests}${NC}"

TEST_START_TIME=$(date +%s)

if [[ "$BROWSER" == "all" && ("$CATEGORY" == "UI" || "$CATEGORY" == "all") ]]; then
    # Run tests for each browser
    BROWSERS=("chromium" "firefox" "webkit")
    for BROWSER_TYPE in "${BROWSERS[@]}"; do
        echo ""
        echo -e "${YELLOW}🌐 Running tests with $BROWSER_TYPE...${NC}"
        export BROWSER_TYPE="$BROWSER_TYPE"
        
        BROWSER_OUTPUT_PATH="$OUTPUT_PATH/$BROWSER_TYPE"
        mkdir -p "$BROWSER_OUTPUT_PATH"
        
        BROWSER_TEST_ARGS=("${TEST_ARGS[@]}")
        # Update results directory for this browser
        for i in "${!BROWSER_TEST_ARGS[@]}"; do
            if [[ "${BROWSER_TEST_ARGS[$i]}" == "--results-directory" ]]; then
                BROWSER_TEST_ARGS[$((i+1))]="$BROWSER_OUTPUT_PATH"
                break
            fi
        done
        
        if ! dotnet test "$PROJECT_PATH" "${BROWSER_TEST_ARGS[@]}"; then
            echo -e "${RED}❌ Tests failed for $BROWSER_TYPE${NC}"
            exit 1
        fi
    done
else
    if ! dotnet test "$PROJECT_PATH" "${TEST_ARGS[@]}"; then
        echo -e "${RED}❌ Tests failed${NC}"
        exit 1
    fi
fi

TEST_END_TIME=$(date +%s)
TEST_DURATION=$((TEST_END_TIME - TEST_START_TIME))
TEST_DURATION_FORMATTED=$(printf "%02d:%02d" $((TEST_DURATION / 60)) $((TEST_DURATION % 60)))

echo ""
echo -e "${GREEN}✅ Tests completed successfully in $TEST_DURATION_FORMATTED${NC}"

# Generate coverage report
if [[ "$COVERAGE" == true ]]; then
    echo ""
    echo -e "${BLUE}📊 Generating coverage report...${NC}"
    
    # Install ReportGenerator if not already installed
    dotnet tool install -g dotnet-reportgenerator-globaltool 2>/dev/null || true
    
    COVERAGE_REPORT_PATH="$OUTPUT_PATH/CoverageReport"
    if reportgenerator \
        -reports:"$OUTPUT_PATH/**/coverage.cobertura.xml" \
        -targetdir:"$COVERAGE_REPORT_PATH" \
        -reporttypes:"Html;JsonSummary"; then
        
        echo -e "${GREEN}✅ Coverage report generated: $COVERAGE_REPORT_PATH${NC}"
        
        if [[ "$OPEN_REPORT" == true ]]; then
            REPORT_FILE="$COVERAGE_REPORT_PATH/index.html"
            if [[ -f "$REPORT_FILE" ]]; then
                if command -v open &> /dev/null; then
                    open "$REPORT_FILE"  # macOS
                elif command -v xdg-open &> /dev/null; then
                    xdg-open "$REPORT_FILE"  # Linux
                fi
            fi
        fi
    else
        echo -e "${YELLOW}⚠️  Failed to generate coverage report${NC}"
    fi
fi

# Generate HTML test report
if [[ "$GENERATE_REPORT" == true ]]; then
    echo ""
    echo -e "${BLUE}📋 Generating test report...${NC}"
    
    # Install dotnet-trx2html if not already installed
    dotnet tool install -g dotnet-trx2html 2>/dev/null || true
    
    # Find all TRX files and convert them to HTML
    find "$OUTPUT_PATH" -name "*.trx" -type f | while read -r TRX_FILE; do
        HTML_REPORT_PATH="${TRX_FILE%.trx}.html"
        if trx2html "$TRX_FILE" "$HTML_REPORT_PATH"; then
            echo -e "${GREEN}✅ Generated HTML report: $HTML_REPORT_PATH${NC}"
        fi
    done
    
    if [[ "$OPEN_REPORT" == true ]]; then
        FIRST_HTML_REPORT=$(find "$OUTPUT_PATH" -name "*.html" -type f | head -n 1)
        if [[ -f "$FIRST_HTML_REPORT" ]]; then
            if command -v open &> /dev/null; then
                open "$FIRST_HTML_REPORT"  # macOS
            elif command -v xdg-open &> /dev/null; then
                xdg-open "$FIRST_HTML_REPORT"  # Linux
            fi
        fi
    fi
fi

# Summary
echo ""
echo -e "${GREEN}🎉 Test execution completed!${NC}"
echo -e "${GRAY}Results available in: $OUTPUT_PATH${NC}"

# List generated artifacts
ARTIFACT_COUNT=$(find "$OUTPUT_PATH" -type f | wc -l)
if [[ $ARTIFACT_COUNT -gt 0 ]]; then
    echo ""
    echo -e "${YELLOW}Generated artifacts:${NC}"
    find "$OUTPUT_PATH" -type f | head -10 | while read -r ARTIFACT; do
        echo -e "${GRAY}  $ARTIFACT${NC}"
    done
    if [[ $ARTIFACT_COUNT -gt 10 ]]; then
        echo -e "${GRAY}  ... and $((ARTIFACT_COUNT - 10)) more files${NC}"
    fi
fi