using CodyNET.Core.Devices;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class UartDeviceTests
{
    private const ushort BASE = 0xD480;
    private UartDevice _uart = null!;

    [SetUp]
    public void SetUp() => _uart = new UartDevice(BASE);

    private byte Reg(ushort offset) => _uart.Read((ushort)(BASE + offset));
    private void WriteReg(ushort offset, byte value) => _uart.Write((ushort)(BASE + offset), value);

    [Test]
    public void DefaultRegistersAreZero()
    {
        Assert.AreEqual(0, Reg(UartDevice.UART_CNTL));
        Assert.AreEqual(0, Reg(UartDevice.UART_CMND));
    }

    [Test]
    public void DisabledByDefault()
    {
        Assert.IsFalse(_uart.IsEnabled());
    }

    [Test]
    public void EnabledWhenCommandBit0Set()
    {
        WriteReg(UartDevice.UART_CMND, 0x01);
        Assert.IsTrue(_uart.IsEnabled());
    }

    [Test]
    public void StatusReflectsEnabledState()
    {
        // Disabled: status = 0
        _uart.UpdateState();
        Assert.AreEqual(0x00, Reg(UartDevice.UART_STAT));

        // Enabled: status = 0x40
        WriteReg(UartDevice.UART_CMND, 0x01);
        _uart.UpdateState();
        Assert.AreEqual(0x40, Reg(UartDevice.UART_STAT));
    }

    [Test]
    public void StatusRegisterIsReadOnly()
    {
        WriteReg(UartDevice.UART_CMND, 0x01);
        _uart.UpdateState();
        WriteReg(UartDevice.UART_STAT, 0x00); // should be ignored
        Assert.AreEqual(0x40, Reg(UartDevice.UART_STAT));
    }

    [Test]
    public void RxHeadAndTailAreZeroInitially()
    {
        Assert.AreEqual(0, Reg(UartDevice.UART_RXHD));
        Assert.AreEqual(0, Reg(UartDevice.UART_RXTL));
    }

    [Test]
    public void TxHeadAndTailAreZeroInitially()
    {
        Assert.AreEqual(0, Reg(UartDevice.UART_TXHD));
        Assert.AreEqual(0, Reg(UartDevice.UART_TXTL));
    }

    [Test]
    public void WritingRxHeadUpdatesRegister()
    {
        WriteReg(UartDevice.UART_RXHD, 0x03);
        Assert.AreEqual(0x03, Reg(UartDevice.UART_RXHD));
    }

    [Test]
    public void WritingTxTailUpdatesRegister()
    {
        WriteReg(UartDevice.UART_TXTL, 0x02);
        Assert.AreEqual(0x02, Reg(UartDevice.UART_TXTL));
    }

    [Test]
    public void WriteAndReadTransmitBuffer()
    {
        WriteReg(UartDevice.UART_TXBF, 0xAB);
        Assert.AreEqual(0xAB, Reg(UartDevice.UART_TXBF));
    }

    [Test]
    public void WriteAndReadReceiveBuffer()
    {
        WriteReg(UartDevice.UART_RXBF, 0xCD);
        Assert.AreEqual(0xCD, Reg(UartDevice.UART_RXBF));
    }

    [Test]
    public void DisablingUartResetsBuffers()
    {
        WriteReg(UartDevice.UART_CMND, 0x01); // enable
        WriteReg(UartDevice.UART_RXHD, 0x03);
        WriteReg(UartDevice.UART_TXHD, 0x02);

        WriteReg(UartDevice.UART_CMND, 0x00); // disable
        _uart.UpdateState();

        Assert.AreEqual(0, Reg(UartDevice.UART_RXHD));
        Assert.AreEqual(0, Reg(UartDevice.UART_TXHD));
    }

    [Test]
    public void ControlRegisterIsReadWrite()
    {
        WriteReg(UartDevice.UART_CNTL, 0x55);
        Assert.AreEqual(0x55, Reg(UartDevice.UART_CNTL));
    }
}
