# FM100 Current Roadmap

## Current Verified State

- Build: `dotnet build D:\My\github\FM100\FM100.sln` passes with 0 warnings.
- Tests: `dotnet test D:\My\github\FM100\FM100.UnitTest\FM100.UnitTest.csproj --no-restore` passes, 153/153.
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
  - Transfer/contract foundation: new saves generate a transfer pool, players have wage/contract fields, the dashboard has a Transfers tab, and signings update budget, squad, bench, and autosave.
  - Contract renewal foundation: squad players have variable initial contract expiries, expiring contracts can be renewed from the Squad view, and renewals update budget, wage, morale, and save state.
  - Morale/motivation foundation: players have persistent motivation, team talks can adjust morale/motivation/stress, and motivated starters improve match-day performance.
  - Tactical depth foundation: lineups store mentality, pressing, and tempo; the Squad view can edit them; match-day performance and fatigue react to tactical choices.
  - Press/media foundation: saves store media events, the dashboard shows press questions, and responses affect fan satisfaction plus squad morale, motivation, stress, and trust.
  - Day progression foundation: the dashboard can advance a rest day; injuries, fatigue, stress, and anxiety recover; media storylines can refresh on the next day.
  - Contract expiry consequences: rest-day progression now flags expiring/expired contracts and unsettled players lose morale, motivation, and coach trust.
  - End-of-season progression foundation: completed seasons award the champion, update Hall of Fame titles, reset seasonal club/player state, resolve open media, refresh the transfer market, and generate next-season leagues/fixtures with the current clubs.
  - Promotion/relegation foundation: end-of-season progression moves bottom clubs down and top clubs up between Serie A/B/C before generating next-season fixtures.
  - Player-club season progression now follows the club's new division after promotion/relegation when selecting the next current league.
  - Multi-season history view foundation: the dashboard includes a History tab with Hall of Fame titles, media archive, and achievement archive backed by `HistoryService`.
  - Season awards foundation: completed seasons now record champion, best attack, best defense, overachiever, and player of the season awards; the History tab exposes the award archive.
  - Transfer negotiation foundation: the Transfers tab now supports discounted bids, accepted offers, counter-offers that lower asking prices, and rejected lowball bids.
  - Recurring media storylines foundation: media events now track storyline type, stage, and pressure; persistent contract, injury, poor-form, or momentum stories can continue across days and appear in dashboard/history.
  - Player development foundation: season rollover now applies reputation, potential, and market-value growth/decline from age, minutes, morale, stress, and potential ceiling; History exposes recent development records.
  - Training focus foundation: the dashboard can set Fitness, Tactical, Recovery, or Youth training; rest-day progression applies focused effects to fatigue, stress, motivation, and confidence.
  - Staff foundation: saves track Coach/Physio/Scout quality, dashboard can upgrade staff with budget, and staff quality modifies training recovery/youth effects plus scouting affordability.
  - Scouting report foundation: transfer candidates now show scout summary, risk label, estimated value, and scout accuracy driven by Scout quality.
  - Season review presentation: History now aggregates each season into a review row combining champion/awards, player development, and media counts.
  - Matchday finance foundation: home matches now generate stadium/fan-satisfaction based revenue, update budget, and record finance history.
  - Finance history presentation: History now shows recent finance records and season reviews include finance counts/totals.
  - Player performance dashboard foundation: Squad now ranks top performers by score, minutes, workload, mood, and availability risk.
  - Transfer offer options: transfer candidates now expose low/fair/asking suggested bids so negotiations have multiple player choices.
  - Staff report foundation: dashboard staff now shows average quality, grade, strength, weakness, and recommended upgrade.
  - Training report foundation: dashboard training now explains benefit and risk for the selected focus/intensity.
  - Contract report foundation: Squad now summarizes urgent renewals, total signing-fee exposure, affordability, and priority player.
  - Rich season award presentation: History classifies title, player, and club awards and orders each season by award importance.
  - Global competition simulation: playing the next fixture now simulates the same matchweek for every club in Serie A, B, and C and updates each league independently.
  - Multi-division standings: the Standings view can switch explicitly between complete Serie A, Serie B, and Serie C tables.
  - 100-season career loop: all three leagues close together, all division champions are archived, the next season starts automatically, and History exposes up to 100 season reviews.
  - Complete AI squads: every Serie A/B/C club owns a 23-player squad and lineup; all simulated matches update player minutes, fatigue, morale, and injury risk, including migrated saves.
  - Generational squad lifecycle: players age each season, veterans retire, academy prospects replace them by position, lineups rebuild, and History archives retirement/promotion events.
  - AI transfer market: AI clubs buy squad upgrades at season rollover, transfer fees update both budgets, squads rebalance to 23 players, and History archives each move.
  - Global contract lifecycle: expired deals trigger sustainable AI renewals or free-agent releases, human non-renewals are enforced, free agents remain signable, and History explains every outcome.
  - Global season finances: all clubs receive sponsorship and table prize money, pay annual wages, update budgets before market decisions, and expose comparable World Finance history.
  - Career fast-forward: dashboard can simulate the remaining global season or up to ten seasons at once, including match effects, player-club finance, annual rollover, persistence, and the season-100 stop.
  - Verified 100-season longevity: an end-to-end test runs all three divisions through 100 complete seasons and proves awards, finances, history, squads, lineups, terminal state, and bounded player-pool growth.
  - Transfer-pool lifecycle cleanup: obsolete market players are removed, free agents age and retire, and long careers no longer accumulate invisible orphan players.
  - Manager legacy: coach identity and preferences persist from creation, while seasons, matches, wins, win percentage, and titles feed the Hall of Fame.
  - Global unbeaten records: every simulated result updates active and all-time club streaks, exposed alongside manager legacy in History.
  - Individual season statistics: every starter accumulates appearances, minutes, goals, assists, and match ratings across global simulation.
  - Best individual seasons: rollover preserves each player's strongest campaign and History ranks all-time records with club, season, output, rating, and appearances.
  - Complete historical tables: every final Serie A/B/C table is snapshotted before reset with full P/W/D/L/GF/GA/GD/points data; History exposes all 300 tables across 100 seasons.
  - Core achievement engine: achievements evaluate independently of WPF after rounds and rollovers, with deduplicated season goals plus title, academy, win, unbeaten, and 10/25/50/100-season milestones.
  - Deep injury system: fatigue/age drive deterministic minor, moderate, or severe injuries; physio quality reduces duration and accelerates recovery; healthy bench players replace unavailable starters.
  - Injury archive: match timelines identify injury incidents and History records severity, initial absence, club, and recovery day across the 100-season simulation.
  - Adaptive AI tactics: every AI club chooses mentality, pressing, and tempo from relative strength, venue, form, fatigue, and tactical intelligence without overriding the human lineup.
  - Opponent tactical preview: Next Match exposes the expected AI approach and physical-load risk before kickoff.
  - Progressive scouting assignments: transfer targets begin with staff-dependent uncertainty, SCOUT assignments improve knowledge on rest days, and exact reputation/potential appear only at 100%.
  - Scouting lifecycle cleanup: completed signings and players leaving the market remove stale assignments, keeping long saves bounded.
  - Staff lifecycle: annual upkeep and triennial renewals affect the player-club budget; underfunding reduces department quality and History archives every review.
  - All-series standings overview: Serie A, Serie B, and Serie C tables are visible together, with a selectable detailed table below.
  - Training sessions: focus preserves a user-controlled 1-3 intensity, every rest-day session records squad-average fatigue/morale/confidence changes, and the dashboard exposes seasonal count plus recent outcomes.
  - Performance-driven lineup: Squad labels players Start/Rotate/Rest/Unavailable and can auto-pick a role-balanced XI from form, output, mental state, fatigue, and availability without overwriting tactical instructions.
  - Player-club career analytics: History joins all 100 final tables with seasonal finances, identifies titles/promotions/relegations, and presents career totals, best finish, movement, points, wins, goals, and net finance.
  - Complete season reports: every club archives its best performer before stat reset; the player-club timeline combines grade, year-over-year trend, record, scoring, finances, outcome, and seasonal star across all 100 years.
  - Dressing-room dynamics: team talks use trust, squad context, and repetition to calculate effectiveness; only one talk is allowed per day, cohesion is reported live, and History archives measured morale/motivation/trust impact.
  - Contextual press management: storylines recommend a response with explicit risk; pressure, response fit, and manager media reputation determine effectiveness and persistent squad, fan, reputation, and board-confidence outcomes exposed in dashboard and History.
  - Global matchday finance: every simulated Serie A/B/C home fixture generates one idempotent club-linked gate receipt in Core; player views remain filtered while 100-season world simulation verifies all revenues.
  - Complete season-review dossiers: each of the 100 player-club seasons combines final grade/result, all three division champions, seasonal star, market, injuries, achievements, media, and filtered club finance in one readable timeline.
  - Standings and simulation feedback refinement: Serie A/B/C overviews and detailed standings use theme-aware aligned DataGrid columns; play/season/decade simulation reports per-match percentage, round, division, latest score, goals, and 1/X/2 statistics live.
  - 100-year Albo d'Oro: History exposes one row per season with the Serie A, Serie B, and Serie C champion, backed by final-table archives with award fallback and verified as 100 seasons / 300 champions.
  - Pre-game 100-year world history: every new game is born with the previous 100 completed calendar seasons already generated (300 full Serie A/B/C tables, 300 champions, and historical title totals). This history is separate from manager-career statistics and does not advance or mutate playable season 1.
  - Unified person directory: every club receives persistent technical, medical, scouting, and executive personnel; the sidebar Search view finds players, staff, and executives by name, role, nationality, or club and opens a complete property sheet for the selected person.
  - 48-club competition structure: Serie A, Serie B, and Serie C each contain exactly 16 clubs; every season persists a 16-club cup for each division plus a Master Cup involving all 48 clubs, with preliminary fixtures and seeded byes.

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

## Phase 5B: Complete

- Advanced standings with goal difference, form, goals for/against. **Complete**
- Player performance dashboard. **Output scoring, workload/risk recommendations, and role-aware auto-pick XI complete**
- Player development. **Generational lifecycle and history presentation complete**
- Training focus. **Focus, intensity, staff effects, session outcomes, and dashboard history complete**
- Staff systems. **Upgrade, gameplay effects, annual contract, cost, and history flow complete**
- Scouting reports. **Progressive knowledge, assignment, reveal, and cleanup flow complete**
- Matchday finance. **Complete globally for every simulated home club with idempotent club-linked history**
- Finance history. **Global season settlement presentation complete**
- Historical statistics. **Complete pre-game 100-year world archive plus player-club career trend, records, movement, and financial analytics**
- Multi-season history views. **Complete table, award, finance, transfer, contract, manager, and player archives**
- 100-season career timeline and terminal season. **Complete: 100 pre-game historical years plus a separate playable 100-season manager career**
- Season summary reports. **Complete 100-season club reports with grade, trend, outcome, finance, and best performer**
- Season awards. **Rich presentation complete**
- Season review presentation. **Complete 100-season dossiers with club, world, player, market, medical, achievement, media, and finance context**
- Achievements. **Core career milestone system complete**

## Phase 5C: Complete

- Transfer market. **Player negotiation and AI market history complete**
- Contracts. **Global renewal, expiry, free-agent, and history flow complete**
- Injuries. **Global severity, physio recovery, replacement, and history flow complete**
- Morale/motivation as gameplay systems. **Contextual team talks, trust, cohesion, repetition, cooldown, match effects, and history complete**
- Tactical depth. **Human instructions, adaptive AI planning, match effects, and opponent preview complete**
- Press/media events. **Recurring contextual storylines, risk briefing, reputation, board confidence, scaled outcomes, and history complete**

## Recommended Immediate Next Task

Phase 5B/5C implementation is complete. Begin a stabilization phase focused on persistence integration tests, save migration coverage, UI visual QA, and packaging/release readiness.
