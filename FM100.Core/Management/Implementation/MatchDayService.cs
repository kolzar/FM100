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
        var averageMotivation = starters.Average(player => player.CurrentState.Motivation);
        var averageFatigue = starters.Average(player => player.CurrentState.Fatigue);
        var moraleBonus = (int)Math.Round((averageMorale - 10) / 4);
        var motivationBonus = averageMotivation >= 16 ? 2 : averageMotivation >= 13 ? 1 : 0;
        var fatiguePenalty = averageFatigue >= 14 ? 2 : averageFatigue >= 9 ? 1 : 0;
        var tacticalModifier = CalculateTacticalModifier(lineup, starters);
        var squadPerformance = (int)Math.Round((averageReputation + club.Reputation) / 2);

        return Math.Clamp(squadPerformance + formBonus + moraleBonus + motivationBonus + tacticalModifier - fatiguePenalty, 8, 20);
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
            player.CurrentState.Fatigue = Math.Clamp(player.CurrentState.Fatigue + 2 + CalculateTacticalFatigueDelta(lineup), 1, 20);
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

    private static int CalculateTacticalModifier(TeamLineup lineup, IReadOnlyCollection<FootballPlayer> starters)
    {
        var averageTacticalIntelligence = starters.Average(player => player.MentalAttributes.TacticalIntelligence);
        var modifier = lineup.Mentality switch
        {
            TacticalMentality.Attacking => 1,
            TacticalMentality.Defensive => starters.Average(player => player.CurrentState.Fatigue) >= 12 ? 1 : 0,
            _ => 0
        };

        if (lineup.Pressing == PressingIntensity.High)
        {
            modifier += averageTacticalIntelligence >= 13 ? 1 : -1;
        }
        else if (lineup.Pressing == PressingIntensity.Low && lineup.Mentality == TacticalMentality.Defensive)
        {
            modifier += 1;
        }

        if (lineup.Tempo == TempoStyle.Fast)
        {
            modifier += starters.Average(player => player.CurrentState.Motivation) >= 13 ? 1 : 0;
        }
        else if (lineup.Tempo == TempoStyle.Slow && lineup.Mentality == TacticalMentality.Attacking)
        {
            modifier -= 1;
        }

        return modifier;
    }

    private static int CalculateTacticalFatigueDelta(TeamLineup lineup)
    {
        var fatigueDelta = 0;

        if (lineup.Pressing == PressingIntensity.High)
        {
            fatigueDelta++;
        }

        if (lineup.Tempo == TempoStyle.Fast)
        {
            fatigueDelta++;
        }

        if (lineup.Mentality == TacticalMentality.Defensive && lineup.Pressing == PressingIntensity.Low)
        {
            fatigueDelta--;
        }

        return fatigueDelta;
    }
}
