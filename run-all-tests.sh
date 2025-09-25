#!/bin/bash

# Azure ML Test Framework - Comprehensive Test Runner
# Runs both C# and TypeScript tests with detailed reporting

set -e

echo "🧪 Azure ML Test Framework - Running All Tests"
echo "=============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_status() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Test results tracking
CSHARP_TESTS_PASSED=false
TYPESCRIPT_TESTS_PASSED=false
BDD_TESTS_PASSED=false

# Create reports directory
mkdir -p NewFramework/Reports/unified-test-results
REPORT_DIR="NewFramework/Reports/unified-test-results"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

echo "📊 Test Results will be saved to: $REPORT_DIR"
echo ""

# Step 1: Run C# NUnit Tests
print_status "Running C# NUnit Tests..."
echo "=========================="

if dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj \
    --configuration Release \
    --logger "trx;LogFileName=csharp-nunit-results-$TIMESTAMP.trx" \
    --logger "html;LogFileName=csharp-nunit-results-$TIMESTAMP.html" \
    --results-directory "$REPORT_DIR" \
    --filter "Category!=BDD" \
    --verbosity normal; then
    print_success "✅ C# NUnit Tests PASSED"
    CSHARP_TESTS_PASSED=true
else
    print_error "❌ C# NUnit Tests FAILED"
    CSHARP_TESTS_PASSED=false
fi

echo ""

# Step 2: Run C# BDD Tests
print_status "Running C# BDD Tests (Reqnroll)..."
echo "=================================="

if dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj \
    --configuration Release \
    --logger "trx;LogFileName=csharp-bdd-results-$TIMESTAMP.trx" \
    --logger "html;LogFileName=csharp-bdd-results-$TIMESTAMP.html" \
    --results-directory "$REPORT_DIR" \
    --filter "Category=BDD" \
    --verbosity normal; then
    print_success "✅ C# BDD Tests PASSED"
    BDD_TESTS_PASSED=true
else
    print_error "❌ C# BDD Tests FAILED"
    BDD_TESTS_PASSED=false
fi

echo ""

# Step 3: Run TypeScript Tests
print_status "Running TypeScript Tests (Playwright)..."
echo "========================================"

cd NewFramework

if npm test -- --reporter=html --output-folder="../$REPORT_DIR/typescript-results-$TIMESTAMP"; then
    print_success "✅ TypeScript Tests PASSED"
    TYPESCRIPT_TESTS_PASSED=true
else
    print_error "❌ TypeScript Tests FAILED"
    TYPESCRIPT_TESTS_PASSED=false
fi

cd ..

echo ""

# Step 4: Generate Unified Test Report
print_status "Generating Unified Test Report..."
echo "================================="

cat > "$REPORT_DIR/unified-test-summary-$TIMESTAMP.html" << EOF
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Azure ML Test Framework - Unified Test Results</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { text-align: center; margin-bottom: 30px; }
        .header h1 { color: #2c3e50; margin-bottom: 10px; }
        .timestamp { color: #7f8c8d; font-size: 14px; }
        .test-section { margin: 20px 0; padding: 20px; border-radius: 8px; border-left: 5px solid #3498db; }
        .test-section h2 { margin-top: 0; color: #2c3e50; }
        .status-passed { background-color: #d4edda; border-left-color: #28a745; }
        .status-failed { background-color: #f8d7da; border-left-color: #dc3545; }
        .status-icon { font-size: 24px; margin-right: 10px; }
        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin: 30px 0; }
        .summary-card { padding: 20px; border-radius: 8px; text-align: center; }
        .summary-card h3 { margin: 0 0 10px 0; }
        .summary-card .number { font-size: 36px; font-weight: bold; margin: 10px 0; }
        .passed-card { background-color: #d4edda; color: #155724; }
        .failed-card { background-color: #f8d7da; color: #721c24; }
        .info-card { background-color: #d1ecf1; color: #0c5460; }
        .footer { text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; color: #7f8c8d; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>🧪 Azure ML Test Framework</h1>
            <h2>Unified Test Results</h2>
            <div class="timestamp">Generated on: $(date)</div>
        </div>

        <div class="summary">
            <div class="summary-card $([ "$CSHARP_TESTS_PASSED" = true ] && echo "passed-card" || echo "failed-card")">
                <h3>C# NUnit Tests</h3>
                <div class="number">$([ "$CSHARP_TESTS_PASSED" = true ] && echo "✅" || echo "❌")</div>
                <div>$([ "$CSHARP_TESTS_PASSED" = true ] && echo "PASSED" || echo "FAILED")</div>
            </div>
            <div class="summary-card $([ "$BDD_TESTS_PASSED" = true ] && echo "passed-card" || echo "failed-card")">
                <h3>C# BDD Tests</h3>
                <div class="number">$([ "$BDD_TESTS_PASSED" = true ] && echo "✅" || echo "❌")</div>
                <div>$([ "$BDD_TESTS_PASSED" = true ] && echo "PASSED" || echo "FAILED")</div>
            </div>
            <div class="summary-card $([ "$TYPESCRIPT_TESTS_PASSED" = true ] && echo "passed-card" || echo "failed-card")">
                <h3>TypeScript Tests</h3>
                <div class="number">$([ "$TYPESCRIPT_TESTS_PASSED" = true ] && echo "✅" || echo "❌")</div>
                <div>$([ "$TYPESCRIPT_TESTS_PASSED" = true ] && echo "PASSED" || echo "FAILED")</div>
            </div>
        </div>

        <div class="test-section $([ "$CSHARP_TESTS_PASSED" = true ] && echo "status-passed" || echo "status-failed")">
            <h2><span class="status-icon">$([ "$CSHARP_TESTS_PASSED" = true ] && echo "✅" || echo "❌")</span>C# NUnit Tests</h2>
            <p><strong>Framework:</strong> .NET 8.0 + NUnit + Playwright</p>
            <p><strong>Test Files:</strong> AzureMLWorkspaceTests.cs</p>
            <p><strong>Features:</strong> PIM Role Activation, Workspace Access, Compute Management, VS Code Integration</p>
            <p><strong>Status:</strong> $([ "$CSHARP_TESTS_PASSED" = true ] && echo "PASSED" || echo "FAILED")</p>
        </div>

        <div class="test-section $([ "$BDD_TESTS_PASSED" = true ] && echo "status-passed" || echo "status-failed")">
            <h2><span class="status-icon">$([ "$BDD_TESTS_PASSED" = true ] && echo "✅" || echo "❌")</span>C# BDD Tests</h2>
            <p><strong>Framework:</strong> Reqnroll (SpecFlow successor) + Gherkin</p>
            <p><strong>Feature Files:</strong> AzureMLWorkspace.feature, AzureAISearch.feature</p>
            <p><strong>Step Definitions:</strong> 25 fully implemented steps</p>
            <p><strong>Status:</strong> $([ "$BDD_TESTS_PASSED" = true ] && echo "PASSED" || echo "FAILED")</p>
        </div>

        <div class="test-section $([ "$TYPESCRIPT_TESTS_PASSED" = true ] && echo "status-passed" || echo "status-failed")">
            <h2><span class="status-icon">$([ "$TYPESCRIPT_TESTS_PASSED" = true ] && echo "✅" || echo "❌")</span>TypeScript Tests</h2>
            <p><strong>Framework:</strong> Playwright + TypeScript</p>
            <p><strong>Test Files:</strong> azure-ml-workspace.spec.ts, example-web.spec.ts, google-search.spec.ts</p>
            <p><strong>Features:</strong> Cross-browser testing, Electron app testing, Web automation</p>
            <p><strong>Status:</strong> $([ "$TYPESCRIPT_TESTS_PASSED" = true ] && echo "PASSED" || echo "FAILED")</p>
        </div>

        <div class="footer">
            <p>Azure ML Test Framework v2.0 | Generated by Unified Test Runner</p>
            <p>For detailed logs, check individual test result files in this directory</p>
        </div>
    </div>
</body>
</html>
EOF

print_success "📊 Unified test report generated: $REPORT_DIR/unified-test-summary-$TIMESTAMP.html"

echo ""
echo "📋 Test Execution Summary:"
echo "========================="
echo "C# NUnit Tests:    $([ "$CSHARP_TESTS_PASSED" = true ] && echo "✅ PASSED" || echo "❌ FAILED")"
echo "C# BDD Tests:      $([ "$BDD_TESTS_PASSED" = true ] && echo "✅ PASSED" || echo "❌ FAILED")"
echo "TypeScript Tests:  $([ "$TYPESCRIPT_TESTS_PASSED" = true ] && echo "✅ PASSED" || echo "❌ FAILED")"

# Overall result
if [ "$CSHARP_TESTS_PASSED" = true ] && [ "$BDD_TESTS_PASSED" = true ] && [ "$TYPESCRIPT_TESTS_PASSED" = true ]; then
    echo ""
    print_success "🎉 ALL TESTS PASSED! Your Azure ML Test Framework is working perfectly!"
    exit 0
else
    echo ""
    print_error "❌ Some tests failed. Check the detailed reports for more information."
    echo ""
    echo "📁 Detailed reports available in: $REPORT_DIR"
    echo "🌐 Open unified-test-summary-$TIMESTAMP.html in your browser for a complete overview"
    exit 1
fi