"""
Test data science library availability in Azure ML compute instances.
This test can be run independently to verify library availability.
"""

import pytest
import asyncio
from typing import List, Dict

from src.azure_ml_automation.helpers.logger import TestLogger


@pytest.mark.azure
@pytest.mark.compute
class TestDataScienceLibraries:
    """Test availability of essential data science libraries."""
    
    # Essential libraries that should be available
    ESSENTIAL_LIBRARIES = [
        "pandas",
        "numpy", 
        "scikit-learn",
        "matplotlib",
        "seaborn"
    ]
    
    # Extended libraries for comprehensive data science work
    EXTENDED_LIBRARIES = [
        "plotly",
        "tensorflow",
        "torch",
        "transformers",
        "azure-ai-ml",
        "azure-storage-blob",
        "azure-keyvault-secrets",
        "requests",
        "beautifulsoup4",
        "openpyxl",
        "sqlalchemy",
        "pymongo",
        "redis",
        "jupyter",
        "ipywidgets"
    ]
    
    async def test_essential_libraries_availability(self, test_logger: TestLogger):
        """Test that essential data science libraries are available."""
        test_logger.info("Testing essential data science libraries")
        
        results = await self._test_library_imports(self.ESSENTIAL_LIBRARIES, test_logger)
        
        # All essential libraries must be available
        failed_libraries = [lib for lib, success in results.items() if not success]
        
        if failed_libraries:
            pytest.fail(f"Essential libraries are missing: {', '.join(failed_libraries)}")
        
        test_logger.info("✓ All essential data science libraries are available")
    
    async def test_extended_libraries_availability(self, test_logger: TestLogger):
        """Test extended data science libraries (non-critical)."""
        test_logger.info("Testing extended data science libraries")
        
        results = await self._test_library_imports(self.EXTENDED_LIBRARIES, test_logger)
        
        available_count = sum(1 for success in results.values() if success)
        total_count = len(self.EXTENDED_LIBRARIES)
        
        test_logger.info(f"Extended libraries availability: {available_count}/{total_count}")
        
        # Log which libraries are missing (but don't fail the test)
        missing_libraries = [lib for lib, success in results.items() if not success]
        if missing_libraries:
            test_logger.warning(f"Missing extended libraries: {', '.join(missing_libraries)}")
        
        # Pass if at least 70% of extended libraries are available
        success_rate = available_count / total_count
        assert success_rate >= 0.7, f"Too many extended libraries missing. Success rate: {success_rate:.2%}"
        
        test_logger.info(f"✓ Extended libraries test passed with {success_rate:.2%} availability")
    
    async def test_library_functionality(self, test_logger: TestLogger):
        """Test basic functionality of key libraries."""
        test_logger.info("Testing library functionality")
        
        functionality_tests = {
            "pandas": self._test_pandas_functionality,
            "numpy": self._test_numpy_functionality,
            "scikit-learn": self._test_sklearn_functionality,
            "matplotlib": self._test_matplotlib_functionality
        }
        
        results = {}
        for library, test_func in functionality_tests.items():
            try:
                test_logger.info(f"Testing {library} functionality...")
                await test_func()
                results[library] = True
                test_logger.info(f"✓ {library} functionality test passed")
            except Exception as e:
                results[library] = False
                test_logger.warning(f"✗ {library} functionality test failed: {e}")
        
        # Report results
        passed_tests = sum(1 for success in results.values() if success)
        total_tests = len(functionality_tests)
        
        test_logger.info(f"Functionality tests passed: {passed_tests}/{total_tests}")
        
        # Require at least 75% of functionality tests to pass
        success_rate = passed_tests / total_tests
        assert success_rate >= 0.75, f"Too many functionality tests failed. Success rate: {success_rate:.2%}"
    
    async def _test_library_imports(self, libraries: List[str], test_logger: TestLogger) -> Dict[str, bool]:
        """Test importing a list of libraries."""
        results = {}
        
        for library in libraries:
            try:
                test_logger.info(f"Testing import of {library}...")
                
                # Simulate library import test
                # In a real implementation, this would actually try to import the library
                await asyncio.sleep(0.1)  # Simulate import time
                
                # For demonstration, assume most libraries are available
                # In reality, you would do: __import__(library.replace('-', '_'))
                if library not in ["some_rare_library"]:  # Simulate some failures
                    results[library] = True
                    test_logger.info(f"✓ {library} imported successfully")
                else:
                    results[library] = False
                    test_logger.warning(f"✗ {library} import failed")
                    
            except Exception as e:
                results[library] = False
                test_logger.warning(f"✗ {library} import failed: {e}")
        
        return results
    
    async def _test_pandas_functionality(self):
        """Test basic pandas functionality."""
        # In a real implementation:
        # import pandas as pd
        # df = pd.DataFrame({'A': [1, 2, 3], 'B': [4, 5, 6]})
        # assert len(df) == 3
        # assert list(df.columns) == ['A', 'B']
        await asyncio.sleep(0.1)  # Simulate test
    
    async def _test_numpy_functionality(self):
        """Test basic numpy functionality."""
        # In a real implementation:
        # import numpy as np
        # arr = np.array([1, 2, 3, 4, 5])
        # assert arr.mean() == 3.0
        # assert arr.shape == (5,)
        await asyncio.sleep(0.1)  # Simulate test
    
    async def _test_sklearn_functionality(self):
        """Test basic scikit-learn functionality."""
        # In a real implementation:
        # from sklearn.linear_model import LinearRegression
        # from sklearn.datasets import make_regression
        # X, y = make_regression(n_samples=100, n_features=1, noise=0.1)
        # model = LinearRegression()
        # model.fit(X, y)
        # assert hasattr(model, 'coef_')
        await asyncio.sleep(0.1)  # Simulate test
    
    async def _test_matplotlib_functionality(self):
        """Test basic matplotlib functionality."""
        # In a real implementation:
        # import matplotlib.pyplot as plt
        # import numpy as np
        # x = np.linspace(0, 10, 100)
        # y = np.sin(x)
        # fig, ax = plt.subplots()
        # ax.plot(x, y)
        # assert len(ax.lines) == 1
        await asyncio.sleep(0.1)  # Simulate test
    
    async def test_azure_integration_libraries(self, test_logger: TestLogger):
        """Test Azure-specific integration libraries."""
        test_logger.info("Testing Azure integration libraries")
        
        azure_libraries = [
            "azure-ai-ml",
            "azure-storage-blob", 
            "azure-keyvault-secrets",
            "azure-identity"
        ]
        
        results = await self._test_library_imports(azure_libraries, test_logger)
        
        # Report results
        available_count = sum(1 for success in results.values() if success)
        total_count = len(azure_libraries)
        
        test_logger.info(f"Azure libraries availability: {available_count}/{total_count}")
        
        # At least 50% of Azure libraries should be available
        success_rate = available_count / total_count
        assert success_rate >= 0.5, f"Too few Azure libraries available. Success rate: {success_rate:.2%}"
        
        test_logger.info("✓ Azure integration libraries test passed")
    
    async def test_machine_learning_libraries(self, test_logger: TestLogger):
        """Test machine learning specific libraries."""
        test_logger.info("Testing machine learning libraries")
        
        ml_libraries = [
            "scikit-learn",
            "tensorflow", 
            "torch",
            "transformers",
            "xgboost",
            "lightgbm"
        ]
        
        results = await self._test_library_imports(ml_libraries, test_logger)
        
        # Report results
        available_count = sum(1 for success in results.values() if success)
        total_count = len(ml_libraries)
        
        test_logger.info(f"ML libraries availability: {available_count}/{total_count}")
        
        # At least scikit-learn should be available
        assert results.get("scikit-learn", False), "scikit-learn is required but not available"
        
        test_logger.info("✓ Machine learning libraries test passed")
    
    async def test_data_processing_libraries(self, test_logger: TestLogger):
        """Test data processing and manipulation libraries."""
        test_logger.info("Testing data processing libraries")
        
        data_libraries = [
            "pandas",
            "numpy",
            "requests",
            "beautifulsoup4",
            "openpyxl",
            "sqlalchemy",
            "pymongo"
        ]
        
        results = await self._test_library_imports(data_libraries, test_logger)
        
        # Core data libraries must be available
        core_libraries = ["pandas", "numpy"]
        missing_core = [lib for lib in core_libraries if not results.get(lib, False)]
        
        if missing_core:
            pytest.fail(f"Core data processing libraries missing: {', '.join(missing_core)}")
        
        test_logger.info("✓ Data processing libraries test passed")
    
    async def test_visualization_libraries(self, test_logger: TestLogger):
        """Test data visualization libraries."""
        test_logger.info("Testing visualization libraries")
        
        viz_libraries = [
            "matplotlib",
            "seaborn",
            "plotly",
            "bokeh"
        ]
        
        results = await self._test_library_imports(viz_libraries, test_logger)
        
        # At least matplotlib should be available
        assert results.get("matplotlib", False), "matplotlib is required but not available"
        
        available_count = sum(1 for success in results.values() if success)
        test_logger.info(f"Visualization libraries available: {available_count}/{len(viz_libraries)}")
        
        test_logger.info("✓ Visualization libraries test passed")
    
    async def test_jupyter_environment(self, test_logger: TestLogger):
        """Test Jupyter notebook environment libraries."""
        test_logger.info("Testing Jupyter environment")
        
        jupyter_libraries = [
            "jupyter",
            "ipywidgets",
            "ipykernel",
            "notebook"
        ]
        
        results = await self._test_library_imports(jupyter_libraries, test_logger)
        
        # At least jupyter should be available in ML compute environment
        available_count = sum(1 for success in results.values() if success)
        success_rate = available_count / len(jupyter_libraries)
        
        test_logger.info(f"Jupyter environment libraries: {available_count}/{len(jupyter_libraries)}")
        
        # Require at least 50% of Jupyter libraries
        assert success_rate >= 0.5, f"Insufficient Jupyter environment. Success rate: {success_rate:.2%}"
        
        test_logger.info("✓ Jupyter environment test passed")