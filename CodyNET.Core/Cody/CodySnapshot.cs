namespace CodyNET.Core.Cody;

public record CodySnapshot
{
    public string Version { get; init; } = "1";
    public DateTime SavedAt { get; init; } = DateTime.UtcNow;
    public long FrequencyHz { get; init; }
    public CpuState CpuState { get; init; } = null!;
    public ViaState ViaState { get; init; } = null!;
    public UartState Uart1State { get; init; } = null!;
    public UartState Uart2State { get; init; } = null!;
    public VidState? VidState { get; init; }
}
