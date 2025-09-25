// Simple test to verify the framework setup
const { test, expect } = require('@playwright/test');

test('Framework setup verification', async ({ page }) => {
  console.log('✅ Playwright is working!');
  console.log('✅ Framework setup is complete!');
  
  // Simple navigation test
  await page.goto('https://example.com');
  await expect(page).toHaveTitle(/Example/);
  
  console.log('✅ Basic navigation test passed!');
});