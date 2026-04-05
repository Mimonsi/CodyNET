using System.Globalization;
using System.Text;
using CodyNET.Common.Utils;

namespace CodyNET.Assembler;

/// <summary>
/// Wrapper for TassAssembler that includes preprocessing
/// </summary>
public static class CodyAssembler
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
        var preFile = FileUtils.GetWithChangedExtension(inputFile, "_pre.asm");
        CodyPreprocessor.PreprocessFile(inputFile, preFile);
        TassAssembler.AssembleFile(preFile, outputFile);
        return outputFile;
    }

    /// <summary>
    /// Assembles the file and additionally returns a mapping from assembled addresses to
    /// line numbers in the preprocessed source (editor_pre.asm).  The caller is responsible
    /// for translating those pre-asm line numbers to original editor line numbers.
    /// </summary>
    public static (FileInfo Output, Dictionary<ushort, int> AddressToPreLine) AssembleFileWithMap(
        FileInfo inputFile, FileInfo? outputFile = null)
    {
        if (!inputFile.Exists)
            throw new FileNotFoundException("Input file does not exist", inputFile.FullName);

        outputFile ??= FileUtils.GetWithChangedExtension(inputFile, ".bin");

        var preFile  = FileUtils.GetWithChangedExtension(inputFile, "_pre.asm");
        var listFile = FileUtils.GetWithChangedExtension(preFile,   ".lst");

        CodyPreprocessor.PreprocessFile(inputFile, preFile);
        TassAssembler.AssembleFile(preFile, outputFile, listFile);

        var addressToPreLine = ParseListingFile(listFile, preFile);
        return (outputFile, addressToPreLine);
    }

    // ── Listing file parser ───────────────────────────────────────────────────

    /// <summary>
    /// Parses a 64tass --list output file and returns a mapping of
    /// assembled address-> 1-based line number in the preprocessed .asm file.
    ///
    /// 64tass listing format (tab-separated, 6 columns):
    ///   [0] prefix+offset  e.g. ".0004" (code), "=$3000" (const), ">0000" (abs data)
    ///   [1] PC address     e.g. "3000"  (empty for = and > lines)
    ///   [2] hex bytes      e.g. "a9 01" (empty for labels/directives without output)
    ///   [3] (reserved)
    ///   [4] disassembly    e.g. "lda #$01"
    ///   [5] source text    exact copy of the corresponding editor_pre.asm line
    ///
    /// We match listing entries against editor_pre.asm by source text.  Duplicates are
    /// consumed in declaration order via per-text queues, so repeated instructions map
    /// to the correct (sequential) source line.
    /// </summary>
    private static Dictionary<ushort, int> ParseListingFile(FileInfo listFile, FileInfo preAsmFile)
    {
        var result = new Dictionary<ushort, int>();
        if (!listFile.Exists || !preAsmFile.Exists) return result;

        // Build a queue per trimmed source text so duplicate instructions are matched
        // in the order they appear in the source file.
        var preAsmLines = File.ReadAllLines(preAsmFile.FullName);
        var textToLineQueues = new Dictionary<string, Queue<int>>(StringComparer.Ordinal);
        for (int i = 0; i < preAsmLines.Length; i++)
        {
            var text = preAsmLines[i].Trim();
            if (string.IsNullOrEmpty(text)) continue;
            if (!textToLineQueues.TryGetValue(text, out var q))
                textToLineQueues[text] = q = new Queue<int>();
            q.Enqueue(i + 1); // 1-based
        }

        foreach (var line in File.ReadAllLines(listFile.FullName))
        {
            // Only '.' prefix lines are actual source code / data entries.
            // '=' lines are constant definitions, '>' lines are pre-org absolute data,
            // ';' lines are listing header comments - all irrelevant for PC mapping.
            if (!line.StartsWith('.')) continue;

            var cols = line.Split('\t');
            // Need at minimum: [0] offset, [1] PC, [2] bytes, [^1] source.
            // NOTE: the number of columns varies with instruction length - 64tass omits
            // the padding tab for 3-byte instructions, giving 5 cols instead of 6.
            // Therefore we use cols[^1] (last element) for the source text, not a fixed index.
            if (cols.Length < 3) continue;

            var pcStr    = cols[1].Trim();   // PC address (hex)
            var hexBytes = cols[2].Trim();   // assembled bytes (empty -> no code emitted)
            var srcText  = cols[^1].Trim();  // verbatim source line (always the last column)

            // Skip entries that don't emit bytes (pure labels, .org, etc.)
            if (string.IsNullOrEmpty(pcStr) || string.IsNullOrEmpty(hexBytes)) continue;
            if (!ushort.TryParse(pcStr, NumberStyles.HexNumber, null, out var address)) continue;
            if (string.IsNullOrEmpty(srcText)) continue;

            // Match against the pre.asm source text to get the 1-based line number.
            if (textToLineQueues.TryGetValue(srcText, out var queue) && queue.Count > 0)
                result.TryAdd(address, queue.Dequeue());
        }

        return result;
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