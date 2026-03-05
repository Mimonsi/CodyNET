namespace CodyNET.Core.Cody;

public class Interrupt
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
            IRQ = this.IRQ || update.IRQ,
            NMI = this.NMI || update.NMI
        };
    }
}