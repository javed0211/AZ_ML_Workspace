using Serilog;
using Serilog.Events;

namespace PlaywrightFramework.Utils
{
    public class Logger
    {
        private static Logger? _instance;
        private static readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly ConfigManager _config;

        private Logger()
        {
            _config = ConfigManager.Instance;
            _logger = InitializeLogger();
        }

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new Logger();
                    }
                }
                return _instance;
            }
        }

        private ILogger InitializeLogger()
        {
            var loggingConfig = _config.GetLoggingSettings();
            
            // Ensure log directory exists
            var logDir = Path.GetDirectoryName(loggingConfig.LogFilePath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            var loggerConfig = new LoggerConfiguration();

            // Set log level
            var logLevel = Enum.Parse<LogEventLevel>(loggingConfig.LogLevel, true);
            loggerConfig.MinimumLevel.Is(logLevel);

            // Console sink
            if (loggingConfig.LogToConsole)
            {
                loggerConfig.WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                );
            }

            // File sink
            if (loggingConfig.LogToFile)
            {
                loggerConfig.WriteTo.File(
                    loggingConfig.LogFilePath,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7
                );
            }

            return loggerConfig.CreateLogger();
        }

        public void Info(string message, object? data = null)
        {
            if (data != null)
                _logger.Information(message + " {@Data}", data);
            else
                _logger.Information(message);
        }

        public void Error(string message, Exception? exception = null, object? data = null)
        {
            if (data != null)
                _logger.Error(exception, message + " {@Data}", data);
            else
                _logger.Error(exception, message);
        }

        public void Warn(string message, object? data = null)
        {
            if (data != null)
                _logger.Warning(message + " {@Data}", data);
            else
                _logger.Warning(message);
        }

        public void Debug(string message, object? data = null)
        {
            if (data != null)
                _logger.Debug(message + " {@Data}", data);
            else
                _logger.Debug(message);
        }

        public void LogTestStart(string testName)
        {
            Info($"🚀 Starting test: {testName}");
        }

        public void LogTestEnd(string testName, string status)
        {
            var emoji = status == "PASSED" ? "✅" : "❌";
            Info($"{emoji} Test completed: {testName} - {status}");
        }

        public void LogStep(string stepDescription)
        {
            Info($"📝 Step: {stepDescription}");
        }

        public void LogAction(string action, string? element = null)
        {
            var message = !string.IsNullOrEmpty(element) ? $"🎯 Action: {action} on {element}" : $"🎯 Action: {action}";
            Info(message);
        }

        // Additional methods for compatibility
        public void LogInfo(string message, object? data = null)
        {
            Info(message, data);
        }

        public void LogError(string message, Exception? exception = null, object? data = null)
        {
            Error(message, exception, data);
        }

        public void LogWarning(string message, object? data = null)
        {
            Warn(message, data);
        }

        public void Information(string message, object? data = null)
        {
            Info(message, data);
        }

        public void Warning(string message, object? data = null)
        {
            Warn(message, data);
        }
    }
}