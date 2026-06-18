# FM100 Current Roadmap

## Current Verified State

- Build: `dotnet build D:\My\github\FM100\FM100.sln` passes with 0 warnings.
- Tests: `dotnet test D:\My\github\FM100\FM100.UnitTest\FM100.UnitTest.csproj --no-restore` passes, 39/39.
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

- Add durable match event persistence.
- Add durable match statistics persistence.
- Ensure played fixtures and generated matches can be reloaded with score and details.
- Add repository interfaces in `FM100.Core/Repositories`.
- Add implementations in `FM100/Data/Repositories`.
- Add schema to `DatabaseInitializer`.
- Add tests for repository serialization/deserialization where possible.

### 5A-2 Match Viewer

- Add a match result/detail view.
- For normal historical matches, show final score and statistics.
- For coached/player matches, show play-by-play events.
- Wire "View Details" from recent results.

### 5A-3 Squad And Lineup Foundation

- Build a squad roster view first.
- Add formation selection.
- Persist selected formation/lineup.
- Drag-and-drop lineup editor can follow after the basic model is stable.

### 5A-4 Dashboard Refinement

- Move toward tabbed or sectioned dashboard: Game, Squad, Standings, Results.
- Show next player fixture clearly on the dashboard.
- Keep all content responsive to full-window layout.

## Phase 5B: Later

- Advanced standings with goal difference, form, goals for/against.
- Player performance dashboard.
- Historical statistics.
- Season summary reports.
- Achievements.

## Phase 5C: Later

- Transfer market.
- Contracts.
- Injuries.
- Morale/motivation as gameplay systems.
- Tactical depth.
- Press/media events.

## Recommended Immediate Next Task

Start `5A-1 Match Data Persistence`.

Reason:

- The UI can already simulate matches.
- The next blocker is durable match history: events, statistics, and detail views need stored data.
- Once this is stable, Match Viewer and Squad/Lineup work can build on reliable persistence instead of temporary in-memory state.
