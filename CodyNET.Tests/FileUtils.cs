using System.Globalization;
using System.Text;

namespace CodyNET.Tests;

public class FileUtils
{
    public static string GetTestDataPath(string filename)
    {
        return Path.Combine(AppContext.BaseDirectory, "testdata", filename);
    }

    /// <summary>
    /// Get bytes from a file containing hex strings separated by spaces/new lines
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static byte[] GetBytesFromFile(string filePath)
    {
        // Remove all strings and new lines
        var content = File.ReadAllText(filePath);
        var byteStrings = content.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);
        var bytes = new byte[byteStrings.Length];
        for (int i = 0; i < byteStrings.Length; i++)
        {            
            bytes[i] = Convert.ToByte(byteStrings[i], 16);
        }
        return bytes;
    }
    
        /// <summary>
    /// Loads a program file and returns the opcode bytes and the load address.
    /// Supports:
    /// - Hex text files (e.g., "A9 01 8D ...", optional BOM, commas, $ or 0x prefixes)
    /// - Binary PRG-like files where the first two bytes are the load address (little-endian)
    /// </summary>
    public static (ushort loadAddress, byte[] bytes) LoadProgram(string filePath, ushort defaultLoadAddress = 0x0600)
    {
        // Read raw bytes first to decide the format.
        byte[] raw = File.ReadAllBytes(filePath);

        if (LooksLikeHexText(raw))
        {
            // Parse as hex text.
            string text = ReadTextRemovingUtf8Bom(raw);
            byte[] bytes = ParseHexText(text);
            return (defaultLoadAddress, bytes);
        }

        // Parse as binary with 2-byte load address header.
        if (raw.Length < 3)
            throw new InvalidDataException("Binary program file is too small to contain a load address + data.");

        ushort loadAddress = (ushort)(raw[0] | (raw[1] << 8));
        byte[] bytesWithoutHeader = raw.Skip(2).ToArray();

        return (loadAddress, bytesWithoutHeader);
    }

    /// <summary>
    /// Heuristic: treat the file as hex text if all bytes are ASCII whitespace or hex-related characters.
    /// This safely distinguishes your minimal.bin (hex text) from codybros.bin (binary).
    /// </summary>
    private static bool LooksLikeHexText(byte[] raw)
    {
        // Skip UTF-8 BOM if present: EF BB BF
        int i = 0;
        if (raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF)
            i = 3;

        for (; i < raw.Length; i++)
        {
            byte b = raw[i];

            // Common separators
            if (b == (byte)' ' || b == (byte)'\n' || b == (byte)'\r' || b == (byte)'\t' || b == (byte)',' || b == (byte)';')
                continue;

            // Hex digits
            bool isHexDigit =
                (b >= (byte)'0' && b <= (byte)'9') ||
                (b >= (byte)'A' && b <= (byte)'F') ||
                (b >= (byte)'a' && b <= (byte)'f');

            if (isHexDigit)
                continue;

            // Optional prefixes: "$" or "0x"/"0X"
            if (b == (byte)'$' || b == (byte)'x' || b == (byte)'X' || b == (byte)'0')
                continue;

            // If we hit any other byte, it's not hex text.
            return false;
        }

        // If it is empty (or only whitespace), treat as text (will fail later with a clear error).
        return true;
    }

    private static string ReadTextRemovingUtf8Bom(byte[] raw)
    {
        // Decode as UTF-8; if it's truly binary, we never call this.
        string text = Encoding.UTF8.GetString(raw);

        // Remove BOM if it survived decoding (rare but harmless).
        return text.TrimStart('\uFEFF');
    }

    /// <summary>
    /// Parses hex tokens into bytes. Supports tokens like:
    /// A9, 01, $A9, $01, 0xA9, 0x01
    /// </summary>
    private static byte[] ParseHexText(string content)
    {
        var tokens = content
            .Split(new[] { ' ', '\n', '\r', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToArray();

        if (tokens.Length == 0)
            throw new InvalidDataException("Hex text file contained no byte tokens.");

        var bytes = new byte[tokens.Length];

        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];

            // Strip common prefixes
            if (token.StartsWith("$", StringComparison.Ordinal))
                token = token.Substring(1);
            else if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(2);

            bytes[i] = byte.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }
}