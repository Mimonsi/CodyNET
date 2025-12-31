namespace CodyPrototype.Assembler;

public interface ICodyAssembler
{
    public byte[] AssembleFile(string file);

    public byte[] Assemble(string assemblyCode);
}