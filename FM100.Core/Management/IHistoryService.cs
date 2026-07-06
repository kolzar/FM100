namespace FM100.Core.Management;

public interface IHistoryService
{
    IReadOnlyList<HistoryTitleEntry> GetTitleHistory(GameState.GameState gameState);

    IReadOnlyList<ManagerHistoryEntry> GetManagerHistory(GameState.GameState gameState);

    IReadOnlyList<UnbeatenHistoryEntry> GetUnbeatenHistory(GameState.GameState gameState, int take = 12);

    IReadOnlyList<BestSeasonHistoryEntry> GetBestSeasonHistory(GameState.GameState gameState, int take = 20);

    IReadOnlyList<LeagueTableHistoryEntry> GetLeagueTableHistory(GameState.GameState gameState, int take = 600);

    IReadOnlyList<RollOfHonourEntry> GetRollOfHonour(GameState.GameState gameState, int take = 100);

    IReadOnlyList<ClubSeasonHistoryEntry> GetClubSeasonHistory(GameState.GameState gameState, Guid clubId, int take = 100);

    ClubCareerSummary GetClubCareerSummary(GameState.GameState gameState, Guid clubId);

    IReadOnlyList<ClubSeasonSummaryEntry> GetClubSeasonSummaries(GameState.GameState gameState, Guid clubId, int take = 100);

    IReadOnlyList<InjuryHistoryEntry> GetInjuryHistory(GameState.GameState gameState, int take = 40);

    IReadOnlyList<StaffHistoryEntry> GetStaffHistory(GameState.GameState gameState, int take = 30);

    IReadOnlyList<TeamTalkHistoryEntry> GetTeamTalkHistory(GameState.GameState gameState, int take = 40);

    IReadOnlyList<MediaStoryEntry> GetMediaHistory(GameState.GameState gameState, int take = 8);

    IReadOnlyList<SeasonAwardEntry> GetAwardHistory(GameState.GameState gameState, int take = 12);

    IReadOnlyList<PlayerDevelopmentEntry> GetPlayerDevelopmentHistory(GameState.GameState gameState, int take = 12);

    IReadOnlyList<PlayerCareerEventEntry> GetPlayerCareerEvents(GameState.GameState gameState, int take = 30);

    IReadOnlyList<TransferHistoryEntry> GetTransferHistory(GameState.GameState gameState, int take = 30);

    IReadOnlyList<ContractHistoryEntry> GetContractHistory(GameState.GameState gameState, int take = 30);

    IReadOnlyList<ClubFinanceHistoryEntry> GetClubFinanceHistory(GameState.GameState gameState, int take = 48);

    IReadOnlyList<FinanceHistoryEntry> GetFinanceHistory(GameState.GameState gameState, int take = 12, Guid? clubId = null);

    IReadOnlyList<SeasonReviewEntry> GetSeasonReviews(GameState.GameState gameState, int take = 100);
}
