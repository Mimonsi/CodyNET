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
        return CreateCody(options, launchScreenHost: true);
    }

    public static void PrepareScreenHost()
    {
        FrontendHostBridge.Reset();
    }

    public static Cody CreateCodyForHostedScreen(CodySetupOptions options)
    {
        return CreateCody(options, launchScreenHost: false);
    }

    private static Cody CreateCody(CodySetupOptions options, bool launchScreenHost)
    {
        IScreenDevice? screen = null;
        IVideoDevice? video = null;
        if (options.EnableScreen)
        {
            screen = launchScreenHost ? CreateScreen() : WaitForScreen();
            video = new VideoDevice();
        }

        Cody cody = new(options, video, screen);
        RegisterBindings(cody);
        if (options.EnableScreen && cody.Keyboard != null)
        {
            FrontendHostBridge.SetKeyboard(cody.Keyboard);
        }
        return cody;
    }

    private static void RegisterBindings(Cody cody)
    {
        FrontendHostBridge.RegisterClockFrequencySetter(frequencyHz => cody.FrequencyHz = frequencyHz);
        FrontendHostBridge.RegisterUart1SourceLoader(fileInfo =>
        {
            cody.LoadUartSource(fileInfo);
        });
        FrontendHostBridge.RegisterRegisterSnapshotProvider(cody.Cpu.GetRegisterSnapshot);
        FrontendHostBridge.RegisterStatusSnapshotProvider(cody.GetStatusSnapshot);
    }

    private static IScreenDevice? CreateScreen()
    {
        Log.Debug("Creating Avalonia screen device...");
        FrontendHostBridge.Reset();

        var uiThread = new Thread(() =>
        {
            try
            {
                CodyNET.Frontend.Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime([]);
            }
            catch (Exception ex)
            {
                FrontendHostBridge.SetInitializationError(ex);
            }
        })
        {
            Name = "CodyNET - Cody Computer Emulator",
            IsBackground = true,
        };

        uiThread.Start();

        return WaitForScreen();
    }

    private static IScreenDevice WaitForScreen()
    {
        if (!FrontendHostBridge.ScreenTask.Wait(ScreenStartupTimeout))
        {
            throw new TimeoutException($"Avalonia screen was not initialized within {ScreenStartupTimeout.TotalSeconds:0} seconds.");
        }

        Log.Info("Avalonia screen device created successfully.");
        return FrontendHostBridge.ScreenTask.GetAwaiter().GetResult();
    }
}
