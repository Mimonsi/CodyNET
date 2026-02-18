using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public class Keyboard : IInputDevice
{
    public ushort StartAddress { get; }
    public ushort EndAddress { get; }
    public bool SupportsRead { get; }
    public bool SupportsWrite { get; }
    public byte Read(ushort address)
    {
        throw new NotImplementedException();
    }

    public void Write(ushort address, byte value)
    {
        throw new NotImplementedException();
    }

    public void GetInputState(Memory memory)
    {
        throw new NotImplementedException();
    }
}