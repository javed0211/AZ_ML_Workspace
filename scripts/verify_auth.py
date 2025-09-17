#!/usr/bin/env python3
"""
Quick script to verify authentication configuration.
Run this to check if Managed Identity is being used correctly.
"""

import sys
import os
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

from azure.identity import ManagedIdentityCredential, ClientSecretCredential, InteractiveBrowserCredential, DefaultAzureCredential
from azure_ml_automation.helpers.auth import create_auth_manager, create_pim_manager
from azure_ml_automation.helpers.config import config


def main():
    print("🔍 Verifying Authentication Configuration")
    print("=" * 50)
    
    # Check configuration
    print(f"USE_MANAGED_IDENTITY: {config.auth.use_managed_identity}")
    print(f"USE_INTERACTIVE_AUTH: {config.auth.use_interactive}")
    print(f"AZURE_CLIENT_SECRET configured: {'Yes' if config.azure.client_secret else 'No'}")
    print()
    
    # Create auth manager
    print("Creating auth manager...")
    auth_manager = create_auth_manager()
    credential = auth_manager.get_credential()
    
    print(f"Credential type: {type(credential).__name__}")
    
    # Check if it's the expected type
    if isinstance(credential, ManagedIdentityCredential):
        print("✅ SUCCESS: Using Managed Identity authentication")
    elif isinstance(credential, ClientSecretCredential):
        print("⚠️  WARNING: Using Client Secret authentication")
        print("   This means AZURE_CLIENT_SECRET is set, which overrides Managed Identity")
        print("   Remove AZURE_CLIENT_SECRET from .env file to use Managed Identity")
    elif isinstance(credential, InteractiveBrowserCredential):
        print("⚠️  WARNING: Using Interactive Browser authentication")
        print("   This means USE_INTERACTIVE_AUTH=true")
    elif isinstance(credential, DefaultAzureCredential):
        print("⚠️  WARNING: Using Default Azure Credential")
        print("   This means Managed Identity is disabled")
    else:
        print(f"❌ ERROR: Unexpected credential type: {type(credential)}")
    
    print()
    
    # Test PIM manager
    print("Creating PIM manager...")
    pim_manager = create_pim_manager(auth_manager)
    pim_credential = pim_manager.auth_manager.get_credential()
    
    if isinstance(pim_credential, ManagedIdentityCredential):
        print("✅ SUCCESS: PIM manager uses Managed Identity authentication")
    else:
        print(f"⚠️  WARNING: PIM manager uses {type(pim_credential).__name__}")
    
    print()
    print("🎯 Recommendations:")
    if not isinstance(credential, ManagedIdentityCredential):
        print("- Remove AZURE_CLIENT_SECRET from your .env file")
        print("- Set USE_MANAGED_IDENTITY=true (or leave unset for default)")
        print("- Set USE_INTERACTIVE_AUTH=false (or leave unset for default)")
    else:
        print("- Configuration is correct for Managed Identity")
        print("- Tests will use Managed Identity authentication")
    
    print()
    print("Done! 🚀")


if __name__ == "__main__":
    main()