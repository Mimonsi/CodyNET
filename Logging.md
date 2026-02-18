# Logging

This project uses a minimal Serilog-based logger via `CodyNET.Common.Utils.Log`.

## Default behavior

- Logs to console.
- Logs to a file under `./logs/`.
- Prints the active log file path on application startup.

## Time modes

`Log.TimeSetting` supports:

- `TimeSetting.Absolute` -> `HH:mm:ss`
- `TimeSetting.AbsoluteWithDate` -> `yyyy-MM-dd HH:mm:ss.fff zzz`
- `TimeSetting.Relative` -> elapsed time since process start (`hh:mm:ss.fff`)

You can also set this through:

- `CODYNET_LOG_TIME_SETTING=Absolute`
- `CODYNET_LOG_TIME_SETTING=AbsoluteWithDate`
- `CODYNET_LOG_TIME_SETTING=Relative`

## Startup options

Set these before the first log call (or before `Log.Initialize()`):

- Start a new file each app start:
  - Code: `Log.StartNewFileOnStartup = true;`
  - Env: `CODYNET_LOG_NEW_FILE_ON_START=1`
- Print active file path at startup (enabled by default):
  - Code: `Log.PrintLogPathOnStartup = true;`
  - Env: `CODYNET_LOG_PRINT_PATH_ON_START=1`
- Disable file logging:
  - Code: `Log.FileLoggingEnabled = false;`
  - Env: `CODYNET_LOG_DISABLE_FILE=1`

## API

```csharp
Log.Initialize(); // optional, called at app startup in Program.cs

Log.Verbose("verbose message");
Log.Debug("debug message");
Log.Info("info message");
Log.Warn("warning message");
Log.Error("error message");
```

Set level with:

```csharp
Log.Level = LogLevel.Debug;
```
