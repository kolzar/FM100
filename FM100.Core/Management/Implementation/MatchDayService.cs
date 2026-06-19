using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.Core.Management.Implementation;

public class MatchDayService : IMatchDayService
{
    public int CalculateMatchPerformance(Club club, GameState.GameState gameState)
    {
        var formBonus = club.GetPoints() switch
        {
            >= 20 => 2,
            >= 10 => 1,
            _ => 0
        };

        if (!gameState.Lineups.TryGetValue(club.Id, out var lineup) ||
            lineup.StartingPlayerIds.Count == 0)
        {
            return Math.Clamp(club.Reputation + formBonus, 8, 20);
        }

        var starters = lineup.StartingPlayerIds
            .Select(id => gameState.Players.TryGetValue(id, out var player) ? player : null)
            .Where(player => player is { IsInjured: false })
            .Select(player => player!)
            .ToList();

        if (starters.Count == 0)
        {
            return Math.Clamp(club.Reputation + formBonus, 8, 20);
        }

        var averageReputation = starters.Average(player => player.Reputation);
        var averageMorale = starters.Average(player => player.CurrentState.Morale);
        var averageFatigue = starters.Average(player => player.CurrentState.Fatigue);
        var moraleBonus = (int)Math.Round((averageMorale - 10) / 4);
        var fatiguePenalty = averageFatigue >= 14 ? 2 : averageFatigue >= 9 ? 1 : 0;
        var squadPerformance = (int)Math.Round((averageReputation + club.Reputation) / 2);

        return Math.Clamp(squadPerformance + formBonus + moraleBonus - fatiguePenalty, 8, 20);
    }

    public void ApplyPlayerMatchEffects(GameState.GameState gameState, Match match, Club homeClub, Club awayClub)
    {
        ApplyClubPlayerMatchEffects(gameState, homeClub, match.HomeGoals.CompareTo(match.AwayGoals));
        ApplyClubPlayerMatchEffects(gameState, awayClub, match.AwayGoals.CompareTo(match.HomeGoals));
    }

    private static void ApplyClubPlayerMatchEffects(GameState.GameState gameState, Club club, int resultComparison)
    {
        if (!gameState.Lineups.TryGetValue(club.Id, out var lineup))
        {
            return;
        }

        var moraleDelta = resultComparison switch
        {
            > 0 => 2,
            < 0 => -2,
            _ => 0
        };

        foreach (var playerId in lineup.StartingPlayerIds)
        {
            if (!gameState.Players.TryGetValue(playerId, out var player))
            {
                continue;
            }

            player.PlayedMinutes += 90;
            player.CurrentState.Fatigue = Math.Clamp(player.CurrentState.Fatigue + 2, 1, 20);
            player.CurrentState.Morale = Math.Clamp(player.CurrentState.Morale + moraleDelta, 1, 20);
            player.CurrentState.Happiness = Math.Clamp(player.CurrentState.Happiness + Math.Sign(moraleDelta), 1, 20);
            player.CurrentState.Confidence = Math.Clamp(player.CurrentState.Confidence + moraleDelta, 1, 20);
            ApplyFatigueInjuryRisk(player);
            player.CurrentState.LastUpdated = DateTime.UtcNow;
        }

        foreach (var playerId in lineup.SubstitutePlayerIds)
        {
            if (!gameState.Players.TryGetValue(playerId, out var player))
            {
                continue;
            }

            player.CurrentState.Fatigue = Math.Clamp(player.CurrentState.Fatigue - 1, 1, 20);
            player.CurrentState.LastUpdated = DateTime.UtcNow;
        }
    }

    private static void ApplyFatigueInjuryRisk(FootballPlayer player)
    {
        if (player.IsInjured || player.CurrentState.Fatigue < 16)
        {
            return;
        }

        player.InjuryDaysRemaining = 7;
        player.InjuryDescription = "Fatigue strain";
    }
}
