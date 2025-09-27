#!/bin/bash

# Cross-Platform Jupyter Notebook Test Runner Script
# Works on macOS, Linux, and Windows (with Git Bash)

echo "🚀 Cross-Platform Jupyter Notebook Test Runner"
echo "=============================================="

# Default values
DEFAULT_NOTEBOOK_FOLDER="/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook"
DEFAULT_NOTEBOOK_FILE="azure_ml_project.ipynb"

# Check if custom folder is provided as argument
if [ "$1" != "" ]; then
    export NOTEBOOK_FOLDER="$1"
    echo "📁 Using custom notebook folder: $1"
else
    export NOTEBOOK_FOLDER="$DEFAULT_NOTEBOOK_FOLDER"
    echo "📁 Using default notebook folder: $DEFAULT_NOTEBOOK_FOLDER"
fi

# Check if custom file is provided as second argument
if [ "$2" != "" ]; then
    export NOTEBOOK_FILE="$2"
    echo "📓 Using custom notebook file: $2"
else
    export NOTEBOOK_FILE="$DEFAULT_NOTEBOOK_FILE"
    echo "📓 Using default notebook file: $DEFAULT_NOTEBOOK_FILE"
fi

# Detect platform
PLATFORM=$(uname -s)
echo "🖥️  Detected platform: $PLATFORM"

# Check if VS Code is available (the test will handle the actual VS Code detection)
echo "🔍 Checking VS Code availability..."
if command -v code &> /dev/null; then
    echo "✅ VS Code CLI is available in PATH"
    code --version | head -1
elif [ -f "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code" ]; then
    echo "✅ VS Code found in Applications folder"
elif [ -f "/Users/oldguard/Downloads/Visual Studio Code.app/Contents/MacOS/Electron" ]; then
    echo "✅ VS Code found in Downloads folder"
elif [ -f "/usr/bin/code" ]; then
    echo "✅ VS Code found in /usr/bin"
else
    echo "⚠️  VS Code CLI not found in PATH, but the test will attempt to locate it automatically"
    echo "💡 If the test fails, install VS Code and run 'Shell Command: Install code command in PATH'"
fi

# Check if the notebook folder exists
if [ -d "$NOTEBOOK_FOLDER" ]; then
    echo "✅ Notebook folder exists: $NOTEBOOK_FOLDER"
    
    # Count .ipynb files
    NOTEBOOK_COUNT=$(find "$NOTEBOOK_FOLDER" -name "*.ipynb" | wc -l)
    echo "📚 Found $NOTEBOOK_COUNT notebook files in folder"
    
    if [ $NOTEBOOK_COUNT -gt 0 ]; then
        echo "📋 Notebook files:"
        find "$NOTEBOOK_FOLDER" -name "*.ipynb" -exec basename {} \;
    fi
else
    echo "⚠️  Notebook folder does not exist: $NOTEBOOK_FOLDER"
    echo "The test will create a fallback notebook for testing."
fi

echo ""
echo "🧪 Running Playwright tests..."
echo "=============================================="

# Run the specific test for existing notebooks
npx playwright test tests/cross-platform-jupyter.test.ts -g "existing notebook" --headed

echo ""
echo "✅ Test execution completed!"
echo "=============================================="