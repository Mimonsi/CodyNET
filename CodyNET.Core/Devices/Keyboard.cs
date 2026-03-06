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
            { "Back", (CodyKeyCode.Arrow, CodyModifier.Meta) }, // Backspace => Meta + Arrow
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

            { "1", (CodyKeyCode.KeyQ, CodyModifier.Cody) }, // 1 => Q + Cody
            { "!", (CodyKeyCode.KeyQ, CodyModifier.Meta) }, // ! => Q + Meta
            { "2", (CodyKeyCode.KeyW, CodyModifier.Cody) }, // 2 => W + Cody
            { "\"", (CodyKeyCode.KeyW, CodyModifier.Meta) }, // " => W + Meta
            { "3", (CodyKeyCode.KeyE, CodyModifier.Cody) }, // 3 => E + Cody
            { "#", (CodyKeyCode.KeyE, CodyModifier.Meta) }, // # => E + Meta
            { "4", (CodyKeyCode.KeyR, CodyModifier.Cody) }, // 4 => R + Cody
            { "$", (CodyKeyCode.KeyR, CodyModifier.Meta) }, // $ => R + Meta
            { "5", (CodyKeyCode.KeyT, CodyModifier.Cody) }, // 5 => T + Cody
            { "%", (CodyKeyCode.KeyT, CodyModifier.Meta) }, // % => T + Meta
            { "6", (CodyKeyCode.KeyY, CodyModifier.Cody) }, // 6 => Y + Cody
            { "^", (CodyKeyCode.KeyY, CodyModifier.Meta) }, // ^ => Y + Meta
            { "7", (CodyKeyCode.KeyU, CodyModifier.Cody) }, // 7 => U + Cody
            { "&", (CodyKeyCode.KeyU, CodyModifier.Meta) }, // & => U + Meta
            { "8", (CodyKeyCode.KeyI, CodyModifier.Cody) }, // 8 => I + Cody
            { "*", (CodyKeyCode.KeyI, CodyModifier.Meta) }, // * => I + Meta
            { "9", (CodyKeyCode.KeyO, CodyModifier.Cody) }, // 9 => O + Cody
            { "(", (CodyKeyCode.KeyO, CodyModifier.Meta) }, // ( => O + Meta
            { "0", (CodyKeyCode.KeyP, CodyModifier.Cody) }, // 0 => P + Cody
            { ")", (CodyKeyCode.KeyP, CodyModifier.Meta) }, // ) => P + Meta

            { "@", (CodyKeyCode.KeyA, CodyModifier.Meta) }, // @ => A + Meta
            { "=", (CodyKeyCode.KeyS, CodyModifier.Meta) }, // = => S + Meta
            { "-", (CodyKeyCode.KeyD, CodyModifier.Meta) }, // - => D + Meta
            { "+", (CodyKeyCode.KeyF, CodyModifier.Meta) }, // + => F + Meta
            { ":", (CodyKeyCode.KeyG, CodyModifier.Meta) }, // : => G + Meta
            { ";", (CodyKeyCode.KeyH, CodyModifier.Meta) }, // ; => H + Meta
            { "'", (CodyKeyCode.KeyJ, CodyModifier.Meta) }, // ' => J + Meta
            { "[", (CodyKeyCode.KeyK, CodyModifier.Meta) }, // [ => K + Meta
            { "]", (CodyKeyCode.KeyL, CodyModifier.Meta) }, // ] => L + Meta
            { "\\", (CodyKeyCode.KeyZ, CodyModifier.Meta) }, // \ => Z + Meta
            { "<", (CodyKeyCode.KeyX, CodyModifier.Meta) }, // < => X + Meta
            { ">", (CodyKeyCode.KeyC, CodyModifier.Meta) }, // > => C + Meta
            { ",", (CodyKeyCode.KeyV, CodyModifier.Meta) }, // , => V + Meta
            { ".", (CodyKeyCode.KeyB, CodyModifier.Meta) }, // . => B + Meta
            { "?", (CodyKeyCode.KeyN, CodyModifier.Meta) }, // ? => N + Meta
            { "/", (CodyKeyCode.KeyM, CodyModifier.Meta) }, // / => M + Meta
            { "Up", (CodyKeyCode.Joystick1Up, CodyModifier.None) }, 
            
            // TODO: Joystick mappings can be added here as needed    
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
    
    private KeyState KeyState;
    public bool UsePhysicalKeyboard = false;

    public Keyboard(KeyState keyState)
    {
        KeyState = keyState;
    }

    private CodyKeyCode? modifierToKey(CodyModifier? modifier)
    {
        return modifier switch
        {
            CodyModifier.Cody => CodyKeyCode.Cody,
            CodyModifier.Meta => CodyKeyCode.Meta,
            _ => null
        };
    }

    private bool SetKeyState(string rawKeyName, string? keySymbolName, bool value)
    {
        if (UsePhysicalKeyboard)
        {
            if (physicalMap.TryGetValue(rawKeyName, out (CodyKeyCode code, CodyModifier modifier) mapping))
            {
                var modifierKey = modifierToKey(mapping.modifier);
                if (modifierKey.HasValue)
                {
                    KeyState.SetPressed(modifierKey.Value, value);
                }
                KeyState.SetPressed(mapping.code, value);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(keySymbolName))
            {
                var logicalKeyName =
                    keySymbolName
                        .ToUpper(); // Use key symbol for logical mapping, as it represents the character that will be input
                if (logicalMap.TryGetValue(logicalKeyName, out (CodyKeyCode code, CodyModifier modifier) mapping))
                {
                    var modifierKey = modifierToKey(mapping.modifier);
                    if (modifierKey.HasValue)
                    {
                        KeyState.SetPressed(modifierKey.Value, value);
                    }
                    KeyState.SetPressed(mapping.code, value);
                    return true;
                }
            }
            if (logicalMap.TryGetValue(rawKeyName, out (CodyKeyCode code, CodyModifier modifier) rawMapping)) // Special keys that don't have a symbol (e.g. Enter, Backspace) can be handled here if needed
            {
                var modifierKey = modifierToKey(rawMapping.modifier);
                if (modifierKey.HasValue)
                {
                    KeyState.SetPressed(modifierKey.Value, value);
                }
                KeyState.SetPressed(rawMapping.code, value);
                return true;
            }
            Log.Warn("No mapping found for key {RawKeyName} with symbol {KeySymbolName} in logical map", rawKeyName, keySymbolName);
        }
        return false;
    }
    
    public bool KeyDown(string keyName, string? keySymbolName)
    {
        return SetKeyState(keyName, keySymbolName, true);
    }
    
    public bool KeyUp(string keyName, string? keySymbolName)
    {
        return SetKeyState(keyName, keySymbolName, false);
    }
}
