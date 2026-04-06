using System.Diagnostics;
using System.Globalization;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;

namespace CodyNET.Core.Devices;

public class Profiler(TimeSpan snapshotInterval, TimeSpan? logInterval)
{
    private static readonly string DumpFilePath = Path.Combine(
        AppContext.BaseDirectory, "profiler.txt");

    private readonly Stopwatch _snapshotStopwatch = Stopwatch.StartNew();
    private readonly Stopwatch _logStopwatch = Stopwatch.StartNew();
    private readonly TimeSpan? _logInterval = logInterval <= TimeSpan.Zero ? null : logInterval;
    private readonly TimeSpan _snapshotInterval = snapshotInterval <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : snapshotInterval;
    private long _lastCycleCount;
    private long _lastFrameCount;
    private long _totalCyclesExecuted;
    private long _targetFrequencyHz;
    public ProfilerSnapshot LastSnapshot = new();
    private readonly List<ProfilerSnapshot> _pendingSnapshots = [];
    private bool TEST_MODE_ENABLED = false; // Only enable for performance tests
    private int SNAPSHOTS_TILL_EXIT = 5;

    static Profiler()
    {
        try { File.WriteAllText(DumpFilePath, ""); } // Clear file on startup
        catch { /* ignore */ }
    }

    public void CalculateSnapshot()
    {
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
            TargetFrames = 60 * elapsedSeconds, // TODO: Find Target Frames
            SecondsElapsed = elapsedSeconds
        };

        _lastCycleCount = _totalCyclesExecuted;
        _lastFrameCount = 0;
        _snapshotStopwatch.Restart();

        if (_logInterval.HasValue)
        {
            _pendingSnapshots.Add(LastSnapshot);
            if (_logStopwatch.Elapsed >= _logInterval.Value)
            {
                _logStopwatch.Restart();
                FlushSnapshotsToFile();
            }
        }
    }

    private void FlushSnapshotsToFile()
    {
        if (_pendingSnapshots.Count == 0)
            return;
        
        try
        {
            var totalSeconds = _pendingSnapshots.Sum(s => s.SecondsElapsed);
            var avgFreq = (long)_pendingSnapshots.Average(s => s.ActualFrequency);
            var avgTarget = _pendingSnapshots[0].TargetFrequency;
            var totalFrames = _pendingSnapshots.Sum(s => s.ActualFrames);
            var avgFps = totalSeconds > 0 ? totalFrames / totalSeconds : 0;
            var freqPct = avgTarget > 0
                ? ((double)avgFreq / avgTarget * 100).ToString("F1", CultureInfo.InvariantCulture)
                : "FAST";

            var ts = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var line = $"{ts}  freq={avgFreq,10} Hz  target={avgTarget,10} Hz  pct={freqPct,6}%  fps={avgFps,5:F1}  window={totalSeconds:F2}s\n";
            File.AppendAllText(DumpFilePath, line);
            _pendingSnapshots.Clear();
            if (TEST_MODE_ENABLED)
            {
                if (SNAPSHOTS_TILL_EXIT == 0)
                {
                    Log.Info("Performance results done, exiting...");
                    Environment.Exit(0);
                }
                else
                {
                    SNAPSHOTS_TILL_EXIT--;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to dump profiler snapshot: {Error}", ex.Message);
        }
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