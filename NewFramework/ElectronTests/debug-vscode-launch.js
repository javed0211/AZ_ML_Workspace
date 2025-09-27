#!/usr/bin/env node

/**
 * Debug VS Code Launch - Find out why VS Code isn't staying visible
 */

const { spawn, exec } = require('child_process');
const fs = require('fs');
const path = require('path');

async function debugVSCodeLaunch() {
    console.log('🔍 Debugging VS Code Launch Issues...\n');
    
    // 1. Check VS Code installations
    console.log('📍 Checking VS Code installations:');
    const vscodeLocations = [
        '/Applications/Visual Studio Code.app',
        '/Users/oldguard/Downloads/Visual Studio Code.app',
        '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/ElectronTests/.vscode-test/vscode-darwin-arm64-1.104.2/Visual Studio Code.app'
    ];
    
    for (const location of vscodeLocations) {
        if (fs.existsSync(location)) {
            console.log(`✅ Found: ${location}`);
        } else {
            console.log(`❌ Not found: ${location}`);
        }
    }
    
    // 2. Check running VS Code processes
    console.log('\n🔍 Checking running VS Code processes:');
    try {
        const { stdout } = await execPromise('ps aux | grep -i "visual studio code" | grep -v grep');
        if (stdout.trim()) {
            console.log('✅ VS Code processes found:');
            console.log(stdout);
        } else {
            console.log('❌ No VS Code processes running');
        }
    } catch (error) {
        console.log('❌ No VS Code processes running');
    }
    
    // 3. Test different launch methods
    console.log('\n🚀 Testing different launch methods:');
    
    const testMethods = [
        {
            name: 'Method 1: Direct executable',
            command: '/Applications/Visual Studio Code.app/Contents/MacOS/Electron',
            args: ['--new-window', '--wait']
        },
        {
            name: 'Method 2: Open command',
            command: 'open',
            args: ['-a', 'Visual Studio Code', '--args', '--new-window']
        },
        {
            name: 'Method 3: Code command',
            command: 'code',
            args: ['--new-window', '--wait']
        }
    ];
    
    for (const method of testMethods) {
        console.log(`\n🧪 Testing ${method.name}:`);
        console.log(`Command: ${method.command} ${method.args.join(' ')}`);
        
        try {
            // Check if command exists
            if (method.command !== 'open' && !fs.existsSync(method.command) && method.command !== 'code') {
                console.log('❌ Command not found');
                continue;
            }
            
            // Launch with timeout
            const child = spawn(method.command, method.args, {
                stdio: 'pipe',
                detached: false
            });
            
            let output = '';
            let errorOutput = '';
            
            child.stdout?.on('data', (data) => {
                output += data.toString();
            });
            
            child.stderr?.on('data', (data) => {
                errorOutput += data.toString();
            });
            
            // Wait for process to start
            await new Promise((resolve) => {
                setTimeout(resolve, 2000);
            });
            
            // Check if VS Code window appeared
            const { stdout: windowCheck } = await execPromise('osascript -e "tell application \\"System Events\\" to get name of every process whose name contains \\"Visual Studio Code\\""').catch(() => ({ stdout: '' }));
            
            if (windowCheck.includes('Visual Studio Code')) {
                console.log('✅ VS Code process detected');
                
                // Check if window is visible
                const { stdout: windowVisible } = await execPromise('osascript -e "tell application \\"Visual Studio Code\\" to get visible"').catch(() => ({ stdout: 'false' }));
                console.log(`Window visible: ${windowVisible.trim()}`);
                
                // Try to bring to front
                await execPromise('osascript -e "tell application \\"Visual Studio Code\\" to activate"').catch(() => {});
                console.log('🎯 Attempted to bring VS Code to front');
            } else {
                console.log('❌ VS Code process not detected');
            }
            
            if (output) console.log(`Output: ${output}`);
            if (errorOutput) console.log(`Error: ${errorOutput}`);
            
            // Clean up
            try {
                child.kill();
            } catch (e) {}
            
        } catch (error) {
            console.log(`❌ Failed: ${error.message}`);
        }
    }
    
    // 4. Check system permissions
    console.log('\n🔐 Checking system permissions:');
    try {
        const { stdout } = await execPromise('osascript -e "tell application \\"System Events\\" to get name of every process"');
        console.log('✅ AppleScript permissions working');
    } catch (error) {
        console.log('❌ AppleScript permissions may be blocked');
        console.log('💡 You may need to grant Terminal/Node.js accessibility permissions in System Preferences > Security & Privacy > Privacy > Accessibility');
    }
    
    console.log('\n📋 Debug complete. Check the results above to identify the issue.');
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
    debugVSCodeLaunch().catch(console.error);
}

module.exports = { debugVSCodeLaunch };