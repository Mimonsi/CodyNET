using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using CodyNET.Assembler;
using CodyNET.Cody;
using JetBrains.Annotations;
using NUnit.Framework;

namespace CodyNET.Tests.SingleStep
{
    [Parallelizable(ParallelScope.All)]
    public class SingleStepTests
    {
        private static TextWriter Output => TestContext.Progress;
        /// <summary>
        /// Tests all opcodes. (Level 4, final goal)
        /// </summary>
        [Test]
        [Explicit("Dev Test")]
        public void TestAllOpcodesFull()
        {
            var options = TestOptions.Full();
            TestRunner.RunAllOpcodes(options, Output);
        }

        /// <summary>
        /// Tests a selection of test cases for a single opcode. (Level 2)
        /// Minimal: one test case
        /// Smoke: sampled test cases (50 by default)
        /// Full: all test cases
        /// </summary>
        /// <param name="opcodeHex"></param>
        [Explicit("Dev Test")]
        [TestCase("90")]
        public void TestOpcode(string opcodeHex)
        {
            var options = TestOptions.Full() with { Verbose = true, StopOnFirstFailure = true};
            var result = TestRunner.RunSingleOpcode(opcodeHex, options, Output);
            Output.WriteLine($"Opcode 0x{opcodeHex}: {result.successful}/{result.total} tests passed.");
        }
        
        /// <summary>
        /// Tests a single test case by its name. (Level 1)
        /// </summary>
        /// <param name="testName"></param>
        [Explicit("Dev Test")]
        [TestCase("32 d9 38")]
        public void TestSingleTestCase(string testName)
        {
            var options = TestOptions.Full();
            var opcode = testName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            TestRunner.RunSingleTestByName(opcode, testName, options, Output);
        }

    }

    internal static class SingleStepMnemonicRunner
    {
        /// <summary>
        /// Tests all opcodes for a given mnemonic. (Level 3)
        /// </summary>
        public static void Run(Mnemonic mnemonic, TestOptions options, TextWriter? output)
        {
            bool outputEnabled = false;

            //options = options with { StopOnFirstFailure = true };
            var instructions = OpcodeLookup.FromMnemonic(mnemonic);
            Dictionary<Instruction, bool> opcodeResults = new Dictionary<Instruction, bool>();
            foreach (var instruction in instructions)
            {
                var opcodeStr = instruction.Opcode.ToString("x2");
                output?.WriteLine($"0x{opcodeStr}: Starting Tests");
                try
                {
                    if (outputEnabled)
                        TestRunner.RunSingleOpcode(opcodeStr, options, output);
                    else
                        TestRunner.RunSingleOpcode(opcodeStr, options, null);
                    output?.WriteLine($"0x{opcodeStr}: Tests Completed");
                    opcodeResults.Add(instruction, true);
                }
                catch (Exception ex)
                {
                    output?.WriteLine($"0x{opcodeStr}: Tests Failed");
                    output?.WriteLine(ex.ToString());
                    opcodeResults.Add(instruction, false);
                }
            }
            output?.WriteLine("=== MNEMONIC TESTS DONE ===");
            var resultText = "";
            int fails = 0;
            foreach (var result in opcodeResults)
            {
                resultText += $"0x{result.Key.Opcode.ToString("x2")} ({result.Key.AddressingMode}): {(result.Value ? "PASS" : "FAIL")}\n";
                if (!result.Value)
                    fails++;
            }
            output?.WriteLine(resultText);
            if (fails > 0)
                throw new Exception($"{fails} Tests failed for mnemonic {mnemonic}:\n{resultText}");
        }
    }

    // --- Runner ---

    internal static class TestRunner
    {
        // Prefer testdata copied to the output directory; fallback to repo path for local runs.
        private static readonly string TestDataDir = ResolveTestDataDir();

        public static void RunAllOpcodes(TestOptions options, TextWriter? output)
        {
            if (!Directory.Exists(TestDataDir))
                throw new DirectoryNotFoundException($"Test data directory not found: {TestDataDir}");

            var files = Directory.EnumerateFiles(TestDataDir, "*.json")
                                 .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                 .ToArray();

            if (files.Length == 0)
                throw new InvalidOperationException($"No *.json files found in {TestDataDir}");
            
            output?.WriteLine($"Starting test run: {options.Mode}, opcode files = {files.Length}");
            
            var degree = options.MaxDegreeOfParallelism <= 0
                ? Environment.ProcessorCount
                : options.MaxDegreeOfParallelism;

            // Keep output readable when running with an output helper.
            if (output is not null && degree > 1)
                degree = 1;

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = degree };

            Parallel.ForEach(files, parallelOptions, file =>
            {
                var opcodeHex = Path.GetFileNameWithoutExtension(file);
                output?.WriteLine(
                    $"=== Opcode {opcodeHex.ToUpperInvariant()} ===");
                RunFile(file, opcodeHex, options, output);
            });


            output?.WriteLine("=== ALL OPCODES DONE ===");
        }

        public static (int successful, int total) RunSingleOpcode(
            string opcodeHex,
            TestOptions options,
            TextWriter? output)
        {
            opcodeHex = opcodeHex.Trim().ToLowerInvariant();

            var file = Path.Combine(TestDataDir, $"{opcodeHex}.json");
            if (!File.Exists(file))
                throw new FileNotFoundException($"Opcode test file not found: {file}");

            return RunFile(file, opcodeHex, options, output);
        }
        
        public static void RunSingleTestByName(
            string opcodeHex,
            string testName,
            TestOptions options,
            TextWriter? output)
        {
            opcodeHex = opcodeHex.Trim().ToLowerInvariant();
            testName = testName.Trim();

            var file = Path.Combine(TestDataDir, $"{opcodeHex}.json");
            if (!File.Exists(file))
                throw new FileNotFoundException($"Opcode test file not found: {file}");

            var tests = LoadTests(file);
            if (tests.Count == 0)
                throw new InvalidOperationException($"No tests found in file: {file}");

            var matches = tests
                .Select((test, index) => new { test, index })
                .Where(match => string.Equals(match.test.Name, testName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
                throw new InvalidOperationException(
                    $"No test case named \"{testName}\" found in opcode {opcodeHex.ToUpperInvariant()}.");

            /*if (matches.Count > 1)
                throw new InvalidOperationException(
                    $"Multiple test cases named \"{testName}\" found in opcode {opcodeHex.ToUpperInvariant()}.");*/

            var selected = matches[0];
            output?.WriteLine(
                $"Opcode {opcodeHex.ToUpperInvariant()}: Running test index={selected.index} name=\"{selected.test.Name}\"");

            try
            {
                ExecuteOne(selected.test, options, output);
            }
            catch (Exception ex)
            {
                if (options.StopOnFirstFailure)
                {
                    throw new SingleStepTestFailureException(
                        $"Single-step test failed. opcode={opcodeHex.ToUpperInvariant()} index={selected.index} name=\"{selected.test.Name}\"",
                        ex);
                }
                var message = $"Test failed. opcode={opcodeHex.ToUpperInvariant()} index={selected.index} name=\"{selected.test.Name}\"";
                output?.WriteLine(message);
                output?.WriteLine(ex.ToString());
                throw;
            }

            output?.WriteLine(
                $"Opcode {opcodeHex.ToUpperInvariant()}: DONE");
        }


        private static (int successful, int total) RunFile(
            string filePath,
            string opcodeHex,
            TestOptions options,
            TextWriter? output)
        {
            var tests = LoadTests(filePath);
            var indices = SelectIndices(tests.Count, options).ToArray();

            if (options.Verbose)
            {
                output?.WriteLine(
                    $"Opcode {opcodeHex.ToUpperInvariant()}: {indices.Length} test cases");
            }

            int total = indices.Length;
            int successful = 0;

            for (int i = 0; i < total; i++)
            {
                var idx = indices[i];
                var test = tests[idx];

                try
                {
                    if (options.Verbose)
                    {
                        output?.WriteLine(
                            $"Opcode {opcodeHex.ToUpperInvariant()}: Running test {i + 1}/{total} index={idx} name=\"{test.Name}\"");
                    }
                    ExecuteOne(test, options, output);
                    successful++;
                }
                catch (Exception ex)
                {
                    if (options.StopOnFirstFailure)
                    {
                        throw new SingleStepTestFailureException(
                            $"Single-step test failed. opcode={opcodeHex.ToUpperInvariant()} index={idx} name=\"{test.Name}\"",
                            ex);
                    }
                    var message = $"Test failed. opcode={opcodeHex.ToUpperInvariant()} index={idx} name=\"{test.Name}\"\nProgram: \n {GetDisassembledProgram(test)}"; //TODO: Add disassembled program
                    output?.WriteLine(message);
                    output?.WriteLine(ex.ToString());

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
            output?.WriteLine(
                $"  Successful: {successful}/{total} ({(successful * 100.0 / total):F2}%)");
            Assert.AreEqual(total, successful);
            return (successful, total);
        }
        
        private static string GetDisassembledProgram(TestCase t)
        {
            var ram = t.Initial.Ram;
            var programBytes = new List<byte>();
            foreach (var pair in ram)
            {
                var addr = (ushort)pair[0];
                var value = (byte)pair[1];
                if (addr >= t.Initial.Pc)
                    programBytes.Add(value);
            }
            return CodyDisassembler.Disassemble(programBytes.ToArray());
        }

        // ===============================
        // Test execution
        // ===============================

        private static void ExecuteOne(TestCase t, TestOptions options, TextWriter? output)
        {
            // CPU hookup with initial state
            var cpu = new Cpu(t.Initial.GetCpuState());
            cpu.SetState(t.Initial.GetCpuState());

            // Execute exactly one instruction
            cpu.Step();

            // Assert final CPU state
            var actual = cpu.GetState();
            var mismatch = GetMismatchMessage(t.Initial.GetCpuState(), t.Final.GetCpuState(), actual);

            if (!string.IsNullOrEmpty(mismatch))
            {
                output?.WriteLine(mismatch);
                throw new SingleStepTestFailureException($"CPU state mismatch in test case {t.Name}:", new Exception(mismatch));
            }

            // Assert final RAM pairs
            foreach (var pair in t.Final.Ram)
            {
                var addr = (ushort)pair[0];
                var expected = (byte)pair[1];
                var actualVal = cpu.Memory.Read(addr);
                if (expected != actualVal)
                {
                    var message = $"RAM mismatch at 0x{addr:X4}: expected 0x{expected:X2} actual 0x{actualVal:X2}";
                    output?.WriteLine(message);
                    throw new SingleStepTestFailureException(message, new Exception(message));
                }
            }

            if (options.Verbose)
                output?.WriteLine(PrettyPrintStates(t.Initial.GetCpuState(), t.Final.GetCpuState(), actual));

            // Optional: cycle count compare (state-only friendly)
            // If your CPU exposes cycles used for last instruction, you can check:
            // if (options.CheckCycleCount && t.Cycles is not null)
            //     Assert.AreEqual(t.Cycles.Count, cpu.CyclesConsumedLastInstruction);
        }

        // ===============================
        // Helpers
        // ===============================

        private static string GetMismatchMessage(CpuState initial, CpuState final, CpuState actual)
        {
            if (final.A == actual.A &&
                final.X == actual.X &&
                final.Y == actual.Y &&
                final.S == actual.S &&
                final.P == actual.P &&
                final.PC == actual.PC)
            {
                return string.Empty;
            }

            return PrettyPrintStates(initial, final, actual);
        }

        private static string PrettyPrintStates(CpuState initial, CpuState final, CpuState actual)
        {
            // Format table with intial, actual and final state next to each other like this
            // Register | Initial | Actual | Expected
            // ---------------------------------------
            // A        | 0x12    | 0x34   | 0x56
            
            var text = "\nRegister | Initial | Actual  | Expected\n";
            text += "---------------------------------------\n";
            if (final.A != actual.A)
                text += $"A (FAIL) | 0x{initial.A:X2}    | 0x{actual.A:X2}   | 0x{final.A:X2}\n";
            else
                text += $"A        | 0x{initial.A:X2}    | 0x{actual.A:X2}   | 0x{final.A:X2}\n";
            if (final.X != actual.X)
                text += $"X (FAIL) | 0x{initial.X:X2}    | 0x{actual.X:X2}   | 0x{final.X:X2}\n";
            else
                text += $"X        | 0x{initial.X:X2}    | 0x{actual.X:X2}   | 0x{final.X:X2}\n";
            if (final.Y != actual.Y)
                text += $"Y (FAIL) | 0x{initial.Y:X2}    | 0x{actual.Y:X2}   | 0x{final.Y:X2}\n";
            else
                text += $"Y        | 0x{initial.Y:X2}    | 0x{actual.Y:X2}   | 0x{final.Y:X2}\n";
            if (final.S != actual.S)
                text += $"S (FAIL) | 0x{initial.S:X2}    | 0x{actual.S:X2}   | 0x{final.S:X2}\n";
            else
                text += $"S        | 0x{initial.S:X2}    | 0x{actual.S:X2}   | 0x{final.S:X2}\n";
            if (final.P != actual.P)
                text += $"P (FAIL) | 0x{initial.P:X2}    | 0x{actual.P:X2}   | 0x{final.P:X2}\n";
            else
                text += $"P        | 0x{initial.P:X2}    | 0x{actual.P:X2}   | 0x{final.P:X2}\n";
            if (final.PC != actual.PC)
                text += $"PC(FAIL) | 0x{initial.PC:X4} | 0x{actual.PC:X4} | 0x{final.PC:X4}\n";
            else
                text += $"PC       | 0x{initial.PC:X4} | 0x{actual.PC:X4} | 0x{final.PC:X4}\n";
            return text;
        }

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
                TestMode.Minimal => [0],
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

        private static string ResolveTestDataDir()
        {
            var outputDir = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "testdata",
                "wdc65c02",
                "v1"));

            if (Directory.Exists(outputDir))
                return outputDir;

            var repoDir = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..",
                "testdata",
                "wdc65c02",
                "v1"));

            return repoDir;
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
        int MaxDegreeOfParallelism,
        bool StopOnFirstFailure = false,
        bool Verbose = false)
    {
        /// <summary>
        /// Runs a minimal set of tests: one test case per opcode file.
        /// </summary>
        /// <returns></returns>
        public static TestOptions Minimal() => new(
            Mode: TestMode.Minimal,
            SamplePerOpcodeFile: 1,
            SampleSeed: 0,
            MaxDegreeOfParallelism: 0,
            StopOnFirstFailure: true,
            Verbose: false
        );

        /// <summary>
        /// Runs a smoke test: samples a number of test cases per opcode file.
        /// </summary>
        /// <param name="samplePerOpcodeFile"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static TestOptions Smoke(int samplePerOpcodeFile = 50, int seed = 1337) => new(
            Mode: TestMode.Smoke,
            SamplePerOpcodeFile: samplePerOpcodeFile,
            SampleSeed: seed,
            MaxDegreeOfParallelism: 0,
            StopOnFirstFailure: false,
            Verbose: false
        );

        /// <summary>
        /// Runs the full set of tests: all test cases per opcode file.
        /// </summary>
        /// <returns></returns>
        public static TestOptions Full() => new(
            Mode: TestMode.Full,
            SamplePerOpcodeFile: 0,
            SampleSeed: 0,
            MaxDegreeOfParallelism: 0,
            StopOnFirstFailure: false,
            Verbose: false
        );

    }

    // --- JSON DTOs matching your structure ---

    [UsedImplicitly]
    internal class TestCase
    {
        public string Name { get; set; } = "";
        public CpuStateDto Initial { get; set; } = new();
        public CpuStateDto Final { get; set; } = new();

        // [ [address, value, "read"/"write"], ... ]
        public List<object[]>? Cycles { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal sealed record CpuStateDto
    {
        public ushort Pc { get; set; }
        public int S { get; set; }
        public int A { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int P { get; set; }

        // [ [address, value], ... ]
        public List<int[]> Ram { get; set; } = [];

        public CpuState GetCpuState()
        {
            return new CpuState()
            {
                PC = Pc,
                S = (byte)S,
                A = (byte)A,
                X = (byte)X,
                Y = (byte)Y,
                P = (byte)P,
                Ram = Ram
            };
        }
    }

    internal static class JsonOptions
    {
        public static readonly JsonSerializerOptions Instance =
            new() { PropertyNameCaseInsensitive = true };
    }

    internal sealed class SingleStepTestFailureException(string message, Exception inner) : Exception(message, inner);
}
