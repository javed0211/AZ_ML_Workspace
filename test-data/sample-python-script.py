#!/usr/bin/env python3
"""
Azure ML Sample Python Script
This script demonstrates basic Azure ML operations and can be executed on remote compute.
"""

import os
import sys
import json
import time
import logging
from datetime import datetime
from pathlib import Path

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

def check_environment():
    """Check the current environment and Azure ML setup."""
    logger.info("=== Environment Check ===")
    
    # Python environment
    logger.info(f"Python version: {sys.version}")
    logger.info(f"Python executable: {sys.executable}")
    logger.info(f"Current working directory: {os.getcwd()}")
    
    # System information
    logger.info(f"Platform: {sys.platform}")
    logger.info(f"Environment variables count: {len(os.environ)}")
    
    # Check for Azure ML related environment variables
    azure_vars = [var for var in os.environ.keys() if 'AZURE' in var.upper()]
    if azure_vars:
        logger.info(f"Azure environment variables found: {len(azure_vars)}")
        for var in sorted(azure_vars):
            # Don't log sensitive values
            if 'SECRET' in var.upper() or 'KEY' in var.upper():
                logger.info(f"  {var}: [HIDDEN]")
            else:
                logger.info(f"  {var}: {os.environ[var]}")
    else:
        logger.info("No Azure environment variables found")

def test_imports():
    """Test importing common libraries used in Azure ML."""
    logger.info("=== Testing Library Imports ===")
    
    # Standard libraries
    try:
        import numpy as np
        import pandas as pd
        logger.info("✓ NumPy and Pandas imported successfully")
    except ImportError as e:
        logger.warning(f"✗ Failed to import NumPy/Pandas: {e}")
    
    # Machine learning libraries
    try:
        import sklearn
        logger.info(f"✓ Scikit-learn {sklearn.__version__} imported successfully")
    except ImportError as e:
        logger.warning(f"✗ Failed to import Scikit-learn: {e}")
    
    # Azure ML SDK
    try:
        import azureml.core
        logger.info(f"✓ Azure ML SDK {azureml.core.VERSION} imported successfully")
    except ImportError as e:
        logger.warning(f"✗ Failed to import Azure ML SDK: {e}")
    
    # Visualization libraries
    try:
        import matplotlib.pyplot as plt
        import seaborn as sns
        logger.info("✓ Matplotlib and Seaborn imported successfully")
    except ImportError as e:
        logger.warning(f"✗ Failed to import visualization libraries: {e}")

def create_sample_data():
    """Create sample data for testing."""
    logger.info("=== Creating Sample Data ===")
    
    try:
        import numpy as np
        import pandas as pd
        
        # Set random seed for reproducibility
        np.random.seed(42)
        
        # Generate sample dataset
        n_samples = 1000
        data = {
            'feature_1': np.random.normal(0, 1, n_samples),
            'feature_2': np.random.normal(5, 2, n_samples),
            'feature_3': np.random.exponential(1, n_samples),
            'feature_4': np.random.uniform(-1, 1, n_samples),
            'category': np.random.choice(['A', 'B', 'C'], n_samples),
            'target': np.random.choice([0, 1], n_samples, p=[0.7, 0.3])
        }
        
        df = pd.DataFrame(data)
        
        # Save to CSV
        output_file = Path('sample_data.csv')
        df.to_csv(output_file, index=False)
        
        logger.info(f"✓ Sample data created with shape: {df.shape}")
        logger.info(f"✓ Data saved to: {output_file.absolute()}")
        
        # Display basic statistics
        logger.info("Data summary:")
        logger.info(f"  Numerical features: {df.select_dtypes(include=[np.number]).columns.tolist()}")
        logger.info(f"  Categorical features: {df.select_dtypes(include=['object']).columns.tolist()}")
        logger.info(f"  Target distribution: {df['target'].value_counts().to_dict()}")
        
        return df
        
    except Exception as e:
        logger.error(f"✗ Failed to create sample data: {e}")
        return None

def run_simple_analysis(df):
    """Run simple data analysis."""
    logger.info("=== Running Simple Analysis ===")
    
    try:
        import numpy as np
        import pandas as pd
        
        if df is None:
            logger.error("No data provided for analysis")
            return
        
        # Basic statistics
        logger.info("Basic statistics:")
        stats = df.describe()
        logger.info(f"  Mean values: {stats.loc['mean'].to_dict()}")
        logger.info(f"  Standard deviations: {stats.loc['std'].to_dict()}")
        
        # Correlation analysis
        numeric_cols = df.select_dtypes(include=[np.number]).columns
        if len(numeric_cols) > 1:
            corr_matrix = df[numeric_cols].corr()
            logger.info("Correlation matrix (top correlations):")
            
            # Find highest correlations (excluding diagonal)
            corr_pairs = []
            for i in range(len(corr_matrix.columns)):
                for j in range(i+1, len(corr_matrix.columns)):
                    col1, col2 = corr_matrix.columns[i], corr_matrix.columns[j]
                    corr_value = corr_matrix.iloc[i, j]
                    corr_pairs.append((col1, col2, abs(corr_value), corr_value))
            
            # Sort by absolute correlation
            corr_pairs.sort(key=lambda x: x[2], reverse=True)
            
            for col1, col2, abs_corr, corr in corr_pairs[:3]:
                logger.info(f"  {col1} <-> {col2}: {corr:.3f}")
        
        # Missing values check
        missing_values = df.isnull().sum()
        if missing_values.sum() > 0:
            logger.warning(f"Missing values found: {missing_values[missing_values > 0].to_dict()}")
        else:
            logger.info("✓ No missing values found")
        
        logger.info("✓ Analysis completed successfully")
        
    except Exception as e:
        logger.error(f"✗ Analysis failed: {e}")

def test_machine_learning(df):
    """Test basic machine learning operations."""
    logger.info("=== Testing Machine Learning ===")
    
    try:
        from sklearn.model_selection import train_test_split
        from sklearn.ensemble import RandomForestClassifier
        from sklearn.metrics import accuracy_score, classification_report
        import numpy as np
        
        if df is None:
            logger.error("No data provided for ML testing")
            return
        
        # Prepare features and target
        feature_cols = [col for col in df.columns if col.startswith('feature_')]
        X = df[feature_cols]
        y = df['target']
        
        logger.info(f"Features: {feature_cols}")
        logger.info(f"Target: target (classes: {sorted(y.unique())})")
        
        # Split data
        X_train, X_test, y_train, y_test = train_test_split(
            X, y, test_size=0.2, random_state=42, stratify=y
        )
        
        logger.info(f"Training set size: {X_train.shape[0]}")
        logger.info(f"Test set size: {X_test.shape[0]}")
        
        # Train model
        model = RandomForestClassifier(n_estimators=50, random_state=42, n_jobs=-1)
        
        start_time = time.time()
        model.fit(X_train, y_train)
        training_time = time.time() - start_time
        
        logger.info(f"✓ Model trained in {training_time:.2f} seconds")
        
        # Make predictions
        y_pred = model.predict(X_test)
        accuracy = accuracy_score(y_test, y_pred)
        
        logger.info(f"✓ Model accuracy: {accuracy:.4f}")
        
        # Feature importance
        feature_importance = dict(zip(feature_cols, model.feature_importances_))
        sorted_features = sorted(feature_importance.items(), key=lambda x: x[1], reverse=True)
        
        logger.info("Feature importance:")
        for feature, importance in sorted_features:
            logger.info(f"  {feature}: {importance:.4f}")
        
        # Save model results
        results = {
            'timestamp': datetime.now().isoformat(),
            'accuracy': float(accuracy),
            'training_time': training_time,
            'feature_importance': feature_importance,
            'model_params': model.get_params()
        }
        
        with open('ml_results.json', 'w') as f:
            json.dump(results, f, indent=2)
        
        logger.info("✓ ML results saved to ml_results.json")
        
    except Exception as e:
        logger.error(f"✗ ML testing failed: {e}")

def test_azure_ml_connection():
    """Test Azure ML workspace connection."""
    logger.info("=== Testing Azure ML Connection ===")
    
    try:
        from azureml.core import Workspace
        
        # Try to connect to workspace
        try:
            ws = Workspace.from_config()
            logger.info(f"✓ Connected to workspace: {ws.name}")
            logger.info(f"  Resource group: {ws.resource_group}")
            logger.info(f"  Location: {ws.location}")
            logger.info(f"  Subscription ID: {ws.subscription_id}")
            
            # List compute targets
            compute_targets = ws.compute_targets
            if compute_targets:
                logger.info(f"  Compute targets: {list(compute_targets.keys())}")
            else:
                logger.info("  No compute targets found")
            
            # List datasets
            datasets = ws.datasets
            if datasets:
                logger.info(f"  Datasets: {list(datasets.keys())[:5]}...")  # Show first 5
            else:
                logger.info("  No datasets found")
                
        except Exception as e:
            logger.warning(f"Could not connect to workspace: {e}")
            logger.info("This is expected if running outside Azure ML environment")
            
    except ImportError:
        logger.warning("Azure ML SDK not available")

def create_test_outputs():
    """Create test output files."""
    logger.info("=== Creating Test Outputs ===")
    
    try:
        # Create outputs directory
        outputs_dir = Path('outputs')
        outputs_dir.mkdir(exist_ok=True)
        
        # Create a simple report
        report = {
            'test_run': {
                'timestamp': datetime.now().isoformat(),
                'script_name': __file__,
                'working_directory': str(Path.cwd()),
                'python_version': sys.version,
            },
            'tests_completed': [
                'environment_check',
                'library_imports',
                'data_creation',
                'data_analysis',
                'machine_learning',
                'azure_ml_connection'
            ],
            'status': 'completed'
        }
        
        report_file = outputs_dir / 'test_report.json'
        with open(report_file, 'w') as f:
            json.dump(report, f, indent=2)
        
        logger.info(f"✓ Test report saved to: {report_file}")
        
        # Create a simple text log
        log_file = outputs_dir / 'execution_log.txt'
        with open(log_file, 'w') as f:
            f.write(f"Script execution completed at: {datetime.now()}\n")
            f.write(f"Working directory: {Path.cwd()}\n")
            f.write(f"Python version: {sys.version}\n")
            f.write("All tests completed successfully!\n")
        
        logger.info(f"✓ Execution log saved to: {log_file}")
        
    except Exception as e:
        logger.error(f"✗ Failed to create test outputs: {e}")

def main():
    """Main execution function."""
    logger.info("🚀 Starting Azure ML Sample Script")
    logger.info(f"Script started at: {datetime.now()}")
    
    try:
        # Run all tests
        check_environment()
        test_imports()
        
        # Create and analyze data
        df = create_sample_data()
        if df is not None:
            run_simple_analysis(df)
            test_machine_learning(df)
        
        # Test Azure ML connection
        test_azure_ml_connection()
        
        # Create outputs
        create_test_outputs()
        
        logger.info("✅ All tests completed successfully!")
        
    except Exception as e:
        logger.error(f"❌ Script execution failed: {e}")
        sys.exit(1)
    
    finally:
        logger.info(f"Script finished at: {datetime.now()}")

if __name__ == "__main__":
    main()