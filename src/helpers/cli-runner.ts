import { spawn, ChildProcess } from 'child_process';
import * as crossSpawn from 'cross-spawn';
import * as treeKill from 'tree-kill';
import { logger, TestLogger } from './logger';
import { config } from './config';

export interface CliResult {
  exitCode: number;
  stdout: string;
  stderr: string;
  duration: number;
  command: string;
  args: string[];
}

export interface CliOptions {
  timeout?: number;
  retries?: number;
  retryDelay?: number;
  cwd?: string;
  env?: Record<string, string>;
  shell?: boolean;
  encoding?: BufferEncoding;
  expectNonZeroExit?: boolean;
  logOutput?: boolean;
  testLogger?: TestLogger;
}

export class CliRunner {
  private processes: Set<ChildProcess> = new Set();

  constructor(private defaultOptions: CliOptions = {}) {}

  async run(command: string, args: string[] = [], options: CliOptions = {}): Promise<CliResult> {
    const mergedOptions = { ...this.defaultOptions, ...options };
    const {
      timeout = config.timeouts.default,
      retries = config.retry.maxRetries,
      retryDelay = config.retry.delay,
      cwd = process.cwd(),
      env = {},
      shell = true,
      encoding = 'utf8',
      expectNonZeroExit = false,
      logOutput = true,
      testLogger,
    } = mergedOptions;

    const fullEnv = { ...process.env, ...env };
    let lastError: Error | null = null;

    for (let attempt = 0; attempt <= retries; attempt++) {
      if (attempt > 0) {
        const delay = retryDelay * Math.pow(2, attempt - 1); // Exponential backoff
        if (testLogger) {
          testLogger.warn(`CLI retry attempt ${attempt}/${retries} after ${delay}ms`, {
            command,
            args,
            attempt,
          });
        }
        await this.sleep(delay);
      }

      try {
        const result = await this.executeCommand(command, args, {
          timeout,
          cwd,
          env: fullEnv,
          shell,
          encoding,
          logOutput,
          testLogger,
        });

        if (!expectNonZeroExit && result.exitCode !== 0) {
          throw new Error(`Command failed with exit code ${result.exitCode}: ${result.stderr}`);
        }

        if (testLogger) {
          testLogger.info('CLI command completed successfully', {
            command,
            args,
            exitCode: result.exitCode,
            duration: result.duration,
            attempt: attempt + 1,
          });
        }

        return result;
      } catch (error) {
        lastError = error as Error;
        if (testLogger) {
          testLogger.error(`CLI command failed on attempt ${attempt + 1}`, {
            command,
            args,
            error: error.message,
            attempt: attempt + 1,
          });
        }

        if (attempt === retries) {
          break;
        }
      }
    }

    throw lastError || new Error('CLI command failed after all retries');
  }

  private async executeCommand(
    command: string,
    args: string[],
    options: {
      timeout: number;
      cwd: string;
      env: Record<string, string>;
      shell: boolean;
      encoding: BufferEncoding;
      logOutput: boolean;
      testLogger?: TestLogger;
    }
  ): Promise<CliResult> {
    return new Promise((resolve, reject) => {
      const startTime = Date.now();
      let stdout = '';
      let stderr = '';
      let timeoutId: NodeJS.Timeout;

      if (options.testLogger) {
        options.testLogger.action(`Executing CLI command: ${command} ${args.join(' ')}`, {
          command,
          args,
          cwd: options.cwd,
        });
      }

      const child = crossSpawn(command, args, {
        cwd: options.cwd,
        env: options.env,
        shell: options.shell,
        stdio: ['pipe', 'pipe', 'pipe'],
      });

      this.processes.add(child);

      // Set up timeout
      timeoutId = setTimeout(() => {
        if (options.testLogger) {
          options.testLogger.error('CLI command timed out', {
            command,
            args,
            timeout: options.timeout,
          });
        }
        this.killProcess(child);
        reject(new Error(`Command timed out after ${options.timeout}ms`));
      }, options.timeout);

      // Handle stdout
      child.stdout?.on('data', (data: Buffer) => {
        const output = data.toString(options.encoding);
        stdout += output;
        if (options.logOutput) {
          logger.debug('CLI stdout', { command, output: output.trim() });
        }
      });

      // Handle stderr
      child.stderr?.on('data', (data: Buffer) => {
        const output = data.toString(options.encoding);
        stderr += output;
        if (options.logOutput) {
          logger.debug('CLI stderr', { command, output: output.trim() });
        }
      });

      // Handle process exit
      child.on('close', (code: number | null) => {
        clearTimeout(timeoutId);
        this.processes.delete(child);

        const duration = Date.now() - startTime;
        const result: CliResult = {
          exitCode: code || 0,
          stdout: stdout.trim(),
          stderr: stderr.trim(),
          duration,
          command,
          args,
        };

        resolve(result);
      });

      // Handle process error
      child.on('error', (error: Error) => {
        clearTimeout(timeoutId);
        this.processes.delete(child);
        if (options.testLogger) {
          options.testLogger.error('CLI process error', { command, args, error: error.message });
        }
        reject(error);
      });
    });
  }

  private killProcess(child: ChildProcess): void {
    if (child.pid) {
      try {
        treeKill(child.pid, 'SIGTERM');
      } catch (error) {
        logger.warn('Failed to kill process tree', { pid: child.pid, error });
        try {
          child.kill('SIGKILL');
        } catch (killError) {
          logger.error('Failed to kill process', { pid: child.pid, error: killError });
        }
      }
    }
  }

  private sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  async cleanup(): Promise<void> {
    const promises = Array.from(this.processes).map(child => {
      return new Promise<void>(resolve => {
        if (child.pid) {
          this.killProcess(child);
          child.on('close', () => resolve());
          // Force resolve after 5 seconds
          setTimeout(() => resolve(), 5000);
        } else {
          resolve();
        }
      });
    });

    await Promise.all(promises);
    this.processes.clear();
  }

  // Azure CLI specific methods
  async azLogin(options: CliOptions = {}): Promise<CliResult> {
    return this.run('az', ['login'], options);
  }

  async azLoginServicePrincipal(
    clientId: string,
    clientSecret: string,
    tenantId: string,
    options: CliOptions = {}
  ): Promise<CliResult> {
    return this.run(
      'az',
      ['login', '--service-principal', '-u', clientId, '-p', clientSecret, '--tenant', tenantId],
      { ...options, logOutput: false } // Don't log secrets
    );
  }

  async azSetSubscription(subscriptionId: string, options: CliOptions = {}): Promise<CliResult> {
    return this.run('az', ['account', 'set', '--subscription', subscriptionId], options);
  }

  async azMlComputeStart(
    computeName: string,
    workspaceName: string,
    resourceGroup: string,
    options: CliOptions = {}
  ): Promise<CliResult> {
    return this.run(
      'az',
      [
        'ml',
        'compute',
        'start',
        '--name',
        computeName,
        '--workspace-name',
        workspaceName,
        '--resource-group',
        resourceGroup,
      ],
      options
    );
  }

  async azMlComputeStop(
    computeName: string,
    workspaceName: string,
    resourceGroup: string,
    options: CliOptions = {}
  ): Promise<CliResult> {
    return this.run(
      'az',
      [
        'ml',
        'compute',
        'stop',
        '--name',
        computeName,
        '--workspace-name',
        workspaceName,
        '--resource-group',
        resourceGroup,
      ],
      options
    );
  }

  async azMlComputeShow(
    computeName: string,
    workspaceName: string,
    resourceGroup: string,
    options: CliOptions = {}
  ): Promise<CliResult> {
    return this.run(
      'az',
      [
        'ml',
        'compute',
        'show',
        '--name',
        computeName,
        '--workspace-name',
        workspaceName,
        '--resource-group',
        resourceGroup,
        '--output',
        'json',
      ],
      options
    );
  }

  async azMlJobCreate(
    jobFile: string,
    workspaceName: string,
    resourceGroup: string,
    options: CliOptions = {}
  ): Promise<CliResult> {
    return this.run(
      'az',
      [
        'ml',
        'job',
        'create',
        '--file',
        jobFile,
        '--workspace-name',
        workspaceName,
        '--resource-group',
        resourceGroup,
      ],
      options
    );
  }

  // PowerShell specific methods
  async runPowerShell(script: string, options: CliOptions = {}): Promise<CliResult> {
    return this.run('powershell', ['-Command', script], options);
  }

  async runPowerShellFile(scriptPath: string, options: CliOptions = {}): Promise<CliResult> {
    return this.run('powershell', ['-File', scriptPath], options);
  }

  // Utility methods
  async waitForPattern(
    command: string,
    args: string[],
    pattern: RegExp,
    maxWaitTime: number = 60000,
    options: CliOptions = {}
  ): Promise<CliResult> {
    const startTime = Date.now();
    let lastResult: CliResult;

    while (Date.now() - startTime < maxWaitTime) {
      try {
        lastResult = await this.run(command, args, { ...options, retries: 0 });
        if (pattern.test(lastResult.stdout) || pattern.test(lastResult.stderr)) {
          return lastResult;
        }
      } catch (error) {
        // Continue waiting even if command fails
      }

      await this.sleep(2000); // Wait 2 seconds between checks
    }

    throw new Error(`Pattern ${pattern} not found within ${maxWaitTime}ms`);
  }

  parseJsonOutput(result: CliResult): any {
    try {
      return JSON.parse(result.stdout);
    } catch (error) {
      throw new Error(`Failed to parse JSON output: ${error.message}\nOutput: ${result.stdout}`);
    }
  }
}

// Global CLI runner instance
export const cliRunner = new CliRunner({
  timeout: config.timeouts.default,
  retries: config.retry.maxRetries,
  retryDelay: config.retry.delay,
  logOutput: true,
});