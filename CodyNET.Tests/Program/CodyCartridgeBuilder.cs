using NUnit.Framework;

namespace CodyNET.Tests.Program;

using System;
using System.IO;

public class CodyCartridgeBuilder
{
    /// <summary>
    /// Small binary to cycle through all background colors
    /// </summary>
    [Test]
    public void TestBuildColorCycleCart()
    {
        const ushort entry = 0xE000;
        const int cartSize = 0x2000; // 8 KiB

        byte[] program =
        {
            0x78,                   // SEI
            0xD8,                   // CLD
            0xA9, 0x00,             // LDA #$00
            0x8D, 0x02, 0xD0,       // STA $D002
            0x1A,                   // INA
            0x20, 0x0E, 0xE0,       // JSR $E00E
            0x4C, 0x04, 0xE0,       // JMP $E004
            0xA0, 0x40,             // LDY #$40
            0xA2, 0xFF,             // LDX #$FF
            0xCA,                   // DEX
            0xD0, 0xFD,             // BNE
            0x88,                   // DEY
            0xD0, 0xF8,             // BNE
            0x60                    // RTS
        };

        var cart = new byte[cartSize];

        // Copy program at $E000 (offset 0)
        Array.Copy(program, cart, program.Length);

        // Set vectors (relative to $E000 window)
        WriteWord(cart, 0x1FFA, entry); // NMI
        WriteWord(cart, 0x1FFC, entry); // RESET
        WriteWord(cart, 0x1FFE, entry); // IRQ

        File.WriteAllBytes("colorcycle_cart.bin", cart);
    }

    // Writes a 16-bit word in little-endian order.
    private static void WriteWord(byte[] buffer, int offset, ushort value)
    {
        buffer[offset]     = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)(value >> 8);
    }
}