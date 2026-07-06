using FM100.Core.GameState;
using FM100.Domain.Club;

namespace FM100.Core.Management.Implementation;

public sealed class SeasonFinanceService : ISeasonFinanceService
{
    public SeasonFinanceReport ApplySeasonSettlement(GameState.GameState gameState)
    {
        var records = new List<ClubFinanceHistoryRecord>();
        foreach (var league in gameState.Leagues.Values.Where(league => league.Season == gameState.CurrentSeason))
        {
            var table = league.ClubIds
                .Select(id => gameState.Clubs.GetValueOrDefault(id))
                .Where(club => club != null)
                .Select(club => club!)
                .OrderByDescending(club => club.GetPoints())
                .ThenByDescending(club => club.GetGoalDifference())
                .ThenByDescending(club => club.GoalsFor)
                .ThenBy(club => club.Name)
                .ToList();

            for (var index = 0; index < table.Count; index++)
            {
                var club = table[index];
                var position = index + 1;
                var sponsorship = GetBaseSponsorship(club.Division) + club.Reputation * 2 + club.FanSatisfaction;
                var prizeMoney = (table.Count - index) * GetPrizeMultiplier(club.Division);
                var wageCost = club.PlayerIds
                    .Select(playerId => gameState.Players.GetValueOrDefault(playerId))
                    .Where(player => player != null)
                    .Sum(player => Math.Max(0, player!.WageInMillions));
                var netAmount = sponsorship + prizeMoney - wageCost;
                club.BudgetInMillions = Math.Max(0, club.BudgetInMillions + netAmount);
                club.UpdatedAt = DateTime.UtcNow;

                var record = new ClubFinanceHistoryRecord
                {
                    Season = gameState.CurrentSeason,
                    ClubId = club.Id,
                    ClubName = club.Name,
                    FinalPosition = position,
                    SponsorshipInMillions = sponsorship,
                    PrizeMoneyInMillions = prizeMoney,
                    WageCostInMillions = wageCost,
                    NetAmountInMillions = netAmount,
                    ClosingBudgetInMillions = club.BudgetInMillions
                };
                gameState.ClubFinanceHistory.Add(record);
                records.Add(record);

                if (club.Id == gameState.PlayerClubId)
                {
                    gameState.Finances.Add(new FinanceRecord
                    {
                        Season = gameState.CurrentSeason,
                        Day = gameState.DaysElapsed,
                        Type = "SeasonSettlement",
                        AmountInMillions = netAmount,
                        ClubId = club.Id,
                        ClubName = club.Name,
                        Description = $"Season settlement: sponsor EUR {sponsorship}M + prize EUR {prizeMoney}M - wages EUR {wageCost}M."
                    });
                }
            }
        }

        return new SeasonFinanceReport(
            records.Count,
            records.Sum(record => record.SponsorshipInMillions),
            records.Sum(record => record.PrizeMoneyInMillions),
            records.Sum(record => record.WageCostInMillions),
            records.Sum(record => record.NetAmountInMillions));
    }

    private static int GetBaseSponsorship(Division division) => division switch
    {
        Division.SerieA => 55,
        Division.SerieB => 35,
        Division.SerieC => 20,
        _ => 20
    };

    private static int GetPrizeMultiplier(Division division) => division switch
    {
        Division.SerieA => 3,
        Division.SerieB => 2,
        Division.SerieC => 1,
        _ => 1
    };
}
