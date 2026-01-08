namespace CodyPrototype.Utils;

public class Math
{
    // Input example: 1536, "Hz" => Output: "1.536 kHz"
    public static string AdjustSLI(double number, string unit)
    {
        if (number >= 1_000_000_000)
            return (number / 1_000_000_000.0).ToString("0.###") + " G" + unit;
        if (number >= 1_000_000)
            return (number / 1_000_000.0).ToString("0.###") + " M" + unit;
        if (number >= 1_000)
            return (number / 1_000.0).ToString("0.###") + " k" + unit;
        return number + " " + unit;
    }
}