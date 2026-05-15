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
    /// Creates the schema for FootballPlayer table.
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
            ";

            command.ExecuteNonQuery();
        }
    }
}
