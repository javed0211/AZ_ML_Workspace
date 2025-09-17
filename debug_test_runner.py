#!/usr/bin/env python3
"""
Debug script to help identify where tests are hanging.
This script runs tests with detailed logging and timeout handling.
"""

import asyncio
import sys
import signal
import time
from pathlib import Path

def timeout_handler(signum, frame):
    print("\n🚨 TEST TIMEOUT DETECTED!")
    print("The test has been running for too long and was terminated.")
    print("Check the logs and HTML report for details.")
    sys.exit(124)

async def run_test_with_timeout():
    """Run a specific test with timeout and detailed logging."""
    
    print("🔍 Debug Test Runner")
    print("=" * 50)
    
    # Set up timeout (5 minutes)
    if hasattr(signal, 'SIGALRM'):  # Unix systems
        signal.signal(signal.SIGALRM, timeout_handler)
        signal.alarm(300)  # 5 minutes
    
    try:
        import subprocess
        
        # Run the simple test first
        print("🧪 Running simple authentication test...")
        result = subprocess.run([
            sys.executable, "-m", "pytest",
            "tests/ml_workspace/workspace_management/test_pim_compute_vscode_workflow.py::TestPIMComputeVSCodeWorkflow::test_pim_manager_uses_managed_identity",
            "-v", "-s", "--tb=short", "--maxfail=1"
        ], capture_output=True, text=True, timeout=60)
        
        print(f"Exit code: {result.returncode}")
        if result.stdout:
            print("STDOUT:")
            print(result.stdout)
        if result.stderr:
            print("STDERR:")
            print(result.stderr)
            
        if result.returncode == 0:
            print("✅ Simple test passed!")
        else:
            print("❌ Simple test failed!")
            return
            
        # Now try the navigation test with shorter timeout
        print("\n🧪 Running navigation test...")
        result2 = subprocess.run([
            sys.executable, "-m", "pytest",
            "tests/ml_workspace/workspace_management/test_pim_compute_vscode_workflow.py::TestPIMComputeVSCodeWorkflow::test_azure_ml_page_navigation_only",
            "-v", "-s", "--tb=short", "--maxfail=1"
        ], capture_output=True, text=True, timeout=120)
        
        print(f"Exit code: {result2.returncode}")
        if result2.stdout:
            print("STDOUT:")
            print(result2.stdout)
        if result2.stderr:
            print("STDERR:")
            print(result2.stderr)
            
    except subprocess.TimeoutExpired:
        print("🚨 TEST TIMED OUT!")
        print("The test is likely stuck in an infinite loop or waiting for something.")
        
    except Exception as e:
        print(f"❌ Error running tests: {e}")
        
    finally:
        if hasattr(signal, 'SIGALRM'):
            signal.alarm(0)  # Cancel the alarm
            
    # Show report location
    report_path = Path("test-results/reports/pytest-report.html")
    if report_path.exists():
        print(f"\n📊 HTML Report available at: {report_path.absolute()}")
        print("Open this file in your browser to see detailed test results.")
    
    # Show log files
    log_path = Path("test-results/logs")
    if log_path.exists():
        print(f"\n📝 Log files available at: {log_path.absolute()}")
        for log_file in log_path.glob("*.log"):
            print(f"  - {log_file.name}")

if __name__ == "__main__":
    asyncio.run(run_test_with_timeout())