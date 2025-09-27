import * as assert from 'assert';
import * as vscode from 'vscode';

suite('Extension Test Suite', () => {
    vscode.window.showInformationMessage('Start all tests.');

    test('VS Code should be running', () => {
        assert.ok(vscode.window);
        assert.ok(vscode.workspace);
        assert.ok(vscode.commands);
        console.log('✅ VS Code is running and accessible via API');
    });

    test('Should be able to execute commands', async () => {
        // Test executing a built-in VS Code command
        await vscode.commands.executeCommand('workbench.action.showCommands');
        console.log('✅ Command palette opened successfully');
        
        // Close command palette
        await vscode.commands.executeCommand('workbench.action.closeQuickOpen');
        console.log('✅ Command palette closed successfully');
    });

    test('Should be able to create and edit files', async () => {
        // Create a new untitled document
        const document = await vscode.workspace.openTextDocument({
            content: 'Hello, World!\nThis is a test file created via VS Code API.',
            language: 'plaintext'
        });
        
        assert.ok(document);
        assert.strictEqual(document.getText().includes('Hello, World!'), true);
        console.log('✅ Document created and content verified');
        
        // Show the document in editor
        await vscode.window.showTextDocument(document);
        console.log('✅ Document opened in editor');
    });

    test('Should be able to access workspace', () => {
        const workspaceFolders = vscode.workspace.workspaceFolders;
        console.log('Workspace folders:', workspaceFolders?.length || 0);
        
        // Test workspace configuration
        const config = vscode.workspace.getConfiguration();
        assert.ok(config);
        console.log('✅ Workspace configuration accessible');
    });

    test('Should be able to show notifications', async () => {
        await vscode.window.showInformationMessage('Test notification from automated test');
        console.log('✅ Notification shown successfully');
    });
});