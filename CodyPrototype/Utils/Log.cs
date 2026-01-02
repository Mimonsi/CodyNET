using System;

namespace CodyPrototype.Utils;

public class Log
{
    public static void Info(string message)
    {
        var text = $"[INFO] {message}";
        Console.WriteLine(text);
        ToFile(text);
    }

    public static void Warn(string message)
    {
        var text = $"[WARN] {message}";
        Console.WriteLine(text);
        ToFile(text);
    }

    public static void Error(string message)
    {
        var text = $"[ERROR] {message}";
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