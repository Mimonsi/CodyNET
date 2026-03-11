using System.Globalization;

namespace CodyNET.Host;

public static class CliValueParser
{
    public static long ParseClock(string? clock)
    {
        if (string.IsNullOrWhiteSpace(clock))
            return 1_000_000;

        var text = clock.Trim().ToLowerInvariant();
        long multiplier;

        if (text.EndsWith("mhz", StringComparison.Ordinal))
        {
            multiplier = 1_000_000;
            text = text[..^3];
        }
        else if (text.EndsWith("khz", StringComparison.Ordinal))
        {
            multiplier = 1_000;
            text = text[..^3];
        }
        else if (text.EndsWith("hz", StringComparison.Ordinal))
        {
            multiplier = 1;
            text = text[..^2];
        }
        else
        {
            multiplier = 1;
        }

        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value) && value > 0)
            return checked(value * multiplier);

        throw new ArgumentException($"Invalid clock format: {clock}");
    }

    public static ushort? ParseOptionalAddress(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return ParseAddress(value, paramName);
    }

    public static ushort ParseAddress(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Missing 16-bit address for {paramName}.");

        var text = value.Trim();

        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return ParseHex(text[2..], value, paramName);

        if (text.StartsWith('$'))
            return ParseHex(text[1..], value, paramName);

        if (ContainsHexLetter(text))
            return ParseHex(text, value, paramName);

        if (ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort parsedDecimal))
            return parsedDecimal;

        throw new ArgumentException($"Invalid 16-bit address for {paramName}: '{value}'.");
    }

    public static FileInfo? ParseExistingFile(string? path, string paramName)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
            throw new ArgumentException($"File specified in {paramName} does not exist: '{path}'.");

        return fileInfo;
    }

    private static bool ContainsHexLetter(string text)
    {
        foreach (var ch in text)
        {
            if ((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F'))
                return true;
        }

        return false;
    }

    private static ushort ParseHex(string text, string originalValue, string paramName)
    {
        if (ushort.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort parsed))
            return parsed;

        throw new ArgumentException($"Invalid 16-bit address for {paramName}: '{originalValue}'.");
    }
}
