namespace CodyPrototype;

// Mnemonic enum
public enum Mnemonic
{
    ADC,
    AND,
    ASL,
    BBR0,
    BBR1,
    BBR2,
    BBR3,
    BBR4,
    BBR5,
    BBR6,
    BBR7,
    BBS0,
    BBS1,
    BBS2,
    BBS3,
    BBS4,
    BBS5,
    BBS6,
    BBS7,
    BCC,
    BCS,
    BEQ,
    BIT,
    BMI,
    BNE,
    BPL,
    BRA,
    BRK,
    BVC,
    BVS,
    CLC,
    CLD,
    CLI,
    CLV,
    CMP,
    CPX,
    CPY,
    DEC,
    DEX,
    DEY,
    EOR,
    INC,
    INX,
    INY,
    JMP,
    JSR,
    LDA,
    LDX,
    LDY,
    LSR,
    NOP,
    ORA,
    PHA,
    PHP,
    PHX,
    PHY,
    PLA,
    PLP,
    PLX,
    PLY,
    RMB0,
    RMB1,
    RMB2,
    RMB3,
    RMB4,
    RMB5,
    RMB6,
    RMB7,
    ROL,
    ROR,
    RTI,
    RTS,
    SBC,
    SEC,
    SED,
    SEI,
    SMB0,
    SMB1,
    SMB2,
    SMB3,
    SMB4,
    SMB5,
    SMB6,
    SMB7,
    STA,
    STP,
    STX,
    STY,
    STZ,
    TAX,
    TAY,
    TRB,
    TSB,
    TSX,
    TXA,
    TXS,
    TYA,
    WAI,
}

/// <summary>
/// Taken from https://github.com/iTitus/cody_emulator/blob/main/src/opcode.rs
/// </summary>
public enum AddressingMode 
{
    /// i (Implied), s (Stack)
    None,
    /// A
    Accumulator,
    /// \#
    Immediate,
    /// a
    Absolute,
    /// a,x
    AbsoluteIndexedX,
    /// a,y
    AbsoluteIndexedY,
    /// (a)
    AbsoluteIndirect,
    /// (a,x)
    AbsoluteIndexedIndirectX,
    /// r
    ProgramCounterRelative,
    /// zp
    ZeroPage,
    /// zp,x
    ZeroPageIndexedX,
    /// zp,y
    ZeroPageIndexedY,
    /// (zp)
    ZeroPageIndirect,
    /// (zp,x)
    ZeroPageIndexedIndirectX,
    /// (zp),y
    ZeroPageIndirectIndexedY,
}

public class OpcodeLookup
{
    public List<Instruction> Instructions;

    /// <summary>
    /// Initializes all instructions accoding to 65C02 instruction set: https://feertech.com/legion/reference65c02.html#BRK
    /// </summary>
    public OpcodeLookup()
    {
        //new(0x00, Mnemonic.BRK, AddressingMode.Implied, 1, 7),
        //new(0x10, Mnemonic.BPL, AddressingMode.Immediate, 2, 2)
        
        // Instructions sorted by Mnemonic alphabetically
        Instructions = new List<Instruction>()
        {
            // ADC (Add to Accumulator with Carry)
            new (0x69, Mnemonic.ADC, AddressingMode.Immediate, 2, 2),
            new (0x65, Mnemonic.ADC, AddressingMode.ZeroPage, 2, 3),
            new (0x75, Mnemonic.ADC, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x6D, Mnemonic.ADC, AddressingMode.Absolute, 3, 4),
            new (0x7D, Mnemonic.ADC, AddressingMode.AbsoluteIndexedX, 3, 4),
            new (0x79, Mnemonic.ADC, AddressingMode.AbsoluteIndexedY, 3, 4),
            new (0x61, Mnemonic.ADC, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0x71, Mnemonic.ADC, AddressingMode.ZeroPageIndirectIndexedY, 2, 5),
            
            // AND (AND with Accumulator)
            new (0x29, Mnemonic.AND, AddressingMode.Immediate, 2, 2),
            new (0x25, Mnemonic.AND, AddressingMode.Immediate, 2, 2),
            new (0x35, Mnemonic.AND, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x2D, Mnemonic.AND, AddressingMode.Absolute, 3, 4),
            new (0x3D, Mnemonic.AND, AddressingMode.AbsoluteIndexedX, 3, 4),
            new (0x39, Mnemonic.AND, AddressingMode.AbsoluteIndexedY, 3, 4),
            new (0x32, Mnemonic.AND, AddressingMode.ZeroPageIndirect, 2, 5),
            new (0x21, Mnemonic.AND, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0x31, Mnemonic.AND, AddressingMode.ZeroPageIndirectIndexedY, 2, 5),
            
            // ASL (Arithmetic Shift Left)
            new (0x0A, Mnemonic.ASL, AddressingMode.None, 1, 2),
            new (0x06, Mnemonic.ASL, AddressingMode.ZeroPage, 2, 5),
            new (0x16, Mnemonic.ASL, AddressingMode.ZeroPageIndexedX, 2, 6),
            new (0x0E, Mnemonic.ASL, AddressingMode.Absolute, 3, 6),
            new (0x1E, Mnemonic.ASL, AddressingMode.AbsoluteIndexedX, 3, 7),
            
            // BBR (Branch On Bit Reset)
            new (0x0f, Mnemonic.BBR0, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4), // +1 if branch taken
            new (0x1f, Mnemonic.BBR1, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x2f, Mnemonic.BBR2, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x3f, Mnemonic.BBR3, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x4f, Mnemonic.BBR4, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x5f, Mnemonic.BBR5, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x6f, Mnemonic.BBR6, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x7f, Mnemonic.BBR7, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            
            // BBS (Branch On Bit Set)
            new (0x8f, Mnemonic.BBS0, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4), // +1 if branch taken
            new (0x9f, Mnemonic.BBS1, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xaf, Mnemonic.BBS2, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xbf, Mnemonic.BBS3, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xcf, Mnemonic.BBS4, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xdf, Mnemonic.BBS5, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xef, Mnemonic.BBS6, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xff, Mnemonic.BBS7, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            
            // BCC (Branch if Carry Clear)
            new (0x90, Mnemonic.BCC, AddressingMode.Immediate, 2, 2), // +1 if branch taken, +2 if to a new page
            
            // BCS (Branch if Carry Set)
            new (0xb0, Mnemonic.BCS, AddressingMode.Immediate, 2, 2), // +1 if branch taken, +2 if to a new page
            
            // BEQ (Branch if Equal)
            new (0xf0, Mnemonic.BEQ, AddressingMode.Immediate, 2, 2), // +1 if branch taken, +2 if to a new page
            
            // BIT (Bit Test)
            new (0x89, Mnemonic.BIT, AddressingMode.Immediate, 2, 2),
            new (0x24, Mnemonic.BIT, AddressingMode.ZeroPage, 2, 3),
            new (0x34, Mnemonic.BIT, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x2C, Mnemonic.BIT, AddressingMode.Absolute, 3, 4),
            new (0x3C, Mnemonic.BIT, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed
            
            // BMI (Branch if Minus)
            new (0x30, Mnemonic.BMI, AddressingMode.Immediate, 2, 2), // +1 if branch taken, +2 if to a new page
            
            // BNE (Branch if Not Equal)
            new (0xd0, Mnemonic.BNE, AddressingMode.Immediate, 2, 2), // +1 if branch taken, +2 if to a new page
            
            // BPL (Branch if Positive)
            new (0x10, Mnemonic.BPL, AddressingMode.Immediate, 2, 2), // +1 if branch taken, +2 if to a new page
            
            // BRA (Branch Always)
            new (0x80, Mnemonic.BRA, AddressingMode.Immediate, 2, 3), // +1 if branch taken
            
            // BRK (Break / Interrupt)
            new (0x00, Mnemonic.BRK, AddressingMode.None, 1, 7),
            
            // BVC (Branch if Overflow Clear)
            new (0x50, Mnemonic.BVC, AddressingMode.Immediate, 2, 2), // +1 if branch taken, +2 if to a new page
            
            // BVS (Branch if Overflow Set)
            new (0x70, Mnemonic.BVS, AddressingMode.Immediate, 2, 2), // +1 if branch taken, +2 if to a new page
            
            // CLC (Clear Carry)
            new (0x18, Mnemonic.CLC, AddressingMode.None, 1, 2),
            
            // CLD (Clear Decimal Mode)
            new (0xD8, Mnemonic.CLD, AddressingMode.None, 1, 2),
            
            // CLI (Clear Interrupt Disable)
            new (0x58, Mnemonic.CLI, AddressingMode.None, 1, 2),
            
            // CLV (Clear Overflow)
            new (0xB8, Mnemonic.CLV, AddressingMode.None, 1, 2),

            // CMP (Compare with Accumulator)
            new (0xC9, Mnemonic.CMP, AddressingMode.Immediate, 2, 2),
            new (0xC5, Mnemonic.CMP, AddressingMode.ZeroPage, 2, 3),
            new (0xD5, Mnemonic.CMP, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0xCD, Mnemonic.CMP, AddressingMode.Absolute, 3, 4),
            new (0xDD, Mnemonic.CMP, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed
            new (0xD9, Mnemonic.CMP, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed
            new (0xD2, Mnemonic.CMP, AddressingMode.ZeroPageIndirect, 2, 5),
            new (0xC1, Mnemonic.CMP, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0xD1, Mnemonic.CMP, AddressingMode.ZeroPageIndirectIndexedY, 2, 5), // +1 if page crossed
            
            // CPX (Compare with X)
            new (0xE0, Mnemonic.CPX, AddressingMode.Immediate, 2, 2),
            new (0xE4, Mnemonic.CPX, AddressingMode.ZeroPage, 2, 3),
            new (0xEC, Mnemonic.CPX, AddressingMode.Absolute, 3, 4),
            
            // CPY (Compare with Y)
            new (0xC0, Mnemonic.CPY, AddressingMode.Immediate, 2, 2),
            new (0xC4, Mnemonic.CPY, AddressingMode.ZeroPage, 2, 3),
            new (0xCC, Mnemonic.CPY, AddressingMode.Absolute, 3, 4),
            
            // DEC (Decrement by one)
            new (0x3A, Mnemonic.DEC, AddressingMode.None, 1, 2),
            new (0xC6, Mnemonic.DEC, AddressingMode.ZeroPage, 2, 5),
            new (0xD6, Mnemonic.DEC, AddressingMode.ZeroPageIndexedX, 2, 6),
            new (0xCE, Mnemonic.DEC, AddressingMode.Absolute, 3, 6),
            new (0xDE, Mnemonic.DEC, AddressingMode.AbsoluteIndexedX, 3, 7),
            
            // DEX (Decrement X by one)
            new (0xCA, Mnemonic.DEX, AddressingMode.None, 1, 2),
            
            // DEY (Decrement Y by one)
            new (0x88, Mnemonic.DEY, AddressingMode.None, 1, 2),

            // EOR (Exclusive OR with Accumulator)
            new (0x49, Mnemonic.EOR, AddressingMode.Immediate, 2, 2),
            new (0x45, Mnemonic.EOR, AddressingMode.ZeroPage, 2, 3),
            new (0x55, Mnemonic.EOR, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x4D, Mnemonic.EOR, AddressingMode.Absolute, 3, 4),
            new (0x5D, Mnemonic.EOR, AddressingMode.AbsoluteIndexedX, 3, 4),
            new (0x59, Mnemonic.EOR, AddressingMode.AbsoluteIndexedY, 3, 4),
            new (0x41, Mnemonic.EOR, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0x51, Mnemonic.EOR, AddressingMode.ZeroPageIndirectIndexedY, 2, 5),
            
            // INC (Increment by one)
            new (0x1A, Mnemonic.INC, AddressingMode.None, 1, 2),
            new (0xE6, Mnemonic.INC, AddressingMode.ZeroPage, 2, 5),
            new (0xF6, Mnemonic.INC, AddressingMode.ZeroPageIndexedX, 2, 6),
            new (0xEE, Mnemonic.INC, AddressingMode.Absolute, 3, 6),
            new (0xFE, Mnemonic.INC, AddressingMode.AbsoluteIndexedX, 3, 7),
            
            // INX (Increment X by one)
            new (0xE8, Mnemonic.INX, AddressingMode.None, 1, 2),
            
            // INY (Increment Y by one)
            new (0xC8, Mnemonic.INY, AddressingMode.None, 1, 2),
            
            // JMP (Jump)
            new (0x4C, Mnemonic.JMP, AddressingMode.Absolute, 3, 3),
            new (0x6C, Mnemonic.JMP, AddressingMode.AbsoluteIndirect, 3, 6),
            new (0x7C, Mnemonic.JMP, AddressingMode.AbsoluteIndexedIndirectX, 3, 6),
            
            // JSR (Jump to Subroutine)
            new (0x20, Mnemonic.JSR, AddressingMode.Absolute, 3, 6),
            new (0xFC, Mnemonic.JSR, AddressingMode.AbsoluteIndexedIndirectX, 3, 6),
            new (0x7C, Mnemonic.JSR, AddressingMode.AbsoluteIndexedIndirectX, 3, 6),
        };
    }
    
    public Instruction FromOpcode(byte opcode)
    {
        return Instructions.First(i => i.Opcode == opcode);
    }
    
    public List<Instruction> FromMnemonic(Mnemonic mnemonic)
    {
        return Instructions.Where(i => i.Mnemonic == mnemonic).ToList();
    }
}