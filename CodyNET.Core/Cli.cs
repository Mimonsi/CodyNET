using System.CommandLine;
using System.CommandLine.Parsing;
using CodyNET.Assembler;
using CodyNET.Disassembler;

namespace CodyNET;

public static class Cli
{
    public static RootCommand BuildRootCommand()
    {
        // Root
        var root = new RootCommand("CodyNET CLI");

        // Optional: global verbose (-v / --verbose) as simple bool for now (help-first)
        var verboseOption = new Option<bool>("--verbose")
        {
            Description = "Enable verbose logging"
        };
        verboseOption.Aliases.Add("-v");
        root.Options.Add(verboseOption);

        // Subcommands
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

    private static Command BuildRunCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("run", "Run a binary file with the emulator");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Binary file (raw or cartridge image if --as-cartridge is set)"
        };
        cmd.Arguments.Add(fileArg);

        var asCartridge = new Option<bool>("--as-cartridge")
        {
            Description = "Load <file> as cartridge (expects cartridge header)"
        };

        var loadAddress = new Option<string>("--load-address")
        {
            Description = "Load address for raw binaries (default: 0xE000)"
        };
        loadAddress.Aliases.Add("-l");

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

        var debug = new Option<bool>("--debug")
        {
            Description = "Enable debugger (interactive)"
        };
        debug.Aliases.Add("-d");

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
            // Values are retrieved via parseResult.GetValue(option/argument). :contentReference[oaicite:3]{index=3}
            var file = parseResult.GetValue(fileArg);
            bool isVerbose = parseResult.GetValue(verboseOption);
            bool isFast = parseResult.GetValue(fast);
            bool isDebug = parseResult.GetValue(debug);

            Console.WriteLine($"[run] file={file} verbose={isVerbose} fast={isFast} debug={isDebug}");
            return 0;
        });

        return cmd;
    }

    private static Command BuildAssembleCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("assemble", "Assemble a source file into a binary");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Assembly source file (.s / .asm)"
        };
        cmd.Arguments.Add(fileArg);

        var output = new Option<FileInfo?>("--output")
        {
            Description = "Output binary path (default: <input>.bin)"
        };
        output.Aliases.Add("-o");

        var format = new Option<string?>("--format")
        {
            Description = "Output format: raw | cartridge"
        };

        var loadAddress = new Option<string>("--load-address")
        {
            Description = "Load address metadata / ORG for raw output (default: 0xE000)"
        };
        loadAddress.Aliases.Add("-l");

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
            var file = parseResult.GetValue(fileArg);
            bool isVerbose = parseResult.GetValue(verboseOption);
            Console.WriteLine($"[assemble] file={file} verbose={isVerbose}");
            return 0;
        });

        return cmd;
    }

    private static Command BuildDisassembleCommand(Option<bool> verboseOption)
    {
        var cmd = new Command("disassemble", "Disassemble a binary into assembly");

        var fileArg = new Argument<FileInfo>("file")
        {
            Description = "Binary file (raw or cartridge)"
        };
        cmd.Arguments.Add(fileArg);

        var asCartridge = new Option<bool>("--as-cartridge")
        {
            Description = "Treat <file> as cartridge image (header present)"
        };

        var loadAddress = new Option<string>("--load-address")
        {
            Description = "Base address for raw binaries (default: 0xE000)"
        };
        loadAddress.Aliases.Add("-l");

        var output = new Option<FileInfo?>("--output")
        {
            Description = "Output assembly path (default: <input>.asm)"
        };
        output.Aliases.Add("-o");

        cmd.Options.Add(asCartridge);
        cmd.Options.Add(loadAddress);
        cmd.Options.Add(output);

        cmd.SetAction(parseResult =>
        {
            var inputFile = parseResult.GetValue(fileArg);
            var outputFile = parseResult.GetValue(output);
            //bool isVerbose = parseResult.GetValue(verboseOption);
            CodyDisassembler.DisassembleFile(inputFile, outputFile);
            //Console.WriteLine($"[disassemble] file={file} verbose={isVerbose}");
            return 0;
        });

        return cmd;
    }
}
