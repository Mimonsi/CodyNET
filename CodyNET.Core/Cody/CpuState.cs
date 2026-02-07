namespace CodyNET.Core.Cody;

public class CpuState
{
    public byte A;
    public byte X;
    public byte Y;
    public byte S;
    public byte P; // Status register
    public ushort PC;
    public List<int[]> Ram = [];
}