import { FullConfig } from '@playwright/test';
import { Logger } from './Utils/Logger';
import * as fs from 'fs';
import * as path from 'path';

async function globalTeardown(config: FullConfig) {
  const logger = Logger.getInstance();
  
  logger.info('🧹 Starting global test teardown...');
  
  // Generate test summary
  const reportsDir = './Reports';
  const testResultsFile = path.join(reportsDir, 'test-results.json');
  
  if (fs.existsSync(testResultsFile)) {
    try {
      const results = JSON.parse(fs.readFileSync(testResultsFile, 'utf8'));
      const summary = {
        totalTests: results.stats?.total || 0,
        passed: results.stats?.passed || 0,
        failed: results.stats?.failed || 0,
        skipped: results.stats?.skipped || 0,
        duration: results.stats?.duration || 0
      };
      
      logger.info('📊 Test Execution Summary:', summary);
      
      // Write summary to file
      const summaryFile = path.join(reportsDir, 'test-summary.json');
      fs.writeFileSync(summaryFile, JSON.stringify(summary, null, 2));
      
    } catch (error) {
      logger.error('Failed to generate test summary', error);
    }
  }
  
  // Clean up old screenshots (keep last 10)
  const screenshotsDir = path.join(reportsDir, 'screenshots');
  if (fs.existsSync(screenshotsDir)) {
    try {
      const files = fs.readdirSync(screenshotsDir)
        .map(file => ({
          name: file,
          path: path.join(screenshotsDir, file),
          time: fs.statSync(path.join(screenshotsDir, file)).mtime.getTime()
        }))
        .sort((a, b) => b.time - a.time);
      
      if (files.length > 10) {
        const filesToDelete = files.slice(10);
        filesToDelete.forEach(file => {
          fs.unlinkSync(file.path);
          logger.debug(`Cleaned up old screenshot: ${file.name}`);
        });
        logger.info(`Cleaned up ${filesToDelete.length} old screenshots`);
      }
    } catch (error) {
      logger.error('Failed to clean up screenshots', error);
    }
  }
  
  // Log final message
  logger.info('✅ Global teardown completed successfully');
  logger.info('📁 Reports available in: ./Reports/');
  logger.info('🌐 HTML Report: ./Reports/html-report/index.html');
}

export default globalTeardown;