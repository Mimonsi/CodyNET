using System.Diagnostics;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CodyNET.Common.Utils;

public enum TimeSetting
{
    Absolute,
    AbsoluteWithDate,
    Relative
}

public enum LogLevel
{
    Error,
    Warn,
    Info,
    Debug,
    Trace,
}

public static class Log
{
    private const string DisableFileSinkEnvVar = "CODYNET_LOG_DISABLE_FILE";
    private const string StartNewFileOnStartupEnvVar = "CODYNET_LOG_NEW_FILE_ON_START";
    private const string TimeSettingEnvVar = "CODYNET_LOG_TIME_SETTING";

    private static readonly object InitLock = new();
    private static readonly LoggingLevelSwitch ConsoleLevelSwitch = new(MapLevel(LogLevel.Debug));
    private static readonly LoggingLevelSwitch FileLevelSwitch = new(MapLevel(LogLevel.Trace));
    private static readonly DateTimeOffset ProcessStartTime = new(Process.GetCurrentProcess().StartTime);

    private static bool initialized;
    private static LogLevel level = LogLevel.Debug;
    private static LogLevel consoleLevel = LogLevel.Debug;
    private static LogLevel fileLevel = LogLevel.Trace;
    private static TimeSetting timeSetting =
        ParseTimeSetting(Environment.GetEnvironmentVariable(TimeSettingEnvVar)) ?? TimeSetting.Relative;

    public static bool FileLoggingEnabled { get; set; } =
        !IsTrue(Environment.GetEnvironmentVariable(DisableFileSinkEnvVar));

    public static bool StartNewFileOnStartup { get; set; } =
        IsTrue(Environment.GetEnvironmentVariable(StartNewFileOnStartupEnvVar));
    
    public static bool BreakOnLoggedErrors { get; set; } = true;

    public static string? LogFilePath { get; private set; }

    public static TimeSetting TimeSetting
    {
        get => timeSetting;
        set => timeSetting = value;
    }

    public static LogLevel Level
    {
        get => level;
        set
        {
            level = value;
            ConsoleLevel = value;
            FileLevel = value;
        }
    }
    
    public static LogLevel ConsoleLevel
    {
        get => consoleLevel;
        set
        {
            consoleLevel = value;
            ConsoleLevelSwitch.MinimumLevel = MapLevel(value);
        }
    }

    public static LogLevel FileLevel
    {
        get => fileLevel;
        set
        {
            fileLevel = value;
            FileLevelSwitch.MinimumLevel = MapLevel(value);
        }
    }

    public static void Initialize() => EnsureInitialized();

    public static void Trace(string message)
    {
        EnsureInitialized();
        global::Serilog.Log.Verbose("{Message}", message);
    }
    
    public static void Trace(string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Verbose(messageTemplate, propertyValues);
    }

    public static void Trace(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Verbose(exception, messageTemplate, propertyValues);
    }

    public static void Debug(string message)
    {
        EnsureInitialized();
        global::Serilog.Log.Debug("{Message}", message);
    }
    
    public static void Debug(string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Debug(messageTemplate, propertyValues);
    }

    public static void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Debug(exception, messageTemplate, propertyValues);
    }

    public static void Info(string message)
    {
        EnsureInitialized();
        global::Serilog.Log.Information("{Message}", message);
    }
    
    public static void Info(string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Information(messageTemplate, propertyValues);
    }

    public static void Info(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Information(exception, messageTemplate, propertyValues);
    }

    public static void Warn(string message)
    {
        EnsureInitialized();
        global::Serilog.Log.Warning("{Message}", message);
    }
    
    public static void Warn(string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Warning(messageTemplate, propertyValues);
    }

    public static void Warn(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Warning(exception, messageTemplate, propertyValues);
    }

    public static void Error(string message)
    {
        EnsureInitialized();
        global::Serilog.Log.Error("{Message}", message);
        BreakIfConfigured();
    }
    
    public static void Error(string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Error(messageTemplate, propertyValues);
        BreakIfConfigured();
    }

    public static void Error(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        EnsureInitialized();
        global::Serilog.Log.Error(exception, messageTemplate, propertyValues);
        BreakIfConfigured();
    }

    [Conditional("DEBUG")]
    private static void BreakIfConfigured()
    {
        if (BreakOnLoggedErrors && Debugger.IsAttached)
            Debugger.Break();
    }

    private static void EnsureInitialized()
    {
        if (initialized)
            return;

        lock (InitLock)
        {
            if (initialized)
                return;

            var outputTemplate = GetOutputTemplate(timeSetting);
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .Enrich.With(new LevelTextEnricher());

            if (timeSetting == TimeSetting.Relative)
                loggerConfig = loggerConfig.Enrich.With(new UptimeEnricher());

            loggerConfig = loggerConfig.WriteTo.Logger(lc => lc
                .MinimumLevel.ControlledBy(ConsoleLevelSwitch)
                .WriteTo.Console(outputTemplate: outputTemplate));

            if (FileLoggingEnabled)
            {
                LogFilePath = BuildLogFilePath(StartNewFileOnStartup);
                var latestLogFilePath = BuildLatestLogFilePath();
                ResetLatestLogFile(latestLogFilePath);
                loggerConfig = loggerConfig.WriteTo.File(
                    path: LogFilePath,
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(2),
                    outputTemplate: outputTemplate,
                    levelSwitch: FileLevelSwitch);
                loggerConfig = loggerConfig.WriteTo.File(
                    path: latestLogFilePath,
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromMilliseconds(250),
                    outputTemplate: outputTemplate,
                    levelSwitch: FileLevelSwitch);
            }
            else
            {
                LogFilePath = null;
            }

            global::Serilog.Log.Logger = loggerConfig.CreateLogger();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => global::Serilog.Log.CloseAndFlush();

            initialized = true;
        }
    }

    private static string BuildLogFilePath(bool startNewFileOnStartup)
    {
        var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(logsDirectory);

        var fileName = startNewFileOnStartup
            ? $"codynet-{DateTime.Now:yyyyMMdd-HHmmss}.log"
            : $"codynet-{DateTime.Now:yyyyMMdd}.log";

        return Path.Combine(logsDirectory, fileName);
    }

    private static string BuildLatestLogFilePath()
    {
        var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(logsDirectory);
        return Path.Combine(logsDirectory, "latest.log");
    }

    private static void ResetLatestLogFile(string latestLogFilePath)
    {
        try
        {
            File.WriteAllText(latestLogFilePath, string.Empty);
        }
        catch (IOException)
        {
            // Keep going if another process temporarily holds the file.
        }
    }

    private static string GetOutputTemplate(TimeSetting setting) => setting switch
    {
        TimeSetting.Absolute => "[{Timestamp:HH:mm:ss}][{LevelText,-5}] {Message:lj}{NewLine}{Exception}",
        TimeSetting.AbsoluteWithDate => "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}][{LevelText,-5}] {Message:lj}{NewLine}{Exception}",
        TimeSetting.Relative => "[{Uptime}][{LevelText,-5}] {Message:lj}{NewLine}{Exception}",
        _ => "[{Timestamp:HH:mm:ss}][{LevelText,-5}] {Message:lj}{NewLine}{Exception}"
    };

    private static TimeSetting? ParseTimeSetting(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<TimeSetting>(value, ignoreCase: true, out var parsed))
            return parsed;

        return value.Trim().ToLowerInvariant() switch
        {
            "withdate" => TimeSetting.AbsoluteWithDate,
            "absolute_with_date" => TimeSetting.AbsoluteWithDate,
            _ => null
        };
    }

    private static LogEventLevel MapLevel(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Warn => LogEventLevel.Warning,
        LogLevel.Info => LogEventLevel.Information,
        LogLevel.Debug => LogEventLevel.Debug,
        LogLevel.Trace => LogEventLevel.Verbose,
        _ => LogEventLevel.Information
    };

    private static bool IsTrue(string? value) =>
        value is not null &&
        (value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("on", StringComparison.OrdinalIgnoreCase));

    private static bool IsFalse(string? value) =>
        value is not null &&
        (value.Equals("0", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("off", StringComparison.OrdinalIgnoreCase));

    private sealed class UptimeEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var elapsed = DateTimeOffset.Now - ProcessStartTime;
            var formatted = $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}";
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Uptime", formatted));
        }
    }

    private sealed class LevelTextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var text = logEvent.Level switch
            {
                LogEventLevel.Verbose => "TRACE",
                LogEventLevel.Debug => "DEBUG",
                LogEventLevel.Information => "INFO",
                LogEventLevel.Warning => "WARN",
                LogEventLevel.Error => "ERROR",
                LogEventLevel.Fatal => "FATAL",
                _ => "INFO"
            };
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LevelText", text));
        }
    }
}
