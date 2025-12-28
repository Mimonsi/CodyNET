using System;
using static CodyPrototype.Mnemonic;
using static CodyPrototype.AddressingMode;

namespace CodyPrototype;

public class Cpu
{
    private byte _a;
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
    
    private void UpdateRegisterFlags()
    {
        Status.Zero = (A == 0);
        Status.Negative = (A & 0x80) != 0;
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
    
    public void RunUntilFinish()
    {
        while (Step())
        {
        }
    }

    public bool Step()
    {
        if (PC >= Memory.Length)
            return false;
        Instruction instruction = OpcodeLookup.FromOpcode(Memory[PC++]);
        int cycles = instruction.Cycles;
        int extraCyles = 0;

        switch (instruction.Mnemonic)
        {
            // ADC
            case ADC:
                (var value, var pageCross) = ReadValueOperand(instruction.AddressingMode);
                if (pageCross) extraCyles += 1;
                if (Status.DecimalMode)
                {
                    extraCyles += 1;
                    DoAdditionDecimal(value);
                }
                else
                {
                    DoAddition(value);
                }
                
                break;
            
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

            default:
                throw new NotSupportedException($"Unsupported instruction {instruction.Mnemonic}: Opcode {instruction.Opcode:X2} not implemented.");
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
        return ReadShort(PC+=2);
    }
    
    private byte ReadByte(ushort address)
    {
        return Memory[address];
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