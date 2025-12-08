using System;
using static CodyPrototype.Mnemonic;
using static CodyPrototype.AddressingMode;

namespace CodyPrototype;

public class Cpu65C02
{
    public byte A, X, Y, S; // 8 bit registers
    public Status Status;
    public ushort PC; // 16 bit program counter
    public readonly byte[] Memory = new byte[65536];
    public readonly OpcodeLookup OpcodeLookup = new();
    
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

    public void Step()
    {
        Instruction instruction = OpcodeLookup.FromOpcode(Memory[PC++]);
        int cycles = instruction.Cycles;

        switch (instruction.Mnemonic)
        {
            // ADC
            case ADC:
                ReadValueOperand(instruction.AddressingMode);
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