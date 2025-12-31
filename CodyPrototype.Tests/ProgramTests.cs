using CodyPrototype.Assembler;
using CodyPrototype.Cpu;

namespace CodyPrototype.Tests;

public class ProgramTests
{
    [Test]
    public void TestMisc()
    {

    }
    
    [Test]
    public void TestAssembler()
    {
        ICodyAssembler assembler = new TassAssembler();
        var bytes = assembler.AssembleFile("testdata/minimal.s");
        var expectedBytes = GetBytesFromFile("testdata/minimal.bin");
        Assert.That(bytes, Is.EqualTo(expectedBytes));
    }
    
    private static byte[] GetBytesFromFile(string filePath)
    {
        // Remove all strings and new lines
        var content = File.ReadAllText(filePath);
        var byteStrings = content.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);
        var bytes = new byte[byteStrings.Length];
        for (int i = 0; i < byteStrings.Length; i++)
        {            
            bytes[i] = Convert.ToByte(byteStrings[i], 16);
        }
        return bytes;
    }
    
    
    [Test]
    public void TestMinimal()
    {
        Cpu.Cpu cpu = new();
        var bytes = GetBytesFromFile("testdata/minimal.bin");
        cpu.LoadProgram(bytes, 0x0600);
        cpu.RunUntilFinish();
        var state = cpu.GetState();

        var finalState = new CpuState()
        {
            A = 8,
            X = 0,
            Y = 0,
            PC = 0x0610,
            S = 0xFF,
            P = 0,
            Memory = new Dictionary<ushort, byte>()
            {
                { 0x200, 0x1 },
                { 0x201, 0x5 },
                { 0x202, 0x8 },
            }
        };
        var (success, message) = finalState.CheckState(state);
        Assert.IsTrue(success, message);
    }
}