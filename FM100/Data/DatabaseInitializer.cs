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
        else
        {
            // Database exists, but ensure all required tables exist
            EnsureTablesExist();
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
            command.CommandText = GetCreateTablesSql();
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Ensures all required tables exist in the database.
    /// </summary>
    private static void EnsureTablesExist()
    {
        using (var connection = new SQLiteConnection(GetConnectionString()))
        {
            connection.Open();

            var requiredTables = new[]
            {
                "Clubs",
                "FootballPlayers",
                "Leagues",
                "Fixtures",
                "Matches",
                "MatchEvents",
                "MatchStatistics",
                "GameSaves"
            };
            bool needsCreation = false;

            var command = connection.CreateCommand();
            foreach (var tableName in requiredTables)
            {
                command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
                var result = command.ExecuteScalar();
                if (result == null)
                {
                    needsCreation = true;
                    break;
                }
            }

            // If any table is missing, create all (using IF NOT EXISTS to avoid errors)
            if (needsCreation)
            {
                command.CommandText = GetCreateTablesSql();
                command.ExecuteNonQuery();
            }

            EnsureColumnExists(connection, "Fixtures", "ScheduledDate", "TEXT");
            EnsureColumnExists(connection, "Fixtures", "MatchWeek", "INTEGER NOT NULL DEFAULT 1");
            EnsureColumnExists(connection, "Fixtures", "IsPlayed", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumnExists(connection, "Fixtures", "MatchId", "TEXT");
            EnsureColumnExists(connection, "Fixtures", "UpdatedAt", "TEXT NOT NULL DEFAULT ''");
            EnsureColumnExists(connection, "Fixtures", "MatchDate", "TEXT NOT NULL DEFAULT ''");
            EnsureColumnExists(connection, "Fixtures", "Status", "INTEGER NOT NULL DEFAULT 0");

            EnsureColumnExists(connection, "Matches", "HomeGoals", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumnExists(connection, "Matches", "AwayGoals", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumnExists(connection, "Matches", "PlayedAt", "TEXT");
            EnsureColumnExists(connection, "Matches", "Events", "TEXT NOT NULL DEFAULT '[]'");
            EnsureColumnExists(connection, "Matches", "HomePerformanceRating", "INTEGER NOT NULL DEFAULT 10");
            EnsureColumnExists(connection, "Matches", "AwayPerformanceRating", "INTEGER NOT NULL DEFAULT 10");
            EnsureColumnExists(connection, "Matches", "HomeScore", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumnExists(connection, "Matches", "AwayScore", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumnExists(connection, "Matches", "MatchData", "TEXT NOT NULL DEFAULT '{}'");
            EnsureColumnExists(connection, "Matches", "UpdatedAt", "TEXT NOT NULL DEFAULT ''");

            EnsureColumnExists(connection, "FootballPlayers", "Position", "INTEGER NOT NULL DEFAULT 3");
            EnsureColumnExists(connection, "FootballPlayers", "InjuryDaysRemaining", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumnExists(connection, "FootballPlayers", "InjuryDescription", "TEXT NOT NULL DEFAULT ''");
        }
    }

    private static void EnsureColumnExists(SQLiteConnection connection, string tableName, string columnName, string columnDefinition)
    {
        var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName})";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        command = connection.CreateCommand();
        command.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Gets the SQL for creating all tables.
    /// </summary>
    private static string GetCreateTablesSql()
    {
        return @"
                CREATE TABLE IF NOT EXISTS Clubs (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Abbreviation TEXT NOT NULL,
                    Division INTEGER NOT NULL,
                    City TEXT NOT NULL,
                    StadiumName TEXT NOT NULL,
                    StadiumCapacity INTEGER NOT NULL,
                    BudgetInMillions INTEGER NOT NULL,
                    Reputation INTEGER NOT NULL,
                    FanSatisfaction INTEGER NOT NULL,
                    SeasonWins INTEGER NOT NULL,
                    SeasonDraws INTEGER NOT NULL,
                    SeasonLosses INTEGER NOT NULL,
                    GoalsFor INTEGER NOT NULL,
                    GoalsAgainst INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_club_division ON Clubs(Division);
                CREATE INDEX IF NOT EXISTS idx_club_name ON Clubs(Name);

                CREATE TABLE IF NOT EXISTS FootballPlayers (
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
                    Position INTEGER NOT NULL DEFAULT 3,
                    Potential INTEGER NOT NULL,
                    Reputation INTEGER NOT NULL,
                    MarketValue INTEGER NOT NULL,
                    InjuryDaysRemaining INTEGER NOT NULL DEFAULT 0,
                    InjuryDescription TEXT NOT NULL DEFAULT '',
                    CurrentState TEXT NOT NULL,
                    MentalAttributes TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_player_name ON FootballPlayers(FirstName, LastName);
                CREATE INDEX IF NOT EXISTS idx_player_shirt ON FootballPlayers(ShirtNumber);

                CREATE TABLE IF NOT EXISTS Leagues (
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

                CREATE INDEX IF NOT EXISTS idx_league_season ON Leagues(Season, Division);

                CREATE TABLE IF NOT EXISTS Fixtures (
                    Id TEXT PRIMARY KEY,
                    LeagueId TEXT NOT NULL,
                    HomeClubId TEXT NOT NULL,
                    AwayClubId TEXT NOT NULL,
                    ScheduledDate TEXT NOT NULL,
                    MatchDate TEXT NOT NULL DEFAULT '',
                    MatchWeek INTEGER NOT NULL,
                    IsPlayed INTEGER NOT NULL,
                    Status INTEGER NOT NULL DEFAULT 0,
                    MatchId TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_fixture_league ON Fixtures(LeagueId);

                CREATE TABLE IF NOT EXISTS Matches (
                    Id TEXT PRIMARY KEY,
                    FixtureId TEXT NOT NULL,
                    HomeClubId TEXT NOT NULL,
                    AwayClubId TEXT NOT NULL,
                    HomeGoals INTEGER NOT NULL,
                    AwayGoals INTEGER NOT NULL,
                    HomeScore INTEGER NOT NULL DEFAULT 0,
                    AwayScore INTEGER NOT NULL DEFAULT 0,
                    Status INTEGER NOT NULL,
                    PlayedAt TEXT NOT NULL,
                    Events TEXT NOT NULL,
                    MatchData TEXT NOT NULL DEFAULT '{}',
                    HomePerformanceRating INTEGER NOT NULL,
                    AwayPerformanceRating INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL DEFAULT '',
                    FOREIGN KEY (FixtureId) REFERENCES Fixtures(Id)
                );

                CREATE INDEX IF NOT EXISTS idx_match_fixture ON Matches(FixtureId);

                CREATE TABLE IF NOT EXISTS MatchEvents (
                    Id TEXT PRIMARY KEY,
                    MatchId TEXT NOT NULL,
                    TeamId TEXT NOT NULL,
                    EventType INTEGER NOT NULL,
                    Minute INTEGER NOT NULL,
                    Description TEXT NOT NULL,
                    EmotionalImpact INTEGER NOT NULL,
                    Timestamp TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (MatchId) REFERENCES Matches(Id)
                );

                CREATE INDEX IF NOT EXISTS idx_matchevent_match ON MatchEvents(MatchId);

                CREATE TABLE IF NOT EXISTS MatchStatistics (
                    Id TEXT PRIMARY KEY,
                    MatchId TEXT NOT NULL,
                    TeamId TEXT NOT NULL,
                    GoalsScored INTEGER NOT NULL,
                    GoalsAgainst INTEGER NOT NULL,
                    Possession TEXT NOT NULL,
                    Shots INTEGER NOT NULL,
                    ShotsOnTarget INTEGER NOT NULL,
                    Fouls INTEGER NOT NULL,
                    YellowCards INTEGER NOT NULL,
                    RedCards INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (MatchId) REFERENCES Matches(Id)
                );

                CREATE INDEX IF NOT EXISTS idx_matchstats_match ON MatchStatistics(MatchId);
                CREATE INDEX IF NOT EXISTS idx_matchstats_team ON MatchStatistics(TeamId);

                CREATE TABLE IF NOT EXISTS GameSaves (
                    Id TEXT PRIMARY KEY,
                    PlayerClubId TEXT NOT NULL,
                    Season INTEGER NOT NULL,
                    Budget INTEGER NOT NULL,
                    SaveName TEXT NOT NULL,
                    SaveData TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_gamesave_club ON GameSaves(PlayerClubId);
            ";
    }
}
