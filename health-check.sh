#!/bin/bash
################################################################################
# Health Check Script for Linux Self-Hosted Agent
# Verifies all dependencies and configurations are correct
#
# Usage: ./health-check.sh
################################################################################

set -u  # Exit on undefined variable

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

PASS=0
FAIL=0
WARN=0

# Check function
check() {
    local name="$1"
    local command="$2"
    local required="${3:-true}"
    
    printf "%-40s" "$name: "
    
    if eval "$command" &> /dev/null; then
        echo -e "${GREEN}✓ PASS${NC}"
        ((PASS++))
        return 0
    else
        if [ "$required" = "true" ]; then
            echo -e "${RED}✗ FAIL${NC}"
            ((FAIL++))
        else
            echo -e "${YELLOW}⚠ WARN${NC} (optional)"
            ((WARN++))
        fi
        return 1
    fi
}

# Check with version
check_version() {
    local name="$1"
    local command="$2"
    local required="${3:-true}"
    
    printf "%-40s" "$name: "
    
    if version=$(eval "$command" 2>&1); then
        echo -e "${GREEN}✓${NC} $version"
        ((PASS++))
        return 0
    else
        if [ "$required" = "true" ]; then
            echo -e "${RED}✗ FAIL${NC}"
            ((FAIL++))
        else
            echo -e "${YELLOW}⚠ WARN${NC} (optional)"
            ((WARN++))
        fi
        return 1
    fi
}

# Check environment variable
check_env() {
    local name="$1"
    local var="$2"
    local required="${3:-true}"
    
    printf "%-40s" "$name: "
    
    if [ -n "${!var:-}" ]; then
        # Mask sensitive values
        local value="${!var}"
        local masked="${value:0:4}...${value: -4}"
        echo -e "${GREEN}✓${NC} Set ($masked)"
        ((PASS++))
        return 0
    else
        if [ "$required" = "true" ]; then
            echo -e "${RED}✗ FAIL${NC} Not set"
            ((FAIL++))
        else
            echo -e "${YELLOW}⚠ WARN${NC} Not set (optional)"
            ((WARN++))
        fi
        return 1
    fi
}

# Check file exists
check_file() {
    local name="$1"
    local file="$2"
    local required="${3:-true}"
    
    printf "%-40s" "$name: "
    
    if [ -f "$file" ]; then
        echo -e "${GREEN}✓${NC} Found"
        ((PASS++))
        return 0
    else
        if [ "$required" = "true" ]; then
            echo -e "${RED}✗ FAIL${NC} Not found"
            ((FAIL++))
        else
            echo -e "${YELLOW}⚠ WARN${NC} Not found (optional)"
            ((WARN++))
        fi
        return 1
    fi
}

# Check network connectivity
check_network() {
    local name="$1"
    local url="$2"
    
    printf "%-40s" "$name: "
    
    if curl -s -o /dev/null -w "%{http_code}" --max-time 5 "$url" | grep -qE "^(200|401|403)"; then
        echo -e "${GREEN}✓${NC} Reachable"
        ((PASS++))
        return 0
    else
        echo -e "${RED}✗ FAIL${NC} Unreachable"
        ((FAIL++))
        return 1
    fi
}

echo ""
echo "=========================================="
echo "  Health Check - Azure ML Test Framework"
echo "=========================================="
echo ""

# System Information
echo "=== System Information ==="
echo "OS: $(uname -s)"
echo "Kernel: $(uname -r)"
echo "Architecture: $(uname -m)"
echo "Hostname: $(hostname)"
echo ""

# Core Dependencies
echo "=== Core Dependencies ==="
check_version ".NET SDK" "dotnet --version"
check_version "Node.js" "node --version"
check_version "npm" "npm --version"
check "git" "command -v git"
check "curl" "command -v curl"
check "wget" "command -v wget"
echo ""

# Playwright
echo "=== Playwright ==="
check_version "Playwright CLI" "npx playwright --version"
check "Chromium browser" "npx playwright install --dry-run chromium 2>&1 | grep -q 'is already installed'" false
check "Firefox browser" "npx playwright install --dry-run firefox 2>&1 | grep -q 'is already installed'" false
echo ""

# System Libraries
echo "=== System Libraries ==="
check "libicu" "ldconfig -p | grep -q libicu"
check "libasound2" "ldconfig -p | grep -q libasound"
check "libssl" "ldconfig -p | grep -qE 'libssl\.(so|1)'"
check "libgdiplus" "ldconfig -p | grep -q libgdiplus" false
echo ""

# Project Files
echo "=== Project Files ==="
check_file "Project file" "NewFramework/CSharpTests/PlaywrightFramework.csproj"
check_file "Configuration" "NewFramework/Config/appsettings.json"
check_file "Reqnroll config" "NewFramework/CSharpTests/reqnroll.json"
check_file "Allure config" "NewFramework/CSharpTests/allureConfig.json" false
echo ""

# Azure Credentials
echo "=== Azure Credentials ==="
check_env "Azure Tenant ID" "AZURE_TENANT_ID" false
check_env "Azure Client ID" "AZURE_CLIENT_ID" false
check_env "Azure Client Secret" "AZURE_CLIENT_SECRET" false
check_env "Azure Subscription ID" "AZURE_SUBSCRIPTION_ID" false
echo ""

# Network Connectivity
echo "=== Network Connectivity ==="
check_network "Azure Management API" "https://management.azure.com"
check_network "NuGet.org" "https://api.nuget.org/v3/index.json"
check_network "npm registry" "https://registry.npmjs.org"
check_network "GitHub" "https://github.com"
echo ""

# Project Build
echo "=== Project Build ==="
if [ -f "NewFramework/CSharpTests/PlaywrightFramework.csproj" ]; then
    printf "%-40s" "Project builds: "
    cd NewFramework/CSharpTests
    if dotnet build --no-restore --verbosity quiet > /dev/null 2>&1; then
        echo -e "${GREEN}✓ PASS${NC}"
        ((PASS++))
    else
        echo -e "${RED}✗ FAIL${NC}"
        ((FAIL++))
        echo "  Run 'dotnet build' for details"
    fi
    cd ../..
else
    echo "Skipping build check (not in project directory)"
fi
echo ""

# Optional Tools
echo "=== Optional Tools ==="
check_version "Allure" "allure --version | head -n1" false
check "Azure CLI" "command -v az" false
check "Docker" "command -v docker" false
echo ""

# Disk Space
echo "=== Disk Space ==="
df -h . | tail -n1 | awk '{printf "%-40s %s available\n", "Disk space:", $4}'
echo ""

# Summary
echo "=========================================="
echo "  Summary"
echo "=========================================="
echo -e "${GREEN}Passed:${NC}  $PASS"
echo -e "${YELLOW}Warnings:${NC} $WARN"
echo -e "${RED}Failed:${NC}  $FAIL"
echo ""

if [ $FAIL -eq 0 ]; then
    echo -e "${GREEN}✓ All critical checks passed!${NC}"
    echo ""
    echo "You can now run tests:"
    echo "  cd NewFramework/CSharpTests"
    echo "  dotnet test"
    echo ""
    exit 0
else
    echo -e "${RED}✗ Some critical checks failed.${NC}"
    echo ""
    echo "Please fix the issues above before running tests."
    echo "See LINUX_SELF_HOSTED_AGENT_REQUIREMENTS.md for details."
    echo ""
    exit 1
fi