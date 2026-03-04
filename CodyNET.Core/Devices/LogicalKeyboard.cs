using System.Collections.Concurrent;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public enum CodyKeyCode : byte
{
    KeyQ = 0,
    KeyE = 1,
    KeyT = 2,
    KeyU = 3,
    KeyO = 4,
    KeyA = 5,
    KeyD = 6,
    KeyG = 7,
    KeyJ = 8,
    KeyL = 9,
    Cody = 10,
    KeyX = 11,
    KeyV = 12,
    KeyN = 13,
    Meta = 14,
    KeyZ = 15,
    KeyC = 16,
    KeyB = 17,
    KeyM = 18,
    Enter = 19,
    KeyS = 20,
    KeyF = 21,
    KeyH = 22,
    KeyK = 23,
    Space = 24,
    KeyW = 25,
    KeyR = 26,
    KeyY = 27,
    KeyI = 28,
    KeyP = 29,
    Joystick1Up = 30,
    Joystick1Down = 31,
    Joystick1Left = 32,
    Joystick1Right = 33,
    Joystick1Fire = 34,
    Joystick2Up = 35,
    Joystick2Down = 36,
    Joystick2Left = 37,
    Joystick2Right = 38,
    Joystick2Fire = 39,
}

public enum CodyModifier
{
    None,
    Cody,
    Meta
}

public class LogicalKeyboard : IInputDevice
{
    private Dictionary<string, (CodyKeyCode, CodyModifier)> keyMap =
        new()
        {
            { "Q", (CodyKeyCode.KeyQ, CodyModifier.None) },
            { "E", (CodyKeyCode.KeyE, CodyModifier.None) },
            { "T", (CodyKeyCode.KeyT, CodyModifier.None) },
            { "U", (CodyKeyCode.KeyU, CodyModifier.None) },
            { "O", (CodyKeyCode.KeyO, CodyModifier.None) },
            { "A", (CodyKeyCode.KeyA, CodyModifier.None) },
            { "D", (CodyKeyCode.KeyD, CodyModifier.None) },
            { "G", (CodyKeyCode.KeyG, CodyModifier.None) },
            { "J", (CodyKeyCode.KeyJ, CodyModifier.None) },
            { "L", (CodyKeyCode.KeyL, CodyModifier.None) },
            { "LeftCtrl", (CodyKeyCode.Cody, CodyModifier.Cody) }, // CTRL => Cody
            { "X", (CodyKeyCode.KeyX, CodyModifier.None) },
            { "V", (CodyKeyCode.KeyV, CodyModifier.None) },
            { "N", (CodyKeyCode.KeyN, CodyModifier.None) },
            { "LeftAlt", (CodyKeyCode.Meta, CodyModifier.Meta) }, // ALT => Meta
            { "S", (CodyKeyCode.KeyS, CodyModifier.None) },
            { "F", (CodyKeyCode.KeyF, CodyModifier.None) },
            { "H", (CodyKeyCode.KeyH, CodyModifier.None) },
            { "K", (CodyKeyCode.KeyK, CodyModifier.None) },
            { "Space", (CodyKeyCode.Space, CodyModifier.None) },
            { "W", (CodyKeyCode.KeyW, CodyModifier.None) },
            { "R", (CodyKeyCode.KeyR, CodyModifier.None) },
            { "Y", (CodyKeyCode.KeyY, CodyModifier.None) },
            { "I", (CodyKeyCode.KeyI, CodyModifier.None) },
            { "P", (CodyKeyCode.KeyP, CodyModifier.None) },
            { "Up", (CodyKeyCode.Joystick1Up, CodyModifier.None) },
            // DEAD END: ! would not work...
            
            // Joystick mappings can be added here as needed    
        };
    public const ushort KEYBOARD_BASE = 0xD100;
    public const ushort REG_STATUS = KEYBOARD_BASE;
    public const ushort REG_CODE = KEYBOARD_BASE + 1;
    public const ushort REG_CONTROL = KEYBOARD_BASE + 2;

    private readonly ConcurrentQueue<byte> keyQueue = new();
    private byte lastCode;

    public ushort StartAddress => KEYBOARD_BASE;
    public ushort EndAddress => REG_CONTROL;
    public bool SupportsRead => true;
    public bool SupportsWrite => true;

    public byte Read(ushort address)
    {
        return 0;
    }

    public void Write(ushort address, byte value)
    {

    }

    public void GetInputState(Memory memory)
    {
        _ = memory;
    }

    public bool KeyPressed(string keyName, bool ctrl, bool shift, bool alt)
    {
        return true;
    }
}