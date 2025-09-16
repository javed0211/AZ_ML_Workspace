"""Base page class for all page objects."""

import asyncio
from abc import ABC, abstractmethod
from typing import Any, Dict, List, Optional, Union

from playwright.async_api import Page, Locator, expect

from ..helpers.config import config
from ..helpers.logger import TestLogger, log_browser_action


class BasePage(ABC):
    """Base class for all page objects."""
    
    def __init__(self, page: Page, test_logger: Optional[TestLogger] = None):
        self.page = page
        self.test_logger = test_logger
        self.timeout = config.timeouts.default
        self.navigation_timeout = config.timeouts.navigation
    
    @abstractmethod
    async def is_page_loaded(self) -> bool:
        """Check if the page is loaded."""
        pass
    
    @abstractmethod
    def get_page_identifier(self) -> str:
        """Get a unique identifier for this page."""
        pass
    
    async def wait_for_page_load(self, timeout: Optional[int] = None) -> None:
        """Wait for the page to load."""
        timeout = timeout or self.navigation_timeout
        
        if self.test_logger:
            self.test_logger.action(f"Waiting for {self.get_page_identifier()} to load")
        
        start_time = asyncio.get_event_loop().time()
        
        while asyncio.get_event_loop().time() - start_time < timeout / 1000:
            if await self.is_page_loaded():
                if self.test_logger:
                    self.test_logger.info(f"{self.get_page_identifier()} loaded successfully")
                return
            
            await asyncio.sleep(1)
        
        raise TimeoutError(f"{self.get_page_identifier()} did not load within {timeout}ms")
    
    async def navigate_to(self, url: str, timeout: Optional[int] = None) -> None:
        """Navigate to a URL."""
        timeout = timeout or self.navigation_timeout
        
        if self.test_logger:
            self.test_logger.action(f"Navigating to {url}")
        
        log_browser_action("navigate", url=url)
        
        await self.page.goto(url, timeout=timeout)
        await self.wait_for_page_load(timeout)
    
    async def click_element(
        self,
        selector: str,
        timeout: Optional[int] = None,
        force: bool = False
    ) -> None:
        """Click an element."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action(f"Clicking element: {selector}")
        
        log_browser_action("click", selector=selector)
        
        await self.page.click(selector, timeout=timeout, force=force)
    
    async def fill_element(
        self,
        selector: str,
        value: str,
        timeout: Optional[int] = None
    ) -> None:
        """Fill an input element."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action(f"Filling element {selector} with value")
        
        log_browser_action("fill", selector=selector, value="***" if "password" in selector.lower() else value)
        
        await self.page.fill(selector, value, timeout=timeout)
    
    async def type_text(
        self,
        selector: str,
        text: str,
        delay: Optional[int] = None,
        timeout: Optional[int] = None
    ) -> None:
        """Type text into an element."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action(f"Typing text into element: {selector}")
        
        log_browser_action("type", selector=selector, text=text)
        
        await self.page.type(selector, text, delay=delay, timeout=timeout)
    
    async def wait_for_element(
        self,
        selector: str,
        state: str = "visible",
        timeout: Optional[int] = None
    ) -> Locator:
        """Wait for an element to be in a specific state."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action(f"Waiting for element {selector} to be {state}")
        
        log_browser_action("wait_for_element", selector=selector, state=state)
        
        locator = self.page.locator(selector)
        await locator.wait_for(state=state, timeout=timeout)
        return locator
    
    async def is_element_visible(
        self,
        selector: str,
        timeout: Optional[int] = None
    ) -> bool:
        """Check if an element is visible."""
        timeout = timeout or self.timeout
        
        try:
            await self.wait_for_element(selector, "visible", timeout)
            return True
        except Exception:
            return False
    
    async def is_element_hidden(
        self,
        selector: str,
        timeout: Optional[int] = None
    ) -> bool:
        """Check if an element is hidden."""
        timeout = timeout or self.timeout
        
        try:
            await self.wait_for_element(selector, "hidden", timeout)
            return True
        except Exception:
            return False
    
    async def get_element_text(
        self,
        selector: str,
        timeout: Optional[int] = None
    ) -> str:
        """Get text content of an element."""
        timeout = timeout or self.timeout
        
        log_browser_action("get_text", selector=selector)
        
        element = await self.wait_for_element(selector, "visible", timeout)
        return await element.text_content() or ""
    
    async def get_element_attribute(
        self,
        selector: str,
        attribute: str,
        timeout: Optional[int] = None
    ) -> Optional[str]:
        """Get an attribute value from an element."""
        timeout = timeout or self.timeout
        
        log_browser_action("get_attribute", selector=selector, attribute=attribute)
        
        element = await self.wait_for_element(selector, "visible", timeout)
        return await element.get_attribute(attribute)
    
    async def select_option(
        self,
        selector: str,
        value: Union[str, List[str]],
        timeout: Optional[int] = None
    ) -> None:
        """Select option(s) from a select element."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action(f"Selecting option {value} from {selector}")
        
        log_browser_action("select_option", selector=selector, value=value)
        
        await self.page.select_option(selector, value, timeout=timeout)
    
    async def upload_file(
        self,
        selector: str,
        file_path: str,
        timeout: Optional[int] = None
    ) -> None:
        """Upload a file to a file input element."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action(f"Uploading file {file_path} to {selector}")
        
        log_browser_action("upload_file", selector=selector, file_path=file_path)
        
        await self.page.set_input_files(selector, file_path, timeout=timeout)
    
    async def wait_for_navigation(
        self,
        timeout: Optional[int] = None,
        wait_until: str = "networkidle"
    ) -> None:
        """Wait for navigation to complete."""
        timeout = timeout or self.navigation_timeout
        
        if self.test_logger:
            self.test_logger.action("Waiting for navigation")
        
        log_browser_action("wait_for_navigation", wait_until=wait_until)
        
        await self.page.wait_for_load_state(wait_until, timeout=timeout)
    
    async def scroll_to_element(
        self,
        selector: str,
        timeout: Optional[int] = None
    ) -> None:
        """Scroll to an element."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action(f"Scrolling to element: {selector}")
        
        log_browser_action("scroll_to_element", selector=selector)
        
        element = await self.wait_for_element(selector, "attached", timeout)
        await element.scroll_into_view_if_needed()
    
    async def hover_element(
        self,
        selector: str,
        timeout: Optional[int] = None
    ) -> None:
        """Hover over an element."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action(f"Hovering over element: {selector}")
        
        log_browser_action("hover", selector=selector)
        
        await self.page.hover(selector, timeout=timeout)
    
    async def press_key(
        self,
        key: str,
        selector: Optional[str] = None,
        timeout: Optional[int] = None
    ) -> None:
        """Press a key, optionally on a specific element."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action(f"Pressing key {key}" + (f" on {selector}" if selector else ""))
        
        log_browser_action("press_key", key=key, selector=selector)
        
        if selector:
            await self.page.press(selector, key, timeout=timeout)
        else:
            await self.page.keyboard.press(key)
    
    async def take_screenshot(
        self,
        path: Optional[str] = None,
        full_page: bool = False
    ) -> bytes:
        """Take a screenshot of the page."""
        if self.test_logger:
            self.test_logger.action("Taking screenshot")
        
        log_browser_action("screenshot", path=path, full_page=full_page)
        
        return await self.page.screenshot(path=path, full_page=full_page)
    
    async def get_page_title(self) -> str:
        """Get the page title."""
        return await self.page.title()
    
    async def get_current_url(self) -> str:
        """Get the current URL."""
        return self.page.url
    
    async def wait_for_url(
        self,
        url_pattern: str,
        timeout: Optional[int] = None
    ) -> None:
        """Wait for URL to match a pattern."""
        timeout = timeout or self.navigation_timeout
        
        if self.test_logger:
            self.test_logger.action(f"Waiting for URL pattern: {url_pattern}")
        
        log_browser_action("wait_for_url", url_pattern=url_pattern)
        
        await self.page.wait_for_url(url_pattern, timeout=timeout)
    
    async def execute_javascript(
        self,
        script: str,
        *args: Any
    ) -> Any:
        """Execute JavaScript in the page context."""
        if self.test_logger:
            self.test_logger.action("Executing JavaScript")
        
        log_browser_action("execute_javascript", script=script[:100] + "..." if len(script) > 100 else script)
        
        return await self.page.evaluate(script, *args)
    
    async def wait_for_condition(
        self,
        condition_func,
        timeout: Optional[int] = None,
        poll_interval: float = 1.0
    ) -> Any:
        """Wait for a custom condition to be true."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.action("Waiting for custom condition")
        
        start_time = asyncio.get_event_loop().time()
        
        while asyncio.get_event_loop().time() - start_time < timeout / 1000:
            try:
                result = await condition_func()
                if result:
                    return result
            except Exception:
                pass  # Continue waiting
            
            await asyncio.sleep(poll_interval)
        
        raise TimeoutError(f"Condition not met within {timeout}ms")
    
    # Assertion helpers
    async def assert_element_visible(
        self,
        selector: str,
        timeout: Optional[int] = None
    ) -> None:
        """Assert that an element is visible."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.assertion(f"Element {selector} is visible", True)
        
        await expect(self.page.locator(selector)).to_be_visible(timeout=timeout)
    
    async def assert_element_hidden(
        self,
        selector: str,
        timeout: Optional[int] = None
    ) -> None:
        """Assert that an element is hidden."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.assertion(f"Element {selector} is hidden", True)
        
        await expect(self.page.locator(selector)).to_be_hidden(timeout=timeout)
    
    async def assert_element_text(
        self,
        selector: str,
        expected_text: str,
        timeout: Optional[int] = None
    ) -> None:
        """Assert that an element contains specific text."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.assertion(f"Element {selector} contains text '{expected_text}'", True)
        
        await expect(self.page.locator(selector)).to_contain_text(expected_text, timeout=timeout)
    
    async def assert_page_title(
        self,
        expected_title: str,
        timeout: Optional[int] = None
    ) -> None:
        """Assert that the page has a specific title."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.assertion(f"Page title is '{expected_title}'", True)
        
        await expect(self.page).to_have_title(expected_title, timeout=timeout)
    
    async def assert_url_contains(
        self,
        url_fragment: str,
        timeout: Optional[int] = None
    ) -> None:
        """Assert that the current URL contains a specific fragment."""
        timeout = timeout or self.timeout
        
        if self.test_logger:
            self.test_logger.assertion(f"URL contains '{url_fragment}'", True)
        
        await expect(self.page).to_have_url(f"*{url_fragment}*", timeout=timeout)