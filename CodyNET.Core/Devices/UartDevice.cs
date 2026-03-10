using CodyNET.Common;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public class UartDevice(ushort startAddress, UartSource? source = null) : IMemoryMappedDevice
{
    public ushort StartAddress { get; init; } = startAddress;
    public ushort EndAddress => (ushort)(StartAddress + UART_SIZE - 1);  // 32 bytes for UART registers
    public bool SupportsRead { get; } = true;
    public bool SupportsWrite { get; } = true;
    
    //public const ushort UART1_BASE = 0xD480;
    //public const ushort UART2_BASE = 0xD4A0;
    
    /// <summary> Control register </summary>
    public const ushort UART_CNTL = 0;
    /// <summary> Command register </summary>
    public const ushort UART_CMND = 1;
    /// <summary> Status register </summary>
    public const ushort UART_STAT = 2;
    /// <summary> Receive ring buffer head register </summary>
    public const ushort UART_RXHD = 4;
    /// <summary> Receive ring buffer tail register </summary>
    public const ushort UART_RXTL = 5;
    /// <summary> Transmit ring buffer head register </summary>
    public const ushort UART_TXHD = 6;
    /// <summary> Transmit ring buffer tail register </summary>
    public const ushort UART_TXTL = 7;
    /// <summary> Ring buffer size </summary>
    public const ushort UART_BUFFER_SIZE = 8;
    /// <summary> Receive ring buffer (8 bytes) </summary>
    public const ushort UART_RXBF = 8;
    /// <summary> Transmit ring buffer (8 bytes) </summary>
    public const ushort UART_TXBF = UART_RXBF + UART_BUFFER_SIZE;
    /// <summary> End location </summary>
    public const ushort UART_SIZE = UART_TXBF + UART_BUFFER_SIZE;

    private byte _control;
    private byte _command;
    private byte _status;
    private RingBuffer _receiveBuffer = new(UART_BUFFER_SIZE);
    private RingBuffer _transmitBuffer = new(UART_BUFFER_SIZE);
    private UartSource _source = source ?? UartSource.Empty;
    public UartSource Source
    {
        get => _source;
        set
        {
            _source = value ?? UartSource.Empty;
            Log.Info("UART source set to {Source}", _source); // TODO: Check if additional steps are required
        }
    }
    public Interrupt Update(long cycle)
    {
        UpdateState();

        if (!IsEnabled())
            return Interrupt.None;

        FlushTransmit();
        FillReceive();
        
        return Interrupt.None;
    }

    public bool IsEnabled() => (_command & 0x1) != 0; // Bit 0 of command register enables the UART

    public void UpdateState()
    {
        if (IsEnabled())
            _status = 0x40;
        else
        {
            _status = 0x0;
            _receiveBuffer.Reset();
            _transmitBuffer.Reset();
        }
    }
    
    public byte Read(ushort address)
    {
        ushort offset = (ushort)(address - StartAddress);
        return offset switch
        {
            UART_CNTL => _control,
            UART_CMND => _command,
            UART_STAT => _status,
            UART_RXHD => _receiveBuffer.Head,
            UART_RXTL => _receiveBuffer.Tail,
            UART_TXHD => _transmitBuffer.Head,
            UART_TXTL => _transmitBuffer.Tail,
            >= UART_RXBF and < UART_TXBF => _receiveBuffer.Get((byte)(offset - UART_RXBF)),
            >= UART_TXBF and < UART_SIZE => _transmitBuffer.Get((byte)(offset - UART_TXBF)),
            _ => 0 // Unmapped addresses return 0
        };
    }

    public void Write(ushort address, byte value)
    {
        ushort offset = (ushort)(address - StartAddress);
        switch (offset)
        {
            case UART_CNTL:
                _control = value;
                break;
            case UART_CMND:
                _command = value;
                break;
            case UART_STAT:
                // Status register is read-only
                break;
            case UART_RXHD:
                _receiveBuffer.Head = value;
                break;
            case UART_RXTL:
                _receiveBuffer.Tail = value;
                break;
            case UART_TXHD:
                _transmitBuffer.Head = value;
                break;
            case UART_TXTL:
                _transmitBuffer.Tail = value;
                break;
            default:
                if (offset is >= UART_RXBF and < UART_TXBF)
                    _receiveBuffer.Set((byte)(offset - UART_RXBF), value);
                else if (offset is >= UART_TXBF and < UART_SIZE)
                    _transmitBuffer.Set((byte)(offset - UART_TXBF), value);
                break;
        }
    }

    private void FlushTransmit()
    {
        while (true)
        {
            byte? data = _transmitBuffer.Pop();
            if (!data.HasValue)
                break;
            // For demonstration, we just log the transmitted byte as a character
            char c = (char)data.Value;
            Log.Verbose("UART Transmit: '{c}' ({char})", c, ToPrintable(data.Value));
        }
    }

    private void FillReceive()
    {
        while (!_receiveBuffer.IsFull)
        {
            byte? value = _source.Read();
            if (!value.HasValue)
                break;

            _receiveBuffer.Push(value.Value);
            
            Log.Verbose("UART rx: push byte {Byte} ({Char}), remaining {Position}/{Length}",
            value.Value,
            ToPrintable(value.Value),
            _source.Position,
            _source.Length);
        }
    }
    
    private static string ToPrintable(byte value)
    {
        char c = (char)value;
        return char.IsControl(c) ? "." : c.ToString();
    }
}