using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.League;
using FM100.MonoGame.Infrastructure;
using FM100.MonoGame.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FM100.MonoGame;

internal sealed class Fm100MonoGameApp : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _pixel;
    private RuntimeTextRenderer? _text;
    private readonly ClubGenerator _clubGenerator = new();
    private readonly GameManager _gameManager;
    private readonly GameProgressionService _gameProgressionService = new();
    private readonly HistoryService _historyService = new();
    private readonly PersonDirectoryService _personDirectoryService = new();

    private ScreenId _screen = ScreenId.Menu;
    private DashboardSection _dashboardSection = DashboardSection.Overview;
    private Division _selectedDivision = Division.SerieA;
    private int _selectedClubIndex;
    private int _selectedMenuIndex;
    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;
    private Dictionary<Division, List<Club>> _clubCatalog = [];
    private GameState? _gameState;
    private string _statusMessage = "MonoGame frontend ready.";
    private int _standingsDivisionIndex;
    private int _fixtureScrollIndex;
    private int _historyScrollIndex;
    private int _searchScrollIndex;

    public Fm100MonoGameApp()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 920;
        _graphics.SynchronizeWithVerticalRetrace = true;
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.Title = "FM100 MonoGame";

        _gameManager = new GameManager(new LeagueManager(), _clubGenerator, new InMemoryClubRepository());
        BuildClubCatalog();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _text = new RuntimeTextRenderer(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        if (keyboard.IsKeyDown(Keys.Escape) && WasKeyPressed(Keys.Escape, keyboard))
        {
            if (_screen == ScreenId.Menu)
            {
                Exit();
            }
            else if (_screen == ScreenId.ClubSelection)
            {
                _screen = ScreenId.Menu;
            }
        }

        switch (_screen)
        {
            case ScreenId.Menu:
                UpdateMenu(keyboard, mouse);
                break;
            case ScreenId.ClubSelection:
                UpdateClubSelection(keyboard, mouse);
                break;
            case ScreenId.Dashboard:
                UpdateDashboard(keyboard, mouse);
                break;
        }

        _previousKeyboardState = keyboard;
        _previousMouseState = mouse;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null || _pixel == null || _text == null)
        {
            return;
        }

        GraphicsDevice.Clear(new Color(16, 19, 24));
        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);

        switch (_screen)
        {
            case ScreenId.Menu:
                DrawMenu();
                break;
            case ScreenId.ClubSelection:
                DrawClubSelection();
                break;
            case ScreenId.Dashboard:
                DrawDashboard();
                break;
        }

        DrawStatusBar();
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _text?.Dispose();
        _pixel?.Dispose();
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }

    private void UpdateMenu(KeyboardState keyboard, MouseState mouse)
    {
        var options = new[] { "NEW GAME", "EXIT" };
        if (WasKeyPressed(Keys.Down, keyboard))
        {
            _selectedMenuIndex = (_selectedMenuIndex + 1) % options.Length;
        }

        if (WasKeyPressed(Keys.Up, keyboard))
        {
            _selectedMenuIndex = (_selectedMenuIndex - 1 + options.Length) % options.Length;
        }

        if (WasKeyPressed(Keys.Enter, keyboard))
        {
            ActivateMenuOption(_selectedMenuIndex);
        }

        for (var index = 0; index < options.Length; index++)
        {
            if (WasLeftClicked(GetMenuButtonBounds(index), mouse))
            {
                _selectedMenuIndex = index;
                ActivateMenuOption(index);
            }
        }
    }

    private void UpdateClubSelection(KeyboardState keyboard, MouseState mouse)
    {
        if (WasKeyPressed(Keys.Left, keyboard))
        {
            SwitchDivision(-1);
        }

        if (WasKeyPressed(Keys.Right, keyboard))
        {
            SwitchDivision(1);
        }

        var clubs = _clubCatalog[_selectedDivision];
        if (WasKeyPressed(Keys.Down, keyboard) && clubs.Count > 0)
        {
            _selectedClubIndex = Math.Min(clubs.Count - 1, _selectedClubIndex + 1);
        }

        if (WasKeyPressed(Keys.Up, keyboard) && clubs.Count > 0)
        {
            _selectedClubIndex = Math.Max(0, _selectedClubIndex - 1);
        }

        if (WasKeyPressed(Keys.Enter, keyboard) && clubs.Count > 0)
        {
            StartNewGame(clubs[_selectedClubIndex]);
        }

        foreach (var division in Enum.GetValues<Division>())
        {
            if (WasLeftClicked(GetDivisionTabBounds(division), mouse))
            {
                _selectedDivision = division;
                _selectedClubIndex = 0;
            }
        }

        for (var index = 0; index < clubs.Count; index++)
        {
            if (WasLeftClicked(GetClubRowBounds(index), mouse))
            {
                _selectedClubIndex = index;
                StartNewGame(clubs[index]);
            }
        }
    }

    private void UpdateDashboard(KeyboardState keyboard, MouseState mouse)
    {
        if (_gameState == null)
        {
            return;
        }

        if (WasKeyPressed(Keys.C, keyboard) || WasLeftClicked(GetContinueButtonBounds(), mouse))
        {
            _gameProgressionService.AdvanceDays(_gameState);
            var status = MatchPresentationService.BuildMatchdayStatus(_gameState, GetNextFixture(), GetActiveFixtures());
            _statusMessage = status.NoticeText;
        }

        if (WasKeyPressed(Keys.D1, keyboard)) _dashboardSection = DashboardSection.Overview;
        if (WasKeyPressed(Keys.D2, keyboard)) _dashboardSection = DashboardSection.Standings;
        if (WasKeyPressed(Keys.D3, keyboard)) _dashboardSection = DashboardSection.Fixtures;
        if (WasKeyPressed(Keys.D4, keyboard)) _dashboardSection = DashboardSection.History;
        if (WasKeyPressed(Keys.D5, keyboard)) _dashboardSection = DashboardSection.Search;

        foreach (var section in Enum.GetValues<DashboardSection>())
        {
            if (WasLeftClicked(GetSidebarSectionBounds(section), mouse))
            {
                _dashboardSection = section;
            }
        }

        if (_dashboardSection == DashboardSection.Standings)
        {
            if (WasKeyPressed(Keys.Left, keyboard))
            {
                _standingsDivisionIndex = (_standingsDivisionIndex + 2) % 3;
            }

            if (WasKeyPressed(Keys.Right, keyboard))
            {
                _standingsDivisionIndex = (_standingsDivisionIndex + 1) % 3;
            }
        }

        if (_dashboardSection == DashboardSection.Fixtures)
        {
            var max = Math.Max(0, BuildFixtureRows().Count - 12);
            if (WasKeyPressed(Keys.Down, keyboard)) _fixtureScrollIndex = Math.Min(max, _fixtureScrollIndex + 1);
            if (WasKeyPressed(Keys.Up, keyboard)) _fixtureScrollIndex = Math.Max(0, _fixtureScrollIndex - 1);
        }

        if (_dashboardSection == DashboardSection.History)
        {
            var max = Math.Max(0, BuildHistoryLines().Count - 14);
            if (WasKeyPressed(Keys.Down, keyboard)) _historyScrollIndex = Math.Min(max, _historyScrollIndex + 1);
            if (WasKeyPressed(Keys.Up, keyboard)) _historyScrollIndex = Math.Max(0, _historyScrollIndex - 1);
        }

        if (_dashboardSection == DashboardSection.Search)
        {
            var max = Math.Max(0, BuildSearchRows().Count - 14);
            if (WasKeyPressed(Keys.Down, keyboard)) _searchScrollIndex = Math.Min(max, _searchScrollIndex + 1);
            if (WasKeyPressed(Keys.Up, keyboard)) _searchScrollIndex = Math.Max(0, _searchScrollIndex - 1);
        }

        var nextFixture = GetNextFixture();
        var matchdayStatus = MatchPresentationService.BuildMatchdayStatus(_gameState, nextFixture, GetActiveFixtures());
        if (nextFixture != null && matchdayStatus.IsMatchDay && (WasKeyPressed(Keys.P, keyboard) || WasLeftClicked(GetPlayMatchButtonBounds(), mouse)))
        {
            PlayNextFixture();
        }
    }

    private void DrawMenu()
    {
        DrawTitle("FM100 MONOGAME");
        DrawSubtitle("New frontend shell running on MonoGame");

        DrawMenuButton("NEW GAME", GetMenuButtonBounds(0), _selectedMenuIndex == 0);
        DrawMenuButton("EXIT", GetMenuButtonBounds(1), _selectedMenuIndex == 1);
    }

    private void DrawClubSelection()
    {
        DrawTitle("CLUB SELECTION");
        DrawSubtitle("48 clubs split across Serie A, B and C");

        foreach (var division in Enum.GetValues<Division>())
        {
            var selected = division == _selectedDivision;
            DrawPanel(GetDivisionTabBounds(division), selected ? new Color(42, 111, 196) : new Color(32, 39, 50));
            _text!.DrawText(_spriteBatch!, FormatDivision(division), new Vector2(GetDivisionTabBounds(division).X + 18, GetDivisionTabBounds(division).Y + 14), Color.White, 20, true);
        }

        var clubs = _clubCatalog[_selectedDivision];
        DrawPanel(new Rectangle(90, 180, 600, 620), new Color(22, 27, 34));
        for (var index = 0; index < clubs.Count; index++)
        {
            var bounds = GetClubRowBounds(index);
            var selected = index == _selectedClubIndex;
            DrawPanel(bounds, selected ? new Color(52, 83, 141) : new Color(28, 34, 42));
            var club = clubs[index];
            _text!.DrawText(_spriteBatch!, $"{club.Name}  |  {club.City}  |  REP {club.Reputation}/20", new Vector2(bounds.X + 18, bounds.Y + 13), Color.White, 18, selected);
        }

        if (clubs.Count > 0)
        {
            var club = clubs[_selectedClubIndex];
            DrawPanel(new Rectangle(740, 180, 760, 620), new Color(22, 27, 34));
            _text!.DrawText(_spriteBatch!, club.Name, new Vector2(770, 210), Color.White, 34, true);
            _text.DrawMultilineText(
                _spriteBatch!,
                [
                    $"Division: {FormatDivision(club.Division)}",
                    $"City: {club.City}",
                    $"Stadium: {club.Stadium.Name}",
                    $"Budget: EUR {club.BudgetInMillions}M",
                    $"Reputation: {club.Reputation}/20",
                    "",
                    "Enter or click to start a new career.",
                    "Use Left/Right to switch division, Up/Down to move."
                ],
                new Vector2(770, 280),
                new Color(220, 226, 236),
                22,
                12);
        }
    }

    private void DrawDashboard()
    {
        if (_gameState == null)
        {
            return;
        }

        var playerClub = _gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return;
        }

        var currentDate = MatchPresentationService.GetCurrentGameDate(_gameState);
        var nextFixture = GetNextFixture();
        var matchdayStatus = MatchPresentationService.BuildMatchdayStatus(_gameState, nextFixture, GetActiveFixtures());

        DrawSidebar(playerClub, currentDate, matchdayStatus);
        DrawDashboardHeader(playerClub, matchdayStatus);

        switch (_dashboardSection)
        {
            case DashboardSection.Overview:
                DrawOverview(playerClub, nextFixture, matchdayStatus);
                break;
            case DashboardSection.Standings:
                DrawStandingsSection(playerClub);
                break;
            case DashboardSection.Fixtures:
                DrawFixturesSection();
                break;
            case DashboardSection.History:
                DrawHistorySection();
                break;
            case DashboardSection.Search:
                DrawSearchSection();
                break;
        }
    }

    private void DrawSidebar(Club playerClub, DateTime currentDate, MatchdayStatus matchdayStatus)
    {
        DrawPanel(new Rectangle(0, 0, 240, GraphicsDevice.Viewport.Height), new Color(18, 22, 28));
        _text!.DrawText(_spriteBatch!, "FM100", new Vector2(30, 28), Color.White, 30, true);
        _text.DrawText(_spriteBatch!, currentDate.ToString("dd/MM/yyyy"), new Vector2(30, 74), matchdayStatus.IsMatchDay ? new Color(94, 203, 144) : new Color(109, 158, 235), 24, true);
        _text.DrawMultilineText(
            _spriteBatch!,
            [
                playerClub.Name,
                FormatDivision(playerClub.Division),
                "",
                "1 Overview",
                "2 Standings",
                "3 Fixtures",
                "4 History",
                "5 Search",
                "",
                "C: continue day",
                "P: play match",
                "ESC: close"
            ],
            new Vector2(30, 132),
            new Color(220, 226, 236),
            16,
            10);

        foreach (var section in Enum.GetValues<DashboardSection>())
        {
            var selected = section == _dashboardSection;
            DrawPanel(GetSidebarSectionBounds(section), selected ? new Color(42, 111, 196) : new Color(24, 29, 36));
            _text.DrawText(_spriteBatch!, section.ToString().ToUpperInvariant(), new Vector2(GetSidebarSectionBounds(section).X + 14, GetSidebarSectionBounds(section).Y + 10), Color.White, 16, selected);
        }
    }

    private void DrawStatusBar()
    {
        DrawPanel(new Rectangle(0, GraphicsDevice.Viewport.Height - 36, GraphicsDevice.Viewport.Width, 36), new Color(10, 12, 16));
        _text!.DrawText(_spriteBatch!, _statusMessage, new Vector2(16, GraphicsDevice.Viewport.Height - 28), new Color(196, 202, 212), 14);
    }

    private void DrawTitle(string text) =>
        _text!.DrawText(_spriteBatch!, text, new Vector2(90, 70), Color.White, 44, true);

    private void DrawSubtitle(string text) =>
        _text!.DrawText(_spriteBatch!, text, new Vector2(92, 130), new Color(160, 170, 184), 20);

    private void DrawMenuButton(string label, Rectangle bounds, bool selected)
    {
        DrawPanel(bounds, selected ? new Color(42, 111, 196) : new Color(32, 39, 50));
        _text!.DrawText(_spriteBatch!, label, new Vector2(bounds.X + 28, bounds.Y + 18), Color.White, 24, true);
    }

    private void DrawButton(Rectangle bounds, string label, Color background)
    {
        DrawPanel(bounds, background);
        _text!.DrawText(_spriteBatch!, label, new Vector2(bounds.X + 18, bounds.Y + 14), Color.White, 20, true);
    }

    private void DrawPanel(Rectangle bounds, Color color)
    {
        _spriteBatch!.Draw(_pixel!, bounds, color);
        _spriteBatch.Draw(_pixel!, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), new Color(65, 74, 88));
        _spriteBatch.Draw(_pixel!, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), new Color(65, 74, 88));
        _spriteBatch.Draw(_pixel!, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), new Color(65, 74, 88));
        _spriteBatch.Draw(_pixel!, new Rectangle(bounds.Right - 1, bounds.Y, 1, bounds.Height), new Color(65, 74, 88));
    }

    private void ActivateMenuOption(int index)
    {
        if (index == 0)
        {
            _screen = ScreenId.ClubSelection;
            _statusMessage = "Choose a club to start the MonoGame career flow.";
            return;
        }

        Exit();
    }

    private void StartNewGame(Club club)
    {
        _gameState = _gameManager.StartNewGameAsync(
            club.Name,
            club.Division,
            difficulty: 5,
            managerName: "MonoGame Manager",
            managerNationality: "Italian",
            preferredFormation: "4-3-3",
            managerPersonality: "Balanced").GetAwaiter().GetResult();
        _personDirectoryService.EnsureDirectory(_gameState);
        _screen = ScreenId.Dashboard;
        _dashboardSection = DashboardSection.Overview;
        _statusMessage = $"Career started with {club.Name}.";
    }

    private void PlayNextFixture()
    {
        if (_gameState == null)
        {
            return;
        }

        var nextFixture = GetNextFixture();
        if (nextFixture == null)
        {
            return;
        }

        var homeClub = _gameState.Clubs[nextFixture.HomeClubId];
        var awayClub = _gameState.Clubs[nextFixture.AwayClubId];
        var simulator = new MatchSimulator();
        var match = simulator.SimulateMatchAsync(homeClub, awayClub, homeClub.Reputation, awayClub.Reputation).GetAwaiter().GetResult();
        match.FixtureId = nextFixture.Id;
        match.HomeClubId = homeClub.Id;
        match.AwayClubId = awayClub.Id;
        match.Status = MatchStatus.Completed;
        match.PlayedAt = DateTime.UtcNow;
        nextFixture.IsPlayed = true;
        nextFixture.MatchId = match.Id;
        _gameState.Matches[match.Id] = match;
        homeClub.SeasonWins += match.HomeGoals > match.AwayGoals ? 1 : 0;
        homeClub.SeasonDraws += match.HomeGoals == match.AwayGoals ? 1 : 0;
        homeClub.SeasonLosses += match.HomeGoals < match.AwayGoals ? 1 : 0;
        awayClub.SeasonWins += match.AwayGoals > match.HomeGoals ? 1 : 0;
        awayClub.SeasonDraws += match.HomeGoals == match.AwayGoals ? 1 : 0;
        awayClub.SeasonLosses += match.AwayGoals < match.HomeGoals ? 1 : 0;
        homeClub.GoalsFor += match.HomeGoals;
        homeClub.GoalsAgainst += match.AwayGoals;
        awayClub.GoalsFor += match.AwayGoals;
        awayClub.GoalsAgainst += match.HomeGoals;
        _statusMessage = $"{homeClub.Name} {match.HomeGoals}-{match.AwayGoals} {awayClub.Name}";
    }

    private Fixture? GetNextFixture()
    {
        if (_gameState?.GetCurrentLeague() is not { } league)
        {
            return null;
        }

        return league.FixtureIds
            .Select(id => _gameState.Fixtures.GetValueOrDefault(id))
            .Where(fixture => fixture != null
                && !fixture.IsPlayed
                && (fixture.HomeClubId == _gameState.PlayerClubId || fixture.AwayClubId == _gameState.PlayerClubId))
            .OrderBy(fixture => fixture!.ScheduledDate)
            .FirstOrDefault();
    }

    private List<Fixture> GetActiveFixtures()
    {
        if (_gameState == null)
        {
            return [];
        }

        return _gameState.Leagues.Values
            .Where(league => league.Season == _gameState.CurrentSeason)
            .SelectMany(league => league.FixtureIds)
            .Select(id => _gameState.Fixtures.GetValueOrDefault(id))
            .Where(fixture => fixture != null)
            .Select(fixture => fixture!)
            .ToList();
    }

    private List<StandingRow> BuildStandings(Division division)
    {
        if (_gameState == null)
        {
            return [];
        }

        return _gameState.Clubs.Values
            .Where(club => club.Division == division)
            .OrderByDescending(club => club.GetPoints())
            .ThenByDescending(club => club.GetGoalDifference())
            .ThenByDescending(club => club.GoalsFor)
            .ThenBy(club => club.Name)
            .Select((club, index) => new StandingRow(club.Id, index + 1, club.Name, club.GetPoints(), club.GetGoalDifference()))
            .ToList();
    }

    private List<string> BuildFixtureRows()
    {
        if (_gameState == null)
        {
            return [];
        }

        var upcoming = GetActiveFixtures()
            .Where(fixture => !fixture.IsPlayed)
            .OrderBy(fixture => fixture.ScheduledDate)
            .Take(12)
            .Select(fixture => $"UP {fixture.ScheduledDate.ToLocalTime():dd/MM} | W{fixture.MatchWeek} | {GetClubName(fixture.HomeClubId)} vs {GetClubName(fixture.AwayClubId)}");
        var results = _gameState.Matches.Values
            .OrderByDescending(match => match.PlayedAt)
            .Take(16)
            .Select(match => $"FT {match.PlayedAt.ToLocalTime():dd/MM} | {GetClubName(match.HomeClubId)} {match.HomeGoals}-{match.AwayGoals} {GetClubName(match.AwayClubId)}");
        return upcoming.Concat([""]).Concat(results).ToList();
    }

    private List<string> BuildHistoryLines()
    {
        if (_gameState == null)
        {
            return [];
        }

        var lines = new List<string>();
        lines.AddRange(_historyService.GetRollOfHonour(_gameState).Take(10)
            .Select(entry => $"{entry.Season}: Serie A {entry.SerieAChampion} | Serie B {entry.SerieBChampion} | Serie C {entry.SerieCChampion}"));
        lines.Add(string.Empty);
        lines.AddRange(_historyService.GetCupRollOfHonour(_gameState).Take(8)
            .Select(entry => $"{entry.Season}: A Cup {entry.SerieACupWinner} | B Cup {entry.SerieBCupWinner} | C Cup {entry.SerieCCupWinner} | Master {entry.MasterCupWinner}"));
        return lines;
    }

    private List<string> BuildSearchRows()
    {
        if (_gameState == null)
        {
            return [];
        }

        return _personDirectoryService.Search(_gameState, category: PersonCategory.All, take: 40)
            .Select(entry => $"{entry.PersonType,-9} | {entry.FullName,-24} | {entry.Role,-20} | {entry.ClubName,-18} | {entry.Reputation,2}/20")
            .ToList();
    }

    private string GetClubName(Guid clubId) =>
        _gameState?.Clubs.GetValueOrDefault(clubId)?.Name ?? "Unknown Club";

    private void BuildClubCatalog()
    {
        _clubCatalog = Enum.GetValues<Division>()
            .ToDictionary(
                division => division,
                division => _clubGenerator.GenerateClubsForDivision(division)
                    .OrderBy(club => club.Name)
                    .ToList());
    }

    private void SwitchDivision(int delta)
    {
        var divisions = Enum.GetValues<Division>();
        var currentIndex = Array.IndexOf(divisions, _selectedDivision);
        var nextIndex = (currentIndex + delta + divisions.Length) % divisions.Length;
        _selectedDivision = divisions[nextIndex];
        _selectedClubIndex = 0;
    }

    private Rectangle GetMenuButtonBounds(int index) => new(90, 230 + index * 96, 360, 72);

    private Rectangle GetSidebarSectionBounds(DashboardSection section) => new(24, 344 + (int)section * 52, 192, 40);

    private Rectangle GetDivisionTabBounds(Division division)
    {
        var index = (int)division;
        return new Rectangle(90 + index * 190, 180, 170, 56);
    }

    private Rectangle GetClubRowBounds(int index) => new(110, 208 + index * 34, 560, 28);

    private Rectangle GetContinueButtonBounds() => new(316, 470, 240, 56);

    private Rectangle GetPlayMatchButtonBounds() => new(590, 470, 240, 56);

    private bool WasKeyPressed(Keys key, KeyboardState currentState) =>
        currentState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);

    private bool WasLeftClicked(Rectangle bounds, MouseState mouse) =>
        bounds.Contains(mouse.Position) &&
        mouse.LeftButton == ButtonState.Pressed &&
        _previousMouseState.LeftButton == ButtonState.Released;

    private static string FormatDivision(Division division) => division switch
    {
        Division.SerieA => "Serie A",
        Division.SerieB => "Serie B",
        _ => "Serie C"
    };

    private enum ScreenId
    {
        Menu,
        ClubSelection,
        Dashboard
    }

    private enum DashboardSection
    {
        Overview,
        Standings,
        Fixtures,
        History,
        Search
    }

    private sealed record StandingRow(Guid ClubId, int Position, string Name, int Points, int GoalDifference);

    private void DrawDashboardHeader(Club playerClub, MatchdayStatus matchdayStatus)
    {
        DrawPanel(new Rectangle(280, 48, 1280, 140), new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, $"{playerClub.Name}  |  Season {_gameState!.CurrentSeason}", new Vector2(320, 80), Color.White, 30, true);
        _text.DrawText(_spriteBatch!, matchdayStatus.NoticeText, new Vector2(320, 128), matchdayStatus.IsMatchDay ? new Color(94, 203, 144) : new Color(109, 158, 235), 22, true);
    }

    private void DrawOverview(Club playerClub, Fixture? nextFixture, MatchdayStatus matchdayStatus)
    {
        DrawPanel(new Rectangle(280, 220, 620, 620), new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, "NEXT MATCH", new Vector2(316, 252), Color.White, 24, true);
        _text.DrawMultilineText(
            _spriteBatch!,
            [
                $"Record: {playerClub.SeasonWins}-{playerClub.SeasonDraws}-{playerClub.SeasonLosses}",
                $"Points: {playerClub.GetPoints()}",
                $"Goal Difference: {playerClub.GetGoalDifference()}",
                $"Budget: EUR {playerClub.BudgetInMillions}M",
                ""
            ],
            new Vector2(316, 300),
            new Color(220, 226, 236),
            18,
            8);

        if (nextFixture != null)
        {
            var homeName = GetClubName(nextFixture.HomeClubId);
            var awayName = GetClubName(nextFixture.AwayClubId);
            _text.DrawMultilineText(
                _spriteBatch!,
                [
                    $"{homeName} vs {awayName}",
                    $"Week {nextFixture.MatchWeek}",
                    nextFixture.ScheduledDate.ToLocalTime().ToString("dd/MM/yyyy"),
                    matchdayStatus.IsMatchDay ? "Ready to play now" : $"Available in {matchdayStatus.DaysUntilNextFixture} day(s)"
                ],
                new Vector2(316, 430),
                Color.White,
                22,
                12);
        }

        DrawButton(GetContinueButtonBounds(), matchdayStatus.ContinueLabel, matchdayStatus.IsMatchDay ? new Color(42, 111, 196) : new Color(44, 145, 94));
        DrawButton(GetPlayMatchButtonBounds(), "PLAY MATCH", matchdayStatus.IsMatchDay ? new Color(44, 145, 94) : new Color(70, 74, 82));

        DrawPanel(new Rectangle(940, 220, 620, 290), new Color(22, 27, 34));
        _text.DrawText(_spriteBatch!, "STANDINGS SNAPSHOT", new Vector2(976, 252), Color.White, 24, true);
        var standings = BuildStandings(playerClub.Division).Take(10).ToList();
        for (var index = 0; index < standings.Count; index++)
        {
            var row = standings[index];
            var y = 300 + index * 22;
            var highlight = row.ClubId == playerClub.Id ? new Color(94, 203, 144) : Color.White;
            _text.DrawText(_spriteBatch!, $"{row.Position,2}. {row.Name,-20} {row.Points,3} pts  GD {row.GoalDifference,3}", new Vector2(976, y), highlight, 18, row.ClubId == playerClub.Id);
        }

        DrawPanel(new Rectangle(940, 540, 620, 300), new Color(22, 27, 34));
        _text.DrawText(_spriteBatch!, "MATCH COMMENTARY", new Vector2(976, 572), Color.White, 24, true);
        var lines = GetLatestCommentaryLines();
        _text.DrawMultilineText(_spriteBatch!, lines, new Vector2(976, 620), new Color(220, 226, 236), 17, 10);
    }

    private void DrawStandingsSection(Club playerClub)
    {
        var division = (Division)_standingsDivisionIndex;
        DrawPanel(new Rectangle(280, 220, 1280, 620), new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, $"FULL STANDINGS - {FormatDivision(division)}", new Vector2(316, 252), Color.White, 24, true);
        _text.DrawText(_spriteBatch!, "Left/Right to switch division", new Vector2(1230, 252), new Color(160, 170, 184), 16);
        var standings = BuildStandings(division);
        for (var index = 0; index < standings.Count; index++)
        {
            var row = standings[index];
            var y = 300 + index * 28;
            var highlight = row.ClubId == playerClub.Id ? new Color(94, 203, 144) : Color.White;
            _text.DrawText(_spriteBatch!, $"{row.Position,2}. {row.Name,-22} {row.Points,3} pts  GD {row.GoalDifference,3}", new Vector2(316, y), highlight, 20, row.ClubId == playerClub.Id);
        }
    }

    private void DrawFixturesSection()
    {
        DrawPanel(new Rectangle(280, 220, 1280, 620), new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, "FIXTURES AND RESULTS", new Vector2(316, 252), Color.White, 24, true);
        _text.DrawText(_spriteBatch!, "Up/Down to scroll", new Vector2(1370, 252), new Color(160, 170, 184), 16);
        var rows = BuildFixtureRows().Skip(_fixtureScrollIndex).Take(18).ToList();
        for (var index = 0; index < rows.Count; index++)
        {
            _text.DrawText(_spriteBatch!, rows[index], new Vector2(316, 302 + index * 26), Color.White, 18);
        }
    }

    private void DrawHistorySection()
    {
        DrawPanel(new Rectangle(280, 220, 1280, 620), new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, "100-YEAR HISTORY", new Vector2(316, 252), Color.White, 24, true);
        _text.DrawText(_spriteBatch!, "Up/Down to scroll", new Vector2(1370, 252), new Color(160, 170, 184), 16);
        var lines = BuildHistoryLines().Skip(_historyScrollIndex).Take(18).ToList();
        _text.DrawMultilineText(_spriteBatch!, lines, new Vector2(316, 300), new Color(220, 226, 236), 18, 12);
    }

    private void DrawSearchSection()
    {
        DrawPanel(new Rectangle(280, 220, 1280, 620), new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, "PERSON SEARCH", new Vector2(316, 252), Color.White, 24, true);
        _text.DrawText(_spriteBatch!, "Directory preview - Up/Down to scroll", new Vector2(1180, 252), new Color(160, 170, 184), 16);
        var rows = BuildSearchRows().Skip(_searchScrollIndex).Take(18).ToList();
        for (var index = 0; index < rows.Count; index++)
        {
            _text.DrawText(_spriteBatch!, rows[index], new Vector2(316, 302 + index * 26), Color.White, 16);
        }
    }

    private IReadOnlyList<string> GetLatestCommentaryLines()
    {
        if (_gameState == null)
        {
            return ["No commentary available yet."];
        }

        var latest = _gameState.Matches.Values
            .Where(match => match.HomeClubId == _gameState.PlayerClubId || match.AwayClubId == _gameState.PlayerClubId)
            .OrderByDescending(match => match.PlayedAt)
            .FirstOrDefault();
        if (latest == null)
        {
            var nextFixture = GetNextFixture();
            return nextFixture == null
                ? ["No commentary available yet."]
                : [$"Commentary room ready for {GetClubName(nextFixture.HomeClubId)} vs {GetClubName(nextFixture.AwayClubId)}."];
        }

        return MatchPresentationService.BuildCommentary(latest, _gameState.Clubs).Take(10).ToList();
    }
}
