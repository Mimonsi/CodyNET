using System.CommandLine;

namespace CodyNET.Core;

public static class InteractiveShell
{
    public static int Run(RootCommand root)
    {
        Console.WriteLine("CodyNET interactive CLI");
        Console.WriteLine("Type 'help' for help, 'exit' to quit.");
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
                // Equivalent to: codynet --help
                var pr = root.Parse(new[] { "--help" });
                _ = pr.Invoke();
                continue;
            }

            // Allow users to type commands without leading executable name.
            // Example: run program.bin --fast
            string[] args;
            try
            {
                args = CommandLineTokenizer.Tokenize(line).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Parse error: {ex.Message}");
                continue;
            }

            try
            {
                ParseResult parseResult = root.Parse(args);
                int exitCode = parseResult.Invoke();
                if (exitCode != 0)
                    Console.WriteLine($"(exit code {exitCode})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error: {ex.Message}");
            }
        }
    }

    // Minimal tokenizer: supports quotes, e.g. --uart1-source "C:\path with spaces\in.txt"
    private static class CommandLineTokenizer
    {
        public static IEnumerable<string> Tokenize(string input)
        {
            var token = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && char.IsWhiteSpace(c))
                {
                    if (token.Length > 0)
                    {
                        yield return token.ToString();
                        token.Clear();
                    }
                    continue;
                }

                token.Append(c);
            }

            if (inQuotes)
                throw new FormatException("Unterminated quote.");

            if (token.Length > 0)
                yield return token.ToString();
        }
    }
}
