using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public enum CodyKeyCode : byte
{
    // Row 1
    KeyQ = 0,
    KeyE = 1,
    KeyT = 2,
    KeyU = 3,
    KeyO = 4,
    // Row 2
    KeyA = 5,
    KeyD = 6,
    KeyG = 7,
    KeyJ = 8,
    KeyL = 9,
    // Row 3
    Cody = 10,
    KeyX = 11,
    KeyV = 12,
    KeyN = 13,
    Meta = 14,
    // Row 4
    KeyZ = 15,
    KeyC = 16,
    KeyB = 17,
    KeyM = 18,
    Arrow = 19,
    // Row 5
    KeyS = 20,
    KeyF = 21,
    KeyH = 22,
    KeyK = 23,
    Space = 24,
    // Row 6
    KeyW = 25,
    KeyR = 26,
    KeyY = 27,
    KeyI = 28,
    KeyP = 29,
    Joystick1Up = 30,
    Joystick1Down = 31,
    Joystick1Left = 32,
    Joystick1Right = 33,
    Joystick1Fire = 34,
    Joystick2Up = 35,
    Joystick2Down = 36,
    Joystick2Left = 37,
    Joystick2Right = 38,
    Joystick2Fire = 39,
}

public class KeyState
{
    public readonly byte[] State = new byte[8];

    public KeyState()
    {
        for(int i = 0;i < State.Length; i++)
        {
            State[i] = 0xFF; // Unpressed keys read as 1.
        }
    }
    
    /// <summary>
    /// Set a key as currently pressed
    /// </summary>
    /// <param name="code"></param>
    /// <param name="pressed"></param>
    public void SetPressed(CodyKeyCode code, bool pressed)
    {
        byte c = (byte)code;

        // Rust:
        // bit = (code % 5) + 3;
        // index = code / 5;
        int bit = (c % 5) + 3;   // Bits 3..7
        int index = c / 5;       // 0..7 (for 0..39)

        byte mask = (byte)(1 << bit);

        // Active-low:
        // pressed -> clear bit
        // released -> set bit
        if (pressed) State[index] = (byte)(State[index] & ~mask);
        else         State[index] = (byte)(State[index] |  mask);
    }
}

public class VersatileInterfaceAdapter : IMemoryMappedDevice
{
    private const ushort VIA_BASE = 0x9F00;
    // VIA register offsets.
    // Input/Output Register B
    private const ushort VIA_IORB = 0x00;
    // Input/Output Register A
    private const ushort VIA_IORA = 0x01;
    // Data Direction Register B
    private const ushort VIA_DDRB = 0x02;
    // Data Direction Register A
    private const ushort VIA_DDRA = 0x03;
    // Timer 1 Counter Low
    private const ushort VIA_T1CL = 0x04;
    // Timer 1 Counter High
    private const ushort VIA_T1CH = 0x05;
    // Timer 1 Latch Low
    private const ushort VIA_T1LL = 0x06;
    // Timer 1 Latch High
    private const ushort VIA_T1LH = 0x07;
    // Timer 2 Counter Low
    private const ushort VIA_T2CL = 0x08;
    // Timer 2 Counter High
    private const ushort VIA_T2CH = 0x09;
    // Shift Register
    private const ushort VIA_SR = 0x0A;
    // Auxiliary Control Register
    private const ushort VIA_ACR = 0x0B;
    // Peripheral Control Register
    private const ushort VIA_PCR = 0x0C;
    // Interrupt Flag Register
    private const ushort VIA_IFR = 0x0D;
    // Interrupt Enable Register
    private const ushort VIA_IER = 0x0E;
    // VIA IORA NO HANDSHAKE
    private const ushort VIA_IORA_NOHS = 0x0F;
    
    private const byte IFR_TIMER1 = 0x40;
    private const byte IFR_TIMER2 = 0x20;
    
    private readonly byte[] _registers = new byte[16];
    
    public KeyState KeyState { get; } = new();

    private long _lastUpdateCycle;

    private byte _t1LatchLo, _t1LatchHi;
    private ushort _t1Counter;
    private bool _t1Enabled;

    private byte _t2LatchLo, _t2LatchHi;
    private ushort _t2Counter;
    private bool _t2Enabled;

    private byte _ifr;
    private byte _ier;

    public ushort StartAddress { get; } = VIA_BASE;
    public ushort EndAddress { get; } = VIA_BASE + 0x0F;
    public bool SupportsRead { get; } = true;
    public bool SupportsWrite { get; } = true;
    
    private int timer1Counter;
    private ushort timer1Latch;
    private long lastCycle;

    /// <summary>
    /// Returns current state of keyboard
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public byte ReadIora()
    {
        byte ddr = _registers[VIA_DDRA];
        byte ior = _registers[VIA_IORA];

        if (ddr != 0x07)
            throw new NotSupportedException($"VIA: read IORA expects DDRA=0x07 (Cody), but was 0x{ddr:X2}");

        byte output = (byte)(ior & ddr);
        byte keys;
        lock (KeyState)
        {
            keys = KeyState.State[output];
        }

        return (byte)(keys | output);
    }

    /// <summary>
    /// Set the state of the interrupt flag register, which controls which interrupts are currently active.
    /// </summary>
    /// <param name="ifr"></param>
    private void SetIfr(byte ifr)
    {
        ifr = (byte)(ifr & 0x7F);
        
        if ((ifr & _ier) != 0)
            ifr |= 0x80; // Set bit 7 if any enabled interrupts are active.
        
        _ifr = ifr;
    }

    /// <summary>
    /// Set the state of the interrupt enable register, which controls which interrupts are currently enabled.
    /// </summary>
    /// <param name="value"></param>
    private void SetIer(byte value)
    {
        if ((value & 0x80) != 0)
            _ier |= (byte)(value & 0x7F); // Set bits 0..6 for enabled interrupts, if bit 7 is set.
        else
            _ier &= (byte)~(value & 0x7F); // Clear bits 0..6 for disabled interrupts, if bit 7 is not set.

        SetIfr(value);
    }

    public byte Read(ushort address)
    {
        ushort offset = (ushort)(address - VIA_BASE);
        return offset switch
        {
            VIA_IORA => ReadIora(),

            VIA_T1CL => ReadT1CL(),
            VIA_T1CH => (byte)(_t1Counter >> 8),
            VIA_T1LL => _t1LatchLo,
            VIA_T1LH => _t1LatchHi,

            VIA_T2CL => ReadT2CL(),
            VIA_T2CH => (byte)(_t2Counter >> 8),

            VIA_IFR => _ifr,
            VIA_IER => (byte)(_ier | 0x80),

            <= 0x0F => _registers[offset],

            _ => 0
        };
    }
    
    /// <summary>
    /// Read Timer 1 Counter Low, which also clears the Timer 1 interrupt flag in the IFR.
    /// </summary>
    /// <returns></returns>
    byte ReadT1CL()
    {
        SetIfr((byte)(_ifr & ~IFR_TIMER1));
        return (byte)(_t1Counter & 0xFF);
    }

    /// <summary>
    /// Read Timer 2 Counter Low, which also clears the Timer 2 interrupt flag in the IFR.
    /// </summary>
    /// <returns></returns>
    byte ReadT2CL()
    {
        SetIfr((byte)(_ifr & ~IFR_TIMER2));
        return (byte)(_t2Counter & 0xFF);
    }

    public void Write(ushort address, byte value)
    {
        ushort offset = (ushort)(address - VIA_BASE);
        switch (offset)
        {
            case VIA_T1CL:
                _t1LatchLo = value;
                break;

            case VIA_T1CH: // Writing to Timer 1 Counter High also starts the timer, per 6522 behavior.
                _t1LatchHi = value;
                SetIfr((byte)(_ifr & ~IFR_TIMER1));
                _t1Counter = (ushort)(_t1LatchLo | (_t1LatchHi << 8));
                _t1Enabled = true;
                break;

            case VIA_T1LL:
                _t1LatchLo = value;
                break;

            case VIA_T1LH: // Writing to Timer 1 Latch High does NOT start the timer, but does clear the Timer 1 interrupt flag in the IFR.
                _t1LatchHi = value;
                SetIfr((byte)(_ifr & ~IFR_TIMER1));
                break;

            case VIA_T2CL:
                _t2LatchLo = value;
                break;

            case VIA_T2CH:
                _t2LatchHi = value;
                SetIfr((byte)(_ifr & ~IFR_TIMER2));
                _t2Counter = (ushort)(_t2LatchLo | (_t2LatchHi << 8));
                _t2Enabled = true;
                break;

            case VIA_IFR:
                // Attention: Real 6522 behavior is different, but this should work
                SetIfr(value);
                break;

            case VIA_IER:
                SetIer(value);
                break;

            default:
                if (offset <= 0x0F)
                    _registers[offset] = value;
                break;
        }
    }

    /// <summary>
    /// Update state of via, reduce timer per cpu cycle
    /// </summary>
    /// <param name="cycle"></param>
    /// <returns></returns>
    public Interrupt Update(long cycle)
    {
        var cyclesElapsed = cycle - _lastUpdateCycle;
        _lastUpdateCycle = cycle;

        byte acr = _registers[VIA_ACR];

        for (long i = 0; i < cyclesElapsed; i++)
        {
            // Timer1
            _t1Counter = (ushort)((_t1Counter - 1));
            if (_t1Counter == 0) // If counter reaches 0, set interrupt flag and reload counter from latch.
            {
                if (_t1Enabled)
                {
                    SetIfr((byte)(_ifr | IFR_TIMER1)); // Timer1 flag
                    
                    // ACR bit6: continuous? -> Disable timer if not continuous, otherwise reload from latch.
                    if ((acr & IFR_TIMER1) == 0)
                        _t1Enabled = false;
                }
                
                // Reload from latch
                _t1Counter = (ushort)(_t1LatchLo | (_t1LatchHi << 8));

                if ((acr & 0x80) != 0)
                {
                    // TODO: PB7 toggling
                }
            }
            
            // Timer2
            if ((acr & 0x20) != 0)
            {
                // TODO: count PB6 pulses
            }
            else
            {
                _t2Counter = (ushort)(_t2Counter - 1);
            }

            if (_t2Counter == 0 && _t2Enabled)
            {
                SetIfr((byte)(_ifr | 0x20)); // Timer2 flag
                _t2Enabled = false;
            }
        }
        return (_ifr & 0x80) != 0 ? Interrupt.IrqReq : Interrupt.None;
    }
}