using System;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CodyNET.Common.Utils;
using CodyNET.Common.Video;
using CodyNET.Frontend.Controls;

namespace CodyNET.Frontend;

public partial class MainWindow : Window
{
    private ScreenControl? screen;
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeScreen();
        KeyDown += OnKeyDown;
        Closed += OnWindowClosed; // Close whole application on window close
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        Log.Info("Main window closed. Exiting application.");
        Environment.Exit(0);
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
    
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        Log.Verbose("Key down: {Key} (Modifiers: {Modifiers})", e.Key, e.KeyModifiers);
        var keyboard = ScreenHostBridge.Keyboard;
        if (keyboard == null)
            return;

        bool shift = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        bool ctrl = (e.KeyModifiers & KeyModifiers.Control) != 0;
        bool alt = (e.KeyModifiers & KeyModifiers.Alt) != 0;
        if (keyboard.KeyPressed(e.Key.ToString(), ctrl, shift, alt))
        {
            e.Handled = true;
        }
    }
}
