#!/bin/bash

# VS Code Installation Script for macOS
# This script will install VS Code using the most appropriate method

set -e

echo "🚀 VS Code Installation Script for macOS"
echo "========================================"

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to check if VS Code is already installed
check_vscode_installed() {
    if [ -f "/Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code" ] || 
       [ -f "/Applications/Visual Studio Code.app/Contents/MacOS/Electron" ]; then
        return 0
    else
        return 1
    fi
}

# Check if VS Code is already installed
if check_vscode_installed; then
    echo "✅ VS Code is already installed!"
    
    if [ -f "/Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code" ]; then
        VSCODE_PATH="/Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code"
    else
        VSCODE_PATH="/Applications/Visual Studio Code.app/Contents/MacOS/Electron"
    fi
    
    echo "📍 Found at: $VSCODE_PATH"
    
    # Get version
    echo "📋 Version info:"
    "$VSCODE_PATH" --version 2>/dev/null || echo "Could not get version info"
    
    echo ""
    echo "🎉 VS Code is ready for Electron testing!"
    exit 0
fi

echo "📦 VS Code not found. Installing..."

# Method 1: Try Homebrew (preferred)
if command_exists brew; then
    echo "🍺 Installing VS Code via Homebrew..."
    brew install --cask visual-studio-code
    
    # Wait a moment for installation to complete
    sleep 2
    
    if check_vscode_installed; then
        echo "✅ VS Code installed successfully via Homebrew!"
    else
        echo "❌ Homebrew installation failed, trying direct download..."
    fi
else
    echo "🍺 Homebrew not found, trying direct download..."
fi

# Method 2: Direct download if Homebrew failed or not available
if ! check_vscode_installed; then
    echo "📥 Downloading VS Code directly from Microsoft..."
    
    # Create temporary directory
    TEMP_DIR=$(mktemp -d)
    cd "$TEMP_DIR"
    
    # Download VS Code
    echo "⬇️  Downloading VS Code for macOS..."
    curl -L "https://code.visualstudio.com/sha/download?build=stable&os=darwin-universal" -o VSCode.zip
    
    # Extract
    echo "📦 Extracting VS Code..."
    unzip -q VSCode.zip
    
    # Move to Applications
    echo "📁 Installing to /Applications..."
    sudo mv "Visual Studio Code.app" /Applications/
    
    # Cleanup
    cd /
    rm -rf "$TEMP_DIR"
    
    if check_vscode_installed; then
        echo "✅ VS Code installed successfully via direct download!"
    else
        echo "❌ Installation failed. Please install VS Code manually from https://code.visualstudio.com/"
        exit 1
    fi
fi

# Final verification
echo ""
echo "🔍 Final verification..."

if check_vscode_installed; then
    if [ -f "/Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code" ]; then
        VSCODE_PATH="/Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code"
    else
        VSCODE_PATH="/Applications/Visual Studio Code.app/Contents/MacOS/Electron"
    fi
    
    echo "✅ VS Code installation successful!"
    echo "📍 Installed at: $VSCODE_PATH"
    
    # Get version
    echo "📋 Version info:"
    "$VSCODE_PATH" --version 2>/dev/null || echo "Could not get version info"
    
    echo ""
    echo "🎉 VS Code is now ready for Electron testing!"
    echo "💡 You can now run: npm test"
    
else
    echo "❌ Installation verification failed"
    echo "Please install VS Code manually from: https://code.visualstudio.com/"
    exit 1
fi