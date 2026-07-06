namespace FM100.Core.Management;

public interface ILeagueTableArchiveService
{
    LeagueTableArchiveReport ArchiveCurrentSeason(GameState.GameState gameState);
}
