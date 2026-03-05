using System.Collections.Concurrent;
using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

namespace CodyNET.Core.Devices;

public enum CodyModifier
{
    None,
    Cody,
    Meta
}

public class Keyboard
{
    private Dictionary<string, (CodyKeyCode, CodyModifier)> logicalMap =
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
            { "Z", (CodyKeyCode.KeyZ, CodyModifier.None) },
            { "C", (CodyKeyCode.KeyC, CodyModifier.None) },
            { "B", (CodyKeyCode.KeyB, CodyModifier.None) },
            { "M", (CodyKeyCode.KeyM, CodyModifier.None) },
            { "Enter", (CodyKeyCode.Arrow, CodyModifier.None) },
            { "Return", (CodyKeyCode.Arrow, CodyModifier.None) },
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
    
    private Dictionary<string, (CodyKeyCode, CodyModifier)> physicalMap =
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
            { "Z", (CodyKeyCode.KeyZ, CodyModifier.None) },
            { "C", (CodyKeyCode.KeyC, CodyModifier.None) },
            { "B", (CodyKeyCode.KeyB, CodyModifier.None) },
            { "M", (CodyKeyCode.KeyM, CodyModifier.None) },
            { "Enter", (CodyKeyCode.Arrow, CodyModifier.None) },
            { "Return", (CodyKeyCode.Arrow, CodyModifier.None) },
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
            // Joysticks:
            // Joystick 1: Arrow keys + Right Shift
            // Joystick 2: Numpad 8/5/4/6 + Numpad 0
            { "Up", (CodyKeyCode.Joystick1Up, CodyModifier.None) },
            { "Down", (CodyKeyCode.Joystick1Down, CodyModifier.None) },
            { "Left", (CodyKeyCode.Joystick1Left, CodyModifier.None) },
            { "Right", (CodyKeyCode.Joystick1Right, CodyModifier.None) },
            { "RightShift", (CodyKeyCode.Joystick1Fire, CodyModifier.None) },
            { "NumPad8", (CodyKeyCode.Joystick2Up, CodyModifier.None) },
            { "NumPad5", (CodyKeyCode.Joystick2Down, CodyModifier.None) },
            { "NumPad4", (CodyKeyCode.Joystick2Left, CodyModifier.None) },
            { "NumPad6", (CodyKeyCode.Joystick2Right, CodyModifier.None) },
            { "NumPad0", (CodyKeyCode.Joystick2Fire, CodyModifier.None) },
        };
    
    public static string TranslateLogicalKeyUS(string keyName, bool ctrl, bool shift, bool alt)
    {
        switch (keyName)
        {
            case "D1":
                return shift ? "!" : "1";
            case "D2":
                return shift ? "@" : "2";
            case "D3":
                return shift ? "#" : "3";
            case "D4":
                return shift ? "$" : "4";
            case "D5":
                return shift ? "%" : "5";
            case "D6":
                return shift ? "^" : "6";
            case "D7":
                return shift ? "&" : "7";
            case "D8":
                return shift ? "*" : "8";
            case "D9":
                return shift ? "(" : "9";
            case "D0":
                return shift ? ")" : "0";
            case "OemCloseBrackets":
                return shift ? "+" : "=";
            case "Oem4":
                return "-";
            case "Oem3":
                return shift ? ":" : ";";
            case "OemQuotes":
                return shift ? "\"" : "'";
            case "OemSemicolon":
                return "[";
            case "OemPlus":
                return "]";
            case "OemBackslash":
                return "\\";
            case "OemComma":
                return shift ? "<" : ",";
            case "OemPeriod":
                return shift ? ">" : ".";
            case "OemMinus":
                return shift ? "?" : "/";
            default:
                return keyName;
        }
    }
    
    // German layout
    public static string TranslateLogicalKeyDE(string keyName, bool ctrl, bool shift, bool alt)
    {
        switch (keyName)
        {
            case "D1":
                return shift ? "!" : "1";
            case "D2":
                return shift ? "\"" : "2";
            case "D4":
                return shift ? "$" : "4";
            case "D5":
                return shift ? "%" : "5";
            case "D6":
                return shift ? "&" : "6";
            case "D7":
                return shift ? "/" : "7";
            case "D8":
                if (ctrl && alt)
                    return "[";
                return shift ? "(" : "8";
            case "D9":
                if (ctrl && alt)
                    return "]";
                return shift ? ")" : "9";
            case "D0":
                return shift ? "=" : "0";
            case "Q": // @
                if (ctrl && alt)
                    return "@";
                return "Q";
            case "OemQuestion":
                return shift ? "'" : "#";
            case "OemPipe":
                return shift ? "^" : "Q";
            case "OemPlus":
                return shift ? "*" : "+";
            case "OemMinus":
                return "-";
            case "OemComma":
                return shift ? ";" : ",";
            case "OemPeriod":
                return shift ? ":" : ".";
            case "Oem4":
                if (ctrl && alt)
                    return "\\";
                if (shift)
                    return "?";
                return keyName;
            case "OemBackslash":
                return shift ? ">" : "<";
            default:
                return keyName;
        }
    }
    
    private KeyState KeyState;

    public Keyboard(KeyState keyState)
    {
        KeyState = keyState;
    }
    
    public bool KeyDown(string keyName, bool ctrl, bool shift, bool alt)
    {
        if (physicalMap.TryGetValue(keyName, out (CodyKeyCode code, CodyModifier modifier) mapping))
        {
            KeyState.SetPressed(mapping.code, true);
            return true;
        }

        return false;
    }
    
    public bool KeyUp(string keyName, bool ctrl, bool shift, bool alt)
    {
        if (physicalMap.TryGetValue(keyName, out (CodyKeyCode code, CodyModifier modifier) mapping))
        {
            KeyState.SetPressed(mapping.code, false);
            return true;
        }

        return false;
    }
}
