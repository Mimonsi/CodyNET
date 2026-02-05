using CodyNET.Utils;

namespace CodyNET.Assembler;

public class CodyAssembler : ICodyAssembler
{
    public static byte[] AssembleFile(string file)
    {
        try
        {
            string assemblyCode = File.ReadAllText(file);
            Log.Info($"Assembling file {file}");
            return Assemble(assemblyCode);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to assemble file {file}: {ex.Message}");
            return Array.Empty<byte>();
        }
    }
    
    public static byte[] Assemble(string assemblyCode)
    {
        // Placeholder implementation
        // In a real assembler, this method would parse the assembly code
        // and convert it into machine code (byte array).
        List<byte> instructions = new List<byte>();
        
        return instructions.ToArray();
    }
}