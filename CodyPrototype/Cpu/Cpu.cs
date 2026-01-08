using System;
using System.Diagnostics;
using CodyPrototype.Utils;
using static CodyPrototype.Mnemonic;
using static CodyPrototype.AddressingMode;

namespace CodyPrototype.Cpu;

public enum StepResult
{
    Continue,
    Pause,
    Finished,
}

public class Cpu
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

    public byte S; // 8 bit registers
    // public byte P; Status used instead 
    public Status Status;
    public ushort PC; // 16 bit program counter
    public readonly byte[] Memory = new byte[65536];
    public readonly OpcodeLookup OpcodeLookup = new();
    
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

    
    public void Reset(ushort startAddress)
    {
        PC = startAddress;
        A = X = Y = 0;
        S = 0xFF;
    }
    
    public void LoadProgram(byte[] program, ushort startAddress)
    {
        Array.Copy(program, 0, Memory, startAddress, program.Length);
        Reset(startAddress);
    }
    
    public static Cpu FromState(CpuState state)
    {
        var cpu = new Cpu()
        {
            A = state.A,
            X = state.X,
            Y = state.Y,
            PC = state.PC,
            S = state.S,
            Status = CpuHelpers.StatusFromByte(state.P)
        };
        
        foreach (var kvp in state.Memory)
        {
            cpu.Memory[kvp.Key] = kvp.Value;
        }

        return cpu;
    }

    private bool SingleStepMode = false;
    public int RunUntilFinish()
    {
        int stepsPerformed = 0;
        StepResult lastResult = StepResult.Continue;
        while (lastResult != StepResult.Finished)
        {
            lastResult = Step();
            stepsPerformed++;
            if (lastResult == StepResult.Pause || SingleStepMode)
            {
                Debugger();
            }
        }
        return stepsPerformed;
    }

    private void Debugger()
    {
        // TODO: Actual debugger implementation
        //Log.Info("Debugger Paused Execution");
        Log.Info("Enter 'c' or 'continue' to resume, 'regs' to print registers, 'step' to step one instruction.");
        while (true)
        {
            string cmd = Console.ReadLine();
            if (cmd is "c" or "continue")
            {
                SingleStepMode = false;
                break;
            }
            if (cmd == "regs")
            {
                Log.Info($"PC={PC:X4} A={A:X2} X={X:X2} Y={Y:X2} S={S:X2} P={CpuHelpers.ByteFromStatus(Status):X2}");
            }
            else if (cmd == "step")
            {
                SingleStepMode = true;
                Log.Info("Single Step Mode Enabled");
                break;
            }
        }
        //Log.Info("Debugger Resumed Execution");
    }

    private Instruction instruction;
    private int cycles;
    private int extraCycles;
    public StepResult Step()
    {
        if (PC >= Memory.Length)
            return StepResult.Finished;
        instruction = OpcodeLookup.FromOpcode(Memory[PC++]);
        cycles = instruction.Cycles;
        extraCycles = 0;

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
            
            case BRK: return StepResult.Finished; break; // TODO: Implement BRK, currently just ends program
            
            case CLC:
                Status.Carry = false;
                break;
            case CLD:
                Status.DecimalMode = false;
                break;
            case CLI:
                Status.InterruptDisable = false;
                break;
            case CLV:
                Status.Overflow = false;
                break;
            case SEC:
                Status.Carry = true;
                break;
            case SED: 
                Status.DecimalMode = true;
                break;
            case SEI:
                Status.InterruptDisable = true;
                break;
            
            case LDA: DoLDA(); break;
            case LDX: DoLDX(); break;
            case LDY: DoLDY(); break;
            
            case NOP:
                break;
            
            case STA: DoSTA(); break;
                
                
            /*case 0xEA: // NOP
                break;

            case 0xA9: // LDA #imm
                A = Memory[PC++];
                break;

            case 0x8D: // STA abs
                byte lo = Memory[PC++];
                byte hi = Memory[PC++];
                ushort addr = (ushort)((hi << 8) | lo);
                Memory[addr] = A;
                break;

            case 0x4C: // JMP abs
                byte l = Memory[PC++];
                byte h = Memory[PC++];
                PC = (ushort)((h << 8) | l);
                break;*/

            // Artificial instructions
            case DBP: return StepResult.Pause;
            case DRS: DoDRS(); break;
            case DMP: DoDMP(); break;
            
            default:
                throw new NotSupportedException($"Unsupported instruction {instruction.Mnemonic}: Opcode {instruction.Opcode:X2} not implemented.");
        }

        return StepResult.Continue;
    }
    
    #region Instruction Implementations
    
    private bool DoDRS()
    {
        var (index, _) = ReadValueOperand(instruction.AddressingMode);
        Log.Info($"Register Dump #{index}\nPC={PC:X4} A={A:X2} X={X:X2} Y={Y:X2} S={S:X2} P={CpuHelpers.ByteFromStatus(Status):X2}");
        return true;
    }
    
    /// <summary>
    /// Dumps all non-zero memory contents to the log
    /// </summary>
    /// <returns></returns>
    private bool DoDMP()
    {
        string text = "Memory Dump:\n";
        foreach(var kvp in Memory.Select((value, index) => new { value, index }))
        {
            if (kvp.value != 0)
            {
                text += $"[{kvp.index:X4}] = {kvp.value:X2}\n";
            }
        }
        Log.Info(text);
        return true;
    }

    private bool DoADC()
    {
        (var value, var pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) extraCycles += 1;
        if (Status.DecimalMode)
        {
            extraCycles += 1;
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
        if (pageCross) extraCycles += 1;
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
            if (pageCross) extraCycles += 1;
            var value = ReadByte(address);
            var newValue = (byte)(value << 1);
            WriteByte(address, newValue);
            UpdateRegisterFlags(newValue);
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
            extraCycles += (((oldPc ^ target) & 0xFF00) != 0 ? 2 : 1);
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
            extraCycles += (((oldPc ^ target) & 0xFF00) != 0 ? 2 : 1);
        }

        return true;
    }
    
    private bool DoBranch(bool p0)
    {
        // TODO
        return true;
    }

    private bool DoLDA()
    {
        var (value, pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) extraCycles += 1;
        A = value;
        return true;
    }
    
    private bool DoLDX()
    {
        var (value, pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) extraCycles += 1;
        X = value;
        return true;
    }
    
    private bool DoLDY()
    {
        var (value, pageCross) = ReadValueOperand(instruction.AddressingMode);
        if (pageCross) extraCycles += 1;
        Y = value;
        return true;
    }

    private bool DoSTA()
    {
        var (addr, _) = ReadAddressOperand(instruction.AddressingMode);
        WriteByte(addr, A);
        return true;
    }
    
    #endregion

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
        return Memory[address];
    }
    
    private void WriteByte(ushort address, byte newValue)
    {
        Memory[address] = newValue;
    }
    
    private ushort ReadShort(ushort address)
    {
        byte low = ReadByte(address);
        byte high = ReadByte((ushort)(address + 1));
        return (ushort)((high << 8) | low);
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
    
}