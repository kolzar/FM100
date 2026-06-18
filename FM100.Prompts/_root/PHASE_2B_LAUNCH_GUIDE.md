# 🚀 PHASE 2B - DATABASE INTEGRATION LAUNCH GUIDE

## Current Status: ✅ Ready to Begin

**Previous Phase**: Phase 2 ✅ COMPLETE  
**Current Phase**: Phase 2B - DATABASE INTEGRATION  
**Timeline**: 6-8 hours estimated  
**Priority**: HIGH  

---

## What You're Starting With

### ✅ Already Complete From Phase 2
- GameManager orchestration
- Club/League/Fixture generation
- Match simulation engine
- Professional WPF UI (5 screens)
- DI infrastructure
- Repository pattern foundation
- Test framework (38+ tests)
- Database initializer stub

### ⏳ What Phase 2B Will Deliver
- Complete database schema
- League/Fixture/Match repositories (database-backed)
- Save/Load game system
- Real data binding in Dashboard
- Match results → standings updates
- Season completion and progression

---

## Phase 2B Roadmap

### Step 1: Database Schema & Initialization (2 hours)

**Objective**: Create and initialize all necessary database tables

**Files to Create/Modify**:
- `FM100.Data/DatabaseInitializer.cs` - Add schema creation
- `FM100.Data/Repositories/Implementation/LeagueRepository.cs` - Update to use DB
- `FM100.Data/Repositories/Implementation/FixtureRepository.cs` - Update to use DB
- `FM100.Data/Repositories/Implementation/MatchRepository.cs` - Update to use DB

**Tasks**:
1. [ ] Define SQL schema (Leagues, Fixtures, Matches, MatchEvents tables)
2. [ ] Implement schema creation in DatabaseInitializer
3. [ ] Verify tables are created on first run
4. [ ] Add seed data for test league

**Key Pattern**:
```csharp
// Example: LeagueRepository with Dapper + SQLite
public async Task<League> CreateAsync(League league)
{
	using (var connection = _connectionFactory.CreateConnection())
	{
		await connection.OpenAsync();
		const string sql = "INSERT INTO Leagues (Name, Division, Season) VALUES (@Name, @Division, @Season)";
		await connection.ExecuteAsync(sql, league);
	}
}
```

---

### Step 2: Repository Implementation (2 hours)

**Objective**: Implement all data repositories with real database operations

**Files to Update**:
- `FM100.Data/Repositories/Implementation/LeagueRepository.cs`
- `FM100.Data/Repositories/Implementation/FixtureRepository.cs`
- `FM100.Data/Repositories/Implementation/MatchRepository.cs`

**Methods to Implement**:

**LeagueRepository**:
- [x] CreateAsync(League)
- [x] GetByIdAsync(int)
- [x] GetBySeasonAsync(int)
- [x] UpdateAsync(League)
- [x] GetStandingsAsync(int leagueId)
- [x] UpdateStandingsAsync(List<LeagueStanding>)

**FixtureRepository**:
- [x] CreateAsync(Fixture)
- [x] GetByIdAsync(int)
- [x] GetByLeagueAsync(int leagueId)
- [x] GetFixturesForClubAsync(int clubId)
- [x] UpdateAsync(Fixture)
- [x] GetUpcomingFixturesAsync(int clubId, int count)
- [x] GetPastResultsAsync(int clubId, int count)

**MatchRepository**:
- [x] CreateAsync(Match)
- [x] GetByIdAsync(int)
- [x] GetByFixtureAsync(int fixtureId)
- [x] UpdateAsync(Match)
- [x] GetMatchDetailsAsync(int matchId)

**Key Pattern**:
```csharp
// Use QueryAsync for multiple results
public async Task<IEnumerable<Fixture>> GetByLeagueAsync(int leagueId)
{
	using (var connection = _connectionFactory.CreateConnection())
	{
		await connection.OpenAsync();
		const string sql = @"
			SELECT * FROM Fixtures 
			WHERE LeagueId = @LeagueId 
			ORDER BY FixtureDate";
		return await connection.QueryAsync<Fixture>(sql, new { LeagueId = leagueId });
	}
}
```

---

### Step 3: GameManager Integration (1.5 hours)

**Objective**: Wire GameManager to use persisted repositories

**Files to Modify**:
- `FM100.Core/Management/Implementation/GameManager.cs`

**Methods to Update**:
- [x] StartNewGameAsync() - Save league to DB
- [x] ProgressSeasonAsync() - Persist match results and standings
- [x] SaveGameAsync() - Persist GameState
- [x] LoadGameAsync() - Load GameState from DB

**Key Changes**:
```csharp
// Inject repositories into GameManager
public GameManager(
	ILeagueManager leagueManager,
	IMatchSimulator matchSimulator,
	ClubGenerator clubGenerator,
	ILeagueRepository leagueRepository,        // NEW
	IFixtureRepository fixtureRepository,      // NEW
	IMatchRepository matchRepository)          // NEW
{
	// Store for use in methods
}

// Persist after creating season
public async Task<GameState> StartNewGameAsync(Club selectedClub, int difficulty)
{
	var gameState = new GameState { /* ... */ };

	// Save to DB
	await _leagueRepository.CreateAsync(gameState.League);
	foreach (var fixture in gameState.League.Fixtures)
	{
		await _fixtureRepository.CreateAsync(fixture);
	}

	return gameState;
}
```

---

### Step 4: UI Data Binding (1.5 hours)

**Objective**: Replace placeholder data with real database queries

**Files to Modify**:
- `FM100/Views/GameDashboardView.xaml.cs`

**Updates Needed**:
- [x] RefreshFixtures() - Query from FixtureRepository
- [x] RefreshStandings() - Query from LeagueRepository
- [x] RefreshResults() - Query from FixtureRepository (past matches)

**Key Pattern**:
```csharp
private async void RefreshFixtures()
{
	var serviceProvider = (Application.Current as App)?.ServiceProvider;
	var fixtureRepository = serviceProvider?.GetRequiredService<IFixtureRepository>();

	if (fixtureRepository != null && _gameState?.CurrentClub != null)
	{
		var upcomingFixtures = await fixtureRepository.GetUpcomingFixturesAsync(
			_gameState.CurrentClub.ClubId, 
			5);

		UpcomingFixtures.ItemsSource = upcomingFixtures.ToList();
	}
}
```

---

### Step 5: Match Result Processing (1.5 hours)

**Objective**: Update standings after match completion

**Files to Create/Modify**:
- `FM100.Core/Management/Implementation/MatchSimulator.cs` - Already exists
- `FM100.Core/Management/ILeagueManager.cs` - Add method
- `FM100.Core/Management/Implementation/LeagueManager.cs` - Implement update logic

**Key Pattern**:
```csharp
// In LeagueManager
public async Task UpdateLeagueStandingsAsync(Match match, int leagueId)
{
	// Get the fixture
	var fixture = await _fixtureRepository.GetByIdAsync(match.FixtureId);

	// Mark as played and link match
	fixture.IsPlayed = true;
	fixture.MatchId = match.MatchId;
	await _fixtureRepository.UpdateAsync(fixture);

	// Update club stats
	var homeClub = match.HomeClub;
	var awayClub = match.AwayClub;

	if (match.HomeGoals > match.AwayGoals)
	{
		homeClub.SeasonWins++;
		awayClub.SeasonLosses++;
	}
	else if (match.HomeGoals < match.AwayGoals)
	{
		homeClub.SeasonLosses++;
		awayClub.SeasonWins++;
	}
	else
	{
		homeClub.SeasonDraws++;
		awayClub.SeasonDraws++;
	}

	homeClub.GoalsFor += match.HomeGoals;
	homeClub.GoalsAgainst += match.AwayGoals;
	awayClub.GoalsFor += match.AwayGoals;
	awayClub.GoalsAgainst += match.HomeGoals;

	// Recalculate standings
	var standings = CalculateStandings(leagueId);
	await _leagueRepository.UpdateStandingsAsync(standings);
}
```

---

### Step 6: Save/Load System (1 hour)

**Objective**: Persist and restore complete game state

**Files to Create**:
- `FM100.Core/Services/GameSaveService.cs` - NEW

**Methods**:
```csharp
public class GameSaveService
{
	// Serialize GameState to JSON
	public string SerializeGameState(GameState gameState) { }

	// Deserialize GameState from JSON
	public GameState DeserializeGameState(string json) { }

	// Save to database
	public async Task SaveAsync(GameState gameState, string saveName) { }

	// Load from database
	public async Task<GameState> LoadAsync(string saveName) { }

	// Get all saves
	public async Task<List<GameSave>> GetAllSavesAsync() { }

	// Delete save
	public async Task DeleteAsync(string saveName) { }
}
```

---

## Implementation Order

**DO IN THIS ORDER**:

1. ✅ **First**: Create database schema (Step 1)
   - Tables must exist before repositories can use them
   - Foundation for everything else

2. ✅ **Second**: Implement repositories (Step 2)
   - Must be complete before wiring into GameManager
   - Core data access layer

3. ✅ **Third**: Update GameManager (Step 3)
   - Depends on repositories being ready
   - Central orchestrator needs persistence

4. ✅ **Fourth**: Update UI (Step 4)
   - Depends on GameManager and repositories
   - Visual layer consumes data

5. ✅ **Fifth**: Match result processing (Step 5)
   - Depends on DB and repositories
   - Completes match simulation loop

6. ✅ **Sixth**: Save/Load system (Step 6)
   - Last because it uses all previous components
   - Optional in first MVP but recommended

---

## Testing Checklist

### After Step 1 (Database Schema)
- [ ] Application starts without DB errors
- [ ] Tables are created on first run
- [ ] Can connect to database

### After Step 2 (Repositories)
- [ ] Can insert data via repositories
- [ ] Can query data via repositories
- [ ] Data persists between runs
- [ ] Write unit tests for repository methods

### After Step 3 (GameManager Integration)
- [ ] StartNewGameAsync saves league to DB
- [ ] Data is readable from DB after save
- [ ] ProgressSeasonAsync updates standings
- [ ] Tests pass for GameManager

### After Step 4 (UI Data Binding)
- [ ] Dashboard shows real fixtures
- [ ] Dashboard shows real standings
- [ ] Dashboard shows real results
- [ ] All UI reads from database

### After Step 5 (Match Processing)
- [ ] Match completion updates standings
- [ ] Club stats update correctly
- [ ] League standings recalculated
- [ ] UI reflects changes

### After Step 6 (Save/Load)
- [ ] Can save game state
- [ ] Can load saved game
- [ ] Multiple saves possible
- [ ] Delete save works
- [ ] UI shows save list

---

## Key Files Reference

### Database Layer
- `FM100.Data/DatabaseInitializer.cs` - Schema creation
- `FM100.Data/Repositories/ILeagueRepository.cs` - Interface
- `FM100.Data/Repositories/Implementation/LeagueRepository.cs` - Implementation
- `FM100.Data/Repositories/IFixtureRepository.cs` - Interface
- `FM100.Data/Repositories/Implementation/FixtureRepository.cs` - Implementation
- `FM100.Data/Repositories/IMatchRepository.cs` - Interface
- `FM100.Data/Repositories/Implementation/MatchRepository.cs` - Implementation

### Business Logic
- `FM100.Core/Management/Implementation/GameManager.cs` - Main orchestrator
- `FM100.Core/Management/Implementation/LeagueManager.cs` - League logic
- `FM100.Core/Management/Implementation/MatchSimulator.cs` - Match simulation
- `FM100.Core/Services/GameSaveService.cs` - Persistence (NEW)

### UI
- `FM100/Views/GameDashboardView.xaml.cs` - Dashboard with data binding
- `FM100/App.xaml.cs` - DI registration for new services

### Tests
- `FM100.UnitTest/` - Add tests for repository operations

---

## Common Issues & Solutions

### Issue: "Table already exists" error
**Solution**: Delete the database file and rebuild
```bash
rm FM100.db
dotnet build && dotnet run --project FM100
```

### Issue: Foreign key constraints
**Solution**: Ensure all related entities exist before inserting
```csharp
// Create league first
await _leagueRepository.CreateAsync(league);

// Then create fixtures referencing the league
foreach (var fixture in fixtures)
{
	fixture.LeagueId = league.LeagueId;  // Set the FK
	await _fixtureRepository.CreateAsync(fixture);
}
```

### Issue: Concurrent database access
**Solution**: Use connection pooling and ensure async operations
```csharp
// Always await database calls
var league = await _leagueRepository.GetByIdAsync(leagueId);

// Never block
// ❌ WRONG: var league = _leagueRepository.GetByIdAsync(leagueId).Result;
```

### Issue: Data not persisting
**Solution**: Check that changes are awaited and committed
```csharp
// Ensure you await
await _leagueRepository.UpdateAsync(league);

// Not
_leagueRepository.UpdateAsync(league);  // ❌ WRONG - returns Task but not awaited
```

---

## Success Criteria

Phase 2B is complete when:

- [x] Database schema is created automatically
- [x] All repositories work with real database
- [x] GameManager saves/loads from DB
- [x] UI shows real data from database
- [x] Match results update standings
- [x] Save/Load system works
- [x] All tests still pass
- [x] Build is green (0 errors)
- [x] No breaking changes to Phase 2 UI
- [x] Ready for Phase 3

---

## Quick Start for Phase 2B

```bash
# 1. Verify current state
cd D:\My\github\FM100
dotnet build              # Should be green

# 2. Start implementing Step 1
# Edit FM100.Data/DatabaseInitializer.cs
# Add schema creation

# 3. Build and test
dotnet build
dotnet run --project FM100

# 4. Verify database was created
# Check for FM100.db in project root

# 5. Continue with Steps 2-6
# Follow the roadmap above

# 6. Commit progress
git add -A
git commit -m "🗄️ Implement database schema and repository"
```

---

## Phase 2B Phase Gates

### ✅ GATE 1: Database Foundation (2 hours)
**Exit Criteria**:
- [ ] Schema created
- [ ] Tables exist
- [ ] Repositories can query empty tables
- [ ] Tests pass

### ✅ GATE 2: Data Persistence (2 hours)
**Exit Criteria**:
- [ ] Can insert and query real data
- [ ] GameManager uses repositories
- [ ] Data survives application restart
- [ ] Tests pass

### ✅ GATE 3: UI Integration (1.5 hours)
**Exit Criteria**:
- [ ] Dashboard shows real data
- [ ] All data sources are DB-backed
- [ ] No more placeholder data
- [ ] Tests pass

### ✅ GATE 4: Complete Game Loop (1.5 hours)
**Exit Criteria**:
- [ ] Match completion updates standings
- [ ] League logic fully integrated
- [ ] Save/Load works
- [ ] All tests pass

---

## Documentation to Update

As you complete Phase 2B, keep these updated:

- [ ] CONTINUE_DEVELOPMENT.md - Mark tasks as complete
- [ ] CODE.md - Document new repository methods
- [ ] DATABASE_SCHEMA.md - Create detailed schema documentation
- [ ] Git commits - Make meaningful commits after each step

---

## Ready to Begin?

You have everything you need:

✅ Clean codebase from Phase 2  
✅ All infrastructure in place  
✅ Clear roadmap  
✅ Estimated timeline (6-8 hours)  
✅ Success criteria  
✅ Common issues documented  

### Next Steps:
1. Read this document completely
2. Start with Step 1: Database Schema
3. Follow the roadmap sequentially
4. Build and test after each step
5. Commit progress regularly
6. Update documentation as you go

**Good luck! Phase 2B will take FM100 from playable prototype to persistent game.** 🎮

---

**Status**: Ready to Start ✅  
**Estimated Duration**: 6-8 hours  
**Difficulty**: Moderate  
**Priority**: High  
**Next Phase**: Phase 3 (Polish & Advanced Features)  

