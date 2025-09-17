# Test Structure Documentation

This document describes the organized test structure for the Azure ML Workspace automation project.

## Test Organization

Tests are organized into logical groups based on functionality and service area:

### 1. ML Workspace Tests (`/tests/ml_workspace/`)
Tests related to core Azure ML workspace functionality:

- **`workspace_management/`** - Compute instances, workspace access, authentication
- **`notebooks/`** - Notebook creation, execution, and management
- **`datasets/`** - Dataset upload, management, and versioning
- **`models/`** - Model training, registration, and deployment
- **`experiments/`** - Experiment tracking and job management

### 2. AI Document Search Tests (`/tests/ai_document_search/`)
Tests for AI-powered document search capabilities:

- **`search_functionality/`** - Basic and semantic search features
- **`document_processing/`** - Text extraction, OCR, classification
- **`indexing/`** - Document indexing and metadata management
- **`cognitive_search/`** - Azure Cognitive Search integration

### 3. Speech Services Tests (`/tests/speech_services/`)
Tests for Azure Speech Services:

- **`speech_to_text/`** - Audio transcription and recognition
- **`text_to_speech/`** - Speech synthesis and voice options
- **`speech_translation/`** - Multi-language speech translation
- **`voice_recognition/`** - Voice pattern recognition and custom models

### 4. Integration Tests (`/tests/integration/`)
End-to-end workflow tests that span multiple services:

- Cross-service integration scenarios
- Complete ML pipelines
- Multi-step workflows

### 5. Performance Tests (`/tests/performance/`)
Load testing and performance validation:

- Concurrent operations testing
- Throughput measurements
- Memory usage monitoring
- API rate limiting tests

### 6. Security Tests (`/tests/security/`)
Authentication, authorization, and security validation:

- Authentication flows
- Role-based access control
- Token management
- Security headers and policies

## Running Tests

### Run All Tests
```bash
pytest
```

### Run Tests by Category
```bash
# ML Workspace tests
pytest tests/ml_workspace/

# AI Document Search tests
pytest tests/ai_document_search/

# Speech Services tests
pytest tests/speech_services/

# Integration tests
pytest tests/integration/

# Performance tests
pytest tests/performance/

# Security tests
pytest tests/security/
```

### Run Tests by Marker
```bash
# Run only notebook tests
pytest -m notebook

# Run only compute tests
pytest -m compute

# Run only document search tests
pytest -m document_search

# Run only speech-to-text tests
pytest -m speech_to_text

# Run only performance tests
pytest -m performance

# Run only security tests
pytest -m security
```

### Run Tests by Specific Functionality
```bash
# Run workspace management tests
pytest tests/ml_workspace/workspace_management/

# Run document processing tests
pytest tests/ai_document_search/document_processing/

# Run text-to-speech tests
pytest tests/speech_services/text_to_speech/
```

## Test Markers

The following pytest markers are available:

### Service-Specific Markers
- `@pytest.mark.notebook` - Notebook execution tests
- `@pytest.mark.compute` - Compute instance lifecycle tests
- `@pytest.mark.dataset` - Dataset management tests
- `@pytest.mark.model` - Model management tests
- `@pytest.mark.experiment` - Experiment/job tests
- `@pytest.mark.document_search` - Document search tests
- `@pytest.mark.document_processing` - Document processing tests
- `@pytest.mark.speech_to_text` - Speech-to-text tests
- `@pytest.mark.text_to_speech` - Text-to-speech tests
- `@pytest.mark.speech_translation` - Speech translation tests
- `@pytest.mark.voice_recognition` - Voice recognition tests

### General Markers
- `@pytest.mark.integration` - Integration tests
- `@pytest.mark.performance` - Performance tests
- `@pytest.mark.security` - Security tests
- `@pytest.mark.azure` - Tests requiring Azure resources
- `@pytest.mark.slow` - Long-running tests

## Test Configuration

Tests are configured through:
- `conftest.py` - Global fixtures and configuration
- Environment variables for Azure credentials
- Configuration files for service endpoints

## Adding New Tests

When adding new tests:

1. Place them in the appropriate category folder
2. Use descriptive test names that indicate what is being tested
3. Add appropriate pytest markers
4. Include proper logging and assertions
5. Follow the existing test patterns and structure

## Test Data

Test data is stored in the `/test-data/` directory and includes:
- Sample datasets
- Sample notebooks
- Audio files for speech testing
- Configuration templates

## Artifacts

Test artifacts are stored in `/test-results/` including:
- Screenshots on test failures
- Video recordings of test runs
- Trace files for debugging
- Performance metrics
- Test reports