namespace CodyPrototype.Tests;

public sealed class RawCpuState
{
    public ushort pc { get; set; }
    public byte a  { get; set; }
    public byte x  { get; set; }
    public byte y  { get; set; }
    public byte s { get; set; }
    public byte p  { get; set; }
    
    public List<ushort[]> ram { get; set; } = new();
}
