using CodyPrototype.assembler;

namespace CodyPrototype.Tests;

public class ProgramTests
{
    [Test]
    public void TestAssembler()
    {
        ICodyAssembler assembler = new TassAssembler();
        var bytes = assembler.AssembleFile("testdata/minimal.asm");
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
        Cpu cpu = new();
        var bytes = File.ReadAllBytes("testdata/minimal.bin");
        cpu.LoadProgram(bytes, 0x0600);
        Assert.Pass();
    }
}