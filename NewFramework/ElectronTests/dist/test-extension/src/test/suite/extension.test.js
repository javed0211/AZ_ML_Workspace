"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
const assert = __importStar(require("assert"));
const vscode = __importStar(require("vscode"));
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
