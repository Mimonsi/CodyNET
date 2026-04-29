using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CodyNET.Assembler;

/// <summary>
/// Replaces custom CodyNET debug commands by their actual implementations
/// </summary>
internal static class CodyPreprocessor
{
    private static Dictionary<string, string> commandReplacements = new Dictionary<string, string>
    {
        // Example: "DBP" is replaced by "STZ $FF00" (a dummy instruction that does nothing but can be used as a breakpoint)
        { "DBP", "STZ $$FF00" },
        { "DRS", "STZ $$FF01" },
        { "DMP", "STZ $$FF02" },
    };
    internal static void PreprocessFile(FileInfo inputFile, FileInfo? outputFile = null)
    {
        if (!inputFile.Exists)
            throw new  FileNotFoundException("Input file does not exist", inputFile.FullName);
        if (outputFile == null)
        {
            outputFile = new FileInfo(inputFile.FullName.Replace(inputFile.Extension, "_pre.asm"));
        }

        var inputCode = File.ReadAllText(inputFile.FullName);
        var outputCode = Preprocess(inputCode);
        if (outputFile.Directory is { Exists: false })
        {
            outputFile.Directory.Create();
        }

        File.WriteAllText(outputFile.FullName, outputCode);
    }
    
    internal static string Preprocess(string code)
    {
        return Preprocess(code.Split('\n').ToList());
    }

    internal static string Preprocess(List<string> lines)
    {
        var code = "";

        foreach (var line in lines)
        {
            string codeLine = line;

            if (!string.IsNullOrEmpty(codeLine))
            {
                foreach (var replacement in commandReplacements) // TODO: Test
                {
                    codeLine = Regex.Replace(
                        codeLine,
                        @$"^(\s*){Regex.Escape(replacement.Key)}\b",
                        "$1" + replacement.Value,
                        RegexOptions.IgnoreCase);
                }
            }

            code += $"{codeLine}\n";
        }

        return code;
    }
}
