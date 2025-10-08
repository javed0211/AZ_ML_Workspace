# Azure AI Services Test Suite - Setup Checklist

## ✅ Pre-requisites Checklist

### Azure Resources
- [ ] **Azure Subscription** - Active Azure subscription
- [ ] **Azure AI Search Service** - Created and running
  - [ ] Service name noted
  - [ ] Endpoint URL copied
  - [ ] Admin API key obtained
  - [ ] Pricing tier selected (Basic or higher recommended)
- [ ] **Azure Document Intelligence Service** - Created and running
  - [ ] Service name noted
  - [ ] Endpoint URL copied
  - [ ] API key obtained
  - [ ] Region noted (some features region-specific)
- [ ] **Azure Storage Account** (Optional - for custom model training)
  - [ ] Blob container created
  - [ ] Training data uploaded
  - [ ] SAS token generated

### Local Environment
- [ ] **Node.js** - Version 18+ installed
- [ ] **npm** - Latest version
- [ ] **Python** - Version 3.8+ installed (for running generated scripts)
- [ ] **VS Code** - Installed and accessible
- [ ] **Git** - Repository cloned

### Project Setup
- [ ] Repository cloned to local machine
- [ ] Navigate to ElectronTests directory
- [ ] Dependencies installed (`npm install`)
- [ ] TypeScript compiled (`npm run build`)
- [ ] Playwright browsers installed (`npm run install-browsers`)

## 📝 Configuration Checklist

### Step 1: Update appsettings.json
Location: `/NewFramework/Config/appsettings.json`

- [ ] Open appsettings.json
- [ ] Locate "AzureAISearch" section
- [ ] Update the following fields:
  ```json
  "AzureAISearch": {
    "ServiceName": "your-search-service",           ← Update
    "Endpoint": "https://your-service.search...",   ← Update
    "ApiKey": "your-api-key",                       ← Update
    "IndexName": "documents-index"                  ← Keep or change
  }
  ```
- [ ] Locate "DocumentIntelligence" section
- [ ] Update the following fields:
  ```json
  "DocumentIntelligence": {
    "ServiceName": "your-doc-intelligence",         ← Update
    "Endpoint": "https://your-service.cognitive...", ← Update
    "ApiKey": "your-api-key"                        ← Update
  }
  ```
- [ ] Save the file

### Step 2: Verify VS Code Path
- [ ] Check VS Code executable path in appsettings.json
- [ ] Update if necessary:
  - **macOS:** `/Applications/Visual Studio Code.app/Contents/MacOS/Electron`
  - **Windows:** `C:\Users\%USERNAME%\AppData\Local\Programs\Microsoft VS Code\Code.exe`
  - **Linux:** `/usr/share/code/code`

### Step 3: Environment Variables (Optional)
- [ ] Create `.env` file in ElectronTests directory
- [ ] Add the following:
  ```bash
  AZURE_SEARCH_ENDPOINT=https://your-service.search.windows.net
  AZURE_SEARCH_API_KEY=your-api-key
  AZURE_DOC_INTEL_ENDPOINT=https://your-service.cognitiveservices.azure.com/
  AZURE_DOC_INTEL_API_KEY=your-api-key
  ```
- [ ] Add `.env` to `.gitignore` (security!)

## 🧪 Testing Checklist

### Pre-Test Verification
- [ ] All dependencies installed
- [ ] Configuration updated
- [ ] VS Code can be launched
- [ ] No other VS Code instances running
- [ ] Sufficient disk space for test artifacts

### Run Tests
- [ ] Navigate to ElectronTests directory
  ```bash
  cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/ElectronTests
  ```

- [ ] Build TypeScript
  ```bash
  npm run build
  ```

- [ ] Run all tests (first time - headed mode recommended)
  ```bash
  npx playwright test tests/azure-ai-services.test.ts --headed
  ```

- [ ] Verify tests are running
  - [ ] VS Code launches
  - [ ] Files are created
  - [ ] No errors in console

- [ ] Run specific test suites
  ```bash
  # AI Search only
  npx playwright test tests/azure-ai-services.test.ts -g "Azure AI Search"
  
  # Document Intelligence only
  npx playwright test tests/azure-ai-services.test.ts -g "Document Intelligence"
  
  # Integration only
  npx playwright test tests/azure-ai-services.test.ts -g "Integration"
  ```

### Post-Test Verification
- [ ] Check test results
  ```bash
  # View HTML report
  npx playwright show-report
  ```

- [ ] Verify generated files exist:
  - [ ] `azure-search-config.json`
  - [ ] `azure_search_indexer.py`
  - [ ] `azure_search_queries.py`
  - [ ] `document-intelligence-config.json`
  - [ ] `document_intelligence_analyzer.py`
  - [ ] `custom_model_trainer.py`
  - [ ] `integrated_document_pipeline.py`
  - [ ] `azure-ai-requirements.txt`

- [ ] Check screenshots in `test-results/`
- [ ] Review test logs for any warnings

## 🐍 Python Scripts Checklist

### Setup Python Environment
- [ ] Create virtual environment (recommended)
  ```bash
  python -m venv venv
  source venv/bin/activate  # macOS/Linux
  # or
  venv\Scripts\activate  # Windows
  ```

- [ ] Install dependencies
  ```bash
  pip install -r azure-ai-requirements.txt
  ```

### Test Python Scripts
- [ ] Update credentials in scripts
- [ ] Test Azure AI Search script
  ```bash
  python azure_search_indexer.py
  ```
- [ ] Test Document Intelligence script
  ```bash
  python document_intelligence_analyzer.py
  ```
- [ ] Test integrated pipeline
  ```bash
  python integrated_document_pipeline.py
  ```

## 🔍 Troubleshooting Checklist

### If Tests Fail to Start
- [ ] Check Node.js version: `node --version` (should be 18+)
- [ ] Check npm version: `npm --version`
- [ ] Rebuild project: `npm run clean && npm install && npm run build`
- [ ] Check VS Code path in config
- [ ] Close all VS Code instances
- [ ] Check disk space

### If VS Code Doesn't Launch
- [ ] Verify VS Code is installed
- [ ] Check executable path in appsettings.json
- [ ] Try launching VS Code manually
- [ ] Check system permissions
- [ ] Review error logs in console

### If Files Aren't Created
- [ ] Check write permissions in test directory
- [ ] Verify VS Code editor is accessible
- [ ] Check for errors in test output
- [ ] Run in headed mode to see what's happening
- [ ] Check screenshots for clues

### If Azure API Calls Fail
- [ ] Verify API keys are correct
- [ ] Check endpoint URLs
- [ ] Verify services are running in Azure Portal
- [ ] Check network connectivity
- [ ] Review Azure service quotas
- [ ] Check for rate limiting

### If Python Scripts Fail
- [ ] Verify Python version: `python --version` (should be 3.8+)
- [ ] Check dependencies installed: `pip list`
- [ ] Update credentials in scripts
- [ ] Check Azure service status
- [ ] Review error messages carefully

## 📊 Success Criteria

### Tests Pass If:
- [x] All 11 tests complete successfully
- [x] VS Code launches and closes properly
- [x] All 8 files are generated
- [x] No critical errors in logs
- [x] Screenshots show expected content
- [x] Test report shows all green

### Python Scripts Work If:
- [x] No import errors
- [x] Can connect to Azure services
- [x] Can create indexes (AI Search)
- [x] Can analyze documents (Document Intelligence)
- [x] Can search indexed documents
- [x] Proper error handling works

## 🎯 Next Steps After Success

### Immediate
- [ ] Review generated Python scripts
- [ ] Understand the code structure
- [ ] Read full documentation
- [ ] Experiment with modifications

### Short Term
- [ ] Customize scripts for your use case
- [ ] Add your own test documents
- [ ] Create custom search indexes
- [ ] Train custom Document Intelligence models

### Long Term
- [ ] Integrate into CI/CD pipeline
- [ ] Deploy to production
- [ ] Monitor performance
- [ ] Optimize costs
- [ ] Scale as needed

## 📚 Documentation Review Checklist

- [ ] Read `AZURE_AI_SERVICES_QUICK_START.md`
- [ ] Read `tests/AZURE_AI_SERVICES_README.md`
- [ ] Review `AZURE_AI_SERVICES_ARCHITECTURE.md`
- [ ] Understand `AZURE_AI_SERVICES_SUMMARY.md`
- [ ] Bookmark Azure documentation links

## 🔐 Security Checklist

- [ ] API keys not committed to Git
- [ ] `.env` file in `.gitignore`
- [ ] Credentials not in screenshots
- [ ] Access keys rotated regularly
- [ ] Least privilege access configured
- [ ] Azure Key Vault considered for production

## 💰 Cost Management Checklist

- [ ] Understand Azure AI Search pricing tier
- [ ] Understand Document Intelligence pricing
- [ ] Monitor usage in Azure Portal
- [ ] Set up cost alerts
- [ ] Delete test resources when not needed
- [ ] Use free tier for development if available

## 🎓 Learning Checklist

### Beginner
- [ ] Understand what Azure AI Search does
- [ ] Understand what Document Intelligence does
- [ ] Run tests successfully
- [ ] Review generated code

### Intermediate
- [ ] Modify test parameters
- [ ] Customize Python scripts
- [ ] Create custom indexes
- [ ] Process real documents

### Advanced
- [ ] Train custom models
- [ ] Optimize search performance
- [ ] Implement production pipeline
- [ ] Handle edge cases

## ✨ Optional Enhancements

- [ ] Add more test cases
- [ ] Implement error recovery
- [ ] Add performance benchmarks
- [ ] Create custom analyzers
- [ ] Implement caching
- [ ] Add monitoring/logging
- [ ] Create dashboards
- [ ] Automate deployment

## 📞 Support Resources

- [ ] Bookmark Azure AI Search docs
- [ ] Bookmark Document Intelligence docs
- [ ] Join Azure community forums
- [ ] Follow Azure updates
- [ ] Save support contact info

## ✅ Final Verification

Before considering setup complete:
- [ ] All tests pass
- [ ] All files generated
- [ ] Python scripts work
- [ ] Documentation reviewed
- [ ] Security measures in place
- [ ] Cost monitoring configured
- [ ] Backup/recovery plan considered

---

## 🎉 Completion

Once all items are checked:
- ✅ Setup is complete
- ✅ Tests are working
- ✅ Scripts are functional
- ✅ Ready for development
- ✅ Ready for production (with proper security)

**Congratulations! Your Azure AI Services test suite is ready to use!**

---

**Checklist Version:** 1.0.0  
**Last Updated:** 2024  
**Estimated Setup Time:** 30-60 minutes