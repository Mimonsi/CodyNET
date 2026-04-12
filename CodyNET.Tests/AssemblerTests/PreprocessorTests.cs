using CodyNET.Assembler;
using NUnit.Framework;

namespace CodyNET.Tests.AssemblerTests;

public class PreprocessorTests
{
    [Test]
    public void DbpReplacedWithStz()
    {
        var result = CodyPreprocessor.Preprocess("DBP");
        Assert.That(result.Trim(), Is.EqualTo("STZ $FF00"));
    }

    [Test]
    public void DbpReplacementIsCaseInsensitive()
    {
        var lower = CodyPreprocessor.Preprocess("dbp").Trim();
        var upper = CodyPreprocessor.Preprocess("DBP").Trim();
        var mixed = CodyPreprocessor.Preprocess("Dbp").Trim();
        Assert.AreEqual("STZ $FF00", lower);
        Assert.AreEqual("STZ $FF00", upper);
        Assert.AreEqual("STZ $FF00", mixed);
    }

    [Test]
    public void DbpWithLeadingWhitespacePreservesIndent()
    {
        var result = CodyPreprocessor.Preprocess("  DBP");
        Assert.That(result.Trim(), Is.EqualTo("STZ $FF00"));
        Assert.That(result, Does.StartWith("  STZ"));
    }

    [Test]
    public void NonDbpLinesPassThrough()
    {
        var code = "LDA #$01\nSTA $0200";
        var result = CodyPreprocessor.Preprocess(code);
        Assert.That(result, Does.Contain("LDA #$01"));
        Assert.That(result, Does.Contain("STA $0200"));
    }

    [Test]
    public void DbpAsSubstringIsNotReplaced()
    {
        // "DBPORT" should not be replaced — the regex uses \b word boundary
        var result = CodyPreprocessor.Preprocess("DBPORT");
        Assert.That(result, Does.Not.Contain("STZ"));
    }

    [Test]
    public void StringAndListOverloadProduceSameResult()
    {
        var code = "LDA #1\nDBP\nSTA $0200\n\n";
        var fromString = CodyPreprocessor.Preprocess(code);
        var fromList = CodyPreprocessor.Preprocess(code.Split('\n').ToList());
        Assert.AreEqual(fromString, fromList);
    }

    [Test]
    public void MultipleDbpLinesAreAllReplaced()
    {
        var code = "DBP\nLDA #1\nDBP";
        var result = CodyPreprocessor.Preprocess(code);
        var occurrences = result.Split(new[] { "STZ $FF00" }, StringSplitOptions.None).Length - 1;
        Assert.AreEqual(2, occurrences);
    }

    [Test]
    public void EmptyStringProducesNewline()
    {
        var result = CodyPreprocessor.Preprocess("");
        Assert.AreEqual("\n", result);
    }
}
