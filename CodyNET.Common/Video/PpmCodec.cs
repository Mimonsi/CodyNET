using System.Text;

namespace CodyNET.Common.Video;

public static class PpmCodec
{
    public static VideoFrame Load(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        if (!TryParseP6(bytes, out var frame))
            throw new InvalidDataException($"Invalid PPM (P6) file: {filePath}");
        return frame;
    }

    public static void Save(VideoFrame frame, string filePath)
    {
        if (frame.Width <= 0 || frame.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(frame), "Frame dimensions must be > 0.");
        if (frame.Pixels.Length != frame.Width * frame.Height)
            throw new ArgumentException("Pixel buffer size must match width*height.", nameof(frame));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var bw = new BinaryWriter(fs);

        var header = $"P6\n{frame.Width} {frame.Height}\n255\n";
        bw.Write(Encoding.ASCII.GetBytes(header));

        for (int i = 0; i < frame.Pixels.Length; i++)
        {
            uint p = frame.Pixels[i];
            byte r = (byte)(p >> 24);
            byte g = (byte)(p >> 16);
            byte b = (byte)(p >> 8);
            bw.Write(r);
            bw.Write(g);
            bw.Write(b);
        }
    }

    private static bool TryParseP6(byte[] bytes, out VideoFrame frame)
    {
        frame = default;
        int i = 0;

        if (!TryReadToken(bytes, ref i, out string magic) || !string.Equals(magic, "P6", StringComparison.Ordinal))
            return false;
        if (!TryReadToken(bytes, ref i, out string widthToken) || !int.TryParse(widthToken, out int width) || width <= 0)
            return false;
        if (!TryReadToken(bytes, ref i, out string heightToken) || !int.TryParse(heightToken, out int height) || height <= 0)
            return false;
        if (!TryReadToken(bytes, ref i, out string maxToken) || !int.TryParse(maxToken, out int maxValue) || maxValue != 255)
            return false;

        SkipWhitespaceAndComments(bytes, ref i);

        int pixelCount = width * height;
        if (pixelCount <= 0)
            return false;

        int rgbLength = pixelCount * 3;
        if (i + rgbLength > bytes.Length)
            return false;

        var pixels = new uint[pixelCount];
        int p = i;
        for (int idx = 0; idx < pixelCount; idx++)
        {
            byte r = bytes[p++];
            byte g = bytes[p++];
            byte b = bytes[p++];
            pixels[idx] = ((uint)r << 24) | ((uint)g << 16) | ((uint)b << 8) | 0xFF;
        }

        frame = new VideoFrame(width, height, pixels);
        return true;
    }

    private static bool TryReadToken(byte[] bytes, ref int i, out string token)
    {
        token = string.Empty;
        SkipWhitespaceAndComments(bytes, ref i);

        if (i >= bytes.Length)
            return false;

        int start = i;
        while (i < bytes.Length && !char.IsWhiteSpace((char)bytes[i]) && bytes[i] != (byte)'#')
            i++;

        if (i <= start)
            return false;

        token = Encoding.ASCII.GetString(bytes, start, i - start);
        return true;
    }

    private static void SkipWhitespaceAndComments(byte[] bytes, ref int i)
    {
        while (i < bytes.Length)
        {
            if (char.IsWhiteSpace((char)bytes[i]))
            {
                i++;
                continue;
            }

            if (bytes[i] == (byte)'#')
            {
                while (i < bytes.Length && bytes[i] != (byte)'\n')
                    i++;
                continue;
            }

            break;
        }
    }
}
