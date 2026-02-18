using System.CommandLine;
using System.CommandLine.Completions;
using System.Globalization;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Disassembler;

namespace CodyNET.Core;

public static class Cli
{
    public static RootCommand BuildRootCommand()
    {
        var root = new RootCommand("CodyNET CLI");

        var verboseOption = new Option<bool>("--verbose", ["-v"])
        {
            Description = "Enable verbose logging"
        };
        root.Options.Add(verboseOption);

        root.Subcommands.Add(BuildListCommand());
        root.Subcommands.Add(BuildBootCommand(verboseOption));
        root.Subcommands.Add(BuildRunCommand(verboseOption));
        root.Subcommands.Add(BuildAssembleCommand(verboseOption));
        root.Subcommands.Add(BuildDisassembleCommand(verboseOption));
        root.Subcommands.Add(BuildLogTestCommand());

        root.SetAction(_ =>
        {
            Console.WriteLine("Use --help to see available commands.");
            return 0;
        });

        return root;
    }

    private static Command BuildListCommand()
    {
        var cmd = new Command("list", "Lists .s source files and .bin binaries in the current directory")
        {
            Aliases = { "ls", "dir" }
        };
        
        var recursive = new Option<bool>("--recursive", ["-r"])
        {
            Description = "Shows number of files in subdirectories",
            DefaultValueFactory = _ => false
        };
        cmd.Add(recursive);
        
        // Action (placeholder)
        cmd.SetAction(parseResult =>
        {
            bool rec = parseResult.GetValue(recursive);
            Console.WriteLine("Available .s files:");
            Console.WriteLine(GetSubdirFilesText("*.s", rec));
                
            Console.WriteLine("Available .bin files:");
            Console.WriteLine(GetSubdirFilesText("*.bin", rec));
        });
        return cmd;
    }

    private static Command BuildLogTestCommand()
    {
        var cmd = new Command("logtest", "Write example logs for all levels and structured logging");

        cmd.SetAction(_ =>
        {
            var previousLevel = Log.Level;
            Log.Level = LogLevel.Trace;

            try
            {
                var user = "cli-user";
                var operation = "logtest";
                var durationMs = 42;

                Log.Trace("Trace sample: operation={Operation}", operation);
                Log.Debug("Debug sample: operation={Operation} durationMs={DurationMs}", operation, durationMs);
                Log.Info("Info sample: User={User} Operation={Operation}", user, operation);
                Log.Info($"Info sample with normal message {operation}");
                Log.Warn("Warn sample: User={User} Retries={Retries}", user, 1);
                Log.Error("Error sample: User={User} ErrorCode={ErrorCode}", user, "E_LOGTEST");

                Log.Info("Structured sample with named properties User={User} Operation={Operation} DurationMs={DurationMs}",
                    user, operation, durationMs);

                try
                {
                    throw new InvalidOperationException("Sample exception from logtest command");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Structured exception sample User={User} Operation={Operation}", user, operation);
                }
            }
            finally
            {
                Log.Level = previousLevel;
            }

            return 0;
        });

        return cmd;
    }

    private static string GetSubdirFilesText(string pattern, bool recursive = false)
    {
        var lines = new List<string>();
        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), pattern);
        foreach (var file in files)
            lines.Add($"  {Path.GetFileName(file)}");

        if (recursive)
        {
            var subdirs = Directory.GetDirectories(Directory.GetCurrentDirectory());
            foreach (var subdir in subdirs)
            {
                var count = Directory.GetFiles(subdir, pattern).Length;
                if (count > 0)
                    lines.Add($"  {Path.GetFileName(subdir)}/: {count} files");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static Command BuildBootCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("boot", "Boot the emulator with the built-in CodyBASIC");
        
        var physicalKeyboard = new Option<bool>("--physical-keyboard")
        {
            Description = "Physical Cody keyboard mapping (ignores host layout)"
        };
        
        var clock = new Option<string?>("--clock")
        {
            Description = "Target CPU clock rate (e.g. 1000000, 1MHz, 500kHz)",
            DefaultValueFactory = _ => "1MHz" // Default to 1 MHz
        };

        var fast = new Option<bool>("--fast")
        {
            Description = "Run as fast as possible (ignores --clock)"
        };

        cmd.Add(physicalKeyboard);
        cmd.Add(clock);
        cmd.Add(fast);
        
        cmd.SetAction(parseResult =>
        {
            if (parseResult.GetValue(verboseOption))
                Log.Level = LogLevel.Trace;
            ExecuteBootCommand(
                parseResult.GetValue(physicalKeyboard),
                parseResult.GetValue(clock),
                parseResult.GetValue(fast)
            );
            return 0;
        });

        return cmd;
    }

    private static void ExecuteBootCommand(bool physicalKeyboard, string? clock, bool fast)
    {
        Log.Info("Executing boot command");
        _ = physicalKeyboard; // keyboard device wiring is pending

        var setupOptions = new CodySetupOptions
        {
            FrequencyHz = fast ? -1 : ParseClock(clock),
            // TODO: Delegate all options
        };

        Cody.Cody cody = new Cody.Cody();
        cody.Boot(runtimeOptions: setupOptions);
    }

    private static Command BuildRunCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("run", "Run a binary file with the emulator");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Binary file (raw or cartridge image if --as-cartridge is set)"
        }.AcceptExistingOnly();
        cmd.Arguments.Add(fileArg);

        var asCartridge = new Option<bool>("--as-cartridge")
        {
            Description = "Load <file> as cartridge (expects cartridge header)",
            DefaultValueFactory = _ => false
        };

        var loadAddress = new Option<string>("--load-address", ["-l"])
        {
            Description = "Load address for raw binaries (default: 0xE000)",
            DefaultValueFactory = _ => "0xE000"
        };

        var resetVector = new Option<string?>("--reset-vector") { Description = "Override Reset Vector (0xFFFC)" };
        var irqVector = new Option<string?>("--irq-vector") { Description = "Override IRQ Vector (0xFFFE)" };
        var nmiVector = new Option<string?>("--nmi-vector") { Description = "Override NMI Vector (0xFFFA)" };

        var uart1Source = new Option<string?>("--uart1-source")
        {
            Description = "Path of file used to fill the UART1 receive buffer with bytes"
        };

        var fixNewlines = new Option<bool>("--fix-newlines")
        {
            Description = "Normalize newlines when reading UART text input (CRLF -> LF)"
        };

        var physicalKeyboard = new Option<bool>("--physical-keyboard")
        {
            Description = "Physical Cody keyboard mapping (ignores host layout)"
        };

        var debug = new Option<bool>("--debug", ["-d"])
        {
            Description = "Enable debugger (interactive)"
        };

        var clock = new Option<string?>("--clock")
        {
            Description = "Target CPU clock rate (e.g. 1000000, 1MHz, 500kHz)",
            DefaultValueFactory = _ => "1MHz"
        };

        var fast = new Option<bool>("--fast")
        {
            Description = "Run as fast as possible (ignores --clock)"
        };

        cmd.Options.Add(asCartridge);
        cmd.Options.Add(loadAddress);
        cmd.Options.Add(resetVector);
        cmd.Options.Add(irqVector);
        cmd.Options.Add(nmiVector);
        cmd.Options.Add(uart1Source);
        cmd.Options.Add(fixNewlines);
        cmd.Options.Add(physicalKeyboard);
        cmd.Options.Add(debug);
        cmd.Options.Add(clock);
        cmd.Options.Add(fast);

        cmd.SetAction(parseResult =>
        {
            if (parseResult.GetValue(verboseOption))
                Log.Level = LogLevel.Trace;

            var inputFile = parseResult.GetValue(fileArg)
                ?? throw new ArgumentException("Missing input file argument.");
            var parsedLoadAddress = parseResult.GetValue(loadAddress) ?? "0xE000";

            ExecuteRunCommand(
                inputFile,
                parseResult.GetValue(asCartridge),
                parsedLoadAddress,
                parseResult.GetValue(resetVector),
                parseResult.GetValue(irqVector),
                parseResult.GetValue(nmiVector),
                parseResult.GetValue(uart1Source),
                parseResult.GetValue(fixNewlines),
                parseResult.GetValue(physicalKeyboard),
                parseResult.GetValue(debug),
                parseResult.GetValue(clock),
                parseResult.GetValue(fast));

            return 0;
        });

        return cmd;
    }

    private static long ParseClock(string? clock)
    {
        if (string.IsNullOrWhiteSpace(clock))
            return 1_000_000;

        var text = clock.Trim().ToLowerInvariant();
        long multiplier;

        if (text.EndsWith("mhz", StringComparison.Ordinal))
        {
            multiplier = 1_000_000;
            text = text[..^3];
        }
        else if (text.EndsWith("khz", StringComparison.Ordinal))
        {
            multiplier = 1_000;
            text = text[..^3];
        }
        else if (text.EndsWith("hz", StringComparison.Ordinal))
        {
            multiplier = 1;
            text = text[..^2];
        }
        else
        {
            multiplier = 1;
        }

        if (long.TryParse(text, out long value) && value > 0)
            return value * multiplier;

        throw new ArgumentException($"Invalid clock format: {clock}");
    }

    private static void ExecuteRunCommand(
        FileInfo inputFile,
        bool asCartridge,
        string loadAddress,
        string? resetVector,
        string? irqVector,
        string? nmiVector,
        string? uart1Source,
        bool fixNewlines,
        bool physicalKeyboard,
        bool debug,
        string? clock,
        bool fast)
    {
        _ = uart1Source;      // UART device wiring is pending
        _ = fixNewlines;      // UART device wiring is pending
        _ = physicalKeyboard; // keyboard device wiring is pending
        Log.Info("Executing run command");

        var setupOptions = new CodySetupOptions
        {
            EnableDebugger = debug,
            EnableVideo = false,
            FrequencyHz = fast ? -1 : ParseClock(clock)
        };

        var loadOptions = new CodyLoadOptions
        {
            AsCartridge = asCartridge,
            LoadAddress = ParseHexUShort(loadAddress, nameof(loadAddress)),
            ResetVectorOverride = ParseOptionalHexUShort(resetVector, nameof(resetVector)),
            IrqVectorOverride = ParseOptionalHexUShort(irqVector, nameof(irqVector)),
            NmiVectorOverride = ParseOptionalHexUShort(nmiVector, nameof(nmiVector))
        };

        Cody.Cody cody = new Cody.Cody();
        cody.RunBinaryFile(inputFile.FullName, loadOptions, setupOptions);
    }

    private static ushort? ParseOptionalHexUShort(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return ParseHexUShort(value, paramName);
    }

    private static ushort ParseHexUShort(string value, string paramName)
    {
        var text = value.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            text = text[2..];

        if (ushort.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort parsed))
            return parsed;

        throw new ArgumentException($"Invalid 16-bit hex value for {paramName}: '{value}'.");
    }

    private static Command BuildAssembleCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("assemble", "Assemble a source file into a binary");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Assembly source file (.s)"
        }.AcceptExistingOnly();

        fileArg.CompletionSources.Add(_ =>
        {
            return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.s")
                .Select(f => new CompletionItem(f));
        });

        cmd.Arguments.Add(fileArg);

        var output = new Option<FileInfo?>("--output", ["-o"])
        {
            Description = "Output binary path (default: <input>.bin)"
        }.AcceptLegalFileNamesOnly();

        var format = new Option<string?>("--format")
        {
            Description = "Output format: raw | cartridge"
        };

        var loadAddress = new Option<string>("--load-address", ["-l"])
        {
            Description = "Load address metadata / ORG for raw output (default: 0xE000)",
            DefaultValueFactory = _ => "0xE000"
        };

        var warnAsError = new Option<bool>("--warn-as-error")
        {
            Description = "Treat warnings as errors"
        };

        cmd.Options.Add(output);
        cmd.Options.Add(format);
        cmd.Options.Add(loadAddress);
        cmd.Options.Add(warnAsError);

        cmd.SetAction(parseResult =>
        {
            if (parseResult.GetValue(verboseOption))
                Log.Level = LogLevel.Trace;

            var inputFile = parseResult.GetValue(fileArg)
                ?? throw new ArgumentException("Missing input file argument.");

            ExecuteAssembleCommand(
                inputFile,
                parseResult.GetValue(output),
                parseResult.GetValue(format),
                parseResult.GetValue(loadAddress),
                parseResult.GetValue(warnAsError));

            return 0;
        });

        return cmd;
    }

    private static void ExecuteAssembleCommand(
        FileInfo inputFile,
        FileInfo? outputFile,
        string? format,
        string? loadAddress,
        bool warnAsError)
    {
        _ = inputFile;
        _ = outputFile;
        _ = format;
        _ = loadAddress;
        _ = warnAsError;
        // TODO: Implement assembler call
        Log.Info("Executing assemble command");
    }

    private static Command BuildDisassembleCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("disassemble", "Disassemble a binary into assembly");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Binary file (raw or cartridge)"
        }.AcceptExistingOnly();
        cmd.Arguments.Add(fileArg);

        var asCartridge = new Option<bool>("--as-cartridge")
        {
            Description = "Treat <file> as cartridge image (header present)"
        };

        var loadAddress = new Option<string>("--load-address", ["-l"])
        {
            Description = "Base address for raw binaries (default: 0xE000)",
            DefaultValueFactory = _ => "0xE000"
        };

        var output = new Option<FileInfo?>("--output", ["-o"])
        {
            Description = "Output assembly path (default: <input>.s)"
        }.AcceptLegalFileNamesOnly();

        cmd.Options.Add(asCartridge);
        cmd.Options.Add(loadAddress);
        cmd.Options.Add(output);

        cmd.SetAction(parseResult =>
        {
            if (parseResult.GetValue(verboseOption))
                Log.Level = LogLevel.Trace;

            var inputFile = parseResult.GetValue(fileArg)
                ?? throw new ArgumentException("Missing input file argument.");

            ExecuteDisassembleCommand(
                inputFile,
                parseResult.GetValue(output),
                parseResult.GetValue(asCartridge),
                parseResult.GetValue(loadAddress));

            return 0;
        });

        return cmd;
    }

    private static void ExecuteDisassembleCommand(
        FileInfo inputFile,
        FileInfo? outputFile,
        bool asCartridge,
        string? loadAddress)
    {
        _ = asCartridge;
        _ = loadAddress;
        Log.Info("Executing disassemble command");
        // TODO: Include cartridge header parsing if asCartridge is true (override loadAddress) and load address
        CodyDisassembler.DisassembleFile(inputFile, outputFile);
    }
}
