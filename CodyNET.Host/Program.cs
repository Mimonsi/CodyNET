using System.CommandLine;
using System.Diagnostics;
using CodyNET.Common.Utils;

namespace CodyNET.Host;

public class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        Log.TimeSetting = TimeSetting.Relative;
        Log.StartNewFileOnStartup = true;
        Log.Initialize();

        var startupOptions = ParseStartupOptions(args);
        RootCommand root = Cli.BuildRootCommand();
        var invocationConfig = new InvocationConfiguration
        {
            EnableDefaultExceptionHandler = !Debugger.IsAttached
        };

        if (startupOptions.Interactive)
        {
            if (startupOptions.CommandArgs.Length > 0)
                root.Parse(startupOptions.CommandArgs).Invoke(invocationConfig);

            return InteractiveShell.Run(root, invocationConfig);
        }

        Log.Level = LogLevel.Debug;
        Log.ConsoleLevel = LogLevel.Debug;
        Log.Info("Starting logger. Log File Path: {LogFilePath}", Log.LogFilePath!);

        return root.Parse(startupOptions.CommandArgs).Invoke(invocationConfig);
    }

    private static StartupOptions ParseStartupOptions(string[] args)
    {
        var interactive = args.Length == 0;
        var filteredArgs = new List<string>(args.Length);

        foreach (var arg in args)
        {
            if (arg is "-i" or "--interactive")
            {
                interactive = true;
                continue;
            }

            filteredArgs.Add(arg);
        }

        return new StartupOptions(interactive, [.. filteredArgs]);
    }

    private sealed record StartupOptions(bool Interactive, string[] CommandArgs);
}