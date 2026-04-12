using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class MemoryTests
{
    private Memory _mem = null!;

    [SetUp]
    public void SetUp() => _mem = new Memory();

    [Test]
    public void RamReadWrite()
    {
        _mem.Write(0x0200, 0xAB);
        Assert.AreEqual(0xAB, _mem.Read(0x0200));
    }

    [Test]
    public void PropReadWrite()
    {
        _mem.Write(0xB000, 0xCD);
        Assert.AreEqual(0xCD, _mem.Read(0xB000));
    }

    [Test]
    public void RomIsReadOnlyByDefault()
    {
        _mem.LoadBytes([0x42], 0xE000);
        _mem.Write(0xE000, 0xFF); // should be silently ignored
        Assert.AreEqual(0x42, _mem.Read(0xE000));
    }

    [Test]
    public void RomWritableWhenFlagSet()
    {
        _mem.LoadBytes([0x42], 0xE000);
        _mem.RomIsWritable = true;
        _mem.Write(0xE000, 0xFF);
        Assert.AreEqual(0xFF, _mem.Read(0xE000));
    }

    [Test]
    public void ForceWriteBypassesRomProtection()
    {
        _mem.LoadBytes([0x42], 0xE000);
        _mem.ForceWrite(0xE000, 0x99);
        Assert.AreEqual(0x99, _mem.Read(0xE000));
        // RomIsWritable should be restored to false
        Assert.IsFalse(_mem.RomIsWritable);
    }

    [Test]
    public void LoadBytesAcrossRamPropBoundary()
    {
        byte[] data = [0x11, 0x22, 0x33, 0x44];
        _mem.LoadBytes(data, 0x9FFE);
        Assert.AreEqual(0x11, _mem.Read(0x9FFE));
        Assert.AreEqual(0x22, _mem.Read(0x9FFF));
        Assert.AreEqual(0x33, _mem.Read(0xA000));
        Assert.AreEqual(0x44, _mem.Read(0xA001));
    }

    [Test]
    public void RegisteredDeviceInterceptsRead()
    {
        var device = new StubDevice(0x8000, 0x8000, supportsRead: true);
        _mem.RegisterDevice(device);
        _mem.Read(0x8000);
        Assert.AreEqual(1, device.ReadCount);
    }

    [Test]
    public void RegisteredDeviceInterceptsWrite()
    {
        var device = new StubDevice(0x8000, 0x8000, supportsWrite: true);
        _mem.RegisterDevice(device);
        _mem.Write(0x8000, 0x55);
        Assert.AreEqual(1, device.WriteCount);
    }

    [Test]
    public void UnregisteredDeviceNoLongerInterceptsReads()
    {
        var device = new StubDevice(0x8000, 0x8000, supportsRead: true);
        _mem.RegisterDevice(device);
        _mem.UnregisterDevice(device);
        _mem.Read(0x8000);
        Assert.AreEqual(0, device.ReadCount);
    }

    [Test]
    public void ZeroPageReadWrite()
    {
        _mem.Write(0x0000, 0x01);
        _mem.Write(0x00FF, 0xFF);
        Assert.AreEqual(0x01, _mem.Read(0x0000));
        Assert.AreEqual(0xFF, _mem.Read(0x00FF));
    }

    private class StubDevice(ushort start, ushort end, bool supportsRead = false, bool supportsWrite = false) : IMemoryMappedDevice
    {
        public ushort StartAddress { get; } = start;
        public ushort EndAddress { get; } = end;
        public bool SupportsRead { get; } = supportsRead;
        public bool SupportsWrite { get; } = supportsWrite;
        public int ReadCount;
        public int WriteCount;

        public byte Read(ushort address) { ReadCount++; return 0x00; }
        public void Write(ushort address, byte value) { WriteCount++; }
        public Interrupt Update(long cycle) => Interrupt.None;
    }
}
