namespace CodyNET.Core.Cody;

public record UartState
{
    public byte Control { get; init; }
    public byte Command { get; init; }
    public byte Status { get; init; }
    public byte RxHead { get; init; }
    public byte RxTail { get; init; }
    public byte[] RxData { get; init; } = [];
    public byte TxHead { get; init; }
    public byte TxTail { get; init; }
    public byte[] TxData { get; init; } = [];
}
