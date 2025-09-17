"""Logging utilities for Azure ML automation framework."""

import json
import logging
import sys
import time
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, Optional
from uuid import uuid4

import structlog
from colorama import Fore, Style, init as colorama_init

from .config import config

# Initialize colorama for cross-platform colored output
colorama_init(autoreset=True)


def setup_logging() -> None:
    """Set up structured logging configuration."""
    
    # Create logs directory with safe path handling
    try:
        artifacts_path = config.artifacts.path
        # Ensure we have a valid path, not an environment variable
        if not artifacts_path or ';' in artifacts_path or len(artifacts_path) > 260:
            artifacts_path = "./test-results"
        logs_dir = Path(artifacts_path) / "logs"
        logs_dir.mkdir(parents=True, exist_ok=True)
    except Exception:
        # Fallback to current directory if path creation fails
        logs_dir = Path("./logs")
        logs_dir.mkdir(parents=True, exist_ok=True)
    
    # Configure structlog
    structlog.configure(
        processors=[
            structlog.stdlib.filter_by_level,
            structlog.stdlib.add_logger_name,
            structlog.stdlib.add_log_level,
            structlog.stdlib.PositionalArgumentsFormatter(),
            structlog.processors.TimeStamper(fmt="iso"),
            structlog.processors.StackInfoRenderer(),
            structlog.processors.format_exc_info,
            structlog.processors.UnicodeDecoder(),
            structlog.processors.JSONRenderer() if config.logging.format == "json" 
            else structlog.dev.ConsoleRenderer(colors=True),
        ],
        context_class=dict,
        logger_factory=structlog.stdlib.LoggerFactory(),
        wrapper_class=structlog.stdlib.BoundLogger,
        cache_logger_on_first_use=True,
    )
    
    # Configure standard library logging
    logging.basicConfig(
        format="%(message)s",
        stream=sys.stdout,
        level=getattr(logging, config.logging.level.upper()),
    )
    
    # Add file handlers
    root_logger = logging.getLogger()
    
    # Combined log file
    combined_handler = logging.FileHandler(logs_dir / "combined.log")
    combined_handler.setLevel(logging.DEBUG)
    combined_formatter = logging.Formatter(
        '{"timestamp": "%(asctime)s", "level": "%(levelname)s", "message": "%(message)s"}'
    )
    combined_handler.setFormatter(combined_formatter)
    root_logger.addHandler(combined_handler)
    
    # Error log file
    error_handler = logging.FileHandler(logs_dir / "error.log")
    error_handler.setLevel(logging.ERROR)
    error_handler.setFormatter(combined_formatter)
    root_logger.addHandler(error_handler)


# Set up logging
setup_logging()

# Get the main logger
logger = structlog.get_logger("azure-ml-automation")


class TestLogger:
    """Test-specific logger with correlation ID and structured logging."""
    
    def __init__(self, test_name: str, correlation_id: Optional[str] = None):
        self.test_name = test_name
        self.correlation_id = correlation_id or self._generate_correlation_id()
        self._logger = logger.bind(
            test_name=test_name,
            correlation_id=self.correlation_id
        )
    
    def _generate_correlation_id(self) -> str:
        """Generate a unique correlation ID for the test."""
        timestamp = int(time.time() * 1000)
        unique_id = str(uuid4())[:8]
        return f"test-{timestamp}-{unique_id}"
    
    def info(self, message: str, **kwargs: Any) -> None:
        """Log info message."""
        self._logger.info(message, **kwargs)
    
    def error(self, message: str, **kwargs: Any) -> None:
        """Log error message."""
        self._logger.error(message, **kwargs)
    
    def warning(self, message: str, **kwargs: Any) -> None:
        """Log warning message."""
        self._logger.warning(message, **kwargs)
    
    def debug(self, message: str, **kwargs: Any) -> None:
        """Log debug message."""
        self._logger.debug(message, **kwargs)
    
    def step(self, step_name: str, **kwargs: Any) -> None:
        """Log a test step."""
        self.info(f"Step: {step_name}", step=step_name, **kwargs)
    
    def action(self, action_name: str, **kwargs: Any) -> None:
        """Log a test action."""
        self.info(f"Action: {action_name}", action=action_name, **kwargs)
    
    def assertion(self, assertion: str, result: bool, **kwargs: Any) -> None:
        """Log a test assertion."""
        status = "PASSED" if result else "FAILED"
        level = "info" if result else "error"
        message = f"Assertion: {assertion} - {status}"
        
        getattr(self._logger, level)(
            message, 
            assertion=assertion, 
            result=result, 
            **kwargs
        )
    
    def get_correlation_id(self) -> str:
        """Get the correlation ID for this test logger."""
        return self.correlation_id


class PerformanceLogger:
    """Performance logging utilities."""
    
    def __init__(self):
        self._start_times: Dict[str, float] = {}
        self._logger = logger.bind(component="performance")
    
    def start(self, operation: str) -> None:
        """Start timing an operation."""
        self._start_times[operation] = time.time()
        self._logger.debug(f"Performance: Started {operation}")
    
    def end(self, operation: str, **kwargs: Any) -> float:
        """End timing an operation and return duration."""
        start_time = self._start_times.get(operation)
        if not start_time:
            self._logger.warning(f"Performance: No start time found for {operation}")
            return 0.0
        
        duration = time.time() - start_time
        del self._start_times[operation]
        
        self._logger.info(
            f"Performance: {operation} completed",
            operation=operation,
            duration=duration,
            **kwargs
        )
        
        return duration
    
    async def measure(self, operation: str, func, **kwargs: Any):
        """Measure the execution time of an async function."""
        self.start(operation)
        try:
            result = await func()
            self.end(operation, success=True, **kwargs)
            return result
        except Exception as error:
            self.end(operation, success=False, error=str(error), **kwargs)
            raise


# Global performance logger instance
performance_logger = PerformanceLogger()


def log_test_start(test_name: str, **kwargs: Any) -> TestLogger:
    """Create a test logger and log test start."""
    test_logger = TestLogger(test_name)
    test_logger.info("Test started", **kwargs)
    return test_logger


def log_test_end(test_logger: TestLogger, success: bool, **kwargs: Any) -> None:
    """Log test end with success status."""
    status = "passed" if success else "failed"
    test_logger.info(f"Test {status}", success=success, **kwargs)


def log_api_call(method: str, url: str, status_code: Optional[int] = None, **kwargs: Any) -> None:
    """Log an API call."""
    logger.info(
        "API call",
        method=method,
        url=url,
        status_code=status_code,
        **kwargs
    )


def log_browser_action(action: str, selector: Optional[str] = None, **kwargs: Any) -> None:
    """Log a browser action."""
    logger.debug(
        "Browser action",
        action=action,
        selector=selector,
        **kwargs
    )


def log_cli_command(command: str, args: list, **kwargs: Any) -> None:
    """Log a CLI command execution."""
    logger.debug(
        "CLI command",
        command=command,
        args=args,
        **kwargs
    )


class ColoredConsoleHandler(logging.Handler):
    """Custom console handler with colored output."""
    
    COLORS = {
        'DEBUG': Fore.CYAN,
        'INFO': Fore.GREEN,
        'WARNING': Fore.YELLOW,
        'ERROR': Fore.RED,
        'CRITICAL': Fore.MAGENTA,
    }
    
    def emit(self, record):
        try:
            color = self.COLORS.get(record.levelname, '')
            message = self.format(record)
            print(f"{color}{message}{Style.RESET_ALL}")
        except Exception:
            self.handleError(record)


# Export commonly used items
__all__ = [
    "logger",
    "TestLogger", 
    "PerformanceLogger",
    "performance_logger",
    "log_test_start",
    "log_test_end", 
    "log_api_call",
    "log_browser_action",
    "log_cli_command",
]