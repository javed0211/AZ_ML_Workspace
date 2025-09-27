#!/bin/bash

# Azure ML Compute Instance Automation - Bash Script
# This script provides Unix/Linux/macOS automation for Azure ML compute instances

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default values
CONFIG_PATH="config/azure-ml-automation-config.json"
CREATE_ONLY=false
VSCODE_ONLY=false
SYNC_ONLY=false
CLEANUP=false
INSTALL_DEPS=false

# Function to print colored output
print_color() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

# Function to print usage
usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --config PATH          Path to configuration file (default: config/azure-ml-automation-config.json)"
    echo "  --create-only          Only create compute instance, don't setup VS Code"
    echo "  --vscode-only          Only setup VS Code connection (assume compute exists)"
    echo "  --sync-only            Only start file synchronization"
    echo "  --cleanup              Cleanup and delete compute instance"
    echo "  --install-deps         Install required dependencies"
    echo "  --help                 Show this help message"
    echo ""
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --config)
            CONFIG_PATH="$2"
            shift 2
            ;;
        --create-only)
            CREATE_ONLY=true
            shift
            ;;
        --vscode-only)
            VSCODE_ONLY=true
            shift
            ;;
        --sync-only)
            SYNC_ONLY=true
            shift
            ;;
        --cleanup)
            CLEANUP=true
            shift
            ;;
        --install-deps)
            INSTALL_DEPS=true
            shift
            ;;
        --help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to install Python dependencies
install_python_dependencies() {
    print_color $YELLOW "Installing required Python packages..."
    
    local packages=(
        "azure-ai-ml"
        "azure-identity"
        "paramiko"
        "watchdog"
        "psutil"
        "requests"
    )
    
    for package in "${packages[@]}"; do
        print_color $CYAN "Installing $package..."
        if pip install "$package" --upgrade; then
            print_color $GREEN "✓ $package installed successfully"
        else
            print_color $RED "✗ Failed to install $package"
        fi
    done
}

# Function to check Python installation
check_python() {
    if ! command_exists python3; then
        if ! command_exists python; then
            print_color $RED "Python is not installed. Please install Python 3.7+ first."
            exit 1
        else
            PYTHON_CMD="python"
        fi
    else
        PYTHON_CMD="python3"
    fi
    
    # Check Python version
    local python_version=$($PYTHON_CMD --version 2>&1 | cut -d' ' -f2)
    local major_version=$(echo $python_version | cut -d'.' -f1)
    local minor_version=$(echo $python_version | cut -d'.' -f2)
    
    if [[ $major_version -lt 3 ]] || [[ $major_version -eq 3 && $minor_version -lt 7 ]]; then
        print_color $RED "Python 3.7+ is required. Found: $python_version"
        exit 1
    fi
    
    print_color $GREEN "✓ Python $python_version found"
}

# Function to check pip installation
check_pip() {
    if ! command_exists pip3; then
        if ! command_exists pip; then
            print_color $RED "pip is not installed. Please install pip first."
            exit 1
        else
            PIP_CMD="pip"
        fi
    else
        PIP_CMD="pip3"
    fi
    
    print_color $GREEN "✓ pip found"
}

# Function to check VS Code installation
check_vscode() {
    local vscode_paths=(
        "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code"
        "/usr/local/bin/code"
        "/usr/bin/code"
        "$HOME/.local/bin/code"
    )
    
    # Check if code is in PATH
    if command_exists code; then
        VSCODE_CMD="code"
        print_color $GREEN "✓ VS Code found in PATH"
        return 0
    fi
    
    # Check common installation paths
    for path in "${vscode_paths[@]}"; do
        if [[ -f "$path" ]]; then
            VSCODE_CMD="$path"
            print_color $GREEN "✓ VS Code found at: $path"
            return 0
        fi
    done
    
    print_color $RED "VS Code not found. Please install VS Code first."
    print_color $YELLOW "Download from: https://code.visualstudio.com/"
    return 1
}

# Function to install VS Code extensions
install_vscode_extensions() {
    local extensions=(
        "ms-python.python"
        "ms-toolsai.jupyter"
        "ms-vscode-remote.remote-ssh"
        "ms-azuretools.vscode-azureml"
        "ms-vscode.powershell"
    )
    
    print_color $YELLOW "Installing VS Code extensions..."
    
    for extension in "${extensions[@]}"; do
        print_color $CYAN "Installing extension: $extension"
        if "$VSCODE_CMD" --install-extension "$extension" --force; then
            print_color $GREEN "✓ $extension installed"
        else
            print_color $RED "✗ Failed to install $extension"
        fi
    done
}

# Function to generate SSH key pair
generate_ssh_key() {
    local ssh_dir="$HOME/.ssh"
    local private_key="$ssh_dir/id_rsa"
    local public_key="$ssh_dir/id_rsa.pub"
    
    if [[ ! -d "$ssh_dir" ]]; then
        mkdir -p "$ssh_dir"
        chmod 700 "$ssh_dir"
    fi
    
    if [[ ! -f "$private_key" ]]; then
        print_color $YELLOW "Generating SSH key pair..."
        
        if command_exists ssh-keygen; then
            ssh-keygen -t rsa -b 2048 -f "$private_key" -N "" -C "azureml-automation-$(date +%Y%m%d)"
            chmod 600 "$private_key"
            chmod 644 "$public_key"
            print_color $GREEN "✓ SSH key pair generated"
        else
            print_color $RED "ssh-keygen not found. Please install OpenSSH."
            return 1
        fi
    else
        print_color $GREEN "✓ SSH key pair already exists"
    fi
    
    return 0
}

# Function to check Azure CLI
check_azure_cli() {
    if command_exists az; then
        print_color $GREEN "✓ Azure CLI found"
        
        # Check if logged in
        if az account show >/dev/null 2>&1; then
            print_color $GREEN "✓ Azure CLI is authenticated"
        else
            print_color $YELLOW "Azure CLI is not authenticated. You may need to run 'az login'"
        fi
    else
        print_color $YELLOW "Azure CLI not found. Install it for better Azure integration."
        print_color $YELLOW "Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    fi
}

# Function to setup environment
setup_environment() {
    print_color $CYAN "Setting up environment..."
    
    # Create necessary directories
    mkdir -p "$(dirname "$CONFIG_PATH")"
    mkdir -p "logs"
    
    # Set up Python virtual environment if requested
    if [[ "${SETUP_VENV:-false}" == "true" ]]; then
        if [[ ! -d "venv" ]]; then
            print_color $YELLOW "Creating Python virtual environment..."
            $PYTHON_CMD -m venv venv
        fi
        
        print_color $YELLOW "Activating virtual environment..."
        source venv/bin/activate
    fi
}

# Function to run the main Python automation
run_automation() {
    local python_script="azure-ml-compute-automation.py"
    
    if [[ ! -f "$python_script" ]]; then
        print_color $RED "Python automation script not found: $python_script"
        exit 1
    fi
    
    # Build command arguments
    local args=("$python_script" "--config" "$CONFIG_PATH")
    
    if [[ "$CREATE_ONLY" == "true" ]]; then
        args+=("--create-only")
    fi
    
    if [[ "$VSCODE_ONLY" == "true" ]]; then
        args+=("--vscode-only")
    fi
    
    if [[ "$SYNC_ONLY" == "true" ]]; then
        args+=("--sync-only")
    fi
    
    if [[ "$CLEANUP" == "true" ]]; then
        args+=("--cleanup")
    fi
    
    print_color $YELLOW "Running Python automation script..."
    print_color $CYAN "Command: $PYTHON_CMD ${args[*]}"
    
    if $PYTHON_CMD "${args[@]}"; then
        print_color $GREEN "✓ Automation completed successfully!"
        
        if [[ "$CREATE_ONLY" != "true" && "$CLEANUP" != "true" ]]; then
            print_color $GREEN "VS Code should now be connected to your Azure ML compute instance."
            print_color $YELLOW "You can also manually connect using: ssh azureml-compute"
        fi
    else
        print_color $RED "✗ Automation failed"
        exit 1
    fi
}

# Function to show system information
show_system_info() {
    print_color $CYAN "System Information:"
    print_color $CYAN "==================="
    echo "OS: $(uname -s)"
    echo "Architecture: $(uname -m)"
    echo "Kernel: $(uname -r)"
    
    if command_exists lsb_release; then
        echo "Distribution: $(lsb_release -d | cut -f2)"
    fi
    
    echo "Shell: $SHELL"
    echo "User: $USER"
    echo "Home: $HOME"
    echo ""
}

# Main function
main() {
    print_color $CYAN "Azure ML Compute Instance Automation - Bash Edition"
    print_color $CYAN "===================================================="
    echo ""
    
    # Show system info
    show_system_info
    
    # Install dependencies if requested
    if [[ "$INSTALL_DEPS" == "true" ]]; then
        check_python
        check_pip
        install_python_dependencies
        return 0
    fi
    
    # Check prerequisites
    print_color $YELLOW "Checking prerequisites..."
    check_python
    check_pip
    
    if ! check_vscode; then
        print_color $YELLOW "Continuing without VS Code (you can install it later)"
    else
        install_vscode_extensions
    fi
    
    check_azure_cli
    
    if ! generate_ssh_key; then
        print_color $RED "Failed to generate SSH key"
        exit 1
    fi
    
    # Setup environment
    setup_environment
    
    # Run the automation
    run_automation
}

# Trap to handle script interruption
trap 'print_color $YELLOW "\nScript interrupted by user."; exit 130' INT

# Run main function
main "$@"

print_color $CYAN "Script execution completed."