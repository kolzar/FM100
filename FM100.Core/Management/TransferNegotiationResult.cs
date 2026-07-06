namespace FM100.Core.Management;

public sealed class TransferNegotiationResult : TransferResult
{
    public bool Accepted { get; init; }
    public bool Countered { get; init; }
    public int OfferInMillions { get; init; }
    public int? CounterOfferInMillions { get; init; }
}
