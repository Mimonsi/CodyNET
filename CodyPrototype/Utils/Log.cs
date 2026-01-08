using System;

namespace CodyPrototype.Utils;

public enum TimeSetting
{
    Absolute,
    AbsoluteWithDate,
    Relative,
}

public enum LogLevel
{
    Error,
    Warn,
    Info,
    Debug,
    Trace
}
public class Log
{
    public static TimeSetting TimeSetting = TimeSetting.Relative;
    public static LogLevel Level = LogLevel.Debug;
    
    public static void Trace(string message)
    {
        if (Level < LogLevel.Trace)
            return;
        Write($"[TRACE] {message}");
    }
    public static void Debug(string message)
    {
        if (Level < LogLevel.Debug)
            return;
        Write($"[DEBUG] {message}");
    }
    
    public static void Info(string message)
    {
        if (Level < LogLevel.Info)
            return;
        Write($"[INFO] {message}");
    }

    public static void Warn(string message)
    {
        if (Level < LogLevel.Warn)
            return;
        Write($"[WARN] {message}");
    }

    public static void Error(string message)
    {
        if (Level < LogLevel.Error)
            return;
        Write($"[ERROR] {message}");
    }

    private static void Write(string text)
    {
        var timeText = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss}";
        if (TimeSetting == TimeSetting.Absolute)
            timeText = $"{DateTime.Now:HH:mm:ss}";
        if (TimeSetting == TimeSetting.Relative)
            timeText = $"{DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime:hh\\:mm\\:ss\\:ffff}";
        text = $"[{timeText}] {text}";
        Console.WriteLine(text);
        ToFile(text);
    }

    private static void ToFile(string message)
    {
        var path = "cody_log.txt";
        using var writer = File.AppendText(path);
        writer.WriteLine(message);
    }
}