# 📊 Beautiful Allure Reports Guide

This guide explains how to generate and view beautiful HTML test reports using Allure Framework.

## 🎯 What is Allure?

Allure Framework is a flexible, lightweight multi-language test report tool that shows a very concise representation of what has been tested in a neat web report form.

**Features:**
- 🎨 Beautiful, modern UI with dark/light themes
- 📊 Interactive charts and graphs
- 📸 Screenshot attachments on failures
- 🏷️ Test categorization and filtering
- 📈 Historical trends and statistics
- 🔍 Detailed test execution timeline
- 📝 Step-by-step test execution details

---

## 🚀 Quick Start

### 1. Install Allure Command Line Tool

#### **macOS (using Homebrew)**
```bash
brew install allure
```

#### **Windows (using Scoop)**
```powershell
scoop install allure
```

#### **Linux (manual installation)**
```bash
# Download and extract
wget https://github.com/allure-framework/allure2/releases/download/2.25.0/allure-2.25.0.tgz
tar -zxvf allure-2.25.0.tgz
sudo mv allure-2.25.0 /opt/allure

# Add to PATH
echo 'export PATH="/opt/allure/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

#### **Verify Installation**
```bash
allure --version
# Should output: 2.25.0 or similar
```

---

## 📋 Running Tests and Generating Reports

### **Step 1: Run Your Tests**

```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests

# Run all tests
dotnet test

# Run specific category
dotnet test --filter "Category=smoke"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

This will generate test results in the `allure-results` directory.

### **Step 2: Generate HTML Report**

```bash
# Generate and open report in browser
allure serve allure-results

# Or generate static HTML report
allure generate allure-results -o allure-report --clean

# Open the generated report
open allure-report/index.html  # macOS
start allure-report/index.html # Windows
xdg-open allure-report/index.html # Linux
```

---

## 🎨 Report Features

### **1. Overview Dashboard**
- Total tests executed
- Pass/Fail/Skip statistics
- Success rate percentage
- Test duration
- Flaky tests detection

### **2. Test Suites**
- Organized by features
- Hierarchical test structure
- Quick filtering and search

### **3. Graphs and Charts**
- Status breakdown (pie chart)
- Severity distribution
- Duration trends
- Test execution timeline

### **4. Test Details**
- Step-by-step execution
- Screenshots on failures
- Logs and attachments
- Stack traces for errors
- Execution time per step

### **5. Categories**
- Product defects
- Test defects
- Known issues
- Flaky tests

---

## 🏷️ Using Allure Attributes in Tests

You can enhance your test reports with Allure attributes:

### **Example: Adding Metadata to Tests**

```csharp
using Allure.Net.Commons;
using NUnit.Framework;

[TestFixture]
[AllureNUnit]
public class MyTests
{
    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("John Doe")]
    [AllureFeature("Azure AI Search")]
    [AllureStory("Search Index Creation")]
    [AllureTag("smoke", "api")]
    public void TestSearchIndexCreation()
    {
        // Add step
        AllureApi.Step("Create search index", () =>
        {
            // Your test code
        });

        // Add attachment
        AllureApi.AddAttachment("Request Body", "application/json", 
            Encoding.UTF8.GetBytes("{\"name\": \"test-index\"}"));

        // Add link
        AllureApi.AddLink("Documentation", "https://docs.microsoft.com/azure");
    }
}
```

### **Available Attributes**

| Attribute | Description | Example |
|-----------|-------------|---------|
| `[AllureSeverity]` | Test importance | `blocker`, `critical`, `normal`, `minor`, `trivial` |
| `[AllureOwner]` | Test owner | `"John Doe"` |
| `[AllureFeature]` | Feature name | `"Azure AI Search"` |
| `[AllureStory]` | User story | `"Search Index Creation"` |
| `[AllureTag]` | Tags | `"smoke"`, `"regression"` |
| `[AllureIssue]` | Issue link | `"JIRA-123"` |
| `[AllureTms]` | Test management | `"TMS-456"` |

---

## 📸 Screenshots and Attachments

Screenshots are automatically attached to Allure reports on test failures (configured in `TestHooks.cs`).

### **Manual Attachments**

```csharp
// Add text attachment
AllureApi.AddAttachment("Log Output", "text/plain", logContent);

// Add JSON attachment
AllureApi.AddAttachment("API Response", "application/json", jsonResponse);

// Add screenshot
AllureApi.AddAttachment("Screenshot", "image/png", screenshotPath);

// Add file
AllureApi.AddAttachment("Test Data", "text/csv", File.ReadAllBytes("data.csv"));
```

---

## 🔧 Configuration

### **Allure Configuration File** (`allureConfig.json`)

```json
{
  "allure": {
    "directory": "allure-results",
    "links": [
      "https://github.com/your-repo/issues/{issue}",
      "https://github.com/your-repo/issues/{tms}"
    ]
  }
}
```

### **Environment Variables**

You can set environment variables to customize Allure behavior:

```bash
# Set results directory
export ALLURE_RESULTS_DIRECTORY=./allure-results

# Set report directory
export ALLURE_REPORT_DIRECTORY=./allure-report
```

---

## 📊 Report Examples

### **Command Line Output**
```bash
$ allure serve allure-results

Generating report to temp directory...
Report successfully generated to /tmp/allure-report
Starting web server...
Server started at <http://localhost:45678/>. Press <Ctrl+C> to exit
```

### **Report Structure**
```
allure-report/
├── index.html          # Main report page
├── data/               # Test data
├── export/             # Export functionality
├── history/            # Historical data
├── plugins/            # Allure plugins
└── widgets/            # Dashboard widgets
```

---

## 🎯 Best Practices

### **1. Organize Tests with Features and Stories**
```csharp
[AllureFeature("Azure AI Services")]
[AllureStory("Document Intelligence")]
public class DocumentIntelligenceTests { }
```

### **2. Use Severity Levels Appropriately**
- `blocker` - Critical functionality, blocks testing
- `critical` - Major features, high priority
- `normal` - Standard functionality
- `minor` - Minor features, low priority
- `trivial` - UI/UX improvements

### **3. Add Meaningful Steps**
```csharp
AllureApi.Step("Given I have a valid Azure subscription", () => {
    // Setup code
});

AllureApi.Step("When I create a search index", () => {
    // Action code
});

AllureApi.Step("Then the index should be created successfully", () => {
    // Assertion code
});
```

### **4. Attach Relevant Information**
- API request/response bodies
- Configuration files
- Log files
- Screenshots on failures
- Performance metrics

### **5. Use Tags for Filtering**
```csharp
[AllureTag("smoke", "api", "azure")]
```

---

## 🚀 CI/CD Integration

### **GitHub Actions Example**

```yaml
name: Run Tests with Allure

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Install dependencies
      run: dotnet restore
    
    - name: Run tests
      run: dotnet test --logger "console;verbosity=detailed"
      working-directory: ./NewFramework/CSharpTests
    
    - name: Get Allure history
      uses: actions/checkout@v3
      if: always()
      continue-on-error: true
      with:
        ref: gh-pages
        path: gh-pages
    
    - name: Generate Allure Report
      uses: simple-elf/allure-report-action@master
      if: always()
      with:
        allure_results: NewFramework/CSharpTests/allure-results
        allure_history: allure-history
        keep_reports: 20
    
    - name: Deploy report to GitHub Pages
      if: always()
      uses: peaceiris/actions-gh-pages@v3
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_branch: gh-pages
        publish_dir: allure-history
```

### **Azure DevOps Pipeline Example**

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '9.0.x'

- script: dotnet restore
  displayName: 'Restore dependencies'

- script: dotnet test
  displayName: 'Run tests'
  workingDirectory: '$(System.DefaultWorkingDirectory)/NewFramework/CSharpTests'

- task: PublishAllureReport@1
  inputs:
    resultsDir: '$(System.DefaultWorkingDirectory)/NewFramework/CSharpTests/allure-results'
    reportDir: '$(Build.ArtifactStagingDirectory)/allure-report'

- task: PublishBuildArtifacts@1
  inputs:
    pathToPublish: '$(Build.ArtifactStagingDirectory)/allure-report'
    artifactName: 'allure-report'
```

---

## 🔍 Troubleshooting

### **Issue: Allure command not found**
```bash
# Verify installation
which allure

# Reinstall if needed
brew reinstall allure  # macOS
```

### **Issue: No test results found**
```bash
# Check if allure-results directory exists
ls -la allure-results/

# Ensure tests ran successfully
dotnet test --logger "console;verbosity=detailed"
```

### **Issue: Report not opening in browser**
```bash
# Generate static report instead
allure generate allure-results -o allure-report --clean

# Manually open
open allure-report/index.html
```

### **Issue: Screenshots not appearing**
- Verify screenshot path is correct in `TestHooks.cs`
- Ensure screenshots are saved before test completion
- Check file permissions

---

## 📚 Additional Resources

- **Allure Documentation**: https://docs.qameta.io/allure/
- **Allure .NET**: https://github.com/allure-framework/allure-csharp
- **Allure Examples**: https://demo.qameta.io/allure/
- **Allure Docker**: https://github.com/fescobar/allure-docker-service

---

## 🎨 Report Customization

### **Custom Categories** (`categories.json`)

Create a `categories.json` file in your test project:

```json
[
  {
    "name": "Product Defects",
    "matchedStatuses": ["failed"],
    "messageRegex": ".*AssertionError.*"
  },
  {
    "name": "Test Defects",
    "matchedStatuses": ["broken"],
    "messageRegex": ".*NullReferenceException.*"
  },
  {
    "name": "Known Issues",
    "matchedStatuses": ["failed"],
    "messageRegex": ".*KnownIssue.*"
  },
  {
    "name": "Flaky Tests",
    "matchedStatuses": ["failed", "passed"],
    "messageRegex": ".*"
  }
]
```

### **Custom Environment Info** (`environment.properties`)

```properties
Browser=Chrome
Environment=QA
API.Version=v2.0
Azure.Region=East US
Test.Suite=Azure AI Services
```

---

## 💡 Pro Tips

1. **Historical Trends**: Keep `allure-results/history` folder to track trends over time
2. **Parallel Execution**: Allure handles parallel test execution automatically
3. **Retries**: Failed test retries are tracked and displayed
4. **Flaky Tests**: Automatically detects tests that sometimes pass/fail
5. **Performance**: View test execution timeline to identify slow tests
6. **Export**: Export reports as JSON for custom processing

---

## 🎯 Quick Commands Reference

```bash
# Generate and serve report (auto-opens browser)
allure serve allure-results

# Generate static report
allure generate allure-results -o allure-report --clean

# Generate report with history
allure generate allure-results -o allure-report

# Open existing report
allure open allure-report

# Clean results
rm -rf allure-results/*

# Clean report
rm -rf allure-report/*
```

---

## ✅ Summary

You now have beautiful Allure reports integrated into your test framework! 

**Next Steps:**
1. Install Allure CLI: `brew install allure`
2. Run tests: `dotnet test`
3. Generate report: `allure serve allure-results`
4. Enjoy beautiful test reports! 🎉

For questions or issues, refer to the [Allure Documentation](https://docs.qameta.io/allure/).