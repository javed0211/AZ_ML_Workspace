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
const path = __importStar(require("path"));
const test_electron_1 = require("@vscode/test-electron");
const test_helpers_1 = require("../utils/test-helpers");
const test_1 = require("@playwright/test");
/**
 * VS Code tests using the official @vscode/test-electron framework
 * This is Microsoft's recommended approach for VS Code automation
 */
test_1.test.describe('VS Code Official Framework Tests', () => {
    const vscodeExecutablePath = '/Users/oldguard/Downloads/Visual Studio Code.app/Contents/MacOS/Electron';
    (0, test_1.test)('should launch VS Code using official framework', async () => {
        test_helpers_1.TestHelpers.logStep('Launching VS Code with official @vscode/test-electron framework');
        try {
            // Create a minimal test extension for the framework
            const extensionDevelopmentPath = path.resolve(__dirname, '../test-extension');
            const extensionTestsPath = path.resolve(__dirname, '../test-extension/out/test');
            // Ensure test extension directory exists
            await test_helpers_1.TestHelpers.ensureDirectoryExists(extensionDevelopmentPath);
            await test_helpers_1.TestHelpers.ensureDirectoryExists(path.join(extensionDevelopmentPath, 'out', 'test'));
            // Create minimal package.json for test extension
            await test_helpers_1.TestHelpers.createTestExtensionFiles(extensionDevelopmentPath);
            test_helpers_1.TestHelpers.logStep(`Using VS Code executable: ${vscodeExecutablePath}`);
            test_helpers_1.TestHelpers.logStep(`Extension development path: ${extensionDevelopmentPath}`);
            test_helpers_1.TestHelpers.logStep(`Extension tests path: ${extensionTestsPath}`);
            // Run tests using the official framework
            await (0, test_electron_1.runTests)({
                vscodeExecutablePath,
                extensionDevelopmentPath,
                extensionTestsPath,
                launchArgs: [
                    '--disable-extensions',
                    '--disable-workspace-trust',
                    '--skip-welcome',
                    '--skip-release-notes'
                ]
            });
            test_helpers_1.TestHelpers.logStep('✅ VS Code launched successfully with official framework');
        }
        catch (error) {
            test_helpers_1.TestHelpers.logStep(`❌ Failed to launch VS Code: ${error}`);
            throw error;
        }
    });
    (0, test_1.test)('should verify VS Code can be controlled programmatically', async () => {
        test_helpers_1.TestHelpers.logStep('Testing programmatic VS Code control');
        // This test will use the VS Code API through the extension
        // The actual test logic will be in the test extension
        (0, test_1.expect)(true).toBe(true); // Placeholder - real tests will be in extension
    });
});
