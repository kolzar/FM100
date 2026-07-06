using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;
using FM100.Core.Management;

namespace FM100.Core.GameState;

/// <summary>
/// Represents the complete state of an active game.
/// </summary>
public class GameState
{
    /// <summary>
    /// Unique save ID.
    /// </summary>
    public Guid SaveId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Player's selected club.
    /// </summary>
    public Guid PlayerClubId { get; set; }

    /// <summary>
    /// Current season number.
    /// </summary>
    public int CurrentSeason { get; set; } = 1;

    /// <summary>
    /// Whether the 100-season career horizon has been reached.
    /// </summary>
    public bool IsCareerComplete { get; set; }

    /// <summary>
    /// Persistent identity and preferences of the human manager.
    /// </summary>
    public ManagerProfile Manager { get; set; } = new();

    /// <summary>
    /// Current league.
    /// </summary>
    public Guid? CurrentLeagueId { get; set; }

    /// <summary>
    /// All clubs in the game (across all divisions).
    /// </summary>
    public Dictionary<Guid, Club> Clubs { get; set; } = [];

    /// <summary>
    /// All leagues (3 divisions).
    /// </summary>
    public Dictionary<Guid, League> Leagues { get; set; } = [];

    /// <summary>
    /// All generated fixtures for the active game.
    /// </summary>
    public Dictionary<Guid, Fixture> Fixtures { get; set; } = [];

    /// <summary>
    /// All completed matches for the active game.
    /// </summary>
    public Dictionary<Guid, Match> Matches { get; set; } = [];

    /// <summary>
    /// All generated players available in the active game.
    /// </summary>
    public Dictionary<Guid, FootballPlayer> Players { get; set; } = [];

    /// <summary>
    /// Selected lineups by club ID.
    /// </summary>
    public Dictionary<Guid, TeamLineup> Lineups { get; set; } = [];

    /// <summary>
    /// Hall of Fame records.
    /// </summary>
    public HallOfFame HallOfFame { get; set; } = new();

    /// <summary>
    /// Career achievements unlocked in this save.
    /// </summary>
    public List<AchievementRecord> Achievements { get; set; } = [];

    /// <summary>
    /// Season awards recorded during the career.
    /// </summary>
    public List<SeasonAwardRecord> SeasonAwards { get; set; } = [];

    /// <summary>
    /// Player development changes recorded across completed seasons.
    /// </summary>
    public List<PlayerDevelopmentRecord> PlayerDevelopmentHistory { get; set; } = [];

    /// <summary>
    /// Retirements and academy promotions recorded across the career.
    /// </summary>
    public List<PlayerCareerEventRecord> PlayerCareerEvents { get; set; } = [];

    /// <summary>
    /// Completed transfers between AI-controlled clubs.
    /// </summary>
    public List<TransferHistoryRecord> TransferHistory { get; set; } = [];

    /// <summary>
    /// Contract renewals and free-agent releases across all clubs.
    /// </summary>
    public List<ContractHistoryRecord> ContractHistory { get; set; } = [];

    /// <summary>
    /// Annual financial settlements for every club in the simulated world.
    /// </summary>
    public List<ClubFinanceHistoryRecord> ClubFinanceHistory { get; set; } = [];

    /// <summary>
    /// Final Serie A/B/C tables archived before each season reset.
    /// </summary>
    public List<LeagueTableArchiveRecord> LeagueTableArchive { get; set; } = [];

    /// <summary>
    /// One hundred completed seasons generated before the manager career begins.
    /// Calendar years are used here so they never collide with career season numbers.
    /// </summary>
    public List<LeagueTableArchiveRecord> HistoricalLeagueTableArchive { get; set; } = [];

    public List<SeasonAwardRecord> HistoricalSeasonAwards { get; set; } = [];

    public Dictionary<Guid, int> HistoricalTitlesByClub { get; set; } = [];

    public int HistoricalStartYear { get; set; }

    public int HistoricalEndYear { get; set; }

    public DateTime? HistoricalWorldGeneratedAt { get; set; }

    /// <summary>
    /// Best performer for every club in each completed season.
    /// </summary>
    public List<ClubSeasonStarRecord> ClubSeasonStars { get; set; } = [];

    /// <summary>
    /// Injury incidents and recoveries across the simulated world.
    /// </summary>
    public List<InjuryHistoryRecord> InjuryHistory { get; set; } = [];

    public List<StaffHistoryRecord> StaffHistory { get; set; } = [];

    public List<TeamTalkHistoryRecord> TeamTalkHistory { get; set; } = [];

    /// <summary>
    /// Active and completed scouting assignments by player.
    /// </summary>
    public Dictionary<Guid, ScoutingAssignmentRecord> ScoutingAssignments { get; set; } = [];

    /// <summary>
    /// Players currently available to sign in the transfer market.
    /// </summary>
    public List<TransferListing> TransferMarket { get; set; } = [];

    /// <summary>
    /// Press and media events generated during the career.
    /// </summary>
    public List<MediaEventRecord> MediaEvents { get; set; } = [];

    /// <summary>
    /// Financial movements recorded during the career.
    /// </summary>
    public List<FinanceRecord> Finances { get; set; } = [];

    /// <summary>
    /// Current training setup for the player's squad.
    /// </summary>
    public TrainingSetup Training { get; set; } = new();

    /// <summary>
    /// Training sessions completed by the player's squad.
    /// </summary>
    public List<TrainingHistoryRecord> TrainingHistory { get; set; } = [];

    /// <summary>
    /// Staff quality for the player's club.
    /// </summary>
    public StaffSetup Staff { get; set; } = new();

    /// <summary>
    /// Game creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last save timestamp.
    /// </summary>
    public DateTime LastSavedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Game difficulty (0-10).
    /// </summary>
    public int Difficulty { get; set; } = 5;

    /// <summary>
    /// Total in-game time elapsed (simulated days).
    /// </summary>
    public int DaysElapsed { get; set; } = 0;

    public Dictionary<Guid, int> CurrentUnbeatenStreaks { get; set; } = [];

    public Dictionary<Guid, DateTime> CurrentUnbeatenStreakStarts { get; set; } = [];

    /// <summary>
    /// Gets the player's club.
    /// </summary>
    public Club? GetPlayerClub() => Clubs.TryGetValue(PlayerClubId, out var club) ? club : null;

    /// <summary>
    /// Gets the current league.
    /// </summary>
    public League? GetCurrentLeague() => CurrentLeagueId.HasValue && 
        Leagues.TryGetValue(CurrentLeagueId.Value, out var league) ? league : null;
}

/// <summary>
/// Hall of Fame tracking system (100-year records).
/// </summary>
public class HallOfFame
{
    /// <summary>
    /// Most titles won (club -> count).
    /// </summary>
    public Dictionary<Guid, int> TitlesByClub { get; set; } = [];

    /// <summary>
    /// Top managers by seasons managed.
    /// </summary>
    public List<ManagerRecord> TopManagers { get; set; } = [];

    /// <summary>
    /// Longest unbeaten streaks.
    /// </summary>
    public List<UnbeatableStreak> UnbeatableStreaks { get; set; } = [];

    /// <summary>
    /// Best individual seasons (player -> stats).
    /// </summary>
    public Dictionary<Guid, SeasonRecord> BestSeasons { get; set; } = [];
}

public class ManagerProfile
{
    public string Name { get; set; } = "Manager";
    public string Nationality { get; set; } = "Italian";
    public string PreferredFormation { get; set; } = "4-3-3";
    public string Personality { get; set; } = "Balanced";
    public int MediaReputation { get; set; } = 10;
    public int BoardConfidence { get; set; } = 10;
}

/// <summary>
/// Achievement unlocked during a career save.
/// </summary>
public class AchievementRecord
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Season { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Award recorded when a season is completed.
/// </summary>
public class SeasonAwardRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public string AwardKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string WinnerName { get; set; } = string.Empty;
    public Guid? ClubId { get; set; }
    public Guid? PlayerId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Development change recorded for a player at season rollover.
/// </summary>
public class PlayerDevelopmentRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public Guid? ClubId { get; set; }
    public int Season { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int ReputationBefore { get; set; }
    public int ReputationAfter { get; set; }
    public int PotentialBefore { get; set; }
    public int PotentialAfter { get; set; }
    public int MarketValueBefore { get; set; }
    public int MarketValueAfter { get; set; }
    public int PlayedMinutes { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PlayerCareerEventRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public Guid PlayerId { get; set; }
    public Guid ClubId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string ClubName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TransferHistoryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public Guid FromClubId { get; set; }
    public string FromClubName { get; set; } = string.Empty;
    public Guid ToClubId { get; set; }
    public string ToClubName { get; set; } = string.Empty;
    public int FeeInMillions { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ContractHistoryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public Guid ClubId { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public int ContractExpiresSeason { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ClubFinanceHistoryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public Guid ClubId { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public int FinalPosition { get; set; }
    public int SponsorshipInMillions { get; set; }
    public int PrizeMoneyInMillions { get; set; }
    public int WageCostInMillions { get; set; }
    public int NetAmountInMillions { get; set; }
    public int ClosingBudgetInMillions { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LeagueTableArchiveRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public Division Division { get; set; }
    public List<LeagueTableArchiveRow> Rows { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LeagueTableArchiveRow
{
    public int Position { get; set; }
    public Guid ClubId { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Played { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int GoalDifference { get; set; }
}

public class ClubSeasonStarRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public Guid ClubId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Appearances { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int AverageRating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class InjuryHistoryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public int Day { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public Guid ClubId { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public string InjuryType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int InitialDays { get; set; }
    public int? RecoveredAtDay { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class StaffHistoryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public int CostInMillions { get; set; }
    public int CoachQualityBefore { get; set; }
    public int CoachQualityAfter { get; set; }
    public int PhysioQualityBefore { get; set; }
    public int PhysioQualityAfter { get; set; }
    public int ScoutQualityBefore { get; set; }
    public int ScoutQualityAfter { get; set; }
    public int ContractExpiresSeason { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ScoutingAssignmentRecord
{
    public Guid PlayerId { get; set; }
    public int Progress { get; set; }
    public int StartedDay { get; set; }
    public int LastUpdatedDay { get; set; }
}

/// <summary>
/// Transfer market listing stored in the save game.
/// </summary>
public class TransferListing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public int AskingPriceInMillions { get; set; }
    public int WageDemandInMillions { get; set; }
    public int ContractYears { get; set; } = 3;
    public bool IsFreeAgent { get; set; }
    public DateTime ListedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Press/media event stored in the save game.
/// </summary>
public class MediaEventRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string StorylineKey { get; set; } = string.Empty;
    public int StorylineStage { get; set; } = 1;
    public int PressureLevel { get; set; } = 1;
    public string Headline { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public int Season { get; set; }
    public int Day { get; set; }
    public bool IsResolved { get; set; }
    public string Response { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string RecommendedResponse { get; set; } = string.Empty;
    public string RiskLabel { get; set; } = "Managed";
    public int ResponseEffectiveness { get; set; }
    public int MediaReputationBefore { get; set; }
    public int MediaReputationAfter { get; set; }
    public int FanSatisfactionBefore { get; set; }
    public int FanSatisfactionAfter { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>
/// Training setup selected by the player.
/// </summary>
public class TrainingSetup
{
    public TrainingFocus Focus { get; set; } = TrainingFocus.Balanced;
    public int Intensity { get; set; } = 2;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum TrainingFocus
{
    Balanced,
    Fitness,
    Tactical,
    Recovery,
    Youth
}

public class TrainingHistoryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public int Day { get; set; }
    public int Days { get; set; }
    public TrainingFocus Focus { get; set; }
    public int Intensity { get; set; }
    public int PlayersAffected { get; set; }
    public decimal AverageFatigueBefore { get; set; }
    public decimal AverageFatigueAfter { get; set; }
    public decimal AverageMoraleBefore { get; set; }
    public decimal AverageMoraleAfter { get; set; }
    public decimal AverageConfidenceBefore { get; set; }
    public decimal AverageConfidenceAfter { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TeamTalkHistoryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public int Day { get; set; }
    public TeamTalkStyle Style { get; set; }
    public int Effectiveness { get; set; }
    public int AffectedPlayers { get; set; }
    public decimal MoraleBefore { get; set; }
    public decimal MoraleAfter { get; set; }
    public decimal MotivationBefore { get; set; }
    public decimal MotivationAfter { get; set; }
    public decimal TrustBefore { get; set; }
    public decimal TrustAfter { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Staff department quality for the player's club.
/// </summary>
public class StaffSetup
{
    public int CoachQuality { get; set; } = 10;
    public int PhysioQuality { get; set; } = 10;
    public int ScoutQuality { get; set; } = 10;
    public int ContractExpiresSeason { get; set; } = 3;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum StaffDepartment
{
    Coaching,
    Physio,
    Scouting
}

/// <summary>
/// Financial movement stored in the save game.
/// </summary>
public class FinanceRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Season { get; set; }
    public int Day { get; set; }
    public string Type { get; set; } = string.Empty;
    public int AmountInMillions { get; set; }
    public Guid? MatchId { get; set; }
    public Guid? ClubId { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Manager record for hall of fame.
/// </summary>
public class ManagerRecord
{
    public Guid ClubId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public int Seasons { get; set; }
    public int Titles { get; set; }
    public int MatchesPlayed { get; set; }
    public int MatchesWon { get; set; }
    public double WinPercentage { get; set; }
}

/// <summary>
/// Unbeatable streak record.
/// </summary>
public class UnbeatableStreak
{
    public Guid ClubId { get; set; }
    public int MatchCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Individual season record.
/// </summary>
public class SeasonRecord
{
    public Guid PlayerId { get; set; }
    public Guid? ClubId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Season { get; set; }
    public int Appearances { get; set; }
    public int GoalsScored { get; set; }
    public int Assists { get; set; }
    public int AverageRating { get; set; }
}
