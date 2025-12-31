using System;
using System.Collections.Generic;
using System.Linq;
using static CodyPrototype.Mnemonic;
// ReSharper disable InconsistentNaming

namespace CodyPrototype;

// Mnemonic enum (Alphabetical, additional debug instructions at the bottom)
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
    DBP,
    DRS,
    DMP,
}

/// <summary>
/// Taken from https://github.com/iTitus/cody_emulator/blob/main/src/opcode.rs
/// </summary>
public enum AddressingMode
{
    /// i (Implied), s (Stack) = No operand
    None,
    /// A = Operand is the A-register
    Accumulator,
    /// \# = Operand is literal value, e.g. LDA #$10
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
    /// Initializes all instructions according to 65C02 instruction set: https://feertech.com/legion/reference65c02.html
    /// </summary>
    public OpcodeLookup()
    {
        // Instructions sorted by Mnemonic alphabetically
        Instructions =
        [
            // ADC (Add with Carry) ☑
            new(0x69, ADC, AddressingMode.Immediate, 2, 2),
            new(0x65, ADC, AddressingMode.ZeroPage, 2, 3),
            new(0x75, ADC, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0x6D, ADC, AddressingMode.Absolute, 3, 4),
            new(0x7D, ADC, AddressingMode.AbsoluteIndexedX, 3, 4),
            new(0x79, ADC, AddressingMode.AbsoluteIndexedY, 3, 4),
            new(0x61, ADC, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new(0x71, ADC, AddressingMode.ZeroPageIndirectIndexedY, 2, 5),
            new(0x72, ADC, AddressingMode.ZeroPageIndirect, 2, 5),

            // AND (AND with Accumulator) ☑
            new(0x29, AND, AddressingMode.Immediate, 2, 2),
            new(0x25, AND, AddressingMode.ZeroPage, 2, 3),
            new(0x35, AND, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0x2D, AND, AddressingMode.Absolute, 3, 4),
            new(0x3D, AND, AddressingMode.AbsoluteIndexedX, 3, 4),
            new(0x39, AND, AddressingMode.AbsoluteIndexedY, 3, 4),
            new(0x32, AND, AddressingMode.ZeroPageIndirect, 2, 5),
            new(0x21, AND, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new(0x31, AND, AddressingMode.ZeroPageIndirectIndexedY, 2, 5),

            // ASL (Arithmetic Shift Left) ☑
            new(0x0A, ASL, AddressingMode.Accumulator, 1, 2),
            new(0x06, ASL, AddressingMode.ZeroPage, 2, 5),
            new(0x16, ASL, AddressingMode.ZeroPageIndexedX, 2, 6),
            new(0x0E, ASL, AddressingMode.Absolute, 3, 6),
            new(0x1E, ASL, AddressingMode.AbsoluteIndexedX, 3, 7),

            // BBR (Branch On Bit Reset) ☑
            new(0x0F, BBR0, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3,
                4), // +1 if branch taken
            new(0x1F, BBR1, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0x2F, BBR2, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0x3F, BBR3, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0x4F, BBR4, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0x5F, BBR5, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0x6F, BBR6, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0x7F, BBR7, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),

            // BBS (Branch On Bit Set) ☑
            new(0x8F, BBS0, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3,
                4), // +1 if branch taken
            new(0x9F, BBS1, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0xAF, BBS2, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0xBF, BBS3, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0xCF, BBS4, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0xDF, BBS5, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0xEF, BBS6, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),
            new(0xFF, BBS7, AddressingMode.ZeroPage, AddressingMode.ProgramCounterRelative, 3, 4),

            // BCC (Branch if Carry Clear)
            new(0x90, BCC, AddressingMode.ProgramCounterRelative, 2,
                2), // +1 if branch taken, +2 if to a new page

            // BCS (Branch if Carry Set)
            new(0xB0, BCS, AddressingMode.ProgramCounterRelative, 2,
                2), // +1 if branch taken, +2 if to a new page

            // BEQ (Branch if Equal)
            new(0xF0, BEQ, AddressingMode.ProgramCounterRelative, 2,
                2), // +1 if branch taken, +2 if to a new page

            // BIT (Bit Test)
            new(0x89, BIT, AddressingMode.Immediate, 2, 2),
            new(0x24, BIT, AddressingMode.ZeroPage, 2, 3),
            new(0x34, BIT, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0x2C, BIT, AddressingMode.Absolute, 3, 4),
            new(0x3C, BIT, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed

            // BMI (Branch if Minus)
            new(0x30, BMI, AddressingMode.ProgramCounterRelative, 2,
                2), // +1 if branch taken, +2 if to a new page

            // BNE (Branch if Not Equal)
            new(0xD0, BNE, AddressingMode.ProgramCounterRelative, 2,
                2), // +1 if branch taken, +2 if to a new page

            // BPL (Branch if Positive)
            new(0x10, BPL, AddressingMode.ProgramCounterRelative, 2,
                2), // +1 if branch taken, +2 if to a new page

            // BRA (Branch Always)
            new(0x80, BRA, AddressingMode.ProgramCounterRelative, 2, 3), // +1 if branch taken

            // BRK (Break / Interrupt)
            new(0x00, BRK, AddressingMode.None, 1, 7),

            // BVC (Branch if Overflow Clear)
            new(0x50, BVC, AddressingMode.ProgramCounterRelative, 2,
                2), // +1 if branch taken, +2 if to a new page

            // BVS (Branch if Overflow Set)
            new(0x70, BVS, AddressingMode.ProgramCounterRelative, 2,
                2), // +1 if branch taken, +2 if to a new page

            // CLC (Clear Carry) ☑
            new(0x18, CLC, AddressingMode.None, 1, 2),

            // CLD (Clear Decimal Mode) ☑
            new(0xD8, CLD, AddressingMode.None, 1, 2),

            // CLI (Clear Interrupt Disable) ☑
            new(0x58, CLI, AddressingMode.None, 1, 2),

            // CLV (Clear Overflow) ☑
            new(0xB8, CLV, AddressingMode.None, 1, 2),

            // CMP (Compare with Accumulator)
            new(0xC9, CMP, AddressingMode.Immediate, 2, 2),
            new(0xC5, CMP, AddressingMode.ZeroPage, 2, 3),
            new(0xD5, CMP, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0xCD, CMP, AddressingMode.Absolute, 3, 4),
            new(0xDD, CMP, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed
            new(0xD9, CMP, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed
            new(0xD2, CMP, AddressingMode.ZeroPageIndirect, 2, 5),
            new(0xC1, CMP, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new(0xD1, CMP, AddressingMode.ZeroPageIndirectIndexedY, 2, 5), // +1 if page crossed

            // CPX (Compare with X)
            new(0xE0, CPX, AddressingMode.Immediate, 2, 2),
            new(0xE4, CPX, AddressingMode.ZeroPage, 2, 3),
            new(0xEC, CPX, AddressingMode.Absolute, 3, 4),

            // CPY (Compare with Y)
            new(0xC0, CPY, AddressingMode.Immediate, 2, 2),
            new(0xC4, CPY, AddressingMode.ZeroPage, 2, 3),
            new(0xCC, CPY, AddressingMode.Absolute, 3, 4),

            // DEC (Decrement by one)
            new(0x3A, DEC, AddressingMode.Accumulator, 1, 2),
            new(0xC6, DEC, AddressingMode.ZeroPage, 2, 5),
            new(0xD6, DEC, AddressingMode.ZeroPageIndexedX, 2, 6),
            new(0xCE, DEC, AddressingMode.Absolute, 3, 6),
            new(0xDE, DEC, AddressingMode.AbsoluteIndexedX, 3, 7),

            // DEX (Decrement X by one)
            new(0xCA, DEX, AddressingMode.None, 1, 2),

            // DEY (Decrement Y by one)
            new(0x88, DEY, AddressingMode.None, 1, 2),

            // EOR (Exclusive OR with Accumulator)
            new(0x49, EOR, AddressingMode.Immediate, 2, 2),
            new(0x45, EOR, AddressingMode.ZeroPage, 2, 3),
            new(0x55, EOR, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0x4D, EOR, AddressingMode.Absolute, 3, 4),
            new(0x5D, EOR, AddressingMode.AbsoluteIndexedX, 3, 4),
            new(0x59, EOR, AddressingMode.AbsoluteIndexedY, 3, 4),
            new(0x52, EOR, AddressingMode.ZeroPageIndirect, 2, 5),
            new(0x41, EOR, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new(0x51, EOR, AddressingMode.ZeroPageIndirectIndexedY, 2, 5),

            // INC (Increment by one)
            new(0x1A, INC, AddressingMode.Accumulator, 1, 2),
            new(0xE6, INC, AddressingMode.ZeroPage, 2, 5),
            new(0xF6, INC, AddressingMode.ZeroPageIndexedX, 2, 6),
            new(0xEE, INC, AddressingMode.Absolute, 3, 6),
            new(0xFE, INC, AddressingMode.AbsoluteIndexedX, 3, 7),

            // INX (Increment X by one)
            new(0xE8, INX, AddressingMode.None, 1, 2),

            // INY (Increment Y by one)
            new(0xC8, INY, AddressingMode.None, 1, 2),

            // JMP (Jump)
            new(0x4C, JMP, AddressingMode.Absolute, 3, 3),
            new(0x6C, JMP, AddressingMode.AbsoluteIndirect, 3, 6),
            new(0x7C, JMP, AddressingMode.AbsoluteIndexedIndirectX, 3, 6),

            // JSR (Jump to Subroutine)
            new(0x20, JSR, AddressingMode.Absolute, 3, 6),
            new(0xFC, JSR, AddressingMode.AbsoluteIndexedIndirectX, 3, 8),

            // LDA (Load Accumulator) ☑
            new(0xA9, LDA, AddressingMode.Immediate, 2, 2),
            new(0xA5, LDA, AddressingMode.ZeroPage, 2, 3),
            new(0xB5, LDA, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0xAD, LDA, AddressingMode.Absolute, 3, 4),
            new(0xBD, LDA, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed
            new(0xB9, LDA, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed
            new(0xB2, LDA, AddressingMode.ZeroPageIndirect, 2, 5),
            new(0xA1, LDA, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new(0xB1, LDA, AddressingMode.ZeroPageIndirectIndexedY, 2, 5), // +1 if page crossed

            // LDX (Load X) ☑
            new(0xA2, LDX, AddressingMode.Immediate, 2, 2),
            new(0xA6, LDX, AddressingMode.ZeroPage, 2, 3),
            new(0xB6, LDX, AddressingMode.ZeroPageIndexedY, 2, 4),
            new(0xAE, LDX, AddressingMode.Absolute, 3, 4),
            new(0xBE, LDX, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed

            // LDY (Load Y) ☑
            new(0xA0, LDY, AddressingMode.Immediate, 2, 2),
            new(0xA4, LDY, AddressingMode.ZeroPage, 2, 3),
            new(0xB4, LDY, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0xAC, LDY, AddressingMode.Absolute, 3, 4),
            new(0xBC, LDY, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed

            // LSR (Logical Shift Right)
            new(0x4A, LSR, AddressingMode.Accumulator, 1, 2),
            new(0x46, LSR, AddressingMode.ZeroPage, 2, 5),
            new(0x56, LSR, AddressingMode.ZeroPageIndexedX, 2, 6),
            new(0x4E, LSR, AddressingMode.Absolute, 3, 6),
            new(0x5E, LSR, AddressingMode.AbsoluteIndexedX, 3, 7),

            // NOP (No Operation) ☑
            new(0xEA, NOP, AddressingMode.None, 1, 2),

            // ORA (OR with Accumulator)
            new(0x09, ORA, AddressingMode.Immediate, 2, 2),
            new(0x05, ORA, AddressingMode.ZeroPage, 2, 3),
            new(0x15, ORA, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0x0D, ORA, AddressingMode.Absolute, 3, 4),
            new(0x1D, ORA, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed
            new(0x19, ORA, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed
            new(0x12, ORA, AddressingMode.ZeroPageIndirect, 2, 5),
            new(0x01, ORA, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new(0x11, ORA, AddressingMode.ZeroPageIndirectIndexedY, 2, 5), // +1 if page crossed

            // PHA/PHX/PHY (Push Accumulator/X/Y)
            new(0x48, PHA, AddressingMode.None, 1, 3),
            new(0xDA, PHX, AddressingMode.None, 1, 3),
            new(0x5A, PHY, AddressingMode.None, 1, 3),

            // PHP (Push Processor Status)
            new(0x08, PHP, AddressingMode.None, 1, 3),

            // PLA/PLX/PLY (Pull Accumulator/X/Y)
            new(0x68, PLA, AddressingMode.None, 1, 4),
            new(0xFA, PLX, AddressingMode.None, 1, 4),
            new(0x7A, PLY, AddressingMode.None, 1, 4),

            // PLP (Pull Processor Status)
            new(0x28, PLP, AddressingMode.None, 1, 4),

            // RMB (Reset Memory Bit)
            new(0x07, RMB0, AddressingMode.ZeroPage, 2, 5),
            new(0x17, RMB1, AddressingMode.ZeroPage, 2, 5),
            new(0x27, RMB2, AddressingMode.ZeroPage, 2, 5),
            new(0x37, RMB3, AddressingMode.ZeroPage, 2, 5),
            new(0x47, RMB4, AddressingMode.ZeroPage, 2, 5),
            new(0x57, RMB5, AddressingMode.ZeroPage, 2, 5),
            new(0x67, RMB6, AddressingMode.ZeroPage, 2, 5),
            new(0x77, RMB7, AddressingMode.ZeroPage, 2, 5),

            // ROL (Rotate Left)
            new(0x2A, ROL, AddressingMode.Accumulator, 1, 2),
            new(0x26, ROL, AddressingMode.ZeroPage, 2, 5),
            new(0x36, ROL, AddressingMode.ZeroPageIndexedX, 2, 6),
            new(0x2E, ROL, AddressingMode.Absolute, 3, 6),
            new(0x3E, ROL, AddressingMode.AbsoluteIndexedX, 3, 7),

            // ROR (Rotate Right)
            new(0x6A, ROR, AddressingMode.Accumulator, 1, 2),
            new(0x66, ROR, AddressingMode.ZeroPage, 2, 5),
            new(0x76, ROR, AddressingMode.ZeroPageIndexedX, 2, 6),
            new(0x6E, ROR, AddressingMode.Absolute, 3, 6),
            new(0x7E, ROR, AddressingMode.AbsoluteIndexedX, 3, 7),

            // RTI (Return from Interrupt)
            new(0x40, RTI, AddressingMode.None, 1, 6),

            // RTS (Return from Subroutine)
            new(0x60, RTS, AddressingMode.None, 1, 6),

            // SBC (Subtract with Borrow)
            new(0xE9, SBC, AddressingMode.Immediate, 2, 2),
            new(0xE5, SBC, AddressingMode.ZeroPage, 2, 3),
            new(0xF5, SBC, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0xED, SBC, AddressingMode.Absolute, 3, 4),
            new(0xFD, SBC, AddressingMode.AbsoluteIndexedX, 3, 4), // +1 if page crossed
            new(0xF9, SBC, AddressingMode.AbsoluteIndexedY, 3, 4), // +1 if page crossed
            new(0xF2, SBC, AddressingMode.ZeroPageIndirect, 2, 5),
            new(0xE1, SBC, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new(0xF1, SBC, AddressingMode.ZeroPageIndirectIndexedY, 2, 5), // +1 if page crossed

            // SEC (Set Carry) ☑
            new(0x38, SEC, AddressingMode.None, 1, 2),

            // SED (Set Decimal Mode) ☑
            new(0xF8, SED, AddressingMode.None, 1, 2),

            // SEI (Set Interrupt Disable) ☑
            new(0x78, SEI, AddressingMode.None, 1, 2),

            // SMB (Set Memory Bit)
            new(0x87, SMB0, AddressingMode.ZeroPage, 2, 5),
            new(0x97, SMB1, AddressingMode.ZeroPage, 2, 5),
            new(0xA7, SMB2, AddressingMode.ZeroPage, 2, 5),
            new(0xB7, SMB3, AddressingMode.ZeroPage, 2, 5),
            new(0xC7, SMB4, AddressingMode.ZeroPage, 2, 5),
            new(0xD7, SMB5, AddressingMode.ZeroPage, 2, 5),
            new(0xE7, SMB6, AddressingMode.ZeroPage, 2, 5),
            new(0xF7, SMB7, AddressingMode.ZeroPage, 2, 5),

            // STA (Store Accumulator)
            new(0x85, STA, AddressingMode.ZeroPage, 2, 3),
            new(0x95, STA, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0x8D, STA, AddressingMode.Absolute, 3, 4),
            new(0x9D, STA, AddressingMode.AbsoluteIndexedX, 3, 5),
            new(0x99, STA, AddressingMode.AbsoluteIndexedY, 3, 5),
            new(0x92, STA, AddressingMode.ZeroPageIndirect, 2, 5),
            new(0x81, STA, AddressingMode.ZeroPageIndexedIndirectX, 2, 6),
            new(0x91, STA, AddressingMode.ZeroPageIndirectIndexedY, 2, 6),

            // STP (Stop Processor)
            new(0xDB, STP, AddressingMode.None, 1, 3),

            // STX (Store X)
            new(0x86, STX, AddressingMode.ZeroPage, 2, 3),
            new(0x96, STX, AddressingMode.ZeroPageIndexedY, 2, 4),
            new(0x8E, STX, AddressingMode.Absolute, 3, 4),

            // STY (Store Y)
            new(0x84, STY, AddressingMode.ZeroPage, 2, 3),
            new(0x94, STY, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0x8C, STY, AddressingMode.Absolute, 3, 4),

            // STZ (Store Zero)
            new(0x64, STZ, AddressingMode.ZeroPage, 2, 3),
            new(0x74, STZ, AddressingMode.ZeroPageIndexedX, 2, 4),
            new(0x9C, STZ, AddressingMode.Absolute, 3, 4),
            new(0x9E, STZ, AddressingMode.AbsoluteIndexedX, 3, 5),

            // TAX/TAY/TXA/TYA (Transfer)
            new(0xAA, TAX, AddressingMode.None, 1, 2),
            new(0xA8, TAY, AddressingMode.None, 1, 2),
            new(0x8A, TXA, AddressingMode.None, 1, 2),
            new(0x98, TYA, AddressingMode.None, 1, 2),

            // TRB (Test and Reset Bits)
            new(0x14, TRB, AddressingMode.ZeroPage, 2, 5),
            new(0x1C, TRB, AddressingMode.Absolute, 3, 6),

            // TSB (Test and Set Bits)
            new(0x04, TSB, AddressingMode.ZeroPage, 2, 5),
            new(0x0C, TSB, AddressingMode.Absolute, 3, 6),

            // TSX/TXS (Transfer)
            new(0xBA, TSX, AddressingMode.None, 1, 2),
            new(0x9A, TXS, AddressingMode.None, 1, 2),

            // WAI (Wait for Interrupt)
            new(0xCB, WAI, AddressingMode.None, 1, 3)
        ];

        AddDebugInstructions();
    }

    /// <summary>
    /// Add custom opcodes for Emulator Debugger
    /// </summary>
    public void AddDebugInstructions()
    {
        Instructions.AddRange(
        [
            // DBP (Debugger Breakpoint)
            new (0x42, DBP, AddressingMode.None, 1, 2),
            // DRS (Dump Registers)
            new (0x43, DRS, AddressingMode.None, 1, 2),
            // DMP (Dump Memory Page)
            new (0x44, DMP, AddressingMode.None, 1, 2),
        ]);

        // Unofficial / undefined opcodes that behave as NOPs on the 65C02
        // new (0x02, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x03, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x0B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x13, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x1B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x22, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x23, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x2B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x33, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x3B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x42, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x43, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x44, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x4B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x53, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x54, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x5B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x5C, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x62, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x63, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x6B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x73, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x7B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x82, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x83, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x8B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x93, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0x9B, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xA3, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xAB, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xB3, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xBB, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xC2, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xC3, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xD3, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xD4, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xDC, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xE2, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xE3, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xEB, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xF3, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xF4, Mnemonic.NOP, AddressingMode.None, 1, 2),
        // new (0xFB, Mnemonic.NOP, AddressingMode.None, 1, 2),
    }

    public Instruction FromOpcode(byte opcode)
    {
        try
        {
            return Instructions.First(i => i.Opcode == opcode);
        }
        catch(Exception x)
        {
            Console.WriteLine("No instruction matching opcode: " + opcode.ToString("X2"));
            throw x;
        }
    }

    public List<Instruction> FromMnemonic(Mnemonic mnemonic)
    {
        return Instructions.Where(i => i.Mnemonic == mnemonic).ToList();
    }
}
