using Serilog;

namespace PlaywrightFramework.Utils
{
    public static class LoggerExtensions
    {
        public static void LogInfo(this ILogger logger, string message)
        {
            logger.Information(message);
        }

        public static void LogWarning(this ILogger logger, string message)
        {
            logger.Warning(message);
        }

        public static void LogError(this ILogger logger, string message)
        {
            logger.Error(message);
        }

        public static void LogError(this ILogger logger, Exception ex, string message)
        {
            logger.Error(ex, message);
        }

        public static void LogAction(this ILogger logger, string action, string? element = null)
        {
            var message = !string.IsNullOrEmpty(element) ? $"🎯 Action: {action} on {element}" : $"🎯 Action: {action}";
            logger.Information(message);
        }

        public static void LogStep(this ILogger logger, string stepDescription)
        {
            logger.Information($"📝 Step: {stepDescription}");
        }

        // Extensions for the custom Logger class
        public static void LogInfo(this Logger logger, string message)
        {
            logger.Info(message);
        }

        public static void LogWarning(this Logger logger, string message)
        {
            logger.Warn(message);
        }

        public static void LogError(this Logger logger, string message)
        {
            logger.Error(message);
        }

        public static void LogError(this Logger logger, Exception exception, string message)
        {
            logger.Error(message, exception);
        }

        public static void LogAction(this Logger logger, string action, string? element = null)
        {
            logger.LogAction(action, element);
        }

        public static void LogStep(this Logger logger, string stepDescription)
        {
            logger.LogStep(stepDescription);
        }
    }
}