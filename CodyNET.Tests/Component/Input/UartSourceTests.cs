using CodyNET.Common;
using NUnit.Framework;

namespace CodyNET.Tests.Component.Input;

public class UartSourceTests
{
    [Test]
    public void FromFile_WithNormalizeLineEndingsFalse_PreservesRawBytes()
    {
        string path = Path.GetTempFileName();

        try
        {
            byte[] expected = [0x43, 0x0D, 0x0A, 0x00, 0xFF, 0x42];
            File.WriteAllBytes(path, expected);

            var source = UartSource.FromFile(new FileInfo(path), normalizeLineEndings: false);
            var actual = ReadAll(source);

            Assert.That(actual, Is.EqualTo(expected));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void FromFile_WithNormalizeLineEndingsTrue_NormalizesTextAndAppendsTerminator()
    {
        string path = Path.GetTempFileName();

        try
        {
            File.WriteAllText(path, "10 PRINT \"HI\"\r\n\r\n20 END\r\n");

            var source = UartSource.FromFile(new FileInfo(path), normalizeLineEndings: true);
            var actual = ReadAll(source);

            byte[] expected = System.Text.Encoding.ASCII.GetBytes("10 PRINT \"HI\"\n20 END\n\n");
            Assert.That(actual, Is.EqualTo(expected));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static byte[] ReadAll(UartSource source)
    {
        var bytes = new List<byte>();

        while (source.Read() is { } value)
        {
            bytes.Add(value);
        }

        return bytes.ToArray();
    }
}
