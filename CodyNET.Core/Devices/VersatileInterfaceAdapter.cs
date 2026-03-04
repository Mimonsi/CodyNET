using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public class VersatileInterfaceAdapter : IMemoryMappedDevice
{
    public ushort StartAddress { get; } = 0xD200;
    public ushort EndAddress { get; } = 0xD20F;
    public bool SupportsRead { get; } = true;
    public bool SupportsWrite { get; } = false;
    
    public Interrupt Update(long cycle)
    {
        // TODO: Check
        return Interrupt.None;
    }
    
    public byte Read(ushort address)
    {
        return 0;
    }

    public void Write(ushort address, byte value)
    {
        
    }
}