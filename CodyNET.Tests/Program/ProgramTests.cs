using CodyNET.Assembler;
using CodyPrototype.Utils;

using NUnit.Framework;

namespace CodyNET.Tests.Program;
using CodyNET.Cody;

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
    }
}
