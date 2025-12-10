namespace CodyPrototype;

public static class CpuHelpers
{
    public static void CheckState(this CpuState expected, CpuState actual)
    {
        if (actual.PC != expected.PC ||
            actual.A != expected.A ||
            actual.X != expected.X ||
            actual.Y != expected.Y ||
            actual.S != expected.S ||
            actual.P != expected.P)
        {
            throw new Exception("CPU state does not match expected state.");
        }

        foreach (var kvp in expected.Memory)
        {
            if (actual.Memory[kvp.Key] != kvp.Value)
            {
                throw new Exception($"Memory at address {kvp.Key:X4} does not match expected value.");
            }
        }
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
        
        for (ushort addr = 0; addr < cpu.Memory.Length; addr++)
        {
            state.Memory[addr] = cpu.Memory[addr];
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