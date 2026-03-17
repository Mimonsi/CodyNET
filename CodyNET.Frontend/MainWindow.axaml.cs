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
    private static readonly TimeSpan RegisterRefreshInterval = TimeSpan.FromMilliseconds(100);

    private ScreenControl? screen;
    private DispatcherTimer? registerRefreshTimer;
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeScreen();
        InitializeRegisterPanel();
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        Opened += OnOpened;
        
        Closed += OnWindowClosed; // Close whole application on window close
    }
    
    private void OnOpened(object? sender, EventArgs e)
    {
        screen?.Focus();
        RefreshRegisterValues();
        registerRefreshTimer?.Start();
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        registerRefreshTimer?.Stop();
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

    private void InitializeRegisterPanel()
    {
        registerRefreshTimer = new DispatcherTimer
        {
            Interval = RegisterRefreshInterval
        };
        registerRefreshTimer.Tick += (_, _) => RefreshRegisterValues();
    }

    private void RefreshRegisterValues()
    {
        var snapshot = FrontendHostBridge.GetRegisterSnapshot();
        if (snapshot == null)
        {
            return;
        }

        SetText("RegisterAHexText", snapshot.A.ToString("X2"));
        SetText("RegisterADecText", snapshot.A.ToString(CultureInfo.InvariantCulture));
        SetText("RegisterXHexText", snapshot.X.ToString("X2"));
        SetText("RegisterXDecText", snapshot.X.ToString(CultureInfo.InvariantCulture));
        SetText("RegisterYHexText", snapshot.Y.ToString("X2"));
        SetText("RegisterYDecText", snapshot.Y.ToString(CultureInfo.InvariantCulture));
        SetText("RegisterSPHexText", snapshot.S.ToString("X2"));
        SetText("RegisterSPDecText", snapshot.S.ToString(CultureInfo.InvariantCulture));
        SetText("RegisterPCHexText", snapshot.PC.ToString("X4"));
        SetText("RegisterPCDecText", snapshot.PC.ToString(CultureInfo.InvariantCulture));
        SetText("RegisterPHexText", snapshot.P.ToString("X2"));
        SetText("RegisterPDecText", snapshot.P.ToString(CultureInfo.InvariantCulture));

        SetFlagText("FlagCarryText", snapshot.Carry);
        SetFlagText("FlagZeroText", snapshot.Zero);
        SetFlagText("FlagInterruptDisableText", snapshot.InterruptDisable);
        SetFlagText("FlagDecimalText", snapshot.Decimal);
        SetFlagText("FlagBreakText", snapshot.Break);
        SetFlagText("FlagOverflowText", snapshot.Overflow);
        SetFlagText("FlagNegativeText", snapshot.Negative);
    }

    private void SetFlagText(string controlName, bool value)
    {
        SetText(controlName, value ? "1" : "0");
    }

    private void SetText(string controlName, string text)
    {
        var textBlock = this.FindControl<TextBlock>(controlName);
        if (textBlock != null)
        {
            textBlock.Text = text;
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
