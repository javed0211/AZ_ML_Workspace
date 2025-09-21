# Integration Quick Reference Guide

## Overview

This guide provides quick reference information for TypeScript and Python.NET integrations in the Azure ML Workspace Testing Framework.

## TypeScript Integration

### Quick Setup
```bash
cd AzureMLWorkspace.Tests/typescript-utils
npm init -y
npm install --save-dev typescript @playwright/test ts-node
```

### Basic Usage Example
```csharp
// C# - Using TypeScript utilities
public class AdvancedBrowserTask : ITask
{
    public async Task PerformAs(IActor actor)
    {
        var tsIntegration = actor.Using<TypeScriptIntegration>();
        var result = await tsIntegration.ExecuteTypeScriptFunction<NetworkLog[]>(
            "captureNetworkTraffic", 
            new { timeout = 30000 }
        );
        actor.Remember("NetworkLogs", result);
    }
}
```

```typescript
// TypeScript - Advanced browser utilities
export async function captureNetworkTraffic(options: { timeout: number }): Promise<NetworkLog[]> {
    const logs: NetworkLog[] = [];
    // Implementation here
    return logs;
}
```

### Key Features
- ✅ Advanced Playwright features
- ✅ Type-safe browser automation
- ✅ Performance monitoring
- ✅ Network traffic analysis
- ✅ Custom element interactions

### Documentation
📖 [Complete TypeScript Integration Guide](TYPESCRIPT_INTEGRATION.md)

---

## Python.NET Integration

### Quick Setup
```bash
# Create Python environment
python -m venv venv-azureml-tests
source venv-azureml-tests/bin/activate  # Linux/macOS
# venv-azureml-tests\Scripts\activate   # Windows

# Install packages
pip install azure-ai-ml scikit-learn pandas numpy
```

### Basic Usage Example
```csharp
// C# - Using Python.NET for ML validation
public class ValidateMLModel : ITask
{
    public async Task PerformAs(IActor actor)
    {
        var mlValidation = actor.Using<UseMLValidation>();
        var result = await mlValidation.ValidateModelPerformance(
            "models/trained_model.pkl",
            "data/test_data.csv"
        );
        
        if (!result.IsValid)
        {
            throw new ModelValidationException($"Model accuracy {result.Accuracy:P2} below threshold");
        }
    }
}
```

```python
# Python - ML validation script
def validate_model_performance(model_path, test_data_path, **kwargs):
    model = joblib.load(model_path)
    test_data = pd.read_csv(test_data_path)
    
    X_test = test_data.iloc[:, :-1]
    y_test = test_data.iloc[:, -1]
    y_pred = model.predict(X_test)
    
    accuracy = accuracy_score(y_test, y_pred)
    return {
        'is_valid': accuracy >= kwargs.get('accuracy_threshold', 0.85),
        'accuracy': accuracy,
        'precision': precision_score(y_test, y_pred, average='weighted'),
        'recall': recall_score(y_test, y_pred, average='weighted')
    }
```

### Key Features
- ✅ ML model validation
- ✅ Data quality assessment
- ✅ Statistical analysis
- ✅ Azure ML Python SDK integration
- ✅ Custom analytics

### Documentation
📖 [Complete Python.NET Integration Guide](PYTHON_NET_INTEGRATION.md)

---

## Integration Comparison

| Feature | TypeScript | Python.NET |
|---------|------------|------------|
| **Primary Use Case** | Browser automation & UI testing | ML validation & data analysis |
| **Language** | TypeScript/JavaScript | Python |
| **Execution** | Node.js process | In-process via Python.NET |
| **Performance** | Fast for UI operations | Optimized for ML computations |
| **Dependencies** | npm packages | pip packages |
| **Type Safety** | Full TypeScript support | C# interfaces with Python dynamic |
| **Debugging** | Browser dev tools, VS Code | Python debugger, logging |

## Configuration Examples

### TypeScript Configuration
```json
// tsconfig.json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "commonjs",
    "strict": true,
    "esModuleInterop": true
  }
}
```

### Python.NET Configuration
```json
// appsettings.json
{
  "Python": {
    "ExecutablePath": "python",
    "VirtualEnvironmentPath": "./venv-azureml-tests",
    "ScriptsPath": "./python-scripts",
    "TimeoutSeconds": 300
  }
}
```

## Common Patterns

### Screenplay Pattern Integration

Both integrations follow the Screenplay pattern:

```csharp
// Actor uses abilities
var actor = Actor.Named("DataScientist")
    .Can(UseMLValidation.WithConfiguration(config))
    .Can(UseAdvancedBrowser.WithTypeScriptUtilities());

// Actor performs tasks
await actor.AttemptsTo(
    ValidateModel.At("model.pkl").WithTestData("test.csv"),
    CapturePerformanceMetrics.ForPage("/workspace")
);

// Actor asks questions
var isModelValid = await actor.AsksFor(ModelAccuracy.IsAboveThreshold(0.85));
var loadTime = await actor.AsksFor(PageLoadTime.InMilliseconds());
```

### Error Handling

```csharp
// TypeScript integration error handling
try
{
    var result = await tsIntegration.ExecuteTypeScriptFunction<T>("functionName", parameters);
}
catch (TypeScriptExecutionException ex)
{
    _logger.LogError(ex, "TypeScript execution failed");
    throw;
}

// Python.NET integration error handling
try
{
    using (Py.GIL())
    {
        var result = pythonModule.function_name(parameters);
    }
}
catch (PythonException ex)
{
    _logger.LogError(ex, "Python execution failed");
    throw new MLValidationException("Python operation failed", ex);
}
```

## Best Practices Summary

### TypeScript
- Use TypeScript interfaces for type safety
- Implement proper error handling
- Optimize for browser performance
- Cache expensive operations
- Use async/await consistently

### Python.NET
- Always acquire GIL before Python operations
- Implement proper resource disposal
- Use connection pooling for high throughput
- Monitor memory usage
- Validate inputs before Python calls

## Troubleshooting Quick Fixes

### TypeScript Issues
```bash
# Clear node modules and reinstall
rm -rf node_modules package-lock.json
npm install

# Check TypeScript compilation
npx tsc --noEmit

# Verify Playwright installation
npx playwright install
```

### Python.NET Issues
```bash
# Verify Python installation
python --version
which python

# Check package installation
pip list | grep azure-ai-ml

# Test Python.NET initialization
python -c "import sys; print(sys.path)"
```

## Getting Started Checklist

### TypeScript Integration
- [ ] Node.js 18+ installed
- [ ] TypeScript project initialized
- [ ] Playwright dependencies installed
- [ ] C# integration class implemented
- [ ] Build process configured

### Python.NET Integration
- [ ] Python 3.8+ installed
- [ ] Virtual environment created
- [ ] Required packages installed
- [ ] Python.Runtime package referenced
- [ ] GIL management implemented

## Support and Resources

- 📚 [TypeScript Integration Guide](TYPESCRIPT_INTEGRATION.md) - Complete setup and usage
- 📚 [Python.NET Integration Guide](PYTHON_NET_INTEGRATION.md) - ML validation and analytics
- 📚 [Technical Documentation](TECHNICAL_DOCUMENTATION.md) - Framework architecture
- 🔧 [Project README](../AzureMLWorkspace.Tests/README.md) - Getting started guide

---

*This quick reference guide provides essential information for both integrations. For detailed implementation examples and advanced features, refer to the complete integration guides.*