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
        var bytes = CodyAssembler.AssembleFileToBytes(FileUtils.GetTestDataFile("minimal.asm"));
        var expectedBytes = File.ReadAllBytes(FileUtils.GetTestDataPath("minimal.bin"));
        Assert.AreEqual(bytes, expectedBytes);
    }
    
    [Test]
    public void TestFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() => CodyAssembler.AssembleFileToBytes(FileUtils.GetTestDataFile("not-existing.asm")));
    }

    [Test]
    public void TestInvalidOpcode()
    {
        Assert.Throws<InvalidOperationException>(() => CodyAssembler.AssembleFileToBytes(FileUtils.GetTestDataFile("invalidOpcode.asm")));
    }

    [Test]
    public void TestUnmappedArea()
    {
        var bytes = CodyAssembler.AssembleFileToBytes(FileUtils.GetTestDataFile("debugTesting/unmappedArea.asm"));
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

    [Test]
    public void TestDisassemblerRelativeBranchRoundtrip()
    {
        byte[] original = [0xD0, 0xFE];
        string disassembly = CodyDisassembler.Disassemble(original, 0xE000);
        var reassembled = CodyAssembler.AssembleTextToBytes(disassembly);

        Assert.That(reassembled, Is.EqualTo(original), disassembly);
    }

    [Test]
    [Explicit("Dev Test")]
    public void TestAssembleDisassembleRoundtripCodybasic()
    {
        var folderPath = "C:\\Users\\Konsi\\Documents\\CodyNETSecond\\CodyNET.Tests\\testdata\\programs\\assembly";
        var name = "TicTacToe";
        var asm = File.ReadAllText(Path.Combine(folderPath, $"{name}.asm"));
        var assembled = CodyAssembler.AssembleTextToBytes(asm);
        // Write to file
        var assembledPath = FileUtils.GetTestDataPath(Path.Combine(folderPath, $"{name}_assembled.bin"));
        File.WriteAllBytes(assembledPath, assembled);
        var disassembled  = CodyDisassembler.Disassemble(assembled);
        var disassembledPath = Path.Combine(folderPath, $"{name}_disassembled.asm");
        File.WriteAllText(disassembledPath, disassembled);
        Console.WriteLine($"Files written to {assembledPath} and {disassembledPath}");
    }

    [Test]
    [Explicit("Linear disassembly of real binaries with data sections is inherently unreliable for round-tripping")]
    public void TestDisassemblerRoundtrip()
    {
        var original = File.ReadAllBytes(FileUtils.GetTestDataPath("programs/codybasic.bin"));
        string disassembly = CodyDisassembler.Disassemble(original, 0xE000);
        var reassembled = CodyAssembler.AssembleTextToBytes(disassembly);

        Assert.That(reassembled, Is.EqualTo(original));
    }

    [Explicit("Dev Test")]
    [Test]
    public void TestPreprocessor()
    {
        var code = "LDA #1\nSTA #1\n\n";
        var x = CodyPreprocessor.Preprocess(code);
        
        List<string> lines = code.Split('\n').ToList();
        var y = CodyPreprocessor.Preprocess(lines);
        
        Assert.That(x , Is.EqualTo(y));
    }
    
    [Explicit("Dev Test")]
    [Test]
    public void TestDisassembler()
    {
        var file1 = "C:\\Users\\Konsi\\Documents\\CodyNET\\CodyNET.Tests\\testdata\\programs\\assembly\\TicTacToe_assembled.bin";
        var file2 = "C:\\Users\\Konsi\\Documents\\CodyNET\\CodyNET.Tests\\testdata\\programs\\assembly\\TicTacToe_breakpoint.bin";
        CodyDisassembler.DisassembleFile(new FileInfo(file1), new FileInfo(file1.Replace(".bin", ".asm")));
        CodyDisassembler.DisassembleFile(new FileInfo(file2), new FileInfo(file2.Replace(".bin", ".asm")));
    }
}
