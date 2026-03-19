using System.Diagnostics;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;

namespace CodyNET.Core.Devices;

public class Profiler
{
    private readonly Stopwatch _snapshotStopwatch = Stopwatch.StartNew();
    private readonly TimeSpan? _logInterval;
    private readonly TimeSpan _snapshotInterval;
    private long _lastCycleCount;
    private long _lastFrameCount;
    private long _totalCyclesExecuted;
    private long _targetFrequencyHz;
    public ProfilerSnapshot LastSnapshot;

    public Profiler(TimeSpan snapshotInterval, TimeSpan? logInterval)
    {
        _logInterval = logInterval <= TimeSpan.Zero ? null : logInterval;
        _snapshotInterval = snapshotInterval <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : snapshotInterval;
        LastSnapshot = new ProfilerSnapshot();
    }

    public void CalculateSnapshot()
    {
        // TODO Next: Calculate actual clock and frames based on the last log interval, and update LastSnapshot accordingly.

        var elapsedSeconds = _snapshotStopwatch.Elapsed.TotalSeconds;
        if (elapsedSeconds <= 0)
            return;
        
        var cyclesInWindow = _totalCyclesExecuted - _lastCycleCount;
        var averageFrequencyHz = (long) (cyclesInWindow / (elapsedSeconds));
        
        LastSnapshot = new ProfilerSnapshot()
        {
            ActualFrequency = averageFrequencyHz,
            TargetFrequency = _targetFrequencyHz,
            ActualFrames = _lastFrameCount,
            TargetFrames = 60, // TODO: Find Target Frames
            SecondsElapsed = elapsedSeconds
        };
        
        _lastCycleCount = _totalCyclesExecuted;
        _lastFrameCount = 0;
        _snapshotStopwatch.Restart();
        LogSnapshot();
    }

    private void LogSnapshot()
    {
        if (LastSnapshot.TargetFrequency <= 0)
            Log.Info("CPU frequency avg ({windowSeconds:F1}s): {avgHz:N0} Hz (FAST Mode)",
                LastSnapshot.SecondsElapsed, LastSnapshot.ActualFrequency, LastSnapshot.TargetFrequency, LastSnapshot.FrequencyTargetPercent);
        else
            Log.Info("CPU frequency avg ({windowSeconds:F1}s): {avgHz:N0} Hz (target: {targetHz:N0} Hz, {utilizationPercent:F1}% of target)",
            LastSnapshot.SecondsElapsed, LastSnapshot.ActualFrequency, LastSnapshot.TargetFrequency, LastSnapshot.FrequencyTargetPercent);
        Log.Info("Average frames rendered in window: {actualFrames} (target: {targetFrames}, {frameTargetPercent:F1}% of target)",
            LastSnapshot.ActualFrames, LastSnapshot.TargetFrames, LastSnapshot.FrameTargetPercent);
    }

    public void SampleCpu(long totalCyclesExecuted, long targetFrequencyHz)
    {
        _totalCyclesExecuted = totalCyclesExecuted;
        _targetFrequencyHz = targetFrequencyHz;
        if (_snapshotStopwatch.Elapsed >= _snapshotInterval)
            CalculateSnapshot();
    }

    public void FrameRendered()
    {
        _lastFrameCount++;
    }
}