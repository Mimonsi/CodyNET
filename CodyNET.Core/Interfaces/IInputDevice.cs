using CodyNET.Core.Cody;

namespace CodyNET.Core.Interfaces;

// Derive classes like LogicalKeyboard, PhysicalKeyboard, Joystick1, Joystick2
public interface IInputDevice : IMemoryMappedDevice
{
    public void GetInputState(Memory memory);
}