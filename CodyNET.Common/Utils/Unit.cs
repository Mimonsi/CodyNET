namespace CodyNET.Common.Utils;

public class Unit
{
    // Input example: 1536, "Hz" => Output: "1.536 kHz"
    public static string FormatSi(double number, string unit)
    {
        if (number < 0)
            return "MAX";
        if (number >= 1_000_000_000)
            return (number / 1_000_000_000.0).ToString("0.###") + " G" + unit;
        if (number >= 1_000_000)
            return (number / 1_000_000.0).ToString("0.###") + " M" + unit;
        if (number >= 1_000)
            return (number / 1_000.0).ToString("0.###") + " k" + unit;
        return number + " " + unit;
    }

    // Parses strings like "3.5 MHz", "500kHz", "500k", "3.5m", "1000000" into Hz (as long).
    // Returns false if the input cannot be parsed.
    public static bool TryParseSi(string? input, string unit, out long result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var s = input.Trim();

        // Strip trailing unit suffix (case-insensitive), e.g. "Hz", "hz"
        if (s.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            s = s[..^unit.Length].TrimEnd();

        double multiplier = 1.0;

        if (s.EndsWith("G", StringComparison.OrdinalIgnoreCase)) { multiplier = 1_000_000_000.0; s = s[..^1].TrimEnd(); }
        else if (s.EndsWith("M", StringComparison.OrdinalIgnoreCase)) { multiplier = 1_000_000.0; s = s[..^1].TrimEnd(); }
        else if (s.EndsWith("k", StringComparison.OrdinalIgnoreCase)) { multiplier = 1_000.0; s = s[..^1].TrimEnd(); }

        if (!double.TryParse(s, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var number))
            return false;

        result = (long)Math.Round(number * multiplier);
        return result > 0;
    }
}