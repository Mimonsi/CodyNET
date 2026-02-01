using CodyNET.Assembler;
using CodyPrototype.Utils;

namespace CodyNET.Tests.Program;
using CodyNET.Cody;

/// <summary>
/// Full program tests. Set a specific cpu and memory state, run the program and verify the end state.
/// </summary>
public class ProgramTests
{
    [Fact]
    public void MinimalProgram()
    {
        Cody cody = new Cody();
        cody.RunAssemblyFile(FileUtils.GetTestDataPath("minimal.s"));
    }
}