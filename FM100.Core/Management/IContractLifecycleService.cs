namespace FM100.Core.Management;

public interface IContractLifecycleService
{
    ContractLifecycleReport ResolveExpiredContracts(GameState.GameState gameState);
}
