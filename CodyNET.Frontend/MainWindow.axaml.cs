using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CodyNET.Common.Utils;
using CodyNET.Common.Video;
using CodyNET.Core.Devices;
using CodyNET.Frontend.Controls;
using MsBox.Avalonia;

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
            FrontendHostBridge.SetScreen(screen);
        }
    }

    private void OnScaleMenuClick(object? sender, RoutedEventArgs e)
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
    
    private async void OnLoadUart1Click(object? sender, RoutedEventArgs e)
    {
        if (screen == null || sender is not MenuItem menuItem)
        {
            return;
        }
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Load UART1 Source",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("BASIC files")
                    {
                        Patterns = new[] { "*.bas" }
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
        });
        
        if (files.Count == 0)
            return;

        var path = files[0].TryGetLocalPath();
        if (path == null)
            return;

        var fileInfo = new FileInfo(path);
        FrontendHostBridge.LoadUartSource(fileInfo);
    }
    
    private void MessageBox(string title, string message)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsPopupAsync(this);
        });
    }
    
    private void OnClockMenuClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
            return;

        if (menuItem.Tag is not string tagValue)
            return;

        if (!long.TryParse(tagValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out long frequencyHz))
            return;

        FrontendHostBridge.SetClockFrequency(frequencyHz);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            var keyboard = FrontendHostBridge.Keyboard;
            if (keyboard == null)
                return;

            //Log.Verbose("Key down: {Key}, Symbol: {Symbol}, Ctrl: {Ctrl}, Shift: {Shift}, Alt: {Alt}", e.Key, e.KeySymbol, ctrl, shift, alt);

            if (keyboard.KeyDown(e.Key.ToString(), e.KeySymbol))
                e.Handled = true;
        }
        catch (Exception x)
        {
            Log.Error(x.ToString());
        }
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        var keyboard = FrontendHostBridge.Keyboard;
        if (keyboard == null)
            return;

        if (keyboard.KeyUp(e.Key.ToString(), e.KeySymbol))
            e.Handled = true;
    }

    private void OnToggleDebuggerClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
