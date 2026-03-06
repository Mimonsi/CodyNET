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
    
    /// <summary>
    /// Translates key input to text written, German Layout:
    /// Shift + 1 => !
    /// Shift + 2 => "
    /// ...
    /// </summary>
    /// <param name="keyName"></param>
    /// <param name="ctrl"></param>
    /// <param name="shift"></param>
    /// <param name="alt"></param>
    /// <returns></returns>
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
            case "D3":
                return shift ? "§" : "3";
            default:
                return keyName;
        }
    }

    private string TranslateLogicalKey(string keyName, bool ctrl, bool shift, bool alt, string locale)
    {
        return locale switch
        {
            "de-DE" => TranslateLogicalKeyDE(keyName, ctrl, shift, alt),
            _ => TranslateLogicalKeyUS(keyName, ctrl, shift, alt)
        };
    }
    
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

    private bool SetKeyState(string rawKeyName, bool ctrl, bool shift, bool alt, bool value)
    {
        if (UsePhysicalKeyboard)
        {
            if (physicalMap.TryGetValue(rawKeyName, out (CodyKeyCode code, CodyModifier modifier) mapping))
            {
                var modifierKey = modifierToKey(mapping.modifier);
                if (modifierKey.HasValue)
                {
                    Log.Verbose("Setting pressed to {Value} for modifier {ModifierKey} due to key {RawKeyName}", value, modifierKey.Value, rawKeyName);
                    KeyState.SetPressed(modifierKey.Value, value);
                }
                Log.Verbose("Setting pressed to {Value} for key {Code} due to key {RawKeyName}", value, mapping.code, rawKeyName);
                KeyState.SetPressed(mapping.code, value);
            }
        }
        else
        {
            // TODO: Fix logical
            var translatedKey = TranslateLogicalKey(rawKeyName, ctrl, shift, alt, "de-DE");
            if (logicalMap.TryGetValue(translatedKey, out (CodyKeyCode code, CodyModifier modifier) mapping))
            {
                var modifierKey = modifierToKey(mapping.modifier);
                if (modifierKey.HasValue)
                {
                    Log.Verbose("Setting pressed to {Value} for modifier {ModifierKey} due to key {RawKeyName}", value, modifierKey.Value, rawKeyName);
                    KeyState.SetPressed(modifierKey.Value, value);
                }
                Log.Verbose("Setting pressed to {Value} for key {Code} due to key {RawKeyName}", value, mapping.code, rawKeyName);
                KeyState.SetPressed(mapping.code, value);
            }

        }

        return false;
    }
    
    public bool KeyDown(string keyName, bool ctrl, bool shift, bool alt)
    {
        return SetKeyState(keyName, ctrl, shift, alt, true);
    }
    
    public bool KeyUp(string keyName, bool ctrl, bool shift, bool alt)
    {
        return SetKeyState(keyName, ctrl, shift, alt, false);
    }
}
