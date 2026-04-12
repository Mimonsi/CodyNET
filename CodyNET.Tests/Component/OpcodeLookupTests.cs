using CodyNET.Common;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class OpcodeLookupTests
{
    [Test]
    public void NoDuplicateOpcodes()
    {
        var opcodes = OpcodeLookup.Instructions.Select(i => i.Opcode).ToList();
        var duplicates = opcodes.GroupBy(o => o).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.IsEmpty(duplicates, $"Duplicate opcodes: {string.Join(", ", duplicates.Select(o => $"${o:X2}"))}");
    }

    [Test]
    public void AllInstructionsHaveValidByteCount()
    {
        foreach (var instr in OpcodeLookup.Instructions)
        {
            Assert.That(instr.Bytes, Is.InRange(1, 3),
                $"Instruction {instr.Mnemonic} (${instr.Opcode:X2}) has invalid byte count: {instr.Bytes}");
        }
    }

    [Test]
    public void AllInstructionsHavePositiveCycles()
    {
        foreach (var instr in OpcodeLookup.Instructions)
        {
            Assert.That(instr.Cycles, Is.GreaterThan(0),
                $"Instruction {instr.Mnemonic} (${instr.Opcode:X2}) has non-positive cycle count: {instr.Cycles}");
        }
    }

    [TestCase((byte)0xA9, Mnemonic.LDA, AddressingMode.Immediate, 2)]
    [TestCase((byte)0xEA, Mnemonic.NOP, AddressingMode.None, 1)]
    [TestCase((byte)0x20, Mnemonic.JSR, AddressingMode.Absolute, 3)]
    [TestCase((byte)0x60, Mnemonic.RTS, AddressingMode.None, 1)]
    [TestCase((byte)0x00, Mnemonic.BRK, AddressingMode.None, 1)]
    [TestCase((byte)0x4C, Mnemonic.JMP, AddressingMode.Absolute, 3)]
    [TestCase((byte)0x8D, Mnemonic.STA, AddressingMode.Absolute, 3)]
    [TestCase((byte)0xAD, Mnemonic.LDA, AddressingMode.Absolute, 3)]
    public void KnownOpcodesMapsCorrectly(byte opcode, Mnemonic expectedMnemonic, AddressingMode expectedMode, int expectedBytes)
    {
        var instr = OpcodeLookup.Instructions.FirstOrDefault(i => i.Opcode == opcode);
        Assert.IsNotNull(instr, $"Opcode ${opcode:X2} not found");
        Assert.AreEqual(expectedMnemonic, instr!.Mnemonic);
        Assert.AreEqual(expectedMode, instr.AddressingMode);
        Assert.AreEqual(expectedBytes, instr.Bytes);
    }

    [Test]
    public void InstructionCollectionIsNotEmpty()
    {
        Assert.IsNotEmpty(OpcodeLookup.Instructions);
    }

    [Test]
    public void ImpliedInstructionsHaveOneByteAndNoOperand()
    {
        var implied = OpcodeLookup.Instructions.Where(i => i.AddressingMode == AddressingMode.None);
        foreach (var instr in implied)
        {
            Assert.AreEqual(1, instr.Bytes,
                $"{instr.Mnemonic} (${instr.Opcode:X2}) implied but has {instr.Bytes} bytes");
        }
    }
}
