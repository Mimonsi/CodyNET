using CodyPrototype.Cpu;

namespace CodyPrototype.Tests;

public sealed class SingleStepTest
{
    public CpuState Initial { get; set; } = null!;
    public CpuState Final { get; set; } = null!;
    public string TestName { get; set; } = string.Empty;
    public int Cycles { get; set; }
    
}
