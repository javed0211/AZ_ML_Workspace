module.exports = {
  default: {
    require: [
      'StepDefinitions/*.ts'
    ],
    requireModule: [
      'ts-node/register'
    ],
    format: [
      'progress-bar',
      'json:../../Reports/cucumber-report.json',
      'html:../../Reports/cucumber-report.html',
      'junit:../../Reports/junit-results.xml'
    ],
    formatOptions: {
      snippetInterface: 'async-await'
    },
    publishQuiet: true,
    dryRun: false,
    failFast: false,
    strict: true,
    worldParameters: {
      headless: true,
      slowMo: 0,
      timeout: 30000
    }
  }
};