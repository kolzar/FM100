namespace FM100.Core.Management;

public class ContractRenewalResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? PlayerId { get; init; }
}
