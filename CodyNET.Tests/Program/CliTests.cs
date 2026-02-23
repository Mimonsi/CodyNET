using CodyNET.Core;
using CodyNET.Host;
using NUnit.Framework;

namespace CodyNET.Tests.Program;

public class CliTests
{
    [Explicit]
    [Test]
    public void TestCli()
    {
        string input = "assemble test.asm";
        var result = Cli.BuildRootCommand().Parse(input);
        Assert.That(result.Errors, Is.Empty);
    }

    public void TestBootDefaults()
    {
        string input = "boot";
        // TODO: How to test executed logic?
         var result = Cli.BuildRootCommand().Parse(input);
         Assert.That(result.Errors, Is.Empty);
    }
}