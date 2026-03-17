namespace CodyNET.Core.Cody;

public sealed record CpuRegisterSnapshot
{
    public required byte A { get; init; }
    public required byte X { get; init; }
    public required byte Y { get; init; }
    public required byte S { get; init; }
    public required byte P { get; init; }
    public required ushort PC { get; init; }
    public required bool Carry { get; init; }
    public required bool Zero { get; init; }
    public required bool InterruptDisable { get; init; }
    public required bool Decimal { get; init; }
    public required bool Break { get; init; }
    public required bool Overflow { get; init; }
    public required bool Negative { get; init; }
}
