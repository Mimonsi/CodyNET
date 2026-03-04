using Avalonia;
using CodyNET.Common.Utils;
using CodyNET.Common.Video;
using CodyNET.Core.Cody;
using CodyNET.Core.Devices;
using CodyNET.Core.Interfaces;
using CodyNET.Frontend;

namespace CodyNET.Host;

public static class CodyFactory
{
    private static readonly TimeSpan ScreenStartupTimeout = TimeSpan.FromSeconds(10);

    public static Cody CreateCody(CodySetupOptions options)
    {
        IScreenDevice? screen = null;
        IVideoDevice? video = null;
        if (options.EnableScreen)
        {
            screen = CreateScreen();
            video = new VideoDevice();
        }

        var keyboard = CreateKeyboard();

        Cody cody = new(options, video, screen, keyboard);
        return cody;
    }

    private static IScreenDevice? CreateScreen()
    {
        Log.Debug("Creating Avalonia screen device...");
        ScreenHostBridge.Reset();
        

        var uiThread = new Thread(() =>
        {
            try
            {
                CodyNET.Frontend.Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime([]);
            }
            catch (Exception ex)
            {
                ScreenHostBridge.SetInitializationError(ex);
            }
        })
        {
            Name = "CodyNET - Cody Computer Emulator",
            IsBackground = true,
        };
        
        uiThread.Start();

        if (!ScreenHostBridge.ScreenTask.Wait(ScreenStartupTimeout))
        {
            throw new TimeoutException($"Avalonia screen was not initialized within {ScreenStartupTimeout.TotalSeconds:0} seconds.");
        }

        Log.Info("Avalonia screen device created successfully.");
        return ScreenHostBridge.ScreenTask.GetAwaiter().GetResult();
    }

    private static Keyboard? CreateKeyboard()
    {
        var keyboard = new Keyboard();
        ScreenHostBridge.SetKeyboard(keyboard);
        return keyboard;
    }
}