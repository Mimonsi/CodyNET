using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Path = System.IO.Path;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using CodyNET.Assembler;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Core.Devices;
using CodyNET.Frontend.Controls;
using MsBox.Avalonia;
using Math = System.Math;

namespace CodyNET.Frontend;

public partial class MainWindow : Window
{
    private static readonly TimeSpan RegisterRefreshInterval = TimeSpan.FromMilliseconds(100);

    private static readonly TimeSpan FooterRefreshInterval = TimeSpan.FromMilliseconds(250);

    private ScreenControl? screen;
    private DispatcherTimer? registerRefreshTimer;
    private DispatcherTimer? footerRefreshTimer;
    private string? _loadedAssemblyPath;
    private static readonly long[] ClockStepFrequencies = [100_000, 500_000, 1_000_000, 2_000_000, 5_000_000, 10_000_000, -1];
    private static readonly string[] ClockStepLabels    = ["100 kHz", "500 kHz", "1 MHz", "2 MHz", "5 MHz", "10 MHz", "∞"];

    private bool _isAssemblyDirty;
    private bool _suppressCodeEditorEvents;
    private bool _suppressClockSliderEvents;
    private bool _debugUiActive;
    private int _lastClockComboIndex = 2;
    private BreakpointMargin? _breakpointMargin;
    private CurrentLineRenderer? _currentLineRenderer;
    private readonly Dictionary<int, bool> _codeEditorBreakpointLines = new();
    private Dictionary<ushort, int> _addressToSourceLine = new();
    private List<string> _codeLines = [];
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeScreen();
        InitializePanels();
        InitializeCodeEditor();
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        Opened += OnOpened;

        Closed += OnWindowClosed; // Close whole application on window close

        InitUi();
    }

    private void InitializeCodeEditor()
    {
        _breakpointMargin = new BreakpointMargin(_codeEditorBreakpointLines, ToggleBreakpoint);
        CodeEditorTextBox.TextArea.LeftMargins.Insert(0, _breakpointMargin);

        CodeEditorTextBox.Document.TextChanged += OnCodeEditorDocumentTextChanged;

        using var stream = AssetLoader.Open(new Uri("avares://CodyNET.Frontend/Assets/65C02.xshd"));
        using var xmlReader = XmlReader.Create(stream);
        var xshd = HighlightingLoader.LoadXshd(xmlReader);
        CodeEditorTextBox.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);

        _currentLineRenderer = new CurrentLineRenderer();
        CodeEditorTextBox.TextArea.TextView.BackgroundRenderers.Add(_currentLineRenderer);
    }

    private string _fullVersionText;

    private void InitUi()
    {
        _fullVersionText = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "unknown";
        var version = _fullVersionText.Contains('+') ? _fullVersionText[.._fullVersionText.IndexOf('+')] : _fullVersionText;
        FooterVersionText.Text = $"CodyNET - Version {version}";
        ToolTip.SetTip(FooterVersionText, _fullVersionText);
        SetCodePanelState(null, null);
    }
    
    private void OnOpened(object? sender, EventArgs e)
    {
        screen?.Focus();
        
        // Set buttons visible only if debug mode is enabled. Debug UI can also be disabled while debug mode is on
        var debugMode = FrontendHostBridge.DebugMode;
        DebugMenu.IsVisible = debugMode;
        LoadAssemblyMenuItem.IsVisible = debugMode;
        SetDebugUiActive(debugMode);
        RefreshRegisterValues();
        RefreshBreakpointsPanel();
        SyncInitialClockFrequency();
        registerRefreshTimer?.Start();
        footerRefreshTimer?.Start();
    }

    private void SyncInitialClockFrequency()
    {
        var hz = FrontendHostBridge.InitialClockFrequency;
        if (hz == 0)
            return;

        var step = Array.IndexOf(ClockStepFrequencies, hz);
        _lastClockComboIndex = step >= 0 ? step : ClockStepFrequencies.Length;
        _suppressClockSliderEvents = true;
        ClockSpeedComboBox.SelectedIndex = _lastClockComboIndex;
        _suppressClockSliderEvents = false;
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
        if (!_debugUiActive)
            return;
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

        UpdateCurrentLineHighlight(snapshot.PC);
    }

    private void UpdateCurrentLineHighlight(ushort pc)
    {
        if (_currentLineRenderer is null) return;

        _addressToSourceLine.TryGetValue(pc, out var sourceLine);  // 0 if not found
        var newLine = sourceLine > 0 ? sourceLine : (int?)null;

        if (_currentLineRenderer.CurrentLine == newLine) return;   // nothing changed

        _currentLineRenderer.CurrentLine = newLine;
        CodeEditorTextBox.TextArea.TextView.InvalidateVisual();

        // Auto-scroll to the highlighted line when paused/stepping (not while running,
        // because the PC jumps around too fast and would be distracting).
        if (newLine is not null)
        {
            var status = FrontendHostBridge.GetStatusSnapshot();
            if (status?.RunStatus == RunStatus.Paused)
            {
                // CodeEditorTextBox.TextArea.Caret.Line   = newLine.Value;
                // CodeEditorTextBox.TextArea.Caret.Column = 1;
                // CodeEditorTextBox.TextArea.Caret.BringCaretToView();
                
                CodeEditorTextBox.ScrollToLine(newLine.Value);
            }
        }
    }

    private void RefreshBreakpointsPanel()
    {
        var breakpointsPanel = BreakpointsPanel;
        breakpointsPanel.Children.Clear();

        AddBreakpointHeader();

        _breakpointMargin?.InvalidateVisual();

        var count = _codeEditorBreakpointLines.Count;
        BreakpointBadge.IsVisible = count > 0;
        BreakpointBadgeText.Text = count.ToString();

        if (count == 0)
        {
            AddBreakpointPlaceholder("No breakpoints configured.");
            return;
        }

        foreach (int i in _codeEditorBreakpointLines.Keys.OrderBy(x => x))
        {
            AddBreakpointRow(i);
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
                ClockActualLabel.Foreground = Brush.Parse("#31c436");
                MenuPauseResumeButton.Header = "_Pause [F9]";
                MenuStepButton.IsEnabled = false;
                EmulatorPauseResumeButton.Content = "Pause [F9]";
                EmulatorStepButton.IsEnabled = false;
                break;
            case RunStatus.Paused:
                FooterStatusText.Text = "Paused";
                FooterStatusText.Foreground = Brush.Parse("#ff2414");
                ClockActualLabel.Foreground = Brush.Parse("#ff2414");
                MenuPauseResumeButton.Header = "_Resume [F9]";
                MenuStepButton.IsEnabled = true;
                EmulatorPauseResumeButton.Content = "Resume [F9]";
                EmulatorStepButton.IsEnabled = true;
                break;
        }
        
        if (status.ProfilerSnapshot == null)
            return;
        var actualFrequencyText = Unit.FormatSi(status.ProfilerSnapshot.ActualFrequency, "Hz");
        ClockActualLabel.Text = status.ProfilerSnapshot.TargetFrequency > 0
            ? $"{actualFrequencyText} ({Math.Round(status.ProfilerSnapshot.FrequencyTargetPercent)}%)"
            : actualFrequencyText;
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
        if (screen == null || sender is not MenuItem menuItem || menuItem.Parent is not MenuItem viewMenu)
        {
            return;
        }

        // Uncheck all sibling scale items
        foreach (var child in viewMenu.Items)
        {
            if (child is MenuItem sibling && sibling.ToggleType == MenuItemToggleType.CheckBox)
                sibling.IsChecked = sibling == menuItem;
        }

        var isAuto = menuItem.Tag is string tagValue && tagValue == "auto";
        screen.FitToContainer = isAuto;
        ScreenScrollViewer.HorizontalScrollBarVisibility = isAuto
            ? Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
            : Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
        ScreenScrollViewer.VerticalScrollBarVisibility = isAuto
            ? Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
            : Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;

        if (!isAuto && menuItem.Tag is string scaleValue
            && double.TryParse(scaleValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double scale))
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
        //UpdateCodePanel(fileInfo);
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
        SyncClockSliderToFrequency(frequencyHz);
    }

    private void OnClockComboBoxChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressClockSliderEvents || ClockSpeedComboBox is null)
            return;

        var step = ClockSpeedComboBox.SelectedIndex;

        if (step < 0 || step >= ClockStepFrequencies.Length)
            return; // "Custom…" is handled by OnClockDropDownClosed

        _lastClockComboIndex = step;
        FrontendHostBridge.SetClockFrequency(ClockStepFrequencies[step]);
    }

    private void OnClockDropDownClosed(object? sender, EventArgs e)
    {
        if (ClockSpeedComboBox.SelectedIndex == ClockStepFrequencies.Length)
            _ = OpenCustomClockDialogAsync();
    }

    private async Task OpenCustomClockDialogAsync()
    {
        var dialog = new ClockFrequencyDialog();
        await dialog.ShowDialog(this);

        if (dialog.ResultHz is { } hz)
        {
            FrontendHostBridge.SetClockFrequency(hz);
            SyncClockSliderToFrequency(hz);
            // If no preset matched, keep "Custom…" selected and update the actual label
            if (Array.IndexOf(ClockStepFrequencies, hz) < 0)
                ClockActualLabel.Text = Unit.FormatSi(hz, "Hz");
        }
        else
        {
            // User cancelled — restore previous ComboBox state without touching the emulator
            _suppressClockSliderEvents = true;
            ClockSpeedComboBox.SelectedIndex = _lastClockComboIndex;
            _suppressClockSliderEvents = false;
        }
    }

    private void SyncClockSliderToFrequency(long hz)
    {
        var step = Array.IndexOf(ClockStepFrequencies, hz);
        if (step < 0)
            return;

        _suppressClockSliderEvents = true;
        ClockSpeedComboBox.SelectedIndex = step;
        _suppressClockSliderEvents = false;
    }

    private void OnPauseResumeButtonClick(object? sender, RoutedEventArgs e)
    {
        var status = FrontendHostBridge.GetStatusSnapshot();
        if (status == null) return;
        if (status.RunStatus == RunStatus.Running)
        {
            FrontendHostBridge.SetRunState(0);
        }
        else if (status.RunStatus == RunStatus.Paused)
        {
            FrontendHostBridge.SetRunState(-1);
        }
    }

    private void OnStepButtonClick(object? sender, RoutedEventArgs e)
    {
        if (!MenuStepButton.IsEnabled)
            return;
        FrontendHostBridge.SetRunState(1);
    }
    
    private List<Key> DebuggerControlKeys = new() { Key.F8, Key.F9 };

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
                    return;
            }


            if (CodeEditorTextBox.TextArea.IsFocused)
                return;

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
        if (DebuggerControlKeys.Contains(e.Key))
        {
            // Do not send key up events for debugger control keys to avoid issues with key repeat and lost key up events
            return;
        }
        if (CodeEditorTextBox.TextArea.IsFocused)
            return;
        var keyboard = FrontendHostBridge.Keyboard;
        if (keyboard == null)
            return;

        if (keyboard.KeyUp(e.Key.ToString(), e.KeySymbol))
            e.Handled = true;
    }

    private void OnToggleDebuggerClick(object? sender, RoutedEventArgs e)
    {
        bool show;
        if (sender is MenuItem menuItem)
        {
            show = menuItem.IsChecked;
            ShowDebuggerMenuItem.IsChecked = show;
        }
        else
        {
            show = ShowDebuggerMenuItem.IsChecked;
        }
        SetDebugUiActive(show);
    }

    private void SetDebugUiActive(bool enabled)
    {
        _debugUiActive = enabled;
        ShowDebuggerMenuItem.IsChecked = enabled;

        var cols = WorkspaceGrid.ColumnDefinitions;

        IgnoreBreakpointsToggle.IsVisible = enabled;
        // Column 0: Left Sidebar, 1: Splitter, 3: Splitter, 4: Code Panel
        LeftSidebar.IsVisible = enabled;
        LeftSplitter.IsVisible = enabled;
        RightSplitter.IsVisible = enabled;
        CodePanel.IsVisible = enabled;

        // Lower panel (Breakpoints) and its splitter
        LowerPanel.IsVisible = enabled;
        LowerPanelSplitter.IsVisible = enabled;

        if (enabled)
        {
            cols[0].Width = new GridLength(300);
            cols[0].MinWidth = 220;
            cols[1].Width = new GridLength(4);
            cols[3].Width = new GridLength(4);
            cols[4].Width = new GridLength(400);
            cols[4].MinWidth = 220;

            // Restore screen row split
            var rows = MainColumn.RowDefinitions;
            rows[2].Height = new GridLength(4);
            rows[3].Height = new GridLength(250);
        }
        else
        {
            cols[0].Width = new GridLength(0);
            cols[0].MinWidth = 0;
            cols[1].Width = new GridLength(0);
            cols[3].Width = new GridLength(0);
            cols[4].Width = new GridLength(0);
            cols[4].MinWidth = 0;

            // Screen takes full height (no lower panel)
            var rows = MainColumn.RowDefinitions;
            rows[2].Height = new GridLength(0);
            rows[3].Height = new GridLength(0);
        }
    }

    private void OnIgnoreBreakpointsClick(object? sender, RoutedEventArgs e)
    {
        var debugger = FrontendHostBridge.Debugger;
        if (debugger == null) return;

        // Sync state from whichever control was clicked
        bool ignore;
        if (sender is MenuItem menuItem)
        {
            ignore = menuItem.IsChecked;
            IgnoreBreakpointsToggle.IsChecked = ignore;
        }
        else
        {
            ignore = IgnoreBreakpointsToggle.IsChecked == true;
            IgnoreBreakpointsMenuItem.IsChecked = ignore;
        }

        debugger.IgnoreBreakpoints = ignore;
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
            Text = "Line",
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

    private string GetLineText(int line)
    {
        if (line - 1 >= _codeLines.Count)
            return "ERROR";
        return _codeLines[line-1];
    }

    private void AddBreakpointRow(int line)
    {
        var row = CreateBreakpointRowGrid();
        var enabled = _codeEditorBreakpointLines[line];

        var enabledToggle = new CheckBox
        {
            IsChecked = enabled,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Tag = line,
        };
        enabledToggle.IsCheckedChanged += OnBreakpointCheckedChanged;
        row.Children.Add(CreateBreakpointCell(enabledToggle, 0));

        var lineNumberText = new TextBlock
        {
            Text = $"{line}",
            Foreground = Brush.Parse(enabled ? "#00FF8E" : "#8191B0"),
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand),
            Tag = line,
            TextDecorations = TextDecorations.Underline,
        };
        lineNumberText.PointerPressed += OnBreakpointLineNumberClicked;
        row.Children.Add(CreateBreakpointCell(lineNumberText, 1, "code-text"));

        var codeText = GetLineText(line).TrimStart();
        var codeBlock = new TextBlock
        {
            Text = codeText,
            Foreground = Brush.Parse(enabled ? "#00FF8E" : "#8191B0"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        ToolTip.SetTip(codeBlock, codeText);
        row.Children.Add(CreateBreakpointCell(codeBlock, 2, "code-text"));

        var deleteButton = new Button
        {
            Content = "×",
            Tag = line,
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
            ColumnDefinitions = new ColumnDefinitions("40,60,*,36"),
            ColumnSpacing = 8,
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

    private void OnBreakpointLineNumberClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TextBlock { Tag: int line })
            return;
        JumpToEditorLine(line);
        e.Handled = true;
    }

    private void JumpToEditorLine(int line)
    {
        CodeEditorTextBox.TextArea.Caret.Line = line;
        CodeEditorTextBox.TextArea.Caret.Column = 1;
        CodeEditorTextBox.TextArea.Caret.BringCaretToView();
        CodeEditorTextBox.Focus();
    }

    private void OnBreakpointCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox { Tag: int line } checkBox)
            return;

        _codeEditorBreakpointLines[line] = checkBox.IsChecked ?? false;
        RefreshBreakpointsPanel();
    }

    private void OnBreakpointDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int line)
            return;

        _codeEditorBreakpointLines.Remove(line);
        RefreshBreakpointsPanel();
    }
    
    private async void OnLoadAssemblyClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Load Assembly Source",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Assembly files")
                    {
                        Patterns = ["*.asm"]
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = ["*.*"]
                    }
                ]
            });

        if (files.Count == 0)
            return;

        var path = files[0].TryGetLocalPath();
        if (path == null)
            return;

        LoadAssemblyIntoEditor(new FileInfo(path));
    }
    
    private void LoadAssemblyIntoEditor(FileInfo fileInfo)
    {
        if (!IsAssemblySourceFile(fileInfo))
        {
            MessageBox("Unsupported File", "Please choose a .asm file.");
            return;
        }

        string sourceText;
        try
        {
            _codeLines = File.ReadAllLines(fileInfo.FullName).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to read source file {File}", fileInfo.FullName);
            MessageBox("Load Error", $"Could not read source file:\n{fileInfo.FullName}");
            return;
        }
        
        _codeEditorBreakpointLines.Clear();

        var codeLinesProcessed = new List<string>();
        var breakpointLines = new List<int>();
        for(int i = 0; i < _codeLines.Count; i++)
        {
            var line = _codeLines[i];
            if (line.Contains("DBP", StringComparison.OrdinalIgnoreCase))
            {
                // The breakpoint belongs to the next non-DBP line, which will land at
                // codeLinesProcessed.Count + 1 (1-based) in the stripped output.
                breakpointLines.Add(codeLinesProcessed.Count + 1);
            }
            else
            {
                codeLinesProcessed.Add(line);
            }
        }
        _codeLines = codeLinesProcessed;
        sourceText = string.Join(Environment.NewLine, _codeLines);

        AddMultipleBreakpoints(breakpointLines.ToArray());

        _loadedAssemblyPath = fileInfo.FullName;
        _isAssemblyDirty = false;

        _suppressCodeEditorEvents = true;
        CodeEditorTextBox.Text = sourceText;
        _suppressCodeEditorEvents = false;

        SetCodePanelState(fileInfo.Name, sourceText);
        CompileAssemblyButton.IsEnabled = true;
        SendAssemblyOverUartButton.IsEnabled = false;
    }
    
    private void SetCodePanelState(string? fileName, string? sourceText)
    {
        var hasFile = !string.IsNullOrWhiteSpace(fileName);
        CodeEditor.IsVisible = hasFile;
        //CodeEditorGutterScrollViewer.IsVisible = hasFile;

        CodeFileNameText.Text = hasFile
            ? BuildCodePanelFileLabel(fileName!, sourceText)
            : "No .asm file loaded";
    }
    
    private string BuildCodePanelFileLabel(string fileName, string? sourceText)
    {
        var dirtySuffix = _isAssemblyDirty ? " *" : string.Empty;
        var lineCount = CountLines(sourceText);
        return $"{fileName}{dirtySuffix}  •  {lineCount} lines";
    }
    
    private static int CountLines(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return 1;

        var count = 1;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                count++;
        }

        return count;
    }

    private void OnCompileAssemblyClick(object? sender, RoutedEventArgs e)
    {
        // Always read directly from the editor so edits are never missed.
        var editorLines = (CodeEditorTextBox.Text ?? string.Empty)
            .Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .ToList();

        // Build finalCode and simultaneously track which pre.asm line number
        // (after DBP expansion: each DBP → 2 lines) maps to which editor line.
        var finalCode = new List<string>();
        var preAsmLineToSourceLine = new Dictionary<int, int>();
        int preAsmLine = 1;

        for (int i = 0; i < editorLines.Count; i++)
        {
            var lineNumber = i + 1;
            var lineText   = editorLines[i];

            if (_codeEditorBreakpointLines.TryGetValue(lineNumber, out var enabled) && enabled)
            {
                // DBP → "LDA #$01\nSTA $FF00" (2 lines in pre.asm).
                // Map both expansion lines to this editor line so the highlight appears
                // when the emulator pauses at the breakpoint trap (STA $FF00).
                preAsmLineToSourceLine[preAsmLine]     = lineNumber; // LDA #$01
                preAsmLineToSourceLine[preAsmLine + 1] = lineNumber; // STA $FF00
                preAsmLine += 2;

                finalCode.Add($"DBP ; BREAKPOINT LINE {lineNumber}");
            }

            preAsmLineToSourceLine[preAsmLine] = lineNumber;
            preAsmLine++;
            finalCode.Add(lineText);
        }

        var inputFile = new FileInfo("editor.asm");
        File.WriteAllText(inputFile.FullName, string.Join("\n", finalCode));

        var (_, addressToPreLine) = CodyAssembler.AssembleFileWithMap(inputFile);

        // Combine: address → pre.asm line → original editor line
        _addressToSourceLine = new Dictionary<ushort, int>();
        foreach (var (address, preLine) in addressToPreLine)
        {
            if (preAsmLineToSourceLine.TryGetValue(preLine, out var sourceLine))
                _addressToSourceLine[address] = sourceLine;
        }

        SendAssemblyOverUartButton.IsEnabled = true;
    }

    private void OnSendAssemblyOverUartClick(object? sender, RoutedEventArgs e)
    {
        var file = new FileInfo("editor.bin");
        FrontendHostBridge.LoadUartSource(file);
        Log.Info("Sent {path} to Uart", file.FullName);
        SendAssemblyOverUartButton.IsEnabled = false;
    }

    private void OnCodeEditorDocumentTextChanged(object? sender, EventArgs e)
    {
        if (_suppressCodeEditorEvents)
            return;

        _isAssemblyDirty = true;
        _addressToSourceLine.Clear(); // Clear mapping
        var sourceText = CodeEditorTextBox.Text ?? string.Empty;
        _codeLines = sourceText.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        SetCodePanelState(GetLoadedAssemblyFileName(), sourceText);
    }
    
    private string GetLoadedAssemblyFileName()
    {
        if (string.IsNullOrWhiteSpace(_loadedAssemblyPath))
            return "Unnamed assembly";

        return Path.GetFileName(_loadedAssemblyPath);
    }

    private static bool IsAssemblySourceFile(FileInfo fileInfo)
    {
        var extension = fileInfo.Extension;
        return extension.Equals(".asm", StringComparison.OrdinalIgnoreCase);
    }


    private void AddMultipleBreakpoints(int[] lineNumbers)
    {
        foreach (int number in lineNumbers)
        {
            _codeEditorBreakpointLines.TryAdd(number, true);
        }
        RefreshBreakpointsPanel(); // Only refresh panel once
    }

    private void RemoveBreakpoint(int lineNumber)
    {
        try
        {
            _codeEditorBreakpointLines.Remove(lineNumber);
            RefreshBreakpointsPanel();
        }
        catch (Exception _)
        {
            // ignored
        }
    }

    private void ToggleBreakpoint(int lineNumber)
    {
        if (!_codeEditorBreakpointLines.TryAdd(lineNumber, true))
            _codeEditorBreakpointLines.Remove(lineNumber, out _);
        RefreshBreakpointsPanel();
    }

    private void OnCpuResetButtonClick(object? sender, RoutedEventArgs e)
    {
        FrontendHostBridge.ResetEmulator();
    }
}
