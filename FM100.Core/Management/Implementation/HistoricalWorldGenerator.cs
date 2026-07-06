using FM100.Core.GameState;
using FM100.Domain.Club;

namespace FM100.Core.Management.Implementation;

public sealed class HistoricalWorldGenerator : IHistoricalWorldGenerator
{
    public HistoricalWorldGenerationResult Generate(GameState.GameState gameState, int years = 100)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        var yearsToGenerate = Math.Clamp(years, 1, 100);
        if (gameState.HistoricalLeagueTableArchive.Count > 0)
        {
            return new HistoricalWorldGenerationResult(
                gameState.HistoricalLeagueTableArchive.Select(record => record.Season).Distinct().Count(),
                gameState.HistoricalLeagueTableArchive.Count,
                gameState.HistoricalSeasonAwards.Count(award => award.Title == "League Champion"),
                gameState.HistoricalStartYear,
                gameState.HistoricalEndYear);
        }

        var endYear = DateTime.UtcNow.Year - 1;
        var startYear = endYear - yearsToGenerate + 1;
        var memberships = Enum.GetValues<Division>()
            .ToDictionary(
                division => division,
                division => gameState.Clubs.Values
                    .Where(club => club.Division == division)
                    .OrderBy(club => club.Name)
                    .Select(club => club.Id)
                    .ToList());

        for (var year = endYear; year >= startYear; year--)
        {
            var tables = new Dictionary<Division, LeagueTableArchiveRecord>();
            foreach (var division in Enum.GetValues<Division>())
            {
                var table = BuildTable(gameState, memberships[division], division, year);
                gameState.HistoricalLeagueTableArchive.Add(table);
                tables[division] = table;

                var champion = table.Rows[0];
                gameState.HistoricalTitlesByClub[champion.ClubId] =
                    gameState.HistoricalTitlesByClub.GetValueOrDefault(champion.ClubId) + 1;
                gameState.HistoricalSeasonAwards.Add(new SeasonAwardRecord
                {
                    Season = year,
                    AwardKey = $"history:{year}:{division}:champion",
                    Title = "League Champion",
                    WinnerName = champion.ClubName,
                    ClubId = champion.ClubId,
                    Description = $"{champion.ClubName} won {FormatDivision(division)} in {year} with {champion.Points} points.",
                    CreatedAt = new DateTime(year, 6, 30, 12, 0, 0, DateTimeKind.Utc)
                });
            }

            memberships = BuildPreviousMemberships(tables);
        }

        gameState.HistoricalStartYear = startYear;
        gameState.HistoricalEndYear = endYear;
        gameState.HistoricalWorldGeneratedAt = DateTime.UtcNow;
        return new HistoricalWorldGenerationResult(
            yearsToGenerate,
            yearsToGenerate * 3,
            yearsToGenerate * 3,
            startYear,
            endYear);
    }

    private static LeagueTableArchiveRecord BuildTable(
        GameState.GameState gameState,
        IReadOnlyCollection<Guid> clubIds,
        Division division,
        int year)
    {
        var clubs = clubIds
            .Select(clubId => gameState.Clubs.GetValueOrDefault(clubId))
            .Where(club => club != null)
            .Select(club => club!)
            .OrderBy(club => club.Name)
            .ToList();
        var random = new Random(GetStableSeed(year, division, clubs.Select(club => club.Name)));
        var played = Math.Max(0, (clubs.Count - 1) * 2);
        var candidates = clubs.Select(club =>
        {
            var strength = club.Reputation * 5 + random.Next(0, 61);
            var wins = Math.Clamp(played == 0 ? 0 : 4 + strength * Math.Max(1, played - 10) / 160 + random.Next(-3, 4), 0, played);
            var draws = Math.Clamp(played == 0 ? 0 : 5 + random.Next(-2, 5), 0, played - wins);
            var losses = played - wins - draws;
            var goalsFor = Math.Max(0, wins * 2 + draws + random.Next(4, 22));
            var goalsAgainst = Math.Max(0, losses * 2 + draws + random.Next(3, 19));
            return new LeagueTableArchiveRow
            {
                ClubId = club.Id,
                ClubName = club.Name,
                Played = played,
                Wins = wins,
                Draws = draws,
                Losses = losses,
                GoalsFor = goalsFor,
                GoalsAgainst = goalsAgainst,
                GoalDifference = goalsFor - goalsAgainst,
                Points = wins * 3 + draws
            };
        })
        .OrderByDescending(row => row.Points)
        .ThenByDescending(row => row.GoalDifference)
        .ThenByDescending(row => row.GoalsFor)
        .ThenBy(row => row.ClubName)
        .ToList();

        for (var index = 0; index < candidates.Count; index++)
        {
            candidates[index].Position = index + 1;
        }

        return new LeagueTableArchiveRecord
        {
            Season = year,
            Division = division,
            Rows = candidates,
            CreatedAt = new DateTime(year, 6, 30, 12, 0, 0, DateTimeKind.Utc)
        };
    }

    private static Dictionary<Division, List<Guid>> BuildPreviousMemberships(
        IReadOnlyDictionary<Division, LeagueTableArchiveRecord> tables)
    {
        var serieA = tables[Division.SerieA].Rows.Select(row => row.ClubId).ToList();
        var serieB = tables[Division.SerieB].Rows.Select(row => row.ClubId).ToList();
        var serieC = tables[Division.SerieC].Rows.Select(row => row.ClubId).ToList();
        var exchangeCount = Math.Min(3, new[] { serieA.Count, serieB.Count, serieC.Count }.Min());
        if (exchangeCount == 0)
        {
            return new Dictionary<Division, List<Guid>>
            {
                [Division.SerieA] = serieA,
                [Division.SerieB] = serieB,
                [Division.SerieC] = serieC
            };
        }

        var fromAToB = serieA.TakeLast(exchangeCount).ToList();
        var fromBToA = serieB.Take(exchangeCount).ToList();
        var fromBToC = serieB.TakeLast(exchangeCount).ToList();
        var fromCToB = serieC.Take(exchangeCount).ToList();
        return new Dictionary<Division, List<Guid>>
        {
            [Division.SerieA] = serieA.Except(fromAToB).Concat(fromBToA).ToList(),
            [Division.SerieB] = serieB.Except(fromBToA).Except(fromBToC).Concat(fromAToB).Concat(fromCToB).ToList(),
            [Division.SerieC] = serieC.Except(fromCToB).Concat(fromBToC).ToList()
        };
    }

    private static int GetStableSeed(int year, Division division, IEnumerable<string> clubNames)
    {
        unchecked
        {
            var hash = 17;
            foreach (var character in $"{year}|{division}|{string.Join('|', clubNames)}")
            {
                hash = hash * 31 + character;
            }

            return hash;
        }
    }

    private static string FormatDivision(Division division) => division switch
    {
        Division.SerieA => "Serie A",
        Division.SerieB => "Serie B",
        _ => "Serie C"
    };
}
