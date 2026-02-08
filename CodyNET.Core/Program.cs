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

    /*private static void Test1()
    {
        // Tests breakpoint with other debug DRG behind it
        Log.Info("Starting Test1");
        Cpu.Cpu cpu = new();
        var bytes = GetBytesFromFile("testdata/drs_after_dbp.bin");
        cpu.LoadProgram(bytes, 0x0600);
        cpu.RunUntilFinish();
        Log.Info("Done");
    }
    
    public static byte[] GetBytesFromFile(string filePath)
    {
        var file = Path.Combine(@"C:\Users\Konsi\GoogleDrive\Uni\Bachelorarbeit\CodyNET.Core\CodyPrototype.Tests", filePath);
        // Remove all strings and new lines
        var content = File.ReadAllText(file);
        var byteStrings = content.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);
        var bytes = new byte[byteStrings.Length];
        for (int i = 0; i < byteStrings.Length; i++)
        {            
            bytes[i] = Convert.ToByte(byteStrings[i], 16);
        }
        return bytes;
    }*/
}