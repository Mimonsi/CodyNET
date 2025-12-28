using System.Text.Json;

namespace CodyPrototype.Tests;

[TestFixture]
public class SingleStepTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        Converters = { new CycleRawConverter() }
    };

    private static string GetTestDataFolder()
    {
        string folder = Path.Combine(TestContext.CurrentContext.TestDirectory, "wdc65c02", "v1");

        if (!Directory.Exists(folder))
        {
            Assert.Inconclusive($"Test data not found. Ensure the 65x02 SingleStepTests data exists at '{folder}'.");
        }

        return folder;
    }

    [TestCaseSource(nameof(LoadAllSingleStepTests))]
    public void TestAllBytecodes(SingleStepTest test)
    {
        Console.WriteLine("Running test: " + test.TestName);
        var cpu = Cpu.FromState(test.Initial);
        
        cpu.RunUntilFinish();
        
        Assert.That(cpu.GetState(), Is.EqualTo(test.Final));
    }
    
    public static IEnumerable<TestCaseData> CasesForBytecode => LoadSingleStepTestsBytecode("18");
    [TestCaseSource(nameof(CasesForBytecode))]
    public void TestBytecode(SingleStepTest test)
    {
        Console.WriteLine("Running test: " + test.TestName);
        var cpu = Cpu.FromState(test.Initial);
        
        cpu.RunUntilFinish();
        
        Assert.That(cpu.GetState(), Is.EqualTo(test.Final));
    }

    private static IEnumerable<TestCaseData> LoadAllSingleStepTests()
    {
        string folder = GetTestDataFolder();
        foreach (var file in Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories))
        {
            string bytecode = Path.GetFileNameWithoutExtension(file);
            foreach (var test in LoadSingleStepTestsBytecode(bytecode))
            {
                yield return test;
            }
        }
    }
    
    private static IEnumerable<TestCaseData> LoadSingleStepTestsBytecode(string bytecode)
    {
        string folder = GetTestDataFolder();
        var path = Path.Combine(folder, bytecode + ".json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            if (string.IsNullOrEmpty(json))
                yield break;
            
            var testsInFile = JsonSerializer.Deserialize<RawSingleStepTest[]>(json, JsonOptions);

            if (testsInFile == null || testsInFile.Length == 0)
            {
                Console.WriteLine("No tests found in file: " + path);
                yield break;
            }

            foreach (var raw in testsInFile)
            {
                var test = new SingleStepTest
                {
                    Initial = ConvertState(raw.Initial),
                    Final = ConvertState(raw.Final),
                    TestName = Path.GetFileNameWithoutExtension(raw.Name)
                };
                
                yield return new TestCaseData(test).SetName(test.TestName);
            }
        }
    }

    private static IEnumerable<TestCaseData> LoadSingleStepTests()
    {
        string folder = GetTestDataFolder();

        foreach (var file in Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories))
        {
            string json = File.ReadAllText(file);
            if (string.IsNullOrEmpty(json))
                continue;

            var testsInFile = JsonSerializer.Deserialize<RawSingleStepTest[]>(json, JsonOptions);

            if (testsInFile == null || testsInFile.Length == 0)
            {
                Console.WriteLine("No tests found in file: " + file);
                continue;
            }

            foreach (var raw in testsInFile)
            {
                var test = new SingleStepTest
                {
                    Initial = ConvertState(raw.Initial),
                    Final = ConvertState(raw.Final),
                    TestName = raw.Name
                };
                
                yield return new TestCaseData(test).SetName(test.TestName);
            }
        }
    }
    
    private static CpuState ConvertState(RawCpuState raw)
    {
        var state = new CpuState
        {
            A = raw.a,
            X = raw.x,
            Y = raw.y,
            PC = raw.pc,
            S = raw.s,
            P = raw.p
            
        };

        foreach (var kv in raw.ram)
        {
            ushort addr = kv[0];
            byte val = (byte)kv[1];
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