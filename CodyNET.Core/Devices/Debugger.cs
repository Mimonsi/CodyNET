using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;
using CodyNET.Utils;

namespace CodyNET.Core.Devices;

public class Debugger(Cpu cpu) : IMemoryMappedDevice
{
    public ushort StartAddress => 0xFF00;
    public ushort EndAddress => 0xFFFF;
    public bool SupportsRead { get; } = false;
    public bool SupportsWrite { get; } = true;
    private Cpu cpu = cpu;
    bool ReadAllowed => false;


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

    private void DBP(byte value)
    {
        Log.Info($"DBP: {value}");
    }
    
    private void DRS(byte index)
    {
        Log.Info($"Register Dump #{index}\nPC={cpu.PC:X4} A={cpu.A:X2} X={cpu.X:X2} Y={cpu.Y:X2} S={cpu.S:X2} P={cpu.Status.ToByte():X2}");
    }
    
    private void DMP(byte index)
    {
        string text = $"Memory Dump #{index}:\n";
        foreach(var kvp in cpu.Memory.ram.Select((value, index) => new { value, index })) // TODO: Do not access ram directly
        {
            if (kvp.value != 0)
            {
                text += $"[{kvp.index:X4}] = {kvp.value:X2}\n";
            }
        }
        Log.Info(text);
    }
}