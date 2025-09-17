#!/usr/bin/env python3
"""
Isolated browser test to identify the exact hanging point.
"""

import asyncio
import sys
from playwright.async_api import async_playwright

async def test_isolated_navigation():
    """Test navigation without any fixtures or complex setup."""
    print("🔍 Starting isolated browser test...")
    
    try:
        async with async_playwright() as p:
            print("✓ Playwright initialized")
            
            # Launch browser
            browser = await p.chromium.launch(
                headless=True,
                args=['--no-sandbox', '--disable-dev-shm-usage']
            )
            print("✓ Browser launched")
            
            # Create context
            context = await browser.new_context(
                viewport={"width": 1920, "height": 1080},
                ignore_https_errors=True
            )
            print("✓ Context created")
            
            # Create page
            page = await context.new_page()
            print("✓ Page created")
            
            # Test URL from the test file
            test_url = "https://ml.azure.com/workspaces?tid=afd0e3db-52d8-42c3-9648-3e3a1c3c1d5f"
            print(f"🌐 Navigating to: {test_url}")
            
            try:
                # Navigate with timeout
                await page.goto(test_url, timeout=30000)
                print("✓ Navigation completed")
                
                # Get basic page info
                title = await page.title()
                url = page.url
                print(f"✓ Page title: {title}")
                print(f"✓ Current URL: {url}")
                
                # Wait for network idle
                try:
                    await page.wait_for_load_state("networkidle", timeout=10000)
                    print("✓ Network idle reached")
                except Exception as e:
                    print(f"⚠️ Network idle timeout: {e}")
                
                # Check if page has any content
                try:
                    body_text = await page.locator("body").text_content()
                    if body_text and len(body_text.strip()) > 0:
                        print(f"✓ Page has content ({len(body_text)} characters)")
                    else:
                        print("⚠️ Page appears to be empty")
                except Exception as e:
                    print(f"⚠️ Could not get page content: {e}")
                
            except Exception as nav_error:
                print(f"❌ Navigation failed: {nav_error}")
                
                # Try to get current state
                try:
                    current_url = page.url
                    print(f"Current URL after error: {current_url}")
                except:
                    pass
            
            # Clean up
            await context.close()
            await browser.close()
            print("✓ Browser closed")
            
    except Exception as e:
        print(f"❌ Test failed: {e}")
        return False
    
    print("✅ Isolated test completed successfully")
    return True

if __name__ == "__main__":
    success = asyncio.run(test_isolated_navigation())
    sys.exit(0 if success else 1)