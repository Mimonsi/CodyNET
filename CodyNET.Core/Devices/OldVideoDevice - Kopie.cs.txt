using System;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

/// <summary>
/// Cody VID register device (0xD001..0xD0FF) + software renderer.
/// Video RAM (screen/color/character/sprite data) lives in Propeller RAM at 0xA000..0xDFFF.
/// This device only stores VID registers and exposes RenderPixels() that reads from system memory.
/// </summary>
public sealed class OldVideoDevice : IMemoryMappedDevice
{
    // === Addressing (absolute CPU addresses) ===
    public const ushort VID_BASE = 0xD000;

    // D001..D006 control registers used by renderer (see Rust reference)
    public const ushort REG_CONTROL      = 0xD001;
    public const ushort REG_BORDER_COLOR = 0xD002;
    public const ushort REG_BASE         = 0xD003;
    public const ushort REG_SCROLL       = 0xD004;
    public const ushort REG_SCREEN_COLORS= 0xD005;
    public const ushort REG_SPRITE       = 0xD006;

    // Row effects: D040..D05F (control), D060..D07F (data)
    public const ushort ROWFX_CTRL_BASE  = 0xD040; // 32 bytes
    public const ushort ROWFX_DATA_BASE  = 0xD060; // 32 bytes

    // Sprite register bank base depends on sprite reg high nibble:
    // sprite_bank_start = 0xD080 + 0x20 * (sprite >> 4)
    public const ushort SPRITE_BANK_BASE = 0xD080;

    // === Visible output geometry (matches Rust constants) ===
    public const int CONTENT_WIDTH  = 160;
    public const int CONTENT_HEIGHT = 200;
    public const int HIRES_WIDTH    = 2 * CONTENT_WIDTH;
    public const int BORDER_X       = 4;
    public const int BORDER_Y       = 8;
    public const int OUT_WIDTH      = HIRES_WIDTH + 2 * BORDER_X; // 328
    public const int OUT_HEIGHT     = CONTENT_HEIGHT + 2 * BORDER_Y; // 216

    // Lores tile: 4x8, Hires tile: 8x8
    private const int TILE_W_LORES = 4;
    private const int TILE_W_HIRES = 8;
    private const int TILE_H       = 8;
    private const int TILES_X      = 40; // fixed in reference (tile_index = tile_y * 40 + tile_x)

    // Sprite geometry in lores
    private const int SPRITE_W = 12;
    private const int SPRITE_H = 21;

    // === IMemoryMappedDevice ===
    public ushort StartAddress { get; } = VID_BASE;
    public ushort EndAddress   { get; } = 0xD0FF;
    public bool SupportsRead   { get; } = true;
    public bool SupportsWrite  { get; } = true;

    // store only the VID register page (D000..D0FF), index by low byte
    private readonly byte[] _regs = new byte[0x100];

    /// <summary>Set by writes to VID registers; frontend can use this as a render hint.</summary>
    public bool Dirty { get; private set; } = true;

    public byte Read(ushort address)
    {
        if (address < VID_BASE || address > 0xD0FF) return 0;
        return _regs[address - VID_BASE];
    }

    public void Write(ushort address, byte value)
    {
        if (address < VID_BASE || address > 0xD0FF) return;

        var idx = address - VID_BASE;
        if (_regs[idx] == value) return;

        _regs[idx] = value;
        Dirty = true;
    }

    public void ClearDirty() => Dirty = false;

    // === Palette (matches Rust palette) ===
    // Stored as 0xAARRGGBB (Avalonia/WriteableBitmap-friendly)
    private static readonly uint[] Palette =
    {
        0xFF000000, // 0 BLACK
        0xFFFFFFFF, // 1 WHITE
        0xFFCC0000, // 2 RED
        0xFF99FFD9, // 3 CYAN
        0xFFCC0099, // 4 PURPLE
        0xFF33FF66, // 5 GREEN
        0xFF0D0066, // 6 BLUE
        0xFFFFE699, // 7 YELLOW
        0xFFFFBF99, // 8 ORANGE
        0xFFCC4D00, // 9 BROWN
        0xFFFF9999, // A LIGHT_RED
        0xFF666666, // B DARK_GRAY
        0xFF999999, // C GRAY
        0xFF99FFB3, // D LIGHT_GREEN
        0xFFA699FF, // E LIGHT_BLUE
        0xFFCCCCCC, // F LIGHT_GRAY
    };

    /// <summary>
    /// Renders the current frame into a pixel buffer (size OUT_WIDTH*OUT_HEIGHT),
    /// reading video RAM via <paramref name="readMem"/> and VID registers from this device.
    /// </summary>
    /// <param name="readMem">Function to read a byte from the global memory map (CPU address space).</param>
    /// <param name="pixels">ARGB buffer, length must be OUT_WIDTH*OUT_HEIGHT.</param>
    public void RenderPixels(Func<ushort, byte> readMem, Span<uint> pixels)
    {
        if (pixels.Length != OUT_WIDTH * OUT_HEIGHT)
            throw new ArgumentException($"pixels must be {OUT_WIDTH * OUT_HEIGHT} entries.");

        // --- decode control flags (exactly like Rust) ---
        byte control = Read(REG_CONTROL);
        bool hiresMode        = (control & 0x20) != 0;
        bool disableVideo     = (control & 0x01) != 0;
        bool enableVScroll    = (control & 0x02) != 0 && !hiresMode;
        bool enableHScroll    = (control & 0x04) != 0 && !hiresMode;
        bool enableRowEffects = (control & 0x08) != 0;
        bool bitmapMode       = (control & 0x10) != 0;

        // border fill
        byte border = Read(REG_BORDER_COLOR);
        uint borderColor = Palette[border & 0x0F];
        pixels.Fill(borderColor);

        // color memory bank start (0xA000 + 0x400*(border>>4))
        ushort colorMemStart = (ushort)(0xA000 + 0x400 * (border >> 4));

        if (disableVideo)
            return;

        int width  = CONTENT_WIDTH - (enableHScroll ? 2 * 4 : 0);
        int height = CONTENT_HEIGHT - (enableVScroll ? 8 : 0);
        if (hiresMode) width *= 2;

        int borderX = BORDER_X + (enableHScroll ? 2 * 2 : 0);
        int borderY = BORDER_Y + (enableVScroll ? 4 : 0);

        byte baseReg        = Read(REG_BASE);         // can be modified by row effects
        byte scrollReg      = Read(REG_SCROLL);       // can be modified by row effects
        byte screenColors   = Read(REG_SCREEN_COLORS);// can be modified by row effects
        byte spriteReg      = Read(REG_SPRITE);       // can be modified by row effects

        for (int y = 0; y < height; y++)
        {
            // Apply row effects on tile row boundaries (like Rust: enable_row_effects && in_tile_y == 0)
            int tileY = y / TILE_H;
            int inTileY = y % TILE_H;
            if (enableRowEffects && inTileY == 0)
            {
                for (int effectIndex = 0; effectIndex < 32; effectIndex++)
                {
                    byte fxCtrl = Read((ushort)(ROWFX_CTRL_BASE + effectIndex));
                    if ((fxCtrl & 0x80) == 0) continue;     // disabled
                    int row = fxCtrl & 0x1F;
                    if (row != tileY) continue;

                    int destination = (fxCtrl >> 5) & 0x3;  // 0..3
                    byte fxData = Read((ushort)(ROWFX_DATA_BASE + effectIndex));
                    switch (destination)
                    {
                        case 0: baseReg      = fxData; break;
                        case 1: scrollReg    = fxData; break;
                        case 2: screenColors = fxData; break;
                        case 3: spriteReg    = fxData; break;
                    }
                }
            }

            RenderLine(
                y,
                width,
                hiresMode,
                bitmapMode,
                enableVScroll,
                enableHScroll,
                baseReg,
                scrollReg,
                screenColors,
                spriteReg,
                colorMemStart,
                borderX,
                borderY,
                readMem,
                pixels
            );
        }

        Dirty = false;
    }

    private static void RenderLine(
        int y,
        int width,
        bool hiresMode,
        bool bitmapMode,
        bool enableVScroll,
        bool enableHScroll,
        byte baseReg,
        byte scrollReg,
        byte screenColors,
        byte spriteReg,
        ushort colorMemStart,
        int borderX,
        int borderY,
        Func<ushort, byte> readMem,
        Span<uint> pixels
    )
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

            byte localColors = readMem((ushort)(colorMemStart + tileIndex));

            uint color;
            if (hiresMode)
            {
                // hires: 1bpp, no background/sprites/fine scroll
                byte rowData = bitmapMode
                    ? readMem((ushort)(screenMemStart + 8 * tileIndex + inTileY))
                    : ReadCharRow(readMem, screenMemStart, charMemStart, tileIndex, inTileY);

                int bit = (rowData >> (7 - inTileX)) & 0x1;
                int palIndex = bit == 0 ? (localColors & 0x0F) : (localColors >> 4);
                color = Palette[palIndex];
            }
            else
            {
                // lores: 2bpp with 4 color sources (local low, local high, screen low, screen high)
                byte rowData = bitmapMode
                    ? readMem((ushort)(screenMemStart + 8 * tileIndex + inTileY))
                    : ReadCharRow(readMem, screenMemStart, charMemStart, tileIndex, inTileY);

                int twoBits = (rowData >> (2 * (3 - inTileX))) & 0x3;
                int palIndex = twoBits switch
                {
                    0 => (localColors & 0x0F),
                    1 => (localColors >> 4),
                    2 => (screenColors & 0x0F),
                    3 => (screenColors >> 4),
                    _ => 0
                };

                // sprites (lores only)
                palIndex = ApplySprites(
                    palIndex,
                    x,
                    y,
                    spriteReg,
                    readMem
                );

                color = Palette[palIndex];

                // lores doubles horizontal pixels
                int outY = y + borderY;
                int outX = 2 * x + borderX;
                int p = outY * OUT_WIDTH + outX;
                pixels[p] = color;
                pixels[p + 1] = color;
                continue;
            }

            // hires: one pixel per x
            int outY2 = y + borderY;
            int outX2 = x + borderX;
            pixels[outY2 * OUT_WIDTH + outX2] = color;
        }
    }

    private static byte ReadCharRow(
        Func<ushort, byte> readMem,
        ushort screenMemStart,
        ushort charMemStart,
        int tileIndex,
        int inTileY
    )
    {
        byte ch = readMem((ushort)(screenMemStart + tileIndex));
        return readMem((ushort)(charMemStart + 8 * ch + inTileY));
    }

    private static int ApplySprites(
        int basePalIndex,
        int x,
        int y,
        byte spriteReg,
        Func<ushort, byte> readMem
    )
    {
        int paletteIndex = basePalIndex;

        int commonColor = spriteReg & 0x0F;
        ushort spriteBankStart = (ushort)(SPRITE_BANK_BASE + 0x20 * (spriteReg >> 4));

        for (int spriteIndex = 0; spriteIndex < 8; spriteIndex++)
        {
            ushort spriteDataStart = (ushort)(spriteBankStart + 4 * spriteIndex);

            byte posX = readMem(spriteDataStart);
            int minX = posX - SPRITE_W;
            int maxX = posX;
            if (x < minX || x >= maxX) continue;

            byte posY = readMem((ushort)(spriteDataStart + 1));
            int minY = posY - SPRITE_H;
            int maxY = posY;
            if (y < minY || y >= maxY) continue;

            byte spriteColors = readMem((ushort)(spriteDataStart + 2));
            byte spriteLocationIndex = readMem((ushort)(spriteDataStart + 3));
            ushort spriteLocation = (ushort)(0xA000 + 0x40 * spriteLocationIndex);

            int inSpriteX = x - minX;
            int inSpriteY = y - minY;
            int spritePixelIndex = inSpriteY * SPRITE_W + inSpriteX;
            int spriteByteIndex = spritePixelIndex / 4;
            int shift = 2 * (3 - (spritePixelIndex % 4));
            int pixel = (readMem((ushort)(spriteLocation + spriteByteIndex)) >> shift) & 0x3;

            switch (pixel)
            {
                case 0: break; // transparent
                case 1: paletteIndex = spriteColors & 0x0F; break;
                case 2: paletteIndex = spriteColors >> 4; break;
                case 3: paletteIndex = commonColor; break;
            }
        }

        return paletteIndex;
    }
}
