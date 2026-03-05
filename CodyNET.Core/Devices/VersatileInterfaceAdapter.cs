using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public class VersatileInterfaceAdapter : IMemoryMappedDevice
{
    private const ushort VIA_BASE = 0x9F00;
    private const byte IFR_TIMER1 = 0x40;

    public ushort StartAddress { get; } = VIA_BASE;
    public ushort EndAddress { get; } = VIA_BASE + 0x0F;
    public bool SupportsRead { get; } = true;
    public bool SupportsWrite { get; } = true;

    private readonly byte[] registers = new byte[0x10];
    private int timer1Counter;
    private ushort timer1Latch;
    private long lastCycle;
    
    public Interrupt Update(long cycle)
    {
        if (lastCycle == 0)
        {
            lastCycle = cycle;
            return Interrupt.None;
        }

        var delta = cycle - lastCycle;
        lastCycle = cycle;

        if (delta <= 0 || timer1Counter <= 0)
        {
            return IsIrqActive() ? new Interrupt { IRQ = true } : Interrupt.None;
        }

        timer1Counter -= (int)delta;

        if (timer1Counter <= 0)
        {
            SetInterruptFlag(IFR_TIMER1);

            // ACR bit 6 = Timer1 continuous mode.
            if ((registers[0x0B] & IFR_TIMER1) != 0)
            {
                timer1Counter = timer1Latch > 0 ? timer1Latch : 1;
            }
            else
            {
                timer1Counter = 0;
            }
        }

        return IsIrqActive() ? new Interrupt { IRQ = true } : Interrupt.None;
    }
    
    public byte Read(ushort address)
    {
        var offset = (byte)(address - VIA_BASE);
        return offset switch
        {
            0x04 => ReadTimer1CounterLow(),
            0x05 => (byte)((ushort)Math.Max(timer1Counter, 0) >> 8),
            0x0D => GetInterruptFlags(),
            0x0E => (byte)(registers[0x0E] | 0x80),
            _ => registers[offset]
        };
    }

    public void Write(ushort address, byte value)
    {
        var offset = (byte)(address - VIA_BASE);
        registers[offset] = value;

        switch (offset)
        {
            case 0x04:
                timer1Latch = (ushort)((timer1Latch & 0xFF00) | value);
                break;
            case 0x05:
                timer1Latch = (ushort)((value << 8) | (timer1Latch & 0x00FF));
                timer1Counter = timer1Latch > 0 ? timer1Latch : 1;
                ClearInterruptFlag(IFR_TIMER1);
                break;
            case 0x0D:
                // Writing 1s clears corresponding IFR bits.
                registers[0x0D] = (byte)(registers[0x0D] & ~value);
                break;
            case 0x0E:
                // IER bit 7 selects set/clear for bits 0-6.
                if ((value & 0x80) != 0)
                {
                    registers[0x0E] = (byte)(registers[0x0E] | (value & 0x7F));
                }
                else
                {
                    registers[0x0E] = (byte)(registers[0x0E] & ~(value & 0x7F));
                }

                break;
        }
    }

    private byte ReadTimer1CounterLow()
    {
        ClearInterruptFlag(IFR_TIMER1);
        return (byte)Math.Max(timer1Counter, 0);
    }

    private void SetInterruptFlag(byte mask)
    {
        registers[0x0D] = (byte)(registers[0x0D] | mask);
    }

    private void ClearInterruptFlag(byte mask)
    {
        registers[0x0D] = (byte)(registers[0x0D] & ~mask);
    }

    private byte GetInterruptFlags()
    {
        var flags = (byte)(registers[0x0D] & 0x7F);
        if ((flags & registers[0x0E]) != 0)
        {
            flags |= 0x80;
        }

        return flags;
    }

    private bool IsIrqActive() => (registers[0x0D] & registers[0x0E] & 0x7F) != 0;
}