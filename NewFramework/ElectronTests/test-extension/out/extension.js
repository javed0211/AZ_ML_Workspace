"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.deactivate = exports.activate = void 0;
const vscode = require("vscode");
function activate(context) {
    console.log('VS Code Test Extension activated');
    // Register a simple command for testing
    const disposable = vscode.commands.registerCommand('vscode-test-extension.helloWorld', () => {
        vscode.window.showInformationMessage('Hello World from VS Code Test Extension!');
    });
    context.subscriptions.push(disposable);
}
exports.activate = activate;
function deactivate() {
    console.log('VS Code Test Extension deactivated');
}
exports.deactivate = deactivate;
//# sourceMappingURL=extension.js.map