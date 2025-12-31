using System;

namespace CodyPrototype.Cpu;

public static class CpuHelpers
{
    public static (bool, string) CheckState(this CpuState expected, CpuState actual)
    {
        List<string> errors = new List<string>();
        if (expected.A != actual.A)
            errors.Add($"A is {actual.A:X2}, expected {expected.A:X2}");
        if (expected.X != actual.X)
            errors.Add($"X is {actual.X:X2}, expected {expected.X:X2}");
        if (expected.Y != actual.Y)
            errors.Add($"Y is {actual.Y:X2}, expected {expected.Y:X2}");
        if (expected.PC != actual.PC)
            errors.Add($"PC is {actual.PC:X4}, expected {expected.PC:X4}");
        if (expected.S != actual.S)
            errors.Add($"S is {actual.S:X2}, expected {expected.S:X2}");
        if (expected.P != actual.P)
            errors.Add($"P is {actual.P:X2}, expected {expected.P:X2}");

        foreach (var kvp in expected.Memory)
        {
            if (actual.Memory[kvp.Key] != kvp.Value)
            {
                errors.Add($"Memory at address {kvp.Key:X4} is {actual.Memory[kvp.Key]:X2}, expected {kvp.Value:X2}");
            }
        }
        if (errors.Count > 0)
            return (false, string.Join("\n", errors));
        return (true, "States match");
    }

    public static CpuState GetState(this Cpu cpu)
    {
        var state = new CpuState
        {
            PC = cpu.PC,
            A = cpu.A,
            X = cpu.X,
            Y = cpu.Y,
            S = cpu.S,
            P = ByteFromStatus(cpu.Status)
        };
        for(int addr = 0; addr < cpu.Memory.Length; addr++)
        {
            state.Memory[(ushort) addr] = cpu.Memory[addr];
        }

        return state;
    }
    
    public static byte ByteFromStatus(Status status)
    {
        byte result = 0;
        if (status.Carry) result |= 0x01;
        if (status.Zero) result |= 0x02;
        if (status.InterruptDisable) result |= 0x04;
        if (status.DecimalMode) result |= 0x08;
        if (status.BreakCommand) result |= 0x10;
        if (status.Overflow) result |= 0x40;
        if (status.Negative) result |= 0x80;
        return result;
    }

    public static Status StatusFromByte(byte stateP)
    {
        return new Status
        {
            Carry = (stateP & 0x01) != 0,
            Zero = (stateP & 0x02) != 0,
            InterruptDisable = (stateP & 0x04) != 0,
            DecimalMode = (stateP & 0x08) != 0,
            BreakCommand = (stateP & 0x10) != 0,
            Overflow = (stateP & 0x40) != 0,
            Negative = (stateP & 0x80) != 0
        };
    }
}