using Bogus;
using FM100.Domain.Base.Attribute;
using FM100.Domain.FootballPlayer;
using FM100.Data.Repositories;

namespace FM100.Data.Seeders;

/// <summary>
/// Generates fake football players using Bogus library.
/// </summary>
public class FootballPlayerSeeder
{
    private readonly IFootballPlayerRepository _repository;

    public FootballPlayerSeeder(IFootballPlayerRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Seeds the database with fake football players if empty.
    /// </summary>
    public async Task SeedIfEmptyAsync(int playerCount = 23)
    {
        var existingCount = await _repository.GetCountAsync();

        if (existingCount == 0)
        {
            var players = GeneratePlayersForTeam(playerCount);
            await _repository.AddManyAsync(players);
        }
    }

    /// <summary>
    /// Generates a list of fake football players.
    /// </summary>
    public List<FootballPlayer> GeneratePlayersForTeam(int count = 23)
    {
        var positions = CreatePositionPlan(count);
        var faker = new Faker<FootballPlayer>()
            .RuleFor(p => p.Id, f => Guid.NewGuid())
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.BirthDate, f => f.Date.Past(35, DateTime.Now.AddYears(-17)))
            .RuleFor(p => p.Age, (f, p) => CalculateAge(p.BirthDate))
            .RuleFor(p => p.Nationality, f => f.Address.Country())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence(5))
            .RuleFor(p => p.Height, f => f.Random.Int(168, 200))
            .RuleFor(p => p.Weight, f => f.Random.Int(65, 95))
            .RuleFor(p => p.ShirtNumber, f => f.Random.Int(1, 99).OrNull(f, 0.3f) ?? 0)
            .RuleFor(p => p.Position, f => f.PickRandom<PlayerPosition>())
            .RuleFor(p => p.Potential, f => f.Random.Int(60, 99))
            .RuleFor(p => p.Reputation, f => f.Random.Int(1, 20))
            .RuleFor(p => p.MarketValue, f => f.Random.Int(1, 100))
            .RuleFor(p => p.CurrentState, f => new DynamicState
            {
                Happiness = f.Random.Int(1, 20),
                Anger = f.Random.Int(1, 20),
                Fear = f.Random.Int(1, 20),
                Sadness = f.Random.Int(1, 20),
                Anxiety = f.Random.Int(1, 20),
                Morale = f.Random.Int(1, 20)
            })
            .RuleFor(p => p.MentalAttributes, f => new MentalAttributes
            {
                Composure = f.Random.Int(1, 20),
                Concentration = f.Random.Int(1, 20),
                Leadership = f.Random.Int(1, 20),
                Courage = f.Random.Int(1, 20),
                Aggression = f.Random.Int(1, 20),
                TacticalIntelligence = f.Random.Int(1, 20),
                Resilience = f.Random.Int(1, 20),
                Ambition = f.Random.Int(1, 20),
                Discipline = f.Random.Int(1, 20),
                Loyalty = f.Random.Int(1, 20),
                PressureHandling = f.Random.Int(1, 20)
            })
            .RuleFor(p => p.CurrentMatchEmotionalState, f => null);

        var players = faker.Generate(count) ?? new List<FootballPlayer>();
        for (var i = 0; i < players.Count && i < positions.Count; i++)
        {
            players[i].Position = positions[i];
        }

        return players;
    }

    private static List<PlayerPosition> CreatePositionPlan(int count)
    {
        var plan = new List<PlayerPosition>();
        plan.AddRange(Enumerable.Repeat(PlayerPosition.Goalkeeper, Math.Min(3, count)));
        plan.AddRange(Enumerable.Repeat(PlayerPosition.Defender, Math.Max(0, Math.Min(7, count - plan.Count))));
        plan.AddRange(Enumerable.Repeat(PlayerPosition.Midfielder, Math.Max(0, Math.Min(7, count - plan.Count))));
        plan.AddRange(Enumerable.Repeat(PlayerPosition.Forward, Math.Max(0, count - plan.Count)));
        return plan;
    }

    /// <summary>
    /// Calculates age from birth date.
    /// </summary>
    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }
}
