#!/bin/bash

# 📊 Generate Beautiful Allure HTML Report
# This script runs tests and generates an Allure report

set -e

echo "🚀 Starting test execution and report generation..."
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Clean previous results
echo -e "${BLUE}🧹 Cleaning previous test results...${NC}"
rm -rf allure-results/*
rm -rf allure-report/*

# Run tests
echo -e "${BLUE}🧪 Running tests...${NC}"
echo ""

if [ "$1" == "--smoke" ]; then
    echo "Running smoke tests only..."
    dotnet test --filter "Category=smoke" --logger "console;verbosity=normal"
elif [ "$1" == "--category" ] && [ -n "$2" ]; then
    echo "Running tests with category: $2"
    dotnet test --filter "Category=$2" --logger "console;verbosity=normal"
else
    echo "Running all tests..."
    dotnet test --logger "console;verbosity=normal"
fi

echo ""
echo -e "${GREEN}✅ Tests completed!${NC}"
echo ""

# Check if allure is installed
if ! command -v allure &> /dev/null; then
    echo -e "${YELLOW}⚠️  Allure CLI is not installed!${NC}"
    echo ""
    echo "Please install Allure:"
    echo "  macOS:   brew install allure"
    echo "  Windows: scoop install allure"
    echo "  Linux:   See ALLURE_REPORTS_GUIDE.md"
    echo ""
    exit 1
fi

# Check if results exist
if [ ! -d "allure-results" ] || [ -z "$(ls -A allure-results)" ]; then
    echo -e "${YELLOW}⚠️  No test results found in allure-results directory${NC}"
    echo "Tests may not have generated Allure results."
    exit 1
fi

# Generate and serve report
echo -e "${BLUE}📊 Generating Allure report...${NC}"
echo ""

if [ "$1" == "--static" ]; then
    # Generate static report
    allure generate allure-results -o allure-report --clean
    echo ""
    echo -e "${GREEN}✅ Static report generated!${NC}"
    echo -e "📂 Report location: ${BLUE}$SCRIPT_DIR/allure-report/index.html${NC}"
    echo ""
    echo "To view the report, open:"
    echo "  open allure-report/index.html"
else
    # Serve report (opens in browser)
    echo -e "${GREEN}🌐 Opening report in browser...${NC}"
    echo ""
    allure serve allure-results
fi