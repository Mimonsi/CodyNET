using CodyNET.Assembler;
using CodyNET.Utils;
using CodyPrototype.Utils;
using NUnit.Framework;

namespace CodyNET.Tests.AssemblerTests;

public class AssemblerTests
{
    [Test]
    public void TestAssembler()
    {
        ICodyAssembler assembler = new TassAssembler();
        var bytes = assembler.AssembleFile(FileUtils.GetTestDataPath("minimal.s"));
        var expectedBytes = FileUtils.GetBytesFromFile(FileUtils.GetTestDataPath("minimal.bin"));
        Assert.AreEqual(bytes, expectedBytes);
    }
    
    [Test]
    public void TestFileNotFound()
    {
        var assembler = new TassAssembler();
        Assert.Throws<FileNotFoundException>(() => assembler.AssembleFile(FileUtils.GetTestDataPath("not-existing.s")));
    }

    [Test]
    public void TestInvalidOpcode()
    {
        var assembler = new TassAssembler();
        Assert.Throws<InvalidOperationException>(() => assembler.AssembleFile(FileUtils.GetTestDataPath("invalidOpcode.s")));
    }

    [Test]
    public void TestDisassemblerSingle()
    {
        var result = CodyDisassembler.Disassemble([0x69, 0x53, 0x00]);
        result = result.Replace("\r\n", "\n").TrimEnd();
        var expected = "ADC #$53\nBRK";
        Assert.AreEqual(expected, result);
    }
    
    [Test]
    public void TestDisassemblerMinimal()
    {
        var result = CodyDisassembler.Disassemble([0xA9, 0x01 , 0x8D , 0x00 , 0x02 , 0xA9 , 0x05 , 0x8D , 0x01 , 0x02 , 0xA9 , 0x08 , 0x8D , 0x02 , 0x02]);
        result = result.Replace("\r\n", "\n").TrimEnd();
        var expected = "LDA #$01\nSTA $0200\nLDA #$05\nSTA $0201\nLDA #$08\nSTA $0202";
        Assert.AreEqual(expected, result);
    }
}
