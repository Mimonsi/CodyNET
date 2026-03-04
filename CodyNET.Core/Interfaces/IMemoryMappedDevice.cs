using CodyNET.Core.Cody;

namespace CodyNET.Core.Interfaces;

/// <summary>
/// Represents a memory-mapped device with read and write capabilities.
/// </summary>
public interface IMemoryMappedDevice
{
    /// <summary>
    /// Address range that the device responds to. Read/Write calls within this range will be propagated to the device.
    /// </summary>
    ushort StartAddress { get; }
    ushort EndAddress { get; }
    /// <summary>
    /// Indicates whether the device handles reads within its address range.
    /// </summary>
    bool SupportsRead { get; }

    /// <summary>
    /// Indicates whether the device handles writes within its address range.
    /// </summary>
    bool SupportsWrite { get; }

    Interrupt Update(long cycle);
    
    byte Read(ushort address);
    void Write(ushort address, byte value);
}