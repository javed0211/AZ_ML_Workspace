#!/bin/bash

# Azure Speech Services Test Setup Script
# This script helps set up the Speech Services test environment

set -e

echo "=========================================="
echo "Azure Speech Services Test Setup"
echo "=========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

echo "Working directory: $SCRIPT_DIR"
echo ""

# Step 1: Check .NET installation
echo "Step 1: Checking .NET installation..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "${GREEN}✓${NC} .NET SDK found: $DOTNET_VERSION"
else
    echo -e "${RED}✗${NC} .NET SDK not found. Please install .NET 9.0 or later."
    echo "Download from: https://dotnet.microsoft.com/download"
    exit 1
fi
echo ""

# Step 2: Restore NuGet packages
echo "Step 2: Restoring NuGet packages..."
if dotnet restore; then
    echo -e "${GREEN}✓${NC} NuGet packages restored successfully"
else
    echo -e "${RED}✗${NC} Failed to restore NuGet packages"
    exit 1
fi
echo ""

# Step 3: Check for Speech SDK
echo "Step 3: Checking for Microsoft.CognitiveServices.Speech package..."
if dotnet list package | grep -q "Microsoft.CognitiveServices.Speech"; then
    echo -e "${GREEN}✓${NC} Speech SDK package found"
else
    echo -e "${YELLOW}!${NC} Speech SDK package not found. Installing..."
    dotnet add package Microsoft.CognitiveServices.Speech --version 1.38.0
fi
echo ""

# Step 4: Create test data directories
echo "Step 4: Creating test data directories..."
mkdir -p TestData/audio
mkdir -p TestData/output
mkdir -p Reports/logs
mkdir -p allure-results
echo -e "${GREEN}✓${NC} Directories created"
echo ""

# Step 5: Check for audio files
echo "Step 5: Checking for test audio files..."
AUDIO_DIR="TestData/audio"
AUDIO_FILES=("sample-english.wav" "sample-spanish.wav" "sample-french.wav")
MISSING_FILES=()

for file in "${AUDIO_FILES[@]}"; do
    if [ -f "$AUDIO_DIR/$file" ]; then
        echo -e "${GREEN}✓${NC} Found: $file"
    else
        echo -e "${YELLOW}!${NC} Missing: $file"
        MISSING_FILES+=("$file")
    fi
done

if [ ${#MISSING_FILES[@]} -gt 0 ]; then
    echo ""
    echo -e "${YELLOW}Warning:${NC} Some audio files are missing."
    echo "You can:"
    echo "  1. Record your own audio files"
    echo "  2. Generate them using Azure TTS"
    echo "  3. Download samples from Common Voice"
    echo ""
    echo "See TestData/audio/README.md for details."
    echo ""
    
    # Offer to create sample audio on macOS
    if [[ "$OSTYPE" == "darwin"* ]]; then
        read -p "Would you like to generate sample audio files using macOS 'say' command? (y/n) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            echo "Generating sample audio files..."
            say "This is a test of speech recognition accuracy" -o "$AUDIO_DIR/sample-english.wav" --data-format=LEI16@16000
            say "Esta es una prueba de reconocimiento de voz" -o "$AUDIO_DIR/sample-spanish.wav" -v Monica --data-format=LEI16@16000
            say "Ceci est un test de reconnaissance vocale" -o "$AUDIO_DIR/sample-french.wav" -v Thomas --data-format=LEI16@16000
            echo -e "${GREEN}✓${NC} Sample audio files generated"
        fi
    fi
fi
echo ""

# Step 6: Check configuration
echo "Step 6: Checking configuration..."
CONFIG_FILE="../Config/appsettings.json"

if [ -f "$CONFIG_FILE" ]; then
    echo -e "${GREEN}✓${NC} Configuration file found"
    
    # Check if Speech Services configuration exists
    if grep -q "SpeechServices" "$CONFIG_FILE"; then
        echo -e "${GREEN}✓${NC} Speech Services configuration found"
        
        # Check if placeholder values are still present
        if grep -q "your-speech-services-key" "$CONFIG_FILE"; then
            echo -e "${YELLOW}!${NC} Configuration contains placeholder values"
            echo ""
            echo "Please update the following in $CONFIG_FILE:"
            echo "  - SubscriptionKey: Your Azure Speech Services key"
            echo "  - Region: Your Azure region (e.g., uksouth, eastus)"
            echo ""
            echo "Or set environment variables:"
            echo "  export AZURE_SPEECH_KEY='your-key'"
            echo "  export AZURE_SPEECH_REGION='your-region'"
        else
            echo -e "${GREEN}✓${NC} Configuration appears to be set"
        fi
    else
        echo -e "${RED}✗${NC} Speech Services configuration not found in appsettings.json"
        exit 1
    fi
else
    echo -e "${RED}✗${NC} Configuration file not found: $CONFIG_FILE"
    exit 1
fi
echo ""

# Step 7: Build the project
echo "Step 7: Building the project..."
if dotnet build --no-restore; then
    echo -e "${GREEN}✓${NC} Project built successfully"
else
    echo -e "${RED}✗${NC} Build failed"
    exit 1
fi
echo ""

# Step 8: Check for Reqnroll code generation
echo "Step 8: Checking Reqnroll code generation..."
FEATURE_FILE="Features/AzureSpeechServices.feature"
CODEBEHIND_FILE="Features/AzureSpeechServices.feature.cs"

if [ -f "$FEATURE_FILE" ]; then
    echo -e "${GREEN}✓${NC} Feature file found"
    
    if [ -f "$CODEBEHIND_FILE" ]; then
        echo -e "${GREEN}✓${NC} Code-behind file generated"
    else
        echo -e "${YELLOW}!${NC} Code-behind file not found. Regenerating..."
        dotnet build /t:ReqnrollGenerate
    fi
else
    echo -e "${RED}✗${NC} Feature file not found: $FEATURE_FILE"
    exit 1
fi
echo ""

# Step 9: Run a quick smoke test
echo "Step 9: Running smoke tests..."
echo "This will verify your Azure Speech Services credentials..."
echo ""

read -p "Would you like to run smoke tests now? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Running smoke tests..."
    if dotnet test --filter "Category=smoke&Category=speech" --logger "console;verbosity=normal"; then
        echo -e "${GREEN}✓${NC} Smoke tests passed!"
    else
        echo -e "${YELLOW}!${NC} Some smoke tests failed. This is expected if:"
        echo "  - Azure credentials are not configured"
        echo "  - Audio files are missing"
        echo "  - Network connectivity issues"
    fi
else
    echo "Skipping smoke tests."
fi
echo ""

# Summary
echo "=========================================="
echo "Setup Summary"
echo "=========================================="
echo ""
echo "✓ .NET SDK: Installed"
echo "✓ NuGet Packages: Restored"
echo "✓ Project: Built successfully"
echo "✓ Test Structure: Ready"
echo ""
echo "Next Steps:"
echo "1. Configure Azure Speech Services credentials in appsettings.json"
echo "2. Add test audio files to TestData/audio/"
echo "3. Run tests with: dotnet test --filter \"Category=speech\""
echo ""
echo "For more information, see:"
echo "  - README_AZURE_SPEECH_SERVICES.md"
echo "  - TestData/audio/README.md"
echo ""
echo "Quick Test Commands:"
echo "  dotnet test --filter \"Category=smoke&Category=speech\"     # Smoke tests"
echo "  dotnet test --filter \"Category=stt\"                       # Speech-to-Text"
echo "  dotnet test --filter \"Category=tts\"                       # Text-to-Speech"
echo "  dotnet test --filter \"Category=speech\"                    # All tests"
echo ""
echo -e "${GREEN}Setup complete!${NC}"