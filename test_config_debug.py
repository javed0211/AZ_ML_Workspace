#!/usr/bin/env python3
"""
Test config loading to see if that's causing the hang.
"""

try:
    print("🔍 Testing config loading...")
    
    from src.azure_ml_automation.helpers.config import config
    print("✓ Config imported successfully")
    
    print(f"Artifacts path: {config.artifacts.path}")
    print(f"Azure tenant ID: {config.azure.tenant_id}")
    print(f"Timeouts navigation: {config.timeouts.navigation}")
    
    print("✅ Config loading test completed")
    
except Exception as e:
    print(f"❌ Config loading failed: {e}")
    import traceback
    traceback.print_exc()