using CodyNET.Utils;
using static CodyNET.Cody.Mnemonic;
using static CodyNET.Cody.AddressingMode;

namespace CodyNET.Cody;

public enum StepResult
{
    Success,
    UnknownOpcode,
    PcOverflow,
    EmptyBytecode,
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
            return StepResult.PcOverflow;
        instruction = OpcodeLookup.FromOpcode(Memory.Read(PC++));
        if (instruction.Opcode == 0)
            return StepResult.EmptyBytecode;
            
        cycles = instruction.Cycles;
        
        switch (instruction.Mnemonic)
        {
            case ADC: DoADC(); break;
            default:
                return StepResult.UnknownOpcode;
        }
        TotalCyclesExecuted += cycles;
        
        return StepResult.Success;
    }
    
    public void RunUntilFinish()
    {
        StepResult lastResult = StepResult.Success;
        while (lastResult != StepResult.PcOverflow && lastResult != StepResult.EmptyBytecode)
        {
            lastResult = Step();
        }
        Log.Info("Program finished execution.");
    }
    
    #region CPU Instructions
    private bool DoADC()
    {
        (var value, var pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) cycles += 1;
        if (Status.DecimalMode)
        {
            cycles += 1;
            DoAdditionDecimal(value);
        }
        else
        {
            DoAddition(value);
        }

        return true;
    }
    
    // Add Accumulator and Carry
    private void DoAddition(byte value)
    {
        int carryIn = Status.Carry ? 1 : 0;
        int sum = A + value + carryIn;

        Status.Carry = sum > 0xFF;
        byte result = (byte)(sum & 0xFF);

        // Set Overflow flag
        bool overflow = (~(A ^ value) & (A ^ result) & 0x80) != 0;
        Status.Overflow = overflow;

        A = result;
    }

    private void DoAdditionDecimal(byte value)
    {
        int carryIn = Status.Carry ? 1 : 0;
        int lowNibbleSum = (A & 0x0F) + (value & 0x0F) + carryIn;
        int adjustLow = (lowNibbleSum > 9) ? 6 : 0;

        int highNibbleSum = (A >> 4) + (value >> 4) + ((lowNibbleSum + adjustLow) > 0x0F ? 1 : 0);
        int adjustHigh = (highNibbleSum > 9) ? 6 : 0;

        int total = lowNibbleSum + adjustLow + ((highNibbleSum + adjustHigh) << 4);

        Status.Carry = total > 0xFF;
        byte result = (byte)(total & 0xFF);

        // Set Overflow flag
        bool overflow = (~(A ^ value) & (A ^ result) & 0x80) != 0;
        Status.Overflow = overflow;

        A = result;
    }
    
    #endregion
    
    #region Operand Reading and Memory
    
    private (byte value, bool pageCross) ReadValueOperand(AddressingMode addressingMode)
    {
        switch (addressingMode)
        {
            case Accumulator:
                return (A, false);
            case Immediate:
                return (ReadByteIncPc(), false);
            default:
                (ushort address, bool pageCross) = ReadAddressOperand(addressingMode);
                return (ReadByte(address), pageCross);
        }
    }
    
    private (ushort address, bool pageCross) ReadAddressOperand(AddressingMode addressingMode)
    {
        switch (addressingMode)
        {
            case Absolute:
            {
                return (ReadShortIncPc(), false);
            }
            default:
                throw new NotSupportedException($"Unsupported addressing mode: {addressingMode}");
            // TODO: Add more addressing modes
        }
    }

    private byte ReadByteIncPc()
    {
        return ReadByte(PC++);
    }
    
    private ushort ReadShortIncPc()
    {
        var addr = PC;
        PC += 2;
        return ReadShort(addr);
    }

    private byte ReadByte(ushort address)
    {
        return Memory.Read(address);
    }
    
    private ushort ReadShort(ushort address)
    {
        byte low = ReadByte(address);
        byte high = ReadByte((ushort)(address + 1));
        return (ushort)((high << 8) | low);
    }
    #endregion
}