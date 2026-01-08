using System;
using System.Collections.Generic;
using System.IO;
using CodyPrototype.Assembler;
using CodyPrototype.Cpu;
using CodyPrototype.Utils;
using NUnit.Framework;

namespace CodyPrototype.Tests;

public class ProgramTests
{
    [Test]
    public void TestMisc()
    {

    }
    
    [Test]
    public void TestAssembler()
    {
        ICodyAssembler assembler = new TassAssembler();
        var bytes = assembler.AssembleFile("testdata/minimal.s");
        var expectedBytes = FileUtils.GetBytesFromFile("testdata/minimal.bin");
        Assert.That(bytes, Is.EqualTo(expectedBytes));
    }
    
    [Test]
    public void TestMinimal()
    {
        Cpu.Cpu cpu = new();
        var bytes = FileUtils.GetBytesFromFile("testdata/minimal.bin");
        cpu.LoadProgram(bytes, 0x0600);
        cpu.RunUntilFinish();
        var state = cpu.GetState();

        var finalState = new CpuState()
        {
            A = 8,
            X = 0,
            Y = 0,
            PC = 0x0610,
            S = 0xFF,
            P = 0,
            Memory = new Dictionary<ushort, byte>()
            {
                { 0x200, 0x1 },
                { 0x201, 0x5 },
                { 0x202, 0x8 },
            }
        };
        var (success, message) = finalState.CheckState(state);
        Assert.IsTrue(success, message);
    }
    
    [Test]
    public void TestAssembledMinimalDrs()
    {
        Log.Info("Starting TestAssembledMinimalDRS");
        Cpu.Cpu cpu = new();
        var bytes = FileUtils.GetBytesFromFile("testdata/minimalDRS.bin");
        cpu.LoadProgram(bytes, 0x0600);
        cpu.RunUntilFinish();
        var state = cpu.GetState();

        var finalState = new CpuState()
        {
            A = 8,
            X = 0,
            Y = 0,
            PC = 0x0612,
            S = 0xFF,
            P = 0,
            Memory = new Dictionary<ushort, byte>()
            {
                { 0x200, 0x1 },
                { 0x201, 0x5 },
                { 0x202, 0x8 },
            }
        };
        var (success, message) = finalState.CheckState(state);
        Assert.IsTrue(success, message);
    }
    
    [Test]
    public void TestAssembledMinimalDbp()
    {
        Log.Info("Starting TestAssembledMinimalDBP");
        Cpu.Cpu cpu = new();
        var bytes = FileUtils.GetBytesFromFile("testdata/minimalDBP.bin");
        Console.SetIn(new StringReader("regs\ncontinue\n"));
        cpu.LoadProgram(bytes, 0x0600);
        cpu.RunUntilFinish();
        var state = cpu.GetState();

        var finalState = new CpuState()
        {
            A = 8,
            X = 0,
            Y = 0,
            PC = 0x0611,
            S = 0xFF,
            P = 0,
            Memory = new Dictionary<ushort, byte>()
            {
                { 0x200, 0x1 },
                { 0x201, 0x5 },
                { 0x202, 0x8 },
            }
        };
        var (success, message) = finalState.CheckState(state);
        Assert.IsTrue(success, message);
    }
    
    [Test]
    public void TestAssembledMinimalDmp()
    {
        Log.Info("Starting TestAssembledMinimalDMP");
        Cpu.Cpu cpu = new();
        var bytes = FileUtils.GetBytesFromFile("testdata/minimalDMP.bin");
        cpu.LoadProgram(bytes, 0x0600);
        cpu.RunUntilFinish();
        var state = cpu.GetState();

        var finalState = new CpuState()
        {
            A = 8,
            X = 0,
            Y = 0,
            PC = 0x0611,
            S = 0xFF,
            P = 0,
            Memory = new Dictionary<ushort, byte>()
            {
                { 0x200, 0x1 },
                { 0x201, 0x5 },
                { 0x202, 0x8 },
            }
        };
        var (success, message) = finalState.CheckState(state);
        Assert.IsTrue(success, message);
    }

    [Test]
    public void TestPerformance()
    {
        Log.Info("Starting TestPerformance");
        Cpu.Cpu cpu = new();
        var bytes = FileUtils.GetBytesFromFile("testdata/performance_big.bin");
        cpu.LoadProgram(bytes, 0x0600);
        cpu.CyclesPerSecond = 1_00000; // 100 kHz for testing
        Log.Level = LogLevel.Debug;
        var startTime = DateTime.Now;
        var steps = cpu.RunUntilFinish();
        var endTime = DateTime.Now;
        var duration = endTime - startTime;
        Log.Info($"Execution Time: {duration.TotalMilliseconds} ms for {steps} steps. Average {steps / duration.TotalSeconds} steps/s");
    }
}