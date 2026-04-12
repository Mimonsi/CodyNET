using CodyNET.Host;
using NUnit.Framework;

namespace CodyNET.Tests.Program;

public class CliValueParserTests
{
    // ── ParseClock ────────────────────────────────────────────────────────────

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ParseClockNullOrEmptyReturnsOneMhz(string? input)
    {
        Assert.AreEqual(1_000_000, CliValueParser.ParseClock(input));
    }

    [TestCase("2mhz", 2_000_000L)]
    [TestCase("1MHz", 1_000_000L)]
    [TestCase("500khz", 500_000L)]
    [TestCase("1KHz", 1_000L)]
    [TestCase("60hz", 60L)]
    [TestCase("1Hz", 1L)]
    [TestCase("1000", 1000L)]
    public void ParseClockParsesValidValues(string input, long expected)
    {
        Assert.AreEqual(expected, CliValueParser.ParseClock(input));
    }

    [TestCase("0mhz")]
    [TestCase("abc")]
    [TestCase("-1mhz")]
    [TestCase("1.5mhz")]
    public void ParseClockThrowsOnInvalidInput(string input)
    {
        Assert.Throws<ArgumentException>(() => CliValueParser.ParseClock(input));
    }

    // ── ParseAddress ─────────────────────────────────────────────────────────

    [TestCase("$E000", (ushort)0xE000)]
    [TestCase("0xE000", (ushort)0xE000)]
    [TestCase("0xe000", (ushort)0xE000)]
    [TestCase("57344", (ushort)0xE000)]  // decimal 0xE000
    [TestCase("DEAD", (ushort)0xDEAD)]  // hex letters -> auto-hex
    [TestCase("$0000", (ushort)0x0000)]
    [TestCase("$FFFF", (ushort)0xFFFF)]
    public void ParseAddressParsesValidValues(string input, ushort expected)
    {
        Assert.AreEqual(expected, CliValueParser.ParseAddress(input, "test"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ParseAddressThrowsOnNullOrEmpty(string? input)
    {
        Assert.Throws<ArgumentException>(() => CliValueParser.ParseAddress(input, "test"));
    }

    [TestCase("$FFFFF")]   // too large
    [TestCase("xyz")]
    [TestCase("99999")]    // too large for ushort
    public void ParseAddressThrowsOnInvalidInput(string input)
    {
        Assert.Throws<ArgumentException>(() => CliValueParser.ParseAddress(input, "test"));
    }

    // ── ParseOptionalAddress ──────────────────────────────────────────────────

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ParseOptionalAddressReturnsNullForEmpty(string? input)
    {
        Assert.IsNull(CliValueParser.ParseOptionalAddress(input, "test"));
    }

    [Test]
    public void ParseOptionalAddressReturnsValueWhenProvided()
    {
        Assert.AreEqual((ushort)0x1000, CliValueParser.ParseOptionalAddress("$1000", "test"));
    }

    // ── ParseExistingFile ─────────────────────────────────────────────────────

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ParseExistingFileReturnsNullForEmpty(string? input)
    {
        Assert.IsNull(CliValueParser.ParseExistingFile(input, "test"));
    }

    [Test]
    public void ParseExistingFileThrowsWhenFileDoesNotExist()
    {
        Assert.Throws<ArgumentException>(() =>
            CliValueParser.ParseExistingFile("C:\\does\\not\\exist.bin", "test"));
    }

    [Test]
    public void ParseExistingFileReturnsFileInfoForExistingFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var result = CliValueParser.ParseExistingFile(tempFile, "test");
            Assert.IsNotNull(result);
            Assert.AreEqual(tempFile, result!.FullName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
