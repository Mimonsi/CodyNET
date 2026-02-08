using CodyNET.Core;
using NUnit.Framework;

namespace CodyNET.Tests.Program;

public class CliTests
{
    [Test]
    public void TestCli()
    {
        string input = "assemble test.asm";
        var result = Cli.BuildRootCommand().Parse(input);
        Assert.That(result.Errors, Is.Empty);
    }
}