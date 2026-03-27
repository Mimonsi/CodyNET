using CodyNET.Assembler;
using CodyNET.Core.Cody;
using NUnit.Framework;

namespace CodyNET.Tests.Program;

/// <summary>
/// Full program tests. Set a specific cpu and memory state, run the program and verify the end state.
/// </summary>
public class ProgramTests
{
    [Explicit]
    [Test]
    public void MinimalProgram()
    {
        Cody cody = new Cody(CodySetupOptions.Default);
        var loadOptions = new CodyLoadOptions()
        {
            File = new FileInfo(FileUtils.GetTestDataPath("minimal.asm")),
        };
        cody.RunAssemblyFile(loadOptions);
        Assert.True(true);
    }

    [Test]
    public void MiscTest()
    {

    }
}
