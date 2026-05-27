using System.Data.SQLite;
using System.IO;

namespace FM100.Data;

/// <summary>
/// Handles SQLite database initialization and schema creation.
/// </summary>
public static class DatabaseInitializer
{
    private static readonly string DbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FM100",
        "FM100.db");

    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    public static string GetConnectionString()
    {
        return $"Data Source={DbPath};Version=3;";
    }

    /// <summary>
    /// Initializes the database, creating the directory and tables if needed.
    /// </summary>
    public static void Initialize()
    {
        // Ensure directory exists
        var dbDirectory = Path.GetDirectoryName(DbPath);
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        // Create tables if database doesn't exist
        if (!File.Exists(DbPath))
        {
            CreateTables();
        }
    }

    /// <summary>
    /// Creates the schema for all tables.
    /// </summary>
    private static void CreateTables()
    {
        using (var connection = new SQLiteConnection(GetConnectionString()))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE FootballPlayers (
                    Id TEXT PRIMARY KEY,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    BirthDate TEXT NOT NULL,
                    Age INTEGER NOT NULL,
                    Nationality TEXT NOT NULL,
                    Description TEXT,
                    Height INTEGER NOT NULL,
                    Weight INTEGER NOT NULL,
                    ShirtNumber INTEGER NOT NULL,
                    Potential INTEGER NOT NULL,
                    Reputation INTEGER NOT NULL,
                    MarketValue INTEGER NOT NULL,
                    CurrentState TEXT NOT NULL,
                    MentalAttributes TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE INDEX idx_player_name ON FootballPlayers(FirstName, LastName);
                CREATE INDEX idx_player_shirt ON FootballPlayers(ShirtNumber);

                CREATE TABLE Leagues (
                    Id TEXT PRIMARY KEY,
                    Season INTEGER NOT NULL,
                    Division INTEGER NOT NULL,
                    ClubIds TEXT NOT NULL,
                    FixtureIds TEXT NOT NULL,
                    CompletedMatchIds TEXT NOT NULL,
                    Standings TEXT NOT NULL,
                    StartDate TEXT NOT NULL,
                    EndDate TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE INDEX idx_league_season ON Leagues(Season, Division);

                CREATE TABLE Fixtures (
                    Id TEXT PRIMARY KEY,
                    LeagueId TEXT NOT NULL,
                    HomeClubId TEXT NOT NULL,
                    AwayClubId TEXT NOT NULL,
                    ScheduledDate TEXT NOT NULL,
                    MatchWeek INTEGER NOT NULL,
                    IsPlayed INTEGER NOT NULL,
                    MatchId TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    FOREIGN KEY(LeagueId) REFERENCES Leagues(Id)
                );

                CREATE INDEX idx_fixture_league ON Fixtures(LeagueId, MatchWeek);
                CREATE INDEX idx_fixture_scheduled ON Fixtures(ScheduledDate);
                CREATE INDEX idx_fixture_clubs ON Fixtures(HomeClubId, AwayClubId);

                CREATE TABLE Matches (
                    Id TEXT PRIMARY KEY,
                    FixtureId TEXT NOT NULL,
                    HomeClubId TEXT NOT NULL,
                    AwayClubId TEXT NOT NULL,
                    HomeGoals INTEGER NOT NULL,
                    AwayGoals INTEGER NOT NULL,
                    Status INTEGER NOT NULL,
                    PlayedAt TEXT NOT NULL,
                    Events TEXT NOT NULL,
                    HomePerformanceRating INTEGER NOT NULL,
                    AwayPerformanceRating INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY(FixtureId) REFERENCES Fixtures(Id)
                );

                CREATE INDEX idx_match_fixture ON Matches(FixtureId);
                CREATE INDEX idx_match_clubs ON Matches(HomeClubId, AwayClubId);
                CREATE INDEX idx_match_status ON Matches(Status);

                CREATE TABLE GameSaves (
                    SaveId TEXT PRIMARY KEY,
                    SaveName TEXT NOT NULL,
                    PlayerClubId TEXT NOT NULL,
                    CurrentSeason INTEGER NOT NULL,
                    CurrentLeagueId TEXT,
                    Clubs TEXT NOT NULL,
                    Leagues TEXT NOT NULL,
                    HallOfFame TEXT NOT NULL,
                    Difficulty INTEGER NOT NULL,
                    DaysElapsed INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    LastSavedAt TEXT NOT NULL
                );

                CREATE INDEX idx_save_date ON GameSaves(LastSavedAt DESC);
                CREATE INDEX idx_save_season ON GameSaves(CurrentSeason);
            ";

            command.ExecuteNonQuery();
        }
    }
}
