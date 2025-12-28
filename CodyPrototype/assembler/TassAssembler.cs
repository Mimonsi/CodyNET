using System.Diagnostics;
using System.Text;

namespace CodyPrototype.assembler;

public class TassAssembler : ICodyAssembler
{
        private readonly string _tassPath = @"C:\Program Files\64tass\64tass.exe";
        /// Full path to 64tass executable (e.g. "C:\Tools\64tass\64tass.exe" or "/usr/bin/64tass").
        private readonly string _args = "--mw65c02 --nostart";
        /// CPU selection argument. For 65C02 you likely want something like "--m65c02" or similar
        /// depending on the 64tass version/syntax you use.
        /// If you do not need explicit selection, pass empty string.

        public byte[] AssembleFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                throw new ArgumentException("Input file path must not be empty.", nameof(file));

            if (!File.Exists(file))
                throw new FileNotFoundException("Assembly input file not found.", file);

            file = EnsureUtf8WithoutBom(file);
            string tempOutputFile = Path.Combine(Path.GetTempPath(), $"cody_{Guid.NewGuid():N}.bin");

            try
            {
                // 64tass arguments:
                // - input file
                // - output file
                // - choose binary output (raw bytes)
                //
                // NOTE: Flags can differ slightly by version. If this fails, run "64tass --help"
                // and adjust the flags. The structure stays the same.
                //
                // Common patterns seen:
                //   64tass input.asm -o out.bin --nostart
                //   64tass input.asm -o out.bin --output out.bin --format raw
                //
                // We'll use a conservative approach: "-o" for output + "raw/binary format" switch.
                var args = new StringBuilder();

                // CPU switch (optional)
                if (!string.IsNullOrEmpty(_args))
                {
                    args.Append(_args);
                    args.Append(' ');
                }

                // Output file
                args.Append("-o ");
                args.Append('"').Append(tempOutputFile).Append('"');
                args.Append(' ');
                
                // Input file
                args.Append('"').Append(Path.GetFullPath(file)).Append('"');

                var psi = new ProcessStartInfo
                {
                    FileName = _tassPath,
                    Arguments = args.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                if (proc == null)
                    throw new InvalidOperationException("Failed to start 64tass process.");

                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();

                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"64tass failed with exit code {proc.ExitCode}.{Environment.NewLine}" +
                        $"Args: {psi.Arguments}{Environment.NewLine}" +
                        $"STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}" +
                        $"STDERR:{Environment.NewLine}{stderr}");
                }

                if (!File.Exists(tempOutputFile))
                {
                    throw new InvalidOperationException(
                        "64tass reported success but output file was not created. " +
                        "Check output-format flags and 64tass version.");
                }

                return File.ReadAllBytes(tempOutputFile);
            }
            finally
            {
                //TryDelete(tempOutputFile);
            }
        }

        public byte[] Assemble(string assemblyCode)
        {
            if (assemblyCode == null)
                throw new ArgumentNullException(nameof(assemblyCode));

            string tempInputFile = Path.Combine(Path.GetTempPath(), $"cody_{Guid.NewGuid():N}.asm");

            try
            {
                File.WriteAllText(tempInputFile, assemblyCode, Encoding.UTF8);
                return AssembleFile(tempInputFile);
            }
            finally
            {
                TryDelete(tempInputFile);
            }
        }
        
        private static string EnsureUtf8WithoutBom(string inputPath)
        {
            byte[] bytes = File.ReadAllBytes(inputPath);

            // UTF-8 BOM = EF BB BF
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                string tempNoBom = Path.Combine(Path.GetTempPath(), $"cody_{Guid.NewGuid():N}_nobom.asm");
                File.WriteAllBytes(tempNoBom, bytes[3..]);
                return tempNoBom;
            }

            return inputPath;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // Intentionally ignore cleanup failures.
            }
        }
}