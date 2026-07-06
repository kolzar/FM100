namespace FM100.Core.Management;

public interface ISquadLifecycleService
{
    SquadLifecycleReport ApplySeasonRollover(GameState.GameState gameState);
}
