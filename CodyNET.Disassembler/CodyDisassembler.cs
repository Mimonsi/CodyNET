using System.Text;
using CodyNET.Common;

namespace CodyNET.Disassembler;

public sealed record CodyDisassemblerOptions
{
    public bool AsCartridge { get; init; }
    public ushort LoadAddress { get; init; } = 0xE000;
}

public class CodyDisassembler
{
    public static void DisassembleFile(FileInfo input, FileInfo output)
    {
        DisassembleFile(input, output, new CodyDisassemblerOptions());
    }

    public static void DisassembleFile(FileInfo input, FileInfo output, CodyDisassemblerOptions options)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (!input.Exists)
            throw new FileNotFoundException("Input file not found", input.FullName);

        var bytes = File.ReadAllBytes(input.FullName);
        var (program, loadAddress) = ParseInput(bytes, options);
        string disassembly = Disassemble(program, loadAddress);
        
        File.WriteAllText(output.FullName, disassembly);
    }

    public static string Disassemble(byte[] bytes)
    {
        return Disassemble(bytes, null);
    }

    public static string Disassemble(byte[] bytes, ushort? loadAddress)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));

        var builder = new StringBuilder();
        if (loadAddress.HasValue)
        {
            builder.AppendLine($"* = ${loadAddress.Value:X4}");
            builder.AppendLine();
        }

        int index = 0;

        while (index < bytes.Length)
        {
            int instructionStartIndex = index;
            byte opcode = bytes[index++];
            var instruction = OpcodeLookup.Instructions.FirstOrDefault(item => item.Opcode == opcode);

            if (instruction == null)
            {
                builder.AppendLine($".byte ${opcode:X2}");
                continue;
            }

            ushort instructionAddress = (ushort)((loadAddress ?? 0) + instructionStartIndex);
            var operands = new List<string>();
            string operand = FormatOperand(instruction.AddressingMode, bytes, ref index, instructionAddress, instruction.Bytes);
            if (operand == "??")
            {
                // Ran out of bytes mid-instruction; emit the opcode as raw data
                index = instructionStartIndex + 1;
                builder.AppendLine($".byte ${opcode:X2}");
                continue;
            }
            if (!string.IsNullOrEmpty(operand))
                operands.Add(operand);

            if (instruction.AddressingMode2.HasValue)
            {
                string operand2 = FormatOperand(instruction.AddressingMode2.Value, bytes, ref index, instructionAddress, instruction.Bytes);
                if (operand2 == "??")
                {
                    index = instructionStartIndex + 1;
                    builder.AppendLine($".byte ${opcode:X2}");
                    continue;
                }
                if (!string.IsNullOrEmpty(operand2))
                    operands.Add(operand2);
            }

            builder.AppendLine(FormatInstruction(instruction.Mnemonic, operands));
        }

        return builder.ToString();
    }

    private static (byte[] Program, ushort LoadAddress) ParseInput(byte[] bytes, CodyDisassemblerOptions options)
    {
        if (!options.AsCartridge)
            return (bytes, options.LoadAddress);

        if (bytes.Length < 4)
            throw new InvalidDataException("Cartridge header must be at least 4 bytes.");

        ushort start = (ushort)(bytes[0] | (bytes[1] << 8));
        ushort end = (ushort)(bytes[2] | (bytes[3] << 8));
        int length = end - start + 1;

        if (length <= 0)
            throw new InvalidDataException("Cartridge header invalid (end < start).");

        if (bytes.Length < length + 4)
            throw new InvalidDataException("Cartridge image truncated (data shorter than header claims).");

        var program = new byte[length];
        Buffer.BlockCopy(bytes, 4, program, 0, length);
        return (program, start);
    }

    // Bit-manipulation instructions (RMB0-7, SMB0-7, BBR0-7, BBS0-7) embed the bit number
    // in the mnemonic (e.g. RMB4), but 64tass expects it as a separate operand: rmb 4,$zp
    private static string FormatInstruction(Mnemonic mnemonic, List<string> operands)
    {
        string name = mnemonic.ToString();
        if (name.Length == 4)
        {
            string prefix = name[..3];
            if (prefix is "RMB" or "SMB" or "BBR" or "BBS")
            {
                char bitNum = name[3];
                return $"{prefix.ToLowerInvariant()} {bitNum},{string.Join(",", operands)}";
            }
        }
        if (operands.Count == 0)
            return name;
        return $"{name} {string.Join(", ", operands)}";
    }

    private static string FormatOperand(AddressingMode mode, byte[] bytes, ref int index, ushort instructionAddress, int instructionSize)
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
                AddressingMode.ProgramCounterRelative => FormatRelativeTarget(value, instructionAddress, instructionSize),
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

    private static string FormatRelativeTarget(byte relativeOffset, ushort instructionAddress, int instructionSize)
    {
        int nextInstructionAddress = instructionAddress + instructionSize;
        int signedOffset = unchecked((sbyte)relativeOffset);
        ushort targetAddress = (ushort)(nextInstructionAddress + signedOffset);
        return $"${targetAddress:X4}";
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
