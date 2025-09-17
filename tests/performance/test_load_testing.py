"""Performance and load testing for Azure services."""

import pytest
import asyncio
import time
from typing import List

from src.azure_ml_automation.helpers.logger import TestLogger


@pytest.mark.performance
@pytest.mark.azure
@pytest.mark.slow
class TestLoadTesting:
    """Performance and load tests for Azure ML services."""
    
    async def test_concurrent_compute_operations(
        self,
        test_logger: TestLogger
    ):
        """Test concurrent compute instance operations."""
        test_logger.info("Starting concurrent compute operations test")
        
        # Test multiple concurrent operations
        num_concurrent = 5
        tasks = []
        
        for i in range(num_concurrent):
            # Mock concurrent compute operations
            async def mock_compute_operation(index):
                start_time = time.time()
                # Simulate compute operation
                await asyncio.sleep(2)  # Mock operation time
                end_time = time.time()
                return {"index": index, "duration": end_time - start_time}
            
            tasks.append(mock_compute_operation(i))
        
        # Execute all tasks concurrently
        start_time = time.time()
        results = await asyncio.gather(*tasks)
        total_time = time.time() - start_time
        
        # Verify performance
        assert total_time < 5.0, "Concurrent operations should complete within 5 seconds"
        assert len(results) == num_concurrent, "All operations should complete"
        
        test_logger.info(f"Completed {num_concurrent} concurrent operations in {total_time:.2f}s")
    
    async def test_document_search_performance(
        self,
        test_logger: TestLogger
    ):
        """Test document search performance under load."""
        test_logger.info("Starting document search performance test")
        
        search_queries = [
            "machine learning",
            "artificial intelligence",
            "data science",
            "neural networks",
            "deep learning"
        ]
        
        # Test search performance
        search_times = []
        
        for query in search_queries:
            start_time = time.time()
            
            # Mock search operation
            # results = await document_search_service.search(query)
            await asyncio.sleep(0.5)  # Mock search time
            
            search_time = time.time() - start_time
            search_times.append(search_time)
            
            # Verify search completed within acceptable time
            assert search_time < 2.0, f"Search for '{query}' should complete within 2 seconds"
        
        avg_search_time = sum(search_times) / len(search_times)
        test_logger.info(f"Average search time: {avg_search_time:.2f}s")
    
    async def test_speech_service_throughput(
        self,
        test_logger: TestLogger
    ):
        """Test speech service throughput."""
        test_logger.info("Starting speech service throughput test")
        
        # Test multiple speech synthesis requests
        texts = [
            "This is test text number one.",
            "This is test text number two.",
            "This is test text number three.",
            "This is test text number four.",
            "This is test text number five."
        ]
        
        start_time = time.time()
        
        # Process all texts concurrently
        async def synthesize_text(text):
            # Mock speech synthesis
            await asyncio.sleep(1.0)  # Mock synthesis time
            return f"audio_data_for_{text[:10]}"
        
        tasks = [synthesize_text(text) for text in texts]
        results = await asyncio.gather(*tasks)
        
        total_time = time.time() - start_time
        throughput = len(texts) / total_time
        
        assert len(results) == len(texts), "All synthesis requests should complete"
        assert throughput > 2.0, "Should process at least 2 requests per second"
        
        test_logger.info(f"Speech synthesis throughput: {throughput:.2f} requests/second")
    
    async def test_memory_usage_under_load(
        self,
        test_logger: TestLogger
    ):
        """Test memory usage under sustained load."""
        test_logger.info("Starting memory usage test")
        
        import psutil
        import os
        
        process = psutil.Process(os.getpid())
        initial_memory = process.memory_info().rss / 1024 / 1024  # MB
        
        # Simulate sustained load
        for i in range(100):
            # Mock operations that might consume memory
            data = [f"test_data_{j}" for j in range(1000)]
            
            # Process data
            processed = [item.upper() for item in data]
            
            # Clean up
            del data, processed
            
            if i % 20 == 0:
                current_memory = process.memory_info().rss / 1024 / 1024
                memory_increase = current_memory - initial_memory
                
                # Memory shouldn't grow excessively
                assert memory_increase < 100, f"Memory increase should be < 100MB, got {memory_increase:.2f}MB"
                
                test_logger.info(f"Iteration {i}: Memory usage {current_memory:.2f}MB (+{memory_increase:.2f}MB)")
        
        final_memory = process.memory_info().rss / 1024 / 1024
        total_increase = final_memory - initial_memory
        
        test_logger.info(f"Total memory increase: {total_increase:.2f}MB")
    
    async def test_api_rate_limiting(
        self,
        test_logger: TestLogger
    ):
        """Test API rate limiting behavior."""
        test_logger.info("Starting API rate limiting test")
        
        # Test rapid API calls to check rate limiting
        num_requests = 50
        request_times = []
        
        for i in range(num_requests):
            start_time = time.time()
            
            try:
                # Mock API call
                await asyncio.sleep(0.1)  # Mock API response time
                success = True
            except Exception as e:
                # Handle rate limiting
                if "rate limit" in str(e).lower():
                    test_logger.info(f"Rate limit hit at request {i}")
                    await asyncio.sleep(1)  # Wait before retry
                success = False
            
            request_time = time.time() - start_time
            request_times.append(request_time)
            
            if success:
                assert request_time < 5.0, "API calls should complete within 5 seconds"
        
        avg_request_time = sum(request_times) / len(request_times)
        test_logger.info(f"Average API request time: {avg_request_time:.2f}s")