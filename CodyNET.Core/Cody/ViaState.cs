namespace CodyNET.Core.Cody;

public record ViaState
{
    public byte[] Registers { get; init; } = [];
    public byte T1LatchLo { get; init; }
    public byte T1LatchHi { get; init; }
    public ushort T1Counter { get; init; }
    public bool T1Enabled { get; init; }
    public byte T2LatchLo { get; init; }
    public byte T2LatchHi { get; init; }
    public ushort T2Counter { get; init; }
    public bool T2Enabled { get; init; }
    public byte Ifr { get; init; }
    public byte Ier { get; init; }
    public long LastUpdateCycle { get; init; }
}
