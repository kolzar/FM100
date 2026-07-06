namespace FM100.Core.Management;

public sealed record TransferOfferOption(
    string Key,
    string Label,
    int AmountInMillions,
    bool IsLikelyAccepted);
