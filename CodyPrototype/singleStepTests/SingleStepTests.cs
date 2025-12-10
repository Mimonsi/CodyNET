using System.Text.Json;
using NUnit.Framework;

namespace CodyPrototype.singleStepTests;

[TestFixture]
public class SingleStepTests
{
    public static IEnumerable<TestCaseData> Cases => LoadSingleStepTests();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };


    [TestCaseSource(nameof(Cases))]
    public void ExecuteSingleStep(SingleStepTest test)
    {
        var cpu = Cpu.FromState(test.Initial);
        
        cpu.Step(); // TODO: Loop until finished
        
        Assert.That(cpu.GetState(), Is.EqualTo(test.Expected));
    }

    private static IEnumerable<TestCaseData> LoadSingleStepTests()
    {
        string folder = Path.Combine(TestContext.CurrentContext.TestDirectory, "tests", "wdc65c02");

        foreach (var file in Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories))
        {
            string json = File.ReadAllText(file);

            var raw = JsonSerializer.Deserialize<RawTestFile>(json, JsonOptions);

            if (raw == null)
                continue;

            var test = new SingleStepTest
            {
                Initial = ConvertState(raw.Initial),
                Expected = ConvertState(raw.Expected),
                TestName = Path.GetFileNameWithoutExtension(file)
            };

            yield return new TestCaseData(test).SetName(test.TestName);
        }
    }
    
    private static CpuState ConvertState(RawCpuState raw)
    {
        var state = new CpuState
        {
            PC = ParseUShort(raw.pc),
            A  = ParseByte(raw.a),
            X  = ParseByte(raw.x),
            Y  = ParseByte(raw.y),
            S = ParseByte(raw.sp),
            P  = ParseByte(raw.p)
        };

        foreach (var kv in raw.ram)
        {
            ushort addr = ParseUShort(kv.Key);
            byte val = ParseByte(kv.Value);
            state.Memory[addr] = val;
        }

        return state;
    }
    
    private static byte ParseByte(string hex)
    {
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex[2..];

        return Convert.ToByte(hex, 16);
    }

    private static ushort ParseUShort(string hex)
    {
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex[2..];

        return Convert.ToUInt16(hex, 16);
    }


}