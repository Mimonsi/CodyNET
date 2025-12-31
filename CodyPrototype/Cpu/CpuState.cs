namespace CodyPrototype.Cpu;

/// <summary>
/// A snapshot of the CPU state for testing
/// </summary>
public sealed class CpuState
{
    public ushort PC { get; set; }
    public byte A { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte S { get; set; } // Stack Pointer
    public byte P { get; set; } // Status register

    /// <summary>
    /// Memory overrides: dictionary of address->value
    /// Only the given addresses differ from some base test RAM file.
    /// </summary>
    public Dictionary<ushort, byte> Memory { get; set; } = new();
}