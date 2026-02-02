using CodyNET.Utils;

namespace CodyNET.Cody;

public enum StepResult
{
    Success,
    UnknownOpcode,
    Finished,
}

public class Cpu()
{
    private byte _a;
    /// <summary>
    /// Safe access to registers that updates flags on set
    /// </summary>
    public byte A
    {
        get => _a;
        private set
        {
            _a = value;
            UpdateRegisterFlags();
        }
    }
    
    private byte _x;
    public byte X
    {
        get => _x;
        private set
        {
            _x = value;
            UpdateRegisterFlags();
        }
    }
    
    private byte _y;
    public byte Y
    {
        get => _y;
        private set
        {
            _y = value;
            UpdateRegisterFlags();
        }
    }

    public byte S; // Stack Pointer
    public Status Status;
    public ushort PC; // 16 bit program counter
    public readonly Memory Memory = new(); // 64KB memory
    public readonly OpcodeLookup OpcodeLookup = new();
    //public long CyclesPerSecond = 1_000_000; // 1 MHz, typical for 65C02
    public long CyclesPerSecond = 10;
    public long TotalCyclesExecuted = 0;


    public Cpu(CpuState initialState) : this()
    {
        SetState(initialState);
    }

    #region CpuState
    public void SetState(CpuState state)
    {
        _a = state.A;
        _x = state.X;
        _y = state.Y;
        S = state.S;
        Status = new Status(state.P);
        PC = state.PC;
        Memory.SetFromList(state.Ram);
    }

    public CpuState GetState()
    {
        return new CpuState()
        {
            A = A,
            X = X,
            Y = Y,
            S = S,
            P = Status.ToByte(),
            PC = PC,
            Ram = Memory.GetAsList()
        };
    }
    #endregion
    
    /// <summary>
    /// Update Zero and Negative flags based on the value of the Accumulator
    /// </summary>
    private void UpdateRegisterFlags()
    {
        Status.Zero = (A == 0);
        Status.Negative = (A & 0x80) != 0;
    }
    
    /// <summary>
    /// Update Zero and Negative flags based on the given value
    /// </summary>
    /// <param name="value"></param>
    private void UpdateRegisterFlags(byte value)
    {
        Status.Zero = (value == 0);
        Status.Negative = (value & 0x80) != 0;
    }
    
    /// <summary>
    /// Resets the CPU state, setting the program counter to the specified start address and clearing registers.
    /// </summary>
    /// <param name="startAddress"></param>
    public void Reset(ushort startAddress)
    {
        PC = startAddress;
        A = X = Y = 0;
        S = 0xFF;
    }
    
    /// <summary>
    /// Loads a program into RAM at the specified start address and resets the CPU's program counter to that address.
    /// </summary>
    /// <param name="program"></param>
    /// <param name="startAddress"></param>
    /// <exception cref="ArgumentException"></exception>
    public void LoadRam(byte[] program, ushort startAddress)
    {
        if (startAddress + program.Length > Memory.Size)
            throw new ArgumentException($"Program does not fit in memory at the given start address. ({startAddress} + {program.Length} > {Memory.Size})");
        Memory.CopyFrom(program, startAddress);
        Reset(startAddress);
    }

    private Instruction? instruction;
    private int cycles;
    public StepResult Step()
    {
        if (PC >= Memory.Size - 1)
            return StepResult.Finished;
        instruction = OpcodeLookup.FromOpcode(Memory.Read(PC++));
        cycles = instruction.Cycles;
        
        switch (instruction.Mnemonic)
        {
            default:
                return StepResult.UnknownOpcode;
        }
    }
    
    public void RunUntilFinish()
    {
        StepResult lastResult = StepResult.Success;
        while (lastResult != StepResult.Finished)
        {
            lastResult = Step();
        }
        Log.Info("Program finished execution.");
    }
}