namespace CodyNET.Assembler;

public interface ICodyAssembler
{
    public static abstract byte[] AssembleFile(string file);

    public static abstract byte[] Assemble(string assemblyCode);
}