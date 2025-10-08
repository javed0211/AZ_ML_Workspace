# 🚀 Allure Reports - Quick Start

## 30-Second Setup

```bash
# 1. Install Allure (one-time)
brew install allure  # macOS

# 2. Run tests and generate report
cd NewFramework/CSharpTests
./generate-allure-report.sh

# That's it! Report opens in browser automatically 🎉
```

---

## 📋 Common Commands

### **Run Tests & Generate Report**

```bash
# All tests with report
./generate-allure-report.sh

# Smoke tests only
./generate-allure-report.sh --smoke

# Specific category
./generate-allure-report.sh --category AzureAI

# Generate static HTML (doesn't auto-open)
./generate-allure-report.sh --static
```

### **Windows PowerShell**

```powershell
# All tests with report
.\generate-allure-report.ps1

# Smoke tests only
.\generate-allure-report.ps1 -Smoke

# Specific category
.\generate-allure-report.ps1 -Category AzureAI

# Generate static HTML
.\generate-allure-report.ps1 -Static
```

---

## 🎨 What You'll See

### **Dashboard Overview**
- ✅ Total tests: 35
- 📊 Pass rate: 94%
- ⏱️ Duration: 2m 15s
- 📈 Trend graphs

### **Test Details**
- Step-by-step execution
- Screenshots on failures
- API request/response
- Execution timeline
- Error stack traces

### **Filters & Categories**
- By feature (Azure AI Search, Document Intelligence)
- By severity (critical, normal, minor)
- By status (passed, failed, skipped)
- By tags (smoke, regression, api)

---

## 📸 Screenshots

Screenshots are **automatically captured** on test failures and attached to the report!

---

## 🔧 Manual Report Generation

```bash
# Step 1: Run tests
dotnet test

# Step 2: Generate report
allure serve allure-results

# Or generate static HTML
allure generate allure-results -o allure-report --clean
open allure-report/index.html
```

---

## 💡 Pro Tips

1. **Keep History**: Don't delete `allure-results/history` to see trends
2. **CI/CD**: Reports work great in GitHub Actions/Azure DevOps
3. **Parallel Tests**: Allure handles parallel execution automatically
4. **Export**: Export reports as JSON for custom dashboards

---

## 🆘 Troubleshooting

### Allure not found?
```bash
# macOS
brew install allure

# Windows
scoop install allure

# Verify
allure --version
```

### No test results?
```bash
# Check results directory
ls -la allure-results/

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### Report not opening?
```bash
# Generate static report
allure generate allure-results -o allure-report --clean

# Open manually
open allure-report/index.html  # macOS
start allure-report/index.html # Windows
```

---

## 📚 Full Documentation

See **[ALLURE_REPORTS_GUIDE.md](ALLURE_REPORTS_GUIDE.md)** for:
- Advanced configuration
- CI/CD integration
- Custom categories
- Report customization
- Best practices

---

## ✅ Checklist

- [ ] Install Allure CLI
- [ ] Run `./generate-allure-report.sh`
- [ ] View beautiful report in browser
- [ ] Share report with team
- [ ] Integrate with CI/CD (optional)

**That's it! Enjoy your beautiful test reports! 🎉**