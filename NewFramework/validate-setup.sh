#!/bin/bash

# Azure ML Workspace Test Framework Validation Script
# This script validates that all components are properly set up

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to log messages
log_info() {
    echo -e "${GREEN}[✓]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

log_error() {
    echo -e "${RED}[✗]${NC} $1"
}

log_header() {
    echo -e "${BLUE}$1${NC}"
}

# Validation counters
PASSED=0
FAILED=0

# Function to validate file exists
validate_file() {
    local file_path="$1"
    local description="$2"
    
    if [ -f "$file_path" ]; then
        log_info "$description exists"
        ((PASSED++))
        return 0
    else
        log_error "$description missing: $file_path"
        ((FAILED++))
        return 1
    fi
}

# Function to validate directory exists
validate_directory() {
    local dir_path="$1"
    local description="$2"
    
    if [ -d "$dir_path" ]; then
        log_info "$description exists"
        ((PASSED++))
        return 0
    else
        log_error "$description missing: $dir_path"
        ((FAILED++))
        return 1
    fi
}

# Function to validate JSON file
validate_json() {
    local json_file="$1"
    local description="$2"
    
    if [ -f "$json_file" ]; then
        if python3 -m json.tool "$json_file" > /dev/null 2>&1; then
            log_info "$description is valid JSON"
            ((PASSED++))
            return 0
        else
            log_error "$description has invalid JSON syntax"
            ((FAILED++))
            return 1
        fi
    else
        log_error "$description file missing"
        ((FAILED++))
        return 1
    fi
}

# Start validation
log_header "🔍 Azure ML Workspace Test Framework Validation"
log_header "================================================"

# Validate core framework structure
log_header "\n📁 Framework Structure Validation"
validate_directory "TypeScriptTests" "TypeScript Tests Directory"
validate_directory "CSharpTests" "C# Tests Directory"
validate_directory "Utils" "Utilities Directory"
validate_directory "Config" "Configuration Directory"
validate_directory "Documentation" "Documentation Directory"
validate_directory "Reports" "Reports Directory"

# Validate TypeScript files
log_header "\n📝 TypeScript Files Validation"
validate_file "TypeScriptTests/azure-ml-workspace.spec.ts" "Azure ML TypeScript Test Suite"
validate_file "Utils/AzureMLUtils.ts" "Azure ML TypeScript Utilities"
validate_file "Utils/ConfigManager.ts" "TypeScript Configuration Manager"
validate_file "playwright.config.ts" "Playwright Configuration"
validate_file "package.json" "Node.js Package Configuration"
validate_file "tsconfig.json" "TypeScript Configuration"

# Validate C# files
log_header "\n🔷 C# Files Validation"
validate_file "CSharpTests/Tests/AzureMLWorkspaceTests.cs" "Azure ML C# Test Suite"
validate_file "CSharpTests/Utils/AzureMLUtils.cs" "Azure ML C# Utilities"
validate_file "CSharpTests/Utils/ConfigManager.cs" "C# Configuration Manager"
validate_file "CSharpTests/PlaywrightFramework.csproj" "C# Project File"

# Validate configuration files
log_header "\n⚙️  Configuration Files Validation"
validate_json "Config/appsettings.json" "Application Settings"

# Validate CLI runners
log_header "\n🚀 CLI Runners Validation"
validate_file "run-azure-ml-tests.sh" "Azure ML Test Runner Script"
validate_file "run-tests.sh" "General Test Runner Script"

# Check if CLI runners are executable
if [ -x "run-azure-ml-tests.sh" ]; then
    log_info "Azure ML Test Runner is executable"
    ((PASSED++))
else
    log_warning "Azure ML Test Runner is not executable (run: chmod +x run-azure-ml-tests.sh)"
    ((FAILED++))
fi

# Validate documentation
log_header "\n📚 Documentation Validation"
validate_file "Documentation/AzureML-Automation-Guide.md" "Azure ML Automation Guide"
validate_file "README.md" "Framework README"

# Check Node.js dependencies
log_header "\n📦 Dependencies Validation"
if [ -f "package.json" ]; then
    if command -v npm >/dev/null 2>&1; then
        log_info "npm is available"
        ((PASSED++))
        
        if [ -d "node_modules" ]; then
            log_info "Node.js dependencies are installed"
            ((PASSED++))
        else
            log_warning "Node.js dependencies not installed (run: npm install)"
            ((FAILED++))
        fi
    else
        log_error "npm is not available"
        ((FAILED++))
    fi
fi

# Check .NET dependencies
if [ -f "CSharpTests/PlaywrightFramework.csproj" ]; then
    if command -v dotnet >/dev/null 2>&1; then
        log_info ".NET CLI is available"
        ((PASSED++))
        
        if [ -d "CSharpTests/bin" ]; then
            log_info "C# project is built"
            ((PASSED++))
        else
            log_warning "C# project not built (run: cd CSharpTests && dotnet build)"
            ((FAILED++))
        fi
    else
        log_error ".NET CLI is not available"
        ((FAILED++))
    fi
fi

# Validate test scenarios mapping
log_header "\n🎯 Test Scenarios Validation"

# Check if all scenarios from original feature file are covered
declare -a scenarios=(
    "Access Azure ML Workspace"
    "Start Compute Instance"
    "Stop Compute Instance"
    "Manage Multiple Compute Instances"
    "VS Code Desktop Integration"
)

for scenario in "${scenarios[@]}"; do
    if grep -q "$scenario" "TypeScriptTests/azure-ml-workspace.spec.ts" 2>/dev/null; then
        log_info "Scenario automated: $scenario"
        ((PASSED++))
    else
        log_error "Scenario missing: $scenario"
        ((FAILED++))
    fi
done

# Check utility functions
log_header "\n🔧 Utility Functions Validation"

declare -a utility_functions=(
    "navigateToWorkspace"
    "handleAuthentication"
    "startComputeInstance"
    "stopComputeInstance"
    "waitForComputeState"
    "startVSCodeDesktop"
)

for func in "${utility_functions[@]}"; do
    if grep -q "$func" "Utils/AzureMLUtils.ts" 2>/dev/null; then
        log_info "Utility function exists: $func"
        ((PASSED++))
    else
        log_error "Utility function missing: $func"
        ((FAILED++))
    fi
done

# Final validation summary
log_header "\n📊 Validation Summary"
log_header "===================="

TOTAL=$((PASSED + FAILED))
SUCCESS_RATE=$((PASSED * 100 / TOTAL))

echo -e "Total Checks: $TOTAL"
echo -e "${GREEN}Passed: $PASSED${NC}"
echo -e "${RED}Failed: $FAILED${NC}"
echo -e "Success Rate: $SUCCESS_RATE%"

if [ $FAILED -eq 0 ]; then
    log_header "\n🎉 All validations passed! The Azure ML Workspace test framework is properly set up."
    echo -e "\n${GREEN}You can now run tests using:${NC}"
    echo -e "  ${BLUE}./run-azure-ml-tests.sh --headed${NC}  # Run all tests in headed mode"
    echo -e "  ${BLUE}./run-azure-ml-tests.sh --test \"workspace access\"${NC}  # Run specific test"
    echo -e "  ${BLUE}./run-azure-ml-tests.sh --help${NC}  # See all options"
    exit 0
else
    log_header "\n⚠️  Some validations failed. Please address the issues above before running tests."
    exit 1
fi