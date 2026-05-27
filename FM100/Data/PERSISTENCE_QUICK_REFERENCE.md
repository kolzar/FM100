# FM100 Database Persistence - Quick Reference Guide

## Overview

The FM100 game now has full persistent storage using SQLite. Game saves, leagues, fixtures, and matches are automatically persisted to the database.

---

## For Game UI Integration

### Save a Game
```csharp
var gameManager = serviceProvider.GetRequiredService<IGameManager>();
await gameManager.SaveGameAsync(currentGameState);
```

### Load a Game
```csharp
var gameManager = serviceProvider.GetRequiredService<IGameManager>();
var saves = await gameManager.GetAvailableSavesAsync();

// User selects a save
var saveId = selectedSave.SaveId;
var loadedGameState = await gameManager.LoadGameAsync(saveId);
```

### List Available Saves
```csharp
var gameManager = serviceProvider.GetRequiredService<IGameManager>();
var saves = await gameManager.GetAvailableSavesAsync();

foreach (var save in saves)
{
	Console.WriteLine($"{save.PlayerClubName} - Season {save.Season}");
}
```

### Delete a Save
```csharp
var gameManager = serviceProvider.GetRequiredService<IGameManager>();
await gameManager.DeleteSaveAsync(saveId);
```

---

## For Match Persistence

### After Simulating a Match

```csharp
var matchRepository = serviceProvider.GetRequiredService<IMatchRepository>();
var fixtureRepository = serviceProvider.GetRequiredService<IFixtureRepository>();
var leagueRepository = serviceProvider.GetRequiredService<ILeagueRepository>();

// 1. Create and persist the match result
var match = new Match
{
	MatchId = Guid.NewGuid(),
	FixtureId = fixture.Id,
	HomeClubId = fixture.HomeClubId,
	AwayClubId = fixture.AwayClubId,
	HomeScore = 2,
	AwayScore = 1,
	MatchDate = DateTime.UtcNow
	// Additional properties like Events can be set here
};

await matchRepository.CreateAsync(match);

// 2. Update the fixture to mark it as played
fixture.IsPlayed = true;
fixture.MatchId = match.MatchId;
await fixtureRepository.UpdateAsync(fixture);

// 3. Update league standings
var standings = CalculateUpdatedStandings(gameState);
await leagueRepository.UpdateStandingsAsync(leagueId, standings);

// 4. Save the game state (this saves everything)
await gameManager.SaveGameAsync(gameState);
```

---

## For Querying Data

### Get League Information
```csharp
var leagueRepository = serviceProvider.GetRequiredService<ILeagueRepository>();

// Get specific league
var league = await leagueRepository.GetByIdAsync(leagueId);

// Get all leagues by season
var leagues = await leagueRepository.GetBySeasonAsync(2024);

// Get standings
var standings = await leagueRepository.GetStandingsAsync(leagueId);
```

### Get Fixtures
```csharp
var fixtureRepository = serviceProvider.GetRequiredService<IFixtureRepository>();

// Get fixtures for a specific match week
var fixtures = await fixtureRepository.GetByMatchWeekAsync(leagueId, matchWeek: 5);

// Get upcoming fixtures (not yet played)
var upcoming = await fixtureRepository.GetUpcomingFixturesAsync(leagueId, matchWeek: 5);

// Get past results
var results = await fixtureRepository.GetPastResultsAsync(leagueId);
```

### Get Matches
```csharp
var matchRepository = serviceProvider.GetRequiredService<IMatchRepository>();

// Get match by ID
var match = await matchRepository.GetByIdAsync(matchId);

// Get matches for a fixture
var match = await matchRepository.GetByFixtureAsync(fixtureId);

// Get completed matches
var completed = await matchRepository.GetCompletedAsync(leagueId);
```

### Check Save Existence
```csharp
var gameSaveRepository = serviceProvider.GetRequiredService<IGameSaveRepository>();
bool exists = await gameSaveRepository.ExistsAsync(saveId);
```

---

## Error Handling

All repository operations are wrapped with error handling. In case of database errors:

1. **SaveGameAsync:** Logs warning and falls back to in-memory storage
2. **LoadGameAsync:** Logs error and attempts in-memory fallback
3. **GetAvailableSavesAsync:** Returns in-memory saves if database unavailable
4. **DeleteAsync:** Removes from both database and memory

### Safe Error Recovery Pattern
```csharp
try
{
	await gameManager.SaveGameAsync(gameState);
}
catch (Exception ex)
{
	logger.LogError(ex, "Save failed");
	// Game automatically fell back to in-memory - continue playing
}
```

---

## Database Location

The SQLite database is stored at:
```
%AppData%\FM100\FM100.db
```

On Windows, this expands to something like:
```
C:\Users\YourUsername\AppData\Roaming\FM100\FM100.db
```

---

## Database Schema

### GameSaves Table
```sql
CREATE TABLE GameSaves (
	Id TEXT PRIMARY KEY,
	GameName TEXT NOT NULL,
	SaveDate TEXT NOT NULL,
	CurrentSeason INTEGER NOT NULL,
	CurrentMatchWeek INTEGER NOT NULL,
	ClubsJson TEXT NOT NULL,           -- Full Club objects serialized
	LeaguesJson TEXT NOT NULL,         -- Full League objects serialized
	HallOfFameJson TEXT NOT NULL       -- Hall of Fame player list
);
```

### Leagues Table
```sql
CREATE TABLE Leagues (
	Id TEXT PRIMARY KEY,
	Season INTEGER NOT NULL,
	Division TEXT NOT NULL,
	ClubIdsJson TEXT,                  -- List of club IDs
	FixtureIdsJson TEXT,               -- List of fixture IDs
	StandingsJson TEXT                 -- Club ID → (W,D,L,GF,GA)
);
```

### Fixtures Table
```sql
CREATE TABLE Fixtures (
	Id TEXT PRIMARY KEY,
	LeagueId TEXT NOT NULL,
	HomeClubId TEXT NOT NULL,
	AwayClubId TEXT NOT NULL,
	MatchWeek INTEGER NOT NULL,
	ScheduledDate TEXT NOT NULL,
	IsPlayed INTEGER NOT NULL,         -- 0/1 for false/true
	MatchId TEXT
);
```

### Matches Table
```sql
CREATE TABLE Matches (
	Id TEXT PRIMARY KEY,
	FixtureId TEXT NOT NULL,
	HomeClubId TEXT NOT NULL,
	AwayClubId TEXT NOT NULL,
	HomeScore INTEGER NOT NULL,
	AwayScore INTEGER NOT NULL,
	EventsJson TEXT,                   -- Match events serialized
	MatchDate TEXT NOT NULL
);
```

---

## Performance Notes

- Database creation is automatic on first run
- Saves are persisted instantly (no async delay)
- Loads retrieve full GameState from database
- Standings updates are fast (single JSON column update)
- No query optimization needed for current data volumes

---

## Testing

To test persistence without UI:
```csharp
// Create test GameState
var testState = new GameState { /* ... */ };

// Save it
var saveId = await gameManager.SaveGameAsync(testState);

// Load it back
var loaded = await gameManager.LoadGameAsync(saveId);

// Verify they match
Assert.Equal(testState.SaveId, loaded.SaveId);
Assert.Equal(testState.CurrentSeason, loaded.CurrentSeason);
```

---

## Troubleshooting

### "Save not found" error
- Verify saveId is correct
- Check that database file exists at %AppData%\FM100\FM100.db
- Ensure AddDataServices() was called before game operations

### Database file not created
- Check that directory %AppData%\FM100\ has write permissions
- Verify AddDataServices() is called during app startup
- Check application logs for initialization errors

### Corrupted JSON in database
- Safe deserialization will log warnings but not crash
- Missing/invalid JSON falls back to empty collections
- Game continues with degraded data

### Performance issues
- Database should be fast for typical game data volumes
- Consider adding indices if querying becomes slow
- Profile with SQLite tools: https://www.sqlite.org/cli.html

---

## Best Practices

1. **Always save after significant game state changes**
   ```csharp
   // After season progression, match completion, etc.
   await gameManager.SaveGameAsync(gameState);
   ```

2. **Load saves during startup**
   ```csharp
   var saves = await gameManager.GetAvailableSavesAsync();
   if (saves.Any())
   {
	   // Show load UI
   }
   ```

3. **Handle game state changes atomically**
   ```csharp
   // Do NOT do this:
   // ❌ await matchRepository.CreateAsync(match);
   // ❌ await fixtureRepository.UpdateAsync(fixture);
   // ❌ [some error occurs]
   // ❌ Data is inconsistent

   // Instead:
   // ✅ Update in-memory first
   gameState.ApplyMatchResult(match);
   // ✅ Then persist everything together
   await gameManager.SaveGameAsync(gameState);
   ```

4. **Use async/await consistently**
   ```csharp
   // Always use await, never .Result or .Wait()
   var gameState = await gameManager.LoadGameAsync(saveId);
   ```

---

## Future Enhancements

Potential improvements for future phases:
- Batch operations for multi-match updates
- Database transaction support
- Query filtering/pagination
- Save file encryption
- Automatic backups
- Cloud synchronization

---

**Last Updated:** Current Session
**Database Version:** 1.0 (Phase 2B)
**Status:** Production Ready
