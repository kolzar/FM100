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

public sealed class Fm100MonoGameApp : Game
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
    private Guid? _selectedSearchPersonId;
    private readonly TableState _standingsTableState = new();
    private readonly TableState _fixturesTableState = new();
    private readonly TableState _historyLeagueTableState = new();
    private readonly TableState _historyCupTableState = new();
    private readonly TableState _searchTableState = new();
    private readonly TableState _searchDetailTableState = new();

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

            foreach (var division in Enum.GetValues<Division>())
            {
                if (WasLeftClicked(GetStandingsDivisionButtonBounds(division), mouse))
                {
                    _standingsDivisionIndex = (int)division;
                }
            }

            HandleTableSort(GetStandingsTableBounds(), GetStandingsColumns(), _standingsTableState, mouse);
        }

        if (_dashboardSection == DashboardSection.Fixtures)
        {
            var max = Math.Max(0, BuildFixtureTableRows().Count - 14);
            if (WasKeyPressed(Keys.Down, keyboard)) _fixtureScrollIndex = Math.Min(max, _fixtureScrollIndex + 1);
            if (WasKeyPressed(Keys.Up, keyboard)) _fixtureScrollIndex = Math.Max(0, _fixtureScrollIndex - 1);
            HandleTableSort(GetFixturesTableBounds(), GetFixtureColumns(), _fixturesTableState, mouse);
        }

        if (_dashboardSection == DashboardSection.History)
        {
            var max = Math.Max(0, BuildHistoryCupRows().Count - 6);
            if (WasKeyPressed(Keys.Down, keyboard)) _historyScrollIndex = Math.Min(max, _historyScrollIndex + 1);
            if (WasKeyPressed(Keys.Up, keyboard)) _historyScrollIndex = Math.Max(0, _historyScrollIndex - 1);
            HandleTableSort(GetHistoryLeagueTableBounds(), GetHistoryLeagueColumns(), _historyLeagueTableState, mouse);
            HandleTableSort(GetHistoryCupTableBounds(), GetHistoryCupColumns(), _historyCupTableState, mouse);
        }

        if (_dashboardSection == DashboardSection.Search)
        {
            var rows = ApplySort(BuildSearchTableRows(), GetSearchColumns(), _searchTableState);
            var max = Math.Max(0, rows.Count - 10);
            if (WasKeyPressed(Keys.Down, keyboard)) _searchScrollIndex = Math.Min(max, _searchScrollIndex + 1);
            if (WasKeyPressed(Keys.Up, keyboard)) _searchScrollIndex = Math.Max(0, _searchScrollIndex - 1);
            HandleTableSort(GetSearchTableBounds(), GetSearchColumns(), _searchTableState, mouse);
            HandleTableSort(GetSearchDetailTableBounds(), GetSearchDetailColumns(), _searchDetailTableState, mouse);
            HandleSearchSelection(rows, mouse);
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
        _standingsDivisionIndex = (int)club.Division;
        _fixtureScrollIndex = 0;
        _historyScrollIndex = 0;
        _searchScrollIndex = 0;
        _selectedSearchPersonId = null;
        ResetTableState(_standingsTableState);
        ResetTableState(_fixturesTableState);
        ResetTableState(_historyLeagueTableState);
        ResetTableState(_historyCupTableState);
        ResetTableState(_searchTableState);
        ResetTableState(_searchDetailTableState);
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
            .Select((club, index) => new StandingRow(
                club.Id,
                index + 1,
                club.Name,
                club.GetPoints(),
                club.GetMatchesPlayed(),
                club.SeasonWins,
                club.SeasonDraws,
                club.SeasonLosses,
                club.GoalsFor,
                club.GoalsAgainst,
                club.GetGoalDifference(),
                club.GetGoalDifference() > 0 ? $"+{club.GetGoalDifference()}" : club.GetGoalDifference().ToString(),
                BuildClubForm(club)))
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

    private Rectangle GetSidebarBounds() => new(0, 0, 240, GraphicsDevice.Viewport.Height);

    private Rectangle GetContentBounds()
    {
        var sidebar = GetSidebarBounds();
        var left = sidebar.Right + 40;
        var top = 48;
        var rightMargin = 40;
        var bottomMargin = 80;
        return new Rectangle(
            left,
            top,
            Math.Max(960, GraphicsDevice.Viewport.Width - left - rightMargin),
            Math.Max(720, GraphicsDevice.Viewport.Height - top - bottomMargin));
    }

    private Rectangle GetHeaderBounds()
    {
        var content = GetContentBounds();
        return new Rectangle(content.X, content.Y, content.Width, 140);
    }

    private Rectangle GetSectionBounds()
    {
        var content = GetContentBounds();
        var header = GetHeaderBounds();
        return new Rectangle(content.X, header.Bottom + 32, content.Width, Math.Max(420, content.Bottom - header.Bottom - 32));
    }

    private Rectangle GetSidebarSectionBounds(DashboardSection section)
    {
        var sidebar = GetSidebarBounds();
        return new Rectangle(sidebar.X + 24, sidebar.Y + 344 + (int)section * 52, sidebar.Width - 48, 40);
    }

    private Rectangle GetDivisionTabBounds(Division division)
    {
        var index = (int)division;
        return new Rectangle(90 + index * 190, 180, 170, 56);
    }

    private Rectangle GetClubRowBounds(int index) => new(110, 208 + index * 34, 560, 28);

    private Rectangle GetStandingsDivisionButtonBounds(Division division)
    {
        var section = GetSectionBounds();
        var width = 140;
        var gap = 12;
        var index = (int)division;
        return new Rectangle(section.Right - ((3 - index) * (width + gap)) + gap, section.Y + 26, width, 36);
    }

    private Rectangle GetStandingsOverviewAreaBounds()
    {
        var section = GetSectionBounds();
        return new Rectangle(section.X + 36, section.Y + 84, section.Width - 72, 250);
    }

    private Rectangle GetStandingsOverviewPanelBounds(Division division)
    {
        var area = GetStandingsOverviewAreaBounds();
        var gap = 16;
        var width = (area.Width - (gap * 2)) / 3;
        return new Rectangle(area.X + (int)division * (width + gap), area.Y, width, area.Height);
    }

    private Rectangle GetStandingsTableBounds()
    {
        var section = GetSectionBounds();
        var overview = GetStandingsOverviewAreaBounds();
        return new Rectangle(section.X + 36, overview.Bottom + 64, section.Width - 72, section.Bottom - overview.Bottom - 100);
    }

    private Rectangle GetFixturesTableBounds()
    {
        var section = GetSectionBounds();
        return new Rectangle(section.X + 36, section.Y + 72, section.Width - 72, section.Height - 108);
    }

    private Rectangle GetHistoryLeagueTableBounds()
    {
        var section = GetSectionBounds();
        var gap = 24;
        var availableHeight = section.Height - 108;
        var tableHeight = Math.Max(180, (availableHeight - gap) / 2);
        return new Rectangle(section.X + 36, section.Y + 72, section.Width - 72, tableHeight);
    }

    private Rectangle GetHistoryCupTableBounds()
    {
        var topTable = GetHistoryLeagueTableBounds();
        var section = GetSectionBounds();
        var y = topTable.Bottom + 24;
        var height = Math.Max(180, section.Bottom - y - 36);
        return new Rectangle(section.X + 36, y, section.Width - 72, height);
    }

    private Rectangle GetSearchTableBounds()
    {
        var section = GetSectionBounds();
        return new Rectangle(section.X + 36, section.Y + 72, section.Width - 72, 320);
    }

    private Rectangle GetSearchDetailHeaderBounds()
    {
        var table = GetSearchTableBounds();
        return new Rectangle(table.X, table.Bottom + 18, table.Width, 56);
    }

    private Rectangle GetSearchDetailTableBounds()
    {
        var header = GetSearchDetailHeaderBounds();
        var section = GetSectionBounds();
        return new Rectangle(header.X, header.Bottom + 12, header.Width, Math.Max(180, section.Bottom - header.Bottom - 24));
    }

    private Rectangle GetOverviewLeftBounds()
    {
        var section = GetSectionBounds();
        var gap = 32;
        var leftWidth = Math.Max(420, (section.Width - gap) / 2);
        return new Rectangle(section.X, section.Y, leftWidth, section.Height);
    }

    private Rectangle GetOverviewRightTopBounds()
    {
        var left = GetOverviewLeftBounds();
        var section = GetSectionBounds();
        var rightX = left.Right + 32;
        var rightWidth = Math.Max(360, section.Right - rightX);
        return new Rectangle(rightX, section.Y, rightWidth, Math.Max(220, (section.Height - 30) / 2));
    }

    private Rectangle GetOverviewRightBottomBounds()
    {
        var top = GetOverviewRightTopBounds();
        var section = GetSectionBounds();
        return new Rectangle(top.X, top.Bottom + 30, top.Width, Math.Max(220, section.Bottom - top.Bottom - 30));
    }

    private Rectangle GetContinueButtonBounds()
    {
        var left = GetOverviewLeftBounds();
        return new Rectangle(left.X + 36, left.Y + Math.Min(left.Height - 96, 250), 240, 56);
    }

    private Rectangle GetPlayMatchButtonBounds()
    {
        var continueButton = GetContinueButtonBounds();
        return new Rectangle(continueButton.Right + 28, continueButton.Y, 240, 56);
    }

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

    private int ResolveMatchWeek(Guid fixtureId) =>
        _gameState?.Fixtures.GetValueOrDefault(fixtureId)?.MatchWeek ?? 0;

    private static void ResetTableState(TableState state)
    {
        state.SortColumnIndex = 0;
        state.Direction = TableSortDirection.Ascending;
    }

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

    private sealed record StandingRow(
        Guid ClubId,
        int Position,
        string Name,
        int Points,
        int Played,
        int Wins,
        int Draws,
        int Losses,
        int GoalsFor,
        int GoalsAgainst,
        int GoalDifference,
        string GoalDifferenceText,
        string Form);

    private void DrawDashboardHeader(Club playerClub, MatchdayStatus matchdayStatus)
    {
        var header = GetHeaderBounds();
        DrawPanel(header, new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, $"{playerClub.Name}  |  Season {_gameState!.CurrentSeason}", new Vector2(header.X + 40, header.Y + 32), Color.White, 30, true);
        _text.DrawText(_spriteBatch!, matchdayStatus.NoticeText, new Vector2(header.X + 40, header.Y + 80), matchdayStatus.IsMatchDay ? new Color(94, 203, 144) : new Color(109, 158, 235), 22, true);
    }

    private void DrawOverview(Club playerClub, Fixture? nextFixture, MatchdayStatus matchdayStatus)
    {
        var left = GetOverviewLeftBounds();
        var topRight = GetOverviewRightTopBounds();
        var bottomRight = GetOverviewRightBottomBounds();

        DrawPanel(left, new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, "NEXT MATCH", new Vector2(left.X + 36, left.Y + 32), Color.White, 24, true);
        _text.DrawMultilineText(
            _spriteBatch!,
            [
                $"Record: {playerClub.SeasonWins}-{playerClub.SeasonDraws}-{playerClub.SeasonLosses}",
                $"Points: {playerClub.GetPoints()}",
                $"Goal Difference: {playerClub.GetGoalDifference()}",
                $"Budget: EUR {playerClub.BudgetInMillions}M",
                ""
            ],
            new Vector2(left.X + 36, left.Y + 80),
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
                new Vector2(left.X + 36, left.Y + 210),
                Color.White,
                22,
                12);
        }

        DrawButton(GetContinueButtonBounds(), matchdayStatus.ContinueLabel, matchdayStatus.IsMatchDay ? new Color(42, 111, 196) : new Color(44, 145, 94));
        DrawButton(GetPlayMatchButtonBounds(), "PLAY MATCH", matchdayStatus.IsMatchDay ? new Color(44, 145, 94) : new Color(70, 74, 82));

        DrawPanel(topRight, new Color(22, 27, 34));
        _text.DrawText(_spriteBatch!, "STANDINGS SNAPSHOT", new Vector2(topRight.X + 36, topRight.Y + 32), Color.White, 24, true);
        var standings = BuildStandings(playerClub.Division).Take(10).ToList();
        for (var index = 0; index < standings.Count; index++)
        {
            var row = standings[index];
            var y = topRight.Y + 80 + index * 22;
            var highlight = row.ClubId == playerClub.Id ? new Color(94, 203, 144) : Color.White;
            _text.DrawText(_spriteBatch!, $"{row.Position,2}. {row.Name,-20} {row.Points,3} pts  GD {row.GoalDifference,3}", new Vector2(topRight.X + 36, y), highlight, 18, row.ClubId == playerClub.Id);
        }

        DrawPanel(bottomRight, new Color(22, 27, 34));
        _text.DrawText(_spriteBatch!, "MATCH COMMENTARY", new Vector2(bottomRight.X + 36, bottomRight.Y + 32), Color.White, 24, true);
        var lines = GetLatestCommentaryLines();
        _text.DrawMultilineText(_spriteBatch!, lines, new Vector2(bottomRight.X + 36, bottomRight.Y + 80), new Color(220, 226, 236), 17, 10);
    }

    private void DrawStandingsSection(Club playerClub)
    {
        var division = (Division)_standingsDivisionIndex;
        var section = GetSectionBounds();
        DrawPanel(section, new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, $"FULL STANDINGS - {FormatDivision(division)}", new Vector2(section.X + 36, section.Y + 32), Color.White, 24, true);
        _text.DrawText(_spriteBatch!, "Left/Right to switch division | Click headers to sort", new Vector2(section.X + 36, section.Y + 58), new Color(160, 170, 184), 16);
        DrawStandingsDivisionButtons(division);
        DrawStandingsOverviewCards(playerClub);
        var sortedRows = ApplySort(BuildStandings(division), GetStandingsColumns(), _standingsTableState);
        DrawTable(
            GetStandingsTableBounds(),
            sortedRows,
            GetStandingsColumns(),
            _standingsTableState,
            row => row.ClubId == playerClub.Id ? new Color(94, 203, 144) : Color.White,
            rowHeight: 28,
            title: "Detailed Table");
    }

    private void DrawFixturesSection()
    {
        var section = GetSectionBounds();
        DrawPanel(section, new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, "FIXTURES AND RESULTS", new Vector2(section.X + 36, section.Y + 32), Color.White, 24, true);
        _text.DrawText(_spriteBatch!, "Up/Down to scroll | Click headers to sort", new Vector2(section.Right - 390, section.Y + 34), new Color(160, 170, 184), 16);
        var rows = ApplySort(BuildFixtureTableRows(), GetFixtureColumns(), _fixturesTableState);
        DrawTable(
            GetFixturesTableBounds(),
            rows,
            GetFixtureColumns(),
            _fixturesTableState,
            row => row.IsPlayed ? new Color(220, 226, 236) : new Color(109, 158, 235),
            rowHeight: 28,
            scrollIndex: _fixtureScrollIndex);
    }

    private void DrawHistorySection()
    {
        var section = GetSectionBounds();
        DrawPanel(section, new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, "100-YEAR HISTORY", new Vector2(section.X + 36, section.Y + 32), Color.White, 24, true);
        _text.DrawText(_spriteBatch!, "Scroll with Up/Down | Click headers to sort", new Vector2(section.Right - 400, section.Y + 34), new Color(160, 170, 184), 16);
        var leagueRows = ApplySort(BuildHistoryLeagueRows(), GetHistoryLeagueColumns(), _historyLeagueTableState);
        var cupRows = ApplySort(BuildHistoryCupRows(), GetHistoryCupColumns(), _historyCupTableState);
        DrawTable(
            GetHistoryLeagueTableBounds(),
            leagueRows,
            GetHistoryLeagueColumns(),
            _historyLeagueTableState,
            _ => new Color(220, 226, 236),
            rowHeight: 28,
            title: "League Roll Of Honour");
        DrawTable(
            GetHistoryCupTableBounds(),
            cupRows,
            GetHistoryCupColumns(),
            _historyCupTableState,
            _ => new Color(220, 226, 236),
            rowHeight: 28,
            title: "Cup Roll Of Honour",
            scrollIndex: _historyScrollIndex / 2);
    }

    private void DrawSearchSection()
    {
        var section = GetSectionBounds();
        DrawPanel(section, new Color(22, 27, 34));
        _text!.DrawText(_spriteBatch!, "PERSON SEARCH", new Vector2(section.X + 36, section.Y + 32), Color.White, 24, true);
        _text.DrawText(_spriteBatch!, "Directory preview | Click headers to sort", new Vector2(section.Right - 380, section.Y + 34), new Color(160, 170, 184), 16);
        var rows = ApplySort(BuildSearchTableRows(), GetSearchColumns(), _searchTableState);
        DrawTable(
            GetSearchTableBounds(),
            rows,
            GetSearchColumns(),
            _searchTableState,
            row => row.PersonId == _selectedSearchPersonId ? new Color(94, 203, 144) : new Color(220, 226, 236),
            rowHeight: 28,
            scrollIndex: _searchScrollIndex);
        DrawSearchDetail();
    }

    private List<FixtureTableRow> BuildFixtureTableRows()
    {
        if (_gameState == null)
        {
            return [];
        }

        var upcoming = GetActiveFixtures()
            .Where(fixture => !fixture.IsPlayed)
            .OrderBy(fixture => fixture.ScheduledDate)
            .Select(fixture => new FixtureTableRow(
                "Upcoming",
                fixture.ScheduledDate.ToLocalTime().ToString("dd/MM"),
                fixture.MatchWeek,
                GetClubName(fixture.HomeClubId),
                "-",
                GetClubName(fixture.AwayClubId),
                false));
        var results = _gameState.Matches.Values
            .OrderByDescending(match => match.PlayedAt)
            .Select(match => new FixtureTableRow(
                "Result",
                match.PlayedAt.ToLocalTime().ToString("dd/MM"),
                ResolveMatchWeek(match.FixtureId),
                GetClubName(match.HomeClubId),
                $"{match.HomeGoals}-{match.AwayGoals}",
                GetClubName(match.AwayClubId),
                true));
        return upcoming.Concat(results).ToList();
    }

    private List<HistoryLeagueRow> BuildHistoryLeagueRows()
    {
        return _gameState == null
            ? []
            : _historyService.GetRollOfHonour(_gameState)
                .Select(entry => new HistoryLeagueRow(entry.Season, entry.SerieAChampion, entry.SerieBChampion, entry.SerieCChampion))
                .ToList();
    }

    private List<HistoryCupRow> BuildHistoryCupRows()
    {
        return _gameState == null
            ? []
            : _historyService.GetCupRollOfHonour(_gameState)
                .Select(entry => new HistoryCupRow(entry.Season, entry.SerieACupWinner, entry.SerieBCupWinner, entry.SerieCCupWinner, entry.MasterCupWinner))
                .ToList();
    }

    private List<SearchTableRow> BuildSearchTableRows()
    {
        if (_gameState == null)
        {
            return [];
        }

        return _personDirectoryService.Search(_gameState, category: PersonCategory.All, take: 80)
            .Select(entry => new SearchTableRow(
                entry.PersonId,
                entry.PersonType,
                entry.FullName,
                entry.Role,
                entry.ClubName,
                entry.Age,
                entry.Nationality,
                entry.Reputation,
                entry.Status))
            .ToList();
    }

    private static string BuildClubForm(Club club)
    {
        var form = new List<string>();
        form.AddRange(Enumerable.Repeat("W", Math.Min(5, club.SeasonWins)));
        form.AddRange(Enumerable.Repeat("D", Math.Max(0, Math.Min(5 - form.Count, club.SeasonDraws))));
        form.AddRange(Enumerable.Repeat("L", Math.Max(0, Math.Min(5 - form.Count, club.SeasonLosses))));
        return form.Count == 0 ? "-" : string.Join(' ', form.Take(5));
    }

    private IReadOnlyList<TableColumn<StandingRow>> GetStandingsColumns() =>
    [
        new TableColumn<StandingRow>("POS", 90, row => row.Position.ToString(), row => row.Position),
        new TableColumn<StandingRow>("CLUB", 280, row => row.Name, row => row.Name),
        new TableColumn<StandingRow>("PTS", 70, row => row.Points.ToString(), row => row.Points),
        new TableColumn<StandingRow>("P", 60, row => row.Played.ToString(), row => row.Played),
        new TableColumn<StandingRow>("W", 60, row => row.Wins.ToString(), row => row.Wins),
        new TableColumn<StandingRow>("D", 60, row => row.Draws.ToString(), row => row.Draws),
        new TableColumn<StandingRow>("L", 60, row => row.Losses.ToString(), row => row.Losses),
        new TableColumn<StandingRow>("GF", 70, row => row.GoalsFor.ToString(), row => row.GoalsFor),
        new TableColumn<StandingRow>("GA", 70, row => row.GoalsAgainst.ToString(), row => row.GoalsAgainst),
        new TableColumn<StandingRow>("GD", 70, row => row.GoalDifferenceText, row => row.GoalDifference),
        new TableColumn<StandingRow>("FORM", 120, row => row.Form, row => row.Form)
    ];

    private IReadOnlyList<TableColumn<FixtureTableRow>> GetFixtureColumns() =>
    [
        new TableColumn<FixtureTableRow>("TYPE", 140, row => row.Type, row => row.Type),
        new TableColumn<FixtureTableRow>("DATE", 120, row => row.DateText, row => row.DateText),
        new TableColumn<FixtureTableRow>("WEEK", 120, row => row.MatchWeek.ToString(), row => row.MatchWeek),
        new TableColumn<FixtureTableRow>("HOME", 320, row => row.HomeClub, row => row.HomeClub),
        new TableColumn<FixtureTableRow>("SCORE", 120, row => row.Score, row => row.Score),
        new TableColumn<FixtureTableRow>("AWAY", 320, row => row.AwayClub, row => row.AwayClub)
    ];

    private IReadOnlyList<TableColumn<HistoryLeagueRow>> GetHistoryLeagueColumns() =>
    [
        new TableColumn<HistoryLeagueRow>("SEASON", 120, row => row.Season.ToString(), row => row.Season),
        new TableColumn<HistoryLeagueRow>("SERIE A", 260, row => row.SerieAChampion, row => row.SerieAChampion),
        new TableColumn<HistoryLeagueRow>("SERIE B", 260, row => row.SerieBChampion, row => row.SerieBChampion),
        new TableColumn<HistoryLeagueRow>("SERIE C", 260, row => row.SerieCChampion, row => row.SerieCChampion)
    ];

    private IReadOnlyList<TableColumn<HistoryCupRow>> GetHistoryCupColumns() =>
    [
        new TableColumn<HistoryCupRow>("SEASON", 120, row => row.Season.ToString(), row => row.Season),
        new TableColumn<HistoryCupRow>("A CUP", 210, row => row.SerieACupWinner, row => row.SerieACupWinner),
        new TableColumn<HistoryCupRow>("B CUP", 210, row => row.SerieBCupWinner, row => row.SerieBCupWinner),
        new TableColumn<HistoryCupRow>("C CUP", 210, row => row.SerieCCupWinner, row => row.SerieCCupWinner),
        new TableColumn<HistoryCupRow>("MASTER", 220, row => row.MasterCupWinner, row => row.MasterCupWinner)
    ];

    private IReadOnlyList<TableColumn<SearchTableRow>> GetSearchColumns() =>
    [
        new TableColumn<SearchTableRow>("TYPE", 120, row => row.Type, row => row.Type),
        new TableColumn<SearchTableRow>("NAME", 250, row => row.Name, row => row.Name),
        new TableColumn<SearchTableRow>("ROLE", 240, row => row.Role, row => row.Role),
        new TableColumn<SearchTableRow>("CLUB", 220, row => row.Club, row => row.Club),
        new TableColumn<SearchTableRow>("AGE", 90, row => row.Age.ToString(), row => row.Age),
        new TableColumn<SearchTableRow>("NAT", 120, row => row.Nationality, row => row.Nationality),
        new TableColumn<SearchTableRow>("REP", 90, row => row.Reputation.ToString(), row => row.Reputation),
        new TableColumn<SearchTableRow>("STATUS", 170, row => row.Status, row => row.Status)
    ];

    private IReadOnlyList<TableColumn<PersonPropertyEntry>> GetSearchDetailColumns() =>
    [
        new TableColumn<PersonPropertyEntry>("GROUP", 220, row => row.Group, row => row.Group),
        new TableColumn<PersonPropertyEntry>("PROPERTY", 340, row => row.Name, row => row.Name),
        new TableColumn<PersonPropertyEntry>("VALUE", 420, row => row.Value, row => row.Value)
    ];

    private IReadOnlyList<T> ApplySort<T>(IReadOnlyList<T> rows, IReadOnlyList<TableColumn<T>> columns, TableState state)
    {
        if (rows.Count == 0 || columns.Count == 0)
        {
            return rows;
        }

        var columnIndex = Math.Clamp(state.SortColumnIndex, 0, columns.Count - 1);
        return TableSortService.Sort(rows, columns[columnIndex].SortValue, state.Direction);
    }

    private void HandleTableSort<T>(Rectangle bounds, IReadOnlyList<TableColumn<T>> columns, TableState state, MouseState mouse)
    {
        var x = bounds.X;
        for (var index = 0; index < columns.Count; index++)
        {
            var headerBounds = new Rectangle(x, bounds.Y, columns[index].Width, 34);
            if (WasLeftClicked(headerBounds, mouse))
            {
                if (state.SortColumnIndex == index)
                {
                    state.Direction = state.Direction == TableSortDirection.Ascending
                        ? TableSortDirection.Descending
                        : TableSortDirection.Ascending;
                }
                else
                {
                    state.SortColumnIndex = index;
                    state.Direction = TableSortDirection.Ascending;
                }
            }

            x += columns[index].Width;
        }
    }

    private void DrawTable<T>(
        Rectangle bounds,
        IReadOnlyList<T> rows,
        IReadOnlyList<TableColumn<T>> columns,
        TableState state,
        Func<T, Color> rowColorSelector,
        int rowHeight,
        string? title = null,
        int scrollIndex = 0)
    {
        DrawPanel(bounds, new Color(22, 27, 34));
        if (!string.IsNullOrWhiteSpace(title))
        {
            _text!.DrawText(_spriteBatch!, title, new Vector2(bounds.X + 18, bounds.Y + 12), Color.White, 20, true);
        }

        var headerY = string.IsNullOrWhiteSpace(title) ? bounds.Y : bounds.Y + 40;
        var x = bounds.X;
        for (var index = 0; index < columns.Count; index++)
        {
            var selected = state.SortColumnIndex == index;
            var headerBounds = new Rectangle(x, headerY, columns[index].Width, 34);
            DrawPanel(headerBounds, selected ? new Color(42, 111, 196) : new Color(32, 39, 50));
            var directionText = selected ? (state.Direction == TableSortDirection.Ascending ? " ^" : " v") : string.Empty;
            _text!.DrawText(_spriteBatch!, columns[index].Header + directionText, new Vector2(x + 10, headerY + 8), Color.White, 16, true);
            x += columns[index].Width;
        }

        var visibleRows = rows.Skip(scrollIndex).Take(Math.Max(1, (bounds.Height - (headerY - bounds.Y) - 42) / rowHeight)).ToList();
        for (var rowIndex = 0; rowIndex < visibleRows.Count; rowIndex++)
        {
            var row = visibleRows[rowIndex];
            var rowY = headerY + 40 + rowIndex * rowHeight;
            var rowBounds = new Rectangle(bounds.X, rowY, bounds.Width, rowHeight - 2);
            DrawPanel(rowBounds, rowIndex % 2 == 0 ? new Color(26, 32, 40) : new Color(21, 26, 33));
            var textColor = rowColorSelector(row);
            x = bounds.X;
            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                _text!.DrawText(_spriteBatch!, columns[columnIndex].DisplayValue(row), new Vector2(x + 10, rowY + 6), textColor, 15);
                x += columns[columnIndex].Width;
            }
        }
    }

    private void DrawStandingsDivisionButtons(Division selectedDivision)
    {
        foreach (var division in Enum.GetValues<Division>())
        {
            var bounds = GetStandingsDivisionButtonBounds(division);
            DrawPanel(bounds, division == selectedDivision ? new Color(42, 111, 196) : new Color(32, 39, 50));
            _text!.DrawText(_spriteBatch!, FormatDivision(division).ToUpperInvariant(), new Vector2(bounds.X + 16, bounds.Y + 10), Color.White, 16, true);
        }
    }

    private IReadOnlyList<TableColumn<StandingRow>> GetStandingsOverviewColumns() =>
    [
        new TableColumn<StandingRow>("#", 44, row => row.Position.ToString(), row => row.Position),
        new TableColumn<StandingRow>("CLUB", 190, row => row.Name, row => row.Name),
        new TableColumn<StandingRow>("PTS", 54, row => row.Points.ToString(), row => row.Points)
    ];

    private void DrawStandingsOverviewCards(Club playerClub)
    {
        foreach (var division in Enum.GetValues<Division>())
        {
            var panel = GetStandingsOverviewPanelBounds(division);
            DrawPanel(panel, new Color(20, 24, 31));
            _text!.DrawText(_spriteBatch!, FormatDivision(division).ToUpperInvariant(), new Vector2(panel.X + 16, panel.Y + 14), division == playerClub.Division ? new Color(94, 203, 144) : Color.White, 17, true);
            DrawTable(
                new Rectangle(panel.X + 10, panel.Y + 42, panel.Width - 20, panel.Height - 52),
                BuildStandings(division).Take(8).ToList(),
                GetStandingsOverviewColumns(),
                new TableState(),
                row => row.ClubId == playerClub.Id ? new Color(94, 203, 144) : new Color(220, 226, 236),
                rowHeight: 24);
        }
    }

    private void HandleSearchSelection(IReadOnlyList<SearchTableRow> rows, MouseState mouse)
    {
        var bounds = GetSearchTableBounds();
        var rowHeight = 28;
        var firstRowY = bounds.Y + 40;
        var visibleCount = Math.Max(1, (bounds.Height - 42) / rowHeight);
        var visibleRows = rows.Skip(_searchScrollIndex).Take(visibleCount).ToList();
        for (var index = 0; index < visibleRows.Count; index++)
        {
            var rowBounds = new Rectangle(bounds.X, firstRowY + index * rowHeight, bounds.Width, rowHeight - 2);
            if (WasLeftClicked(rowBounds, mouse))
            {
                _selectedSearchPersonId = visibleRows[index].PersonId;
                return;
            }
        }
    }

    private PersonDetail? GetSelectedPersonDetail()
    {
        if (_gameState == null)
        {
            return null;
        }

        var rows = ApplySort(BuildSearchTableRows(), GetSearchColumns(), _searchTableState);
        _selectedSearchPersonId ??= rows.FirstOrDefault()?.PersonId;
        return _selectedSearchPersonId.HasValue
            ? _personDirectoryService.GetDetail(_gameState, _selectedSearchPersonId.Value)
            : null;
    }

    private void DrawSearchDetail()
    {
        var detail = GetSelectedPersonDetail();
        var header = GetSearchDetailHeaderBounds();
        DrawPanel(header, new Color(20, 24, 31));
        _text!.DrawText(_spriteBatch!, detail?.FullName ?? "No person selected", new Vector2(header.X + 16, header.Y + 10), new Color(94, 203, 144), 20, true);
        _text.DrawText(_spriteBatch!, detail?.Subtitle ?? "Select a row from the table above.", new Vector2(header.X + 320, header.Y + 12), new Color(220, 226, 236), 14);
        _text.DrawText(_spriteBatch!, detail?.ClubName ?? string.Empty, new Vector2(header.Right - 220, header.Y + 12), new Color(160, 170, 184), 14, true);

        var properties = detail?.Properties ?? [];
        var sorted = ApplySort(properties, GetSearchDetailColumns(), _searchDetailTableState);
        DrawTable(
            GetSearchDetailTableBounds(),
            sorted,
            GetSearchDetailColumns(),
            _searchDetailTableState,
            _ => new Color(220, 226, 236),
            rowHeight: 26);
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

    private sealed class TableState
    {
        public int SortColumnIndex { get; set; }
        public TableSortDirection Direction { get; set; } = TableSortDirection.Ascending;
    }

    private sealed record TableColumn<T>(
        string Header,
        int Width,
        Func<T, string> DisplayValue,
        Func<T, IComparable?> SortValue);

    private sealed record FixtureTableRow(
        string Type,
        string DateText,
        int MatchWeek,
        string HomeClub,
        string Score,
        string AwayClub,
        bool IsPlayed);

    private sealed record HistoryLeagueRow(
        int Season,
        string SerieAChampion,
        string SerieBChampion,
        string SerieCChampion);

    private sealed record HistoryCupRow(
        int Season,
        string SerieACupWinner,
        string SerieBCupWinner,
        string SerieCCupWinner,
        string MasterCupWinner);

    private sealed record SearchTableRow(
        Guid PersonId,
        string Type,
        string Name,
        string Role,
        string Club,
        int Age,
        string Nationality,
        int Reputation,
        string Status);
}
