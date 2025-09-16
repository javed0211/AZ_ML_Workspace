"""CLI command execution utilities for Azure ML automation framework."""

import asyncio
import json
import os
import signal
import subprocess
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Optional, Any, Union

import psutil

from .config import config
from .logger import logger, TestLogger, log_cli_command


@dataclass
class CliResult:
    """Result of a CLI command execution."""
    exit_code: int
    stdout: str
    stderr: str
    duration: float
    command: str
    args: List[str]


@dataclass
class CliOptions:
    """Options for CLI command execution."""
    timeout: Optional[int] = None
    retries: Optional[int] = None
    retry_delay: Optional[int] = None
    cwd: Optional[str] = None
    env: Optional[Dict[str, str]] = None
    shell: bool = True
    encoding: str = "utf-8"
    expect_non_zero_exit: bool = False
    log_output: bool = True
    test_logger: Optional[TestLogger] = None


class CliRunner:
    """Executes CLI commands with retry logic and logging."""
    
    def __init__(self, default_options: Optional[CliOptions] = None):
        self.default_options = default_options or CliOptions()
        self.processes: set = set()
    
    async def run(
        self,
        command: str,
        args: List[str] = None,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Execute a CLI command with retry logic."""
        if args is None:
            args = []
        
        merged_options = self._merge_options(options)
        
        timeout = merged_options.timeout or config.timeouts.default // 1000  # Convert to seconds
        retries = merged_options.retries or config.retry.max_retries
        retry_delay = merged_options.retry_delay or config.retry.delay // 1000  # Convert to seconds
        
        last_error: Optional[Exception] = None
        
        for attempt in range(retries + 1):
            if attempt > 0:
                delay = retry_delay * (2 ** (attempt - 1))  # Exponential backoff
                if merged_options.test_logger:
                    merged_options.test_logger.warning(
                        f"CLI retry attempt {attempt}/{retries} after {delay}s",
                        command=command,
                        args=args,
                        attempt=attempt
                    )
                await asyncio.sleep(delay)
            
            try:
                result = await self._execute_command(command, args, merged_options, timeout)
                
                if not merged_options.expect_non_zero_exit and result.exit_code != 0:
                    raise RuntimeError(
                        f"Command failed with exit code {result.exit_code}: {result.stderr}"
                    )
                
                if merged_options.test_logger:
                    merged_options.test_logger.info(
                        "CLI command completed successfully",
                        command=command,
                        args=args,
                        exit_code=result.exit_code,
                        duration=result.duration,
                        attempt=attempt + 1
                    )
                
                return result
                
            except Exception as error:
                last_error = error
                if merged_options.test_logger:
                    merged_options.test_logger.error(
                        f"CLI command failed on attempt {attempt + 1}",
                        command=command,
                        args=args,
                        error=str(error),
                        attempt=attempt + 1
                    )
                
                if attempt == retries:
                    break
        
        raise last_error or RuntimeError("CLI command failed after all retries")
    
    def _merge_options(self, options: Optional[CliOptions]) -> CliOptions:
        """Merge provided options with defaults."""
        if options is None:
            return self.default_options
        
        return CliOptions(
            timeout=options.timeout or self.default_options.timeout,
            retries=options.retries or self.default_options.retries,
            retry_delay=options.retry_delay or self.default_options.retry_delay,
            cwd=options.cwd or self.default_options.cwd,
            env={**(self.default_options.env or {}), **(options.env or {})},
            shell=options.shell if options.shell is not None else self.default_options.shell,
            encoding=options.encoding or self.default_options.encoding,
            expect_non_zero_exit=options.expect_non_zero_exit,
            log_output=options.log_output if options.log_output is not None else self.default_options.log_output,
            test_logger=options.test_logger or self.default_options.test_logger
        )
    
    async def _execute_command(
        self,
        command: str,
        args: List[str],
        options: CliOptions,
        timeout: int
    ) -> CliResult:
        """Execute a single command."""
        start_time = time.time()
        
        # Prepare environment
        env = {**os.environ, **(options.env or {})}
        cwd = options.cwd or os.getcwd()
        
        # Log command execution
        log_cli_command(command, args, cwd=cwd)
        if options.test_logger:
            options.test_logger.action(
                f"Executing CLI command: {command} {' '.join(args)}",
                command=command,
                args=args,
                cwd=cwd
            )
        
        # Prepare command
        if options.shell:
            cmd = f"{command} {' '.join(args)}"
            process_args = cmd
        else:
            process_args = [command] + args
        
        try:
            # Start process
            process = await asyncio.create_subprocess_shell(
                process_args if options.shell else None,
                **([] if options.shell else [process_args]),
                stdout=asyncio.subprocess.PIPE,
                stderr=asyncio.subprocess.PIPE,
                cwd=cwd,
                env=env
            )
            
            self.processes.add(process)
            
            # Wait for completion with timeout
            try:
                stdout_data, stderr_data = await asyncio.wait_for(
                    process.communicate(),
                    timeout=timeout
                )
            except asyncio.TimeoutError:
                if options.test_logger:
                    options.test_logger.error(
                        "CLI command timed out",
                        command=command,
                        args=args,
                        timeout=timeout
                    )
                await self._kill_process(process)
                raise RuntimeError(f"Command timed out after {timeout}s")
            
            # Decode output
            stdout = stdout_data.decode(options.encoding).strip()
            stderr = stderr_data.decode(options.encoding).strip()
            
            # Log output if requested
            if options.log_output:
                if stdout:
                    logger.debug("CLI stdout", command=command, output=stdout)
                if stderr:
                    logger.debug("CLI stderr", command=command, output=stderr)
            
            duration = time.time() - start_time
            
            result = CliResult(
                exit_code=process.returncode or 0,
                stdout=stdout,
                stderr=stderr,
                duration=duration,
                command=command,
                args=args
            )
            
            return result
            
        except Exception as error:
            if options.test_logger:
                options.test_logger.error(
                    "CLI process error",
                    command=command,
                    args=args,
                    error=str(error)
                )
            raise
        finally:
            self.processes.discard(process)
    
    async def _kill_process(self, process: asyncio.subprocess.Process) -> None:
        """Kill a process and its children."""
        if process.returncode is not None:
            return  # Process already terminated
        
        try:
            # Try to get the process and kill its tree
            if process.pid:
                parent = psutil.Process(process.pid)
                children = parent.children(recursive=True)
                
                # Terminate children first
                for child in children:
                    try:
                        child.terminate()
                    except psutil.NoSuchProcess:
                        pass
                
                # Terminate parent
                try:
                    parent.terminate()
                except psutil.NoSuchProcess:
                    pass
                
                # Wait a bit for graceful termination
                await asyncio.sleep(2)
                
                # Force kill if still running
                for child in children:
                    try:
                        if child.is_running():
                            child.kill()
                    except psutil.NoSuchProcess:
                        pass
                
                try:
                    if parent.is_running():
                        parent.kill()
                except psutil.NoSuchProcess:
                    pass
        
        except Exception as error:
            logger.warning("Failed to kill process tree", pid=process.pid, error=str(error))
            
            # Fallback to simple termination
            try:
                process.terminate()
                await asyncio.sleep(1)
                if process.returncode is None:
                    process.kill()
            except Exception as kill_error:
                logger.error("Failed to kill process", pid=process.pid, error=str(kill_error))
    
    async def cleanup(self) -> None:
        """Clean up all running processes."""
        cleanup_tasks = []
        for process in list(self.processes):
            cleanup_tasks.append(self._kill_process(process))
        
        if cleanup_tasks:
            await asyncio.gather(*cleanup_tasks, return_exceptions=True)
        
        self.processes.clear()
    
    # Azure CLI specific methods
    async def az_login(self, options: Optional[CliOptions] = None) -> CliResult:
        """Execute az login."""
        return await self.run("az", ["login"], options)
    
    async def az_login_service_principal(
        self,
        client_id: str,
        client_secret: str,
        tenant_id: str,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Execute az login with service principal."""
        safe_options = options or CliOptions()
        safe_options.log_output = False  # Don't log secrets
        
        return await self.run(
            "az",
            [
                "login",
                "--service-principal",
                "-u", client_id,
                "-p", client_secret,
                "--tenant", tenant_id
            ],
            safe_options
        )
    
    async def az_set_subscription(
        self,
        subscription_id: str,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Set Azure subscription."""
        return await self.run("az", ["account", "set", "--subscription", subscription_id], options)
    
    async def az_ml_compute_start(
        self,
        compute_name: str,
        workspace_name: str,
        resource_group: str,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Start Azure ML compute instance."""
        return await self.run(
            "az",
            [
                "ml", "compute", "start",
                "--name", compute_name,
                "--workspace-name", workspace_name,
                "--resource-group", resource_group
            ],
            options
        )
    
    async def az_ml_compute_stop(
        self,
        compute_name: str,
        workspace_name: str,
        resource_group: str,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Stop Azure ML compute instance."""
        return await self.run(
            "az",
            [
                "ml", "compute", "stop",
                "--name", compute_name,
                "--workspace-name", workspace_name,
                "--resource-group", resource_group
            ],
            options
        )
    
    async def az_ml_compute_show(
        self,
        compute_name: str,
        workspace_name: str,
        resource_group: str,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Show Azure ML compute instance details."""
        return await self.run(
            "az",
            [
                "ml", "compute", "show",
                "--name", compute_name,
                "--workspace-name", workspace_name,
                "--resource-group", resource_group,
                "--output", "json"
            ],
            options
        )
    
    async def az_ml_job_create(
        self,
        job_file: str,
        workspace_name: str,
        resource_group: str,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Create Azure ML job."""
        return await self.run(
            "az",
            [
                "ml", "job", "create",
                "--file", job_file,
                "--workspace-name", workspace_name,
                "--resource-group", resource_group
            ],
            options
        )
    
    # PowerShell methods (for Windows compatibility)
    async def run_powershell(
        self,
        script: str,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Execute PowerShell script."""
        return await self.run("powershell", ["-Command", script], options)
    
    async def run_powershell_file(
        self,
        script_path: str,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Execute PowerShell script file."""
        return await self.run("powershell", ["-File", script_path], options)
    
    # Utility methods
    async def wait_for_pattern(
        self,
        command: str,
        args: List[str],
        pattern: str,
        max_wait_time: int = 60,
        options: Optional[CliOptions] = None
    ) -> CliResult:
        """Wait for command output to match a pattern."""
        import re
        
        start_time = time.time()
        pattern_regex = re.compile(pattern)
        last_result: Optional[CliResult] = None
        
        safe_options = options or CliOptions()
        safe_options.retries = 0  # No retries for individual attempts
        
        while time.time() - start_time < max_wait_time:
            try:
                last_result = await self.run(command, args, safe_options)
                if (pattern_regex.search(last_result.stdout) or 
                    pattern_regex.search(last_result.stderr)):
                    return last_result
            except Exception:
                # Continue waiting even if command fails
                pass
            
            await asyncio.sleep(2)  # Wait 2 seconds between checks
        
        raise RuntimeError(f"Pattern '{pattern}' not found within {max_wait_time}s")
    
    def parse_json_output(self, result: CliResult) -> Any:
        """Parse JSON output from CLI result."""
        try:
            return json.loads(result.stdout)
        except json.JSONDecodeError as error:
            logger.error("Failed to parse JSON output", stdout=result.stdout, error=str(error))
            raise RuntimeError(f"Invalid JSON output: {error}")


# Global CLI runner instance
cli_runner = CliRunner()