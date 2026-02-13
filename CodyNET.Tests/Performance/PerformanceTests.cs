using CodyNET.Assembler;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Utils;
using NUnit.Framework;
using Math = CodyNET.Utils.Math;

namespace CodyNET.Tests.Program;

public class WaitTests
{
    [Test]
    public void TestWait()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        var totalSleepTime = 0.0;
        for (int i = 0; i < 100; i++)
        {
            stopwatch.Restart();
            Thread.Sleep(50);
            var elapsed = stopwatch.Elapsed.TotalMilliseconds;
            totalSleepTime += elapsed;
            TestLog.Info($"Elapsed time: {elapsed} ms");
        }
        TestLog.Info("Average sleep time: " + (totalSleepTime / 100) + " ms");
        
    }
    
    [Test]
    public void TestWhile()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        var totalSleepTime = 0.0;
        for (int i = 0; i < 100; i++)
        {
            stopwatch.Restart();
            while (stopwatch.Elapsed.Milliseconds < 10)
            {
                
            }
            var elapsed = stopwatch.Elapsed.TotalMilliseconds;
            totalSleepTime += elapsed;
            TestLog.Info($"Elapsed time: {elapsed} ms");
        }
        TestLog.Info("Average sleep time: " + (totalSleepTime / 100) + " ms");
        
    }
}

/// <summary>
/// Full program tests. Set a specific cpu and memory state, run the program and verify the end state.
/// </summary>
[Explicit]
public class PerformanceTests
{
    private const int TestDurationSeconds = 20;
    private const int SuccessMarginPercent = 10; // Allow 1% margin for performance variations
    public void MinimalProgram()
    {
        Cody cody = new Cody();
        cody.RunAssemblyFile(FileUtils.GetTestDataPath("minimal.s"));
        Assert.True(true);
    }

    [Test, Order(1)]
    public void TestPerformance_1KHz()
    {
        var result = RunPerformanceTest(TestDurationSeconds, 1_000);
        Assert.That(result, Is.EqualTo(1_000).Within(SuccessMarginPercent).Percent);
    }
    
    [Test, Order(2)]
    public void TestPerformance_10KHz()
    {
        var result = RunPerformanceTest(TestDurationSeconds, 10_000);
        Assert.That(result, Is.EqualTo(10_000).Within(SuccessMarginPercent).Percent);
    }
    
    [Test, Order(3)]
    public void TestPerformance_100KHz()
    {
        var result = RunPerformanceTest(TestDurationSeconds, 100_000);
        Assert.That(result, Is.EqualTo(100_000).Within(SuccessMarginPercent).Percent);
    }
    
    [Test, Order(4)]
    public void TestPerformance_1MHz()
    {
        var result = RunPerformanceTest(TestDurationSeconds, 1_000_000);
        Assert.That(result, Is.EqualTo(1_000_000).Within(SuccessMarginPercent).Percent);
    }
    
    [Test, Order(5)]
    public void TestPerformance_2MHz()
    {
        var result = RunPerformanceTest(TestDurationSeconds, 2_000_000);
        Assert.That(result, Is.EqualTo(2_000_000).Within(SuccessMarginPercent).Percent);
    }
    
    [Test, Order(6)]
    public void TestPerformance_3MHz()
    {
        var result = RunPerformanceTest(TestDurationSeconds, 3_000_000);
        Assert.That(result, Is.EqualTo(3_000_000).Within(SuccessMarginPercent).Percent);
    }
    
    [Test, Order(7)]
    public void TestPerformance_Max()
    {
        var result = RunPerformanceTest(TestDurationSeconds, 10_000_000);
        Assert.That(result, Is.GreaterThan(1_000));
    }
    
    public double RunPerformanceTest(int seconds, long targetFrequency, bool logDisabled = false)
    {
        TestLog.Info($"Starting Performance for {seconds} seconds with target frequency {Math.FormatSi(targetFrequency, "Hz")}");
        Cody cody = new Cody();
        cody.FrequencyHz = targetFrequency;
        var (loadAddr, program) = Binary.LoadBinary(FileUtils.GetTestDataPath("programs/codybros.bin"), defaultLoadAddress: 0xE000);
        //Log.Info("Program: " + CodyDisassembler.Disassemble(program));
        cody.LoadProgram(program, loadAddr);
        TestLog.Level = LogLevel.Debug;
        long totalCycles = 0;
        for (int i = 0; i < seconds; i++)
        {
            var startTime = DateTime.Now;
            long cycles = 0;
            while(DateTime.Now - startTime < TimeSpan.FromSeconds(1))
            {
                cycles += cody.SingleStep();
            }
            TestLog.Info($"Cycles executed in 1 second: {cycles}. CPU Frequency: {Math.FormatSi(cycles, "Hz")}");
            totalCycles += cycles;
        }
        TestLog.Info("Final Average CPU Frequency: " + Math.FormatSi(totalCycles / (double) seconds, "Hz"));
        return totalCycles / (double) seconds;
    }
}
