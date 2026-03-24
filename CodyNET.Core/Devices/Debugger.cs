using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public class Breakpoint
{
    public bool Enabled = true;
    public ushort Address;
    public string? Text;
}

public class WatchAddress
{
    public bool Enabled = true;
    public ushort Address;
    public bool WatchRead = true;
    public bool WatchWrite = true;
}

public class Debugger(Cpu cpu) : IMemoryMappedDevice, IMemoryAccessTapDevice
{
    private readonly object _breakpointsLock = new();
    public ushort StartAddress => 0xFF00;
    public ushort EndAddress => 0xFFFF;
    public bool SupportsRead { get; } = false;
    public bool SupportsWrite { get; } = true;
    
    // Addresses to watch for writes
    // TODO: Get to work, IMemoryTap concept?
    public List<WatchAddress> WatchAddresses { get; set; } = [];
    // Breakpoints (Essentially watch PC)
    private List<Breakpoint> Breakpoints { get; } = [];

    // Set to true when breakpoint is hit
    public bool RequestPause;

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
    
    public void AddBreakpoint(ushort address, bool enabled, string? text = null)
    {
        lock (_breakpointsLock)
        {
            if (Breakpoints.All(bp => bp.Address != address))
            {
                Breakpoints.Add(new Breakpoint { Address = address, Enabled = enabled, Text = text });
                Log.Debug("[Debugger] Added breakpoint at address {address:X4}. {text}", address, text ?? "");
                return;
            }
        }

        Log.Warn($"[Debugger] Attempted to add duplicate breakpoint at address {address:X4}");
    }

    public void RemoveBreakpoint(ushort address)
    {
        lock (_breakpointsLock)
        {
            if (Breakpoints.All(bp => bp.Address != address))
            {
                Log.Warn($"[Debugger] Attempted to remove non-existent breakpoint at address {address:X4}");
                return;
            }

            Breakpoints.RemoveAll(bp => bp.Address == address);
        }

        Log.Debug("[Debugger] Removed breakpoint at address {address:X4}", address);
    }

    public bool SetBreakpointEnabled(ushort address, bool enabled)
    {
        lock (_breakpointsLock)
        {
            var breakpoint = Breakpoints.FirstOrDefault(bp => bp.Address == address);
            if (breakpoint == null)
                return false;

            breakpoint.Enabled = enabled;
            return true;
        }
    }

    public bool HasBreakpoints()
    {
        lock (_breakpointsLock)
        {
            return Breakpoints.Count > 0;
        }
    }

    public List<Breakpoint> GetBreakpointsSnapshot()
    {
        lock (_breakpointsLock)
        {
            return Breakpoints
                .Select(bp => new Breakpoint
                {
                    Address = bp.Address,
                    Enabled = bp.Enabled,
                    Text = bp.Text
                })
                .ToList();
        }
    }
    
    public void AddWatch(ushort address, bool enabled=true, bool watchRead=true, bool watchWrite=true)
    {
        if (WatchAddresses.All(w => w.Address != address))
        {
            WatchAddresses.Add(new WatchAddress { Address = address,  Enabled = enabled, WatchRead = watchRead, WatchWrite = watchWrite });
            Log.Debug("[Debugger] Added watch on address {{address:X4}}. R: {watchRead}, W: {watchWrite}", address,
                watchRead, watchWrite);
            return;
        }
        Log.Warn($"[Debugger] Attempted to add duplicate watch on address {address:X4}");
    }
    
    public void RemoveWatch(ushort address)
    {
        if (WatchAddresses.All(w => w.Address != address))
        {
            Log.Warn($"[Debugger] Attempted to remove non-existent watch on address {address:X4}");
            return;
        }
        
        WatchAddresses.RemoveAll(w => w.Address == address);
        Log.Debug("[Debugger] Removed watch on address {{address:X4}}", address);
    }

    private void DBP(byte value)
    {
        Log.Info("[Debugger] DBP: {value}", value);
        RequestPause = true;
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
        try
        {
            lock (_breakpointsLock)
            {
                if (RequestPause)
                {
                    RequestPause = false;
                    return true;
                }
                if (Breakpoints.Count == 0)
                    return false;

                if (Breakpoints.Any(bp => bp is { Enabled: true } && bp.Address == cpu.PC))
                {
                    Log.Info("[Debugger] Breakpoint hit at PC={PC:X4}", cpu.PC);
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Breakpoint error:");
        }

        return false;
    }

    #region Memory Tapping
    public void OnRead(ushort address)
    {
        if (WatchAddresses.Any(w => w.Address == address && w.WatchRead))
        {
            Log.Info("[Debugger] Memory Watch: Read from address {address:X4}", address);
        }
    }

    public void OnWrite(ushort address, byte value)
    {
        if (WatchAddresses.Any(w => w.Address == address && w.WatchWrite))
        {
            Log.Info("[Debugger] Memory Watch: Wrote {value:X2} to address {address:X4}", value, address);
        }
    }
    
    #endregion
}
