#!/bin/bash

# Simple script to run the Google search test
# Usage: ./run-google-test.sh [--headed] [--browser chromium|firefox|webkit]

set -e

# Default values
HEADED="false"
BROWSER="chromium"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --headed)
            HEADED="true"
            shift
            ;;
        --browser)
            BROWSER="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 [--headed] [--browser chromium|firefox|webkit]"
            echo ""
            echo "Options:"
            echo "  --headed              Run test in headed mode (visible browser)"
            echo "  --browser BROWSER     Browser to use (chromium, firefox, webkit)"
            echo "  -h, --help           Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

print_info "🚀 Running Google Search Test for Marshall Headphones"
print_info "Browser: $BROWSER"
print_info "Mode: $([ "$HEADED" = "true" ] && echo "Headed (visible)" || echo "Headless")"

# Build the command
cmd="npx playwright test google-search.spec.ts --project=$BROWSER"

if [ "$HEADED" = "true" ]; then
    cmd="$cmd --headed"
fi

print_info "Executing: $cmd"
eval $cmd

print_success "✅ Google search test completed!"
print_info "📊 View detailed report: npx playwright show-report"
print_info "📁 Screenshots saved in: ./Reports/screenshots/"