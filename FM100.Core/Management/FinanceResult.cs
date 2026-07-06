namespace FM100.Core.Management;

public sealed class FinanceResult
{
    public bool Success { get; init; }
    public int AmountInMillions { get; init; }
    public string Message { get; init; } = string.Empty;
}
