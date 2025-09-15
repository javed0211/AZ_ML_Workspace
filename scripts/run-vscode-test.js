#!/usr/bin/env node

/**
 * VS Code Desktop Test Runner
 * Executes VS Code desktop tests with proper setup
 */

const { execSync, spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

async function main() {
  console.log('🚀 VS Code Desktop Test Runner\n');

  try {
    // Check if test files exist
    const testFile = path.join(__dirname, '..', 'src', 'tests', 'electron', 'vscode-desktop.spec.ts');
    if (!fs.existsSync(testFile)) {
      throw new Error(`Test file not found: ${testFile}`);
    }

    console.log('✓ Test file found');

    // Check if test data exists
    const testDataDir = path.join(__dirname, '..', 'test-data');
    if (!fs.existsSync(testDataDir)) {
      throw new Error(`Test data directory not found: ${testDataDir}`);
    }

    console.log('✓ Test data directory found');

    // List test data files
    const testDataFiles = fs.readdirSync(testDataDir);
    console.log('📁 Test data files:');
    testDataFiles.forEach(file => {
      console.log(`   - ${file}`);
    });

    // Set environment variables for testing
    const testEnv = {
      ...process.env,
      NODE_ENV: 'test',
      TEST_MODE: 'vscode-desktop',
      TEST_DATA_DIR: testDataDir,
      PLAYWRIGHT_BROWSERS_PATH: path.join(__dirname, '..', 'node_modules', 'playwright'),
    };

    console.log('\n🔧 Environment setup:');
    console.log(`   NODE_ENV: ${testEnv.NODE_ENV}`);
    console.log(`   TEST_MODE: ${testEnv.TEST_MODE}`);
    console.log(`   TEST_DATA_DIR: ${testEnv.TEST_DATA_DIR}`);

    // Run the test
    console.log('\n🧪 Running VS Code Desktop tests...\n');

    const playwrightArgs = [
      'npx', 'playwright', 'test',
      '--grep', '@electron',
      '--reporter', 'line,html',
      '--output-dir', './test-results/vscode-desktop',
    ];

    // Add headed mode for better visibility
    playwrightArgs.push('--headed');

    console.log(`Executing: ${playwrightArgs.join(' ')}`);

    try {
      execSync(playwrightArgs.join(' '), {
        stdio: 'inherit',
        env: testEnv,
        cwd: path.join(__dirname, '..'),
      });

      console.log('\n✅ VS Code Desktop tests completed successfully!');

    } catch (testError) {
      console.error('\n❌ VS Code Desktop tests failed:', testError.message);
      
      // Try to show test results if available
      const resultsDir = path.join(__dirname, '..', 'test-results', 'vscode-desktop');
      if (fs.existsSync(resultsDir)) {
        console.log('\n📊 Test results available in:', resultsDir);
      }

      throw testError;
    }

  } catch (error) {
    console.error('\n💥 Test runner failed:', error.message);
    process.exit(1);
  }
}

// Helper function to check if VS Code is installed
function checkVSCodeInstallation() {
  console.log('🔍 Checking VS Code installation...');
  
  const possiblePaths = [
    '/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code',
    '/usr/local/bin/code',
    '/opt/homebrew/bin/code',
  ];

  for (const codePath of possiblePaths) {
    if (fs.existsSync(codePath)) {
      console.log(`✓ VS Code found at: ${codePath}`);
      return codePath;
    }
  }

  console.log('⚠️  VS Code not found in standard locations');
  console.log('   Please ensure VS Code is installed and accessible');
  return null;
}

// Helper function to create test output directory
function ensureTestOutputDir() {
  const outputDir = path.join(__dirname, '..', 'test-results', 'vscode-desktop');
  if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
    console.log(`✓ Created test output directory: ${outputDir}`);
  }
  return outputDir;
}

// Run the script if executed directly
if (require.main === module) {
  // Check VS Code installation
  checkVSCodeInstallation();
  
  // Ensure output directory exists
  ensureTestOutputDir();
  
  // Run main function
  main().catch(console.error);
}

module.exports = { main, checkVSCodeInstallation, ensureTestOutputDir };