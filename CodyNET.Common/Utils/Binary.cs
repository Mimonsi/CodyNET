using System.Globalization;

namespace CodyNET.Common.Utils;

public static class Binary
{
    /// <summary>
    /// Get bytes from a file containing hex strings separated by spaces/new lines.
    /// (Not used by the Rust-style loader; kept for tests/tools.)
    /// </summary>
    public static byte[] LoadBinaryText(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var byteStrings = content.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);

        var bytes = new byte[byteStrings.Length];
        for (int i = 0; i < byteStrings.Length; i++)
            bytes[i] = Convert.ToByte(byteStrings[i], 16);

        return bytes;
    }

    /// <summary>
    /// Rust reference binary loader behavior:
    /// - Reads the file as raw bytes (no PRG 2-byte header support).
    /// - If asCartridge=true: expects a 4-byte header:
    ///     [start_lo][start_hi][end_lo][end_hi][data...]
    ///   and truncates data to (end-start+1) bytes, like the Rust drain(4..len+4).
    /// - Final load address:
    ///     overrideLoadAddress ?? (cartridge_start if asCartridge) ?? 0xE000
    /// - Asserts data is not empty.
    /// </summary>
    public static (ushort loadAddress, byte[] data) LoadBinary(
        string filePath,
        bool asCartridge = false,
        ushort? overrideLoadAddress = null,
        ushort defaultLoadAddress = 0xE000)
    {
        byte[] data = File.ReadAllBytes(filePath);

        ushort? loadAddress = overrideLoadAddress;

        if (asCartridge)
        {
            if (data.Length < 4)
                throw new InvalidDataException("Cartridge header must be at least 4 bytes.");

            ushort cartridgeStart = ReadU16LE(data, 0);
            ushort cartridgeEnd   = ReadU16LE(data, 2);

            if (cartridgeStart > cartridgeEnd)
                throw new InvalidDataException("Cartridge start address must be <= end address.");

            // len = end - start + 1, with overflow safety
            int len = (cartridgeEnd - cartridgeStart) + 1;

            int available = data.Length - 4;
            if (available < len)
                throw new InvalidDataException(
                    $"Cartridge data len {available} must be >= implied header len {len}.");

            // Rust: data = data.drain(4..(len + 4)).collect();
            var sliced = new byte[len];
            Buffer.BlockCopy(data, 4, sliced, 0, len);
            data = sliced;

            // Rust: load_address = load_address.or(Some(cartridge_load_address));
            loadAddress ??= cartridgeStart;
        }

        if (data.Length == 0)
            throw new InvalidDataException("Data must not be empty.");

        ushort finalLoadAddress = loadAddress ?? defaultLoadAddress;
        return (finalLoadAddress, data);
    }

    private static ushort ReadU16LE(byte[] bytes, int offset)
        => (ushort)(bytes[offset] | (bytes[offset + 1] << 8));

    // Optional: if you still want hex parsing elsewhere (not Rust-like):
    public static byte[] ParseHexText(string content)
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

            if (token.StartsWith("$", StringComparison.Ordinal))
                token = token[1..];
            else if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                token = token[2..];

            bytes[i] = byte.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }
}
