const { runTests } = require('@vscode/test-electron');
const path = require('path');

async function main() {
    try {
        console.log('🚀 Starting VS Code with official @vscode/test-electron framework');
        
        // Path to your VS Code installation
        const vscodeExecutablePath = '/Users/oldguard/Downloads/Visual Studio Code.app/Contents/MacOS/Electron';
        
        // The folder containing the Extension Manifest package.json
        const extensionDevelopmentPath = path.resolve(__dirname, './test-extension');
        
        // The path to the compiled test runner
        const extensionTestsPath = path.resolve(__dirname, './test-extension/out/test/suite/index');
        
        console.log('📍 VS Code executable:', vscodeExecutablePath);
        console.log('📁 Extension development path:', extensionDevelopmentPath);
        console.log('🧪 Extension tests path:', extensionTestsPath);
        
        // Launch VS Code with the test extension
        await runTests({
            vscodeExecutablePath,
            extensionDevelopmentPath,
            extensionTestsPath,
            launchArgs: [
                '--disable-extensions',
                '--disable-workspace-trust',
                '--skip-welcome',
                '--skip-release-notes',
                '--new-window'
            ]
        });
        
        console.log('✅ All tests completed successfully!');
        
    } catch (err) {
        console.error('❌ Failed to run tests:', err);
        process.exit(1);
    }
}

main();