#!/usr/bin/env python3
"""
Simple browser test with minimal pytest setup to isolate the issue.
"""

import pytest
import asyncio
from playwright.async_api import async_playwright, Browser, BrowserContext, Page
from typing import AsyncGenerator

# Simple fixtures without complex setup
@pytest.fixture(scope="session")
async def simple_browser() -> AsyncGenerator[Browser, None]:
    """Simple browser fixture."""
    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        yield browser
        await browser.close()

@pytest.fixture
async def simple_context(simple_browser: Browser) -> AsyncGenerator[BrowserContext, None]:
    """Simple context fixture."""
    context = await simple_browser.new_context()
    yield context
    await context.close()

@pytest.fixture
async def simple_page(simple_context: BrowserContext) -> AsyncGenerator[Page, None]:
    """Simple page fixture."""
    page = await simple_context.new_page()
    yield page

@pytest.mark.asyncio
async def test_simple_browser_with_fixtures(simple_page: Page):
    """Test using simple fixtures."""
    print("🔍 Testing with simple fixtures...")
    
    await simple_page.goto("https://www.google.com", timeout=30000)
    title = await simple_page.title()
    
    print(f"✓ Page title: {title}")
    assert "Google" in title
    
    print("✅ Simple fixture test completed")

if __name__ == "__main__":
    pytest.main([__file__, "-v", "-s"])