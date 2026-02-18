using System;
using Avalonia;
using CodyNET.Common.Utils;

namespace CodyNET.Frontend;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Log.Initialize();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
