namespace CodyNET.Core.Cody;

public struct Interrupt
{
    public bool IRQ;
    public bool NMI;

    public static Interrupt None => new();
    public static Interrupt IrqReq => new() { IRQ = true };
    public static Interrupt NmiReq => new() { NMI = true };

    public Interrupt Or(Interrupt update)
    {
        return new Interrupt
        {
            IRQ = IRQ || update.IRQ,
            NMI = NMI || update.NMI
        };
    }
}