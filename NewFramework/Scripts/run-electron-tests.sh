#!/bin/bash

# VS Code Electron Tests Runner Script
# This script runs the Electron-based VS Code automation tests

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ELECTRON_TESTS_DIR="$SCRIPT_DIR/../ElectronTests"
REPORTS_DIR="$SCRIPT_DIR/../Reports"

echo "🚀 Running VS Code Electron Tests..."
echo "📁 Test Directory: $ELECTRON_TESTS_DIR"
echo "📊 Reports Directory: $REPORTS_DIR"

# Check if Electron tests directory exists
if [ ! -d "$ELECTRON_TESTS_DIR" ]; then
    echo "❌ Electron tests directory not found: $ELECTRON_TESTS_DIR"
    exit 1
fi

# Navigate to Electron tests directory
cd "$ELECTRON_TESTS_DIR"

# Check if dependencies are installed
if [ ! -d "node_modules" ]; then
    echo "📦 Installing dependencies..."
    npm install
fi

# Check if Playwright browsers are installed
if [ ! -d "node_modules/@playwright" ]; then
    echo "🌐 Installing Playwright browsers..."
    npm run install-browsers
fi

# Build TypeScript
echo "🔨 Building TypeScript..."
npm run build

# Create reports directory if it doesn't exist
mkdir -p "$REPORTS_DIR/electron-test-results"
mkdir -p "$REPORTS_DIR/screenshots"

# Parse command line arguments
TEST_PATTERN=""
HEADED_MODE=false
DEBUG_MODE=false
UI_MODE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --headed)
            HEADED_MODE=true
            shift
            ;;
        --debug)
            DEBUG_MODE=true
            shift
            ;;
        --ui)
            UI_MODE=true
            shift
            ;;
        --grep)
            TEST_PATTERN="$2"
            shift 2
            ;;
        --help)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --headed     Run tests in headed mode (visible browser)"
            echo "  --debug      Run tests in debug mode"
            echo "  --ui         Run tests in UI mode"
            echo "  --grep TEXT  Run only tests matching TEXT"
            echo "  --help       Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                                    # Run all tests"
            echo "  $0 --headed                          # Run with visible browser"
            echo "  $0 --grep \"basic functionality\"      # Run specific tests"
            echo "  $0 --debug                           # Run in debug mode"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Build the test command
TEST_CMD="npx playwright test"

if [ "$HEADED_MODE" = true ]; then
    TEST_CMD="$TEST_CMD --headed"
fi

if [ "$DEBUG_MODE" = true ]; then
    TEST_CMD="$TEST_CMD --debug"
fi

if [ "$UI_MODE" = true ]; then
    TEST_CMD="$TEST_CMD --ui"
fi

if [ -n "$TEST_PATTERN" ]; then
    TEST_CMD="$TEST_CMD --grep \"$TEST_PATTERN\""
fi

# Run the tests
echo "🧪 Executing: $TEST_CMD"
echo ""

if eval $TEST_CMD; then
    echo ""
    echo "✅ Electron tests completed successfully!"
    echo ""
    echo "📊 Test Reports:"
    echo "   HTML Report: $REPORTS_DIR/electron-test-results/index.html"
    echo "   JSON Report: $REPORTS_DIR/electron-test-results.json"
    echo "   Screenshots: $REPORTS_DIR/screenshots/"
    echo ""
    echo "🌐 Open HTML report:"
    echo "   open $REPORTS_DIR/electron-test-results/index.html"
else
    echo ""
    echo "❌ Electron tests failed!"
    echo ""
    echo "🔍 Check the following for troubleshooting:"
    echo "   • Screenshots: $REPORTS_DIR/screenshots/"
    echo "   • Test logs in the output above"
    echo "   • VS Code installation and permissions"
    echo ""
    echo "💡 Try running with --headed to see what's happening:"
    echo "   $0 --headed"
    exit 1
fi