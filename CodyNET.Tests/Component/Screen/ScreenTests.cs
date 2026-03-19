using CodyNET.Common.Video;
using CodyNET.Core.Cody;
using CodyNET.Core.Devices;
using NUnit.Framework;

namespace CodyNET.Tests.Screen;

[TestFixture]
public class ScreenTests
{
    private const ushort ScreenMemStart = 0xA400;
    private const ushort CharMemStart = 0xA000;
    private const ushort ColorMemStart = 0xA800;
    private const byte DefaultBorderRegister = 0x20;
    private const byte DefaultBaseRegister = 0x10;

    [Test]
    public void Vid_RegisterWriteRead_TracksDirtyState()
    {
        var memory = new Memory();
        var vid = CreateVideoDevice(memory);

        Assert.That(vid.Dirty, Is.True);

        _ = vid.RenderTextFrame(memory);
        Assert.That(vid.Dirty, Is.False);

        vid.Write(0xD002, 0x05);
        Assert.That(vid.Dirty, Is.True);
        Assert.That(vid.Read(0xD002), Is.EqualTo(0x05));
    }

    [Test]
    public void RenderTextFrame_WhenVideoDisabled_FillsOnlyBorderColor()
    {
        var memory = new Memory();
        var vid = CreateVideoDevice(memory);

        vid.Write(0xD001, 0x01);
        vid.Write(0xD002, 0x06);

        var frame = vid.RenderTextFrame(memory);

        Assert.That(frame.Pixels[0], Is.EqualTo(Palette(6)));
        Assert.That(GetLoresPixel(frame, 0, 0), Is.EqualTo(Palette(6)));
    }

    [Test]
    public void RenderTextFrame_LoresCharacter_UsesAllFourColorSources()
    {
        var memory = new Memory();
        var vid = CreateVideoDevice(memory);

        InitializeLoresTextMode(memory, vid);
        vid.Write(0xD005, 0x43);
        memory.Write(ColorMemStart, 0x21);
        WriteCharacterRow(memory, 0, 0, 0x1B);

        var frame = vid.RenderTextFrame(memory);

        Assert.That(GetLoresPixel(frame, 0, 0), Is.EqualTo(Palette(1)));
        Assert.That(GetLoresPixel(frame, 1, 0), Is.EqualTo(Palette(2)));
        Assert.That(GetLoresPixel(frame, 2, 0), Is.EqualTo(Palette(3)));
        Assert.That(GetLoresPixel(frame, 3, 0), Is.EqualTo(Palette(4)));
    }

    [Test]
    public void RenderTextFrame_SpriteCoordinatesAreBottomRightAnchored()
    {
        var memory = new Memory();
        var vid = CreateVideoDevice(memory);

        InitializeLoresTextMode(memory, vid);
        WriteSolidSprite(memory, 0x40, 0, 0, 2);
        WriteSolidSprite(memory, 0x40, 11, 20, 2);
        WriteSpriteDescriptor(memory, 0, 0, posX: 12, posY: 21, colors: 0x52, spriteDataBank: 0x40);

        var frame = vid.RenderTextFrame(memory);

        Assert.That(GetLoresPixel(frame, 0, 0), Is.EqualTo(Palette(5)));
        Assert.That(GetLoresPixel(frame, 11, 20), Is.EqualTo(Palette(5)));
        Assert.That(GetLoresPixel(frame, 12, 0), Is.EqualTo(Palette(0)));
        Assert.That(GetLoresPixel(frame, 0, 21), Is.EqualTo(Palette(0)));
    }

    [Test]
    public void RenderTextFrame_SpritePixelsUseLowHighAndCommonColors()
    {
        var memory = new Memory();
        var vid = CreateVideoDevice(memory);

        InitializeLoresTextMode(memory, vid);
        vid.Write(0xD006, 0x07);
        WriteSprite(memory, 0x40, CreateRowPattern(1, 2, 3));
        WriteSpriteDescriptor(memory, 0, 0, posX: 12, posY: 21, colors: 0x65, spriteDataBank: 0x40);

        var frame = vid.RenderTextFrame(memory);

        Assert.That(GetLoresPixel(frame, 0, 0), Is.EqualTo(Palette(5)));
        Assert.That(GetLoresPixel(frame, 1, 0), Is.EqualTo(Palette(6)));
        Assert.That(GetLoresPixel(frame, 2, 0), Is.EqualTo(Palette(7)));
        Assert.That(GetLoresPixel(frame, 3, 0), Is.EqualTo(Palette(0)));
    }

    [Test]
    public void RenderTextFrame_SpriteBankComesFromSpriteRegisterHighNibble()
    {
        var memory = new Memory();
        var vid = CreateVideoDevice(memory);

        InitializeLoresTextMode(memory, vid);
        vid.Write(0xD006, 0x10);
        WriteSolidSprite(memory, 0x40, 0, 0, 2);
        WriteSpriteDescriptor(memory, 1, 0, posX: 12, posY: 21, colors: 0x91, spriteDataBank: 0x40);

        var frame = vid.RenderTextFrame(memory);

        Assert.That(GetLoresPixel(frame, 0, 0), Is.EqualTo(Palette(9)));
    }

    [Test]
    public void RenderTextFrame_OverlappingSpritesPreferHigherSpriteIndex()
    {
        var memory = new Memory();
        var vid = CreateVideoDevice(memory);

        InitializeLoresTextMode(memory, vid);
        WriteSolidSprite(memory, 0x40, 0, 0, 1);
        WriteSolidSprite(memory, 0x41, 0, 0, 1);
        WriteSpriteDescriptor(memory, 0, 0, posX: 12, posY: 21, colors: 0x12, spriteDataBank: 0x40);
        WriteSpriteDescriptor(memory, 0, 1, posX: 12, posY: 21, colors: 0x13, spriteDataBank: 0x41);

        var frame = vid.RenderTextFrame(memory);

        Assert.That(GetLoresPixel(frame, 0, 0), Is.EqualTo(Palette(3)));
    }

    [Test]
    public void RenderTextFrame_RowEffectsApplyEnabledEntriesOnTileBoundaries()
    {
        var memory = new Memory();
        var vid = CreateVideoDevice(memory);

        InitializeLoresTextMode(memory, vid);
        vid.Write(0xD001, 0x08);
        vid.Write(0xD005, 0x02);
        WriteCharacterRows(memory, 0, 0xAA);

        vid.Write((ushort)(VideoDevice.VID_CONTROL_BANK + 0), 0xC1);
        vid.Write((ushort)(VideoDevice.VID_DATA_BANK + 0), 0x05);

        var frame = vid.RenderTextFrame(memory);

        Assert.That(GetLoresPixel(frame, 0, 0), Is.EqualTo(Palette(2)));
        Assert.That(GetLoresPixel(frame, 0, 8), Is.EqualTo(Palette(5)));
    }

    private static VideoDevice CreateVideoDevice(Memory memory)
    {
        var vid = new VideoDevice();
        memory.RegisterDevice(vid);
        return vid;
    }

    private static void InitializeLoresTextMode(Memory memory, VideoDevice vid)
    {
        vid.Write(0xD001, 0x00);
        vid.Write(0xD002, DefaultBorderRegister);
        vid.Write(0xD003, DefaultBaseRegister);
        vid.Write(0xD004, 0x00);
        vid.Write(0xD005, 0x00);
        vid.Write(0xD006, 0x00);

        memory.Write(ScreenMemStart, 0x00);
        memory.Write(ColorMemStart, 0x00);
        WriteCharacterRows(memory, 0, 0x00);
    }

    private static void WriteCharacterRows(Memory memory, byte characterIndex, byte rowData)
    {
        for (int row = 0; row < 8; row++)
            WriteCharacterRow(memory, characterIndex, row, rowData);
    }

    private static void WriteCharacterRow(Memory memory, byte characterIndex, int row, byte rowData)
    {
        memory.Write((ushort)(CharMemStart + characterIndex * 8 + row), rowData);
    }

    private static void WriteSpriteDescriptor(Memory memory, int spriteBank, int spriteIndex, byte posX, byte posY, byte colors, byte spriteDataBank)
    {
        ushort address = (ushort)(VideoDevice.VID_SPRITE_BANKS + spriteBank * 0x20 + spriteIndex * 4);
        memory.Write(address, posX);
        memory.Write((ushort)(address + 1), posY);
        memory.Write((ushort)(address + 2), colors);
        memory.Write((ushort)(address + 3), spriteDataBank);
    }

    private static void WriteSolidSprite(Memory memory, byte spriteDataBank, int localX, int localY, int pixelValue)
    {
        WriteSprite(memory, spriteDataBank, CreateRowPattern((localX, pixelValue)), localY);
    }

    private static void WriteSprite(Memory memory, byte spriteDataBank, byte[] rowPattern, int row = 0)
    {
        ushort spriteStart = (ushort)(0xA000 + (spriteDataBank << 6));
        ushort rowStart = (ushort)(spriteStart + row * 3);
        memory.Write(rowStart, rowPattern[0]);
        memory.Write((ushort)(rowStart + 1), rowPattern[1]);
        memory.Write((ushort)(rowStart + 2), rowPattern[2]);
    }

    private static byte[] CreateRowPattern(params int[] values)
    {
        var pixels = new int[12];
        for (int i = 0; i < values.Length && i < pixels.Length; i++)
            pixels[i] = values[i];
        return EncodeSpriteRow(pixels);
    }

    private static byte[] CreateRowPattern(params (int x, int value)[] pixelsWithValues)
    {
        var pixels = new int[12];
        foreach (var (x, value) in pixelsWithValues)
            pixels[x] = value;
        return EncodeSpriteRow(pixels);
    }

    private static byte[] EncodeSpriteRow(IReadOnlyList<int> pixels)
    {
        Assert.That(pixels.Count, Is.EqualTo(12));

        int packed = 0;
        for (int x = 0; x < 12; x++)
            packed |= (pixels[x] & 0x03) << (2 * (11 - x));

        return
        [
            (byte)((packed >> 16) & 0xFF),
            (byte)((packed >> 8) & 0xFF),
            (byte)(packed & 0xFF)
        ];
    }

    private static uint GetLoresPixel(VideoFrame frame, int x, int y)
    {
        int outputX = 2 * x + VideoDevice.BORDER_X;
        int outputY = y + VideoDevice.BORDER_Y;
        return frame.Pixels[outputY * frame.Width + outputX];
    }

    private static uint Palette(int colorIndex) => VideoDevice.Color.PALETTE[colorIndex].ToRgba32();
}
