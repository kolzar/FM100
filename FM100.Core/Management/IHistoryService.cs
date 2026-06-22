namespace FM100.Core.Management;

public interface IHistoryService
{
    IReadOnlyList<HistoryTitleEntry> GetTitleHistory(GameState.GameState gameState);

    IReadOnlyList<MediaStoryEntry> GetMediaHistory(GameState.GameState gameState, int take = 8);
}
