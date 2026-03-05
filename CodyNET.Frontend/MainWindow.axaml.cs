using System;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CodyNET.Common.Utils;
using CodyNET.Common.Video;
using CodyNET.Core.Devices;
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
        KeyUp += OnKeyUp;
        Opened += OnOpened;
        
        Closed += OnWindowClosed; // Close whole application on window close
    }
    
    private void OnOpened(object? sender, EventArgs e)
    {
        screen?.Focus();
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
        // KeyDown weiter benutzen für Nicht-Text-Tasten:
        // Enter, Backspace, Pfeile, F-Tasten, etc. und Ctrl-shortcuts
        var keyboard = ScreenHostBridge.Keyboard;
        if (keyboard == null)
            return;

        bool shift = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        bool ctrl  = (e.KeyModifiers & KeyModifiers.Control) != 0;
        bool alt   = (e.KeyModifiers & KeyModifiers.Alt) != 0;
        
        Log.Verbose("Key down: {Key} (Modifiers: {Modifiers})", e.Key, e.KeyModifiers);
        var translatedKey = Keyboard.TranslateLogicalKeyDE(e.Key.ToString(), ctrl, shift, alt);
        Log.Verbose("Translated Key: " + translatedKey, ctrl, shift, alt);

        if (keyboard.KeyDown(translatedKey, ctrl, shift, alt))
            e.Handled = true;
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        var keyboard = ScreenHostBridge.Keyboard;
        if (keyboard == null)
            return;

        bool shift = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        bool ctrl  = (e.KeyModifiers & KeyModifiers.Control) != 0;
        bool alt   = (e.KeyModifiers & KeyModifiers.Alt) != 0;
        
        Log.Verbose("Key up: {Key} (Modifiers: {Modifiers})", e.Key, e.KeyModifiers);
        var translatedKey = Keyboard.TranslateLogicalKeyDE(e.Key.ToString(), ctrl, shift, alt);
        Log.Verbose("Translated Key: " + translatedKey, ctrl, shift, alt);

        if (keyboard.KeyUp(translatedKey, ctrl, shift, alt))
            e.Handled = true;
    }
}
