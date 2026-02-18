using System.CommandLine;
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

        // Interactive REPL
        if (interactive)
        {
            // Optional: execute initial command first
            if (filteredArgs.Count > 0)
            {
                root.Parse(filteredArgs.ToArray()).Invoke();
            }

            Log.Level = LogLevel.Trace;
            Log.ConsoleLevel = LogLevel.Debug;
            Log.Info($"Starting logger. Log File Path: {Log.LogFilePath}");
            return InteractiveShell.Run(root);
        }

        // Normal one-shot CLI
        return root.Parse(args).Invoke();
    }
}
