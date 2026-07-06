using FM100.Core.GameState;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.UnitTest.Core.Management;

public class TacticalPlanningServiceTests
{
    [Fact]
    public void PrepareAiPlans_StrongFreshFavoriteAttacksButNeverOverwritesHumanPlan()
    {
        var human = CreateClub("Human", reputation: 10);
        var ai = CreateClub("AI Favorite", reputation: 17);
        var gameState = new GameState
        {
            PlayerClubId = human.Id,
            Clubs = new Dictionary<Guid, Club> { [human.Id] = human, [ai.Id] = ai }
        };
        AddSquad(gameState, human, fatigue: 5, tacticalIntelligence: 12);
        AddSquad(gameState, ai, fatigue: 5, tacticalIntelligence: 14);
        gameState.Lineups[human.Id].Mentality = TacticalMentality.Defensive;
        gameState.Lineups[human.Id].Pressing = PressingIntensity.Low;
        gameState.Lineups[human.Id].Tempo = TempoStyle.Slow;
        var fixture = new Fixture { HomeClubId = human.Id, AwayClubId = ai.Id };

        var plans = new TacticalPlanningService().PrepareAiPlans(gameState, fixture);

        var aiPlan = Assert.Single(plans);
        Assert.Equal(TacticalMentality.Attacking, aiPlan.Mentality);
        Assert.Equal(PressingIntensity.High, aiPlan.Pressing);
        Assert.Equal(TempoStyle.Fast, aiPlan.Tempo);
        Assert.Equal(aiPlan.Mentality, gameState.Lineups[ai.Id].Mentality);
        Assert.Equal(TacticalMentality.Defensive, gameState.Lineups[human.Id].Mentality);
        Assert.Equal(PressingIntensity.Low, gameState.Lineups[human.Id].Pressing);
        Assert.Equal(TempoStyle.Slow, gameState.Lineups[human.Id].Tempo);
    }

    [Fact]
    public void BuildPlan_WeakUnderdogUsesCompactLowLoadApproach()
    {
        var underdog = CreateClub("Underdog", reputation: 7);
        var favorite = CreateClub("Favorite", reputation: 16);
        var gameState = new GameState
        {
            Clubs = new Dictionary<Guid, Club> { [underdog.Id] = underdog, [favorite.Id] = favorite }
        };
        AddSquad(gameState, underdog, fatigue: 8, tacticalIntelligence: 9);
        AddSquad(gameState, favorite, fatigue: 8, tacticalIntelligence: 12);

        var plan = new TacticalPlanningService().BuildPlan(gameState, underdog, favorite, isHome: false);

        Assert.Equal(TacticalMentality.Defensive, plan.Mentality);
        Assert.Equal(PressingIntensity.Low, plan.Pressing);
        Assert.Equal(TempoStyle.Slow, plan.Tempo);
        Assert.Equal("Compact counter", plan.Approach);
    }

    [Fact]
    public void BuildPlan_TiredFavoriteAvoidsHighPressAndFastTempo()
    {
        var favorite = CreateClub("Favorite", reputation: 18);
        var opponent = CreateClub("Opponent", reputation: 10);
        var gameState = new GameState
        {
            Clubs = new Dictionary<Guid, Club> { [favorite.Id] = favorite, [opponent.Id] = opponent }
        };
        AddSquad(gameState, favorite, fatigue: 15, tacticalIntelligence: 15);
        AddSquad(gameState, opponent, fatigue: 8, tacticalIntelligence: 10);

        var plan = new TacticalPlanningService().BuildPlan(gameState, favorite, opponent, isHome: true);

        Assert.Equal(TacticalMentality.Attacking, plan.Mentality);
        Assert.Equal(PressingIntensity.Medium, plan.Pressing);
        Assert.Equal(TempoStyle.Normal, plan.Tempo);
    }

    private static Club CreateClub(string name, int reputation)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..3].ToUpperInvariant(),
            City = name,
            Division = Division.SerieA,
            Reputation = reputation,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 20000 }
        };
    }

    private static void AddSquad(GameState gameState, Club club, int fatigue, int tacticalIntelligence)
    {
        var players = Enumerable.Range(1, 23).Select(index => new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = club.Abbreviation,
            LastName = index.ToString(),
            Position = index <= 3 ? PlayerPosition.Goalkeeper : index <= 10 ? PlayerPosition.Defender : index <= 17 ? PlayerPosition.Midfielder : PlayerPosition.Forward,
            Reputation = club.Reputation,
            Potential = club.Reputation,
            CurrentState = new() { Fatigue = fatigue },
            MentalAttributes = new() { TacticalIntelligence = tacticalIntelligence }
        }).ToList();
        club.PlayerIds = players.Select(player => player.Id).ToList();
        foreach (var player in players)
        {
            gameState.Players[player.Id] = player;
        }

        gameState.Lineups[club.Id] = new TeamLineup
        {
            ClubId = club.Id,
            StartingPlayerIds = players.Take(11).Select(player => player.Id).ToList(),
            SubstitutePlayerIds = players.Skip(11).Select(player => player.Id).ToList()
        };
    }
}
