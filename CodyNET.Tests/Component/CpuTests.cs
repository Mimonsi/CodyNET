using CodyNET.Cody;

namespace CodyNET.Tests.Component;

public class CpuTests
{
    [Fact]
    public void TestLoadRam()
    {
        byte[] ram = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        ushort address = 0x3000;
        
        Cpu cpu = new Cpu();
        cpu.LoadRam(ram, address);
        
        Assert.Equal(0x01, cpu.Memory.Read(0x3000));
        Assert.Equal(0x02, cpu.Memory.Read(0x3001));
        Assert.Equal(0x03, cpu.Memory.Read(0x3002));
        Assert.Equal(0x04, cpu.Memory.Read(0x3003));
    }
}