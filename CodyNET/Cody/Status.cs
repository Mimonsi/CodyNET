namespace CodyNET.Cody;

/// <summary>
/// Processor Flags
/// </summary>
public struct Status
{
    public bool Carry;
    public bool Zero;
    public bool InterruptDisable;
    public bool DecimalMode;
    public bool BreakCommand;
    public bool Overflow;
    public bool Negative;

    public Status()
    {
        Carry = false;
        Zero = false;
        InterruptDisable = false;
        DecimalMode = false;
        BreakCommand = false;
        Overflow = false;
        Negative = false;
    }
    
    public Status (byte statusByte)
    {
        Carry = (statusByte & 0x01) != 0;
        Zero = (statusByte & 0x02) != 0;
        InterruptDisable = (statusByte & 0x04) != 0;
        DecimalMode = (statusByte & 0x08) != 0;
        BreakCommand = (statusByte & 0x10) != 0;
        Overflow = (statusByte & 0x40) != 0;
        Negative = (statusByte & 0x80) != 0;
    }

    public byte ToByte()
    {
        byte p = 0x20;
        if (Carry) p |= 0x01;
        if (Zero) p |= 0x02;
        if (InterruptDisable) p |= 0x04;
        if (DecimalMode) p |= 0x08;
        if (BreakCommand) p |= 0x10;
        if (Overflow) p |= 0x40;
        if (Negative) p |= 0x80;
        return p;
    }

}