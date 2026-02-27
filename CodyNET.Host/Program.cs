using System.CommandLine;
using System.Diagnostics;
using CodyNET.Common.Utils;

namespace CodyNET.Host;

public class Program
{
    public static int Main(string[] args)
    {
        Log.TimeSetting = TimeSetting.Relative;
        Log.StartNewFileOnStartup = true;
        Log.Initialize();
        bool interactive = false;
        
        // TODO: Decide start behavior without parameters:
        // 1. Run Boot command when no parameters are supplied
        // 2. Start in interactive mode when no parameters are supplied
        // 3. Show help when no parameters are supplied

        // Filter args: detect -i / --interactive
        var filteredArgs = new List<string>();

        if (args.Length == 0) // When no arguments are provided, start in interactive mode by default
        {
            interactive = true;
        }
        
        foreach (var arg in args)
        {
            if (arg is "-i" or "--interactive")
            {
                interactive = true;
            }
            else
            {
                filteredArgs.Add(arg);
            }
        }
        
        RootCommand root = Cli.BuildRootCommand();
        var invocationConfig = new InvocationConfiguration // if debugger is attached, don't catch exceptions
        {
            EnableDefaultExceptionHandler = !Debugger.IsAttached
        };

        // Interactive REPL
        if (interactive)
        {
            // Optional: execute initial command first
            if (filteredArgs.Count > 0)
            {
                root.Parse(filteredArgs.ToArray()).Invoke(invocationConfig);
            }
            return InteractiveShell.Run(root, invocationConfig);
        }
        Log.Level = LogLevel.Debug;
        Log.ConsoleLevel = LogLevel.Debug;
        Log.Info("Starting logger. Log File Path: {LogFilePath}", Log.LogFilePath!);

        // Normal one-shot CLI
        return root.Parse(args).Invoke(invocationConfig);
    }
}