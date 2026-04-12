using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CodyNET.Common.Utils;

namespace CodyNET.Assembler;

/// <summary>
///  Wrapper class for embedded 64tass assembler
/// </summary>
internal static class TassAssembler
{
        private static readonly string _tassPath = ResolveAndLogTassPath();

        /// Full path to 64tass executable (e.g. "C:\Tools\64tass\64tass.exe" or "/usr/bin/64tass").
        private static readonly string _args = "--mw65c02 --nostart";
        /// CPU selection argument. For 65C02 you likely want something like "--m65c02" or similar
        /// depending on the 64tass version/syntax you use.
        /// If you do not need explicit selection, pass empty string.

        internal static void AssembleFile(FileInfo inputFile, FileInfo? outputFile = null, FileInfo? listFile = null)
        {
            //inputFile = EnsureUtf8WithoutBom(inputFile);
            
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
            args.Append('"').Append(outputFile.FullName).Append('"');
            args.Append(' ');

            // Listing file (optional – used for address-to-source-line mapping)
            if (listFile != null)
            {
                args.Append("--list=");
                args.Append('"').Append(listFile.FullName).Append('"');
                args.Append(' ');
            }

            // Input file
            args.Append('"').Append(Path.GetFullPath(inputFile.FullName)).Append('"');

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

            var stdoutTask = proc.StandardOutput.ReadToEndAsync();
            var stderrTask = proc.StandardError.ReadToEndAsync();

            proc.WaitForExit();
            Task.WaitAll(stdoutTask, stderrTask);

            string stdout = stdoutTask.Result;
            string stderr = stderrTask.Result;

            Log.Info("64tass output: {output}", stdout);
            if (!string.IsNullOrEmpty(stderr))
            {
                Log.Error(stderr);
                throw new InvalidOperationException($"64tass reported errors:{Environment.NewLine}{stderr}");
            }

            if (proc.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"64tass failed with exit code {proc.ExitCode}.{Environment.NewLine}" +
                    $"Args: {psi.Arguments}{Environment.NewLine}" +
                    $"STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}" +
                    $"STDERR:{Environment.NewLine}{stderr}");
            }

            if (!outputFile.Exists)
            {
                throw new InvalidOperationException(
                    "64tass reported success but output file was not created. " +
                    "Check output-format flags and 64tass version.");
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
        private static string ResolveAndLogTassPath()
        {
            var path = ResolveTassPath();
            Log.Verbose("Using 64tass assembler at path: {path}", path);
            return path;
        }
    

        private static string ResolveTassPath()
        {
            string? overridePath = Environment.GetEnvironmentVariable("CODYNET_64TASS");
            if (!string.IsNullOrWhiteSpace(overridePath))
                return overridePath;

            string osPart = GetOsPart();
            string archPart = GetArchPart();
            string exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "64tass.exe" : "64tass";
            string resourceName = $"tass_{osPart}_{archPart}";

            var assembly = typeof(TassAssembler).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "codynet_tass");
                Directory.CreateDirectory(tempDir);
                string tempPath = Path.Combine(tempDir, exeName);

                if (!File.Exists(tempPath) || new FileInfo(tempPath).Length != stream.Length)
                {
                    using var fs = File.Create(tempPath);
                    stream.CopyTo(fs);
                }

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using var chmod = Process.Start(new ProcessStartInfo("chmod", $"+x \"{tempPath}\"")
                    {
                        UseShellExecute = false
                    });
                    chmod?.WaitForExit();
                }

                return tempPath;
            }

            Log.Warn("No embedded 64tass binary found for {os}-{arch}, falling back to PATH", osPart, archPart);
            return exeName;
        }

        private static string GetOsPart()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "win";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "mac";
            return "linux";
        }

        private static string GetArchPart()
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "arm64",
                Architecture.Arm => "arm",
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
            };
        }
    
}
