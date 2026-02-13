using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public class VID : IMemoryMappedDevice
{
    public readonly struct Color
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(uint rgb)
        {
            R = (byte)((rgb >> 16) & 0xFF);
            G = (byte)((rgb >> 8) & 0xFF);
            B = (byte)(rgb & 0xFF);
            A = 255;
        }

        public static Color BLACK => new(0x000000);
        public static Color WHITE => new(0xFFFFFF);
        public static Color RED => new(0xCC0000);
        public static Color GREEN => new(0x33FF66);
        public static Color BLUE => new(0x0D0066);

        public static Color YELLOW => new(0xFFE699);
        public static Color PURPLE => new(0xCC0099);
        public static Color CYAN => new(0x99FFD9);
        public static Color ORANGE => new(0xFFBF99);
        public static Color BROWN => new(0xCC4D00);
        public static Color GRAY => new(0x999999);
        public static Color LIGHT_GRAY => new(0xCCCCCC);
        public static Color DARK_GRAY => new(0x666666);
        public static Color LIGHT_RED => new(0xFF9999);
        public static Color LIGHT_GREEN => new(0x99FFB3);
        public static Color LIGHT_BLUE => new(0xA699FF);

        public static readonly Color[] PALETTE =
        {
            BLACK,
            WHITE,
            RED,
            CYAN,
            PURPLE,
            GREEN,
            BLUE,
            YELLOW,
            ORANGE,
            BROWN,
            LIGHT_RED,
            DARK_GRAY,
            GRAY,
            LIGHT_GREEN,
            LIGHT_BLUE,
            LIGHT_GRAY
        };
    }
    
    // === Address Constants ===
    public const ushort VID_BASE = 0xD000;
    public const ushort VID_CONTROL_BANK = 0xD040;
    public const ushort VID_DATA_BANK = 0xD060;
    public const ushort VID_SPRITE_BANKS = 0xD080;

    // === IMemoryMappedDevice ===
    public ushort StartAddress => VID_BASE;
    public ushort EndAddress => 0xD0FF;
    public bool SupportsRead => true;
    public bool SupportsWrite => true;
    
    // Screen constants
    // Height and width of character-based screen
    public const byte CONTENT_WIDTH = 160;
    public const ushort HIRES_WIDTH = 2 * CONTENT_WIDTH; // 320 pixels in hires mode
    public const byte CONTENT_HEIGHT = 200;
    public const int BORDER_X = 4;
    public const int BORDER_Y = 8;
    public const int WIDTH = HIRES_WIDTH + 2 * BORDER_X;
    public const int HEIGHT = CONTENT_HEIGHT + 2 * BORDER_Y;
    
    // === Address Constants in shared memory ===
    private const ushort TEXT_SCREEN_BASE = 0xA000; // 1000 bytes
    private const ushort TEXT_COLOR_BASE = 0xA000 + 0x0400;
    private const ushort CHARSET_BASE = 0xA800;
    
    // Sprite geometry in lores
    private const int SPRITE_W = 12;
    private const int SPRITE_H = 21;

    // Store video memory locally in the device, for easy access
    private readonly byte[] _videoMemory = new byte[0x100];
    
    /// <summary>Set by writes to VID registers; frontend can use this as a render hint.</summary>
    public bool Dirty { get; private set; } = true;
    
    public byte Read(ushort address)
    {
        if (address < StartAddress || address > EndAddress)
            throw new ArgumentOutOfRangeException(nameof(address), $"Address {address:X4} is out of range for VID device.");
        return _videoMemory[address - StartAddress];
    }

    public void Write(ushort address, byte value)
    {
        if (address < StartAddress || address > EndAddress)
            throw new ArgumentOutOfRangeException(nameof(address), $"Address {address:X4} is out of range for VID device.");
        _videoMemory[address - StartAddress] = value;
        Dirty = true;
    }

    public readonly record struct VideoFrame(int Width, int Height, uint[] Pixels);

    public VideoFrame RenderTextFrame(Memory memory)
    {
        const int COLS = 40;
        const int ROWS = 25;
        const int CHAR_W = 8;
        const int CHAR_H = 8;
        
        var pixels = new uint[WIDTH * HEIGHT];
        
        // 1. Border color (simple color)
        var border = ColorToRgba(Color.PALETTE[_videoMemory[0x02] & 0x0F]); // 0x02 as border color
        FillRect(pixels, WIDTH, 0, 0, WIDTH, HEIGHT, border);

        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                int cellIndex = row * COLS + col;

                byte ch = memory.Read((ushort)(TEXT_SCREEN_BASE + cellIndex));
                byte colorByte = memory.Read((ushort)(TEXT_COLOR_BASE + cellIndex));
                
                int foregroundColorIndex = colorByte & 0x0F; // Low Nibble
                int backgroundColorIndex = (colorByte >> 4) & 0x0F; // High Nibble

                uint foregroundColor = ColorToRgba(Color.PALETTE[foregroundColorIndex]);
                uint backgroundColor = ColorToRgba(Color.PALETTE[backgroundColorIndex]);
                
                ushort glyphBase = (ushort)(CHARSET_BASE + ch * CHAR_H); // Start address for 8 Bitmap rows

                int px0 = BORDER_X + col * CHAR_W; // Left pixel position of the character cell
                int py0 = BORDER_Y + row * CHAR_H; // Top pixel position of the character cell
                
                for(int gy = 0; gy < CHAR_H; gy++) // gy = pixel-row within the character
                {
                    byte charRowData = memory.Read((ushort)(glyphBase + gy)); // 4 bits for 4 pixels
                    for(int gx = 0; gx < CHAR_W; gx++) // gx = pixel-column within the character
                    {
                        int bit = 3 - gx; // Bit 3 -> 0 from left to right
                        bool pixelOn = ((charRowData >> bit) & 1) != 0; // Check if the bit is set
                        
                        int x = px0 + gx; // Absolute pixel x position in framebuffer
                        int y = py0 + gy; // Absolute pixel y position in framebuffer
                        
                        pixels[y * WIDTH + x] = pixelOn ? foregroundColor : backgroundColor;
                    }
                }
            }
        }

        Dirty = false;
        return new VideoFrame(WIDTH, HEIGHT, pixels);
    }
    
    private static uint ColorToRgba(Color c)
    {
        // RGBA8888 in a uint: 0xRRGGBBAA (matches many pipelines; adjust if your bitmap wants BGRA)
        return ((uint)c.R << 24) | ((uint)c.G << 16) | ((uint)c.B << 8) | c.A;
    }

    private static void FillRect(uint[] pix, int stride, int x, int y, int w, int h, uint color)
    {
        for (int yy = 0; yy < h; yy++)
        {
            int row = (y + yy) * stride + x;
            for (int xx = 0; xx < w; xx++)
                pix[row + xx] = color;
        }
    }
}