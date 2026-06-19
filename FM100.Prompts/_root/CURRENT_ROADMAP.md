# FM100 Current Roadmap

## Current Verified State

- Build: `dotnet build D:\My\github\FM100\FM100.sln` passes with 0 warnings.
- Tests: `dotnet test D:\My\github\FM100\FM100.UnitTest\FM100.UnitTest.csproj --no-restore` passes, 47/47.
- NuGet vulnerability scan is clean.
- Main flow is active: Splash -> Menu -> Club Selection -> Coach Creation -> Game Dashboard.
- Implemented recently:
  - Real club-linked league fixture generation.
  - Play next match from dashboard.
  - Match result updates club records, standings, fixtures, and results.
  - Full `GameState` save/load alignment with current SQLite schema.
  - Light/Dark theme selection with persisted user preference.
  - In-memory caching for clubs and fixtures.
  - Main views stretch to the available application window.
  - Phase 5A-1 foundation: match events and match statistics persistence repositories.
  - Match simulator event descriptions now identify home/away team ownership for persisted card events.
  - Phase 5A-3 foundation: new games generate a 23-player squad for the selected club and the dashboard shows a roster/formation view.
  - Lineup foundation: new games generate a starting XI and bench for the selected club.
  - Lineup editing foundation: roster players can be moved between starting XI and bench from the Squad tab.
  - Drag-and-drop lineup editing works between Starting XI and Bench, alongside START/BENCH buttons.
  - Match simulation now considers starting XI reputation, morale, and fatigue when calculating team performance.
  - Player position foundation: squads now include goalkeepers, defenders, midfielders, and forwards; default lineups respect formation shape.
  - Dashboard next-match preview shows upcoming fixture and projected strength.
  - Played matches update starter fatigue, morale, confidence, happiness, and minutes played.
  - Match-day performance/effects logic moved from WPF code-behind into Core `MatchDayService` with tests.
  - Dashboard now shows recent results with direct access to match details.
  - Advanced standings show P, W-D-L, GF, GA, GD, and recent form.
  - Squad view includes a player status dashboard for minutes, fatigue risk, and low morale.
  - Dashboard includes a season snapshot with played, remaining, win rate, and form.
  - Dashboard includes basic season achievements.
  - Achievements are stored in `GameState` and survive save serialization.
  - Dashboard includes historical stats for matches, goals, points per match, and clean sheets.
  - Season report calculations moved into Core `SeasonReportService` with tests.
  - Injury foundation: players can become unavailable from high fatigue, lineups avoid injured starters when possible, and the Squad view shows injury status.

## Documentation Reconciliation

The project contains many historical completion reports. Several documents claim "production ready" or "fully complete",
but they describe prior intended milestones rather than the current source state. Treat them as design intent and context,
not as authoritative status.

Most useful planning documents:

- `MASTER_PLAN.md`: long-term game feature map.
- `FM100/PHASE_5A_DETAILED_PLAN.md`: most relevant next-phase plan.
- `FM100.Prompts/DEVELOPMENT_STANDARDS.md`: coding and architecture standards.
- `FM100.Prompts/UI_ARCHITECTURE.md`: original UI navigation model.
- `FM100.Prompts/UI_VISUAL_REFERENCE.md`: intended UI shape and layout references.
- `FM100.Prompts/FM100.ARCHITECTURE.md`: emotional/performance system architecture.
- `FM100/Data/PERSISTENCE_QUICK_REFERENCE.md`: persistence usage patterns.

Important mismatch:

- Older UI docs describe `CoachCustomizationView` and `GameView` as the main flow.
- Current app flow uses `ClubSelectionView`, `CoachCreationView`, and `GameDashboardView`.
- Future work should evolve the current flow rather than resurrecting older placeholder views unless there is a clear reason.

## Architecture Rules To Keep

- `FM100.Domain`: data models only.
- `FM100.Core`: business/game logic.
- `FM100.Data`: repositories, SQLite, persistence concerns.
- `FM100`: WPF UI and view orchestration.
- Prefer interfaces and DI.
- Keep tests focused and add tests for contract-level bugs.
- Avoid hardcoded UI colors; use theme resources.
- Avoid fixed root view dimensions; views should stretch inside `MainWindow`.

## Phase 5A: Next Active Development

Primary goal: make the game loop feel playable beyond "simulate next match".

### 5A-1 Match Data Persistence

- Add durable match event persistence. **Complete**
- Add durable match statistics persistence. **Complete**
- Ensure played fixtures and generated matches can be reloaded with score and details. **Foundation complete**
- Add repository interfaces in `FM100.Core/Repositories`. **Complete**
- Add implementations in `FM100/Data/Repositories`. **Complete**
- Add schema to `DatabaseInitializer`. **Complete**
- Add tests for match event attribution behavior. **Complete**
- Add repository serialization/deserialization tests later if the test project is expanded to cover the WPF/Data assembly.

### 5A-2 Match Viewer

- Add a match result/detail view. **Complete**
- For normal historical matches, show final score and statistics. **Complete**
- For coached/player matches, show play-by-play events. **Complete**
- Wire "View Details" from recent results. **Complete**

### 5A-3 Squad And Lineup Foundation

- Build a squad roster view first. **Complete**
- Add formation selection. **Complete**
- Persist selected formation/lineup. **Formation and default lineup persisted in GameState save**
- Add basic lineup editing controls. **Complete**
- Use selected lineup in match simulation. **Complete**
- Add player positions and role-aware default lineup selection. **Complete**
- Move match-day lineup effects into Core service with tests. **Complete**
- Drag-and-drop lineup editor. **Complete**

### 5A-4 Dashboard Refinement

- Move toward tabbed or sectioned dashboard: Game, Squad, Standings, Results. **Complete**
- Show next player fixture clearly on the dashboard. **Complete**
- Show recent results directly on the dashboard. **Complete**
- Keep all content responsive to full-window layout.

## Phase 5B: Later

- Advanced standings with goal difference, form, goals for/against. **Complete**
- Player performance dashboard. **Started**
- Historical statistics. **Foundation complete**
- Season summary reports. **Foundation complete**
- Achievements. **Foundation complete**

## Phase 5C: Later

- Transfer market.
- Contracts.
- Injuries. **Foundation complete**
- Morale/motivation as gameplay systems.
- Tactical depth.
- Press/media events.

## Recommended Immediate Next Task

Continue `5B/5C` feature work.

Reason:

- Phase 5A is complete and verified.
- Continue expanding 5B/5C: fuller season reports, more persistent achievements, and early transfer/contract systems.
