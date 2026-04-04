using CodyNET.Common.Utils;

namespace CodyNET.Common;

public class UartSource
{
    private readonly byte[] _source;
    private int _position;

    public static UartSource Empty { get; } = new(Array.Empty<byte>());

    private UartSource(byte[] source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        Reset();
    }

    public static UartSource FromUtf8String(string str)
    {
        return new UartSource(System.Text.Encoding.UTF8.GetBytes(str));
    }
    
    public static UartSource FromAsciiString(string str)
    {
        return new UartSource(System.Text.Encoding.ASCII.GetBytes(str));
    }

    public static UartSource FromFile(FileInfo file, bool normalizeLineEndings)
    {
        if (!file.Exists)
            throw new FileNotFoundException("UART source file not found", file.FullName);

        var data = new List<byte>();
        
        // Debug Output
        // var x = File.ReadAllBytes(file.FullName);
        // var text = $"UART1 data length: {x.Length}\n";
        // for (int i = 0; i < x.Length; i++)
        // {
        //     byte b = x[i];
        //
        //     string display = b switch
        //     {
        //         (byte)'\n' => "\\n",
        //         (byte)'\r' => "\\r",
        //         (byte)'\t' => "\\t",
        //         >= 0x20 and <= 0x7E => ((char)b).ToString(),
        //         _ => $"\\x{b:X2}"
        //     };
        //
        //     text += $"[{i:0000}] 0x{b:X2} {b,3} {display}\n";
        // }
        //
        // Log.Verbose(text);

        // Normalize Line Ending only works with text based files, not byte based!
        if (normalizeLineEndings)
        {
            foreach (var line in File.ReadLines(file.FullName))
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                foreach (char c in line)
                {
                    // Rust l.bytes() returns exact byte value of character in ASCII.
                    // For CodyBASIC-Source ASCII/UTF-8 is fitting.
                    data.Add((byte)c);
                }

                data.Add((byte)'\n');
            }
            // CodyBASIC LOAD termination line
            data.Add((byte)'\n');
        }
        else
        {
            data = File.ReadAllBytes(file.FullName).ToList();
        }
        return new UartSource(data.ToArray());
    }
    
    public int Position => _position;
    public int Length => _source.Length;
    public bool IsEmpty => _source.Length == 0;
    public bool HasNext => _position < _source.Length;

    public byte? Read()
    {
        if (!HasNext)
            return null;
        return _source[_position++];
    }

    public void Reset()
    {
        _position = 0;
    }
}