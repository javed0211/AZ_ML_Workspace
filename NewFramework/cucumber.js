module.exports = {
  default: {
    // Feature files location - specify the directory, not a glob pattern
    features: ['TypeScriptTests/Features'],
    
    // Step definitions location
    require: [
      'TypeScriptTests/StepDefinitions/GoogleSearchSteps.ts',
      'TypeScriptTests/StepDefinitions/AzureMLWorkspaceSteps.ts'
    ],
    
    // Require TypeScript support
    requireModule: ['ts-node/register'],
    
    // Format options
    format: [
      'progress-bar',
      'json:Reports/cucumber-report.json',
      'html:Reports/cucumber-report.html',
      '@cucumber/pretty-formatter'
    ],
    
    // Parallel execution
    parallel: 2,
    
    // Retry failed scenarios
    retry: 1,
    
    // Tags to run specific scenarios (commented out to run all scenarios by default)
    // tags: '@typescript',
    
    // World parameters
    worldParameters: {
      headless: process.env.HEADLESS === 'true',
      browser: process.env.BROWSER || 'chromium',
      baseUrl: process.env.BASE_URL || 'https://www.google.com'
    },
    
    // Timeout settings
    timeout: 60000,
    
    // Publish results
    publish: false
  }
};