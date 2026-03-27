using System.Text.RegularExpressions;

namespace CodyNET.Assembler;

/// <summary>
/// Replaces custom CodyNET debug commands by their actual implementations
/// </summary>
public static class CodyPreprocessor
{
    public static void PreprocessFile(FileInfo inputFile, FileInfo? outputFile = null)
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
    
    public static string Preprocess(string code)
    {
        return Preprocess(code.Split('\n').ToList());
    }

    public static string Preprocess(List<string> lines)
    {
        var code = "";

        foreach (var line in lines)
        {
            string codeLine = line;

            if (!string.IsNullOrEmpty(codeLine))
            {
                var dbpText = "LDA #$$01\nSTA $$FF00";
                codeLine = Regex.Replace(
                    codeLine,
                    @"^(\s*)DBP\b",
                    "$1" + dbpText,
                    RegexOptions.IgnoreCase);
            }

            code += $"{codeLine}\n";
        }

        return code;
    }
}
