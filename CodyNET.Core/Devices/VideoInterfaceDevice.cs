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

    
    // === Control Registers ===
    private const ushort REG_CONTROL = 0xD001;
    private const ushort REG_BORDER_COLOR = 0xD002;
    private const ushort REG_BASE = 0xD003;
    private const ushort REG_SCROLL = 0xD004;
    private const ushort REG_SCREEN_COLORS = 0xD005;
    private const ushort REG_SPRITE = 0xD006;
    
    // === State flags ===
    private bool disableVideo; // If set, disables display output and only show border color
    private bool enableVScroll; // If set, enables vertical scrolling (and reduces screen height by one row)
    private bool enableHScroll; // If set, enables horizontal scrolling (and reduces screen width by two columns)
    private bool enableRowEffects; // If set, enables row effects
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
    
    // Lores tile: 4x8, Hires tile: 8x8
    private const int TILE_W_LORES = 4;
    private const int TILE_W_HIRES = 8;
    private const int TILE_H       = 8;
    private const int TILES_X      = 40; // fixed in reference (tile_index = tile_y * 40 + tile_x)

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
        Log.Verbose($"VID Write: Address={address:X4} Value={value:X2}");
        _videoMemory[address - StartAddress] = value;
        Dirty = true;
    }
    
    public Interrupt Update(long cycle)
    {
        // TODO: Check
        return Interrupt.None;
    }
    
    public void SetBorderColor(byte colorIndex)
    {
        int i = REG_BORDER_COLOR - StartAddress;
        _videoMemory[i] = (byte)((_videoMemory[i] & 0xF0) | (colorIndex & 0x0F));
        Dirty = true;
    }
    
    private void ReadControlRegister(Memory memory)
    {
        byte control = memory.Read(REG_CONTROL);
        hiresMode = (control & 0x20) != 0;
        disableVideo = (control & 0x01) != 0;
        enableVScroll = (control & 0x02) != 0 && !hiresMode;
        enableHScroll = (control & 0x04) != 0 && !hiresMode;
        enableRowEffects = (control & 0x08) != 0;
        bitmapMode = (control & 0x10) != 0;
        blackAndWhite = (control & 0x40) != 0; // TODO: Check if correct bit, or if even supported
    }
    
    public VideoFrame RenderTextFrame(Memory memory)
    {
        ReadControlRegister(memory);
        
        var pixels = new uint[WIDTH * HEIGHT];
        
        // 1. Fill with border color (0x02 register, low 4 bits as color index)
        var border = memory.Read(REG_BORDER_COLOR);
        var borderColor = Color.PALETTE[border & 0x0F].ToRgba32(); // 0x02 as border color
        var colorMemStart = (ushort)(0xA000 + 0x400 * (border >> 4));
        
        FillRect(pixels, WIDTH, 0, 0, WIDTH, HEIGHT, borderColor);
        if (disableVideo) // if screen disabled, only show border color
        {
            Dirty = false;
            return new VideoFrame(WIDTH, HEIGHT, pixels);
        }
        
        // 2. Render content area
        // Calculate content area dimensions based on control register flags
        
        int width  = CONTENT_WIDTH - (enableHScroll ? 2 * 4 : 0);
        int height = CONTENT_HEIGHT - (enableVScroll ? 8 : 0);
        if (hiresMode) width *= 2;

        int borderX = BORDER_X + (enableHScroll ? 2 * 2 : 0);
        int borderY = BORDER_Y + (enableVScroll ? 4 : 0);

        byte baseReg        = memory.Read(REG_BASE);         // can be modified by row effects
        byte scrollReg      = memory.Read(REG_SCROLL);       // can be modified by row effects
        byte screenColors   = memory.Read(REG_SCREEN_COLORS);// can be modified by row effects
        byte spriteReg      = memory.Read(REG_SPRITE);       // can be modified by row effects

        for (int y = 0; y < height; y++)
        {
            // TODO: Apply row effects
            RenderLine(y, width, baseReg, scrollReg, screenColors, spriteReg, colorMemStart, borderX, borderY, memory, pixels);
        }

        Dirty = false;
        return new VideoFrame(WIDTH, HEIGHT, pixels);
    }

    private void RenderLine(int y, int width, byte baseReg, byte scrollReg, byte screenColors, byte spriteReg, ushort colorMemStart, int borderX, int borderY, Memory memory, uint[] pixels)
    {
        // screen memory bank: 0xA000 + 0x400*(base>>4)
        ushort screenMemStart = (ushort)(0xA000 + 0x400 * (baseReg >> 4));
        // character memory bank: 0xA000 + 0x800*(base & 0xF)
        ushort charMemStart   = (ushort)(0xA000 + 0x800 * (baseReg & 0x0F));
        
        int vScrollAmount = enableVScroll ? (scrollReg & 0x07) : 0;
        int hScrollAmount = enableHScroll ? ((scrollReg >> 4) & 0x03) : 0;

        int tileW = hiresMode ? TILE_W_HIRES : TILE_W_LORES;

        for (int x = 0; x < width; x++)
        {
            int scrolledX = x + hScrollAmount;
            int scrolledY = y + vScrollAmount;
            
            int tileX = scrolledX / tileW;
            int tileY = scrolledY / TILE_H;
            int tileIndex = tileY * TILES_X + tileX;
            
            int inTileX = scrolledX % tileW;
            int inTileY = scrolledY % TILE_H;

            byte localColors = memory.Read((ushort) (colorMemStart + tileIndex));

            uint color;
            if (hiresMode)
            {
                // hires: 1bit per pixel, no background/sprites/fine scroll
                byte rowData = bitmapMode
                    ? memory.Read((ushort)(screenMemStart + 8 * tileIndex + inTileY))
                    : ReadCharRow(memory, screenMemStart, charMemStart, tileIndex, inTileY);
                
                int bit = (rowData >> (7 - inTileX)) & 0x1;
                int palIndex = bit == 0 ? (localColors & 0x0F) : (localColors >> 4);
                color = Color.PALETTE[palIndex].ToRgba32();
            }
            else
            {
                // lores: 2bits per pixel (background/sprite/fine scroll)
                byte rowData = bitmapMode
                    ? memory.Read((ushort)(screenMemStart + 8 * tileIndex + inTileY))
                    : ReadCharRow(memory, screenMemStart, charMemStart, tileIndex, inTileY);
                
                int twoBits = (rowData >> (2 * (3 - inTileX))) & 0x03;
                int paletteIndex = twoBits switch
                {
                    0 => (localColors & 0x0F),
                    1 => (localColors >> 4),
                    2 => (screenColors & 0x0F),
                    3 => (screenColors >> 4),
                    _ => 0
                };
                
                // TODO: Sprites
                paletteIndex = ApplySprites(paletteIndex, x, y, spriteReg, memory);

                color = Color.PALETTE[paletteIndex].ToRgba32();
                
                // lores doubles horizontal pixels
                int outY = y + borderY;
                int outX = 2 * x + borderX;
                int p = outY * WIDTH + outX;
                pixels[p] = color;
                pixels[p + 1] = color;
                continue;
            }
            
            // hires: one pixel per x
            int outY2 = y + borderY;
            int outX2 = x + borderX;
            pixels[outY2 * WIDTH + outX2] = color!;
        }
    }

    private int ApplySprites(int paletteIndex, int i, int i1, byte spriteReg, Memory memory)
    {
        //throw new NotImplementedException();
        // TODO:
        return paletteIndex;
    }

    private byte ReadCharRow(Memory memory, ushort screenMemStart, ushort charMemStart, int tileIndex, int inTileY)
    {
        byte ch = memory.Read((ushort)(screenMemStart + tileIndex));
        var value = memory.Read((ushort)(charMemStart + 8 * ch + inTileY));
        return value;
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
