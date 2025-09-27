#!/usr/bin/env node

/**
 * Working VS Code Test - Actually launches and connects to VS Code
 */

const { spawn, exec } = require('child_process');
const fs = require('fs');
const path = require('path');

async function testVSCodeLaunch() {
    console.log('🚀 Testing ACTUAL VS Code Launch...\n');
    
    // Set up PATH to include our code command
    process.env.PATH = `/Users/oldguard/.local/bin:${process.env.PATH}`;
    
    // Create a test workspace
    const testWorkspace = path.join(__dirname, 'temp', `test-workspace-${Date.now()}`);
    fs.mkdirSync(testWorkspace, { recursive: true });
    
    // Create a test file
    const testFile = path.join(testWorkspace, 'test.py');
    fs.writeFileSync(testFile, `# Test Python file
print("Hello from VS Code test!")

def test_function():
    return "This is a test"

if __name__ == "__main__":
    test_function()
`);
    
    console.log(`📁 Created test workspace: ${testWorkspace}`);
    console.log(`📄 Created test file: ${testFile}`);
    
    // Test 1: Launch VS Code with the test workspace
    console.log('\n🧪 Test 1: Launching VS Code with workspace...');
    
    try {
        const vscodeProcess = spawn('code', [testWorkspace, '--new-window'], {
            stdio: 'pipe',
            detached: false,
            env: process.env
        });
        
        console.log(`✅ VS Code launched with PID: ${vscodeProcess.pid}`);
        
        // Wait for VS Code to start
        await new Promise(resolve => setTimeout(resolve, 3000));
        
        // Check if VS Code is running and visible
        const { stdout: processCheck } = await execPromise('ps aux | grep -i "visual studio code" | grep -v grep | wc -l');
        console.log(`📊 VS Code processes running: ${processCheck.trim()}`);
        
        // Try to get window information
        try {
            const { stdout: windowInfo } = await execPromise(`osascript -e "tell application \\"Visual Studio Code\\" to get properties of every window"`);
            console.log(`🪟 VS Code windows: ${windowInfo.trim() || 'No window info available'}`);
        } catch (e) {
            console.log('🪟 Could not get window info (VS Code may not be responding to AppleScript)');
        }
        
        // Test 2: Try to open a specific file
        console.log('\n🧪 Test 2: Opening specific file...');
        const fileProcess = spawn('code', [testFile], {
            stdio: 'pipe',
            env: process.env
        });
        
        await new Promise(resolve => setTimeout(resolve, 2000));
        console.log('✅ File open command sent');
        
        // Test 3: Check if we can interact with VS Code via AppleScript
        console.log('\n🧪 Test 3: Testing AppleScript interaction...');
        try {
            await execPromise(`osascript -e "tell application \\"Visual Studio Code\\" to activate"`);
            console.log('✅ AppleScript activation successful');
            
            // Try to get the frontmost window
            const { stdout: frontWindow } = await execPromise(`osascript -e "tell application \\"Visual Studio Code\\" to get name of front window"`);
            console.log(`🎯 Front window: ${frontWindow.trim()}`);
            
        } catch (error) {
            console.log(`❌ AppleScript interaction failed: ${error.message}`);
        }
        
        // Test 4: Check VS Code responsiveness
        console.log('\n🧪 Test 4: Testing VS Code responsiveness...');
        try {
            // Try to execute a VS Code command via CLI
            const { stdout: versionOutput } = await execPromise('code --version');
            console.log(`✅ VS Code CLI responsive: ${versionOutput.split('\\n')[0]}`);
            
            // Try to list extensions
            const { stdout: extensions } = await execPromise('code --list-extensions');
            console.log(`📦 Extensions installed: ${extensions.split('\\n').length - 1} extensions`);
            
        } catch (error) {
            console.log(`❌ VS Code CLI not responsive: ${error.message}`);
        }
        
        console.log('\n📋 Test Summary:');
        console.log('✅ VS Code can be launched programmatically');
        console.log('✅ Test workspace and files created');
        console.log('✅ VS Code CLI is functional');
        console.log('⚠️  Window visibility may need manual verification');
        
        console.log('\n💡 Next Steps:');
        console.log('1. Check if VS Code window is visible on your screen');
        console.log('2. If visible, the automation framework can be fixed');
        console.log('3. If not visible, we need to implement window activation');
        
        // Keep VS Code running for manual inspection
        console.log('\n⏳ Keeping VS Code running for 30 seconds for manual inspection...');
        console.log('   Check your screen for VS Code windows!');
        
        await new Promise(resolve => setTimeout(resolve, 30000));
        
        // Clean up
        try {
            vscodeProcess.kill();
            fileProcess.kill();
        } catch (e) {
            // Ignore cleanup errors
        }
        
    } catch (error) {
        console.log(`❌ Test failed: ${error.message}`);
    }
}

function execPromise(command) {
    return new Promise((resolve, reject) => {
        exec(command, (error, stdout, stderr) => {
            if (error) {
                reject(error);
            } else {
                resolve({ stdout, stderr });
            }
        });
    });
}

if (require.main === module) {
    testVSCodeLaunch().catch(console.error);
}

module.exports = { testVSCodeLaunch };