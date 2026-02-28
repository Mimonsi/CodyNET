using CodyNET.Common.Utils;
using CodyNET.Common.Video;
using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public class VideoDevice : IVideoDevice
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
        
        // 0xRRGGBBAA (RGBA8)
        public uint ToRgba32() => (uint)((R << 24) | (G << 16) | (B << 8) | 0xFF);

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
    
    // Control registers and state
    private const int CONTROL_REGISTER = 0xD001;
    private bool disableDisplay; // If set, disables display output and only show border color
    private bool vScrolling; // If set, enables vertical scrolling (and reduces screen height by one row)
    private bool hScrolling; // If set, enables horizontal scrolling (and reduces screen width by two columns)
    private bool rowEffects; // If set, enables row effects
    private bool bitmapMode; // If set, enables bitmap mode
    private bool hiresMode; // If set, enables high resolution mode (320x200 instead of 160x200)
    private bool blackAndWhite; // If set, enables black and white mode (disables color output)

    // === IMemoryMappedDevice ===
    public ushort StartAddress => VID_BASE;
    public ushort EndAddress => 0xD0FF;
    public bool SupportsRead => true;
    public bool SupportsWrite => true;
    
    // Screen constants
    // Height and width of character-based screen
    public const byte CONTENT_WIDTH = 160;
    public const ushort HIRES_WIDTH = 2 * CONTENT_WIDTH; // 320 pixels in hires (high resolution) mode
    public const byte CONTENT_HEIGHT = 200;
    public const int BORDER_X = 4;
    public const int BORDER_Y = 8;
    public const int WIDTH = HIRES_WIDTH + 2 * BORDER_X;
    public const int HEIGHT = CONTENT_HEIGHT + 2 * BORDER_Y;
    
    // === Address Constants in shared memory ===
    private const ushort TEXT_SCREEN_BASE = 0xA000; // 1000 bytes
    private const ushort TEXT_COLOR_BASE = 0xA000 + 0x0400;

    private const ushort CHARSET_BASE = 0xA800;
    // Sprite geometry in lores (low resolution)
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
        Log.Verbose($"VID Write: Address={address:X4} Value={value:X2}");
        _videoMemory[address - StartAddress] = value;
        Dirty = true;
    }
    
    public void SetBorderColor(byte colorIndex)
    {
        _videoMemory[0x02] = (byte)(colorIndex & 0x0F); // Store border color in video memory for easy access
        Dirty = true;
    }
    
    private void ReadControlRegister()
    {
        byte value = _videoMemory[CONTROL_REGISTER - StartAddress];
        disableDisplay = (value & (1 << 0)) != 0;
        vScrolling     = (value & (1 << 1)) != 0;
        hScrolling     = (value & (1 << 2)) != 0;
        rowEffects     = (value & (1 << 3)) != 0;
        bitmapMode     = (value & (1 << 4)) != 0;
        hiresMode      = (value & (1 << 5)) != 0;
        blackAndWhite  = (value & (1 << 6)) != 0;
    }
    
    public VideoFrame RenderTextFrame(Memory memory)
    {
        ReadControlRegister();
        const int COLS = 40;
        const int ROWS = 25;
        const int CHAR_W = 8;
        const int CHAR_H = 8;
        
        var pixels = new uint[WIDTH * HEIGHT];
        
        // 1. Fill with border color (0x02 register, low 4 bits as color index)
        var borderColor = Color.PALETTE[_videoMemory[0x02] & 0x0F].ToRgba32(); // 0x02 as border color
        FillRect(pixels, WIDTH, 0, 0, WIDTH, HEIGHT, borderColor);
        if (disableDisplay) // if screen disabled, only show border color
        {
            Dirty = false;
            return new VideoFrame(WIDTH, HEIGHT, pixels);
        }
        
        var colorRamBank = ???
        

        Dirty = false;
        return new VideoFrame(WIDTH, HEIGHT, pixels);
    }

    // Test only
    private static void FillBlackWhite(uint[] pixels)
    {
        bool isWhite = true;

        int xPos = BORDER_X;
        int yPos = BORDER_Y;
        int w = CONTENT_WIDTH * 2;   // 320
        int h = CONTENT_HEIGHT;      // 200

        uint white = Color.WHITE.ToRgba32();
        uint black = Color.BLACK.ToRgba32();

        for (int y = yPos; y < yPos + h; y++)
        {
            int row = y * WIDTH;

            for (int x = xPos; x < xPos + w; x += 2)
            {
                uint c = isWhite ? white : black;
                pixels[row + x] = c;
                pixels[row + x + 1] = c; // fat pixel
                isWhite = !isWhite;
            }
            isWhite = !isWhite; // Checkboard pattern: flip at end of each row as well
        }
    }

    private static void FillRect(uint[] pix, int stride, int xPos, int yPos, int w, int h, uint color)
    {
        for (int y = 0; y < h; y++)
        {
            int row = (yPos + y) * stride + xPos;
            for (int x = 0; x < w; x++)
                pix[row + x] = color;
        }
    }
}
