"""Pytest configuration and fixtures for Azure ML automation tests."""

import asyncio
import os
from pathlib import Path
from typing import AsyncGenerator, Generator

import pytest
import pytest_asyncio
from playwright.async_api import async_playwright, Browser, BrowserContext, Page, Playwright

from src.azure_ml_automation.helpers.config import config
from src.azure_ml_automation.helpers.logger import log_test_start, log_test_end, TestLogger
from src.azure_ml_automation.helpers.auth import create_auth_manager
from src.azure_ml_automation.helpers.azure_helpers import create_azure_ml_helper
from src.azure_ml_automation.pages.azure_ml_studio import AzureMLStudioPage


# Configure pytest-asyncio
pytest_asyncio.fixture_scope_function = "function"


@pytest.fixture(scope="session")
def event_loop():
    """Create an instance of the default event loop for the test session."""
    loop = asyncio.get_event_loop_policy().new_event_loop()
    yield loop
    loop.close()


@pytest.fixture(scope="session")
async def playwright() -> AsyncGenerator[Playwright, None]:
    """Create Playwright instance."""
    async with async_playwright() as p:
        yield p


@pytest.fixture(scope="session")
async def browser(playwright: Playwright) -> AsyncGenerator[Browser, None]:
    """Create browser instance."""
    browser_type = os.getenv("BROWSER", "chromium").lower()
    headless = os.getenv("HEADLESS", "true").lower() == "true"
    
    if browser_type == "firefox":
        browser = await playwright.firefox.launch(headless=headless)
    elif browser_type == "webkit":
        browser = await playwright.webkit.launch(headless=headless)
    else:
        browser = await playwright.chromium.launch(headless=headless)
    
    yield browser
    await browser.close()


@pytest.fixture
async def context(browser: Browser) -> AsyncGenerator[BrowserContext, None]:
    """Create browser context with common settings."""
    context = await browser.new_context(
        viewport={"width": 1920, "height": 1080},
        ignore_https_errors=True,
        record_video_dir="test-results/videos" if config.artifacts.path else None,
        record_har_path="test-results/traces/trace.har" if config.artifacts.path else None,
    )
    
    # Set up tracing
    await context.tracing.start(screenshots=True, snapshots=True, sources=True)
    
    yield context
    
    # Save trace
    if config.artifacts.path:
        trace_path = Path(config.artifacts.path) / "traces" / f"trace-{context.pages[0].url.replace('/', '_')}.zip"
        trace_path.parent.mkdir(parents=True, exist_ok=True)
        await context.tracing.stop(path=str(trace_path))
    
    await context.close()


@pytest.fixture
async def page(context: BrowserContext) -> AsyncGenerator[Page, None]:
    """Create a new page."""
    page = await context.new_page()
    
    # Set up console logging
    page.on("console", lambda msg: print(f"Console [{msg.type}]: {msg.text}"))
    
    # Set up error handling
    page.on("pageerror", lambda error: print(f"Page error: {error}"))
    
    yield page
    
    # Take screenshot on failure
    if hasattr(page, "_test_failed") and page._test_failed:
        screenshot_path = Path(config.artifacts.path) / "screenshots" / f"failure-{page.url.replace('/', '_')}.png"
        screenshot_path.parent.mkdir(parents=True, exist_ok=True)
        await page.screenshot(path=str(screenshot_path), full_page=True)


@pytest.fixture
def test_logger(request) -> TestLogger:
    """Create test logger for the current test."""
    test_name = request.node.name
    logger = log_test_start(test_name)
    
    yield logger
    
    # Determine if test passed or failed
    test_passed = not (hasattr(request.node, "rep_call") and request.node.rep_call.failed)
    log_test_end(logger, test_passed)


@pytest.fixture
async def auth_manager():
    """Create authenticated Azure manager."""
    manager = create_auth_manager()
    
    # Validate authentication
    is_valid = await manager.validate_authentication()
    if not is_valid:
        pytest.skip("Azure authentication not available")
    
    return manager


@pytest.fixture
async def azure_helper(test_logger: TestLogger):
    """Create Azure ML helper."""
    helper = create_azure_ml_helper(test_logger)
    
    # Validate workspace access
    has_access = await helper.validate_workspace_access()
    if not has_access:
        pytest.skip("Azure ML workspace access not available")
    
    return helper


@pytest.fixture
async def azure_ml_page(page: Page, test_logger: TestLogger) -> AzureMLStudioPage:
    """Create Azure ML Studio page object."""
    return AzureMLStudioPage(page, test_logger)


# Pytest hooks for better reporting
@pytest.hookimpl(tryfirst=True, hookwrapper=True)
def pytest_runtest_makereport(item, call):
    """Make test results available to fixtures."""
    outcome = yield
    rep = outcome.get_result()
    setattr(item, f"rep_{rep.when}", rep)
    
    # Mark page as failed if test failed
    if rep.when == "call" and rep.failed:
        if hasattr(item, "funcargs") and "page" in item.funcargs:
            item.funcargs["page"]._test_failed = True


def pytest_configure(config):
    """Configure pytest with custom markers."""
    config.addinivalue_line("markers", "smoke: mark test as smoke test")
    config.addinivalue_line("markers", "integration: mark test as integration test")
    config.addinivalue_line("markers", "electron: mark test as electron/desktop test")
    config.addinivalue_line("markers", "notebook: mark test as notebook execution test")
    config.addinivalue_line("markers", "compute: mark test as compute lifecycle test")
    config.addinivalue_line("markers", "pim: mark test as privileged identity management test")
    config.addinivalue_line("markers", "website: mark test as website/web interface test")
    config.addinivalue_line("markers", "slow: mark test as slow running")
    config.addinivalue_line("markers", "azure: mark test as requiring Azure resources")


def pytest_collection_modifyitems(config, items):
    """Modify test collection to add markers based on file location."""
    for item in items:
        # Add markers based on test file location
        test_path = str(item.fspath)
        
        if "/smoke/" in test_path:
            item.add_marker(pytest.mark.smoke)
        if "/integration/" in test_path:
            item.add_marker(pytest.mark.integration)
        if "/electron/" in test_path:
            item.add_marker(pytest.mark.electron)
        if "/notebooks/" in test_path:
            item.add_marker(pytest.mark.notebook)
        if "/compute/" in test_path:
            item.add_marker(pytest.mark.compute)
        if "/roles/" in test_path:
            item.add_marker(pytest.mark.pim)
        if "/websites/" in test_path:
            item.add_marker(pytest.mark.website)
        
        # Add azure marker for tests that use azure fixtures
        if any(fixture in item.fixturenames for fixture in ["auth_manager", "azure_helper", "azure_ml_page"]):
            item.add_marker(pytest.mark.azure)


@pytest.fixture(autouse=True)
def setup_test_environment():
    """Set up test environment before each test."""
    # Create artifacts directory
    artifacts_path = Path(config.artifacts.path)
    artifacts_path.mkdir(parents=True, exist_ok=True)
    
    # Create subdirectories
    for subdir in ["screenshots", "videos", "traces", "logs", "reports"]:
        (artifacts_path / subdir).mkdir(exist_ok=True)


# Skip tests based on environment
def pytest_runtest_setup(item):
    """Skip tests based on environment and markers."""
    # Skip Azure tests if not configured
    if item.get_closest_marker("azure"):
        if not all([
            config.azure.tenant_id,
            config.azure.subscription_id,
            config.azure.resource_group,
            config.azure.workspace_name
        ]):
            pytest.skip("Azure configuration not complete")
    
    # Skip slow tests in CI unless explicitly requested
    if item.get_closest_marker("slow") and os.getenv("CI") and not os.getenv("RUN_SLOW_TESTS"):
        pytest.skip("Slow test skipped in CI")
    
    # Skip electron tests if not on supported platform
    if item.get_closest_marker("electron"):
        import platform
        if platform.system() not in ["Windows", "Darwin", "Linux"]:
            pytest.skip("Electron tests not supported on this platform")