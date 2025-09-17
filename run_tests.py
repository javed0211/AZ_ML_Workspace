#!/usr/bin/env python3
"""Test execution script for Azure ML Workspace automation tests."""

import argparse
import subprocess
import sys
from pathlib import Path


def run_command(command: list, description: str) -> bool:
    """Run a command and return success status."""
    print(f"\n{'='*60}")
    print(f"Running: {description}")
    print(f"Command: {' '.join(command)}")
    print(f"{'='*60}")
    
    try:
        result = subprocess.run(command, check=True, capture_output=False)
        print(f"✅ {description} completed successfully")
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ {description} failed with exit code {e.returncode}")
        return False


def main():
    """Main test execution function."""
    parser = argparse.ArgumentParser(description="Run Azure ML Workspace automation tests")
    parser.add_argument(
        "--category",
        choices=[
            "all", "ml_workspace", "ai_document_search", "speech_services",
            "integration", "performance", "security", "smoke", "quick"
        ],
        default="all",
        help="Test category to run"
    )
    parser.add_argument(
        "--marker",
        help="Run tests with specific pytest marker (e.g., notebook, compute, slow)"
    )
    parser.add_argument(
        "--verbose", "-v",
        action="store_true",
        help="Run tests in verbose mode"
    )
    parser.add_argument(
        "--parallel", "-n",
        type=int,
        help="Number of parallel workers (requires pytest-xdist)"
    )
    parser.add_argument(
        "--html-report",
        action="store_true",
        help="Generate HTML test report"
    )
    parser.add_argument(
        "--coverage",
        action="store_true",
        help="Run with coverage reporting"
    )
    
    args = parser.parse_args()
    
    # Base pytest command
    base_cmd = ["python", "-m", "pytest"]
    
    # Add verbosity
    if args.verbose:
        base_cmd.append("-v")
    
    # Add parallel execution
    if args.parallel:
        base_cmd.extend(["-n", str(args.parallel)])
    
    # Add HTML report
    if args.html_report:
        base_cmd.extend(["--html=test-results/report.html", "--self-contained-html"])
    
    # Add coverage
    if args.coverage:
        base_cmd.extend(["--cov=src", "--cov-report=html:test-results/coverage"])
    
    success = True
    
    if args.marker:
        # Run tests with specific marker
        cmd = base_cmd + ["-m", args.marker]
        success = run_command(cmd, f"Tests with marker '{args.marker}'")
    
    elif args.category == "all":
        # Run all tests
        cmd = base_cmd + ["tests/"]
        success = run_command(cmd, "All tests")
    
    elif args.category == "ml_workspace":
        # Run ML workspace tests
        cmd = base_cmd + ["tests/ml_workspace/"]
        success = run_command(cmd, "ML Workspace tests")
    
    elif args.category == "ai_document_search":
        # Run AI document search tests
        cmd = base_cmd + ["tests/ai_document_search/"]
        success = run_command(cmd, "AI Document Search tests")
    
    elif args.category == "speech_services":
        # Run speech services tests
        cmd = base_cmd + ["tests/speech_services/"]
        success = run_command(cmd, "Speech Services tests")
    
    elif args.category == "integration":
        # Run integration tests
        cmd = base_cmd + ["tests/integration/"]
        success = run_command(cmd, "Integration tests")
    
    elif args.category == "performance":
        # Run performance tests
        cmd = base_cmd + ["tests/performance/"]
        success = run_command(cmd, "Performance tests")
    
    elif args.category == "security":
        # Run security tests
        cmd = base_cmd + ["tests/security/"]
        success = run_command(cmd, "Security tests")
    
    elif args.category == "smoke":
        # Run smoke tests (quick validation)
        cmd = base_cmd + ["-m", "smoke"]
        success = run_command(cmd, "Smoke tests")
    
    elif args.category == "quick":
        # Run quick tests (exclude slow tests)
        cmd = base_cmd + ["-m", "not slow", "tests/"]
        success = run_command(cmd, "Quick tests (excluding slow tests)")
    
    # Print summary
    print(f"\n{'='*60}")
    if success:
        print("🎉 Test execution completed successfully!")
    else:
        print("💥 Test execution failed!")
    print(f"{'='*60}")
    
    return 0 if success else 1


if __name__ == "__main__":
    sys.exit(main())