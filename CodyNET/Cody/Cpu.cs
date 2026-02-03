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

    private Instruction instruction;
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
            case AND: DoAND(); break;
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

    private bool DoAND()
    {
        var (value, pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) cycles += 1;
        A = (byte)(A & value);
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

    private void DoAdditionDecimal(byte operand)
    {
        byte accumulator = A;
        int carryIn = Status.Carry ? 1 : 0;

        // Low BCD Digits
        int lowDigitSum = (accumulator & 0x0F) + (operand & 0x0F) + carryIn;

        int decimalCarryFromLow = (lowDigitSum > 9) ? 1 : 0;
        if (decimalCarryFromLow != 0) // Normalize to valid BCD digit (0..9)
            lowDigitSum = (lowDigitSum - 10) & 0x0F;

        // High BCD digit (tens place)
        int highDigitSum = (accumulator >> 4) + (operand >> 4) + decimalCarryFromLow;

        // Overflow precursor (pre-adjust)
        byte overflowReference = (byte)((highDigitSum & 0x08) << 4);

        int decimalCarryOut = (highDigitSum > 9) ? 1 : 0;
        if (decimalCarryOut != 0) // Normalize to valid BCD digit (0..9)
            highDigitSum = (highDigitSum - 10) & 0x0F;

        // Compose final BCD result
        byte result =
            (byte)((highDigitSum << 4) | lowDigitSum);
        A = result;
        // Decimal carry out of the most significant digit
        Status.Carry = decimalCarryOut != 0;
        Status.Overflow =
            ((accumulator ^ overflowReference) &
             (operand     ^ overflowReference) &
             0x80) != 0;
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
            case Absolute: // Full 16 bit address to identify memory location (2 bytes)
            {
                return (ReadShortIncPc(), false);
            }
            case AbsoluteIndexedX: // Full 16 bit address + X register offset
            {
                ushort baseAddress = ReadShortIncPc();
                ushort address = (ushort)(baseAddress + X);
                bool pageCross = (baseAddress & 0xFF00) != (address & 0xFF00);
                return (address, pageCross);
            }
            case AbsoluteIndexedY: // Full 16 bit address + Y register offset
            {
                ushort baseAddress = ReadShortIncPc();
                ushort address = (ushort)(baseAddress + Y);
                bool pageCross = (baseAddress & 0xFF00) != (address & 0xFF00);
                return (address, pageCross);
            }
            case AbsoluteIndirect: // Full 16 bit address pointing to another address
            {
                return (ReadShortIncPc(), false);
            }
            case AbsoluteIndexedIndirectX: // Full 16 bit address + X register offset pointing to another address
            {
                var baseAddress = ReadShortIncPc();
                ushort address = (ushort)(baseAddress + X);
                return (address, false);
            }
            case ProgramCounterRelative: // Relative address from PC (for branches)
            {
                var offset = ReadByteIncPc();
                ushort address = (ushort)(PC + (sbyte)offset);
                return (address, false); // TODO: Check if page crossing is needed
            }
            case ZeroPage: // 8 bit address in first 256 bytes of memory
            {
                return (ReadByteIncPc(), false);
            }
            case ZeroPageIndexedX: // 8 bit address + X register offset in first 256 bytes of memory
            {
                byte baseAddress = ReadByteIncPc();
                byte address = (byte)(baseAddress + X);
                return (address, false);
            }
            case ZeroPageIndexedY: // 8 bit address + Y register offset in first 256 bytes of memory
            {
                byte baseAddress = ReadByteIncPc();
                byte address = (byte)(baseAddress + Y);
                return (address, false);
            }
            case ZeroPageIndirect: // 8 bit address in first 256 bytes of memory pointing to another address
            {
                byte zpAddress = ReadByteIncPc();
                ushort address = ReadShort(zpAddress);
                return (address, false);
            }
            case ZeroPageIndexedIndirectX: // 8 bit address + X register offset in first 256 bytes of memory pointing to another address
            {
                byte zpBaseAddress = ReadByteIncPc();
                byte zpAddress = (byte)(zpBaseAddress + X);
                ushort address = ReadShort(zpAddress);
                return (address, false);
            }
            case ZeroPageIndirectIndexedY: // 8 bit address in first 256 bytes of memory pointing to another address + Y register offset
            {
                byte zpAddress = ReadByteIncPc();
                ushort baseAddress = ReadShort(zpAddress);
                ushort address = (ushort)(baseAddress + Y);
                bool pageCross = (baseAddress & 0xFF00) != (address & 0xFF00);
                return (address, pageCross);
            }
            default:
                throw new NotSupportedException($"Unsupported addressing mode: {addressingMode}");
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