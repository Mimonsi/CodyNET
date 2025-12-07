namespace CodyPrototype;

public class Cpu65C02
{
    public byte A, X, Y, SP; // 8 bit registers
    public ushort PC; // 16 bit program counter
    public byte[] Memory = new byte[65536];
    public OpcodeLookup OpcodeLookup = new OpcodeLookup();
    
    public void Reset(ushort startAddress)
    {
        PC = startAddress;
        A = X = Y = 0;
        SP = 0xFF;
    }
    
    public void LoadProgram(byte[] program, ushort startAddress)
    {
        Array.Copy(program, 0, Memory, startAddress, program.Length);
        Reset(startAddress);
    }

    public void Step()
    {
        byte opcode = Memory[PC++];

        switch (opcode)
        {
            case 0xEA: // NOP
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
                break;

            default:
                throw new NotSupportedException($"Opcode {opcode:X2} not implemented.");
        }
    }
}