import * as vscode from 'vscode';

export function activate(context: vscode.ExtensionContext) {
    console.log('VS Code Test Extension activated');
    
    // Register a simple command for testing
    const disposable = vscode.commands.registerCommand('vscode-test-extension.helloWorld', () => {
        vscode.window.showInformationMessage('Hello World from VS Code Test Extension!');
    });

    context.subscriptions.push(disposable);
}

export function deactivate() {
    console.log('VS Code Test Extension deactivated');
}