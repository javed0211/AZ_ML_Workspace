#!/bin/bash

# VS Code Electron Testing Framework Setup Script
# This script sets up the testing environment for VS Code automation

set -e  # Exit on any error

echo "🚀 Setting up VS Code Electron Testing Framework..."

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js 16 or higher."
    echo "   Visit: https://nodejs.org/"
    exit 1
fi

# Check Node.js version
NODE_VERSION=$(node --version | cut -d'v' -f2 | cut -d'.' -f1)
if [ "$NODE_VERSION" -lt 16 ]; then
    echo "❌ Node.js version 16 or higher is required. Current version: $(node --version)"
    exit 1
fi

echo "✅ Node.js version: $(node --version)"

# Check if VS Code is installed
VSCODE_PATH=""
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    VSCODE_PATH="/Applications/Visual Studio Code.app/Contents/MacOS/Electron"
    if [ ! -f "$VSCODE_PATH" ]; then
        echo "❌ VS Code not found at: $VSCODE_PATH"
        echo "   Please install VS Code from: https://code.visualstudio.com/"
        exit 1
    fi
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Linux
    if command -v code &> /dev/null; then
        VSCODE_PATH=$(which code)
    elif [ -f "/usr/share/code/code" ]; then
        VSCODE_PATH="/usr/share/code/code"
    elif [ -f "/usr/bin/code" ]; then
        VSCODE_PATH="/usr/bin/code"
    else
        echo "❌ VS Code not found. Please install VS Code."
        echo "   Visit: https://code.visualstudio.com/"
        exit 1
    fi
elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
    # Windows (Git Bash/Cygwin)
    VSCODE_PATH="C:\\Program Files\\Microsoft VS Code\\Code.exe"
    if [ ! -f "$VSCODE_PATH" ]; then
        VSCODE_PATH="C:\\Program Files (x86)\\Microsoft VS Code\\Code.exe"
        if [ ! -f "$VSCODE_PATH" ]; then
            echo "❌ VS Code not found. Please install VS Code."
            echo "   Visit: https://code.visualstudio.com/"
            exit 1
        fi
    fi
fi

echo "✅ VS Code found at: $VSCODE_PATH"

# Install npm dependencies
echo "📦 Installing npm dependencies..."
npm install

if [ $? -ne 0 ]; then
    echo "❌ Failed to install npm dependencies"
    exit 1
fi

echo "✅ npm dependencies installed"

# Install Playwright browsers
echo "🌐 Installing Playwright browsers..."
npm run install-browsers

if [ $? -ne 0 ]; then
    echo "❌ Failed to install Playwright browsers"
    exit 1
fi

echo "✅ Playwright browsers installed"

# Build TypeScript
echo "🔨 Building TypeScript..."
npm run build

if [ $? -ne 0 ]; then
    echo "❌ Failed to build TypeScript"
    exit 1
fi

echo "✅ TypeScript built successfully"

# Create temp directory
mkdir -p temp
echo "✅ Temporary directory created"

# Create reports directory
mkdir -p ../Reports/electron-test-results
mkdir -p ../Reports/screenshots
echo "✅ Reports directories created"

# Run a quick test to verify setup
echo "🧪 Running setup verification test..."
npx playwright test --grep "should launch VS Code successfully" --reporter=line

if [ $? -eq 0 ]; then
    echo ""
    echo "🎉 Setup completed successfully!"
    echo ""
    echo "📋 Next steps:"
    echo "   • Run all tests: npm test"
    echo "   • Run tests in headed mode: npm run test:headed"
    echo "   • Run tests with UI: npm run test:ui"
    echo "   • View test reports in: ../Reports/electron-test-results/"
    echo ""
    echo "📖 Documentation: See README.md for detailed usage instructions"
else
    echo ""
    echo "⚠️  Setup completed but verification test failed."
    echo "   This might be due to VS Code configuration or system-specific issues."
    echo "   Try running tests manually: npm test"
    echo ""
    echo "🔧 Troubleshooting:"
    echo "   • Check VS Code installation path in config/vscode-config.ts"
    echo "   • Ensure VS Code can be launched manually"
    echo "   • Check system permissions"
fi

echo ""
echo "🔍 System Information:"
echo "   OS: $OSTYPE"
echo "   Node.js: $(node --version)"
echo "   npm: $(npm --version)"
echo "   VS Code: $VSCODE_PATH"
echo ""