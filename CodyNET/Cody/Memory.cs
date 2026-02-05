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
    
    #region Load Memory
    
    public void CopyFrom(byte[] program, ushort startAddress)
    {
        Array.Copy(program, 0, ram, startAddress, program.Length);
    }

    /// <summary>
    /// Sets memory values from a list of address-value pairs.
    /// </summary>
    /// <param name="addressValueList"></param>
    /// <param name="zeroFill">If enabled (default) zero any other value</param>
    public void SetFromList(List<int[]> addressValueList, bool zeroFill = true)
    {
        if (zeroFill)
        {
            for(int i = 0; i < ram.Length; i++)
            {
                ram[i] = 0;
            }
        }
        foreach (var pair in addressValueList)
        {
            ushort address = (ushort)pair[0];
            byte value = (byte)pair[1];
            Write(address, value);
        }
    }
    
    /// <summary>
    /// Converts non-zero memory values to a list of address-value pairs.
    /// </summary>
    /// <returns></returns>
    public List<int[]> GetAsList()
    {
        var list = new List<int[]>();
        for (int i = 0; i < ram.Length; i++)
        {
            if (ram[i] != 0)
            {
                list.Add([i, ram[i]]);
            }
        }
        return list;
    }
    
    #endregion
    
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
        var mappedDevices = devices.Where(x => x.StartAddress < address && x.EndAddress > address).ToList();

        if (mappedDevices.Count == 0)
        {
            ram[address] = value;
            return;
        }
        foreach (var mappedDevice in mappedDevices)
        {
            mappedDevice.Write(address, value);
        }
    }
    
    public void Push(ushort address, byte value)
    {
        ram[address--] = value;
    }
}