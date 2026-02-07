using System.Text;
using CodyNET.Common;

namespace CodyNET.Assembler;

public class CodyDisassembler
{
    public static string Disassemble(byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));
        
        var builder = new StringBuilder();
        int index = 0;

        while (index < bytes.Length)
        {
            byte opcode = bytes[index++];
            var instruction = OpcodeLookup.Instructions.FirstOrDefault(item => item.Opcode == opcode);

            if (instruction == null)
            {
                builder.AppendLine($"DB ${opcode:X2}");
                continue;
            }

            var operands = new List<string>();
            string operand = FormatOperand(instruction.AddressingMode, bytes, ref index);
            if (!string.IsNullOrEmpty(operand))
                operands.Add(operand);

            if (instruction.AddressingMode2.HasValue)
            {
                string operand2 = FormatOperand(instruction.AddressingMode2.Value, bytes, ref index);
                if (!string.IsNullOrEmpty(operand2))
                    operands.Add(operand2);
            }

            if (operands.Count == 0)
                builder.AppendLine(instruction.Mnemonic.ToString());
            else
                builder.AppendLine($"{instruction.Mnemonic} {string.Join(", ", operands)}");
        }

        return builder.ToString();
    }

    private static string FormatOperand(AddressingMode mode, byte[] bytes, ref int index)
    {
        int size = OperandSize(mode);

        if (size == 0)
        {
            return mode == AddressingMode.Accumulator ? "A" : string.Empty;
        }

        if (index + size > bytes.Length)
        {
            index = bytes.Length;
            return "??";
        }

        if (size == 1)
        {
            byte value = bytes[index++];
            return mode switch
            {
                AddressingMode.Immediate => $"#${value:X2}",
                AddressingMode.ZeroPage => $"${value:X2}",
                AddressingMode.ZeroPageIndexedX => $"${value:X2},X",
                AddressingMode.ZeroPageIndexedY => $"${value:X2},Y",
                AddressingMode.ZeroPageIndirect => $"(${value:X2})",
                AddressingMode.ZeroPageIndexedIndirectX => $"(${value:X2},X)",
                AddressingMode.ZeroPageIndirectIndexedY => $"(${value:X2}),Y",
                AddressingMode.ProgramCounterRelative => $"${value:X2}",
                _ => $"${value:X2}"
            };
        }

        ushort word = (ushort)(bytes[index] | (bytes[index + 1] << 8));
        index += 2;

        return mode switch
        {
            AddressingMode.Absolute => $"${word:X4}",
            AddressingMode.AbsoluteIndexedX => $"${word:X4},X",
            AddressingMode.AbsoluteIndexedY => $"${word:X4},Y",
            AddressingMode.AbsoluteIndirect => $"(${word:X4})",
            AddressingMode.AbsoluteIndexedIndirectX => $"(${word:X4},X)",
            _ => $"${word:X4}"
        };
    }

    private static int OperandSize(AddressingMode mode)
    {
        return mode switch
        {
            AddressingMode.None => 0,
            AddressingMode.Accumulator => 0,
            AddressingMode.Immediate => 1,
            AddressingMode.ZeroPage => 1,
            AddressingMode.ZeroPageIndexedX => 1,
            AddressingMode.ZeroPageIndexedY => 1,
            AddressingMode.ZeroPageIndirect => 1,
            AddressingMode.ZeroPageIndexedIndirectX => 1,
            AddressingMode.ZeroPageIndirectIndexedY => 1,
            AddressingMode.ProgramCounterRelative => 1,
            AddressingMode.Absolute => 2,
            AddressingMode.AbsoluteIndexedX => 2,
            AddressingMode.AbsoluteIndexedY => 2,
            AddressingMode.AbsoluteIndirect => 2,
            AddressingMode.AbsoluteIndexedIndirectX => 2,
            _ => 0
        };
    }
}