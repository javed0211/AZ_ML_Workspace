#!/usr/bin/env python3
"""
Simple browser test to debug the hanging issue.
"""

import pytest
import asyncio
from playwright.async_api import async_playwright

@pytest.mark.asyncio
async def test_simple_browser_launch():
    """Test if browser launches correctly."""
    print("🔍 Testing browser launch...")
    
    async with async_playwright() as p:
        print("✓ Playwright started")
        
        # Launch browser
        browser = await p.chromium.launch(headless=True)
        print("✓ Browser launched")
        
        # Create context
        context = await browser.new_context()
        print("✓ Context created")
        
        # Create page
        page = await context.new_page()
        print("✓ Page created")
        
        # Navigate to a simple page
        await page.goto("https://www.google.com")
        print("✓ Navigation successful")
        
        # Get title
        title = await page.title()
        print(f"✓ Page title: {title}")
        
        # Clean up
        await context.close()
        await browser.close()
        print("✓ Browser closed")

if __name__ == "__main__":
    asyncio.run(test_simple_browser_launch())