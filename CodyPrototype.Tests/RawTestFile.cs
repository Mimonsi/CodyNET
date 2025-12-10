using System.Text.Json.Serialization;

namespace CodyPrototype.Tests;

public sealed class RawTestFile
{
    [JsonPropertyName("initial")]
    public RawCpuState Initial { get; set; } = null!;

    [JsonPropertyName("expected")]
    public RawCpuState Expected { get; set; } = null!;
}