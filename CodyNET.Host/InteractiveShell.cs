using System.CommandLine;
using System.Diagnostics;

namespace CodyNET.Host;

public static class InteractiveShell
{
    private const string Cyan = "\u001b[36m";
    private const string Reset = "\u001b[0m";

    public static int Run(RootCommand root, InvocationConfiguration invocationConfiguration)
    {
        Console.WriteLine("CodyNET interactive CLI");
        Console.WriteLine($"Type '{Cyan}help{Reset}' for help, '{Cyan}exit{Reset}' to quit.");
        Console.WriteLine($"Type '{Cyan}boot{Reset}' to run the emulator into the BASIC Shell.");
        Console.WriteLine();

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line is null)
                return 0;

            line = line.Trim();
            if (line.Length == 0)
                continue;

            if (line.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (line.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
                continue;
            }

            if (line.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                _ = root.Parse("--help").Invoke(invocationConfiguration);
                continue;
            }

            try
            {
                var parseResult = root.Parse(line);
                int exitCode = parseResult.Invoke(invocationConfiguration);
                if (exitCode != 0)
                    Console.WriteLine($"(exit code {exitCode})");
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    throw;

                Console.WriteLine($"Unhandled error: {ex.Message}");
            }
        }
    }
}