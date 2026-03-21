using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Core.Devices;
using CodyNET.Disassembler;
using CodyNET.Frontend.Controls;
using MsBox.Avalonia;
using Math = System.Math;

namespace CodyNET.Frontend;

public partial class MainWindow : Window
{
    private static readonly TimeSpan RegisterRefreshInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan FooterRefreshInterval = TimeSpan.FromMilliseconds(500);

    private ScreenControl? screen;
    private DispatcherTimer? registerRefreshTimer;
    private DispatcherTimer? footerRefreshTimer;
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeScreen();
        InitializePanels();
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        Opened += OnOpened;
        
        Closed += OnWindowClosed; // Close whole application on window close

        InitUi();
    }

    private void InitUi()
    {
        // TODO: Get version
        FooterModeText.Text = $"CodyNET - Version 1";
    }
    
    private void OnOpened(object? sender, EventArgs e)
    {
        screen?.Focus();
        RefreshRegisterValues();
        RefreshBreakpointsPanel();
        registerRefreshTimer?.Start();
        footerRefreshTimer?.Start();
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

    private void InitializePanels()
    {
        registerRefreshTimer = new DispatcherTimer
        {
            Interval = RegisterRefreshInterval
        };
        footerRefreshTimer = new DispatcherTimer
        {
            Interval = FooterRefreshInterval
        };
        registerRefreshTimer.Tick += (_, _) => RefreshRegisterValues();
        footerRefreshTimer.Tick += (_, _) => RefreshFooter();
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

    private void RefreshBreakpointsPanel()
    {
        // TEMP DUMMY
        if (FrontendHostBridge.Debugger != null && !FrontendHostBridge.Debugger.HasBreakpoints())
        {
            FrontendHostBridge.Debugger?.AddBreakpoint(0xC000, true, "LDA #1337");
            FrontendHostBridge.Debugger?.AddBreakpoint(0xE000, false, "LDA #1234567");
            FrontendHostBridge.Debugger?.AddBreakpoint(0xCAFE, false, "LDA #1357");
            FrontendHostBridge.Debugger?.AddBreakpoint(0xC002, true, "STA #1337");
            FrontendHostBridge.Debugger?.AddBreakpoint(0xC003, true, "STA #1337");
            FrontendHostBridge.Debugger?.AddBreakpoint(0xC004, true, "STA #1337");
            FrontendHostBridge.Debugger?.AddBreakpoint(0xC005, true, "STA #1337");
            FrontendHostBridge.Debugger?.AddBreakpoint(0xC006, true, "STA #1337");
        }
        
        var breakpointsPanel = BreakpointsPanel;
        breakpointsPanel.Children.Clear();

        AddBreakpointHeader();

        var debugger = FrontendHostBridge.Debugger;
        if (debugger == null)
        {
            AddBreakpointPlaceholder("Debugger not available.");
            return;
        }

        var breakpoints = debugger.GetBreakpointsSnapshot()
            .OrderBy(bp => bp.Address)
            .ToList();

        if (breakpoints.Count == 0)
        {
            AddBreakpointPlaceholder("No breakpoints configured.");
            return;
        }

        for (var i = 0; i < breakpoints.Count; i++)
        {
            AddBreakpointRow(breakpoints[i]);
        }
    }

    private void RefreshFooter()
    {
        var status = FrontendHostBridge.GetStatusSnapshot();
        if (status == null)
            return;


        switch (status.RunStatus)
        {
            case RunStatus.Running:
                FooterStatusText.Text = "Running";
                FooterStatusText.Foreground = Brush.Parse("#31c436");
                break;
            case RunStatus.Paused:
                FooterStatusText.Text = "Paused";
                FooterStatusText.Foreground = Brush.Parse("#ff2414");
                break;
        }
        
        if (status.ProfilerSnapshot == null)
            return;
        var actualFrequencyText = Unit.FormatSi(status.ProfilerSnapshot.ActualFrequency, "Hz");
        var targetFrequencyText = Unit.FormatSi(status.ProfilerSnapshot.TargetFrequency, "Hz");
        var rightText = $"Speed: {actualFrequencyText}";
        if (status.ProfilerSnapshot.TargetFrequency > 0)
        {
            rightText += $" / {targetFrequencyText} ({Math.Round(status.ProfilerSnapshot.FrequencyTargetPercent)}%)";
        }
        
        SetText("FooterRightText", rightText);
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
                FileTypeFilter =
                [
                    new FilePickerFileType("Binary and BASIC files")
                    {
                        Patterns = ["*.bin", "*.bas"]
                    },
                    new FilePickerFileType("BASIC files")
                    {
                        Patterns = ["*.bas"]
                    },
                    new FilePickerFileType("Binary files")
                    {
                        Patterns = ["*.bin"]
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = ["*.*"]
                    },
                ]
            });
        
        if (files.Count == 0)
            return;

        var path = files[0].TryGetLocalPath();
        if (path == null)
            return;

        var fileInfo = new FileInfo(path);
        UpdateCodePanel(fileInfo);
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

        if (!long.TryParse(tagValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var frequencyHz))
            return;

        FrontendHostBridge.SetClockFrequency(frequencyHz);
    }    
    
    private void OnPauseResumeButtonClick(object? sender, RoutedEventArgs e)
    {
        // If text is "_Pause [F9]", change it to "_Resume [F9]"
        if (MenuPauseResumeButton.Header is not string headerText) return;
        if (headerText.StartsWith("_Pause")) // Clicked on Pause
        {
            MenuPauseResumeButton.Header = headerText.Replace("_Pause", "_Resume");
            MenuStepButton.IsEnabled = true;
            FrontendHostBridge.SetRunState(0);
        }
        else if (headerText.StartsWith("_Resume")) // Clicked on Resume
        {
            MenuPauseResumeButton.Header = headerText.Replace("_Resume", "_Pause");
            MenuStepButton.IsEnabled = false;
            FrontendHostBridge.SetRunState(-1);
        }
    }
    
    private void OnStepButtonClick(object? sender, RoutedEventArgs e)
    {
        if (!MenuStepButton.IsEnabled)
            return;
        FrontendHostBridge.SetRunState(1);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            switch (e.Key) // Handle special debugger buttons (F8, F9)
            {
                // Pause/Resume
                case Key.F9:
                    OnPauseResumeButtonClick(sender, e);
                    return;
                // Step
                case Key.F8:
                    OnStepButtonClick(sender, e);
                    break;
            }


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
        
    }

    private void AddBreakpointHeader()
    {
        var headerRow = CreateBreakpointRowGrid();
        headerRow.Children.Add(CreateBreakpointCell(new TextBlock
        {
            Text = string.Empty,
            VerticalAlignment = VerticalAlignment.Center,
        }, 0));

        headerRow.Children.Add(CreateBreakpointCell(new TextBlock
        {
            Text = "Adresse",
            VerticalAlignment = VerticalAlignment.Center,
        }, 1, "breakpoint-column-header"));

        headerRow.Children.Add(CreateBreakpointCell(new TextBlock
        {
            Text = "Code",
            VerticalAlignment = VerticalAlignment.Center,
        }, 2, "breakpoint-column-header"));

        BreakpointsPanel.Children.Add(new Border
        {
            Child = headerRow,
            Classes = { "breakpoint-header-row" }
        });
    }

    private void AddBreakpointPlaceholder(string text)
    {
        var placeholder = new TextBlock
        {
            Text = text,
            Foreground = Brush.Parse("#8191B0"),
            VerticalAlignment = VerticalAlignment.Center,
        };
        placeholder.Classes.Add("code-text");
        BreakpointsPanel.Children.Add(new Border
        {
            Child = placeholder,
            Classes = { "breakpoint-row" }
        });
    }

    private void AddBreakpointRow(Breakpoint breakpoint)
    {
        var row = CreateBreakpointRowGrid();

        var enabledToggle = new CheckBox
        {
            IsChecked = breakpoint.Enabled,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Tag = breakpoint.Address,
        };
        enabledToggle.IsCheckedChanged += OnBreakpointCheckedChanged;
        row.Children.Add(CreateBreakpointCell(enabledToggle, 0));

        row.Children.Add(CreateBreakpointCell(new TextBlock
        {
            Text = $"PC == ${breakpoint.Address:X4}",
            Foreground = Brush.Parse(breakpoint.Enabled ? "#00FF8E" : "#8191B0"),
            VerticalAlignment = VerticalAlignment.Center,
        }, 1, "code-text"));

        row.Children.Add(CreateBreakpointCell(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(breakpoint.Text) ? "-" : breakpoint.Text,
            Foreground = Brush.Parse(breakpoint.Enabled ? "#00FF8E" : "#8191B0"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        }, 2, "code-text"));

        var deleteButton = new Button
        {
            Content = "×",
            Tag = breakpoint.Address,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        deleteButton.Classes.Add("breakpoint-delete");
        deleteButton.Click += OnBreakpointDeleteClick;
        row.Children.Add(CreateBreakpointCell(deleteButton, 3));
        BreakpointsPanel.Children.Add(new Border
        {
            Child = row,
            Classes = { "breakpoint-row" }
        });
    }

    private Grid CreateBreakpointRowGrid()
    {
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("60,*,*,32"),
            ColumnSpacing = 18,
        };
    }

    private Control CreateBreakpointCell(Control control, int column, params string[] classes)
    {
        Grid.SetColumn(control, column);
        foreach (var className in classes)
        {
            control.Classes.Add(className);
        }

        return control;
    }

    private void OnBreakpointCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.Tag is not ushort address)
            return;

        if (FrontendHostBridge.Debugger?.SetBreakpointEnabled(address, checkBox.IsChecked == true) != true)
            return;
        RefreshBreakpointsPanel();
    }

    private void OnBreakpointDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not ushort address)
            return;

        FrontendHostBridge.Debugger?.RemoveBreakpoint(address);
        RefreshBreakpointsPanel();
    }

    private void UpdateCodePanel(FileInfo fileInfo)
    {
        var codeLinesPanel = this.FindControl<StackPanel>("CodeLinesPanel");
        if (codeLinesPanel == null)
        {
            return;
        }
        codeLinesPanel.Children.Clear();

        string[] lines;
        string disassembled;
        try
        {
            lines = File.ReadAllLines(fileInfo.FullName);
            disassembled = CodyDisassembler.Disassemble(File.ReadAllBytes(fileInfo.FullName));
            lines = disassembled.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to read source file {File}", fileInfo.FullName);
            MessageBox("Load Error", $"Could not read source file:\n{fileInfo.FullName}");
            return;
        }

        if (lines.Length == 0)
        {
            var emptyText = new TextBlock
            {
                Text = "File is empty.",
                Foreground = Brush.Parse("#8191B0")
            };
            emptyText.Classes.Add("code-text");
            codeLinesPanel.Children.Add(emptyText);
            return;
        }

        for (int index = 0; index < lines.Length; index++)
        {
            codeLinesPanel.Children.Add(CreateCodeLine(index + 1, lines[index]));
        }
    }

    private static DockPanel CreateCodeLine(int lineNumber, string lineText)
    {
        var row = new DockPanel
        {
            LastChildFill = true
        };

        var lineNumberBlock = new TextBlock
        {
            Width = 42,
            Text = lineNumber.ToString(CultureInfo.InvariantCulture),
            Foreground = Brush.Parse("#8191B0"),
            VerticalAlignment = VerticalAlignment.Top
        };
        lineNumberBlock.Classes.Add("code-text");

        var lineTextBlock = new TextBlock
        {
            Text = string.IsNullOrEmpty(lineText) ? " " : lineText,
            Foreground = Brush.Parse("#E7EAF2"),
            TextWrapping = TextWrapping.NoWrap,
            VerticalAlignment = VerticalAlignment.Top
        };
        lineTextBlock.Classes.Add("code-text");

        row.Children.Add(lineNumberBlock);
        row.Children.Add(lineTextBlock);

        return row;
    }
}
