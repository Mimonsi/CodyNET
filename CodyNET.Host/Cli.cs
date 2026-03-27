using System.CommandLine;
using System.CommandLine.Completions;
using System.Globalization;
using System.Runtime.ExceptionServices;
using Avalonia;
using CodyNET.Assembler;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Disassembler;

namespace CodyNET.Host;

public static class Cli
{
    private const string DefaultClock = "1MHz";
    private const string DefaultLoadAddress = "0xE000";
    private const string RawFormat = "raw";
    private const string CartridgeFormat = "cartridge";

    private sealed record ExecutionOptions(
        Option<bool>? Debug,
        Option<string?> Uart1Source,
        Option<string?> Uart2Source,
        Option<bool> FixNewlines,
        Option<bool> PhysicalKeyboard,
        Option<string?> Clock,
        Option<bool> Fast,
        Option<bool> Headless,
        Option<bool> StartPaused);
    
    private static void ApplyLogging(ParseResult parseResult, Option<bool> verboseOption)
    {
        if (parseResult.GetValue(verboseOption))
            Log.Level = LogLevel.Verbose;
    }
    
    public static RootCommand BuildRootCommand(bool useMainThreadScreenHost = true)
    {
        var root = new RootCommand("CodyNET");

        var verboseOption = new Option<bool>("--verbose", ["-v"])
        {
            Description = "Enable verbose logging"
        };
        root.Options.Add(verboseOption);

        root.Subcommands.Add(BuildListCommand());
        root.Subcommands.Add(BuildBootCommand(verboseOption, useMainThreadScreenHost));
        root.Subcommands.Add(BuildRunCommand(verboseOption, useMainThreadScreenHost));
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
            Description = "Shows number of files in subdirectories"
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
            Log.Level = LogLevel.Verbose;

            try
            {
                var user = "cli-user";
                var operation = "logtest";
                var durationMs = 42;
                var retries = 1;

                var payload = new
                {
                    User = user,
                    Operation = operation,
                    DurationMs = durationMs,
                    Retries = retries
                };

                // --- VERBOSE ---
                Log.Verbose("VERBOSE plain: no properties");
                Log.Verbose($"VERBOSE interpolated: user={user} op={operation} durationMs={durationMs}");
                Log.Verbose("VERBOSE structured: User={User} Operation={Operation} DurationMs={DurationMs}",
                    user, operation, durationMs);

                // --- DEBUG ---
                Log.Debug("DEBUG plain: no properties");
                Log.Debug($"DEBUG interpolated: user={user} op={operation} durationMs={durationMs}");
                Log.Debug("DEBUG structured: User={User} Operation={Operation} DurationMs={DurationMs}",
                    user, operation, durationMs);

                // --- INFO ---
                Log.Info("INFO plain: no properties");
                Log.Info($"INFO interpolated: user={user} op={operation} durationMs={durationMs}");
                Log.Info("INFO structured: User={User} Operation={Operation} DurationMs={DurationMs}",
                    user, operation, durationMs);

                // --- WARN ---
                Log.Warn("WARN plain: no properties");
                Log.Warn($"WARN interpolated: user={user} retries={retries}");
                Log.Warn("WARN structured: User={User} Retries={Retries} Operation={Operation}",
                    user, retries, operation);

                // --- ERROR (incl. exception) ---
                Log.Error("ERROR plain: no properties");
                Log.Error($"ERROR interpolated: user={user} op={operation} durationMs={durationMs}");
                Log.Error("ERROR structured: User={User} Operation={Operation} DurationMs={DurationMs}",
                    user, operation, durationMs);

                try
                {
                    throw new InvalidOperationException("Sample exception from logtest command");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "ERROR exception structured: User={User} Operation={Operation}", user, operation);
                }

                // --- EXTRA CASES (useful to see Serilog behavior) ---

                // 1) Missing template values (bad practice; shows how Serilog renders missing holes)
                Log.Warn("EXTRA missing args (bad): User={User} Operation={Operation} DurationMs={DurationMs}", user, operation);

                // 2) Escaped braces (literal text, no structured props)
                Log.Info("EXTRA escaped braces: User={{User}} Operation={{Operation}}");

                // 3) Destructuring: capture object fields instead of ToString()
                Log.Debug("EXTRA destructure object: Payload={@Payload}", payload);

                // 4) Stringify: force ToString() even if it's an object (rarely needed)
                Log.Debug("EXTRA stringify object: Payload={$Payload}", payload);
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

    private static ExecutionOptions AddExecutionOptions(Command cmd, bool includeDebug)
    {
        Option<bool>? debug = null;
        if (includeDebug)
        {
            debug = new Option<bool>("--debug", ["-d"])
            {
                Description = "Enable debugger (interactive)"
            };
            cmd.Add(debug);
        }
        
        var uart1Source = new Option<string?>("--uart1-source")
        {
            Description = "Path of file used to fill the UART1 receive buffer with bytes"
        };        
        
        var uart2Source = new Option<string?>("--uart2-source")
        {
            Description = "Path of file used to fill the UART2 receive buffer with bytes"
        };

        var fixNewlines = new Option<bool>("--fix-newlines")
        {
            Description = "Normalize newlines when reading UART text input (CRLF -> LF)"
        };
        
        var physicalKeyboard = new Option<bool>("--physical-keyboard")
        {
            Description = "Physical Cody keyboard mapping (ignores host layout)"
        };
        
        var clock = new Option<string?>("--clock")
        {
            Description = "Target CPU clock rate (e.g. 1000000, 1MHz, 500kHz)",
            DefaultValueFactory = _ => DefaultClock
        };

        var fast = new Option<bool>("--fast")
        {
            Description = "Run as fast as possible (ignores --clock)"
        };
        
        var headless = new Option<bool>("--headless")
        {
            Description = "Run without video output"
        };

        var startPaused = new Option<bool>("--start-paused")
        {
            Description = "Start emulator in paused state"
        };

        if (debug is not null)
            cmd.Add(debug);
        cmd.Add(uart1Source);
        cmd.Add(uart2Source);
        cmd.Add(fixNewlines);
        cmd.Add(physicalKeyboard);
        cmd.Add(clock);
        cmd.Add(fast);
        cmd.Add(headless);
        cmd.Add(startPaused);
        
        return new ExecutionOptions(debug, uart1Source, uart2Source, fixNewlines, physicalKeyboard, clock, fast, headless, startPaused);
    }

    private static CodySetupOptions BuildSetupOptions(ParseResult parseResult, ExecutionOptions options)
    {
        return new CodySetupOptions
        {
            EnableDebugger = options.Debug is not null && parseResult.GetValue(options.Debug),
            PhysicalKeyboard = parseResult.GetValue(options.PhysicalKeyboard),
            FrequencyHz = parseResult.GetValue(options.Fast)
                ? -1
                : CliValueParser.ParseClock(parseResult.GetValue(options.Clock)),
            EnableScreen = !parseResult.GetValue(options.Headless),
            StartPaused = parseResult.GetValue(options.StartPaused)
        };
    }

    private static CodyLoadOptions BuildSharedLoadOptions(ParseResult parseResult, ExecutionOptions options)
    {
        return new CodyLoadOptions
        {
            Uart1Source = CliValueParser.ParseExistingFile(parseResult.GetValue(options.Uart1Source), options.Uart1Source.Name),
            Uart2Source = CliValueParser.ParseExistingFile(parseResult.GetValue(options.Uart2Source), options.Uart2Source.Name),
            FixUartNewlines = parseResult.GetValue(options.FixNewlines)
        };
    }

    private static Command BuildBootCommand(Option<bool> verboseOption, bool useMainThreadScreenHost)
    {
        var cmd = new Command("boot", "Boot the emulator with the built-in CodyBASIC");
        var executionOptions = AddExecutionOptions(cmd, includeDebug: true);
        
        cmd.SetAction(parseResult =>
        {
            ApplyLogging(parseResult, verboseOption);
            var setupOptions = BuildSetupOptions(parseResult, executionOptions);
            var loadOptions = BuildSharedLoadOptions(parseResult, executionOptions);

            if (useMainThreadScreenHost && setupOptions.EnableScreen)
                ExecuteBootCommandWithScreenHost(setupOptions, loadOptions);
            else
                ExecuteBootCommand(setupOptions, loadOptions);

            return 0;
        });

        return cmd;
    }

    private static void ExecuteBootCommand(CodySetupOptions setupOptions, CodyLoadOptions loadOptions)
    {
        Log.Info("Executing boot command");
        Cody cody = CodyFactory.CreateCody(setupOptions);
        cody.Boot(loadOptions);
    }

    private static void ExecuteBootCommandWithScreenHost(CodySetupOptions setupOptions, CodyLoadOptions loadOptions)
    {
        Log.Info("Executing boot command with main-thread screen host");
        RunWithMainThreadScreenHost(() =>
        {
            var cody = CodyFactory.CreateCodyForHostedScreen(setupOptions);
            cody.Boot(loadOptions);
        });
    }

    private static Command BuildRunCommand(Option<bool> verboseOption, bool useMainThreadScreenHost)
    {
        var cmd = new Command("run", "Run a binary file with the emulator");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Binary file (raw or cartridge image if --as-cartridge is set)"
        }.AcceptExistingOnly();
        cmd.Arguments.Add(fileArg);

        var asCartridge = new Option<bool>("--as-cartridge")
        {
            Description = "Load <file> as cartridge (expects cartridge header)"
        };

        var loadAddress = new Option<string>("--load-address", ["-l"])
        {
            Description = $"Load address for raw binaries",
            DefaultValueFactory = _ => DefaultLoadAddress
        };

        var resetVector = new Option<string?>("--reset-vector") { Description = "Override Reset Vector (0xFFFC)" };
        var irqVector = new Option<string?>("--irq-vector") { Description = "Override IRQ Vector (0xFFFE)" };
        var nmiVector = new Option<string?>("--nmi-vector") { Description = "Override NMI Vector (0xFFFA)" };
        var executionOptions = AddExecutionOptions(cmd, includeDebug: true);
        
        cmd.Options.Add(asCartridge);
        cmd.Options.Add(loadAddress);
        cmd.Options.Add(resetVector);
        cmd.Options.Add(irqVector);
        cmd.Options.Add(nmiVector);

        cmd.SetAction(parseResult =>
        {
            ApplyLogging(parseResult, verboseOption);

            var inputFile = parseResult.GetValue(fileArg)
                            ?? throw new ArgumentException("Missing input file argument.");

            var loadOptions = BuildSharedLoadOptions(parseResult, executionOptions) with
            {
                File = inputFile,
                AsCartridge = parseResult.GetValue(asCartridge),
                LoadAddress = CliValueParser.ParseAddress(parseResult.GetValue(loadAddress), loadAddress.Name),
                ResetVectorOverride = CliValueParser.ParseOptionalAddress(parseResult.GetValue(resetVector), resetVector.Name),
                IrqVectorOverride = CliValueParser.ParseOptionalAddress(parseResult.GetValue(irqVector), irqVector.Name),
                NmiVectorOverride = CliValueParser.ParseOptionalAddress(parseResult.GetValue(nmiVector), nmiVector.Name)
            };

            var setupOptions = BuildSetupOptions(parseResult, executionOptions);

            if (useMainThreadScreenHost && setupOptions.EnableScreen)
                ExecuteRunCommandWithScreenHost(setupOptions, loadOptions);
            else
                ExecuteRunCommand(setupOptions, loadOptions);

            return 0;
        });

        return cmd;
    }

    private static void ExecuteRunCommand(CodySetupOptions setupOptions, CodyLoadOptions loadOptions)
    {
        Log.Info("Executing run command");

        Cody cody = CodyFactory.CreateCody(setupOptions);
        cody.RunBinaryFile(loadOptions);
    }

    private static void ExecuteRunCommandWithScreenHost(CodySetupOptions setupOptions, CodyLoadOptions loadOptions)
    {
        Log.Info("Executing run command with main-thread screen host");
        RunWithMainThreadScreenHost(() =>
        {
            var cody = CodyFactory.CreateCodyForHostedScreen(setupOptions);
            cody.RunBinaryFile(loadOptions);
        });
    }

    private static void RunWithMainThreadScreenHost(Action commandAction)
    {
        CodyFactory.PrepareScreenHost();

        ExceptionDispatchInfo? commandException = null;

        CodyNET.Frontend.Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime([], desktop =>
        {
            desktop.Startup += (_, _) =>
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        commandAction();
                    }
                    catch (Exception ex)
                    {
                        commandException = ExceptionDispatchInfo.Capture(ex);
                        Log.Error(ex, "Emulator command failed while running with the main-thread screen host.");
                        desktop.TryShutdown(-1);
                    }
                });
            };
        });

        commandException?.Throw();
    }

    private static Command BuildAssembleCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("assemble", "Assemble a source file into a binary");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Assembly source file (.s or .asm)"
        }.AcceptExistingOnly();

        fileArg.CompletionSources.Add(_ =>
        {
            return Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*")
                .Where(file => file.EndsWith(".s", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".asm", StringComparison.OrdinalIgnoreCase))
                .Select(file => new CompletionItem(file));
        });

        cmd.Arguments.Add(fileArg);

        var output = new Option<FileInfo?>("--output", ["-o"])
        {
            Description = "Output binary path (default: <input>.bin)"
        }.AcceptLegalFileNamesOnly();

        var format = new Option<string>("--format")
        {
            Description = "Output format: raw | cartridge",
            DefaultValueFactory = _ => RawFormat
        };

        var loadAddress = new Option<string>("--load-address", ["-l"])
        {
            Description = "Load address used when creating cartridge output",
            DefaultValueFactory = _ => DefaultLoadAddress
        };

        var warnAsError = new Option<bool>("--warn-as-error")
        {
            Description = "Treat assembler warnings as errors when supported by the underlying assembler"
        };

        cmd.Options.Add(output);
        cmd.Options.Add(format);
        cmd.Options.Add(loadAddress);
        cmd.Options.Add(warnAsError);

        cmd.SetAction(parseResult =>
        {
            ApplyLogging(parseResult, verboseOption);

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
        if (warnAsError)
            throw new NotSupportedException("--warn-as-error is not supported by the current 64tass integration.");

        var normalizedFormat = NormalizeFormat(format);
        var program = TassAssembler.AssembleFile(inputFile.FullName);
        if (program.Length == 0)
            throw new InvalidOperationException("Assembler produced no output bytes.");

        var targetFile = outputFile ?? new FileInfo(Path.ChangeExtension(inputFile.FullName, ".bin"));

        byte[] outputBytes = normalizedFormat == CartridgeFormat
            ? CreateCartridgeImage(program, CliValueParser.ParseAddress(loadAddress, "load-address"))
            : program;

        File.WriteAllBytes(targetFile.FullName, outputBytes);
        Log.Info("Assembled {InputFile} -> {OutputFile} ({Format})", inputFile.FullName, targetFile.FullName, normalizedFormat);
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
            Description = "Base address for raw binaries (accepts decimal, 0x-prefixed hex, or bare hex like E000)",
            DefaultValueFactory = _ => DefaultLoadAddress
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
            ApplyLogging(parseResult, verboseOption);

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

    private static void ExecuteDisassembleCommand(FileInfo inputFile, FileInfo? outputFile, bool asCartridge, string? loadAddress)
    {
        outputFile ??= new FileInfo(Path.ChangeExtension(inputFile.FullName, ".s")); // If no output supplied, use same path and name
        Log.Info("Executing disassemble command");
        try
        {
            CodyDisassembler.DisassembleFile(
                inputFile,
                outputFile,
                new CodyDisassemblerOptions
                {
                    AsCartridge = asCartridge,
                    LoadAddress = CliValueParser.ParseAddress(loadAddress, "load-address")
                });
            Log.Info("File disassembled successfully, output written to {outputFile}", outputFile.FullName);
        }
        catch (Exception x)
        {
            Log.Error(x, "Error executing disassemble command");
        }
    }
    
    private static string NormalizeFormat(string? format)
    {
        var normalized = (format ?? RawFormat).Trim().ToLowerInvariant();
        return normalized switch
        {
            RawFormat => RawFormat,
            CartridgeFormat => CartridgeFormat,
            _ => throw new ArgumentException($"Invalid format '{format}'. Supported values: {RawFormat}, {CartridgeFormat}.")
        };
    }

    private static byte[] CreateCartridgeImage(byte[] program, ushort loadAddress)
    {
        var endAddress = loadAddress + program.Length - 1;
        if (endAddress > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(loadAddress), "Program does not fit into a 16-bit cartridge address range.");

        var output = new byte[program.Length + 4];
        output[0] = (byte)(loadAddress & 0xFF);
        output[1] = (byte)(loadAddress >> 8);
        output[2] = (byte)(endAddress & 0xFF);
        output[3] = (byte)(endAddress >> 8);
        Buffer.BlockCopy(program, 0, output, 4, program.Length);
        return output;
    }
}
