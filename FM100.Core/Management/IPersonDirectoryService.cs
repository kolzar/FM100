namespace FM100.Core.Management;

public interface IPersonDirectoryService
{
    int EnsureDirectory(GameState.GameState gameState);

    IReadOnlyList<PersonSearchEntry> Search(
        GameState.GameState gameState,
        string? searchText = null,
        PersonCategory category = PersonCategory.All,
        Guid? clubId = null,
        int take = 2000);

    PersonDetail? GetDetail(GameState.GameState gameState, Guid personId);
}
