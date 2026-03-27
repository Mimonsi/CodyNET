using System.Text;
using CodyNET.Common.Utils;

namespace CodyNET.Assembler;

/// <summary>
/// Wrapper for TassAssembler that includes preprocessing
/// </summary>
public class CodyAssembler
{
    public static byte[] AssembleFileToBytes(FileInfo inputFile)
    {
        var outputFile = FileUtils.GetWithChangedExtension(inputFile, ".bin");
        AssembleFile(inputFile, outputFile);
        return File.ReadAllBytes(outputFile.FullName);
    }
    
    public static FileInfo AssembleFile(FileInfo inputFile, FileInfo? outputFile = null)
    {
        if (!inputFile.Exists)
            throw new FileNotFoundException("Input file does not exist", inputFile.FullName);
            
        if (outputFile == null)
        {
            outputFile = FileUtils.GetWithChangedExtension(inputFile, ".bin");
        }
        CodyPreprocessor.PreprocessFile(inputFile, outputFile);
        TassAssembler.AssembleFile(inputFile, outputFile);
        return outputFile;
    }
    
    public static byte[] AssembleTextToBytes(string assemblyCode)
    {
        if (string.IsNullOrEmpty(assemblyCode))
            throw new ArgumentNullException(nameof(assemblyCode));

        var guid = Guid.NewGuid().ToString("N");
        string tempInputFile = Path.Combine(Path.GetTempPath(), $"cody_{guid}.asm");
        string tempOutputFile = Path.Combine(Path.GetTempPath(), $"cody_{guid}.asm");

        try
        {
            File.WriteAllText(tempInputFile, assemblyCode, Encoding.UTF8);
            AssembleFile(new FileInfo(tempInputFile), new FileInfo(tempOutputFile));
            return File.ReadAllBytes(tempOutputFile);
        }
        finally
        {
            TryDelete(tempInputFile);
            TryDelete(tempOutputFile);
        }
    }
    
    private static void TryDelete(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Intentionally ignore cleanup failures.
        }
    }
}