namespace CodyPrototype.singleStepTests;

public sealed class RawCpuState
{
    // Registers (hex-coded strings)
    public string pc { get; set; } = "";
    public string a  { get; set; } = "";
    public string x  { get; set; } = "";
    public string y  { get; set; } = "";
    public string sp { get; set; } = "";
    public string p  { get; set; } = "";

    // Memory overrides (hex-coded strings)
    public Dictionary<string, string> ram { get; set; } = new();
}
