using System.Diagnostics;
using System.Globalization;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;

namespace CodyNET.Core.Devices;

public class Profiler
{
    private static readonly string PerfResultsPath = Path.Combine(
        AppContext.BaseDirectory, "benchmark.csv");

    private readonly Stopwatch _snapshotStopwatch = Stopwatch.StartNew();
    private readonly Stopwatch _logStopwatch = Stopwatch.StartNew();
    private readonly TimeSpan? _logInterval;
    private readonly TimeSpan _snapshotInterval;
    private long _lastCycleCount;
    private long _lastFrameCount;
    private long _totalCyclesExecuted;
    private long _targetFrequencyHz;
    public ProfilerSnapshot LastSnapshot = new();
    private readonly List<ProfilerSnapshot> _pendingSnapshots = [];

    // Test mode fields
    private readonly int? _testSampleCount;
    private readonly TimeSpan _warmupDuration;
    private readonly Stopwatch? _warmupStopwatch;
    private bool _warmupComplete;
    private int _remainingSamples;
    private readonly List<long> _allSampleFrequencies = [];

    /// <summary>
    /// True when a performance test run has collected all requested samples.
    /// Checked by the emulator loop to stop gracefully.
    /// </summary>
    public bool TestComplete { get; private set; }

    /// <param name="snapshotInterval">How often to compute a performance snapshot.</param>
    /// <param name="logInterval">How often to flush aggregated snapshots to disk. Null or &lt;= 0 disables file output.</param>
    /// <param name="testSampleCount">When set, enables test mode: collect this many flush cycles then signal completion. Null = normal profiling.</param>
    /// <param name="warmupDuration">Time to wait before collecting test samples (lets the startup routine finish). Only used when testSampleCount is set.</param>
    public Profiler(
        TimeSpan snapshotInterval,
        TimeSpan? logInterval,
        int? testSampleCount = null,
        TimeSpan? warmupDuration = null)
    {
        _snapshotInterval = snapshotInterval <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : snapshotInterval;
        _logInterval = logInterval <= TimeSpan.Zero ? null : logInterval;
        _testSampleCount = testSampleCount;

        if (testSampleCount.HasValue)
        {
            _warmupDuration = warmupDuration ?? TimeSpan.FromSeconds(3);
            _warmupStopwatch = Stopwatch.StartNew();
            _remainingSamples = testSampleCount.Value;

            // Write CSV header
            try { File.WriteAllText(PerfResultsPath, "Sample,FrequencyHz,FPS\n"); }
            catch { /* ignore */ }
        }
    }

    public void CalculateSnapshot()
    {
        var elapsedSeconds = _snapshotStopwatch.Elapsed.TotalSeconds;
        if (elapsedSeconds <= 0)
            return;

        var cyclesInWindow = _totalCyclesExecuted - _lastCycleCount;
        var averageFrequencyHz = (long)(cyclesInWindow / elapsedSeconds);

        LastSnapshot = new ProfilerSnapshot
        {
            ActualFrequency = averageFrequencyHz,
            TargetFrequency = _targetFrequencyHz,
            ActualFrames = _lastFrameCount,
            TargetFrames = 60 * elapsedSeconds,
            SecondsElapsed = elapsedSeconds
        };

        _lastCycleCount = _totalCyclesExecuted;
        _lastFrameCount = 0;
        _snapshotStopwatch.Restart();

        if (_testSampleCount.HasValue && _logInterval.HasValue)
        {
            _pendingSnapshots.Add(LastSnapshot);
            if (_logStopwatch.Elapsed >= _logInterval.Value)
            {
                _logStopwatch.Restart();
                FlushTestSamples();
            }
        }
    }

    private void FlushTestSamples()
    {
        if (_pendingSnapshots.Count == 0)
            return;

        try
        {
            var totalSeconds = _pendingSnapshots.Sum(s => s.SecondsElapsed);
            var avgFreq = (long)_pendingSnapshots.Average(s => s.ActualFrequency);
            var totalFrames = _pendingSnapshots.Sum(s => s.ActualFrames);
            var fps = totalSeconds > 0 ? totalFrames / totalSeconds : 0;
            var sampleNr = _testSampleCount!.Value - _remainingSamples + 1;

            File.AppendAllText(PerfResultsPath,
                string.Create(CultureInfo.InvariantCulture, $"{sampleNr},{avgFreq},{fps:F1}\n"));
            _allSampleFrequencies.Add(avgFreq);
            Log.Info("Perf sample {Sample}/{Total}: {Freq} Hz",
                sampleNr, _testSampleCount.Value, avgFreq);

            _pendingSnapshots.Clear();

            _remainingSamples--;
            if (_remainingSamples <= 0)
            {
                var avg = (long)_allSampleFrequencies.Average();
                var min = _allSampleFrequencies.Min();
                var max = _allSampleFrequencies.Max();
                File.AppendAllText(PerfResultsPath,
                    string.Create(CultureInfo.InvariantCulture, $"Average,{avg},\nMin,{min},\nMax,{max},\n"));

                Log.Info("Performance test complete — avg {Avg} Hz (min {Min}, max {Max}) — {Path}",
                    avg, min, max, PerfResultsPath);
                TestComplete = true;
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to write performance sample: {Error}", ex.Message);
        }
    }

    public void SampleCpu(long totalCyclesExecuted, long targetFrequencyHz)
    {
        _totalCyclesExecuted = totalCyclesExecuted;
        _targetFrequencyHz = targetFrequencyHz;

        // During warmup, just track cycles but don't snapshot
        if (_warmupStopwatch is not null && !_warmupComplete)
        {
            if (_warmupStopwatch.Elapsed < _warmupDuration)
            {
                // Keep resetting the snapshot window so the first real sample is clean
                _lastCycleCount = totalCyclesExecuted;
                _snapshotStopwatch.Restart();
                _logStopwatch.Restart();
                return;
            }

            _warmupComplete = true;
            _lastCycleCount = totalCyclesExecuted;
            _lastFrameCount = 0;
            _snapshotStopwatch.Restart();
            _logStopwatch.Restart();
            Log.Info("Warmup complete. Now collecting {Count} performance samples", _testSampleCount);
        }

        if (_snapshotStopwatch.Elapsed >= _snapshotInterval)
            CalculateSnapshot();
    }

    public void FrameRendered()
    {
        _lastFrameCount++;
    }
}