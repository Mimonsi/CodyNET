using CodyNET.Assembler;
using CodyNET.Utils;
using CodyPrototype.Utils;

namespace CodyNET.Tests.AssemblerTests;

public class AssemblerTests
{
    [Fact]
    public void TestAssembler()
    {
        ICodyAssembler assembler = new TassAssembler();
        var bytes = assembler.AssembleFile(FileUtils.GetTestDataPath("minimal.s"));
        var expectedBytes = FileUtils.GetBytesFromFile(FileUtils.GetTestDataPath("minimal.bin"));
        Assert.Equal(bytes, expectedBytes);
    }
    
    [Fact]
    public void TestFileNotFound()
    {
        var assembler = new TassAssembler();
        Assert.Throws<FileNotFoundException>(() => assembler.AssembleFile(FileUtils.GetTestDataPath("not-existing.s")));
    }

    [Fact]
    public void TestInvalidOpcode()
    {
        var assembler = new TassAssembler();
        Assert.Throws<InvalidOperationException>(() => assembler.AssembleFile(FileUtils.GetTestDataPath("invalidOpcode.s")));
    }
}