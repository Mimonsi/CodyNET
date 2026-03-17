namespace CodyNET.Core.Cody;

public enum RunStatus
{
    Running,
    Paused,
    Breakpoint
}

public sealed record CodyStatusSnapshot
{
    public required RunStatus RunStatus { get; init; }
    public required ProfilerSnapshot? ProfilerSnapshot { get; init; }
}
