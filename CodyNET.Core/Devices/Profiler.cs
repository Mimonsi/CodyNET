using System.Diagnostics;
using CodyNET.Common.Utils;

namespace CodyNET.Core.Devices;

public class Profiler
{
    private readonly Stopwatch _windowStopwatch = Stopwatch.StartNew();
    private readonly TimeSpan _logInterval;
    private long _lastCycleCount;

    public Profiler(TimeSpan logInterval)
    {
        _logInterval = logInterval <= TimeSpan.Zero ? TimeSpan.FromSeconds(5) : logInterval;
    }

    public void SampleCpu(long totalCyclesExecuted, long targetFrequencyHz)
    {
        if (_windowStopwatch.Elapsed < _logInterval)
            return;

        var elapsedSeconds = _windowStopwatch.Elapsed.TotalSeconds;
        if (elapsedSeconds <= 0)
            return;

        var cyclesInWindow = totalCyclesExecuted - _lastCycleCount;
        var averageFrequencyHz = cyclesInWindow / elapsedSeconds;

        if (targetFrequencyHz == -1)
        {
            Log.Info("CPU frequency avg ({windowSeconds:F1}s): {avgHz:N0} Hz (FAST mode)",
                elapsedSeconds, averageFrequencyHz);
        }
        else
        {
            var utilizationPercent = averageFrequencyHz / targetFrequencyHz * 100.0;
            Log.Info("CPU frequency avg ({windowSeconds:F1}s): {avgHz:N0} Hz (target: {targetHz:N0} Hz, {utilizationPercent:F1}% utilization)",
                elapsedSeconds, averageFrequencyHz, targetFrequencyHz, utilizationPercent);
        }

        _lastCycleCount = totalCyclesExecuted;
        _windowStopwatch.Restart();
    }
}