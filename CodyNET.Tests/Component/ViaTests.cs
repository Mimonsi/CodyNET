using CodyNET.Core.Cody;
using CodyNET.Core.Devices;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class ViaTests
{
    private const ushort VIA_BASE = 0x9F00;
    private KeyState _keyState = null!;
    private VersatileInterfaceAdapter _via = null!;

    [SetUp]
    public void SetUp()
    {
        _keyState = new KeyState();
        _via = new VersatileInterfaceAdapter(_keyState);
    }

    private byte Reg(ushort offset) => _via.Read((ushort)(VIA_BASE + offset));
    private void WriteReg(ushort offset, byte value) => _via.Write((ushort)(VIA_BASE + offset), value);

    [Test]
    public void DdrRegistersAreWritable()
    {
        WriteReg(0x02, 0xFF); // DDRB
        WriteReg(0x03, 0x07); // DDRA
        Assert.AreEqual(0xFF, Reg(0x02));
        Assert.AreEqual(0x07, Reg(0x03));
    }

    [Test]
    public void IorBRegisterIsWritable()
    {
        WriteReg(0x02, 0xFF); // DDRB all outputs
        WriteReg(0x00, 0xAA); // IORB
        Assert.AreEqual(0xAA, Reg(0x00));
    }

    [Test]
    public void IerSetBitsWhenBit7High()
    {
        WriteReg(0x0E, 0xC0); // IER: set bit 6 (Timer1)
        // IER read has bit 7 always set per 6522 spec
        Assert.AreEqual(0xC0, Reg(0x0E));
    }

    [Test]
    public void IerClearBitsWhenBit7Low()
    {
        WriteReg(0x0E, 0xC0); // set bit 6
        WriteReg(0x0E, 0x40); // clear bit 6 (bit 7 = 0)
        Assert.AreEqual(0x80, Reg(0x0E)); // only bit 7 remains (always-1 in read)
    }

    [Test]
    public void Timer1StartedByT1ChWrite()
    {
        WriteReg(0x06, 0x04); // T1LL
        WriteReg(0x07, 0x00); // T1LH (does NOT start timer)
        WriteReg(0x04, 0x04); // T1CL latch
        WriteReg(0x05, 0x00); // T1CH: starts timer with counter=0x0004

        // Run 5 cycles — timer fires at 0 and sets IFR bit 6
        _via.Update(5);
        var ifr = Reg(0x0D);
        Assert.That((ifr & 0x40), Is.Not.EqualTo(0), "Timer1 IFR flag should be set after countdown");
    }

    [Test]
    public void T1ClReadClearsTimer1IfrFlag()
    {
        WriteReg(0x04, 0x01);
        WriteReg(0x05, 0x00); // start timer with 1
        _via.Update(2); // fire

        Assert.AreNotEqual(0, Reg(0x0D) & 0x40); // flag set

        _ = Reg(0x04); // reading T1CL clears flag
        Assert.AreEqual(0, Reg(0x0D) & 0x40);
    }

    [Test]
    public void KeyboardStateReflectedInReadIora()
    {
        WriteReg(0x03, 0x07); // DDRA = 0x07 (required)
        WriteReg(0x01, 0x00); // IORA output row = 0

        // No keys pressed: all bits high (active-low)
        byte state = Reg(0x01);
        Assert.AreNotEqual(0x00, state);
    }

    [Test]
    public void PressedKeyIsReflectedInReadIora()
    {
        WriteReg(0x03, 0x07);
        WriteReg(0x01, 0x00); // row 0

        byte stateBefore = Reg(0x01);
        _keyState.SetPressed(CodyKeyCode.KeyQ, true); // KeyQ is in row 0
        byte stateAfter = Reg(0x01);

        Assert.AreNotEqual(stateBefore, stateAfter, "Pressing a key should change IORA read value");
    }

    [Test]
    public void ReadIoraThrowsIfDdrNotSetCorrectly()
    {
        // DDRA default is 0x00, not 0x07
        Assert.Throws<NotSupportedException>(() => Reg(0x01));
    }

    [Test]
    public void StartAddressAndEndAddressCorrect()
    {
        Assert.AreEqual(0x9F00, _via.StartAddress);
        Assert.AreEqual(0x9F0F, _via.EndAddress);
    }
}
