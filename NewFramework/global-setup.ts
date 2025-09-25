import { chromium, FullConfig } from '@playwright/test';
import { Logger } from './Utils/Logger';
import { ConfigManager } from './Utils/ConfigManager';
import * as fs from 'fs';
import * as path from 'path';

async function globalSetup(config: FullConfig) {
  const logger = Logger.getInstance();
  const configManager = ConfigManager.getInstance();
  
  logger.info('🚀 Starting global test setup...');
  
  // Create necessary directories
  const directories = [
    './Reports',
    './Reports/screenshots',
    './Reports/logs',
    './Reports/html-report',
    './Reports/test-results'
  ];
  
  directories.forEach(dir => {
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
      logger.info(`Created directory: ${dir}`);
    }
  });
  
  // Log configuration
  logger.info('Test Configuration:', {
    environment: configManager.getCurrentEnvironment(),
    browser: configManager.getBrowserSettings(),
    testSettings: configManager.getTestSettings()
  });
  
  // Warm up browser (optional)
  if (process.env.WARM_UP_BROWSER === 'true') {
    logger.info('Warming up browser...');
    const browser = await chromium.launch();
    const page = await browser.newPage();
    await page.goto('about:blank');
    await browser.close();
    logger.info('Browser warm-up completed');
  }
  
  logger.info('✅ Global setup completed successfully');
}

export default globalSetup;