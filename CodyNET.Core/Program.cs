using System.CommandLine;
using System.Diagnostics;
using CodyNET.Common.Utils;

namespace CodyNET.Core;

internal static class Program
{
    public static int Main(string[] args)
    {
        Log.TimeSetting = TimeSetting.Relative;
        Log.StartNewFileOnStartup = true;
        Log.Initialize();
        bool interactive = false;

        // Filter args: detect -i / --interactive
        var filteredArgs = new List<string>();

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
        var invocationConfig = new InvocationConfiguration // if debugger is attach, don't catch exceptions
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
        Log.Info($"Starting logger. Log File Path: {Log.LogFilePath}");

        // Normal one-shot CLI
        return root.Parse(args).Invoke(invocationConfig);
    }
}
