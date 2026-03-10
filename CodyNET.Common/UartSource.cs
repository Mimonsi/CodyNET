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

        foreach (var line in File.ReadLines(file.FullName))
        {
            if (string.IsNullOrEmpty(line))
                continue;

            data.AddRange(System.Text.Encoding.ASCII.GetBytes(line));
            data.Add((byte)'\n');
        }

        // CodyBASIC LOAD termination line
        data.Add((byte)'\n');
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