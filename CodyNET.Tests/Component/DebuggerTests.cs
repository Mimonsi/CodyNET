using CodyNET.Core.Cody;
using CodyNET.Core.Devices;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class DebuggerTests
{
    private Cpu _cpu = null!;
    private Debugger _debugger = null!;

    [SetUp]
    public void SetUp()
    {
        _cpu = new Cpu();
        _debugger = new Debugger(_cpu);
    }

    [Test]
    public void NoBreakpointsByDefault()
    {
        Assert.IsFalse(_debugger.HasBreakpoints());
        Assert.IsEmpty(_debugger.GetBreakpointsSnapshot());
    }

    [Test]
    public void AddBreakpointRegistersIt()
    {
        _debugger.AddBreakpoint(0x1000, true);
        Assert.IsTrue(_debugger.HasBreakpoints());
        var snapshot = _debugger.GetBreakpointsSnapshot();
        Assert.AreEqual(1, snapshot.Count);
        Assert.AreEqual(0x1000, snapshot[0].Address);
        Assert.IsTrue(snapshot[0].Enabled);
    }

    [Test]
    public void DuplicateBreakpointIsIgnored()
    {
        _debugger.AddBreakpoint(0x1000, true);
        _debugger.AddBreakpoint(0x1000, true); // duplicate
        Assert.AreEqual(1, _debugger.GetBreakpointsSnapshot().Count);
    }

    [Test]
    public void RemoveBreakpointDeregistersIt()
    {
        _debugger.AddBreakpoint(0x1000, true);
        _debugger.RemoveBreakpoint(0x1000);
        Assert.IsFalse(_debugger.HasBreakpoints());
    }

    [Test]
    public void SetBreakpointEnabledTogglesFlag()
    {
        _debugger.AddBreakpoint(0x2000, true);
        _debugger.SetBreakpointEnabled(0x2000, false);
        Assert.IsFalse(_debugger.GetBreakpointsSnapshot()[0].Enabled);
        _debugger.SetBreakpointEnabled(0x2000, true);
        Assert.IsTrue(_debugger.GetBreakpointsSnapshot()[0].Enabled);
    }

    [Test]
    public void SetBreakpointEnabledReturnsFalseForUnknownAddress()
    {
        Assert.IsFalse(_debugger.SetBreakpointEnabled(0xDEAD, true));
    }

    [Test]
    public void IsAtBreakpointReturnsTrueWhenPcMatchesEnabledBreakpoint()
    {
        // Set PC to known address and add breakpoint there
        _cpu.Memory.RomIsWritable = true;
        _cpu.Memory.Write(0xFFFC, 0x00); // reset vector low
        _cpu.Memory.Write(0xFFFD, 0x30); // reset vector high -> PC = 0x3000
        _cpu.Memory.Write(0x3000, 0xEA); // NOP
        _cpu.Reset();

        _debugger.AddBreakpoint(_cpu.PC, true);
        Assert.IsTrue(_debugger.IsAtBreakpoint());
    }

    [Test]
    public void IsAtBreakpointReturnsFalseForDisabledBreakpoint()
    {
        _cpu.Memory.RomIsWritable = true;
        _cpu.Memory.Write(0xFFFC, 0x00);
        _cpu.Memory.Write(0xFFFD, 0x30);
        _cpu.Reset();

        _debugger.AddBreakpoint(_cpu.PC, false); // disabled
        Assert.IsFalse(_debugger.IsAtBreakpoint());
    }

    [Test]
    public void IgnoreBreakpointsSuppressesHits()
    {
        _cpu.Memory.RomIsWritable = true;
        _cpu.Memory.Write(0xFFFC, 0x00);
        _cpu.Memory.Write(0xFFFD, 0x30);
        _cpu.Reset();

        _debugger.AddBreakpoint(_cpu.PC, true);
        _debugger.IgnoreBreakpoints = true;
        Assert.IsFalse(_debugger.IsAtBreakpoint());
    }

    [Test]
    public void DbpWriteTriggersPause()
    {
        // Before write: no pause
        Assert.IsFalse(_debugger.IsAtBreakpoint());
        // Write to DBP address (0xFF00)
        _debugger.Write(0xFF00, 0x00);
        Assert.IsTrue(_debugger.IsAtBreakpoint());
        // Pause is consumed after first check
        Assert.IsFalse(_debugger.IsAtBreakpoint());
    }

    [Test]
    public void GetBreakpointsSnapshotReturnsCopy()
    {
        _debugger.AddBreakpoint(0x1000, true, "test");
        var snapshot = _debugger.GetBreakpointsSnapshot();
        snapshot.Clear(); // modifying snapshot should not affect internals
        Assert.IsTrue(_debugger.HasBreakpoints());
    }

    [Test]
    public void AddAndRemoveWatch()
    {
        _debugger.AddWatch(0x0200);
        Assert.AreEqual(1, _debugger.WatchAddresses.Count);
        Assert.AreEqual(0x0200, _debugger.WatchAddresses[0].Address);

        _debugger.RemoveWatch(0x0200);
        Assert.IsEmpty(_debugger.WatchAddresses);
    }

    [Test]
    public void DuplicateWatchIsIgnored()
    {
        _debugger.AddWatch(0x0200);
        _debugger.AddWatch(0x0200);
        Assert.AreEqual(1, _debugger.WatchAddresses.Count);
    }
}
