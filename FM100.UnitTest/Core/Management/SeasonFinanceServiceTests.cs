using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class SeasonFinanceServiceTests
{
    [Fact]
    public void ApplySeasonSettlement_ProcessesEveryClubWithPrizeSponsorAndWages()
    {
        var champion = CreateClub("Champion", budget: 100, reputation: 15, fanSatisfaction: 14);
        var runnerUp = CreateClub("Runner Up", budget: 80, reputation: 10, fanSatisfaction: 10);
        champion.SeasonWins = 8;
        runnerUp.SeasonWins = 4;
        var championPlayer = CreatePlayer(wage: 8);
        var runnerUpPlayer = CreatePlayer(wage: 5);
        champion.PlayerIds.Add(championPlayer.Id);
        runnerUp.PlayerIds.Add(runnerUpPlayer.Id);
        var league = new League
        {
            Season = 3,
            Division = Division.SerieA,
            ClubIds = [champion.Id, runnerUp.Id]
        };
        var gameState = new GameState
        {
            PlayerClubId = champion.Id,
            CurrentSeason = 3,
            Clubs = new Dictionary<Guid, Club>
            {
                [champion.Id] = champion,
                [runnerUp.Id] = runnerUp
            },
            Players = new Dictionary<Guid, FootballPlayer>
            {
                [championPlayer.Id] = championPlayer,
                [runnerUpPlayer.Id] = runnerUpPlayer
            },
            Leagues = new Dictionary<Guid, League> { [league.Id] = league }
        };

        var report = new SeasonFinanceService().ApplySeasonSettlement(gameState);

        Assert.Equal(2, report.ClubsProcessed);
        Assert.Equal(2, gameState.ClubFinanceHistory.Count);
        var championRecord = gameState.ClubFinanceHistory.Single(record => record.ClubId == champion.Id);
        var runnerUpRecord = gameState.ClubFinanceHistory.Single(record => record.ClubId == runnerUp.Id);
        Assert.Equal(1, championRecord.FinalPosition);
        Assert.Equal(2, runnerUpRecord.FinalPosition);
        Assert.True(championRecord.PrizeMoneyInMillions > runnerUpRecord.PrizeMoneyInMillions);
        Assert.Equal(8, championRecord.WageCostInMillions);
        Assert.Equal(100 + championRecord.NetAmountInMillions, champion.BudgetInMillions);
        Assert.Equal(80 + runnerUpRecord.NetAmountInMillions, runnerUp.BudgetInMillions);
        var playerFinance = Assert.Single(gameState.Finances);
        Assert.Equal("SeasonSettlement", playerFinance.Type);
        Assert.Equal(championRecord.NetAmountInMillions, playerFinance.AmountInMillions);
        Assert.Equal(report.TotalSponsorshipInMillions + report.TotalPrizeMoneyInMillions - report.TotalWagesInMillions, report.NetWorldAmountInMillions);
    }

    private static Club CreateClub(string name, int budget, int reputation, int fanSatisfaction)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..3].ToUpperInvariant(),
            City = name,
            Division = Division.SerieA,
            BudgetInMillions = budget,
            Reputation = reputation,
            FanSatisfaction = fanSatisfaction,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 30000 }
        };
    }

    private static FootballPlayer CreatePlayer(int wage)
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Finance",
            LastName = "Player",
            WageInMillions = wage
        };
    }
}
