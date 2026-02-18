namespace CodyNET.Tests.Screen;

using System;

// Farben können wegen downsampling auch gemischt werden
public static class ConsoleFramePresenter
{
    // Downsample-Faktoren (je größer, desto weniger Output)
    private const int SX = 2; // 2 Pixel horizontal -> 1 Char
    private const int SY = 4; // 4 Pixel vertikal   -> 1 Char

    public static void ClearFrame()
    {
        Console.Clear();
    }

    public static void PrintFrame(int width, int height, uint[] pixels)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Cursor oben links, damit du "animieren" könntest (optional)
        Console.SetCursorPosition(0, 0);

        int outW = width / SX;
        int outH = height / SY;

        for (int oy = 0; oy < outH; oy++)
        {
            for (int ox = 0; ox < outW; ox++)
            {
                // Durchschnittsfarbe aus SX*SY Pixels
                long r = 0, g = 0, b = 0;
                int count = 0;

                int baseX = ox * SX;
                int baseY = oy * SY;

                for (int dy = 0; dy < SY; dy++)
                {
                    for (int dx = 0; dx < SX; dx++)
                    {
                        int x = baseX + dx;
                        int y = baseY + dy;
                        uint c = pixels[y * width + x];

                        // Deine Packung: 0xRRGGBBAA
                        byte rr = (byte)(c >> 24);
                        byte gg = (byte)(c >> 16);
                        byte bb = (byte)(c >> 8);

                        r += rr; g += gg; b += bb;
                        count++;
                    }
                }

                byte R = (byte)(r / count);
                byte G = (byte)(g / count);
                byte B = (byte)(b / count);

                // TrueColor foreground
                Console.Write($"\x1b[38;2;{R};{G};{B}m█");
            }

            // Reset + newline
            Console.Write("\x1b[0m\n");
        }

        Console.Write("\x1b[0m");
    }
}
