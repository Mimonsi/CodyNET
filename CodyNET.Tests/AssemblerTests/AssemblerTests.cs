using CodyNET.Assembler;
using CodyNET.Disassembler;
using NUnit.Framework;

namespace CodyNET.Tests.AssemblerTests;

/// <summary>
/// These tests require 64tass assembler to be installed and accessible via the path specified in TassAssembler.cs
/// </summary>
public class AssemblerTests
{
    [Test]
    public void TestAssembler()
    {
        var bytes = TassAssembler.AssembleFile(FileUtils.GetTestDataPath("minimal.s"));
        var expectedBytes = FileUtils.GetBytesFromFile(FileUtils.GetTestDataPath("minimal.bin"));
        Assert.AreEqual(bytes, expectedBytes);
    }
    
    [Test]
    public void TestFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() => TassAssembler.AssembleFile(FileUtils.GetTestDataPath("not-existing.s")));
    }

    [Test]
    public void TestInvalidOpcode()
    {
        Assert.Throws<InvalidOperationException>(() => TassAssembler.AssembleFile(FileUtils.GetTestDataPath("invalidOpcode.s")));
    }

    [Test]
    public void TestUnmappedArea()
    {
        var bytes = TassAssembler.AssembleFile(FileUtils.GetTestDataPath("debugTesting/unmappedArea.s"));
        Assert.True(true);
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
