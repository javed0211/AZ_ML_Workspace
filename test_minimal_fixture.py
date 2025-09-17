#!/usr/bin/env python3
"""
Minimal test using pytest fixtures to identify the hanging point.
"""

import pytest
from playwright.async_api import Page

@pytest.mark.asyncio
async def test_minimal_page_fixture(page: Page):
    """Test using just the page fixture."""
    print("🔍 Testing minimal page fixture...")
    
    try:
        print("✓ Page fixture created")
        
        # Simple navigation
        await page.goto("https://www.google.com", timeout=30000)
        print("✓ Navigation to Google successful")
        
        title = await page.title()
        print(f"✓ Page title: {title}")
        
        print("✅ Minimal fixture test completed")
        
    except Exception as e:
        print(f"❌ Minimal fixture test failed: {e}")
        raise

if __name__ == "__main__":
    pytest.main([__file__, "-v", "-s"])