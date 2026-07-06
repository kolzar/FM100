using FM100.Core.GameState;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management.Implementation;

public sealed class PlayerDevelopmentService : IPlayerDevelopmentService
{
    public IReadOnlyList<PlayerDevelopmentRecord> ApplySeasonDevelopment(GameState.GameState gameState)
    {
        var squadClubByPlayerId = gameState.Clubs.Values
            .SelectMany(club => club.PlayerIds.Select(playerId => (PlayerId: playerId, Club: club)))
            .ToDictionary(item => item.PlayerId, item => item.Club);
        var records = new List<PlayerDevelopmentRecord>();

        foreach (var player in gameState.Players.Values)
        {
            var beforeReputation = player.Reputation;
            var beforePotential = player.Potential;
            var beforeMarketValue = player.MarketValue;
            var developmentDelta = CalculateDevelopmentDelta(player);
            var potentialDelta = CalculatePotentialDelta(player, developmentDelta);

            player.Reputation = Math.Clamp(player.Reputation + developmentDelta, 1, 20);
            player.Potential = Math.Clamp(player.Potential + potentialDelta, player.Reputation, 20);
            player.MarketValue = CalculateMarketValue(player, beforeMarketValue);

            if (player.Reputation == beforeReputation &&
                player.Potential == beforePotential &&
                player.MarketValue == beforeMarketValue)
            {
                continue;
            }

            var clubId = squadClubByPlayerId.TryGetValue(player.Id, out var club) ? club.Id : (Guid?)null;
            var record = new PlayerDevelopmentRecord
            {
                PlayerId = player.Id,
                ClubId = clubId,
                Season = gameState.CurrentSeason,
                PlayerName = $"{player.FirstName} {player.LastName}".Trim(),
                ReputationBefore = beforeReputation,
                ReputationAfter = player.Reputation,
                PotentialBefore = beforePotential,
                PotentialAfter = player.Potential,
                MarketValueBefore = beforeMarketValue,
                MarketValueAfter = player.MarketValue,
                PlayedMinutes = player.PlayedMinutes,
                Summary = BuildSummary(player, beforeReputation, beforePotential, beforeMarketValue)
            };

            gameState.PlayerDevelopmentHistory.Add(record);
            records.Add(record);
        }

        return records;
    }

    private static int CalculateDevelopmentDelta(FootballPlayer player)
    {
        var minutesScore = player.PlayedMinutes switch
        {
            >= 2700 => 2,
            >= 1200 => 1,
            <= 180 => -1,
            _ => 0
        };
        var ageScore = player.Age switch
        {
            <= 21 => 1,
            >= 33 => -1,
            _ => 0
        };
        var moraleScore = player.CurrentState.Morale >= 15 || player.CurrentState.Motivation >= 15 ? 1 : 0;
        var stressPenalty = player.CurrentState.Stress >= 16 || player.CurrentState.Fatigue >= 16 ? -1 : 0;
        var ceilingPenalty = player.Reputation >= player.Potential ? -1 : 0;

        return Math.Clamp(minutesScore + ageScore + moraleScore + stressPenalty + ceilingPenalty, -2, 2);
    }

    private static int CalculatePotentialDelta(FootballPlayer player, int developmentDelta)
    {
        if (player.Age <= 21 && developmentDelta > 0 && player.Potential < 20)
        {
            return 1;
        }

        if (player.Age >= 34 && player.PlayedMinutes < 600)
        {
            return -1;
        }

        return 0;
    }

    private static int CalculateMarketValue(FootballPlayer player, int beforeMarketValue)
    {
        var minutesAdjustment = player.PlayedMinutes switch
        {
            >= 2400 => 2,
            >= 1200 => 1,
            <= 180 => -2,
            _ => 0
        };
        var ageAdjustment = player.Age switch
        {
            <= 22 => 2,
            >= 34 => -2,
            _ => 0
        };
        var value = beforeMarketValue + (player.Reputation * 2 + player.Potential) / 12 + minutesAdjustment + ageAdjustment;

        return Math.Clamp(value, 1, 80);
    }

    private static string BuildSummary(
        FootballPlayer player,
        int beforeReputation,
        int beforePotential,
        int beforeMarketValue)
    {
        var reputationChange = player.Reputation - beforeReputation;
        var potentialChange = player.Potential - beforePotential;
        var valueChange = player.MarketValue - beforeMarketValue;

        return $"Rep {FormatDelta(reputationChange)}, Pot {FormatDelta(potentialChange)}, Value {FormatDelta(valueChange)}M after {player.PlayedMinutes} minutes.";
    }

    private static string FormatDelta(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }
}
