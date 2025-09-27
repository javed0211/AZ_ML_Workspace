const { runTests } = require('@vscode/test-electron');
const path = require('path');

async function main() {
    try {
        console.log('🚀 Starting VS Code with official @vscode/test-electron framework (auto-download)');
        
        // The folder containing the Extension Manifest package.json
        const extensionDevelopmentPath = path.resolve(__dirname, './test-extension');
        
        // The path to the compiled test runner
        const extensionTestsPath = path.resolve(__dirname, './test-extension/out/test/suite/index');
        
        console.log('📁 Extension development path:', extensionDevelopmentPath);
        console.log('🧪 Extension tests path:', extensionTestsPath);
        
        // Let the framework download and use its own VS Code instance
        // This should work better than using the system VS Code
        await runTests({
            // Don't specify vscodeExecutablePath - let it download VS Code
            extensionDevelopmentPath,
            extensionTestsPath,
            launchArgs: [
                '--disable-extensions',
                '--disable-workspace-trust',
                '--skip-welcome',
                '--skip-release-notes'
            ]
        });
        
        console.log('✅ All tests completed successfully!');
        
    } catch (err) {
        console.error('❌ Failed to run tests:', err);
        process.exit(1);
    }
}

main();