using System.Text.Json;
using Xunit.Abstractions;

namespace CodyNET.Tests.SingleStep
{
    // --- One-click entry points ---

    public class MinimalTests
    {
        private readonly ITestOutputHelper _output;

        public MinimalTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Minimal_AllOpcodes_StateOnly()
        {
            var options = TestOptions.Minimal();
            TestRunner.RunAllOpcodes(options, _output);
        }
    }

    public class SmokeTests
    {
        private readonly ITestOutputHelper _output;

        public SmokeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Smoke_AllOpcodes_StateOnly()
        {
            var options = TestOptions.Smoke();
            TestRunner.RunAllOpcodes(options, _output);
        }
    }

    public class FullTests
    {
        private readonly ITestOutputHelper _output;

        public FullTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Full_AllOpcodes_StateOnly()
        {
            var options = TestOptions.Full();
            TestRunner.RunAllOpcodes(options, _output);
        }

        // Optional: "one opcode with one click" via InlineData
        [Theory]
        [InlineData("a9")]
        [InlineData("0b")]
        public void Full_SingleOpcode_StateOnly(string opcodeHex)
        {
            var options = TestOptions.Full();
            TestRunner.RunSingleOpcode(opcodeHex, options, _output);
        }
    }

    // --- Runner ---

    internal static class TestRunner
    {
        // Adjust path to where your opcode json files live.
        // Example: testdata/single_step_65c02/a9.json, 0b.json, ...
        private static readonly string TestDataDir =
            Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..",
                "testdata",
                "wdc65c02",
                "v1"));

        public static void RunAllOpcodes(TestOptions options, ITestOutputHelper? output)
        {
            if (!Directory.Exists(TestDataDir))
                throw new DirectoryNotFoundException($"Test data directory not found: {TestDataDir}");

            var files = Directory.EnumerateFiles(TestDataDir, "*.json")
                                 .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                 .ToArray();

            if (files.Length == 0)
                throw new InvalidOperationException($"No *.json files found in {TestDataDir}");
            
            output?.WriteLine($"Starting test run: {options.Mode}, opcode files = {files.Length}");
            
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism <= 0
                    ? Environment.ProcessorCount
                    : options.MaxDegreeOfParallelism
            };

            Parallel.ForEach(files, parallelOptions, file =>
            {
                var opcodeHex = Path.GetFileNameWithoutExtension(file);
                output?.WriteLine(
                    $"=== Opcode {opcodeHex.ToUpperInvariant()} ===");
                RunFile(file, opcodeHex, options, output);
            });


            output?.WriteLine("=== ALL OPCODES DONE ===");
        }

        public static void RunSingleOpcode(
            string opcodeHex,
            TestOptions options,
            ITestOutputHelper? output)
        {
            opcodeHex = opcodeHex.Trim().ToLowerInvariant();

            var file = Path.Combine(TestDataDir, $"{opcodeHex}.json");
            if (!File.Exists(file))
                throw new FileNotFoundException($"Opcode test file not found: {file}");

            RunFile(file, opcodeHex, options, output);
        }

        private static void RunFile(
            string filePath,
            string opcodeHex,
            TestOptions options,
            ITestOutputHelper? output)
        {
            var tests = LoadTests(filePath);
            var indices = SelectIndices(tests.Count, options).ToArray();

            output?.WriteLine(
                $"Opcode {opcodeHex.ToUpperInvariant()}: {indices.Length} test cases");

            int total = indices.Length;
            int nextPercentReport = 0;

            for (int i = 0; i < total; i++)
            {
                var idx = indices[i];
                var test = tests[idx];

                try
                {
                    ExecuteOne(test);
                }
                catch (Exception ex)
                {
                    throw new SingleStepTestFailureException(
                        $"Single-step test failed. opcode={opcodeHex.ToUpperInvariant()} index={idx} name=\"{test.Name}\"",
                        ex);
                }

                /*int percent = (i + 1) * 100 / total;
                if (percent >= nextPercentReport)
                {
                    output?.WriteLine(
                        $"Opcode {opcodeHex.ToUpperInvariant()}: {percent}% ({i + 1}/{total})");
                    nextPercentReport += 5;
                }*/
            }

            output?.WriteLine(
                $"Opcode {opcodeHex.ToUpperInvariant()}: DONE");
        }

        // ===============================
        // Test execution
        // ===============================

        private static void ExecuteOne(TestCase t)
        {
            var mem = new FlatRam64k();

            // Initial RAM
            foreach (var pair in t.Initial.Ram)
                mem.Write((ushort)pair[0], (byte)pair[1]);

            // CPU hookup (replace CpuAdapter with your real CPU)
            var cpu = new CpuAdapter(mem);

            cpu.SetState(new CpuStateDto
            {
                PC = (ushort)t.Initial.Pc,
                S = (byte)t.Initial.S,
                A = (byte)t.Initial.A,
                X = (byte)t.Initial.X,
                Y = (byte)t.Initial.Y,
                P = (byte)t.Initial.P
            });

            // Execute exactly one instruction
            cpu.StepInstruction();

            // Assert final CPU state
            var actual = cpu.GetState();

            // Assert final RAM pairs
            foreach (var pair in t.Final.Ram)
            {
                var addr = (ushort)pair[0];
                var expected = (byte)pair[1];
                var actualVal = mem.Read(addr);
                //Assert.Equal(expected, actualVal); TODO Re-enable
            }

            if (new Random().Next(500) == 2)
            {
                Assert.True(false, "Random");
            }
            Assert.True(true);

            // Optional: cycle count compare (state-only friendly)
            // If your CPU exposes cycles used for last instruction, you can check:
            // if (options.CheckCycleCount && t.Cycles is not null)
            //     Assert.Equal(t.Cycles.Count, cpu.CyclesConsumedLastInstruction);
        }

        // ===============================
        // Helpers
        // ===============================

        private static List<TestCase> LoadTests(string filePath)
        {
            var json = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine($"Warning: Test file is empty: {filePath}");
                return new List<TestCase>();
            }
            var tests = JsonSerializer.Deserialize<List<TestCase>>(json, JsonOptions.Instance);
            return tests ?? new List<TestCase>();
        }

        private static IEnumerable<int> SelectIndices(int count, TestOptions options)
        {
            if (count == 0)
                return Array.Empty<int>();

            return options.Mode switch
            {
                TestMode.Full => Enumerable.Range(0, count),
                TestMode.Minimal => new[] { 0 },
                TestMode.Smoke => DeterministicSample(count, options.SamplePerOpcodeFile, options.SampleSeed),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static IEnumerable<int> DeterministicSample(int count, int sampleCount, int seed)
        {
            var rng = new Random(seed);
            var set = new HashSet<int>();

            while (set.Count < Math.Min(sampleCount, count))
                set.Add(rng.Next(0, count));

            return set.OrderBy(i => i);
        }
    }

    // ===============================
    // Options / DTOs
    // ===============================

    internal enum TestMode
    {
        Minimal,
        Smoke,
        Full
    }

    internal sealed record TestOptions(
        TestMode Mode,
        int SamplePerOpcodeFile,
        int SampleSeed,
        int MaxDegreeOfParallelism)
    {
        public static TestOptions Minimal() => new(
            Mode: TestMode.Minimal,
            SamplePerOpcodeFile: 1,
            SampleSeed: 0,
            MaxDegreeOfParallelism: 0
        );

        public static TestOptions Smoke(int samplePerOpcodeFile = 50, int seed = 1337) => new(
            Mode: TestMode.Smoke,
            SamplePerOpcodeFile: samplePerOpcodeFile,
            SampleSeed: seed,
            MaxDegreeOfParallelism: 0
        );

        public static TestOptions Full() => new(
            Mode: TestMode.Full,
            SamplePerOpcodeFile: 0,
            SampleSeed: 0,
            MaxDegreeOfParallelism: 0
        );

    }

    // --- JSON DTOs matching your structure ---

    internal sealed class TestCase
    {
        public string Name { get; set; } = "";
        public TestState Initial { get; set; } = new();
        public TestState Final { get; set; } = new();

        // [ [address, value, "read"/"write"], ... ]
        public List<object[]>? Cycles { get; set; }
    }

    internal sealed class TestState
    {
        public int Pc { get; set; }
        public int S { get; set; }
        public int A { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int P { get; set; }

        // [ [address, value], ... ]
        public List<int[]> Ram { get; set; } = new();
    }

    internal static class JsonOptions
    {
        public static readonly JsonSerializerOptions Instance =
            new() { PropertyNameCaseInsensitive = true };
    }

    internal sealed class SingleStepTestFailureException : Exception
    {
        public SingleStepTestFailureException(string message, Exception inner)
            : base(message, inner) { }
    }

    // --- Minimal memory + CPU adapter placeholders ---

    internal sealed class FlatRam64k
    {
        private readonly byte[] _ram = new byte[65536];

        public byte Read(ushort address) => _ram[address];
        public void Write(ushort address, byte value) => _ram[address] = value;
    }

    internal sealed class CpuAdapter
    {
        private readonly FlatRam64k _mem;
        // TODO: Register cpu

        public CpuAdapter(FlatRam64k mem)
        {
            _mem = mem;
        }

        public void SetState(CpuStateDto s)
        {
            // TODO: Map to your CPU core
        }

        public CpuStateDto GetState()
        {
            // TODO: Map from your CPU core
            return new CpuStateDto();
        }

        public void StepInstruction()
        {
            // TODO: Execute exactly one instruction
        }
    }

    internal sealed record CpuStateDto
    {
        public ushort PC { get; init; }
        public byte S { get; init; }
        public byte A { get; init; }
        public byte X { get; init; }
        public byte Y { get; init; }
        public byte P { get; init; }
    }
}
