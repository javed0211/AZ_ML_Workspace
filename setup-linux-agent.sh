#!/bin/bash
################################################################################
# Linux Self-Hosted Agent Setup Script
# For Azure ML Test Framework
#
# This script installs all required dependencies for running the test suite
# on a Linux-based Azure DevOps self-hosted agent.
#
# Usage: ./setup-linux-agent.sh
# Or: curl -fsSL https://raw.githubusercontent.com/your-repo/setup-linux-agent.sh | bash
################################################################################

set -e  # Exit on error
set -u  # Exit on undefined variable

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Detect OS
detect_os() {
    if [ -f /etc/os-release ]; then
        . /etc/os-release
        OS=$ID
        OS_VERSION=$VERSION_ID
    else
        log_error "Cannot detect OS. /etc/os-release not found."
        exit 1
    fi
    
    log_info "Detected OS: $OS $OS_VERSION"
}

# Check if running as root
check_root() {
    if [ "$EUID" -eq 0 ]; then
        log_warning "Running as root. This is not recommended for agent setup."
        read -p "Continue anyway? (y/N) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            exit 1
        fi
    fi
}

# Install system dependencies
install_system_deps() {
    log_info "Installing system dependencies..."
    
    case "$OS" in
        ubuntu|debian)
            sudo apt-get update
            sudo apt-get install -y \
                git \
                curl \
                wget \
                unzip \
                ca-certificates \
                gnupg \
                lsb-release \
                libicu-dev \
                libc6-dev \
                libgdiplus \
                libx11-dev \
                libssl-dev \
                libasound2 \
                default-jre
            
            # Try to install libssl1.1, fallback to libssl3 for newer Ubuntu
            if ! sudo apt-get install -y libssl1.1 2>/dev/null; then
                log_warning "libssl1.1 not available, installing libssl3"
                sudo apt-get install -y libssl3
            fi
            ;;
            
        rhel|centos|fedora)
            sudo yum install -y \
                git \
                curl \
                wget \
                unzip \
                ca-certificates \
                libicu \
                glibc-devel \
                libgdiplus \
                libX11-devel \
                openssl-devel \
                alsa-lib \
                java-11-openjdk
            ;;
            
        *)
            log_error "Unsupported OS: $OS"
            exit 1
            ;;
    esac
    
    log_success "System dependencies installed"
}

# Install .NET SDK 9.0
install_dotnet() {
    log_info "Installing .NET 9.0 SDK..."
    
    # Check if already installed
    if command -v dotnet &> /dev/null; then
        DOTNET_VERSION=$(dotnet --version)
        if [[ $DOTNET_VERSION == 9.* ]]; then
            log_success ".NET 9.0 SDK already installed: $DOTNET_VERSION"
            return 0
        else
            log_warning "Found .NET $DOTNET_VERSION, but need 9.0.x"
        fi
    fi
    
    # Download and run installer
    wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    /tmp/dotnet-install.sh --channel 9.0 --install-dir $HOME/.dotnet
    
    # Add to PATH
    export DOTNET_ROOT=$HOME/.dotnet
    export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
    
    # Add to profile for persistence
    if ! grep -q "DOTNET_ROOT" ~/.bashrc; then
        echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
        echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
    fi
    
    # Verify installation
    if dotnet --version &> /dev/null; then
        log_success ".NET SDK installed: $(dotnet --version)"
    else
        log_error ".NET SDK installation failed"
        exit 1
    fi
}

# Install Node.js 18.x
install_nodejs() {
    log_info "Installing Node.js 18.x..."
    
    # Check if already installed
    if command -v node &> /dev/null; then
        NODE_VERSION=$(node --version)
        if [[ $NODE_VERSION == v18.* ]]; then
            log_success "Node.js 18.x already installed: $NODE_VERSION"
            return 0
        else
            log_warning "Found Node.js $NODE_VERSION, but need v18.x"
        fi
    fi
    
    case "$OS" in
        ubuntu|debian)
            curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
            sudo apt-get install -y nodejs
            ;;
            
        rhel|centos|fedora)
            curl -fsSL https://rpm.nodesource.com/setup_18.x | sudo bash -
            sudo yum install -y nodejs
            ;;
    esac
    
    # Verify installation
    if node --version &> /dev/null; then
        log_success "Node.js installed: $(node --version)"
        log_success "npm installed: $(npm --version)"
    else
        log_error "Node.js installation failed"
        exit 1
    fi
}

# Install Playwright browsers
install_playwright() {
    log_info "Installing Playwright browsers..."
    
    # Install Playwright CLI
    npm install -g playwright@1.40.0
    
    # Install browsers with system dependencies
    npx playwright install --with-deps chromium firefox
    
    # Verify installation
    if npx playwright --version &> /dev/null; then
        log_success "Playwright installed: $(npx playwright --version)"
    else
        log_error "Playwright installation failed"
        exit 1
    fi
}

# Install Allure (optional)
install_allure() {
    log_info "Installing Allure reporting tool..."
    
    # Check if already installed
    if command -v allure &> /dev/null; then
        log_success "Allure already installed: $(allure --version)"
        return 0
    fi
    
    # Download and install
    ALLURE_VERSION="2.24.0"
    wget -q https://github.com/allure-framework/allure2/releases/download/${ALLURE_VERSION}/allure-${ALLURE_VERSION}.tgz -O /tmp/allure.tgz
    sudo tar -zxf /tmp/allure.tgz -C /opt/
    sudo ln -sf /opt/allure-${ALLURE_VERSION}/bin/allure /usr/bin/allure
    
    # Verify installation
    if allure --version &> /dev/null; then
        log_success "Allure installed: $(allure --version)"
    else
        log_warning "Allure installation failed (optional)"
    fi
}

# Setup project
setup_project() {
    log_info "Setting up project..."
    
    # Check if we're in the project directory
    if [ ! -f "NewFramework/CSharpTests/PlaywrightFramework.csproj" ]; then
        log_warning "Not in project root directory"
        log_info "Please run this script from the AZ_ML_Workspace directory"
        log_info "Or clone the repository first: git clone <repo-url>"
        return 1
    fi
    
    cd NewFramework/CSharpTests
    
    # Restore NuGet packages
    log_info "Restoring NuGet packages..."
    dotnet restore PlaywrightFramework.csproj
    
    # Build project
    log_info "Building project..."
    dotnet build PlaywrightFramework.csproj --configuration Release
    
    # Make scripts executable
    chmod +x *.sh 2>/dev/null || true
    
    cd ../..
    
    log_success "Project setup complete"
}

# Configure Azure credentials
configure_azure() {
    log_info "Configuring Azure credentials..."
    
    # Check if appsettings.json exists
    if [ ! -f "NewFramework/Config/appsettings.json" ]; then
        log_warning "appsettings.json not found"
        log_info "Please create NewFramework/Config/appsettings.json with your Azure credentials"
        log_info "See documentation for required configuration format"
        return 1
    fi
    
    # Check for environment variables
    if [ -z "${AZURE_TENANT_ID:-}" ]; then
        log_warning "Azure credentials not set in environment variables"
        log_info "You can set them with:"
        echo ""
        echo "  export AZURE_TENANT_ID=\"your-tenant-id\""
        echo "  export AZURE_CLIENT_ID=\"your-client-id\""
        echo "  export AZURE_CLIENT_SECRET=\"your-client-secret\""
        echo "  export AZURE_SUBSCRIPTION_ID=\"your-subscription-id\""
        echo ""
        log_info "Or configure them in appsettings.json"
    else
        log_success "Azure credentials found in environment"
    fi
}

# Run verification tests
verify_installation() {
    log_info "Verifying installation..."
    
    echo ""
    echo "=== Installation Verification ==="
    echo ""
    
    # Check .NET
    if dotnet --version &> /dev/null; then
        echo -e "${GREEN}✓${NC} .NET SDK: $(dotnet --version)"
    else
        echo -e "${RED}✗${NC} .NET SDK: Not found"
    fi
    
    # Check Node.js
    if node --version &> /dev/null; then
        echo -e "${GREEN}✓${NC} Node.js: $(node --version)"
    else
        echo -e "${RED}✗${NC} Node.js: Not found"
    fi
    
    # Check npm
    if npm --version &> /dev/null; then
        echo -e "${GREEN}✓${NC} npm: $(npm --version)"
    else
        echo -e "${RED}✗${NC} npm: Not found"
    fi
    
    # Check Playwright
    if npx playwright --version &> /dev/null; then
        echo -e "${GREEN}✓${NC} Playwright: $(npx playwright --version)"
    else
        echo -e "${RED}✗${NC} Playwright: Not found"
    fi
    
    # Check Allure
    if allure --version &> /dev/null; then
        echo -e "${GREEN}✓${NC} Allure: $(allure --version | head -n1)"
    else
        echo -e "${YELLOW}⚠${NC} Allure: Not found (optional)"
    fi
    
    # Check Azure connectivity
    if curl -s -o /dev/null -w "%{http_code}" https://management.azure.com | grep -q 401; then
        echo -e "${GREEN}✓${NC} Azure connectivity: OK"
    else
        echo -e "${YELLOW}⚠${NC} Azure connectivity: Cannot verify"
    fi
    
    echo ""
}

# Print next steps
print_next_steps() {
    echo ""
    echo "=========================================="
    echo "  Setup Complete! 🎉"
    echo "=========================================="
    echo ""
    echo "Next steps:"
    echo ""
    echo "1. Configure Azure credentials:"
    echo "   - Edit NewFramework/Config/appsettings.json"
    echo "   - Or set environment variables (see documentation)"
    echo ""
    echo "2. Run tests:"
    echo "   cd NewFramework/CSharpTests"
    echo "   dotnet test"
    echo ""
    echo "3. Run specific test suite:"
    echo "   dotnet test --filter \"Category=SpeechServices\""
    echo ""
    echo "4. Generate reports:"
    echo "   ./generate-allure-report.sh"
    echo ""
    echo "Documentation:"
    echo "  - LINUX_SELF_HOSTED_AGENT_REQUIREMENTS.md"
    echo "  - LINUX_QUICK_START.md"
    echo ""
    echo "=========================================="
    echo ""
}

# Main execution
main() {
    echo ""
    echo "=========================================="
    echo "  Azure ML Test Framework"
    echo "  Linux Agent Setup"
    echo "=========================================="
    echo ""
    
    detect_os
    check_root
    
    log_info "Starting installation..."
    echo ""
    
    install_system_deps
    install_dotnet
    install_nodejs
    install_playwright
    install_allure
    
    echo ""
    log_info "Installation complete. Setting up project..."
    echo ""
    
    setup_project || log_warning "Project setup skipped (not in project directory)"
    configure_azure || log_warning "Azure configuration skipped"
    
    echo ""
    verify_installation
    print_next_steps
}

# Run main function
main "$@"