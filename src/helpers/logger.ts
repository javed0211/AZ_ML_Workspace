import * as winston from 'winston';
import { config } from './config';
import * as path from 'path';

// Custom log format for structured logging
const logFormat = winston.format.combine(
  winston.format.timestamp(),
  winston.format.errors({ stack: true }),
  winston.format.json(),
  winston.format.printf(({ timestamp, level, message, ...meta }) => {
    const logEntry = {
      timestamp,
      level,
      message,
      correlationId: meta.correlationId || 'unknown',
      testName: meta.testName || 'unknown',
      ...meta,
    };
    return JSON.stringify(logEntry);
  })
);

// Simple format for console output in development
const simpleFormat = winston.format.combine(
  winston.format.colorize(),
  winston.format.timestamp({ format: 'HH:mm:ss' }),
  winston.format.printf(({ timestamp, level, message, ...meta }) => {
    const metaStr = Object.keys(meta).length ? JSON.stringify(meta, null, 2) : '';
    return `${timestamp} [${level}]: ${message} ${metaStr}`;
  })
);

// Create logger instance
export const logger = winston.createLogger({
  level: config.logging.level,
  format: config.logging.format === 'json' ? logFormat : simpleFormat,
  defaultMeta: {
    service: 'azure-ml-automation',
    version: process.env.npm_package_version || '1.0.0',
  },
  transports: [
    // Console transport
    new winston.transports.Console({
      format: config.logging.format === 'json' ? logFormat : simpleFormat,
    }),
    
    // File transport for all logs
    new winston.transports.File({
      filename: path.join(config.artifacts.path, 'logs', 'combined.log'),
      format: logFormat,
    }),
    
    // File transport for errors only
    new winston.transports.File({
      filename: path.join(config.artifacts.path, 'logs', 'error.log'),
      level: 'error',
      format: logFormat,
    }),
  ],
  
  // Handle uncaught exceptions and rejections
  exceptionHandlers: [
    new winston.transports.File({
      filename: path.join(config.artifacts.path, 'logs', 'exceptions.log'),
      format: logFormat,
    }),
  ],
  
  rejectionHandlers: [
    new winston.transports.File({
      filename: path.join(config.artifacts.path, 'logs', 'rejections.log'),
      format: logFormat,
    }),
  ],
});

// Test-specific logger with correlation ID
export class TestLogger {
  private correlationId: string;
  private testName: string;

  constructor(testName: string, correlationId?: string) {
    this.testName = testName;
    this.correlationId = correlationId || this.generateCorrelationId();
  }

  private generateCorrelationId(): string {
    return `test-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  private log(level: string, message: string, meta: any = {}) {
    logger.log(level, message, {
      ...meta,
      testName: this.testName,
      correlationId: this.correlationId,
    });
  }

  info(message: string, meta?: any) {
    this.log('info', message, meta);
  }

  error(message: string, meta?: any) {
    this.log('error', message, meta);
  }

  warn(message: string, meta?: any) {
    this.log('warn', message, meta);
  }

  debug(message: string, meta?: any) {
    this.log('debug', message, meta);
  }

  step(stepName: string, meta?: any) {
    this.info(`Step: ${stepName}`, { step: stepName, ...meta });
  }

  action(actionName: string, meta?: any) {
    this.info(`Action: ${actionName}`, { action: actionName, ...meta });
  }

  assertion(assertion: string, result: boolean, meta?: any) {
    const level = result ? 'info' : 'error';
    this.log(level, `Assertion: ${assertion} - ${result ? 'PASSED' : 'FAILED'}`, {
      assertion,
      result,
      ...meta,
    });
  }

  getCorrelationId(): string {
    return this.correlationId;
  }
}

// Performance logging utilities
export class PerformanceLogger {
  private startTimes: Map<string, number> = new Map();

  start(operation: string): void {
    this.startTimes.set(operation, Date.now());
    logger.debug(`Performance: Started ${operation}`);
  }

  end(operation: string, meta?: any): number {
    const startTime = this.startTimes.get(operation);
    if (!startTime) {
      logger.warn(`Performance: No start time found for ${operation}`);
      return 0;
    }

    const duration = Date.now() - startTime;
    this.startTimes.delete(operation);

    logger.info(`Performance: ${operation} completed`, {
      operation,
      duration,
      ...meta,
    });

    return duration;
  }

  measure<T>(operation: string, fn: () => Promise<T>, meta?: any): Promise<T> {
    return new Promise(async (resolve, reject) => {
      this.start(operation);
      try {
        const result = await fn();
        this.end(operation, { success: true, ...meta });
        resolve(result);
      } catch (error) {
        this.end(operation, { success: false, error: error.message, ...meta });
        reject(error);
      }
    });
  }
}

// Export performance logger instance
export const performanceLogger = new PerformanceLogger();

// Utility functions for common logging patterns
export function logTestStart(testName: string, meta?: any): TestLogger {
  const testLogger = new TestLogger(testName);
  testLogger.info('Test started', meta);
  return testLogger;
}

export function logTestEnd(testLogger: TestLogger, success: boolean, meta?: any): void {
  testLogger.info(`Test ${success ? 'passed' : 'failed'}`, { success, ...meta });
}

export function logApiCall(method: string, url: string, statusCode?: number, meta?: any): void {
  logger.info('API call', {
    method,
    url,
    statusCode,
    ...meta,
  });
}

export function logBrowserAction(action: string, selector?: string, meta?: any): void {
  logger.debug('Browser action', {
    action,
    selector,
    ...meta,
  });
}

// Create logs directory if it doesn't exist
import * as fs from 'fs';
const logsDir = path.join(config.artifacts.path, 'logs');
if (!fs.existsSync(logsDir)) {
  fs.mkdirSync(logsDir, { recursive: true });
}