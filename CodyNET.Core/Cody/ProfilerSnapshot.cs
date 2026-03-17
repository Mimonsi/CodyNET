namespace CodyNET.Core.Cody;

public sealed record ProfilerSnapshot
{
    public long TargetFrequency;
    public long ActualFrequency;
    public double FrequencyTargetPercent =>  (double)ActualFrequency / TargetFrequency * 100.0;
    public double TargetFrames;
    public double ActualFrames;
    public double FramesTargetPercent => TargetFrames > 0 ? (ActualFrames / TargetFrames) * 100.0 : 0;
    public double Fps => ActualFrames / SecondsElapsed;
    public double SecondsElapsed;
}
