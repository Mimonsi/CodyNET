using CodyNET.Assembler;
using CodyNET.Core.Cody;
using CodyPrototype.Utils;

using NUnit.Framework;

namespace CodyNET.Tests.Program;

/// <summary>
/// Full program tests. Set a specific cpu and memory state, run the program and verify the end state.
/// </summary>
public class ProgramTests
{
    [Test]
    public void MinimalProgram()
    {
        Cody cody = new Cody();
        cody.RunAssemblyFile(FileUtils.GetTestDataPath("minimal.s"));
        Assert.True(true);
    }

    [Test]
    public void MiscTest()
    {

    }
}
