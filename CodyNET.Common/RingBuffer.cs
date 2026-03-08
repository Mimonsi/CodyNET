namespace CodyNET.Common;

public class RingBuffer
{
    private readonly byte[] _buffer;
    private byte _head;
    private byte _tail;

    public RingBuffer(int capacity = 8)
    {
        if (capacity is <= 1 or > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _buffer = new byte[capacity];
    }

    public byte Capacity => (byte)_buffer.Length;

    public byte Head
    {
        get => _head;
        set => _head = (byte)(value % Capacity);
    }

    public byte Tail
    {
        get => _tail;
        set => _tail = (byte)(value % Capacity);
    }

    public byte Length => (byte)((_head - _tail + Capacity) % Capacity);

    public bool IsEmpty => _head == _tail;

    public bool IsFull => (byte)((_head + 1) % Capacity) == _tail;

    public bool Push(byte value)
    {
        if (IsFull)
            return false;

        _buffer[_head] = value;
        _head = (byte)((_head + 1) % Capacity);
        return true;
    }

    public byte? Pop()
    {
        if (IsEmpty)
            return null;

        byte value = _buffer[_tail];
        _tail = (byte)((_tail + 1) % Capacity);
        return value;
    }

    public byte Get(byte index)
    {
        return _buffer[index % Capacity];
    }

    public void Set(byte index, byte value)
    {
        _buffer[index % Capacity] = value;
    }

    public void Reset()
    {
        _head = 0;
        _tail = 0;
        Array.Clear(_buffer);
    }
}