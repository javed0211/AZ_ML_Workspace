using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace PlaywrightFramework.Utils
{
    public class AzureConfig
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string ResourceGroup { get; set; } = string.Empty;
        public string ResourceGroupName => ResourceGroup; // Alias for consistency
        public string WorkspaceName { get; set; } = string.Empty;
        public string MLWorkspaceDisplayName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    public class MFAConfig
    {
        public bool Enabled { get; set; } = false;
        public bool AutoSubmitOTP { get; set; } = false;
        public int OTPTimeoutSeconds { get; set; } = 120;
        public string TOTPSecretKey { get; set; } = string.Empty;
    }

    public class AuthenticationConfig
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseDefaultCredentials { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 300;
        public MFAConfig? MFA { get; set; }
    }

    public class SpeechServicesConfig
    {
        public string SubscriptionKey { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string DefaultLanguage { get; set; } = "en-US";
        public string DefaultVoice { get; set; } = "en-US-JennyNeural";
        public string[] SupportedLanguages { get; set; } = Array.Empty<string>();
        public Dictionary<string, string>? CustomModels { get; set; }
    }

    public class EnvironmentConfig
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DatabaseConnection { get; set; } = string.Empty;
        public AzureConfig? Azure { get; set; }
        public AuthenticationConfig? Authentication { get; set; }
        public SpeechServicesConfig? SpeechServices { get; set; }
    }

    public class BrowserConfig
    {
        public string Type { get; set; } = "chromium";
        public bool Headless { get; set; } = false;
        public int SlowMo { get; set; } = 100;
        public int Timeout { get; set; } = 30000;
        public int ViewportWidth { get; set; } = 1920;
        public int ViewportHeight { get; set; } = 1080;
        public string[] Args { get; set; } = Array.Empty<string>();
    }

    public class TestConfig
    {
        public int DefaultTimeout { get; set; } = 30000;
        public int RetryCount { get; set; } = 2;
        public int ParallelWorkers { get; set; } = 4;
        public bool ScreenshotOnFailure { get; set; } = true;
        public bool VideoOnFailure { get; set; } = true;
        public bool TraceOnFailure { get; set; } = true;
    }

    public class LoggingConfig
    {
        public string LogLevel { get; set; } = "Info";
        public bool LogToFile { get; set; } = true;
        public bool LogToConsole { get; set; } = true;
        public string LogFilePath { get; set; } = "./Reports/logs/test-execution.log";
    }

    public class ElectronConfig
    {
        public string ExecutablePath { get; set; } = string.Empty;
        public string WindowsExecutablePath { get; set; } = string.Empty;
        public string LinuxExecutablePath { get; set; } = string.Empty;
        public string[] Args { get; set; } = Array.Empty<string>();
    }

    public class AppConfig
    {
        public string Environment { get; set; } = "dev";
        public Dictionary<string, EnvironmentConfig> Environments { get; set; } = new();
        public BrowserConfig Browser { get; set; } = new();
        public TestConfig TestSettings { get; set; } = new();
        public LoggingConfig Logging { get; set; } = new();
        public ElectronConfig ElectronApp { get; set; } = new();
    }

    public class ConfigManager
    {
        private static ConfigManager? _instance;
        private static readonly object _lock = new object();
        private readonly AppConfig _config;

        private ConfigManager()
        {
            _config = LoadConfiguration();
        }

        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ConfigManager();
                    }
                }
                return _instance;
            }
        }

        private AppConfig LoadConfiguration()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Config", "appsettings.json");
                var configFile = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<AppConfig>(configFile) ?? new AppConfig();

                // Override environment from environment variable if set
                var envFromProcess = Environment.GetEnvironmentVariable("TEST_ENV");
                if (!string.IsNullOrEmpty(envFromProcess) && config.Environments.ContainsKey(envFromProcess))
                {
                    config.Environment = envFromProcess;
                }

                return config;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load configuration: {ex.Message}", ex);
            }
        }

        public EnvironmentConfig GetCurrentEnvironment()
        {
            if (!_config.Environments.ContainsKey(_config.Environment))
            {
                throw new Exception($"Environment '{_config.Environment}' not found in configuration");
            }
            return _config.Environments[_config.Environment];
        }

        public BrowserConfig GetBrowserSettings() => _config.Browser;

        public TestConfig GetTestSettings() => _config.TestSettings;

        public LoggingConfig GetLoggingSettings() => _config.Logging;

        public ElectronConfig GetElectronSettings() => _config.ElectronApp;

        public AzureConfig GetAzureSettings()
        {
            var currentEnv = GetCurrentEnvironment();
            if (currentEnv.Azure == null)
            {
                throw new Exception($"Azure configuration not found for environment '{_config.Environment}'");
            }
            return currentEnv.Azure;
        }

        public AuthenticationConfig GetAuthenticationSettings()
        {
            var currentEnv = GetCurrentEnvironment();
            if (currentEnv.Authentication == null)
            {
                throw new Exception($"Authentication configuration not found for environment '{_config.Environment}'");
            }
            return currentEnv.Authentication;
        }

        public SpeechServicesConfig GetSpeechServicesSettings()
        {
            var currentEnv = GetCurrentEnvironment();
            if (currentEnv.SpeechServices == null)
            {
                throw new Exception($"SpeechServices configuration not found for environment '{_config.Environment}'");
            }
            return currentEnv.SpeechServices;
        }

        public EnvironmentConfig GetEnvironmentConfig(string envName)
        {
            if (!_config.Environments.ContainsKey(envName))
            {
                throw new Exception($"Environment '{envName}' not found in configuration");
            }
            return _config.Environments[envName];
        }

        public void SetEnvironment(string envName)
        {
            if (!_config.Environments.ContainsKey(envName))
            {
                throw new Exception($"Environment '{envName}' not found in configuration");
            }
            _config.Environment = envName;
        }

        public string[] GetAllEnvironments() => _config.Environments.Keys.ToArray();

        public AppConfig GetConfig() => _config;
    }
}