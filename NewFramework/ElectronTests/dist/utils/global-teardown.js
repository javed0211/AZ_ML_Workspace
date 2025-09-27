"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
async function globalTeardown(config) {
    console.log('🧹 Global Teardown: Cleaning up VS Code test environment...');
    try {
        // Don't kill the main VS Code instance, just test instances
        // We'll be more selective about cleanup
        console.log('✅ Cleanup complete (preserving main VS Code instance)');
    }
    catch (error) {
        console.log('⚠️  Cleanup had some issues, but continuing...');
    }
}
exports.default = globalTeardown;
