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
            new (0x72, Mnemonic.ADC, AddressingMode.ZeroPageIndirect, 2, 5),

            // AND (AND with Accumulator)
            new (0x29, Mnemonic.AND, AddressingMode.Immediate, 2, 2),
            new (0x25, Mnemonic.AND, AddressingMode.ZeroPage, 2, 3),
            new (0x35, Mnemonic.AND, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x2D, Mnemonic.AND, AddressingMode.Absolute, 3, 4),
            new (0x3D, Mnemonic.AND, AddressingMode.AbsoluteIndexedX, 3, 4),
            new (0x39, Mnemonic.AND, AddressingMode.AbsoluteIndexedY, 3, 4),
            new (0x32, Mnemonic.AND, AddressingMode.ZeroPageIndirect, 2, 5),
            new (0x21, Mnemonic.AND, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0x31, Mnemonic.AND, AddressingMode.ZeroPageIndirectIndexedY, 2, 5),

            // ASL (Arithmetic Shift Left)
            new (0x0A, Mnemonic.ASL, AddressingMode.Accumulator, 1, 2),
            new (0x06, Mnemonic.ASL, AddressingMode.ZeroPage, 2, 5),
            new (0x16, Mnemonic.ASL, AddressingMode.ZeroPageIndexedX, 2, 6),
            new (0x0E, Mnemonic.ASL, AddressingMode.Absolute, 3, 6),
            new (0x1E, Mnemonic.ASL, AddressingMode.AbsoluteIndexedX, 3, 7),

            // BBR (Branch On Bit Reset)
            new (0x0F, Mnemonic.BBR0, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4), // +1 if branch taken
            new (0x1F, Mnemonic.BBR1, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x2F, Mnemonic.BBR2, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x3F, Mnemonic.BBR3, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x4F, Mnemonic.BBR4, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x5F, Mnemonic.BBR5, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x6F, Mnemonic.BBR6, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0x7F, Mnemonic.BBR7, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),

            // BBS (Branch On Bit Set)
            new (0x8F, Mnemonic.BBS0, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4), // +1 if branch taken
            new (0x9F, Mnemonic.BBS1, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xAF, Mnemonic.BBS2, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xBF, Mnemonic.BBS3, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xCF, Mnemonic.BBS4, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xDF, Mnemonic.BBS5, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xEF, Mnemonic.BBS6, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new (0xFF, Mnemonic.BBS7, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),

            // BCC (Branch if Carry Clear)
            new (0x90, Mnemonic.BCC, AddressingMode.ProgramCounterRelative, 2, 2), // +1 if branch taken, +2 if to a new page

            // BCS (Branch if Carry Set)
            new (0xB0, Mnemonic.BCS, AddressingMode.ProgramCounterRelative, 2, 2), // +1 if branch taken, +2 if to a new page

            // BEQ (Branch if Equal)
            new (0xF0, Mnemonic.BEQ, AddressingMode.ProgramCounterRelative, 2, 2), // +1 if branch taken, +2 if to a new page

            // BIT (Bit Test)
            new (0x89, Mnemonic.BIT, AddressingMode.Immediate, 2, 2),
            new (0x24, Mnemonic.BIT, AddressingMode.ZeroPage, 2, 3),
            new (0x34, Mnemonic.BIT, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x2C, Mnemonic.BIT, AddressingMode.Absolute, 3, 4),
            new (0x3C, Mnemonic.BIT, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed

            // BMI (Branch if Minus)
            new (0x30, Mnemonic.BMI, AddressingMode.ProgramCounterRelative, 2, 2), // +1 if branch taken, +2 if to a new page

            // BNE (Branch if Not Equal)
            new (0xD0, Mnemonic.BNE, AddressingMode.ProgramCounterRelative, 2, 2), // +1 if branch taken, +2 if to a new page

            // BPL (Branch if Positive)
            new (0x10, Mnemonic.BPL, AddressingMode.ProgramCounterRelative, 2, 2), // +1 if branch taken, +2 if to a new page

            // BRA (Branch Always)
            new (0x80, Mnemonic.BRA, AddressingMode.ProgramCounterRelative, 2, 3), // +1 if branch taken

            // BRK (Break / Interrupt)
            new (0x00, Mnemonic.BRK, AddressingMode.None, 1, 7),

            // BVC (Branch if Overflow Clear)
            new (0x50, Mnemonic.BVC, AddressingMode.ProgramCounterRelative, 2, 2), // +1 if branch taken, +2 if to a new page

            // BVS (Branch if Overflow Set)
            new (0x70, Mnemonic.BVS, AddressingMode.ProgramCounterRelative, 2, 2), // +1 if branch taken, +2 if to a new page

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
            new (0x3A, Mnemonic.DEC, AddressingMode.Accumulator, 1, 2),
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
            new (0x52, Mnemonic.EOR, AddressingMode.ZeroPageIndirect, 2, 5),
            new (0x41, Mnemonic.EOR, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0x51, Mnemonic.EOR, AddressingMode.ZeroPageIndirectIndexedY, 2, 5),

            // INC (Increment by one)
            new (0x1A, Mnemonic.INC, AddressingMode.Accumulator, 1, 2),
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
            new (0xFC, Mnemonic.JSR, AddressingMode.AbsoluteIndexedIndirectX, 3, 8),

            // LDA (Load Accumulator)
            new (0xA9, Mnemonic.LDA, AddressingMode.Immediate, 2, 2),
            new (0xA5, Mnemonic.LDA, AddressingMode.ZeroPage, 2, 3),
            new (0xB5, Mnemonic.LDA, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0xAD, Mnemonic.LDA, AddressingMode.Absolute, 3, 4),
            new (0xBD, Mnemonic.LDA, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed
            new (0xB9, Mnemonic.LDA, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed
            new (0xB2, Mnemonic.LDA, AddressingMode.ZeroPageIndirect, 2, 5),
            new (0xA1, Mnemonic.LDA, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0xB1, Mnemonic.LDA, AddressingMode.ZeroPageIndirectIndexedY, 2, 5), // +1 if page crossed

            // LDX (Load X)
            new (0xA2, Mnemonic.LDX, AddressingMode.Immediate, 2, 2),
            new (0xA6, Mnemonic.LDX, AddressingMode.ZeroPage, 2, 3),
            new (0xB6, Mnemonic.LDX, AddressingMode.ZeroPageIndexedY, 2, 4),
            new (0xAE, Mnemonic.LDX, AddressingMode.Absolute, 3, 4),
            new (0xBE, Mnemonic.LDX, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed

            // LDY (Load Y)
            new (0xA0, Mnemonic.LDY, AddressingMode.Immediate, 2, 2),
            new (0xA4, Mnemonic.LDY, AddressingMode.ZeroPage, 2, 3),
            new (0xB4, Mnemonic.LDY, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0xAC, Mnemonic.LDY, AddressingMode.Absolute, 3, 4),
            new (0xBC, Mnemonic.LDY, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed

            // LSR (Logical Shift Right)
            new (0x4A, Mnemonic.LSR, AddressingMode.Accumulator, 1, 2),
            new (0x46, Mnemonic.LSR, AddressingMode.ZeroPage, 2, 5),
            new (0x56, Mnemonic.LSR, AddressingMode.ZeroPageIndexedX, 2, 6),
            new (0x4E, Mnemonic.LSR, AddressingMode.Absolute, 3, 6),
            new (0x5E, Mnemonic.LSR, AddressingMode.AbsoluteIndexedX, 3, 7),

            // NOP (No Operation)
            new (0xEA, Mnemonic.NOP, AddressingMode.None, 1, 2),

            // ORA (OR with Accumulator)
            new (0x09, Mnemonic.ORA, AddressingMode.Immediate, 2, 2),
            new (0x05, Mnemonic.ORA, AddressingMode.ZeroPage, 2, 3),
            new (0x15, Mnemonic.ORA, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x0D, Mnemonic.ORA, AddressingMode.Absolute, 3, 4),
            new (0x1D, Mnemonic.ORA, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed
            new (0x19, Mnemonic.ORA, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed
            new (0x12, Mnemonic.ORA, AddressingMode.ZeroPageIndirect, 2, 5),
            new (0x01, Mnemonic.ORA, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0x11, Mnemonic.ORA, AddressingMode.ZeroPageIndirectIndexedY, 2, 5), // +1 if page crossed

            // PHA/PHX/PHY (Push Accumulator/X/Y)
            new (0x48, Mnemonic.PHA, AddressingMode.None, 1, 3),
            new (0xDA, Mnemonic.PHX, AddressingMode.None, 1, 3),
            new (0x5A, Mnemonic.PHY, AddressingMode.None, 1, 3),

            // PHP (Push Processor Status)
            new (0x08, Mnemonic.PHP, AddressingMode.None, 1, 3),

            // PLA/PLX/PLY (Pull Accumulator/X/Y)
            new (0x68, Mnemonic.PLA, AddressingMode.None, 1, 4),
            new (0xFA, Mnemonic.PLX, AddressingMode.None, 1, 4),
            new (0x7A, Mnemonic.PLY, AddressingMode.None, 1, 4),

            // PLP (Pull Processor Status)
            new (0x28, Mnemonic.PLP, AddressingMode.None, 1, 4),

            // RMB (Reset Memory Bit)
            new (0x07, Mnemonic.RMB0, AddressingMode.ZeroPage, 2, 5),
            new (0x17, Mnemonic.RMB1, AddressingMode.ZeroPage, 2, 5),
            new (0x27, Mnemonic.RMB2, AddressingMode.ZeroPage, 2, 5),
            new (0x37, Mnemonic.RMB3, AddressingMode.ZeroPage, 2, 5),
            new (0x47, Mnemonic.RMB4, AddressingMode.ZeroPage, 2, 5),
            new (0x57, Mnemonic.RMB5, AddressingMode.ZeroPage, 2, 5),
            new (0x67, Mnemonic.RMB6, AddressingMode.ZeroPage, 2, 5),
            new (0x77, Mnemonic.RMB7, AddressingMode.ZeroPage, 2, 5),

            // ROL (Rotate Left)
            new (0x2A, Mnemonic.ROL, AddressingMode.Accumulator, 1, 2),
            new (0x26, Mnemonic.ROL, AddressingMode.ZeroPage, 2, 5),
            new (0x36, Mnemonic.ROL, AddressingMode.ZeroPageIndexedX, 2, 6),
            new (0x2E, Mnemonic.ROL, AddressingMode.Absolute, 3, 6),
            new (0x3E, Mnemonic.ROL, AddressingMode.AbsoluteIndexedX, 3, 7),

            // ROR (Rotate Right)
            new (0x6A, Mnemonic.ROR, AddressingMode.Accumulator, 1, 2),
            new (0x66, Mnemonic.ROR, AddressingMode.ZeroPage, 2, 5),
            new (0x76, Mnemonic.ROR, AddressingMode.ZeroPageIndexedX, 2, 6),
            new (0x6E, Mnemonic.ROR, AddressingMode.Absolute, 3, 6),
            new (0x7E, Mnemonic.ROR, AddressingMode.AbsoluteIndexedX, 3, 7),

            // RTI (Return from Interrupt)
            new (0x40, Mnemonic.RTI, AddressingMode.None, 1, 6),

            // RTS (Return from Subroutine)
            new (0x60, Mnemonic.RTS, AddressingMode.None, 1, 6),

            // SBC (Subtract with Borrow)
            new (0xE9, Mnemonic.SBC, AddressingMode.Immediate, 2, 2),
            new (0xE5, Mnemonic.SBC, AddressingMode.ZeroPage, 2, 3),
            new (0xF5, Mnemonic.SBC, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0xED, Mnemonic.SBC, AddressingMode.Absolute, 3, 4),
            new (0xFD, Mnemonic.SBC, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed
            new (0xF9, Mnemonic.SBC, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed
            new (0xF2, Mnemonic.SBC, AddressingMode.ZeroPageIndirect, 2, 5),
            new (0xE1, Mnemonic.SBC, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0xF1, Mnemonic.SBC, AddressingMode.ZeroPageIndirectIndexedY, 2, 5), // +1 if page crossed

            // SEC (Set Carry)
            new (0x38, Mnemonic.SEC, AddressingMode.None, 1, 2),

            // SED (Set Decimal Mode)
            new (0xF8, Mnemonic.SED, AddressingMode.None, 1, 2),

            // SEI (Set Interrupt Disable)
            new (0x78, Mnemonic.SEI, AddressingMode.None, 1, 2),

            // SMB (Set Memory Bit)
            new (0x87, Mnemonic.SMB0, AddressingMode.ZeroPage, 2, 5),
            new (0x97, Mnemonic.SMB1, AddressingMode.ZeroPage, 2, 5),
            new (0xA7, Mnemonic.SMB2, AddressingMode.ZeroPage, 2, 5),
            new (0xB7, Mnemonic.SMB3, AddressingMode.ZeroPage, 2, 5),
            new (0xC7, Mnemonic.SMB4, AddressingMode.ZeroPage, 2, 5),
            new (0xD7, Mnemonic.SMB5, AddressingMode.ZeroPage, 2, 5),
            new (0xE7, Mnemonic.SMB6, AddressingMode.ZeroPage, 2, 5),
            new (0xF7, Mnemonic.SMB7, AddressingMode.ZeroPage, 2, 5),

            // STA (Store Accumulator)
            new (0x85, Mnemonic.STA, AddressingMode.ZeroPage, 2, 3),
            new (0x95, Mnemonic.STA, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x8D, Mnemonic.STA, AddressingMode.Absolute, 3, 4),
            new (0x9D, Mnemonic.STA, AddressingMode.AbsoluteIndexedX, 3, 5),
            new (0x99, Mnemonic.STA, AddressingMode.AbsoluteIndexedY, 3, 5),
            new (0x92, Mnemonic.STA, AddressingMode.ZeroPageIndirect, 2, 5),
            new (0x81, Mnemonic.STA, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new (0x91, Mnemonic.STA, AddressingMode.ZeroPageIndirectIndexedY, 2, 6),

            // STP (Stop Processor)
            new (0xDB, Mnemonic.STP, AddressingMode.None, 1, 3),

            // STX (Store X)
            new (0x86, Mnemonic.STX, AddressingMode.ZeroPage, 2, 3),
            new (0x96, Mnemonic.STX, AddressingMode.ZeroPageIndexedY, 2, 4),
            new (0x8E, Mnemonic.STX, AddressingMode.Absolute, 3, 4),

            // STY (Store Y)
            new (0x84, Mnemonic.STY, AddressingMode.ZeroPage, 2, 3),
            new (0x94, Mnemonic.STY, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x8C, Mnemonic.STY, AddressingMode.Absolute, 3, 4),

            // STZ (Store Zero)
            new (0x64, Mnemonic.STZ, AddressingMode.ZeroPage, 2, 3),
            new (0x74, Mnemonic.STZ, AddressingMode.ZeroPageIndexedX, 2, 4),
            new (0x9C, Mnemonic.STZ, AddressingMode.Absolute, 3, 4),
            new (0x9E, Mnemonic.STZ, AddressingMode.AbsoluteIndexedX, 3, 5),

            // TAX/TAY/TXA/TYA (Transfer)
            new (0xAA, Mnemonic.TAX, AddressingMode.None, 1, 2),
            new (0xA8, Mnemonic.TAY, AddressingMode.None, 1, 2),
            new (0x8A, Mnemonic.TXA, AddressingMode.None, 1, 2),
            new (0x98, Mnemonic.TYA, AddressingMode.None, 1, 2),

            // TRB (Test and Reset Bits)
            new (0x14, Mnemonic.TRB, AddressingMode.ZeroPage, 2, 5),
            new (0x1C, Mnemonic.TRB, AddressingMode.Absolute, 3, 6),

            // TSB (Test and Set Bits)
            new (0x04, Mnemonic.TSB, AddressingMode.ZeroPage, 2, 5),
            new (0x0C, Mnemonic.TSB, AddressingMode.Absolute, 3, 6),

            // TSX/TXS (Transfer)
            new (0xBA, Mnemonic.TSX, AddressingMode.None, 1, 2),
            new (0x9A, Mnemonic.TXS, AddressingMode.None, 1, 2),

            // WAI (Wait for Interrupt)
            new (0xCB, Mnemonic.WAI, AddressingMode.None, 1, 3),
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
