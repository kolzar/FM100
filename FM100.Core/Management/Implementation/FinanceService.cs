using FM100.Core.GameState;
using FM100.Domain.League;

namespace FM100.Core.Management.Implementation;

public sealed class FinanceService : IFinanceService
{
    public FinanceResult ApplyMatchdayRevenue(GameState.GameState gameState, Fixture fixture, Match match)
    {
        if (!gameState.Clubs.TryGetValue(fixture.HomeClubId, out var homeClub))
        {
            return new FinanceResult { Success = false, Message = "The home club is not available." };
        }

        var existing = gameState.Finances.FirstOrDefault(record =>
            record.Type == "MatchdayRevenue" && record.MatchId == match.Id && record.ClubId == homeClub.Id);
        if (existing != null)
        {
            return new FinanceResult
            {
                Success = true,
                AmountInMillions = existing.AmountInMillions,
                Message = $"Matchday revenue already recorded: EUR {existing.AmountInMillions}M."
            };
        }

        var baseRevenue = homeClub.Stadium.CalculateMatchRevenue();
        var fanMultiplier = 0.75m + homeClub.FanSatisfaction / 40m;
        var conditionMultiplier = 0.75m + homeClub.Stadium.Condition / 40m;
        var revenue = Math.Max(1, (int)Math.Round(baseRevenue * fanMultiplier * conditionMultiplier, MidpointRounding.AwayFromZero));

        homeClub.BudgetInMillions += revenue;
        gameState.Finances.Add(new FinanceRecord
        {
            Season = gameState.CurrentSeason,
            Day = gameState.DaysElapsed,
            Type = "MatchdayRevenue",
            AmountInMillions = revenue,
            MatchId = match.Id,
            ClubId = homeClub.Id,
            ClubName = homeClub.Name,
            Description = $"{homeClub.Name} earned EUR {revenue}M from matchday revenue."
        });
        gameState.LastSavedAt = DateTime.UtcNow;

        return new FinanceResult
        {
            Success = true,
            AmountInMillions = revenue,
            Message = $"Matchday revenue: EUR {revenue}M."
        };
    }
}
