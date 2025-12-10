using Newtonsoft.Json;

namespace CodyPrototype.Tests;

public sealed class RawSingleStepTest
{
    [JsonProperty("initial")]
    public RawCpuState Initial { get; set; } = null!;
    [JsonProperty("final")]
    public RawCpuState Final { get; set; } = null!;
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("cycles")]
    public List<CycleRaw> Cycles { get; set; }
    
}

public class CycleRaw
{
    public int Address { get; set; }
    public int Value { get; set; }
    public string Operation { get; set; } = string.Empty;
}

