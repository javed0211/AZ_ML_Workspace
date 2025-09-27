const { spawn } = require('child_process');
const path = require('path');

async function launchVSCode() {
    // Path to the VS Code downloaded by the official framework
    const vscodeAppPath = '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/ElectronTests/.vscode-test/vscode-darwin-arm64-1.104.2/Visual Studio Code.app';
    
    console.log('🚀 Launching VS Code using the downloaded version...');
    console.log('📍 VS Code path:', vscodeAppPath);
    
    try {
        // Use the macOS 'open' command to launch VS Code properly
        const process = spawn('open', ['-a', vscodeAppPath], {
            stdio: 'inherit',
            detached: true
        });
        
        process.on('exit', (code) => {
            if (code === 0) {
                console.log('✅ VS Code launched successfully!');
                console.log('🎉 You should see VS Code opening now!');
            } else {
                console.log(`❌ VS Code launch failed with code: ${code}`);
            }
        });
        
        process.on('error', (error) => {
            console.error('❌ Failed to launch VS Code:', error);
        });
        
        // Don't wait for the process to finish - VS Code should open independently
        process.unref();
        
        console.log('🎯 VS Code launch initiated - check your screen!');
        
    } catch (error) {
        console.error('❌ Error launching VS Code:', error);
    }
}

launchVSCode();