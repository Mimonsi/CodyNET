namespace CodyNET.Core.Cody;

public class Interrupt
{
    public bool IRQ;
    public bool NMI;

    public static Interrupt None => new();

    public Interrupt Or(Interrupt update)
    {
        return new Interrupt
        {
            IRQ = this.IRQ || update.IRQ,
            NMI = this.NMI || update.NMI
        };
    }
}