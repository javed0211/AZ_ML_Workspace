#!/bin/bash

# Setup script for Azure ML Compute Instance Automation
# This script prepares the environment and makes scripts executable

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

print_color() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

print_color $CYAN "Azure ML Compute Instance Automation - Setup"
print_color $CYAN "============================================="

# Make scripts executable
print_color $YELLOW "Making scripts executable..."
chmod +x azure-ml-automation.sh
chmod +x azure-ml-compute-automation.py
chmod +x test-automation.py

print_color $GREEN "✓ Scripts are now executable"

# Create necessary directories
print_color $YELLOW "Creating directories..."
mkdir -p config
mkdir -p logs

print_color $GREEN "✓ Directories created"

# Check if configuration file exists
if [[ ! -f "config/azure-ml-automation-config.json" ]]; then
    print_color $YELLOW "Configuration file not found. It will be created on first run."
    print_color $YELLOW "Please update it with your Azure details before running the automation."
fi

# Check Python installation
if command -v python3 >/dev/null 2>&1; then
    PYTHON_CMD="python3"
elif command -v python >/dev/null 2>&1; then
    PYTHON_CMD="python"
else
    print_color $RED "Python is not installed. Please install Python 3.7+ first."
    exit 1
fi

print_color $GREEN "✓ Python found: $($PYTHON_CMD --version)"

# Check pip installation
if command -v pip3 >/dev/null 2>&1; then
    PIP_CMD="pip3"
elif command -v pip >/dev/null 2>&1; then
    PIP_CMD="pip"
else
    print_color $RED "pip is not installed. Please install pip first."
    exit 1
fi

print_color $GREEN "✓ pip found"

# Install Python dependencies
print_color $YELLOW "Installing Python dependencies..."
if $PIP_CMD install -r requirements.txt; then
    print_color $GREEN "✓ Python dependencies installed successfully"
else
    print_color $RED "✗ Failed to install some dependencies"
    print_color $YELLOW "You may need to install them manually"
fi

# Run validation tests
print_color $YELLOW "Running validation tests..."
if $PYTHON_CMD test-automation.py; then
    print_color $GREEN "✓ All validation tests passed"
else
    print_color $YELLOW "⚠ Some validation tests failed. Check the output above."
fi

print_color $CYAN ""
print_color $CYAN "Setup completed!"
print_color $CYAN "==============="
print_color $YELLOW "Next steps:"
print_color $YELLOW "1. Update config/azure-ml-automation-config.json with your Azure details"
print_color $YELLOW "2. Run: ./azure-ml-automation.sh"
print_color $YELLOW "3. Or run: python azure-ml-compute-automation.py"
print_color $CYAN ""
print_color $GREEN "Happy coding with Azure ML! 🚀"