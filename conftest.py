"""Pytest configuration and fixtures for Azure ML automation tests."""

import asyncio
import os
from pathlib import Path
from typing import AsyncGenerator, Generator

import pytest
import pytest_asyncio
from playwright.async_api import async_playwright, Browser, BrowserContext, Page, Playwright

from src.azure_ml_automation.helpers.config import Config

# Create a fresh config instance to avoid cached global config issues
config = Config()
from src.azure_ml_automation.helpers.logger import log_test_start, log_test_end, TestLogger
from src.azure_ml_automation.helpers.auth import create_auth_manager
from src.azure_ml_automation.helpers.azure_helpers import create_azure_ml_helper
from src.azure_ml_automation.pages.azure_ml_studio import AzureMLStudioPage


# Configure pytest-asyncio to use function scope by default
pytest_asyncio.fixture_scope_function = "function"


@pytest.fixture(scope="function")
def event_loop():
    """Create an instance of the default event loop for each test function."""
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    yield loop
    loop.close()


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
async def browser_page() -> AsyncGenerator[Page, None]:
    """Create a browser page using function-scoped fixtures to avoid hanging."""
    print("Creating browser and page...")
    
    async with async_playwright() as p:
        browser = await p.chromium.launch(
            headless=os.getenv("HEADLESS", "false").lower() == "true",  # Changed default to "false"
            args=['--no-sandbox', '--disable-dev-shm-usage']
        )
        print("Browser launched")
        
        context = await browser.new_context(
            viewport={"width": 1920, "height": 1080},
            ignore_https_errors=True
        )
        print("Context created")
        
        page = await context.new_page()
        print("Page created")
        
        # Set up console logging
        page.on("console", lambda msg: print(f"Console [{msg.type}]: {msg.text}"))
        page.on("pageerror", lambda error: print(f"Page error: {error}"))
        
        try:
            yield page
        finally:
            print("Cleaning up browser resources...")
            await context.close()
            await browser.close()
            print("Browser closed")


@pytest.fixture
async def azure_ml_page(browser_page: Page, test_logger: TestLogger) -> AzureMLStudioPage:
    """Create Azure ML Studio page object."""
    return AzureMLStudioPage(browser_page, test_logger)


# Pytest hooks for better reporting
@pytest.hookimpl(tryfirst=True, hookwrapper=True)
def pytest_runtest_makereport(item, call):
    """Make test results available to fixtures."""
    outcome = yield
    rep = outcome.get_result()
    setattr(item, f"rep_{rep.when}", rep)


def pytest_configure(config):
    """Configure pytest with custom markers."""
    config.addinivalue_line("markers", "smoke: mark test as smoke test")
    config.addinivalue_line("markers", "integration: mark test as integration test")
    config.addinivalue_line("markers", "electron: mark test as electron/desktop test")
    config.addinivalue_line("markers", "notebook: mark test as notebook execution test")
    config.addinivalue_line("markers", "compute: mark test as compute lifecycle test")
    config.addinivalue_line("markers", "dataset: mark test as dataset management test")
    config.addinivalue_line("markers", "model: mark test as model management test")
    config.addinivalue_line("markers", "experiment: mark test as experiment/job test")
    config.addinivalue_line("markers", "document_search: mark test as document search test")
    config.addinivalue_line("markers", "document_processing: mark test as document processing test")
    config.addinivalue_line("markers", "speech_to_text: mark test as speech-to-text test")
    config.addinivalue_line("markers", "text_to_speech: mark test as text-to-speech test")
    config.addinivalue_line("markers", "speech_translation: mark test as speech translation test")
    config.addinivalue_line("markers", "voice_recognition: mark test as voice recognition test")
    config.addinivalue_line("markers", "performance: mark test as performance test")
    config.addinivalue_line("markers", "security: mark test as security test")
    config.addinivalue_line("markers", "pim: mark test as privileged identity management test")
    config.addinivalue_line("markers", "website: mark test as website/web interface test")
    config.addinivalue_line("markers", "slow: mark test as slow running")
    config.addinivalue_line("markers", "azure: mark test as requiring Azure resources")


def pytest_collection_modifyitems(config, items):
    """Modify test collection to add markers based on file location."""
    for item in items:
        # Add markers based on test file location
        test_path = str(item.fspath)
        
        # ML Workspace tests
        if "/ml_workspace/" in test_path:
            if "/workspace_management/" in test_path:
                item.add_marker(pytest.mark.compute)
            elif "/notebooks/" in test_path:
                item.add_marker(pytest.mark.notebook)
            elif "/datasets/" in test_path:
                item.add_marker(pytest.mark.dataset)
            elif "/models/" in test_path:
                item.add_marker(pytest.mark.model)
            elif "/experiments/" in test_path:
                item.add_marker(pytest.mark.experiment)
        
        # AI Document Search tests
        if "/ai_document_search/" in test_path:
            if "/search_functionality/" in test_path:
                item.add_marker(pytest.mark.document_search)
            elif "/document_processing/" in test_path:
                item.add_marker(pytest.mark.document_processing)
        
        # Speech Services tests
        if "/speech_services/" in test_path:
            if "/speech_to_text/" in test_path:
                item.add_marker(pytest.mark.speech_to_text)
            elif "/text_to_speech/" in test_path:
                item.add_marker(pytest.mark.text_to_speech)
            elif "/speech_translation/" in test_path:
                item.add_marker(pytest.mark.speech_translation)
            elif "/voice_recognition/" in test_path:
                item.add_marker(pytest.mark.voice_recognition)
        
        # Other test categories
        if "/integration/" in test_path:
            item.add_marker(pytest.mark.integration)
        if "/performance/" in test_path:
            item.add_marker(pytest.mark.performance)
        if "/security/" in test_path:
            item.add_marker(pytest.mark.security)
        if "/electron/" in test_path:
            item.add_marker(pytest.mark.electron)
        if "/roles/" in test_path or "pim" in test_path.lower():
            item.add_marker(pytest.mark.pim)
        if "/websites/" in test_path:
            item.add_marker(pytest.mark.website)
        
        # Add azure marker for tests that use azure fixtures
        if any(fixture in item.fixturenames for fixture in ["auth_manager", "azure_helper", "azure_ml_page"]):
            item.add_marker(pytest.mark.azure)


@pytest.fixture(autouse=True)
def setup_test_environment():
    """Set up test environment before each test."""
    print("Setting up test environment...")
    try:
        # Create artifacts directory
        artifacts_path = Path(config.artifacts.path)
        artifacts_path.mkdir(parents=True, exist_ok=True)
        
        # Create subdirectories
        for subdir in ["screenshots", "videos", "traces", "logs", "reports"]:
            (artifacts_path / subdir).mkdir(exist_ok=True)
        
        print("Test environment set up successfully")
    except Exception as e:
        print(f"Error setting up test environment: {e}")
        # Don't fail the test, just warn
        pass


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