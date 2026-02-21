using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

/// <summary>
/// A simple implementation of a device that "renders" a VideoFrame into a ppm file for external display.
/// </summary>
///  TODO: This should not be in core but in frontend
public class PpmVideoOutput(string filePath = "C:/Users/Konsi/Desktop/screen.ppm") : IDisplayDevice
{
    public void RenderFrame(VideoFrame frame)
    {
        if (frame.Pixels.Length != frame.Width * frame.Height)
            throw new ArgumentException("Pixel buffer size does not match width*height.");

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");

        // PPM P6 header: magic, width height, maxval, then binary RGB data
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var bw = new BinaryWriter(fs);

        var header = $"P6\n{frame.Width} {frame.Height}\n255\n";
        bw.Write(System.Text.Encoding.ASCII.GetBytes(header));

        // Write RGB bytes
        for (int i = 0; i < frame.Pixels.Length; i++)
        {
            uint p = frame.Pixels[i];

            // Assuming 0xRRGGBBAA
            byte r = (byte)(p >> 24);
            byte g = (byte)(p >> 16);
            byte b = (byte)(p >> 8);
            byte a = (byte)(p);

            bw.Write(r);
            bw.Write(g);
            bw.Write(b);
        }
    }
}
