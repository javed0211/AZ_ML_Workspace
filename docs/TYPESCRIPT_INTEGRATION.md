# TypeScript Integration Guide

## Overview

The Azure ML Workspace Testing Framework supports TypeScript integration to enhance Playwright automation capabilities with advanced browser features, custom utilities, and type-safe scripting. This integration allows you to leverage TypeScript's powerful type system and modern JavaScript features alongside the C# Screenplay pattern.

## Table of Contents

1. [Setup and Configuration](#setup-and-configuration)
2. [Project Structure](#project-structure)
3. [Core Integration Patterns](#core-integration-patterns)
4. [Advanced Playwright Features](#advanced-playwright-features)
5. [Custom Utilities](#custom-utilities)
6. [Type Definitions](#type-definitions)
7. [Build and Deployment](#build-and-deployment)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

## Setup and Configuration

### Prerequisites

- Node.js 18+ and npm/yarn
- TypeScript 5.0+
- Playwright for TypeScript
- C# .NET 8.0 project (existing framework)

### Installation

1. **Initialize TypeScript project in the test framework:**

```bash
cd AzureMLWorkspace.Tests
mkdir typescript-utils
cd typescript-utils
npm init -y
```

2. **Install TypeScript dependencies:**

```bash
npm install --save-dev typescript @types/node
npm install --save-dev @playwright/test
npm install --save-dev ts-node tsx
```

3. **Install additional utilities:**

```bash
npm install --save-dev @types/uuid uuid
npm install --save-dev axios @types/axios
npm install --save-dev lodash @types/lodash
```

### TypeScript Configuration

Create `tsconfig.json`:

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "commonjs",
    "lib": ["ES2022", "DOM"],
    "outDir": "./dist",
    "rootDir": "./src",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "declaration": true,
    "declarationMap": true,
    "sourceMap": true,
    "resolveJsonModule": true,
    "experimentalDecorators": true,
    "emitDecoratorMetadata": true
  },
  "include": [
    "src/**/*",
    "types/**/*"
  ],
  "exclude": [
    "node_modules",
    "dist"
  ]
}
```

Create `package.json` scripts:

```json
{
  "scripts": {
    "build": "tsc",
    "build:watch": "tsc --watch",
    "clean": "rimraf dist",
    "test": "playwright test",
    "lint": "eslint src --ext .ts",
    "format": "prettier --write src/**/*.ts"
  }
}
```

## Project Structure

```
AzureMLWorkspace.Tests/
├── typescript-utils/
│   ├── src/
│   │   ├── abilities/           # TypeScript abilities
│   │   ├── tasks/              # TypeScript tasks
│   │   ├── questions/          # TypeScript questions
│   │   ├── utilities/          # Helper utilities
│   │   ├── types/              # Type definitions
│   │   ├── config/             # Configuration
│   │   └── index.ts            # Main exports
│   ├── types/                  # Global type definitions
│   ├── dist/                   # Compiled JavaScript
│   ├── tests/                  # TypeScript-specific tests
│   ├── package.json
│   ├── tsconfig.json
│   └── playwright.config.ts
└── Framework/                  # Existing C# framework
```

## Core Integration Patterns

### 1. TypeScript Ability Integration

Create TypeScript abilities that can be called from C#:

```typescript
// src/abilities/AdvancedBrowserAbility.ts
import { Page, Browser, BrowserContext } from '@playwright/test';

export interface NetworkLog {
  url: string;
  method: string;
  status?: number;
  timestamp: number;
  duration?: number;
  headers: Record<string, string>;
}

export interface PerformanceMetrics {
  loadTime: number;
  domContentLoaded: number;
  firstContentfulPaint: number;
  largestContentfulPaint: number;
  cumulativeLayoutShift: number;
}

export class AdvancedBrowserAbility {
  private networkLogs: NetworkLog[] = [];
  private performanceMetrics: PerformanceMetrics | null = null;

  constructor(private page: Page) {
    this.setupNetworkLogging();
    this.setupPerformanceMonitoring();
  }

  private setupNetworkLogging(): void {
    this.page.on('request', (request) => {
      this.networkLogs.push({
        url: request.url(),
        method: request.method(),
        timestamp: Date.now(),
        headers: request.headers()
      });
    });

    this.page.on('response', (response) => {
      const log = this.networkLogs.find(l => l.url === response.url());
      if (log) {
        log.status = response.status();
        log.duration = Date.now() - log.timestamp;
      }
    });
  }

  private async setupPerformanceMonitoring(): Promise<void> {
    await this.page.addInitScript(() => {
      window.addEventListener('load', () => {
        setTimeout(() => {
          const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
          const paint = performance.getEntriesByType('paint');
          
          (window as any).performanceData = {
            loadTime: navigation.loadEventEnd - navigation.loadEventStart,
            domContentLoaded: navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart,
            firstContentfulPaint: paint.find(p => p.name === 'first-contentful-paint')?.startTime || 0,
            // Additional metrics would be collected here
          };
        }, 1000);
      });
    });
  }

  async captureNetworkTraffic(): Promise<NetworkLog[]> {
    return [...this.networkLogs];
  }

  async getPerformanceMetrics(): Promise<PerformanceMetrics | null> {
    try {
      const metrics = await this.page.evaluate(() => (window as any).performanceData);
      return metrics;
    } catch (error) {
      console.error('Failed to get performance metrics:', error);
      return null;
    }
  }

  async captureFullPageScreenshot(options?: {
    quality?: number;
    format?: 'png' | 'jpeg';
    fullPage?: boolean;
  }): Promise<Buffer> {
    return await this.page.screenshot({
      fullPage: options?.fullPage ?? true,
      type: options?.format ?? 'png',
      quality: options?.quality
    });
  }

  async interceptRequests(urlPattern: string, mockResponse: any): Promise<void> {
    await this.page.route(urlPattern, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockResponse)
      });
    });
  }

  async waitForNetworkIdle(timeout: number = 30000): Promise<void> {
    await this.page.waitForLoadState('networkidle', { timeout });
  }

  async emulateDevice(deviceName: string): Promise<void> {
    const devices = require('@playwright/test').devices;
    if (devices[deviceName]) {
      await this.page.setViewportSize(devices[deviceName].viewport);
      await this.page.setUserAgent(devices[deviceName].userAgent);
    }
  }
}
```

### 2. TypeScript Task Implementation

```typescript
// src/tasks/AdvancedNavigationTask.ts
import { Page } from '@playwright/test';

export interface NavigationOptions {
  waitForSelector?: string;
  timeout?: number;
  expectedTitle?: string;
  performanceThreshold?: number;
}

export class AdvancedNavigationTask {
  constructor(
    private page: Page,
    private url: string,
    private options: NavigationOptions = {}
  ) {}

  async execute(): Promise<void> {
    const startTime = Date.now();

    try {
      // Navigate with advanced options
      await this.page.goto(this.url, {
        waitUntil: 'networkidle',
        timeout: this.options.timeout || 30000
      });

      // Wait for specific selector if provided
      if (this.options.waitForSelector) {
        await this.page.waitForSelector(this.options.waitForSelector, {
          timeout: this.options.timeout || 30000
        });
      }

      // Validate title if expected
      if (this.options.expectedTitle) {
        const actualTitle = await this.page.title();
        if (!actualTitle.includes(this.options.expectedTitle)) {
          throw new Error(`Expected title to contain "${this.options.expectedTitle}", but got "${actualTitle}"`);
        }
      }

      // Check performance threshold
      if (this.options.performanceThreshold) {
        const loadTime = Date.now() - startTime;
        if (loadTime > this.options.performanceThreshold) {
          console.warn(`Navigation took ${loadTime}ms, exceeding threshold of ${this.options.performanceThreshold}ms`);
        }
      }

    } catch (error) {
      throw new Error(`Advanced navigation failed: ${error.message}`);
    }
  }
}
```

### 3. TypeScript Questions

```typescript
// src/questions/ElementQuestions.ts
import { Page, Locator } from '@playwright/test';

export class ElementQuestions {
  constructor(private page: Page) {}

  async isElementVisible(selector: string): Promise<boolean> {
    try {
      const element = this.page.locator(selector);
      return await element.isVisible();
    } catch {
      return false;
    }
  }

  async getElementText(selector: string): Promise<string> {
    const element = this.page.locator(selector);
    return await element.textContent() || '';
  }

  async getElementAttribute(selector: string, attribute: string): Promise<string | null> {
    const element = this.page.locator(selector);
    return await element.getAttribute(attribute);
  }

  async countElements(selector: string): Promise<number> {
    const elements = this.page.locator(selector);
    return await elements.count();
  }

  async isElementEnabled(selector: string): Promise<boolean> {
    try {
      const element = this.page.locator(selector);
      return await element.isEnabled();
    } catch {
      return false;
    }
  }

  async waitForElementState(
    selector: string, 
    state: 'visible' | 'hidden' | 'attached' | 'detached',
    timeout: number = 30000
  ): Promise<boolean> {
    try {
      const element = this.page.locator(selector);
      await element.waitFor({ state, timeout });
      return true;
    } catch {
      return false;
    }
  }
}
```

## Advanced Playwright Features

### 1. Custom Browser Context Management

```typescript
// src/utilities/BrowserContextManager.ts
import { Browser, BrowserContext, Page } from '@playwright/test';

export interface ContextConfig {
  viewport?: { width: number; height: number };
  userAgent?: string;
  locale?: string;
  timezone?: string;
  permissions?: string[];
  geolocation?: { latitude: number; longitude: number };
  colorScheme?: 'light' | 'dark' | 'no-preference';
}

export class BrowserContextManager {
  private contexts: Map<string, BrowserContext> = new Map();

  constructor(private browser: Browser) {}

  async createContext(name: string, config: ContextConfig = {}): Promise<BrowserContext> {
    const context = await this.browser.newContext({
      viewport: config.viewport,
      userAgent: config.userAgent,
      locale: config.locale,
      timezoneId: config.timezone,
      permissions: config.permissions,
      geolocation: config.geolocation,
      colorScheme: config.colorScheme
    });

    this.contexts.set(name, context);
    return context;
  }

  async getContext(name: string): Promise<BrowserContext | undefined> {
    return this.contexts.get(name);
  }

  async closeContext(name: string): Promise<void> {
    const context = this.contexts.get(name);
    if (context) {
      await context.close();
      this.contexts.delete(name);
    }
  }

  async closeAllContexts(): Promise<void> {
    for (const [name, context] of this.contexts) {
      await context.close();
    }
    this.contexts.clear();
  }
}
```

### 2. Advanced Element Interactions

```typescript
// src/utilities/ElementInteractions.ts
import { Page, Locator } from '@playwright/test';

export class ElementInteractions {
  constructor(private page: Page) {}

  async smartClick(selector: string, options: {
    retries?: number;
    delay?: number;
    force?: boolean;
    waitForStable?: boolean;
  } = {}): Promise<void> {
    const element = this.page.locator(selector);
    const retries = options.retries || 3;
    
    for (let i = 0; i < retries; i++) {
      try {
        if (options.waitForStable) {
          await element.waitFor({ state: 'visible' });
          await this.page.waitForTimeout(100); // Brief stability wait
        }
        
        await element.click({ 
          force: options.force,
          delay: options.delay 
        });
        return;
      } catch (error) {
        if (i === retries - 1) throw error;
        await this.page.waitForTimeout(1000);
      }
    }
  }

  async typeWithDelay(selector: string, text: string, delay: number = 100): Promise<void> {
    const element = this.page.locator(selector);
    await element.clear();
    
    for (const char of text) {
      await element.type(char);
      await this.page.waitForTimeout(delay);
    }
  }

  async selectDropdownOption(selector: string, option: string | number): Promise<void> {
    const dropdown = this.page.locator(selector);
    await dropdown.click();
    
    if (typeof option === 'string') {
      await this.page.locator(`text=${option}`).click();
    } else {
      await this.page.locator(`${selector} option`).nth(option).click();
    }
  }

  async dragAndDrop(sourceSelector: string, targetSelector: string): Promise<void> {
    const source = this.page.locator(sourceSelector);
    const target = this.page.locator(targetSelector);
    
    await source.dragTo(target);
  }

  async scrollIntoView(selector: string): Promise<void> {
    const element = this.page.locator(selector);
    await element.scrollIntoViewIfNeeded();
  }

  async hoverAndClick(hoverSelector: string, clickSelector: string): Promise<void> {
    await this.page.locator(hoverSelector).hover();
    await this.page.locator(clickSelector).click();
  }
}
```

## Custom Utilities

### 1. Test Data Management

```typescript
// src/utilities/TestDataManager.ts
import * as fs from 'fs';
import * as path from 'path';

export interface TestData {
  [key: string]: any;
}

export class TestDataManager {
  private dataCache: Map<string, TestData> = new Map();

  async loadTestData(fileName: string): Promise<TestData> {
    if (this.dataCache.has(fileName)) {
      return this.dataCache.get(fileName)!;
    }

    const filePath = path.join(__dirname, '../../TestData', fileName);
    
    try {
      const fileContent = await fs.promises.readFile(filePath, 'utf-8');
      const data = JSON.parse(fileContent);
      this.dataCache.set(fileName, data);
      return data;
    } catch (error) {
      throw new Error(`Failed to load test data from ${fileName}: ${error.message}`);
    }
  }

  generateRandomData(template: any): any {
    // Implementation for generating random test data based on template
    return this.processTemplate(template);
  }

  private processTemplate(template: any): any {
    if (typeof template === 'string') {
      return this.processStringTemplate(template);
    } else if (Array.isArray(template)) {
      return template.map(item => this.processTemplate(item));
    } else if (typeof template === 'object' && template !== null) {
      const result: any = {};
      for (const [key, value] of Object.entries(template)) {
        result[key] = this.processTemplate(value);
      }
      return result;
    }
    return template;
  }

  private processStringTemplate(template: string): string {
    return template.replace(/\{\{(\w+)\}\}/g, (match, placeholder) => {
      switch (placeholder) {
        case 'randomEmail':
          return `test${Math.random().toString(36).substr(2, 9)}@example.com`;
        case 'randomName':
          return `TestUser${Math.random().toString(36).substr(2, 5)}`;
        case 'randomNumber':
          return Math.floor(Math.random() * 1000).toString();
        case 'timestamp':
          return Date.now().toString();
        default:
          return match;
      }
    });
  }
}
```

### 2. API Testing Utilities

```typescript
// src/utilities/ApiTestHelper.ts
import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';

export interface ApiTestConfig {
  baseURL: string;
  timeout?: number;
  headers?: Record<string, string>;
  retries?: number;
}

export class ApiTestHelper {
  private client: AxiosInstance;
  private retries: number;

  constructor(config: ApiTestConfig) {
    this.retries = config.retries || 3;
    this.client = axios.create({
      baseURL: config.baseURL,
      timeout: config.timeout || 30000,
      headers: config.headers || {}
    });

    this.setupInterceptors();
  }

  private setupInterceptors(): void {
    this.client.interceptors.request.use(
      (config) => {
        console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`);
        return config;
      },
      (error) => Promise.reject(error)
    );

    this.client.interceptors.response.use(
      (response) => {
        console.log(`API Response: ${response.status} ${response.config.url}`);
        return response;
      },
      (error) => {
        console.error(`API Error: ${error.response?.status} ${error.config?.url}`);
        return Promise.reject(error);
      }
    );
  }

  async get<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.executeWithRetry(() => this.client.get<T>(url, config));
  }

  async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.executeWithRetry(() => this.client.post<T>(url, data, config));
  }

  async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.executeWithRetry(() => this.client.put<T>(url, data, config));
  }

  async delete<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.executeWithRetry(() => this.client.delete<T>(url, config));
  }

  private async executeWithRetry<T>(operation: () => Promise<T>): Promise<T> {
    let lastError: any;
    
    for (let i = 0; i < this.retries; i++) {
      try {
        return await operation();
      } catch (error) {
        lastError = error;
        if (i < this.retries - 1) {
          await this.delay(1000 * (i + 1)); // Exponential backoff
        }
      }
    }
    
    throw lastError;
  }

  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}
```

## Type Definitions

### 1. Global Types

```typescript
// types/global.d.ts
declare global {
  interface Window {
    performanceData?: {
      loadTime: number;
      domContentLoaded: number;
      firstContentfulPaint: number;
      largestContentfulPaint: number;
      cumulativeLayoutShift: number;
    };
  }
}

export {};
```

### 2. Framework Types

```typescript
// src/types/framework.ts
export interface TestContext {
  testName: string;
  startTime: number;
  endTime?: number;
  status: 'running' | 'passed' | 'failed' | 'skipped';
  artifacts: string[];
  metadata: Record<string, any>;
}

export interface TestResult {
  success: boolean;
  message?: string;
  data?: any;
  duration: number;
  screenshots?: string[];
  logs?: string[];
}

export interface BrowserConfig {
  headless: boolean;
  slowMo: number;
  viewport: { width: number; height: number };
  userAgent?: string;
  locale?: string;
  timezone?: string;
}

export interface TestEnvironment {
  name: string;
  baseUrl: string;
  apiUrl: string;
  credentials: {
    username?: string;
    password?: string;
    apiKey?: string;
  };
  browser: BrowserConfig;
}
```

## Build and Deployment

### 1. Build Configuration

Create `build.config.ts`:

```typescript
// build.config.ts
import { defineConfig } from 'typescript';

export default defineConfig({
  build: {
    outDir: 'dist',
    sourcemap: true,
    minify: false,
    target: 'es2022',
    lib: ['es2022', 'dom'],
    rollupOptions: {
      external: ['@playwright/test'],
      output: {
        globals: {
          '@playwright/test': 'PlaywrightTest'
        }
      }
    }
  }
});
```

### 2. Integration with C# Project

Update the C# project file to include TypeScript build:

```xml
<!-- Add to AzureMLWorkspace.Tests.csproj -->
<Target Name="BuildTypeScript" BeforeTargets="Build">
  <Exec Command="npm run build" WorkingDirectory="typescript-utils" />
</Target>

<Target Name="CleanTypeScript" BeforeTargets="Clean">
  <Exec Command="npm run clean" WorkingDirectory="typescript-utils" />
</Target>
```

### 3. C# Integration Class

```csharp
// Framework/Utilities/TypeScriptIntegration.cs
using System.Diagnostics;
using System.Text.Json;

public class TypeScriptIntegration
{
    private readonly string _typeScriptPath;
    private readonly ILogger<TypeScriptIntegration> _logger;

    public TypeScriptIntegration(ILogger<TypeScriptIntegration> logger)
    {
        _logger = logger;
        _typeScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "typescript-utils");
    }

    public async Task<T> ExecuteTypeScriptFunction<T>(string functionName, object parameters = null)
    {
        var paramJson = parameters != null ? JsonSerializer.Serialize(parameters) : "{}";
        var script = $"node -e \"const {{ {functionName} }} = require('./dist/index.js'); {functionName}({paramJson}).then(console.log);\"";

        var processInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = $"-e \"const {{ {functionName} }} = require('./dist/index.js'); {functionName}({paramJson}).then(console.log);\"",
            WorkingDirectory = _typeScriptPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"TypeScript execution failed: {error}");
        }

        return JsonSerializer.Deserialize<T>(output);
    }
}
```

## Best Practices

### 1. Code Organization

- **Separate concerns**: Keep abilities, tasks, and questions in separate directories
- **Use TypeScript interfaces**: Define clear contracts for all public APIs
- **Implement error handling**: Always handle potential failures gracefully
- **Add comprehensive logging**: Log important operations and errors

### 2. Performance Optimization

- **Lazy loading**: Load heavy resources only when needed
- **Connection pooling**: Reuse browser contexts and pages when possible
- **Async operations**: Use async/await consistently
- **Memory management**: Properly dispose of resources

### 3. Testing Strategy

- **Unit tests**: Test individual TypeScript utilities
- **Integration tests**: Test C# to TypeScript integration
- **End-to-end tests**: Test complete workflows

### 4. Maintenance

- **Version pinning**: Pin dependency versions for stability
- **Regular updates**: Keep dependencies updated for security
- **Documentation**: Maintain up-to-date documentation
- **Code reviews**: Review all TypeScript changes

## Troubleshooting

### Common Issues

1. **Node.js version compatibility**
   - Ensure Node.js 18+ is installed
   - Use `.nvmrc` file to specify version

2. **TypeScript compilation errors**
   - Check `tsconfig.json` configuration
   - Verify all dependencies are installed
   - Clear `node_modules` and reinstall if needed

3. **C# to TypeScript communication**
   - Verify JSON serialization/deserialization
   - Check process execution permissions
   - Validate working directory paths

4. **Playwright integration issues**
   - Ensure Playwright browsers are installed
   - Check browser launch configuration
   - Verify page context management

### Debugging Tips

- Use `console.log` for debugging TypeScript code
- Enable verbose logging in C# integration
- Use browser developer tools for web debugging
- Check process output and error streams

### Performance Issues

- Monitor memory usage during test execution
- Profile TypeScript code execution time
- Optimize browser context reuse
- Implement proper cleanup procedures

This comprehensive TypeScript integration guide provides everything needed to enhance your Azure ML Workspace Testing Framework with powerful TypeScript capabilities while maintaining the existing C# Screenplay pattern architecture.