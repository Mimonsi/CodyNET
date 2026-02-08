using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using CodyNET.Assembler;
using CodyNET.Disassembler;
using CodyNET.Utils;

namespace CodyNET;

public static class Cli
{
    public static RootCommand BuildRootCommand()
    {
        // Root
        var root = new RootCommand("CodyNET CLI");

        // Optional: global verbose (-v / --verbose) as simple bool for now (help-first)
        var verboseOption = new Option<bool>("--verbose", ["-v"])
        {
            Description = "Enable verbose logging"
        };
        root.Options.Add(verboseOption);

        // Subcommands
        root.Subcommands.Add(BuildListCommand());
        root.Subcommands.Add(BuildRunCommand(verboseOption));
        root.Subcommands.Add(BuildAssembleCommand(verboseOption));
        root.Subcommands.Add(BuildDisassembleCommand(verboseOption));

        // If user runs just `codynet` without a command, show help-like behavior:
        // (Keeping it simple: exit code 0)
        root.SetAction(parseResult =>
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

    private static string GetSubdirFilesText(string pattern, bool recursive = false)
    {
        var text = "";
        var sFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), pattern);
        foreach (var file in sFiles)            
        {
            text += $"  {Path.GetFileName(file)}";
        }

        if (recursive)
        {
            var subdirs = Directory.GetDirectories(Directory.GetCurrentDirectory());
            Dictionary<string, int> matchesPerSubdir = new Dictionary<string, int>();
            foreach (var subdir in subdirs)
            {
                matchesPerSubdir.Add(subdir, Directory.GetFiles(subdir, pattern).Length);
            }
            foreach (var kvp in matchesPerSubdir)
            {
                if (kvp.Value > 0)
                    text += $"\n  {Path.GetFileName(kvp.Key)}/: {kvp.Value} files";
            }
        }
        return text;
    }

    private static Command BuildRunCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("run", "Run a binary file with the emulator");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Binary file (raw or cartridge image if --as-cartridge is set)",
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
            DefaultValueFactory =  _ => "0xE000"
        };

        var resetVector = new Option<string?>("--reset-vector") { Description = "Override Reset Vector (0xFFFC)" };
        var irqVector   = new Option<string?>("--irq-vector")   { Description = "Override IRQ Vector (0xFFFE)" };
        var nmiVector   = new Option<string?>("--nmi-vector")   { Description = "Override NMI Vector (0xFFFA)" };

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
            Description = "Target CPU clock rate (e.g. 1000000, 1MHz, 500kHz)"
        };

        var fast = new Option<bool>("--fast")
        {
            Description = "Run as fast as possible (ignores --clock)"
        };

        // Register options
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

        // Action (placeholder)
        cmd.SetAction(parseResult =>
        {
            if (parseResult.GetValue(verboseOption))
                Log.Level = LogLevel.Verbose;
            ExecuteRunCommand(
                parseResult.GetValue(fileArg),
                parseResult.GetValue(asCartridge),
                parseResult.GetValue(loadAddress),
                parseResult.GetValue(resetVector),
                parseResult.GetValue(irqVector),
                parseResult.GetValue(nmiVector),
                parseResult.GetValue(uart1Source),
                parseResult.GetValue(fixNewlines),
                parseResult.GetValue(physicalKeyboard),
                parseResult.GetValue(debug),
                parseResult.GetValue(clock),
                parseResult.GetValue(fast)
            );
            return 0;
        });

        return cmd;
    }
    
    private static void ExecuteRunCommand(FileInfo inputFile, bool asCartridge, string loadAddress, string? resetVector, string? irqVector, string? nmiVector, string? uart1Source, bool fixNewlines, bool physicalKeyboard, bool debug, string? clock, bool fast)
    {
        // TODO: Implement run call
    }

    private static Command BuildAssembleCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("assemble", "Assemble a source file into a binary");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Assembly source file (.s)",
        }.AcceptExistingOnly();
        fileArg.CompletionSources.Add(ctx =>
        {
            return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.s")
                        .Select(f => new CompletionItem(f));
        }); // Simple completion for .s files in current directory - wasn't able to test it yet
        cmd.Arguments.Add(fileArg);

        var output = new Option<FileInfo?>("--output", ["-o"])
        {
            Description = "Output binary path (default: <input>.bin)",
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
                Log.Level = LogLevel.Verbose;
            ExecuteAssembleCommand(
                parseResult.GetValue(fileArg),
                parseResult.GetValue(output),
                parseResult.GetValue(format),
                parseResult.GetValue(loadAddress),
                parseResult.GetValue(warnAsError)
            );
            return 0;
        });

        return cmd;
    }
    
    private static void ExecuteAssembleCommand(FileInfo inputFile, FileInfo? outputFile, string? format, string? loadAddress, bool warnAsError)
    {
        // TODO: Implement assembler call
    }

    private static Command BuildDisassembleCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("disassemble", "Disassemble a binary into assembly");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Binary file (raw or cartridge)",
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
                Log.Level = LogLevel.Verbose;
            ExecuteDisassembleCommand(parseResult.GetValue(fileArg), parseResult.GetValue(output), parseResult.GetValue(asCartridge), parseResult.GetValue(loadAddress));
            return 0;
        });

        return cmd;
    }
    
    private static void ExecuteDisassembleCommand(FileInfo inputFile, FileInfo? outputFile, bool? asCartridge, string? loadAddress)
    {
        // TODO: Include cartridge header parsing if asCartridge is true (override loadAddress) and load address
        CodyDisassembler.DisassembleFile(inputFile, outputFile);
    }
}
