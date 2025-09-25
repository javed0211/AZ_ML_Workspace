#!/bin/bash

# Azure ML Workspace - Old Framework Cleanup Script
# This script safely removes the old Screenplay-based framework
# after successful migration to the new Playwright framework

set -e

echo "🧹 Azure ML Workspace - Old Framework Cleanup"
echo "=============================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to prompt for confirmation
confirm() {
    while true; do
        read -p "$1 [y/N]: " yn
        case $yn in
            [Yy]* ) return 0;;
            [Nn]* ) return 1;;
            * ) echo "Please answer yes or no.";;
        esac
    done
}

# Check if new framework exists and is validated
echo -e "${BLUE}🔍 Checking new framework status...${NC}"
if [ ! -d "NewFramework" ]; then
    echo -e "${RED}❌ Error: NewFramework directory not found!${NC}"
    echo "Please ensure the new framework is properly set up before cleanup."
    exit 1
fi

if [ ! -f "NewFramework/validate-setup.sh" ]; then
    echo -e "${RED}❌ Error: New framework validation script not found!${NC}"
    exit 1
fi

# Run validation
echo -e "${BLUE}🔍 Running new framework validation...${NC}"
cd NewFramework
if ! ./validate-setup.sh > /dev/null 2>&1; then
    echo -e "${RED}❌ Error: New framework validation failed!${NC}"
    echo "Please fix validation issues before cleanup."
    exit 1
fi
cd ..

echo -e "${GREEN}✅ New framework validation passed${NC}"
echo ""

# Show what will be deleted
echo -e "${YELLOW}📋 The following old framework components will be deleted:${NC}"
echo ""
echo "🗂️  Directories:"
echo "   - AzureMLWorkspace.Tests/     (Old Screenplay framework)"
echo "   - TestResults/               (Old test results)"
echo "   - ConfigDebug/               (Old debug config)"
echo ""
echo "📄 Files:"
echo "   - AzureMLWorkspace.sln       (Old solution file)"
echo "   - ScenarioDemo.cs            (Old demo files)"
echo "   - ScenarioDemo.csproj"
echo "   - ScenarioRunner.cs          (Old runner files)"
echo "   - ScenarioRunner.csproj"
echo "   - ConfigTest.cs              (Old config test)"
echo "   - Execute-Scenario.ps1       (Old PowerShell script)"
echo "   - run-tests.ps1              (Old test runners)"
echo "   - run-tests.sh"
echo "   - Sample_Test_Report.html    (Old sample report)"
echo "   - PowerPoint_Content.txt     (Old content file)"
echo ""
echo -e "${GREEN}🛡️  The following will be PRESERVED:${NC}"
echo "   - NewFramework/              (New framework)"
echo "   - docs/                      (Documentation)"
echo "   - test-data/                 (Test data files)"
echo "   - azure-pipelines.yml        (Updated pipeline)"
echo "   - .github/                   (GitHub workflows)"
echo "   - .gitignore, LICENSE        (Git and license files)"
echo "   - MIGRATION_SUMMARY.md       (Migration documentation)"
echo ""

# Final confirmation
if ! confirm "⚠️  Are you sure you want to delete the old framework components?"; then
    echo -e "${YELLOW}🚫 Cleanup cancelled by user${NC}"
    exit 0
fi

echo ""
echo -e "${BLUE}🧹 Starting cleanup...${NC}"

# Create backup directory
BACKUP_DIR="old-framework-backup-$(date +%Y%m%d-%H%M%S)"
echo -e "${BLUE}📦 Creating backup in ${BACKUP_DIR}...${NC}"
mkdir -p "$BACKUP_DIR"

# Function to safely remove with backup
safe_remove() {
    local item="$1"
    if [ -e "$item" ]; then
        echo -e "${YELLOW}🗑️  Removing: $item${NC}"
        # Create backup
        if [ -d "$item" ]; then
            cp -r "$item" "$BACKUP_DIR/" 2>/dev/null || true
        else
            cp "$item" "$BACKUP_DIR/" 2>/dev/null || true
        fi
        # Remove original
        rm -rf "$item"
        echo -e "${GREEN}✅ Removed: $item${NC}"
    else
        echo -e "${YELLOW}⚠️  Not found: $item${NC}"
    fi
}

# Remove old framework components
echo ""
echo -e "${BLUE}🗂️  Removing directories...${NC}"
safe_remove "AzureMLWorkspace.Tests"
safe_remove "TestResults"
safe_remove "ConfigDebug"

echo ""
echo -e "${BLUE}📄 Removing files...${NC}"
safe_remove "AzureMLWorkspace.sln"
safe_remove "ScenarioDemo.cs"
safe_remove "ScenarioDemo.csproj"
safe_remove "ScenarioRunner.cs"
safe_remove "ScenarioRunner.csproj"
safe_remove "ConfigTest.cs"
safe_remove "Execute-Scenario.ps1"
safe_remove "run-tests.ps1"
safe_remove "run-tests.sh"
safe_remove "Sample_Test_Report.html"
safe_remove "PowerPoint_Content.txt"

echo ""
echo -e "${GREEN}🎉 Cleanup completed successfully!${NC}"
echo ""
echo -e "${BLUE}📊 Summary:${NC}"
echo -e "   ✅ Old framework components removed"
echo -e "   ✅ Backup created in: ${BACKUP_DIR}"
echo -e "   ✅ New framework preserved and validated"
echo -e "   ✅ Essential files preserved"
echo ""
echo -e "${GREEN}🚀 You can now use the new framework exclusively:${NC}"
echo -e "   cd NewFramework"
echo -e "   ./run-azure-ml-tests.sh --help"
echo ""
echo -e "${YELLOW}💡 Tip: Keep the backup directory until you're confident everything works correctly${NC}"
echo -e "${YELLOW}    Then you can remove it with: rm -rf ${BACKUP_DIR}${NC}"