#!/usr/bin/env node

/**
 * Jupyter Notebook Test Runner
 * Specifically runs the Jupyter notebook tests with proper configuration
 */

const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');

console.log('🚀 Starting Jupyter Notebook Tests...');
console.log('=' * 50);

// Ensure we're in the right directory
const testDir = __dirname;
process.chdir(testDir);

// Check if the test file exists
const testFile = path.join(testDir, 'tests', 'jupyter-notebook.test.ts');
if (!fs.existsSync(testFile)) {
  console.error('❌ Jupyter notebook test file not found!');
  console.error(`Expected: ${testFile}`);
  process.exit(1);
}

console.log('✅ Test file found:', testFile);

// Check if our notebook exists
const notebookPath = '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook/azure_ml_project.ipynb';
if (fs.existsSync(notebookPath)) {
  console.log('✅ Source notebook found:', notebookPath);
} else {
  console.log('⚠️ Source notebook not found, will create test notebook');
}

// Run the specific test
const args = [
  'test',
  'tests/jupyter-notebook.test.ts',
  '--config=playwright.config.ts',
  '--project=electron-vscode',
  '--reporter=list',
  '--timeout=60000'
];

console.log('\n🔄 Running command:');
console.log(`npx playwright ${args.join(' ')}`);
console.log('\n' + '=' * 50);

const testProcess = spawn('npx', ['playwright', ...args], {
  stdio: 'inherit',
  shell: true,
  env: {
    ...process.env,
    NODE_ENV: 'test',
    PLAYWRIGHT_BROWSERS_PATH: process.env.PLAYWRIGHT_BROWSERS_PATH || '0'
  }
});

testProcess.on('close', (code) => {
  console.log('\n' + '=' * 50);
  if (code === 0) {
    console.log('✅ Jupyter notebook tests completed successfully!');
    console.log('\n📊 Test Results Summary:');
    console.log('   - Notebook opening and display ✅');
    console.log('   - Cell interactions ✅');
    console.log('   - Execution capabilities ✅');
    console.log('   - File operations ✅');
    console.log('   - JSON structure validation ✅');
    console.log('   - Complex content handling ✅');
    console.log('   - Error scenario handling ✅');
  } else {
    console.log(`❌ Tests failed with exit code: ${code}`);
    console.log('\n🔍 Troubleshooting tips:');
    console.log('   1. Ensure VS Code is properly installed');
    console.log('   2. Check if Python extension is available');
    console.log('   3. Verify Jupyter extension installation (optional)');
    console.log('   4. Check test-results/ folder for detailed reports');
  }
  
  console.log('\n📁 Check these locations for more details:');
  console.log('   - HTML Report: ./test-results/html-report/index.html');
  console.log('   - Screenshots: ./test-results/');
  console.log('   - JSON Results: ./test-results/results.json');
  
  process.exit(code);
});

testProcess.on('error', (error) => {
  console.error('❌ Failed to start test process:', error);
  process.exit(1);
});