"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const child_process_1 = require("child_process");
const path_1 = __importDefault(require("path"));
async function globalSetup(config) {
    console.log('🚀 Global Setup: Preparing VS Code test environment...');
    // Ensure code command is available
    const codePath = '/Users/oldguard/.local/bin/code';
    process.env.PATH = `/Users/oldguard/.local/bin:${process.env.PATH}`;
    try {
        // Test if code command works
        (0, child_process_1.execSync)('code --version', { stdio: 'pipe' });
        console.log('✅ VS Code CLI is available');
    }
    catch (error) {
        console.log('❌ VS Code CLI not available, tests may fail');
    }
    // Create temp directory for test workspaces
    const tempDir = path_1.default.join(__dirname, '..', 'temp');
    try {
        (0, child_process_1.execSync)(`mkdir -p "${tempDir}"`, { stdio: 'pipe' });
        console.log('✅ Temp directory created');
    }
    catch (error) {
        console.log('⚠️  Could not create temp directory');
    }
    console.log('✅ Global setup complete');
}
exports.default = globalSetup;
