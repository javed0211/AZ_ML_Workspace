#!/bin/bash

# VS Code Notebook Automation Test Runner
# This script runs the VS Code notebook automation tests using Cucumber and Playwright

echo "🚀 Starting VS Code Notebook Automation Tests"
echo "=============================================="

# Set the working directory to the NewFramework directory
cd "$(dirname "$0")/.."

# Check if VS Code is installed
if ! command -v code &> /dev/null; then
    echo "⚠️  Warning: VS Code command 'code' not found in PATH"
    echo "   VS Code might still work if installed in default location"
fi

# Check if required dependencies are installed
echo "📦 Checking dependencies..."
if ! npm list @cucumber/cucumber &> /dev/null; then
    echo "❌ @cucumber/cucumber not found. Installing dependencies..."
    npm install
fi

if ! npm list @playwright/test &> /dev/null; then
    echo "❌ @playwright/test not found. Installing dependencies..."
    npm install
fi

# Create necessary directories
echo "📁 Creating required directories..."
mkdir -p Reports/screenshots
mkdir -p Reports/logs
mkdir -p TestData

# Set environment variables
export NODE_ENV=test
export HEADLESS=false  # Set to true for headless mode

echo "🧪 Running VS Code Notebook Tests..."
echo ""

# Run specific VS Code notebook tests
echo "Running: Create and execute new notebook test"
npx cucumber-js TypeScriptTests/Features/VSCodeNotebookAutomation.feature:8 \
    --require TypeScriptTests/StepDefinitions/VSCodeNotebookSteps.ts \
    --require-module ts-node/register \
    --format progress \
    --format json:Reports/vscode-notebook-results.json

echo ""
echo "Running: Open and execute existing notebook test"
npx cucumber-js TypeScriptTests/Features/VSCodeNotebookAutomation.feature:18 \
    --require TypeScriptTests/StepDefinitions/VSCodeNotebookSteps.ts \
    --require-module ts-node/register \
    --format progress \
    --format json:Reports/vscode-existing-notebook-results.json

echo ""
echo "Running: Comprehensive notebook workflow test"
npx cucumber-js TypeScriptTests/Features/VSCodeNotebookAutomation.feature:28 \
    --require TypeScriptTests/StepDefinitions/VSCodeNotebookSteps.ts \
    --require-module ts-node/register \
    --format progress \
    --format json:Reports/vscode-comprehensive-results.json

echo ""
echo "Running: Error handling test"
npx cucumber-js TypeScriptTests/Features/VSCodeNotebookAutomation.feature:45 \
    --require TypeScriptTests/StepDefinitions/VSCodeNotebookSteps.ts \
    --require-module ts-node/register \
    --format progress \
    --format json:Reports/vscode-error-handling-results.json

echo ""
echo "✅ VS Code Notebook Automation Tests Completed!"
echo ""
echo "📊 Test Results:"
echo "   - Screenshots: Reports/screenshots/"
echo "   - Logs: Reports/logs/"
echo "   - JSON Results: Reports/vscode-*-results.json"
echo ""
echo "🔍 To run all tests at once:"
echo "   npx cucumber-js TypeScriptTests/Features/VSCodeNotebookAutomation.feature"
echo ""
echo "🎯 To run with specific tags:"
echo "   npx cucumber-js --tags '@vscode and @notebook'"
echo "   npx cucumber-js --tags '@electron'"
echo "   npx cucumber-js --tags '@comprehensive'"
echo ""
echo "⚙️  Configuration:"
echo "   - Headless mode: $HEADLESS"
echo "   - Environment: $NODE_ENV"
echo "   - VS Code path configured in: Config/appsettings.json"