#!/usr/bin/env node

/**
 * Jupyter Setup Verification Script
 * Checks if everything is ready for Jupyter notebook testing
 */

const fs = require('fs');
const path = require('path');

console.log('🔍 Verifying Jupyter Notebook Test Setup...');
console.log('=' * 50);

let allGood = true;

// Check 1: Test file exists
const testFile = path.join(__dirname, 'tests', 'jupyter-notebook.test.ts');
if (fs.existsSync(testFile)) {
  console.log('✅ Jupyter notebook test file exists');
} else {
  console.log('❌ Jupyter notebook test file missing');
  allGood = false;
}

// Check 2: Source notebook exists
const sourceNotebook = '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook/azure_ml_project.ipynb';
if (fs.existsSync(sourceNotebook)) {
  console.log('✅ Source notebook exists');
  
  // Validate notebook structure
  try {
    const content = fs.readFileSync(sourceNotebook, 'utf8');
    const notebook = JSON.parse(content);
    
    if (notebook.cells && notebook.metadata && notebook.nbformat) {
      console.log(`   📊 Valid notebook with ${notebook.cells.length} cells`);
      console.log(`   📝 Format: ${notebook.nbformat}.${notebook.nbformat_minor || 0}`);
    } else {
      console.log('⚠️ Notebook structure may be invalid');
    }
  } catch (error) {
    console.log('⚠️ Could not parse notebook JSON');
  }
} else {
  console.log('⚠️ Source notebook not found (will create test notebook)');
}

// Check 3: Playwright config
const configFile = path.join(__dirname, 'playwright.config.ts');
if (fs.existsSync(configFile)) {
  console.log('✅ Playwright config exists');
} else {
  console.log('❌ Playwright config missing');
  allGood = false;
}

// Check 4: Test helpers
const helpersFile = path.join(__dirname, 'utils', 'test-helpers.ts');
if (fs.existsSync(helpersFile)) {
  console.log('✅ Test helpers exist');
} else {
  console.log('❌ Test helpers missing');
  allGood = false;
}

// Check 5: VS Code Electron utility
const vscodeFile = path.join(__dirname, 'utils', 'vscode-electron.ts');
if (fs.existsSync(vscodeFile)) {
  console.log('✅ VS Code Electron utility exists');
} else {
  console.log('❌ VS Code Electron utility missing');
  allGood = false;
}

// Check 6: Package.json and dependencies
const packageFile = path.join(__dirname, 'package.json');
if (fs.existsSync(packageFile)) {
  console.log('✅ Package.json exists');
  
  try {
    const pkg = JSON.parse(fs.readFileSync(packageFile, 'utf8'));
    const deps = { ...pkg.dependencies, ...pkg.devDependencies };
    
    if (deps['@playwright/test']) {
      console.log('   ✅ Playwright dependency found');
    } else {
      console.log('   ❌ Playwright dependency missing');
      allGood = false;
    }
  } catch (error) {
    console.log('   ⚠️ Could not parse package.json');
  }
} else {
  console.log('❌ Package.json missing');
  allGood = false;
}

// Check 7: Temp directory
const tempDir = path.join(__dirname, 'temp');
if (!fs.existsSync(tempDir)) {
  fs.mkdirSync(tempDir, { recursive: true });
  console.log('✅ Created temp directory');
} else {
  console.log('✅ Temp directory exists');
}

console.log('\n' + '=' * 50);

if (allGood) {
  console.log('🎉 Setup verification PASSED!');
  console.log('\n🚀 Ready to run Jupyter notebook tests:');
  console.log('   node run-jupyter-tests.js');
  console.log('\n📋 Available test cases:');
  console.log('   1. Notebook opening and display');
  console.log('   2. Cell interactions');
  console.log('   3. Execution capabilities');
  console.log('   4. File operations');
  console.log('   5. JSON structure validation');
  console.log('   6. Complex content handling');
  console.log('   7. Error scenario handling');
} else {
  console.log('❌ Setup verification FAILED!');
  console.log('\n🔧 Please fix the issues above before running tests.');
}

console.log('\n📁 Test structure:');
console.log('   tests/jupyter-notebook.test.ts  - Main test file');
console.log('   run-jupyter-tests.js           - Test runner');
console.log('   verify-jupyter-setup.js        - This verification script');
console.log('   utils/test-helpers.ts          - Helper utilities');
console.log('   utils/vscode-electron.ts       - VS Code automation');

process.exit(allGood ? 0 : 1);