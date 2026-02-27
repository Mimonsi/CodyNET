using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public class Debugger(Cpu cpu) : IMemoryMappedDevice, IMemoryAccessTapDevice
{
    public ushort StartAddress => 0xFF00;
    public ushort EndAddress => 0xFFFF;
    public bool SupportsRead { get; } = false;
    public bool SupportsWrite { get; } = true;
    
    // Addresses to watch for writes
    // TODO: Get to work, IMemoryTap concept?
    public List<ushort> WatchAddresses { get; set; } = [];
    // Breakpoints (Essentially watch PC)
    public List<ushort> Breakpoints { get; set; } = [];
    
    public byte Read(ushort address)
    {
        return 0;
    }

    public void Write(ushort address, byte value)
    {
       switch (address)
       {
           case 0xFF00:
               DBP(value);
               break;
           case 0xFF01:
               DRS(value);
               break;
           case 0xFF02:
               DMP(value);
               break; 
            default:
                Log.Warn($"Invalid write to Debugger at address {address:X4}");
               break;
       }
    }
    
    public void AddWatch(ushort address)
    {
        if (!WatchAddresses.Contains(address))
        {
            WatchAddresses.Add(address);
            Log.Debug($"[Debugger] Added watch on address {address:X4}");
        }
    }
    
    public void RemoveWatch(ushort address)
    {
        if (WatchAddresses.Contains(address))
        {
            WatchAddresses.Remove(address);
            Log.Debug($"[Debugger] Removed watch on address {address:X4}");
        }
    }

    private void DBP(byte value)
    {
        Log.Info("[Debugger] DBP: {value}", value);
    }
    
    private void DRS(byte index)
    {
        Log.Info("[Debugger] Register Dump #{index}\nPC={PC:X4} A={A:X2} X={X:X2} Y={Y:X2} S={S:X2} P={P:X2}",
            index, cpu.PC, cpu.A, cpu.X, cpu.Y, cpu.S, cpu.Status.ToByte());
    }
    
    private void DMP(byte index)
    {
        // TODO: Implement
        /*string text = $"Memory Dump #{index}:\n";
        foreach(var kvp in cpu.Memory.ram.Select((value, index) => new { value, index })) // TODO: Do not access ram directly
        {
            if (kvp.value != 0)
            {
                text += $"[{kvp.index:X4}] = {kvp.value:X2}\n";
            }
        }
        Log.Info(text);*/
    }

    /// <summary>
    /// CPU Step event
    /// </summary>
    public bool IsAtBreakpoint()
    {
        if (Breakpoints.Count > 0 && Breakpoints.Contains(cpu.PC))
        {
            Log.Info("[Debugger] Breakpoint hit at PC={PC:X4}", cpu.PC);
            return true;
        }

        return false;
    }

    #region Memory Tapping
    public void OnRead(ushort address)
    {
        // TODO: Differentiate between write and read watch
        /*if (WatchAddresses.Contains(address))
        {
            Log.Info($"[Debugger] Memory Watch: Read from address {address:X4}");
        }*/
    }

    public void OnWrite(ushort address, byte value)
    {
        if (WatchAddresses.Contains(address))
        {
            Log.Info("[Debugger] Memory Watch: Wrote {value:X2} to address {address:X4}", value, address);
        }
    }
    
    #endregion
}