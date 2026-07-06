namespace FM100.Core.Management;

public interface IContractService
{
    ContractReport BuildReport(GameState.GameState gameState);

    IReadOnlyList<ContractRenewalQuote> GetRenewalQuotes(GameState.GameState gameState);

    ContractRenewalQuote? GetRenewalQuote(GameState.GameState gameState, Guid playerId, int extensionYears = 3);

    ContractRenewalResult RenewContract(GameState.GameState gameState, Guid playerId, int extensionYears = 3);
}
