import * as winston from 'winston';
import * as fs from 'fs';
import * as path from 'path';
import { ConfigManager } from './ConfigManager';

export class Logger {
  private static instance: Logger;
  private logger: winston.Logger;
  private config = ConfigManager.getInstance();

  private constructor() {
    this.initializeLogger();
  }

  public static getInstance(): Logger {
    if (!Logger.instance) {
      Logger.instance = new Logger();
    }
    return Logger.instance;
  }

  private initializeLogger(): void {
    const loggingConfig = this.config.getLoggingSettings();
    
    // Ensure log directory exists
    const logDir = path.dirname(loggingConfig.LogFilePath);
    if (!fs.existsSync(logDir)) {
      fs.mkdirSync(logDir, { recursive: true });
    }

    const transports: winston.transport[] = [];

    // Console transport
    if (loggingConfig.LogToConsole) {
      transports.push(
        new winston.transports.Console({
          format: winston.format.combine(
            winston.format.colorize(),
            winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
            winston.format.printf(({ timestamp, level, message }) => {
              return `${timestamp} [${level}]: ${message}`;
            })
          )
        })
      );
    }

    // File transport
    if (loggingConfig.LogToFile) {
      transports.push(
        new winston.transports.File({
          filename: loggingConfig.LogFilePath,
          format: winston.format.combine(
            winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
            winston.format.json()
          )
        })
      );
    }

    this.logger = winston.createLogger({
      level: loggingConfig.LogLevel.toLowerCase(),
      transports: transports
    });
  }

  public info(message: string, meta?: any): void {
    this.logger.info(message, meta);
  }

  public error(message: string, meta?: any): void {
    this.logger.error(message, meta);
  }

  public warn(message: string, meta?: any): void {
    this.logger.warn(message, meta);
  }

  public debug(message: string, meta?: any): void {
    this.logger.debug(message, meta);
  }

  public logTestStart(testName: string): void {
    this.info(`🚀 Starting test: ${testName}`);
  }

  public logTestEnd(testName: string, status: 'PASSED' | 'FAILED'): void {
    const emoji = status === 'PASSED' ? '✅' : '❌';
    this.info(`${emoji} Test completed: ${testName} - ${status}`);
  }

  public logStep(stepDescription: string): void {
    this.info(`📝 Step: ${stepDescription}`);
  }

  public logAction(action: string, element?: string): void {
    const message = element ? `🎯 Action: ${action} on ${element}` : `🎯 Action: ${action}`;
    this.info(message);
  }
}