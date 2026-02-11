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
    public const ushort VID_BASE = 0xD00;
    public const ushort VID_CONTROL_BANK = 0xD040;
    public const ushort VID_DATA_BANK = 0xD060;
    public const ushort VID_SPRITE_BANKS = 0xD080;

    // === IMemoryMappedDevice ===
    public ushort StartAddress => VID_BASE;
    public ushort EndAddress => 0xD0FF;
    public bool SupportsRead => true;
    public bool SupportsWrite => true;
    
    // Screen constants
    public const byte CONTENT_WIDTH = 160;
    public const ushort HIRES_WIDTH = 2 * CONTENT_WIDTH; // 320 pixels in hires mode
    public const byte CONTENT_HEIGHT = 200;
    public const int BORDER_X = 4;
    public const int BORDER_Y = 8;
    public const int WIDTH = HIRES_WIDTH + 2 * BORDER_X;
    public const int HEIGHT = CONTENT_HEIGHT + 2 * BORDER_Y;
    
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
}