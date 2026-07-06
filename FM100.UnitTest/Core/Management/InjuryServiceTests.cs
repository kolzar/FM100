using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class InjuryServiceTests
{
    [Fact]
    public void EvaluateMatchInjury_WithElitePhysio_ReducesSevereRecoveryTime()
    {
        var club = CreateClub();
        var standardPlayer = CreateExhaustedPlayer();
        var elitePlayer = CreateExhaustedPlayer();
        var matchId = Guid.NewGuid();
        var standardState = CreateGameState(club, standardPlayer, physioQuality: 10);
        var eliteClub = CreateClub();
        var eliteState = CreateGameState(eliteClub, elitePlayer, physioQuality: 18);
        var service = new InjuryService();

        var standard = service.EvaluateMatchInjury(
            standardState,
            club,
            standardPlayer,
            new Match { Id = matchId, HomeClubId = club.Id });
        var elite = service.EvaluateMatchInjury(
            eliteState,
            eliteClub,
            elitePlayer,
            new Match { Id = matchId, HomeClubId = eliteClub.Id });

        Assert.NotNull(standard);
        Assert.NotNull(elite);
        Assert.Equal("Severe", standard.Severity);
        Assert.Equal(28, standard.Days);
        Assert.Equal(20, elite.Days);
        Assert.Equal(20, elitePlayer.InjuryDaysRemaining);
        Assert.Single(eliteState.InjuryHistory);
    }

    private static GameState CreateGameState(Club club, FootballPlayer player, int physioQuality)
    {
        club.PlayerIds.Add(player.Id);
        return new GameState
        {
            PlayerClubId = club.Id,
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player },
            Staff = new StaffSetup { PhysioQuality = physioQuality }
        };
    }

    private static Club CreateClub()
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = "Aurora",
            Abbreviation = "AUR",
            City = "Aurora",
            Division = Division.SerieA,
            Stadium = new Stadium { Name = "Aurora Stadium", Capacity = 30000 }
        };
    }

    private static FootballPlayer CreateExhaustedPlayer()
    {
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Injury",
            Age = 31,
            Position = PlayerPosition.Midfielder,
            CurrentState = new() { Fatigue = 20, Morale = 10, Anxiety = 5 }
        };
    }
}
