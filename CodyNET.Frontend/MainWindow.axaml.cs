using System;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Threading;
using CodyNET.Common.Video;
using CodyNET.Frontend.Controls;

namespace CodyNET.Frontend;

public partial class MainWindow : Window
{
    private ScreenControl? screen;
    private DispatcherTimer? frameTimer;
    private readonly string ppmPath = @"C:\Users\Konsi\Desktop\screen.ppm";
    private DateTime lastWriteUtc = DateTime.MinValue;
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeScreen();
        StartPpmPolling(1);
    }
    
    private void InitializeScreen()
    {
        screen = this.FindControl<ScreenControl>("Screen");
        if (screen != null)
        {
            screen.ScaleFactor = 4.0;
            ScreenHostBridge.SetScreen(screen);
        }
    }

    private void OnScaleMenuClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (screen == null || sender is not MenuItem menuItem)
        {
            return;
        }

        if (menuItem.Tag is string tagValue
            && double.TryParse(tagValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double scale))
        {
            screen.ScaleFactor = scale;
        }
    }

    private void StartPpmPolling(int fps)
    {
        if (fps <= 0)
            fps = 60;

        frameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / fps)
        };

        frameTimer.Tick += (_, _) =>
        {
            var targetScreen = screen;
            if (targetScreen is null || !File.Exists(ppmPath))
                return;

            DateTime writeTimeUtc;
            try
            {
                writeTimeUtc = File.GetLastWriteTimeUtc(ppmPath);
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            if (writeTimeUtc <= lastWriteUtc)
                return;

            try
            {
                var frame = PpmCodec.Load(ppmPath);
                targetScreen.RenderFrame(frame);
                lastWriteUtc = writeTimeUtc;
            }
            catch (IOException)
            {
                // File is likely being written right now; retry next tick.
            }
            catch (UnauthorizedAccessException)
            {
                // Retry next tick.
            }
            catch (InvalidDataException)
            {
                // Invalid/incomplete PPM; retry next tick.
            }
        };

        frameTimer.Start();
    }
}
