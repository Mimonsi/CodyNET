namespace CodyNET.Core.Interfaces;

public interface IMemoryAccessTapDevice
{
    public void OnRead(ushort address);
    public void OnWrite(ushort address, byte value);
}