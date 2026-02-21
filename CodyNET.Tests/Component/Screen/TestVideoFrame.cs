using CodyNET.Common.Video;
using CodyNET.Core.Interfaces;
using NUnit.Framework;

namespace CodyNET.Tests.Component.Screen;

public class TestVideoFrame
{
    public static void SaveAsPpm(VideoFrame frame, string path)
    {
        if (frame.Pixels.Length != frame.Width * frame.Height)
            throw new ArgumentException("Pixel buffer size does not match width*height.");

        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");

        // PPM P6 header: magic, width height, maxval, then binary RGB data
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var bw = new BinaryWriter(fs);

        var header = $"P6\n{frame.Width} {frame.Height}\n255\n";
        bw.Write(System.Text.Encoding.ASCII.GetBytes(header));

        // Write RGB bytes
        for (int i = 0; i < frame.Pixels.Length; i++)
        {
            uint p = frame.Pixels[i];

            // Assuming 0xAARRGGBB
            byte r = (byte)((p >> 16) & 0xFF);
            byte g = (byte)((p >> 8) & 0xFF);
            byte b = (byte)(p & 0xFF);

            bw.Write(r);
            bw.Write(g);
            bw.Write(b);
        }
    }
    
    [Test]
    public void TestDumpVideoFrame()
    {
        int fps = 60;
        int offset = 0;
        while (true)
        {
            var frame = MovingRgbFlag(320, 200, offset++);
            SaveAsPpm(frame, "C:/Users/Konsi/Desktop/screen.ppm");
            Thread.Sleep(1000 / fps);
        }
    }
    
    public static VideoFrame MakeTestPattern(int w, int h)
    {
        var px = new uint[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            byte r = (byte)(x * 255 / Math.Max(1, w - 1));
            byte g = (byte)(y * 255 / Math.Max(1, h - 1));
            byte b = 0;
            px[y * w + x] = 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | b; // AARRGGBB
        }
        return new VideoFrame(w, h, px);
    }
    
    public static VideoFrame SolidColor(int w, int h, uint color)
    {
        var px = new uint[w * h];
        for (int i = 0; i < px.Length; i++)
            px[i] = color;
        return new VideoFrame(w, h, px);
    }
    
    public static VideoFrame MovingRgbFlag(int width, int height, int offset)
    {
        var px = new uint[width * height];

        int third = width / 3;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Move the pattern to the right: as offset increases, stripes appear shifted right.
                // We sample the "source" x position with wrap-around.
                int srcX = x - (offset % width);
                if (srcX < 0) srcX += width;

                uint color;
                if (srcX < third)
                    color = 0xFFFF0000u; // Red (AARRGGBB)
                else if (srcX < 2 * third)
                    color = 0xFF00FF00u; // Green
                else
                    color = 0xFF0000FFu; // Blue

                px[y * width + x] = color;
            }
        }

        return new VideoFrame(width, height, px);
    }


}
