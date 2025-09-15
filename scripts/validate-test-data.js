#!/usr/bin/env node

/**
 * Test Data Validation Script
 * Validates that all test data files are properly created and accessible
 */

const fs = require('fs');
const path = require('path');

function validateTestData() {
  console.log('🔍 Validating test data files...\n');

  const testDataDir = path.join(__dirname, '..', 'test-data');
  
  // Expected test data files
  const expectedFiles = [
    'sample-notebook.ipynb',
    'sample-python-script.py',
    'azure-ml-config.json',
    'requirements.txt',
    'vscode-settings.json',
    'sample-dataset.csv'
  ];

  let allValid = true;

  // Check if test data directory exists
  if (!fs.existsSync(testDataDir)) {
    console.error('❌ Test data directory not found:', testDataDir);
    return false;
  }

  console.log('✓ Test data directory found:', testDataDir);

  // Validate each expected file
  expectedFiles.forEach(fileName => {
    const filePath = path.join(testDataDir, fileName);
    
    if (fs.existsSync(filePath)) {
      const stats = fs.statSync(filePath);
      console.log(`✓ ${fileName} (${stats.size} bytes)`);
      
      // Additional validation based on file type
      try {
        if (fileName.endsWith('.json')) {
          const content = fs.readFileSync(filePath, 'utf8');
          JSON.parse(content);
          console.log(`  └─ Valid JSON format`);
        } else if (fileName.endsWith('.csv')) {
          const content = fs.readFileSync(filePath, 'utf8');
          const lines = content.split('\n').filter(line => line.trim());
          console.log(`  └─ ${lines.length} lines (including header)`);
        } else if (fileName.endsWith('.py')) {
          const content = fs.readFileSync(filePath, 'utf8');
          if (content.includes('def main()')) {
            console.log(`  └─ Contains main function`);
          }
        } else if (fileName.endsWith('.ipynb')) {
          const content = fs.readFileSync(filePath, 'utf8');
          const notebook = JSON.parse(content);
          console.log(`  └─ ${notebook.cells.length} notebook cells`);
        }
      } catch (validationError) {
        console.warn(`  ⚠️  Validation warning for ${fileName}: ${validationError.message}`);
      }
      
    } else {
      console.error(`❌ ${fileName} - NOT FOUND`);
      allValid = false;
    }
  });

  // Check for any additional files
  const actualFiles = fs.readdirSync(testDataDir);
  const extraFiles = actualFiles.filter(file => !expectedFiles.includes(file));
  
  if (extraFiles.length > 0) {
    console.log('\n📁 Additional files found:');
    extraFiles.forEach(file => {
      console.log(`   + ${file}`);
    });
  }

  console.log(`\n📊 Validation Summary:`);
  console.log(`   Expected files: ${expectedFiles.length}`);
  console.log(`   Found files: ${actualFiles.length}`);
  console.log(`   Valid: ${allValid ? 'YES' : 'NO'}`);

  return allValid;
}

function showTestDataContents() {
  console.log('\n📄 Test Data Contents Preview:\n');

  const testDataDir = path.join(__dirname, '..', 'test-data');
  
  // Show sample content from each file
  const filesToPreview = [
    'azure-ml-config.json',
    'sample-dataset.csv',
    'vscode-settings.json'
  ];

  filesToPreview.forEach(fileName => {
    const filePath = path.join(testDataDir, fileName);
    
    if (fs.existsSync(filePath)) {
      console.log(`--- ${fileName} ---`);
      
      try {
        const content = fs.readFileSync(filePath, 'utf8');
        
        if (fileName.endsWith('.json')) {
          const jsonData = JSON.parse(content);
          console.log(JSON.stringify(jsonData, null, 2).substring(0, 300) + '...');
        } else if (fileName.endsWith('.csv')) {
          const lines = content.split('\n').slice(0, 5);
          lines.forEach(line => console.log(line));
          console.log('...');
        } else {
          console.log(content.substring(0, 200) + '...');
        }
        
      } catch (error) {
        console.error(`Error reading ${fileName}: ${error.message}`);
      }
      
      console.log('');
    }
  });
}

function main() {
  console.log('🧪 Test Data Validation Tool\n');

  const isValid = validateTestData();
  
  if (isValid) {
    console.log('\n✅ All test data files are valid and ready for testing!');
    
    // Show contents preview
    showTestDataContents();
    
    console.log('🚀 You can now run VS Code Desktop tests with:');
    console.log('   npm run test:electron');
    console.log('   or');
    console.log('   node scripts/run-vscode-test.js');
    
  } else {
    console.log('\n❌ Test data validation failed!');
    console.log('   Please check the missing or invalid files above.');
    process.exit(1);
  }
}

// Run the script if executed directly
if (require.main === module) {
  main();
}

module.exports = { validateTestData, showTestDataContents };