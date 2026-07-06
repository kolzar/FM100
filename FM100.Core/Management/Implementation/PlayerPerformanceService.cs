using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management.Implementation;

public sealed class PlayerPerformanceService : IPlayerPerformanceService
{
    public IReadOnlyList<PlayerPerformanceEntry> GetTopPerformers(
        GameState.GameState gameState,
        Club club,
        int take = 8)
    {
        var recommendedStarterIds = BuildRecommendedStarters(gameState, club)
            .Select(player => player.Id)
            .ToHashSet();

        return club.PlayerIds
            .Select(playerId => gameState.Players.TryGetValue(playerId, out var player) ? player : null)
            .Where(player => player != null)
            .Select(player => BuildEntry(player!, recommendedStarterIds))
            .OrderByDescending(entry => entry.Score)
            .ThenByDescending(entry => entry.PlayedMinutes)
            .ThenBy(entry => entry.PlayerName)
            .Take(Math.Max(0, take))
            .ToList();
    }

    public LineupRecommendationResult ApplyRecommendedLineup(GameState.GameState gameState, Club club)
    {
        var availablePlayers = GetAvailablePlayers(gameState, club).ToList();
        if (availablePlayers.Count < 11)
        {
            return new LineupRecommendationResult(
                false,
                0,
                availablePlayers.Count,
                $"Only {availablePlayers.Count} players are available; 11 are required.");
        }

        var starters = BuildRecommendedStarters(gameState, club);
        if (starters.Count < 11)
        {
            return new LineupRecommendationResult(false, 0, availablePlayers.Count, "A complete recommended XI could not be built.");
        }

        var lineup = gameState.Lineups.TryGetValue(club.Id, out var existing)
            ? existing
            : new TeamLineup { ClubId = club.Id, Formation = club.Formation };
        var previousStarterIds = lineup.StartingPlayerIds.ToHashSet();
        var starterIds = starters.Select(player => player.Id).ToList();
        var changedPlayers = starterIds.Count(playerId => !previousStarterIds.Contains(playerId));
        var starterSet = starterIds.ToHashSet();
        var bench = availablePlayers
            .Where(player => !starterSet.Contains(player.Id))
            .OrderByDescending(CalculateScore)
            .ThenByDescending(player => player.Reputation)
            .Take(12)
            .Select(player => player.Id)
            .ToList();

        lineup.Formation = club.Formation;
        lineup.StartingPlayerIds = starterIds;
        lineup.SubstitutePlayerIds = bench;
        lineup.UpdatedAt = DateTime.UtcNow;
        gameState.Lineups[club.Id] = lineup;
        gameState.LastSavedAt = DateTime.UtcNow;

        return new LineupRecommendationResult(
            true,
            changedPlayers,
            availablePlayers.Count,
            $"Recommended XI applied for {club.Formation}; {changedPlayers} lineup places changed.");
    }

    private static PlayerPerformanceEntry BuildEntry(FootballPlayer player, IReadOnlySet<Guid> recommendedStarterIds)
    {
        var score = CalculateScore(player);

        return new PlayerPerformanceEntry(
            player.Id,
            $"{player.FirstName} {player.LastName}".Trim(),
            player.Position.ToString(),
            score,
            player.PlayedMinutes,
            player.SeasonStats.Goals,
            player.SeasonStats.Assists,
            player.SeasonStats.GetAverageRating(),
            GetWorkload(player.PlayedMinutes),
            GetMood(player),
            GetRisk(player),
            GetRecommendation(player, recommendedStarterIds));
    }

    private static int CalculateScore(FootballPlayer player)
    {
        var state = player.CurrentState;
        var availabilityPenalty = player.IsInjured ? 8 : 0;
        var minutesBonus = player.PlayedMinutes switch
        {
            >= 1800 => 2,
            >= 900 => 1,
            _ => 0
        };
        var mentalScore = (state.Morale + state.Motivation + state.Confidence) / 3;
        var pressurePenalty = (state.Fatigue + state.Stress) / 8;
        var fatiguePenalty = state.Fatigue >= 15 ? 4 : state.Fatigue >= 10 ? 1 : 0;
        var ratingBonus = Math.Max(0, player.SeasonStats.GetAverageRating() - 6);
        var outputBonus = Math.Min(3, (player.SeasonStats.Goals + player.SeasonStats.Assists) / 5);
        var score = player.Reputation + minutesBonus + ratingBonus + outputBonus + (mentalScore - 10) / 2 - pressurePenalty - fatiguePenalty - availabilityPenalty;

        return Math.Clamp(score, 1, 20);
    }

    private static string GetWorkload(int minutes)
    {
        return minutes switch
        {
            >= 1800 => "Core starter",
            >= 900 => "Rotation",
            > 0 => "Limited",
            _ => "Unused"
        };
    }

    private static string GetMood(FootballPlayer player)
    {
        var state = player.CurrentState;
        var average = (state.Morale + state.Motivation + state.Confidence) / 3.0;

        return average switch
        {
            >= 15 => "Inspired",
            >= 11 => "Positive",
            >= 8 => "Flat",
            _ => "Low"
        };
    }

    private static string GetRisk(FootballPlayer player)
    {
        if (player.IsInjured)
        {
            return $"Injured {player.InjuryDaysRemaining}d";
        }

        return player.CurrentState.Fatigue switch
        {
            >= 15 => "High fatigue",
            >= 10 => "Manage load",
            _ => "Available"
        };
    }

    private static string GetRecommendation(FootballPlayer player, IReadOnlySet<Guid> recommendedStarterIds)
    {
        if (player.IsInjured)
        {
            return "UNAVAILABLE";
        }

        if (player.CurrentState.Fatigue >= 15)
        {
            return "REST";
        }

        return recommendedStarterIds.Contains(player.Id) ? "START" : "ROTATE";
    }

    private static IReadOnlyList<FootballPlayer> BuildRecommendedStarters(GameState.GameState gameState, Club club)
    {
        var available = GetAvailablePlayers(gameState, club)
            .OrderByDescending(CalculateScore)
            .ThenByDescending(player => player.Reputation)
            .ThenBy(player => player.ShirtNumber)
            .ToList();
        var shape = GetFormationShape(club.Formation);
        var starters = new List<FootballPlayer>();

        AddByPosition(starters, available, PlayerPosition.Goalkeeper, 1);
        AddByPosition(starters, available, PlayerPosition.Defender, shape.Defenders);
        AddByPosition(starters, available, PlayerPosition.Midfielder, shape.Midfielders);
        AddByPosition(starters, available, PlayerPosition.Forward, shape.Forwards);
        starters.AddRange(available.Where(player => !starters.Contains(player)).Take(11 - starters.Count));

        return starters.Take(11).ToList();
    }

    private static IEnumerable<FootballPlayer> GetAvailablePlayers(GameState.GameState gameState, Club club)
    {
        return club.PlayerIds
            .Select(playerId => gameState.Players.GetValueOrDefault(playerId))
            .Where(player => player is { IsInjured: false } && player.CurrentState.Fatigue < 15)
            .Select(player => player!);
    }

    private static void AddByPosition(
        ICollection<FootballPlayer> starters,
        IEnumerable<FootballPlayer> players,
        PlayerPosition position,
        int count)
    {
        foreach (var player in players.Where(player => player.Position == position && !starters.Contains(player)).Take(count))
        {
            starters.Add(player);
        }
    }

    private static (int Defenders, int Midfielders, int Forwards) GetFormationShape(string formation)
    {
        var parts = formation
            .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => int.TryParse(part, out var value) ? value : 0)
            .Where(value => value > 0)
            .ToList();
        return parts.Count < 3
            ? (4, 3, 3)
            : (parts[0], parts.Skip(1).Take(parts.Count - 2).Sum(), parts[^1]);
    }
}
