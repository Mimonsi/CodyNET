// ============================================
// File: CodyProtoScreen/Controls/ScreenControl.axaml.cs
// ============================================

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using CodyNET.Core.Interfaces;

namespace CodyNET.Frontend.Controls;

public partial class ScreenControl : Control, IMemoryMappedDevice
{
    private const int COLS = 40;
    // Reference apparently has 41?
    private const int ROWS = 25;
    private const int CHAR_WIDTH = 4;
    private const int CHAR_HEIGHT = 4;
    private const double DefaultScaleFactor = 2.0;
    private const string ScreenFontFamily =
        "avares://CodyProtoScreen/Assets/Fonts#C64 Pro Mono, Courier New, Consolas";

    
    public ushort StartAddress => 0x0200;
    //public ushort EndAddress => 0x0E00; // 0x0200 + (80 * 60) = 0x12E0, rounded
    public ushort EndAddress => (ushort)(StartAddress + (COLS * ROWS)); // 0x0200 + (80 * 60) = 0x12E0, rounded
    public bool SupportsRead { get; } = true;
    public bool SupportsWrite { get; } = true;
    private char[,] screenBuffer = new char[ROWS, COLS];
    private bool needsRedraw = true;

    private double scaleFactor = DefaultScaleFactor;

    public double ScaleFactor
    {
        get => scaleFactor;
        set
        {
            var clamped = Math.Max(0.1, value);
            if (Math.Abs(scaleFactor - clamped) < 0.001)
                return;

            scaleFactor = clamped;
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    public ScreenControl()
    {
        InitializeComponent();
        
        // Initialize buffer with spaces
        for (int y = 0; y < ROWS; y++)
            for (int x = 0; x < COLS; x++)
                screenBuffer[y, x] = ' ';
        
        Dispatcher.UIThread.Post(() =>
        {
            InvalidateVisual();
            needsRedraw = false;
        });
        ClipToBounds = true;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(COLS * CHAR_WIDTH * scaleFactor, ROWS * CHAR_HEIGHT * scaleFactor);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return new Size(COLS * CHAR_WIDTH * scaleFactor, ROWS * CHAR_HEIGHT * scaleFactor);
    }


    
    public byte Read(ushort address)
    {
        int offset = address - StartAddress;
        int row = offset / COLS;
        int col = offset % COLS;
        
        if (row >= 0 && row < ROWS && col >= 0 && col < COLS)
            return (byte)screenBuffer[row, col];
        
        return 0;
    }
    
    public void Write(ushort address, byte value)
    {
        int offset = address - StartAddress;
        int row = offset / COLS;
        int col = offset % COLS;
        
        if (row >= 0 && row < ROWS && col >= 0 && col < COLS)
        {
            screenBuffer[row, col] = (char)value;
            needsRedraw = true;
            
            Dispatcher.UIThread.Post(() =>
            {
                if (needsRedraw)
                {
                    InvalidateVisual();
                    needsRedraw = false;
                }
            });

        }
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        var backgroundBrush = new BrushConverter().ConvertFromString("#0d0066") as IBrush;
        
        // Black background
        context.FillRectangle(
            // #0d0066
            backgroundBrush ?? Brushes.Blue,
            new Rect(0, 0, Bounds.Width, Bounds.Height)
        );
        
        // Green phosphor text
        var typeface = new Typeface(new FontFamily(ScreenFontFamily));
        var brush = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        using (context.PushTransform(Matrix.CreateScale(scaleFactor, scaleFactor)))
        {
            for (int y = 0; y < ROWS; y++)
            {
                for (int x = 0; x < COLS; x++)
                {
                    char c = screenBuffer[y, x];
                    if (c != ' ' && c != '\0')
                    {
                        var formattedText = new FormattedText(
                            c.ToString(),
                            System.Globalization.CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            typeface,
                            CHAR_HEIGHT,
                            brush
                        );

                        context.DrawText(
                            formattedText,
                            new Point(x * CHAR_WIDTH, y * CHAR_HEIGHT)
                        );
                    }
                }
            }
        }
    }
}