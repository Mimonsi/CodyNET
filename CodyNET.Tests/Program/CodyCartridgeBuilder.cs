using NUnit.Framework;
using System;
using System.IO;

namespace CodyNET.Tests.Program;

public class CodyCartridgeBuilder
{
    private const ushort Entry = 0xE000;
    private const int CartSize = 0x2000; // 8 KiB

    /// <summary>
    /// Builds a Cody cartridge image with proper vectors.
    /// </summary>
    private static byte[] BuildCartridge(byte[] program)
    {
        if (program.Length > CartSize)
            throw new ArgumentException("Program exceeds cartridge size.");

        var cart = new byte[CartSize];

        // Copy program at $E000 (offset 0)
        Array.Copy(program, cart, program.Length);

        // Set vectors (relative to $E000 window)
        WriteWord(cart, 0x1FFA, Entry); // NMI
        WriteWord(cart, 0x1FFC, Entry); // RESET
        WriteWord(cart, 0x1FFE, Entry); // IRQ

        return cart;
    }

    [Test]
    public void TestBuildColorCycleCart()
    {
        byte[] program =
        {
            0x78,                   // SEI
            0xD8,                   // CLD
            0xA9, 0x00,             // LDA #$00
            0x8D, 0x02, 0xD0,       // STA $D002
            0x1A,                   // INA
            0x20, 0x0E, 0xE0,       // JSR delay
            0x4C, 0x04, 0xE0,       // JMP loop

            // delay:
            0xA0, 0x40,             // LDY #$40
            0xA2, 0xFF,             // LDX #$FF
            0xCA,                   // DEX
            0xD0, 0xFD,             // BNE
            0x88,                   // DEY
            0xD0, 0xF8,             // BNE
            0x60                    // RTS
        };

        var cart = BuildCartridge(program);
        File.WriteAllBytes("colorcycle_cart.bin", cart);
    }

    [Test]
    public void TestCheckerboard()
    {
        byte[] program =
        {
            0x78,                         // SEI
            0xD8,                         // CLD

            0xA9, 0x20,                   // LDA #$20        ; HIRES
            0x8D, 0x01, 0xD0,             // STA $D001

            0xA9, 0x00,                   // LDA #$00
            0x8D, 0x02, 0xD0,             // STA $D002       ; border black

            0xA9, 0xA4,                   // LDA #$A4
            0x85, 0x01,                   // STA $01
            0x64, 0x00,                   // STZ $00

            // PageLoop:
            0xA9, 0x00,                   // LDA #$00
            0xA0, 0x00,                   // LDY #$00

            // ByteLoop:
            0x91, 0x00,                   // STA ($00),Y
            0xC8,                         // INY
            0xD0, 0xFB,                   // BNE ByteLoop

            0xE6, 0x01,                   // INC $01
            0xA5, 0x01,                   // LDA $01
            0xC9, 0xA8,                   // CMP #$A8
            0xD0, 0xEF,                   // BNE PageLoop

            0x4C, 0x23, 0xE0              // JMP forever
        };

        var cart = BuildCartridge(program);
        File.WriteAllBytes("checkerboard.bin", cart);
    }

    private static void WriteWord(byte[] buffer, int offset, ushort value)
    {
        buffer[offset]     = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)(value >> 8);
    }
}