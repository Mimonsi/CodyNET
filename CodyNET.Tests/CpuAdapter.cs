using CodyNET.Cody;

namespace CodyNET.Tests;

public class CpuState
{
    public byte A;
    public byte X;
    public byte Y;
    public byte S; // Stack Pointer
    public Status Status;
    public ushort PC; // 16 bit program counter
    public byte[] Memory = new byte[65536]; // 64KB memory
}

public class CpuAdapter
{
    public Cpu FromState()
    {
        return new Cpu();
    }
}