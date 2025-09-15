#!/usr/bin/env node

/**
 * VS Code Desktop Testing Framework - Example Usage
 * 
 * This script demonstrates how to use the VS Code Desktop testing framework
 * for Azure ML workspace automation testing.
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

// Colors for console output
const colors = {
  reset: '\x1b[0m',
  bright: '\x1b[1m',
  red: '\x1b[31m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  magenta: '\x1b[35m',
  cyan: '\x1b[36m'
};

function log(message, color = 'reset') {
  console.log(`${colors[color]}${message}${colors.reset}`);
}

function section(title) {
  log(`\n${'='.repeat(60)}`, 'cyan');
  log(`${title}`, 'bright');
  log(`${'='.repeat(60)}`, 'cyan');
}

function subsection(title) {
  log(`\n${'-'.repeat(40)}`, 'blue');
  log(`${title}`, 'blue');
  log(`${'-'.repeat(40)}`, 'blue');
}

function runCommand(command, description) {
  log(`\n📋 ${description}`, 'yellow');
  log(`💻 Command: ${command}`, 'cyan');
  
  try {
    const output = execSync(command, { 
      encoding: 'utf8', 
      stdio: 'pipe',
      cwd: process.cwd()
    });
    log(`✅ Success!`, 'green');
    return output;
  } catch (error) {
    log(`❌ Error: ${error.message}`, 'red');
    return null;
  }
}

function checkPrerequisites() {
  section('🔍 CHECKING PREREQUISITES');
  
  // Check Node.js version
  try {
    const nodeVersion = execSync('node --version', { encoding: 'utf8' }).trim();
    log(`✅ Node.js: ${nodeVersion}`, 'green');
  } catch (error) {
    log(`❌ Node.js not found`, 'red');
    return false;
  }
  
  // Check npm
  try {
    const npmVersion = execSync('npm --version', { encoding: 'utf8' }).trim();
    log(`✅ npm: ${npmVersion}`, 'green');
  } catch (error) {
    log(`❌ npm not found`, 'red');
    return false;
  }
  
  // Check if Playwright is installed
  try {
    execSync('npx playwright --version', { encoding: 'utf8', stdio: 'pipe' });
    log(`✅ Playwright is available`, 'green');
  } catch (error) {
    log(`⚠️  Playwright not found - will install`, 'yellow');
  }
  
  // Check test data files
  const testDataFiles = [
    'test-data/vscode-settings.json',
    'test-data/azure-ml-config.json',
    'test-data/sample-notebook.ipynb',
    'test-data/sample-python-script.py'
  ];
  
  let allFilesExist = true;
  testDataFiles.forEach(file => {
    if (fs.existsSync(file)) {
      log(`✅ ${file}`, 'green');
    } else {
      log(`❌ ${file} missing`, 'red');
      allFilesExist = false;
    }
  });
  
  return allFilesExist;
}

function setupEnvironment() {
  section('🛠️  SETTING UP ENVIRONMENT');
  
  // Install dependencies
  runCommand('npm install', 'Installing Node.js dependencies');
  
  // Install Playwright browsers
  runCommand('npx playwright install', 'Installing Playwright browsers');
  
  // Create test results directory
  if (!fs.existsSync('test-results')) {
    fs.mkdirSync('test-results', { recursive: true });
    log(`✅ Created test-results directory`, 'green');
  }
  
  // Create reports directory
  if (!fs.existsSync('test-results/reports')) {
    fs.mkdirSync('test-results/reports', { recursive: true });
    log(`✅ Created test-results/reports directory`, 'green');
  }
}

function demonstrateMockTesting() {
  section('🎭 MOCK TESTING DEMONSTRATION');
  
  subsection('Basic Mock Test');
  const mockEnv = {
    'NODE_ENV': 'test',
    'MOCK_VSCODE': 'true',
    'MOCK_AZURE_SERVICES': 'true',
    'SKIP_AZURE_AUTH': 'true',
    'AZURE_TENANT_ID': 'test-tenant-id',
    'AZURE_CLIENT_ID': 'test-client-id',
    'AZURE_CLIENT_SECRET': 'test-client-secret',
    'AZURE_SUBSCRIPTION_ID': 'test-subscription-id',
    'AZURE_RESOURCE_GROUP': 'test-resource-group',
    'AZURE_ML_WORKSPACE_NAME': 'test-workspace'
  };
  
  const envString = Object.entries(mockEnv)
    .map(([key, value]) => `${key}=${value}`)
    .join(' ');
  
  const mockCommand = `${envString} npx playwright test --grep "@electron.*@integration" --reporter=line --workers=1 --timeout=60000`;
  
  log(`\n🎯 Running mock tests with the following environment:`, 'yellow');
  Object.entries(mockEnv).forEach(([key, value]) => {
    log(`   ${key}=${value}`, 'cyan');
  });
  
  const output = runCommand(mockCommand, 'Running VS Code Desktop tests with mocks');
  
  if (output) {
    log(`\n📊 Test completed successfully!`, 'green');
    log(`Check test-results/ directory for detailed reports`, 'blue');
  }
}

function demonstrateTestCategories() {
  section('🏷️  TEST CATEGORIES DEMONSTRATION');
  
  const testCategories = [
    {
      name: 'All Electron Tests',
      command: 'npx playwright test --grep "@electron" --dry-run',
      description: 'List all VS Code Desktop tests'
    },
    {
      name: 'Integration Tests Only',
      command: 'npx playwright test --grep "@electron.*@integration" --dry-run',
      description: 'List integration tests'
    },
    {
      name: 'Notebook Tests Only',
      command: 'npx playwright test --grep "@electron.*@notebook" --dry-run',
      description: 'List notebook-specific tests'
    },
    {
      name: 'Remote Compute Tests',
      command: 'npx playwright test --grep "@electron.*@remote" --dry-run',
      description: 'List remote compute tests'
    }
  ];
  
  testCategories.forEach(category => {
    subsection(category.name);
    runCommand(category.command, category.description);
  });
}

function demonstrateReporting() {
  section('📊 REPORTING DEMONSTRATION');
  
  // Check if test results exist
  if (fs.existsSync('test-results')) {
    log(`\n📁 Test results directory exists`, 'green');
    
    // List available reports
    const reportFiles = [
      'test-results/reports/test-summary.json',
      'test-results/reports/performance-report.json',
      'test-results/reports/resource-usage-report.json'
    ];
    
    reportFiles.forEach(file => {
      if (fs.existsSync(file)) {
        log(`✅ ${file}`, 'green');
        try {
          const content = JSON.parse(fs.readFileSync(file, 'utf8'));
          log(`   📄 Contains ${Object.keys(content).length} properties`, 'blue');
        } catch (error) {
          log(`   ⚠️  Could not parse JSON`, 'yellow');
        }
      } else {
        log(`❌ ${file} not found`, 'red');
      }
    });
    
    // Demonstrate HTML report
    subsection('HTML Report');
    log(`\n💡 To view the HTML report, run:`, 'yellow');
    log(`   npx playwright show-report`, 'cyan');
    
  } else {
    log(`\n⚠️  No test results found. Run tests first.`, 'yellow');
  }
}

function demonstrateCustomization() {
  section('🎨 CUSTOMIZATION EXAMPLES');
  
  subsection('Custom Environment Variables');
  log(`\n💡 You can customize test behavior with environment variables:`, 'yellow');
  
  const customizations = [
    { var: 'TEST_TIMEOUT=120000', desc: 'Increase test timeout to 2 minutes' },
    { var: 'TEST_WORKERS=2', desc: 'Run tests in parallel with 2 workers' },
    { var: 'DEBUG=*', desc: 'Enable verbose debug logging' },
    { var: 'HEADLESS=false', desc: 'Run tests with visible browser windows' }
  ];
  
  customizations.forEach(({ var: envVar, desc }) => {
    log(`   ${envVar}`, 'cyan');
    log(`     ${desc}`, 'blue');
  });
  
  subsection('Custom Test Commands');
  log(`\n💡 Example custom test commands:`, 'yellow');
  
  const customCommands = [
    {
      command: 'npx playwright test --grep "specific test name" --debug',
      desc: 'Run specific test in debug mode'
    },
    {
      command: 'npx playwright test --project=chromium --grep "@electron"',
      desc: 'Run tests only in Chromium browser'
    },
    {
      command: 'npx playwright test --trace=on --video=on --grep "@electron"',
      desc: 'Run tests with trace and video recording'
    }
  ];
  
  customCommands.forEach(({ command, desc }) => {
    log(`   ${command}`, 'cyan');
    log(`     ${desc}`, 'blue');
  });
}

function demonstrateDebugging() {
  section('🐛 DEBUGGING DEMONSTRATION');
  
  subsection('Debug Mode');
  log(`\n💡 To run tests in debug mode:`, 'yellow');
  log(`   npx playwright test --grep "test name" --debug`, 'cyan');
  log(`     Opens browser in debug mode with step-by-step execution`, 'blue');
  
  subsection('Verbose Logging');
  log(`\n💡 To enable verbose logging:`, 'yellow');
  log(`   DEBUG=* npx playwright test --grep "@electron"`, 'cyan');
  log(`     Shows detailed execution logs`, 'blue');
  
  subsection('Trace and Video');
  log(`\n💡 To capture traces and videos:`, 'yellow');
  log(`   npx playwright test --trace=on --video=on --grep "@electron"`, 'cyan');
  log(`     Records detailed execution traces and videos`, 'blue');
  
  subsection('Common Issues and Solutions');
  const issues = [
    {
      issue: 'Test timeouts',
      solution: 'Increase timeout with --timeout=120000'
    },
    {
      issue: 'Missing mock methods',
      solution: 'Add methods to src/electron/vscode-electron-mock.ts'
    },
    {
      issue: 'Authentication failures',
      solution: 'Check environment variables with echo $AZURE_TENANT_ID'
    },
    {
      issue: 'Test data issues',
      solution: 'Run node scripts/validate-test-data.js'
    }
  ];
  
  issues.forEach(({ issue, solution }) => {
    log(`   ❓ ${issue}`, 'yellow');
    log(`     💡 ${solution}`, 'green');
  });
}

function showUsageExamples() {
  section('📚 USAGE EXAMPLES');
  
  const examples = [
    {
      title: 'Development Testing (Mock Mode)',
      description: 'Fast testing with mocked Azure services',
      command: `NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \\
npx playwright test --grep "@electron.*@integration" --reporter=line`
    },
    {
      title: 'Production Validation (Real Azure)',
      description: 'End-to-end testing with real Azure services',
      setup: `export AZURE_TENANT_ID=your-tenant-id
export AZURE_CLIENT_ID=your-client-id
export AZURE_CLIENT_SECRET=your-client-secret
export AZURE_SUBSCRIPTION_ID=your-subscription-id
export AZURE_RESOURCE_GROUP=your-resource-group
export AZURE_ML_WORKSPACE_NAME=your-workspace-name`,
      command: `NODE_ENV=production npx playwright test --grep "@electron.*@integration"`
    },
    {
      title: 'CI/CD Pipeline',
      description: 'Automated testing in continuous integration',
      command: `NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \\
npx playwright test --grep "@electron.*@integration" --reporter=json`
    },
    {
      title: 'Debug Single Test',
      description: 'Debug specific test with step-by-step execution',
      command: `NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \\
npx playwright test --grep "should launch VS Code and connect" --debug`
    }
  ];
  
  examples.forEach(({ title, description, setup, command }) => {
    subsection(title);
    log(`📝 ${description}`, 'blue');
    
    if (setup) {
      log(`\n🔧 Setup:`, 'yellow');
      setup.split('\n').forEach(line => {
        log(`   ${line}`, 'cyan');
      });
    }
    
    log(`\n💻 Command:`, 'yellow');
    log(`   ${command}`, 'cyan');
  });
}

function showNextSteps() {
  section('🚀 NEXT STEPS');
  
  const steps = [
    {
      step: '1. Run Mock Tests',
      action: 'Execute the mock testing example above to verify setup'
    },
    {
      step: '2. Explore Test Results',
      action: 'Check test-results/ directory and open HTML report'
    },
    {
      step: '3. Customize Tests',
      action: 'Modify test data in test-data/ directory for your needs'
    },
    {
      step: '4. Add New Tests',
      action: 'Create new test files in src/tests/electron/ directory'
    },
    {
      step: '5. Real Azure Testing',
      action: 'Set up Azure credentials and run production validation'
    },
    {
      step: '6. CI/CD Integration',
      action: 'Add tests to your CI/CD pipeline using the examples above'
    }
  ];
  
  steps.forEach(({ step, action }) => {
    log(`\n${step}`, 'bright');
    log(`   ${action}`, 'blue');
  });
  
  log(`\n📖 For detailed documentation, see:`, 'yellow');
  log(`   docs/VSCODE_DESKTOP_TESTING_GUIDE.md`, 'cyan');
  log(`   docs/QUICK_REFERENCE.md`, 'cyan');
}

function main() {
  log(`
██╗   ██╗███████╗     ██████╗ ██████╗ ██████╗ ███████╗
██║   ██║██╔════╝    ██╔════╝██╔═══██╗██╔══██╗██╔════╝
██║   ██║███████╗    ██║     ██║   ██║██║  ██║█████╗  
╚██╗ ██╔╝╚════██║    ██║     ██║   ██║██║  ██║██╔══╝  
 ╚████╔╝ ███████║    ╚██████╗╚██████╔╝██████╔╝███████╗
  ╚═══╝  ╚══════╝     ╚═════╝ ╚═════╝ ╚═════╝ ╚══════╝
                                                       
████████╗███████╗███████╗████████╗██╗███╗   ██╗ ██████╗ 
╚══██╔══╝██╔════╝██╔════╝╚══██╔══╝██║████╗  ██║██╔════╝ 
   ██║   █████╗  ███████╗   ██║   ██║██╔██╗ ██║██║  ███╗
   ██║   ██╔══╝  ╚════██║   ██║   ██║██║╚██╗██║██║   ██║
   ██║   ███████╗███████║   ██║   ██║██║ ╚████║╚██████╔╝
   ╚═╝   ╚══════╝╚══════╝   ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ 
`, 'magenta');
  
  log('VS Code Desktop Testing Framework - Example Usage', 'bright');
  log('Azure ML Workspace Automation Testing', 'blue');
  
  // Check if we should run the full demo
  const args = process.argv.slice(2);
  const runDemo = args.includes('--demo') || args.includes('-d');
  const runTests = args.includes('--test') || args.includes('-t');
  const showHelp = args.includes('--help') || args.includes('-h');
  
  if (showHelp) {
    log(`\nUsage: node examples/example-usage.js [options]`, 'yellow');
    log(`Options:`, 'yellow');
    log(`  --demo, -d    Run full demonstration`, 'cyan');
    log(`  --test, -t    Run mock tests`, 'cyan');
    log(`  --help, -h    Show this help`, 'cyan');
    return;
  }
  
  // Always check prerequisites
  const prereqsOk = checkPrerequisites();
  
  if (!prereqsOk) {
    log(`\n⚠️  Some prerequisites are missing. Please install them first.`, 'yellow');
    return;
  }
  
  if (runDemo) {
    setupEnvironment();
    demonstrateMockTesting();
    demonstrateTestCategories();
    demonstrateReporting();
    demonstrateCustomization();
    demonstrateDebugging();
  }
  
  if (runTests) {
    demonstrateMockTesting();
  }
  
  // Always show usage examples and next steps
  showUsageExamples();
  showNextSteps();
  
  log(`\n🎉 Example usage demonstration complete!`, 'green');
  log(`Run with --demo flag to see full demonstration`, 'blue');
  log(`Run with --test flag to execute mock tests`, 'blue');
}

// Run the main function
if (require.main === module) {
  main();
}

module.exports = {
  checkPrerequisites,
  setupEnvironment,
  demonstrateMockTesting,
  demonstrateTestCategories,
  demonstrateReporting,
  demonstrateCustomization,
  demonstrateDebugging,
  showUsageExamples,
  showNextSteps
};