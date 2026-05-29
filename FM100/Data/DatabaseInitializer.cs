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

            var requiredTables = new[] { "Clubs", "FootballPlayers", "Leagues", "Fixtures", "Matches", "GameSaves" };
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
        }
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
                    Potential INTEGER NOT NULL,
                    Reputation INTEGER NOT NULL,
                    MarketValue INTEGER NOT NULL,
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
                    MatchDate TEXT NOT NULL,
                    Status INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_fixture_league ON Fixtures(LeagueId);

                CREATE TABLE IF NOT EXISTS Matches (
                    Id TEXT PRIMARY KEY,
                    FixtureId TEXT NOT NULL,
                    HomeClubId TEXT NOT NULL,
                    AwayClubId TEXT NOT NULL,
                    HomeScore INTEGER NOT NULL,
                    AwayScore INTEGER NOT NULL,
                    Status INTEGER NOT NULL,
                    MatchData TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    FOREIGN KEY (FixtureId) REFERENCES Fixtures(Id)
                );

                CREATE INDEX IF NOT EXISTS idx_match_fixture ON Matches(FixtureId);

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
