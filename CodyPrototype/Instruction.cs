namespace CodyPrototype;

public class Instruction(byte opcode, Mnemonic mnemonic, AddressingMode addressingMode, int bytes, int cycles)
{
    public byte Opcode = opcode;
    public Mnemonic Mnemonic = mnemonic;
    public AddressingMode AddressingMode = addressingMode;
    public AddressingMode? AddressingMode2 = null;
    public int Bytes = bytes;
    public int Cycles = cycles;
    
    public Instruction(byte opcode, Mnemonic mnemonic, AddressingMode addressingMode, AddressingMode addressingMode2, int bytes, int cycles)
        : this(opcode, mnemonic, addressingMode, bytes, cycles)
    {
        AddressingMode2 = addressingMode2;
    }
}