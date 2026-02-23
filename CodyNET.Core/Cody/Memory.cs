using CodyNET.Common.Utils;
using CodyNET.Core.Interfaces;
using Math = System.Math;

namespace CodyNET.Core.Cody;

public class Memory
{
    private readonly byte[] ram = new byte[0xA000]; // 40 KB of RAM, leaving space for memory-mapped devices (0x0000 - 0x9FFF)
    private readonly byte[] prop = new byte[0x4000]; // 16 KB of Prop RAM, for the Propeller microcontroller (0xA000 - 0xDFFF)
    private readonly byte[] rom = new byte[0x2000]; // 8 KB of ROM, for BASIC and other built-in code (0xE000 - 0xFFFF)
    private List<IMemoryMappedDevice> devices = [];
    // All devices that want to tap into memory access (e.g. for debugging, logging, etc.) can register here and will be notified on every read/write
    private List<IMemoryAccessTapDevice> taps = [];

    /// <summary>
    /// Allows writes to ROM range (used for CPU tests / state restore).
    /// Keep false for normal emulator mode.
    /// </summary>
    public bool RomIsWritable { get; set; } = false;
    
    public void RegisterDevice(IMemoryMappedDevice device)
    {
        Log.Debug($"Registering device {device.GetType().Name} at {device.StartAddress:X4}..{device.EndAddress:X4} (R:{device.SupportsRead} W:{device.SupportsWrite})");
        devices.Add(device);
    }
    
    public void UnregisterDevice(IMemoryMappedDevice device)
    {
        Log.Debug($"Unregistering device {device.GetType().Name} at {device.StartAddress:X4}..{device.EndAddress:X4} (R:{device.SupportsRead} W:{device.SupportsWrite})");
        devices.Remove(device);
    }

    public void RegisterTap(IMemoryAccessTapDevice device)
    {
        Log.Debug($"Registering tap device {device.GetType().Name} for memory access notifications");
        taps.Add(device);
    }
    
    #region Load Memory
    
    public void LoadBytes(byte[] data, ushort startAddress)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) return;

        int remaining = data.Length;
        int src = 0;
        ushort addr = startAddress;

        // 1) RAM: 0000..9FFF
        if (addr < 0xA000 && remaining > 0)
        {
            int ramOff = addr; // addr - 0000
            int can = Math.Min(remaining, 0xA000 - ramOff);
            Buffer.BlockCopy(data, src, ram, ramOff, can);
            src += can; remaining -= can;
            addr = (ushort)(addr + can);
        }

        // 2) PROP: A000..DFFF
        if (addr >= 0xA000 && addr < 0xE000 && remaining > 0)
        {
            int propOff = addr - 0xA000;
            int can = Math.Min(remaining, 0xE000 - addr);
            Buffer.BlockCopy(data, src, prop, propOff, can);
            src += can; remaining -= can;
            addr = (ushort)(addr + can);
        }

        // 3) ROM: E000..FFFF
        if (addr >= 0xE000 && remaining > 0)
        {
            int romOff = addr - 0xE000;
            int can = Math.Min(remaining, 0x10000 - addr); // up to 0xFFFF inclusive

            if (romOff < 0 || romOff + can > rom.Length)
                throw new ArgumentOutOfRangeException(nameof(startAddress), "Write exceeds ROM size.");

            Buffer.BlockCopy(data, src, rom, romOff, can);
            src += can; remaining -= can;
            addr = (ushort)(addr + can);
        }

        // If there's still data, it would wrap past 0xFFFF (Rust would reject / your emulator should reject)
        if (remaining > 0)
            throw new ArgumentOutOfRangeException(nameof(startAddress), "Write exceeds 64KB address space.");
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
            Array.Clear(ram, 0, ram.Length);
            Array.Clear(prop, 0, prop.Length);
            Array.Clear(rom, 0, rom.Length);
        }

        bool oldRomWritable = RomIsWritable;
        RomIsWritable = true;

        foreach (var pair in addressValueList)
        {
            ushort address = (ushort)pair[0];
            byte value = (byte)pair[1];
            Write(address, value);
        }

        RomIsWritable = oldRomWritable;
    }
    
    /// <summary>
    /// Converts non-zero memory values to a list of address-value pairs.
    /// </summary>
    /// <returns></returns>
    public List<int[]> GetAsList()
    {
        var list = new List<int[]>();

        for (int i = 0; i < 0xA000; i++)
        {
            if (ram[i] != 0)
                list.Add([i, ram[i]]);
        }

        for (int i = 0; i < 0x4000; i++)
        {
            if (prop[i] != 0)
                list.Add([0xA000 + i, prop[i]]);
        }

        for (int i = 0; i < 0x2000; i++)
        {
            if (rom[i] != 0)
                list.Add([0xE000 + i, rom[i]]);
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
        foreach(var tap in taps)
            tap.OnRead(address);
        var device = devices.FirstOrDefault(d =>
            d.SupportsRead &&
            address >= d.StartAddress &&
            address <= d.EndAddress);

        if (device != null)
            return device.Read(address);
        
        return address switch
        {
            < 0xA000 => ram[address],
            < 0xE000 => prop[address - 0xA000],
            _        => rom[address - 0xE000],
        };
    }
    
    public void ForceWrite(ushort address, byte value)
    {
        bool oldRomWritable = RomIsWritable;
        RomIsWritable = true;
        Write(address, value);
        RomIsWritable = oldRomWritable;
    }
    
    /// <summary>
    /// Writes a byte to the specified memory address. If the address falls within the range of a registered memory-mapped device,
    /// the write operation is delegated to that device. Otherwise, the byte is written to the internal RAM.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    public void Write(ushort address, byte value)
    {
        foreach(var tap in taps)
            tap.OnWrite(address, value);
        var mapped = devices.Where(d =>
            d.SupportsWrite && address >= d.StartAddress && address <= d.EndAddress).ToList();

        if (mapped.Count > 0)
        {
            foreach (var d in mapped) d.Write(address, value);
            return;
        }
        
        switch (address)
        {
            case < 0xA000:
                ram[address] = value;
                break;
            case < 0xE000:
                prop[address - 0xA000] = value;
                break;
            default:
                // ROM: ignore writes (oder Debug-Log)
                // optional: allow patching vectors via a privileged method
                if (RomIsWritable)
                    rom[address - 0xE000] = value;
                break;
        }
    }
}
