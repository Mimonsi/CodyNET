using System.CommandLine;

namespace CodyNET.Core;

class Program
{
    public static int Main(string[] args)
    {
        bool interactive = false;

        // Filter args: detect -i / --interactive
        var filteredArgs = new List<string>();

        foreach (var arg in args)
        {
            if (arg == "-i" || arg == "--interactive")
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

            return InteractiveShell.Run(root);
        }

        // Normal one-shot CLI
        return root.Parse(args).Invoke();
    }
}