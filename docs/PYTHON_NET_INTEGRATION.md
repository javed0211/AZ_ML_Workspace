# Python.NET Integration Guide

## Overview

The Azure ML Workspace Testing Framework supports Python.NET integration to leverage Python's rich machine learning ecosystem within C# test automation. This integration enables ML model validation, data science computations, and advanced analytics directly within your test scenarios using the Screenplay pattern.

## Table of Contents

1. [Setup and Configuration](#setup-and-configuration)
2. [Architecture Overview](#architecture-overview)
3. [Core Integration Patterns](#core-integration-patterns)
4. [ML Model Validation](#ml-model-validation)
5. [Data Processing and Analysis](#data-processing-and-analysis)
6. [Azure ML Python SDK Integration](#azure-ml-python-sdk-integration)
7. [Custom Python Utilities](#custom-python-utilities)
8. [Performance Considerations](#performance-considerations)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

## Setup and Configuration

### Prerequisites

- Python 3.8+ installed and accessible via PATH
- Required Python packages for ML operations
- Python.NET package (already included in project)
- C# .NET 8.0 project (existing framework)

### Python Environment Setup

1. **Create a dedicated Python environment:**

```bash
# Create virtual environment
python -m venv venv-azureml-tests

# Activate environment (Windows)
venv-azureml-tests\Scripts\activate

# Activate environment (macOS/Linux)
source venv-azureml-tests/bin/activate
```

2. **Install required Python packages:**

```bash
pip install azure-ai-ml==1.12.0
pip install azure-identity==1.15.0
pip install scikit-learn==1.3.2
pip install pandas==2.1.4
pip install numpy==1.24.4
pip install matplotlib==3.8.2
pip install seaborn==0.13.0
pip install joblib==1.3.2
pip install pytest==7.4.3
pip install jupyter==1.0.0
```

3. **Create requirements.txt:**

```txt
azure-ai-ml==1.12.0
azure-identity==1.15.0
scikit-learn==1.3.2
pandas==2.1.4
numpy==1.24.4
matplotlib==3.8.2
seaborn==0.13.0
joblib==1.3.2
pytest==7.4.3
jupyter==1.0.0
```

### C# Configuration

Update your `appsettings.json` to include Python configuration:

```json
{
  "Python": {
    "ExecutablePath": "python",
    "VirtualEnvironmentPath": "./venv-azureml-tests",
    "ScriptsPath": "./python-scripts",
    "TimeoutSeconds": 300,
    "EnableLogging": true,
    "LogLevel": "INFO"
  },
  "MachineLearning": {
    "ModelValidation": {
      "AccuracyThreshold": 0.85,
      "PrecisionThreshold": 0.80,
      "RecallThreshold": 0.80,
      "F1ScoreThreshold": 0.82
    },
    "DataValidation": {
      "MaxMissingValuePercentage": 0.05,
      "OutlierDetectionMethod": "IQR",
      "DataQualityThreshold": 0.95
    }
  }
}
```

## Architecture Overview

### Integration Architecture

```
C# Test Framework
├── Python.NET Bridge
│   ├── PythonEngine Management
│   ├── GIL (Global Interpreter Lock) Handling
│   └── Memory Management
├── ML Validation Abilities
│   ├── Model Performance Validation
│   ├── Data Quality Assessment
│   └── Statistical Analysis
├── Python Script Execution
│   ├── Inline Python Code
│   ├── External Python Scripts
│   └── Jupyter Notebook Execution
└── Result Processing
    ├── Data Serialization
    ├── Error Handling
    └── Performance Metrics
```

### Project Structure

```
AzureMLWorkspace.Tests/
├── Framework/
│   ├── Abilities/
│   │   ├── UseMLValidation.cs      # ML validation ability
│   │   └── UsePythonAnalytics.cs   # Python analytics ability
│   ├── Tasks/
│   │   ├── ValidateModel.cs        # Model validation task
│   │   └── AnalyzeData.cs          # Data analysis task
│   ├── Questions/
│   │   ├── ModelAccuracy.cs        # Model performance questions
│   │   └── DataQuality.cs          # Data quality questions
│   └── Utilities/
│       ├── PythonIntegration.cs    # Core Python.NET wrapper
│       └── MLValidationHelper.cs   # ML-specific utilities
├── python-scripts/
│   ├── ml_validation/
│   │   ├── model_validator.py      # Model validation scripts
│   │   ├── data_analyzer.py        # Data analysis scripts
│   │   └── performance_metrics.py  # Performance calculation
│   ├── azure_ml/
│   │   ├── workspace_client.py     # Azure ML workspace operations
│   │   └── compute_manager.py      # Compute instance management
│   └── utilities/
│       ├── data_utils.py           # Data processing utilities
│       └── visualization.py       # Plotting and visualization
└── TestData/
    ├── sample_datasets/            # Test datasets
    └── trained_models/             # Pre-trained models for testing
```

## Core Integration Patterns

### 1. Python.NET Ability Implementation

```csharp
// Framework/Abilities/UseMLValidation.cs
using Python.Runtime;
using System.Text.Json;

public class UseMLValidation : IAbility, IDisposable
{
    private readonly ILogger<UseMLValidation> _logger;
    private readonly PythonIntegration _pythonIntegration;
    private readonly MLValidationConfig _config;
    private bool _disposed = false;

    public UseMLValidation(
        ILogger<UseMLValidation> logger,
        PythonIntegration pythonIntegration,
        IConfiguration configuration)
    {
        _logger = logger;
        _pythonIntegration = pythonIntegration;
        _config = configuration.GetSection("MachineLearning").Get<MLValidationConfig>();
    }

    public static UseMLValidation WithConfiguration(MLValidationConfig config)
    {
        // Factory method implementation
        var serviceProvider = ServiceLocator.Current;
        var ability = serviceProvider.GetService<UseMLValidation>();
        return ability;
    }

    public async Task<ModelValidationResult> ValidateModelPerformance(
        string modelPath, 
        string testDataPath,
        ModelValidationOptions options = null)
    {
        _logger.LogInformation("Starting model validation for {ModelPath}", modelPath);

        try
        {
            using (Py.GIL())
            {
                dynamic modelValidator = Py.Import("ml_validation.model_validator");
                
                var result = modelValidator.validate_model_performance(
                    model_path: modelPath,
                    test_data_path: testDataPath,
                    accuracy_threshold: options?.AccuracyThreshold ?? _config.ModelValidation.AccuracyThreshold,
                    precision_threshold: options?.PrecisionThreshold ?? _config.ModelValidation.PrecisionThreshold,
                    recall_threshold: options?.RecallThreshold ?? _config.ModelValidation.RecallThreshold,
                    f1_threshold: options?.F1ScoreThreshold ?? _config.ModelValidation.F1ScoreThreshold
                );

                var jsonResult = result.to_json();
                return JsonSerializer.Deserialize<ModelValidationResult>(jsonResult.ToString());
            }
        }
        catch (PythonException ex)
        {
            _logger.LogError(ex, "Python execution failed during model validation");
            throw new MLValidationException($"Model validation failed: {ex.Message}", ex);
        }
    }

    public async Task<DataQualityReport> AssessDataQuality(string dataPath, DataQualityOptions options = null)
    {
        _logger.LogInformation("Assessing data quality for {DataPath}", dataPath);

        try
        {
            using (Py.GIL())
            {
                dynamic dataAnalyzer = Py.Import("ml_validation.data_analyzer");
                
                var result = dataAnalyzer.assess_data_quality(
                    data_path: dataPath,
                    missing_value_threshold: options?.MaxMissingValuePercentage ?? _config.DataValidation.MaxMissingValuePercentage,
                    outlier_method: options?.OutlierDetectionMethod ?? _config.DataValidation.OutlierDetectionMethod,
                    quality_threshold: options?.DataQualityThreshold ?? _config.DataValidation.DataQualityThreshold
                );

                var jsonResult = result.to_json();
                return JsonSerializer.Deserialize<DataQualityReport>(jsonResult.ToString());
            }
        }
        catch (PythonException ex)
        {
            _logger.LogError(ex, "Python execution failed during data quality assessment");
            throw new DataQualityException($"Data quality assessment failed: {ex.Message}", ex);
        }
    }

    public async Task<StatisticalAnalysisResult> PerformStatisticalAnalysis(
        string dataPath, 
        StatisticalAnalysisOptions options)
    {
        _logger.LogInformation("Performing statistical analysis for {DataPath}", dataPath);

        try
        {
            using (Py.GIL())
            {
                dynamic analyzer = Py.Import("ml_validation.statistical_analyzer");
                
                var result = analyzer.perform_analysis(
                    data_path: dataPath,
                    analysis_type: options.AnalysisType,
                    confidence_level: options.ConfidenceLevel,
                    include_visualizations: options.IncludeVisualizations
                );

                var jsonResult = result.to_json();
                return JsonSerializer.Deserialize<StatisticalAnalysisResult>(jsonResult.ToString());
            }
        }
        catch (PythonException ex)
        {
            _logger.LogError(ex, "Python execution failed during statistical analysis");
            throw new StatisticalAnalysisException($"Statistical analysis failed: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _pythonIntegration?.Dispose();
            _disposed = true;
        }
    }
}
```

### 2. Python Integration Utility

```csharp
// Framework/Utilities/PythonIntegration.cs
using Python.Runtime;

public class PythonIntegration : IDisposable
{
    private readonly ILogger<PythonIntegration> _logger;
    private readonly PythonConfig _config;
    private bool _pythonInitialized = false;
    private bool _disposed = false;

    public PythonIntegration(ILogger<PythonIntegration> logger, IConfiguration configuration)
    {
        _logger = logger;
        _config = configuration.GetSection("Python").Get<PythonConfig>();
        InitializePython();
    }

    private void InitializePython()
    {
        try
        {
            if (!PythonEngine.IsInitialized)
            {
                // Set Python path if virtual environment is specified
                if (!string.IsNullOrEmpty(_config.VirtualEnvironmentPath))
                {
                    var pythonPath = Path.Combine(_config.VirtualEnvironmentPath, 
                        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin", 
                        "python");
                    
                    if (File.Exists(pythonPath))
                    {
                        Runtime.PythonDLL = pythonPath;
                    }
                }

                PythonEngine.Initialize();
                
                // Add scripts path to Python sys.path
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(_config.ScriptsPath);
                }

                _pythonInitialized = true;
                _logger.LogInformation("Python.NET initialized successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Python.NET");
            throw new PythonInitializationException("Python.NET initialization failed", ex);
        }
    }

    public T ExecutePythonScript<T>(string scriptPath, Dictionary<string, object> parameters = null)
    {
        if (!_pythonInitialized)
        {
            throw new InvalidOperationException("Python.NET is not initialized");
        }

        try
        {
            using (Py.GIL())
            {
                var scope = Py.CreateScope();
                
                // Set parameters
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        scope.Set(param.Key, param.Value.ToPython());
                    }
                }

                // Execute script
                var scriptContent = File.ReadAllText(scriptPath);
                scope.Exec(scriptContent);

                // Get result
                var result = scope.Get("result");
                return result.As<T>();
            }
        }
        catch (PythonException ex)
        {
            _logger.LogError(ex, "Python script execution failed: {ScriptPath}", scriptPath);
            throw new PythonExecutionException($"Script execution failed: {ex.Message}", ex);
        }
    }

    public async Task<T> ExecutePythonScriptAsync<T>(string scriptPath, Dictionary<string, object> parameters = null)
    {
        return await Task.Run(() => ExecutePythonScript<T>(scriptPath, parameters));
    }

    public void Dispose()
    {
        if (!_disposed && _pythonInitialized)
        {
            try
            {
                PythonEngine.Shutdown();
                _logger.LogInformation("Python.NET shutdown completed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during Python.NET shutdown");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
```

## ML Model Validation

### 1. Model Validation Task

```csharp
// Framework/Tasks/ValidateModel.cs
public class ValidateModel : ITask
{
    private readonly string _modelPath;
    private readonly string _testDataPath;
    private readonly ModelValidationOptions _options;

    public ValidateModel(string modelPath, string testDataPath, ModelValidationOptions options = null)
    {
        _modelPath = modelPath ?? throw new ArgumentNullException(nameof(modelPath));
        _testDataPath = testDataPath ?? throw new ArgumentNullException(nameof(testDataPath));
        _options = options ?? new ModelValidationOptions();
    }

    public string Name => $"Validate ML model at '{_modelPath}' with test data '{_testDataPath}'";

    public async Task PerformAs(IActor actor)
    {
        var mlValidation = actor.Using<UseMLValidation>();
        
        var result = await mlValidation.ValidateModelPerformance(_modelPath, _testDataPath, _options);
        
        actor.Remember("ModelValidationResult", result);
        
        if (!result.IsValid)
        {
            throw new ModelValidationException(
                $"Model validation failed. Accuracy: {result.Accuracy:P2}, " +
                $"Expected: {_options.AccuracyThreshold:P2}");
        }
    }

    public static ValidateModel At(string modelPath) => new ValidateModelBuilder(modelPath);
}

public class ValidateModelBuilder
{
    private readonly string _modelPath;
    private string _testDataPath;
    private ModelValidationOptions _options = new();

    internal ValidateModelBuilder(string modelPath)
    {
        _modelPath = modelPath;
    }

    public ValidateModelBuilder WithTestData(string testDataPath)
    {
        _testDataPath = testDataPath;
        return this;
    }

    public ValidateModelBuilder WithAccuracyThreshold(double threshold)
    {
        _options.AccuracyThreshold = threshold;
        return this;
    }

    public ValidateModelBuilder WithPrecisionThreshold(double threshold)
    {
        _options.PrecisionThreshold = threshold;
        return this;
    }

    public ValidateModelBuilder WithRecallThreshold(double threshold)
    {
        _options.RecallThreshold = threshold;
        return this;
    }

    public ValidateModel Build() => new(_modelPath, _testDataPath, _options);

    public static implicit operator ValidateModel(ValidateModelBuilder builder) => builder.Build();
}
```

### 2. Python Model Validation Script

```python
# python-scripts/ml_validation/model_validator.py
import json
import numpy as np
import pandas as pd
from sklearn.metrics import accuracy_score, precision_score, recall_score, f1_score
from sklearn.metrics import confusion_matrix, classification_report
import joblib
import logging

class ModelValidator:
    def __init__(self, logger=None):
        self.logger = logger or logging.getLogger(__name__)
    
    def validate_model_performance(self, model_path, test_data_path, 
                                 accuracy_threshold=0.85, precision_threshold=0.80,
                                 recall_threshold=0.80, f1_threshold=0.82):
        """
        Validate model performance against test data and thresholds.
        
        Args:
            model_path (str): Path to the trained model file
            test_data_path (str): Path to the test dataset
            accuracy_threshold (float): Minimum required accuracy
            precision_threshold (float): Minimum required precision
            recall_threshold (float): Minimum required recall
            f1_threshold (float): Minimum required F1 score
            
        Returns:
            dict: Validation results with metrics and pass/fail status
        """
        try:
            self.logger.info(f"Loading model from {model_path}")
            model = joblib.load(model_path)
            
            self.logger.info(f"Loading test data from {test_data_path}")
            test_data = pd.read_csv(test_data_path)
            
            # Assume last column is target, rest are features
            X_test = test_data.iloc[:, :-1]
            y_test = test_data.iloc[:, -1]
            
            # Make predictions
            self.logger.info("Making predictions on test data")
            y_pred = model.predict(X_test)
            
            # Calculate metrics
            accuracy = accuracy_score(y_test, y_pred)
            precision = precision_score(y_test, y_pred, average='weighted')
            recall = recall_score(y_test, y_pred, average='weighted')
            f1 = f1_score(y_test, y_pred, average='weighted')
            
            # Generate confusion matrix
            cm = confusion_matrix(y_test, y_pred)
            
            # Generate classification report
            class_report = classification_report(y_test, y_pred, output_dict=True)
            
            # Determine if validation passes
            passes_accuracy = accuracy >= accuracy_threshold
            passes_precision = precision >= precision_threshold
            passes_recall = recall >= recall_threshold
            passes_f1 = f1 >= f1_threshold
            
            is_valid = all([passes_accuracy, passes_precision, passes_recall, passes_f1])
            
            result = {
                'is_valid': is_valid,
                'accuracy': float(accuracy),
                'precision': float(precision),
                'recall': float(recall),
                'f1_score': float(f1),
                'thresholds': {
                    'accuracy': accuracy_threshold,
                    'precision': precision_threshold,
                    'recall': recall_threshold,
                    'f1_score': f1_threshold
                },
                'passes': {
                    'accuracy': passes_accuracy,
                    'precision': passes_precision,
                    'recall': passes_recall,
                    'f1_score': passes_f1
                },
                'confusion_matrix': cm.tolist(),
                'classification_report': class_report,
                'test_samples': len(y_test),
                'model_path': model_path,
                'test_data_path': test_data_path
            }
            
            self.logger.info(f"Model validation completed. Valid: {is_valid}")
            return result
            
        except Exception as e:
            self.logger.error(f"Model validation failed: {str(e)}")
            raise Exception(f"Model validation error: {str(e)}")
    
    def to_json(self, result):
        """Convert result to JSON string for C# consumption."""
        return json.dumps(result, indent=2)

# Global function for direct calling from C#
def validate_model_performance(model_path, test_data_path, **kwargs):
    validator = ModelValidator()
    return validator.validate_model_performance(model_path, test_data_path, **kwargs)
```

## Data Processing and Analysis

### 1. Data Quality Assessment

```python
# python-scripts/ml_validation/data_analyzer.py
import json
import pandas as pd
import numpy as np
from scipy import stats
import logging

class DataAnalyzer:
    def __init__(self, logger=None):
        self.logger = logger or logging.getLogger(__name__)
    
    def assess_data_quality(self, data_path, missing_value_threshold=0.05,
                          outlier_method='IQR', quality_threshold=0.95):
        """
        Assess the quality of a dataset.
        
        Args:
            data_path (str): Path to the dataset
            missing_value_threshold (float): Maximum allowed missing value percentage
            outlier_method (str): Method for outlier detection ('IQR' or 'zscore')
            quality_threshold (float): Overall quality threshold
            
        Returns:
            dict: Data quality assessment results
        """
        try:
            self.logger.info(f"Loading data from {data_path}")
            df = pd.read_csv(data_path)
            
            # Basic statistics
            total_rows = len(df)
            total_columns = len(df.columns)
            
            # Missing values analysis
            missing_values = df.isnull().sum()
            missing_percentages = (missing_values / total_rows) * 100
            columns_with_missing = missing_values[missing_values > 0]
            
            # Data types analysis
            data_types = df.dtypes.value_counts().to_dict()
            
            # Outlier detection
            outliers_info = self._detect_outliers(df, method=outlier_method)
            
            # Duplicate rows
            duplicate_rows = df.duplicated().sum()
            duplicate_percentage = (duplicate_rows / total_rows) * 100
            
            # Data quality score calculation
            missing_score = 1 - (missing_percentages.max() / 100) if len(missing_percentages) > 0 else 1
            outlier_score = 1 - (outliers_info['total_outliers'] / total_rows) if total_rows > 0 else 1
            duplicate_score = 1 - (duplicate_percentage / 100)
            
            overall_quality_score = (missing_score + outlier_score + duplicate_score) / 3
            
            # Determine if quality passes threshold
            quality_passes = overall_quality_score >= quality_threshold
            missing_values_pass = missing_percentages.max() <= (missing_value_threshold * 100)
            
            result = {
                'quality_passes': quality_passes and missing_values_pass,
                'overall_quality_score': float(overall_quality_score),
                'quality_threshold': quality_threshold,
                'dataset_info': {
                    'total_rows': total_rows,
                    'total_columns': total_columns,
                    'data_types': {str(k): int(v) for k, v in data_types.items()}
                },
                'missing_values': {
                    'columns_with_missing': {col: int(count) for col, count in columns_with_missing.items()},
                    'missing_percentages': {col: float(pct) for col, pct in missing_percentages.items() if pct > 0},
                    'max_missing_percentage': float(missing_percentages.max()) if len(missing_percentages) > 0 else 0,
                    'passes_threshold': missing_values_pass
                },
                'outliers': outliers_info,
                'duplicates': {
                    'duplicate_rows': int(duplicate_rows),
                    'duplicate_percentage': float(duplicate_percentage)
                },
                'scores': {
                    'missing_score': float(missing_score),
                    'outlier_score': float(outlier_score),
                    'duplicate_score': float(duplicate_score)
                },
                'data_path': data_path
            }
            
            self.logger.info(f"Data quality assessment completed. Quality passes: {quality_passes}")
            return result
            
        except Exception as e:
            self.logger.error(f"Data quality assessment failed: {str(e)}")
            raise Exception(f"Data quality assessment error: {str(e)}")
    
    def _detect_outliers(self, df, method='IQR'):
        """Detect outliers in numerical columns."""
        numeric_columns = df.select_dtypes(include=[np.number]).columns
        outliers_info = {
            'method': method,
            'columns_analyzed': list(numeric_columns),
            'outliers_by_column': {},
            'total_outliers': 0
        }
        
        for col in numeric_columns:
            if method.upper() == 'IQR':
                Q1 = df[col].quantile(0.25)
                Q3 = df[col].quantile(0.75)
                IQR = Q3 - Q1
                lower_bound = Q1 - 1.5 * IQR
                upper_bound = Q3 + 1.5 * IQR
                outliers = df[(df[col] < lower_bound) | (df[col] > upper_bound)]
            elif method.upper() == 'ZSCORE':
                z_scores = np.abs(stats.zscore(df[col].dropna()))
                outliers = df[z_scores > 3]
            else:
                continue
            
            outlier_count = len(outliers)
            outliers_info['outliers_by_column'][col] = {
                'count': outlier_count,
                'percentage': float((outlier_count / len(df)) * 100)
            }
            outliers_info['total_outliers'] += outlier_count
        
        return outliers_info
    
    def to_json(self, result):
        """Convert result to JSON string for C# consumption."""
        return json.dumps(result, indent=2)

# Global function for direct calling from C#
def assess_data_quality(data_path, **kwargs):
    analyzer = DataAnalyzer()
    return analyzer.assess_data_quality(data_path, **kwargs)
```

### 2. Statistical Analysis

```python
# python-scripts/ml_validation/statistical_analyzer.py
import json
import pandas as pd
import numpy as np
from scipy import stats
import matplotlib.pyplot as plt
import seaborn as sns
import logging
import os

class StatisticalAnalyzer:
    def __init__(self, logger=None):
        self.logger = logger or logging.getLogger(__name__)
    
    def perform_analysis(self, data_path, analysis_type='descriptive',
                        confidence_level=0.95, include_visualizations=False):
        """
        Perform statistical analysis on dataset.
        
        Args:
            data_path (str): Path to the dataset
            analysis_type (str): Type of analysis ('descriptive', 'correlation', 'hypothesis')
            confidence_level (float): Confidence level for statistical tests
            include_visualizations (bool): Whether to generate visualization files
            
        Returns:
            dict: Statistical analysis results
        """
        try:
            self.logger.info(f"Loading data for statistical analysis from {data_path}")
            df = pd.read_csv(data_path)
            
            result = {
                'analysis_type': analysis_type,
                'confidence_level': confidence_level,
                'data_path': data_path,
                'dataset_shape': df.shape
            }
            
            if analysis_type == 'descriptive':
                result.update(self._descriptive_analysis(df))
            elif analysis_type == 'correlation':
                result.update(self._correlation_analysis(df))
            elif analysis_type == 'hypothesis':
                result.update(self._hypothesis_testing(df, confidence_level))
            else:
                result.update(self._comprehensive_analysis(df, confidence_level))
            
            if include_visualizations:
                result['visualizations'] = self._generate_visualizations(df, data_path)
            
            self.logger.info("Statistical analysis completed successfully")
            return result
            
        except Exception as e:
            self.logger.error(f"Statistical analysis failed: {str(e)}")
            raise Exception(f"Statistical analysis error: {str(e)}")
    
    def _descriptive_analysis(self, df):
        """Perform descriptive statistical analysis."""
        numeric_df = df.select_dtypes(include=[np.number])
        
        return {
            'descriptive_statistics': {
                col: {
                    'mean': float(numeric_df[col].mean()),
                    'median': float(numeric_df[col].median()),
                    'std': float(numeric_df[col].std()),
                    'min': float(numeric_df[col].min()),
                    'max': float(numeric_df[col].max()),
                    'q25': float(numeric_df[col].quantile(0.25)),
                    'q75': float(numeric_df[col].quantile(0.75)),
                    'skewness': float(numeric_df[col].skew()),
                    'kurtosis': float(numeric_df[col].kurtosis())
                } for col in numeric_df.columns
            },
            'categorical_summary': {
                col: {
                    'unique_values': int(df[col].nunique()),
                    'most_frequent': str(df[col].mode().iloc[0]) if not df[col].mode().empty else None,
                    'frequency_distribution': df[col].value_counts().head(10).to_dict()
                } for col in df.select_dtypes(include=['object']).columns
            }
        }
    
    def _correlation_analysis(self, df):
        """Perform correlation analysis."""
        numeric_df = df.select_dtypes(include=[np.number])
        correlation_matrix = numeric_df.corr()
        
        # Find strong correlations (> 0.7 or < -0.7)
        strong_correlations = []
        for i in range(len(correlation_matrix.columns)):
            for j in range(i+1, len(correlation_matrix.columns)):
                corr_value = correlation_matrix.iloc[i, j]
                if abs(corr_value) > 0.7:
                    strong_correlations.append({
                        'variable1': correlation_matrix.columns[i],
                        'variable2': correlation_matrix.columns[j],
                        'correlation': float(corr_value)
                    })
        
        return {
            'correlation_matrix': correlation_matrix.to_dict(),
            'strong_correlations': strong_correlations,
            'correlation_summary': {
                'max_correlation': float(correlation_matrix.max().max()),
                'min_correlation': float(correlation_matrix.min().min()),
                'mean_correlation': float(correlation_matrix.mean().mean())
            }
        }
    
    def _hypothesis_testing(self, df, confidence_level):
        """Perform hypothesis testing."""
        numeric_df = df.select_dtypes(include=[np.number])
        alpha = 1 - confidence_level
        
        normality_tests = {}
        for col in numeric_df.columns:
            # Shapiro-Wilk test for normality
            stat, p_value = stats.shapiro(numeric_df[col].dropna().sample(min(5000, len(numeric_df[col].dropna()))))
            normality_tests[col] = {
                'statistic': float(stat),
                'p_value': float(p_value),
                'is_normal': p_value > alpha
            }
        
        return {
            'normality_tests': normality_tests,
            'alpha_level': alpha,
            'confidence_level': confidence_level
        }
    
    def _comprehensive_analysis(self, df, confidence_level):
        """Perform comprehensive analysis combining all methods."""
        result = {}
        result.update(self._descriptive_analysis(df))
        result.update(self._correlation_analysis(df))
        result.update(self._hypothesis_testing(df, confidence_level))
        return result
    
    def _generate_visualizations(self, df, data_path):
        """Generate visualization files."""
        viz_dir = os.path.join(os.path.dirname(data_path), 'visualizations')
        os.makedirs(viz_dir, exist_ok=True)
        
        visualizations = []
        
        # Correlation heatmap
        numeric_df = df.select_dtypes(include=[np.number])
        if len(numeric_df.columns) > 1:
            plt.figure(figsize=(10, 8))
            sns.heatmap(numeric_df.corr(), annot=True, cmap='coolwarm', center=0)
            plt.title('Correlation Matrix')
            heatmap_path = os.path.join(viz_dir, 'correlation_heatmap.png')
            plt.savefig(heatmap_path)
            plt.close()
            visualizations.append(heatmap_path)
        
        # Distribution plots
        for col in numeric_df.columns[:5]:  # Limit to first 5 columns
            plt.figure(figsize=(8, 6))
            sns.histplot(numeric_df[col], kde=True)
            plt.title(f'Distribution of {col}')
            dist_path = os.path.join(viz_dir, f'distribution_{col}.png')
            plt.savefig(dist_path)
            plt.close()
            visualizations.append(dist_path)
        
        return visualizations
    
    def to_json(self, result):
        """Convert result to JSON string for C# consumption."""
        return json.dumps(result, indent=2, default=str)

# Global function for direct calling from C#
def perform_analysis(data_path, **kwargs):
    analyzer = StatisticalAnalyzer()
    return analyzer.perform_analysis(data_path, **kwargs)
```

## Azure ML Python SDK Integration

### 1. Azure ML Workspace Client

```python
# python-scripts/azure_ml/workspace_client.py
import json
from azure.ai.ml import MLClient
from azure.identity import DefaultAzureCredential
import logging

class AzureMLWorkspaceClient:
    def __init__(self, subscription_id, resource_group, workspace_name, logger=None):
        self.logger = logger or logging.getLogger(__name__)
        self.subscription_id = subscription_id
        self.resource_group = resource_group
        self.workspace_name = workspace_name
        
        try:
            credential = DefaultAzureCredential()
            self.ml_client = MLClient(
                credential=credential,
                subscription_id=subscription_id,
                resource_group_name=resource_group,
                workspace_name=workspace_name
            )
            self.logger.info("Azure ML client initialized successfully")
        except Exception as e:
            self.logger.error(f"Failed to initialize Azure ML client: {str(e)}")
            raise
    
    def get_workspace_info(self):
        """Get workspace information."""
        try:
            workspace = self.ml_client.workspaces.get(self.workspace_name)
            return {
                'name': workspace.name,
                'resource_group': workspace.resource_group,
                'location': workspace.location,
                'description': workspace.description,
                'tags': workspace.tags or {}
            }
        except Exception as e:
            self.logger.error(f"Failed to get workspace info: {str(e)}")
            raise
    
    def list_compute_instances(self):
        """List all compute instances in the workspace."""
        try:
            compute_instances = []
            for compute in self.ml_client.compute.list():
                if compute.type == 'ComputeInstance':
                    compute_instances.append({
                        'name': compute.name,
                        'state': compute.state,
                        'vm_size': compute.size,
                        'created_time': str(compute.created_on) if compute.created_on else None,
                        'last_modified': str(compute.last_modified_on) if compute.last_modified_on else None
                    })
            return compute_instances
        except Exception as e:
            self.logger.error(f"Failed to list compute instances: {str(e)}")
            raise
    
    def get_compute_status(self, compute_name):
        """Get status of a specific compute instance."""
        try:
            compute = self.ml_client.compute.get(compute_name)
            return {
                'name': compute.name,
                'state': compute.state,
                'vm_size': compute.size,
                'type': compute.type,
                'created_time': str(compute.created_on) if compute.created_on else None,
                'last_modified': str(compute.last_modified_on) if compute.last_modified_on else None
            }
        except Exception as e:
            self.logger.error(f"Failed to get compute status for {compute_name}: {str(e)}")
            raise
    
    def list_models(self, limit=10):
        """List registered models in the workspace."""
        try:
            models = []
            for model in self.ml_client.models.list():
                models.append({
                    'name': model.name,
                    'version': model.version,
                    'description': model.description,
                    'tags': model.tags or {},
                    'created_time': str(model.creation_context.created_at) if model.creation_context else None
                })
                if len(models) >= limit:
                    break
            return models
        except Exception as e:
            self.logger.error(f"Failed to list models: {str(e)}")
            raise
    
    def to_json(self, result):
        """Convert result to JSON string for C# consumption."""
        return json.dumps(result, indent=2, default=str)

# Global functions for direct calling from C#
def get_workspace_info(subscription_id, resource_group, workspace_name):
    client = AzureMLWorkspaceClient(subscription_id, resource_group, workspace_name)
    return client.get_workspace_info()

def list_compute_instances(subscription_id, resource_group, workspace_name):
    client = AzureMLWorkspaceClient(subscription_id, resource_group, workspace_name)
    return client.list_compute_instances()

def get_compute_status(subscription_id, resource_group, workspace_name, compute_name):
    client = AzureMLWorkspaceClient(subscription_id, resource_group, workspace_name)
    return client.get_compute_status(compute_name)
```

## Performance Considerations

### 1. Memory Management

```csharp
// Framework/Utilities/PythonMemoryManager.cs
public class PythonMemoryManager : IDisposable
{
    private readonly ILogger<PythonMemoryManager> _logger;
    private readonly Timer _memoryCleanupTimer;
    private bool _disposed = false;

    public PythonMemoryManager(ILogger<PythonMemoryManager> logger)
    {
        _logger = logger;
        _memoryCleanupTimer = new Timer(CleanupMemory, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    private void CleanupMemory(object state)
    {
        try
        {
            using (Py.GIL())
            {
                dynamic gc = Py.Import("gc");
                var collected = gc.collect();
                _logger.LogDebug("Python garbage collection completed. Objects collected: {CollectedObjects}", collected);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to perform Python garbage collection");
        }
    }

    public void ForceCleanup()
    {
        CleanupMemory(null);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _memoryCleanupTimer?.Dispose();
            ForceCleanup();
            _disposed = true;
        }
    }
}
```

### 2. Connection Pooling

```csharp
// Framework/Utilities/PythonConnectionPool.cs
public class PythonConnectionPool : IDisposable
{
    private readonly ConcurrentQueue<PythonScope> _availableScopes = new();
    private readonly ConcurrentDictionary<int, PythonScope> _activeScopes = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxConnections;
    private bool _disposed = false;

    public PythonConnectionPool(int maxConnections = 10)
    {
        _maxConnections = maxConnections;
        _semaphore = new SemaphoreSlim(maxConnections, maxConnections);
    }

    public async Task<PythonScope> AcquireScopeAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        if (_availableScopes.TryDequeue(out var scope))
        {
            _activeScopes.TryAdd(scope.GetHashCode(), scope);
            return scope;
        }

        using (Py.GIL())
        {
            scope = Py.CreateScope();
            _activeScopes.TryAdd(scope.GetHashCode(), scope);
            return scope;
        }
    }

    public void ReleaseScope(PythonScope scope)
    {
        if (scope != null && _activeScopes.TryRemove(scope.GetHashCode(), out _))
        {
            _availableScopes.Enqueue(scope);
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            while (_availableScopes.TryDequeue(out var scope))
            {
                scope?.Dispose();
            }

            foreach (var scope in _activeScopes.Values)
            {
                scope?.Dispose();
            }

            _semaphore?.Dispose();
            _disposed = true;
        }
    }
}
```

## Best Practices

### 1. Error Handling

- Always wrap Python.NET calls in try-catch blocks
- Use specific exception types for different failure scenarios
- Log Python exceptions with full stack traces
- Implement retry logic for transient failures

### 2. Resource Management

- Always use `using (Py.GIL())` for Python operations
- Dispose of Python objects properly
- Implement connection pooling for high-throughput scenarios
- Monitor memory usage and implement cleanup routines

### 3. Performance Optimization

- Cache frequently used Python modules
- Use asynchronous operations where possible
- Implement lazy loading for expensive Python libraries
- Consider using Python multiprocessing for CPU-intensive tasks

### 4. Security Considerations

- Validate all input parameters before passing to Python
- Use secure credential management for Azure authentication
- Implement proper access controls for Python script execution
- Sanitize file paths and prevent directory traversal attacks

## Troubleshooting

### Common Issues

1. **Python.NET Initialization Failures**
   - Verify Python installation and PATH configuration
   - Check virtual environment activation
   - Ensure Python.Runtime package compatibility

2. **GIL (Global Interpreter Lock) Issues**
   - Always acquire GIL before Python operations
   - Don't hold GIL longer than necessary
   - Use proper scope management

3. **Memory Leaks**
   - Implement proper disposal patterns
   - Use memory monitoring tools
   - Regular garbage collection

4. **Performance Issues**
   - Profile Python code execution
   - Optimize data serialization/deserialization
   - Consider caching strategies

### Debugging Tips

- Enable detailed logging for Python.NET operations
- Use Python's logging module for script debugging
- Monitor system resources during test execution
- Use profiling tools to identify bottlenecks

This comprehensive Python.NET integration guide provides everything needed to leverage Python's ML capabilities within your C# Azure ML Workspace Testing Framework while maintaining the Screenplay pattern architecture.