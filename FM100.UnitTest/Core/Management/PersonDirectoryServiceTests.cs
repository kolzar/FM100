using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.Personnel;

namespace FM100.UnitTest.Core.Management;

public class PersonDirectoryServiceTests
{
    [Fact]
    public void EnsureDirectory_CreatesEveryRoleForEveryClubAndIsIdempotent()
    {
        var state = CreateState();
        var service = new PersonDirectoryService();

        var created = service.EnsureDirectory(state);
        var repeated = service.EnsureDirectory(state);

        Assert.Equal(state.Clubs.Count * Enum.GetValues<PersonnelRole>().Length, created);
        Assert.Equal(0, repeated);
        Assert.Equal(created, state.Personnel.Count);
        Assert.All(state.Clubs.Values, club => Assert.Equal(Enum.GetValues<PersonnelRole>().Length, club.StaffIds.Count));
        var humanManager = Assert.Single(state.Personnel.Values, person => person.IsHumanManager);
        Assert.Equal("Ada", humanManager.FirstName);
        Assert.Equal("Coach", humanManager.LastName);
        Assert.Equal(PersonnelRole.HeadCoach, humanManager.Role);
    }

    [Fact]
    public void Search_FiltersPlayersStaffExecutivesAndClub()
    {
        var state = CreateState();
        var service = new PersonDirectoryService();
        service.EnsureDirectory(state);
        var home = state.GetPlayerClub()!;

        var playerResults = service.Search(state, "Alex Search", PersonCategory.Players);
        var physios = service.Search(state, "physio", PersonCategory.Staff);
        var homeExecutives = service.Search(state, category: PersonCategory.Executives, clubId: home.Id);

        Assert.Single(playerResults);
        Assert.Equal("Player", playerResults[0].PersonType);
        Assert.Equal(state.Clubs.Count, physios.Count);
        Assert.All(physios, entry => Assert.Equal("Physiotherapist", entry.Role));
        Assert.Equal(5, homeExecutives.Count);
        Assert.All(homeExecutives, entry => Assert.Equal(home.Name, entry.ClubName));
    }

    [Fact]
    public void GetDetail_ReturnsAllPlayerAndStaffPropertyGroups()
    {
        var state = CreateState();
        var service = new PersonDirectoryService();
        service.EnsureDirectory(state);
        var player = state.Players.Values.First();
        var director = state.Personnel.Values.First(person => person.Role == PersonnelRole.SportingDirector);

        var playerDetail = service.GetDetail(state, player.Id)!;
        var directorDetail = service.GetDetail(state, director.Id)!;

        Assert.Contains(playerDetail.Properties, property => property.Group == "Season statistics" && property.Name == "Goals" && property.Value == "7");
        Assert.Contains(playerDetail.Properties, property => property.Group == "Current state" && property.Name == "Morale");
        Assert.Contains(playerDetail.Properties, property => property.Group == "Mental attributes" && property.Name == "Leadership");
        Assert.Contains(directorDetail.Properties, property => property.Group == "Professional attributes" && property.Name == "Negotiation");
        Assert.Contains(directorDetail.Properties, property => property.Name == "Role" && property.Value == "Sporting Director");
    }

    private static GameState CreateState()
    {
        var home = CreateClub("Home FC", Division.SerieA);
        var away = CreateClub("Away FC", Division.SerieB);
        var player = new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = "Alex",
            LastName = "Search",
            Age = 24,
            BirthDate = new DateTime(2002, 3, 5),
            Nationality = "Italian",
            Position = PlayerPosition.Forward,
            Reputation = 14,
            Potential = 17,
            CurrentState = new DynamicState { Morale = 13 },
            MentalAttributes = new MentalAttributes { Leadership = 12 },
            SeasonStats = new PlayerSeasonStats { Appearances = 10, Goals = 7, Assists = 3 }
        };
        home.PlayerIds.Add(player.Id);
        return new GameState
        {
            PlayerClubId = home.Id,
            Manager = new ManagerProfile { Name = "Ada Coach", Nationality = "Italian" },
            Clubs = new Dictionary<Guid, Club> { [home.Id] = home, [away.Id] = away },
            Players = new Dictionary<Guid, FootballPlayer> { [player.Id] = player }
        };
    }

    private static Club CreateClub(string name, Division division) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Abbreviation = name[..3].ToUpperInvariant(),
        Division = division,
        City = name,
        Reputation = 12,
        Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 30_000 }
    };
}
