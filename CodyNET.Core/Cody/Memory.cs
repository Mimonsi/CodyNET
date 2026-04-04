using System.Text;
using CodyNET.Common.Utils;
using CodyNET.Core.Interfaces;
using Math = System.Math;

namespace CodyNET.Core.Cody;

public class Memory
{
    private readonly byte[] _ram = new byte[0xA000]; // 40 KB of RAM, leaving space for memory-mapped devices (0x0000 - 0x9FFF)
    private readonly byte[] _prop = new byte[0x4000]; // 16 KB of Prop RAM, for the Propeller microcontroller (0xA000 - 0xDFFF)
    private readonly byte[] _rom = new byte[0x2000]; // 8 KB of ROM, for BASIC and other built-in code (0xE000 - 0xFFFF)
    private readonly List<IMemoryMappedDevice> _devices = [];
    // For efficient memory access, each read/write operation checks the exact address to see which device is mapped. This is far more efficient than iterating through all devices each time
    private readonly IMemoryMappedDevice?[] _readDeviceMap = new IMemoryMappedDevice?[ushort.MaxValue + 1];
    private readonly List<IMemoryMappedDevice>?[] _writeDeviceMap = new List<IMemoryMappedDevice>?[ushort.MaxValue + 1];
    // All devices that want to tap into memory access (e.g. for debugging, logging, etc.) can register here and will be notified on every read/write
    private IMemoryAccessTapDevice[] _memoryTaps = [];

    /// <summary>
    /// Allows writes to ROM range (used for CPU tests / state restore).
    /// Keep false for normal emulator mode.
    /// </summary>
    public bool RomIsWritable { get; set; } = false;
    
    public void RegisterDevice(IMemoryMappedDevice device)
    {
        Log.Debug("Registering device {DeviceType} at {StartAddress:X4}..{EndAddress:X4} (R:{SupportsRead} W:{SupportsWrite})",
            device.GetType().Name, device.StartAddress, device.EndAddress, device.SupportsRead, device.SupportsWrite);
        _devices.Add(device);
        RebuildDeviceMaps();
    }
    
    public void UnregisterDevice(IMemoryMappedDevice device)
    {
        Log.Debug("Unregistering device {DeviceType} at {StartAddress:X4}..{EndAddress:X4} (R:{SupportsRead} W:{SupportsWrite})",
            device.GetType().Name, device.StartAddress, device.EndAddress, device.SupportsRead, device.SupportsWrite);
        _devices.Remove(device);
        RebuildDeviceMaps();
    }

    /// <summary>
    /// For efficient read/write operations, memory maps
    /// </summary>
    private void RebuildDeviceMaps()
    {
        Array.Clear(_readDeviceMap, 0, _readDeviceMap.Length);

        for (int i = 0; i < _writeDeviceMap.Length; i++)
            _writeDeviceMap[i]?.Clear();

        for (int i = 0; i < _devices.Count; i++)
        {
            var device = _devices[i];
            for (int addr = device.StartAddress; addr <= device.EndAddress; addr++)
            {
                if (device.SupportsRead && _readDeviceMap[addr] == null)
                    _readDeviceMap[addr] = device;

                if (!device.SupportsWrite)
                    continue;

                var writeTargets = _writeDeviceMap[addr];
                if (writeTargets == null)
                {
                    writeTargets = [];
                    _writeDeviceMap[addr] = writeTargets;
                }

                writeTargets.Add(device);
            }
        }
    }

    public void RegisterTap(IMemoryAccessTapDevice device)
    {
        Log.Debug("Registering tap device {DeviceType} for memory access notifications", device.GetType().Name);
        // Sacrifice performance in RegisterTap for better performance in iteration
        var tempList = _memoryTaps.ToList();
        tempList.Add(device);
        _memoryTaps = tempList.ToArray();
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
            Buffer.BlockCopy(data, src, _ram, ramOff, can);
            src += can; remaining -= can;
            addr = (ushort)(addr + can);
        }

        // 2) PROP: A000..DFFF
        if (addr >= 0xA000 && addr < 0xE000 && remaining > 0)
        {
            int propOff = addr - 0xA000;
            int can = Math.Min(remaining, 0xE000 - addr);
            Buffer.BlockCopy(data, src, _prop, propOff, can);
            src += can; remaining -= can;
            addr = (ushort)(addr + can);
        }

        // 3) ROM: E000..FFFF
        if (addr >= 0xE000 && remaining > 0)
        {
            int romOff = addr - 0xE000;
            int can = Math.Min(remaining, 0x10000 - addr); // up to 0xFFFF inclusive

            if (romOff < 0 || romOff + can > _rom.Length)
                throw new ArgumentOutOfRangeException(nameof(startAddress), "Write exceeds ROM size.");

            Buffer.BlockCopy(data, src, _rom, romOff, can);
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
            Array.Clear(_ram, 0, _ram.Length);
            Array.Clear(_prop, 0, _prop.Length);
            Array.Clear(_rom, 0, _rom.Length);
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
            if (_ram[i] != 0)
                list.Add([i, _ram[i]]);
        }

        for (int i = 0; i < 0x4000; i++)
        {
            if (_prop[i] != 0)
                list.Add([0xA000 + i, _prop[i]]);
        }

        for (int i = 0; i < 0x2000; i++)
        {
            if (_rom[i] != 0)
                list.Add([0xE000 + i, _rom[i]]);
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
    public byte Read(ushort address) // PERFORMANCE CRITICAL
    {
        if (_memoryTaps.Length > 0)
            foreach(var tap in _memoryTaps)
                tap.OnRead(address);

        var device = _readDeviceMap[address];
        if (device != null)
            return device.Read(address);
        
        return address switch
        {
            < 0xA000 => _ram[address],
            < 0xE000 => _prop[address - 0xA000],
            _        => _rom[address - 0xE000],
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
    public void Write(ushort address, byte value) // PERFORMANCE CRITICAL
    {
        if (_memoryTaps.Length > 0)
            foreach(var tap in _memoryTaps)
                tap.OnWrite(address, value);

        var mapped = _writeDeviceMap[address];
        if (mapped != null)
        {
            for (int i = 0; i < mapped.Count; i++)
                mapped[i].Write(address, value);
            return;
        }
        
        switch (address)
        {
            case < 0xA000:
                _ram[address] = value;
                break;
            case < 0xE000:
                _prop[address - 0xA000] = value;
                break;
            default:
                // ROM: ignore writes (oder Debug-Log)
                // optional: allow patching vectors via a privileged method
                if (RomIsWritable)
                    _rom[address - 0xE000] = value;
                break;
        }
    }

    /// <summary>
    /// Updates all registered memory-mapped devices that require periodic updates
    /// </summary>
    /// <param name="totalCyclesExecuted"></param>
    /// <returns></returns>
    public Interrupt Update(long totalCyclesExecuted)
    {
        var interrupt = Interrupt.None;
        foreach (var device in _devices)
        {
            interrupt = interrupt.Or(device.Update(totalCyclesExecuted));
        }

        return interrupt;
    }

    public string GetMemoryOverview()
    {
        // Loop through whole memory and print start and end address of each device in-order
        
        // Output format
        /*
        0x0000..0x0010: RAM
        0x0011..0x3FFF: Device A
        0x4000..0x7FFF: Device B
        0x8000..0x9FFF: RAM
        0xA000..0xBFFF: Prop RAM
         */
        
        var sb = new StringBuilder();
        int currentAddress = 0;
        var allDevices = _devices.OrderBy(d => d.StartAddress).ToList();
        foreach (var device in allDevices)
        {
            if (device.StartAddress > currentAddress)
            {
                // There's a gap before this device, which is RAM
                sb.AppendLine($"0x{currentAddress:X4}..0x{device.StartAddress - 1:X4}: RAM");
            }
            sb.AppendLine($"0x{device.StartAddress:X4}..0x{device.EndAddress:X4}: {device.GetType().Name}");
            currentAddress = device.EndAddress + 1;
        }

        return sb.ToString();
    }
}
