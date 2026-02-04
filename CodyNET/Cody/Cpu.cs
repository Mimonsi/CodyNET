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
            UpdateNzFlags(value);
        }
    }
    
    private byte _x;
    public byte X
    {
        get => _x;
        private set
        {
            _x = value;
            UpdateNzFlags(value);
        }
    }
    
    private byte _y;
    public byte Y
    {
        get => _y;
        private set
        {
            _y = value;
            UpdateNzFlags(value);
        }
    }
    
    public const byte INITIAL_STACK_POINTER = 0xFD;
    public const ushort NMI_VECTOR = 0xFFFA;
    public const ushort RESET_VECTOR = 0xFFFC;
    public const ushort IRQ_VECTOR = 0xFFFE;

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
    /// Update Zero and Negative flags based on the given value
    /// </summary>
    /// <param name="value"></param>
    private void UpdateNzFlags(byte value)
    {
        Status.Zero = (value == 0);
        Status.Negative = (value & 0x80) != 0;
    }
    
    /// <summary>
    /// Resets the CPU state, setting the program counter to the specified start address and clearing registers.
    /// </summary>
    /// <param name="startAddress"></param>
    public void ResetToAddress(ushort startAddress)
    {
        Reset();
        PC = startAddress;
    }
    
    public void Reset()
    {
        A = X = Y = 0;
        S = INITIAL_STACK_POINTER;
        Status = new Status()
        {
            InterruptDisable = true
        };
        PC = ReadShort(RESET_VECTOR);
        
        // Additional reset logic for variables
        TotalCyclesExecuted = 0;
        
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
        ResetToAddress(startAddress);
    }

    private Instruction instruction;
    private int cycles;
    public StepResult Step()
    {
        instruction = OpcodeLookup.FromOpcode(Memory.Read(PC++));
            
        cycles = instruction.Cycles;
        
        switch (instruction.Mnemonic)
        {
            case ADC: DoADC(); break;
            case AND: DoAND(); break;
            case ASL: DoASL(); break;
            
            case BBR0: DoBBR(0); break;
            case BBR1: DoBBR(1); break;
            case BBR2: DoBBR(2); break;
            case BBR3: DoBBR(3); break;
            case BBR4: DoBBR(4); break;
            case BBR5: DoBBR(5); break;
            case BBR6: DoBBR(6); break;
            case BBR7: DoBBR(7); break;
            
            case BBS0: DoBBS(0); break;
            case BBS1: DoBBS(1); break;
            case BBS2: DoBBS(2); break;
            case BBS3: DoBBS(3); break;
            case BBS4: DoBBS(4); break;
            case BBS5: DoBBS(5); break;
            case BBS6: DoBBS(6); break;
            case BBS7: DoBBS(7); break;
            
            case BCC: DoBranch(!Status.Carry); break;
            case BCS: DoBranch(Status.Carry); break;
            case BEQ: DoBranch(Status.Zero); break;
            case BIT: DoBIT(); break;
            case BMI: DoBranch(Status.Negative); break;
            case BNE: DoBranch(!Status.Zero); break;
            case BPL: DoBranch(!Status.Negative); break;
            case BRA: DoBranch(true); break;
            case BRK: DoBRK(); break;
            case BVC: DoBranch(!Status.Overflow); break;
            case BVS: DoBranch(Status.Overflow); break;
            
            case CLC: Status.Carry = false; break;
            case CLD: Status.DecimalMode = false; break;
            case CLI: Status.InterruptDisable = false; break;
            case CLV: Status.Overflow = false; break;
            case CMP: DoCompare(A); break;
            case CPX: DoCompare(X); break;
            case CPY: DoCompare(Y); break;
            case DEC: DoDEC(); break;
            case DEX: X--; break;
            case DEY: Y--; break;
            case EOR: DoEOR(); break;
            case INC: DoINC(); break;
            case INX: X++; break;
            case INY: Y++; break;
            case JMP: DoJMP(); break;
            case JSR: DoJSR(); break;
            case LDA: DoLDA(); break;
            case LDX: DoLDX(); break;
            case LDY: DoLDY(); break;
            
            case NOP:
                break;
            
            case SEC: Status.Carry = true; break;
            case SED: Status.DecimalMode = true; break;
            case SEI: Status.InterruptDisable = true; break;
            case STA: DoSTA(); break;
            
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

    private bool DoASL()
    {
        if (instruction.AddressingMode == Accumulator)
        {
            var oldA = A;
            A = (byte)(A << 1);
            Status.Carry = (oldA & 0x80) != 0;
        }
        else
        {
            var (address, pageCross) = ReadAddressOperand(instruction.AddressingMode);
            if (pageCross) cycles += 1;
            var value = ReadByte(address);
            var newValue = (byte)(value << 1);
            Memory.Write(address, newValue);
            UpdateNzFlags(newValue);
            Status.Carry = (value & 0x80) != 0;
        }
        return true;
    }
    
    /// <summary>
    /// Branch if Bit Reset (bit = 0)
    /// </summary>
    /// <param name="bit"></param>
    /// <returns></returns>
    private bool DoBBR(byte bit)
    {
        var (value, _) = ReadValueOperand(ZeroPage);
        var (target, _) = ReadAddressOperand(ProgramCounterRelative);
        // If bit in value is 0 => branch
        if (((value >> bit) & 0x01) == 0)
        {
            ushort oldPc = PC;
            PC = target;

            // Extra cycles: +1 if branch taken, +2 if page boundary crossed
            cycles += (((oldPc ^ target) & 0xFF00) != 0 ? 2 : 1);
        }

        return true;
    }
    
    /// <summary>
    /// Branch if Bit Set (bit = 1)
    /// </summary>
    /// <param name="bit"></param>
    /// <returns></returns>
    private bool DoBBS(byte bit)
    {
        var (value, _) = ReadValueOperand(ZeroPage);
        var (target, _) = ReadAddressOperand(ProgramCounterRelative);
        // If bit in value is 1 => branch
        if (((value >> bit) & 0x01) != 0)
        {
            ushort oldPc = PC;
            PC = target;

            // Extra cycles: +1 if branch taken, +2 if page boundary crossed
            cycles += (((oldPc ^ target) & 0xFF00) != 0 ? 2 : 1);
        }

        return true;
    }
    
    private bool DoBranch(bool condition)
    {
        var (target, _) = ReadAddressOperand(ProgramCounterRelative);
        if (condition)
        {
            var oldPc = PC;
            PC = target;
            if (oldPc >> 8 != target >> 8)
                cycles += 2; // Page crossed
            else
                cycles += 1; // No page crossed
        }
        return true;
    }

    private bool DoBIT()
    {
        var (value, pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) cycles += 1;
        Status.Zero = (A & value) == 0;
        if (instruction.AddressingMode != Immediate)
        {
            Status.Negative = (value & 0x80) != 0;
            Status.Overflow = (value & 0x40) != 0;
        }
        return true;
    }
    
    private bool DoBRK()
    {
        PC++; // Skip unused byte
        PushPC();
        PushFlags(true);
        Status.InterruptDisable = true;
        Status.DecimalMode = false;
        PC = ReadShort(0xFFFE);
        return true;
    }

    private bool DoCompare(byte value1)
    {
        var (value2, pageCross) = ReadValueOperand(instruction.AddressingMode);
        UpdateNzFlags((byte) (value1 - value2));
        Status.Carry = value1 >= value2;
        if (pageCross) cycles += 1;
        return true;
    }
    
    private bool DoDEC()
    {
        if (instruction.AddressingMode == Accumulator)
        {
            A--;
            return true;
        }
        var (address, pageCross) = ReadAddressOperand(instruction.AddressingMode);
        if (pageCross && instruction.AddressingMode == AbsoluteIndexedX) cycles += 1;
        var value = ReadByte(address);
        var newValue = (byte) (value - 1);
        Memory.Write(address, newValue);
        UpdateNzFlags(newValue);
        return true;
    }

    private bool DoEOR()
    {
        var (value, pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) cycles += 1;
        A = (byte)(A ^ value);
        return true;
    }

    private bool DoINC()
    {
        if (instruction.AddressingMode == Accumulator)
        {
            A++;
            return true;
        }
        var (address, pageCross) = ReadAddressOperand(instruction.AddressingMode);
        if (pageCross && instruction.AddressingMode == AbsoluteIndexedX) cycles += 1;
        var value = ReadByte(address);
        var newValue = (byte)(value + 1);
        Memory.Write(address, newValue);
        UpdateNzFlags(newValue);
        return true;
    }

    private bool DoJMP()
    {
        var (targetAddress, pageCross) = ReadAddressOperand(instruction.AddressingMode);
        if (pageCross) cycles += 1;
        PC = targetAddress;
        return true;
    }

    private bool DoJSR()
    {
        var (targetAddress, pageCross) = ReadAddressOperand(instruction.AddressingMode);
        if (pageCross) cycles += 1;
        PushPC();
        PC = targetAddress;
        return true;
    }
    
    private bool DoLDA()
    {
        var (value, pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) cycles += 1;
        A = value;
        return true;
    }
    
    private bool DoLDX()
    {
        var (value, pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) cycles += 1;
        X = value;
        return true;
    }
    
    private bool DoLDY()
    {
        var (value, pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) cycles += 1;
        Y = value;
        return true;
    }

    private bool DoSTA()
    {
        var (addr, _) = ReadAddressOperand(instruction.AddressingMode);
        Memory.Write(addr, A);
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
                var address = ReadShortIncPc();
                return (ReadShort(address), false);
            }
            case AbsoluteIndexedIndirectX: // Full 16 bit address + X register offset pointing to another address
            {
                var baseAddress = ReadShortIncPc();
                ushort address = (ushort)(baseAddress + X);
                return (ReadShort(address), false);
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
                ushort address = ReadShortZeroPage(zpAddress);
                return (address, false);
            }
            case ZeroPageIndexedIndirectX: // 8 bit address + X register offset in first 256 bytes of memory pointing to another address
            {
                byte zpBaseAddress = ReadByteIncPc();
                byte zpAddress = (byte)(zpBaseAddress + X);
                ushort address = ReadShortZeroPage(zpAddress);
                return (address, false);
            }
            case ZeroPageIndirectIndexedY: // 8 bit address in first 256 bytes of memory pointing to another address + Y register offset
            {
                byte zpAddress = ReadByteIncPc();
                ushort baseAddress = ReadShortZeroPage(zpAddress);
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
    
    private ushort ReadShortZeroPage(byte address)
    {
        // Zero-page indirect pointers wrap at 0xFF -> 0x00 for the high byte.
        byte low = ReadByte(address);
        byte high = ReadByte((byte)(address + 1));
        return (ushort)((high << 8) | low);
    }

    private void PushByte(byte value)
    {
        Memory.Write((ushort) (0x0100 + S), value);
        S--;
    }

    private byte PopByte()
    {
        S++;
        return Memory.Read((ushort)(0x0100 + S));
    }

    private void PushPC()
    {
        PushByte((byte)((PC >> 8) & 0xFF)); // High byte
        PushByte((byte)(PC & 0xFF));        // Low byte
    }
    
    private void PopPC()
    {
        byte low = PopByte();
        byte high = PopByte();
        PC = (ushort)((high << 8) | low);
    }
    
    /// <summary>
    /// Pushes the status flags onto the stack.
    /// </summary>
    /// <param name="breakFlag">true for BRK and PHP, false for IRQ and NMI</param>
    private void PushFlags(bool breakFlag)
    {
        PushByte(Status.ToByteForPush(breakFlag));
    }

    private void PopFlags()
    {
        Status = new Status(PopByte());
    }
    
    #endregion
}