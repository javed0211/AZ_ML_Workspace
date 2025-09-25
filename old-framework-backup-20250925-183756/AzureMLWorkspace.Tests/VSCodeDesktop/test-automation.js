#!/usr/bin/env node

// Simple test script to verify the TypeScript automation works
const { spawn } = require('child_process');
const path = require('path');

async function testVSCodeAutomation() {
    console.log('🚀 Testing VS Code Desktop Automation...\n');

    const scriptPath = path.join(__dirname, 'dist', 'vscode-automation.js');
    
    // Test 1: Launch VS Code
    console.log('📝 Test 1: Launching VS Code...');
    const launchParams = JSON.stringify({
        action: 'launch',
        timeout: 30000
    });

    try {
        const launchResult = await runNodeScript(scriptPath, launchParams);
        console.log('✅ Launch result:', launchResult.success ? 'SUCCESS' : 'FAILED');
        console.log('   Message:', launchResult.message);
        
        if (launchResult.success) {
            // Test 2: Check interactivity
            console.log('\n📝 Test 2: Checking VS Code interactivity...');
            const interactivityParams = JSON.stringify({
                action: 'checkInteractivity'
            });
            
            const interactivityResult = await runNodeScript(scriptPath, interactivityParams);
            console.log('✅ Interactivity result:', interactivityResult.success ? 'SUCCESS' : 'FAILED');
            console.log('   Message:', interactivityResult.message);
            
            // Test 3: Check application links
            console.log('\n📝 Test 3: Checking application links...');
            const linksParams = JSON.stringify({
                action: 'checkApplicationLinks'
            });
            
            const linksResult = await runNodeScript(scriptPath, linksParams);
            console.log('✅ Application links result:', linksResult.success ? 'SUCCESS' : 'FAILED');
            console.log('   Message:', linksResult.message);
            if (linksResult.data) {
                console.log('   Data:', JSON.stringify(linksResult.data, null, 2));
            }
            
            // Test 4: Take screenshot
            console.log('\n📝 Test 4: Taking screenshot...');
            const screenshotParams = JSON.stringify({
                action: 'takeScreenshot',
                filename: 'test-screenshot.png'
            });
            
            const screenshotResult = await runNodeScript(scriptPath, screenshotParams);
            console.log('✅ Screenshot result:', screenshotResult.success ? 'SUCCESS' : 'FAILED');
            console.log('   Message:', screenshotResult.message);
            
            // Test 5: Close VS Code
            console.log('\n📝 Test 5: Closing VS Code...');
            const closeParams = JSON.stringify({
                action: 'close'
            });
            
            const closeResult = await runNodeScript(scriptPath, closeParams);
            console.log('✅ Close result:', closeResult.success ? 'SUCCESS' : 'FAILED');
            console.log('   Message:', closeResult.message);
        }
        
    } catch (error) {
        console.error('❌ Test failed:', error.message);
    }
    
    console.log('\n🏁 VS Code Desktop Automation test completed!');
}

function runNodeScript(scriptPath, params) {
    return new Promise((resolve, reject) => {
        const child = spawn('node', [scriptPath, params], {
            stdio: ['pipe', 'pipe', 'pipe']
        });

        let stdout = '';
        let stderr = '';

        child.stdout.on('data', (data) => {
            stdout += data.toString();
        });

        child.stderr.on('data', (data) => {
            stderr += data.toString();
        });

        child.on('close', (code) => {
            if (stderr) {
                console.warn('   Warning:', stderr.trim());
            }
            
            try {
                const result = JSON.parse(stdout.trim());
                resolve(result);
            } catch (parseError) {
                reject(new Error(`Failed to parse result: ${stdout}\nError: ${parseError.message}`));
            }
        });

        child.on('error', (error) => {
            reject(new Error(`Failed to spawn process: ${error.message}`));
        });
    });
}

// Run the test
if (require.main === module) {
    testVSCodeAutomation().catch(console.error);
}

module.exports = { testVSCodeAutomation };