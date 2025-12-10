namespace CodyPrototype.Tests;

public sealed class SingleStepTest
{
    public CpuState Initial { get; set; } = null!;
    public CpuState Expected { get; set; } = null!;
    public string TestName { get; set; } = string.Empty;
    public int Cycles { get; set; }
    
}
