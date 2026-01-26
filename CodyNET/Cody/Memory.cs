using CodyNET.Interfaces;

namespace CodyNET.Cody;

public class Memory
{
    private const int DEFAULT_RAM_SIZE = 0x10000; // 64 KB = 65536 bytes
    public readonly byte[] ram;
    private List<IMemoryMappedDevice> devices = new();
    public int Size => ram.Length;

    public Memory()
    {
        ram = new byte[DEFAULT_RAM_SIZE];
    }
    
    public Memory(int size)
    {
        ram = new byte[size];
    }
    
    public void RegisterDevice(IMemoryMappedDevice device)
    {
        devices.Add(device);
    }
    
    /// <summary>
    /// Reads a byte from the specified memory address. If the address falls within the range of a registered memory-mapped device,
    /// the read operation is delegated to that device. Otherwise, the byte is read from the internal RAM.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public byte Read(ushort address)
    {
        var device = devices.FirstOrDefault(d => 
            address >= d.StartAddress && address <= d.EndAddress);
        
        return device?.Read(address) ?? ram[address];
    }
    
    /// <summary>
    /// Writes a byte to the specified memory address. If the address falls within the range of a registered memory-mapped device,
    /// the write operation is delegated to that device. Otherwise, the byte is written to the internal RAM.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    public void Write(ushort address, byte value)
    {
        var device = devices.FirstOrDefault(d => 
            address >= d.StartAddress && address <= d.EndAddress);
        
        if (device != null)
            device.Write(address, value);
        else
            ram[address] = value;
    }
    
    public void CopyFrom(byte[] program, ushort startAddress)
    {
        Array.Copy(program, 0, ram, startAddress, program.Length);
    }
}