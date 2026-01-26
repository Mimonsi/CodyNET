namespace CodyNET.Interfaces;

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
    
    byte Read(ushort address);
    void Write(ushort address, byte value);
}