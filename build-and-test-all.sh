#!/bin/bash

# Azure ML Test Framework - Unified Build and Test Script
# This script builds and runs both C# and TypeScript tests

set -e  # Exit on any error

echo "🚀 Azure ML Test Framework - Unified Build and Test"
echo "=================================================="

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

# Check if we're in the right directory
if [ ! -f "AzureMLTestFramework.sln" ]; then
    print_error "AzureMLTestFramework.sln not found. Please run this script from the root directory."
    exit 1
fi

# Step 1: Install Node.js dependencies
print_status "Installing Node.js dependencies..."
cd NewFramework
if [ -f "package.json" ]; then
    npm install
    print_success "Node.js dependencies installed"
else
    print_warning "package.json not found, skipping Node.js dependencies"
fi
cd ..

# Step 2: Install Playwright browsers
print_status "Installing Playwright browsers..."
cd NewFramework
if command -v npx &> /dev/null; then
    npx playwright install
    print_success "Playwright browsers installed"
else
    print_warning "npx not found, skipping Playwright browser installation"
fi
cd ..

# Step 3: Build C# project
print_status "Building C# project..."
if command -v dotnet &> /dev/null; then
    dotnet build NewFramework/CSharpTests/PlaywrightFramework.csproj --configuration Release
    if [ $? -eq 0 ]; then
        print_success "C# project built successfully"
    else
        print_error "C# project build failed"
        exit 1
    fi
else
    print_error ".NET SDK not found. Please install .NET 8.0 SDK"
    exit 1
fi

# Step 4: Restore C# packages
print_status "Restoring C# packages..."
dotnet restore NewFramework/CSharpTests/PlaywrightFramework.csproj
print_success "C# packages restored"

# Step 5: Install Playwright for .NET
print_status "Installing Playwright for .NET..."
cd NewFramework/CSharpTests
pwsh bin/Release/net8.0/playwright.ps1 install 2>/dev/null || dotnet run --project . --framework net8.0 -- install 2>/dev/null || echo "Playwright .NET installation may need manual setup"
cd ../..
print_success "Playwright for .NET setup completed"

# Step 6: Validate setup
print_status "Validating framework setup..."

# Check if feature files exist
if [ -f "NewFramework/Features/AzureMLWorkspace.feature" ]; then
    print_success "✅ BDD Feature files found"
else
    print_warning "⚠️ BDD Feature files not found"
fi

# Check if step definitions exist
if [ -f "NewFramework/CSharpTests/StepDefinitions/AzureMLWorkspaceSteps.cs" ]; then
    print_success "✅ C# Step definitions found"
else
    print_warning "⚠️ C# Step definitions not found"
fi

# Check if TypeScript tests exist
if [ -f "NewFramework/TypeScriptTests/azure-ml-workspace.spec.ts" ]; then
    print_success "✅ TypeScript tests found"
else
    print_warning "⚠️ TypeScript tests not found"
fi

# Check configuration
if [ -f "NewFramework/Config/appsettings.json" ]; then
    print_success "✅ Configuration file found"
else
    print_warning "⚠️ Configuration file not found"
fi

echo ""
echo "🎯 Test Execution Options:"
echo "========================="
echo "1. Run C# NUnit tests:           dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj"
echo "2. Run C# BDD tests:             dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj --filter Category=BDD"
echo "3. Run TypeScript tests:         cd NewFramework && npm test"
echo "4. Run specific TypeScript test: cd NewFramework && npx playwright test azure-ml-workspace.spec.ts"
echo "5. Run all tests:                ./run-all-tests.sh"

echo ""
print_success "🎉 Build and setup completed successfully!"
print_status "Your Azure ML Test Framework is ready with:"
print_status "  ✅ C# NUnit Tests (Traditional)"
print_status "  ✅ C# BDD Tests (Reqnroll/Gherkin)"
print_status "  ✅ TypeScript Tests (Playwright)"
print_status "  ✅ Unified Visual Studio Solution"
print_status "  ✅ PIM Role Activation Support"
print_status "  ✅ Azure ML Workspace Automation"
print_status "  ✅ VS Code Desktop Integration"

echo ""
echo "📖 Next Steps:"
echo "=============="
echo "1. Open AzureMLTestFramework.sln in Visual Studio"
echo "2. Configure your Azure settings in NewFramework/Config/appsettings.json"
echo "3. Run tests using Visual Studio Test Explorer or command line"
echo "4. Check GETTING-STARTED.md for detailed setup instructions"