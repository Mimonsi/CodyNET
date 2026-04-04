using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using CodyNET.Common.Video;

namespace CodyNET.Frontend.Controls;

public partial class ScreenControl : Control, IScreenDevice
{
    private const int DefaultWidth = 320;
    private const int DefaultHeight = 200;
    private const double DefaultScaleFactor = 2.0;

    private double scaleFactor = DefaultScaleFactor;
    private bool fitToContainer;
    private VideoFrame currentFrame;
    private WriteableBitmap? bitmap;
    private readonly object frameSync = new();
    private readonly uint[] emptyPixels = new uint[DefaultWidth * DefaultHeight];

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

    public bool FitToContainer
    {
        get => fitToContainer;
        set
        {
            if (fitToContainer == value)
                return;
            fitToContainer = value;
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    public ScreenControl()
    {
        InitializeComponent();

        currentFrame = new VideoFrame(DefaultWidth, DefaultHeight, emptyPixels);
        ClipToBounds = true;
        UseLayoutRounding = true;
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (fitToContainer && !double.IsInfinity(availableSize.Width) && !double.IsInfinity(availableSize.Height))
        {
            return availableSize;
        }
        return new Size(currentFrame.Width * scaleFactor, currentFrame.Height * scaleFactor);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (fitToContainer)
        {
            var scaleX = finalSize.Width / currentFrame.Width;
            var scaleY = finalSize.Height / currentFrame.Height;
            scaleFactor = Math.Min(scaleX, scaleY);
        }
        return finalSize;
    }

    public void RenderFrame(VideoFrame frame)
    {
        if (frame.Width <= 0 || frame.Height <= 0 || frame.Pixels.Length < frame.Width * frame.Height)
        {
            return;
        }

        lock (frameSync)
        {
            currentFrame = frame;
        }

        Dispatcher.UIThread.Post(() =>
        {
            InvalidateVisual();
        }, DispatcherPriority.Render);
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var frame = GetCurrentFrame();
        EnsureBitmap(frame.Width, frame.Height);
        CopyFramePixelsToBitmap(frame);

        if (bitmap is null)
            return;

        var renderWidth = frame.Width * scaleFactor;
        var renderHeight = frame.Height * scaleFactor;
        var offsetX = fitToContainer ? (Bounds.Width - renderWidth) / 2 : 0;
        var offsetY = fitToContainer ? (Bounds.Height - renderHeight) / 2 : 0;
        var destination = new Rect(offsetX, offsetY, renderWidth, renderHeight);
        var source = new Rect(0, 0, frame.Width, frame.Height);
        context.DrawImage(bitmap, source, destination);
    }

    private VideoFrame GetCurrentFrame()
    {
        lock (frameSync)
        {
            return currentFrame;
        }
    }

    private void EnsureBitmap(int width, int height)
    {
        if (bitmap is not null && bitmap.PixelSize.Width == width && bitmap.PixelSize.Height == height)
            return;

        bitmap?.Dispose();
        bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);
    }

    private void CopyFramePixelsToBitmap(VideoFrame frame)
    {
        if (bitmap is null)
            return;

        using var locked = bitmap.Lock();
        int width = frame.Width;
        int height = frame.Height;
        int sourceStride = width * 4;
        byte[] rowBuffer = new byte[sourceStride];

        for (int y = 0; y < height; y++)
        {
            int rowStart = y * width;
            for (int x = 0; x < width; x++)
            {
                uint rgba = frame.Pixels[rowStart + x];
                byte r = (byte)(rgba >> 24);
                byte g = (byte)(rgba >> 16);
                byte b = (byte)(rgba >> 8);
                byte a = (byte)rgba;

                int offset = x * 4;
                rowBuffer[offset] = b;
                rowBuffer[offset + 1] = g;
                rowBuffer[offset + 2] = r;
                rowBuffer[offset + 3] = a;
            }

            Marshal.Copy(
                rowBuffer,
                0,
                locked.Address + y * locked.RowBytes,
                sourceStride);
        }
    }
}
