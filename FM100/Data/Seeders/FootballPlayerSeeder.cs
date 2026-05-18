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
    private readonly Randomizer _randomizer;

    public FootballPlayerSeeder(IFootballPlayerRepository repository)
    {
        _repository = repository;
        _randomizer = new Randomizer();
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
            .RuleFor(p => p.ShirtNumber, f => f.Random.Int(1, 99).OrNull(f, 0.3f))
            .RuleFor(p => p.Potential, f => f.Random.Int(60, 99))
            .RuleFor(p => p.Reputation, f => f.Random.Int(1, 20))
            .RuleFor(p => p.MarketValue, f => f.Random.Int(1, 100))
            .RuleFor(p => p.CurrentState, f => GenerateDynamicState() ?? new DynamicState())
            .RuleFor(p => p.MentalAttributes, f => GenerateMentalAttributes() ?? new MentalAttributes())
            .RuleFor(p => p.CurrentMatchEmotionalState, f => null);

        return faker.Generate(count) ?? new List<FootballPlayer>();
    }

    /// <summary>
    /// Generates a random DynamicState for a player.
    /// </summary>
    private DynamicState GenerateDynamicState()
    {
        try
        {
            if (_randomizer == null)
            {
                _randomizer = new Randomizer();
            }

            return new DynamicState
            {
                Happiness = _randomizer.Int(1, 20),
                Anger = _randomizer.Int(1, 20),
                Fear = _randomizer.Int(1, 20),
                Sadness = _randomizer.Int(1, 20),
                Anxiety = _randomizer.Int(1, 20),
                Morale = _randomizer.Int(1, 20)
            };
        }
        catch
        {
            return new DynamicState();
        }
    }

    /// <summary>
    /// Generates random MentalAttributes for a player.
    /// </summary>
    private MentalAttributes GenerateMentalAttributes()
    {
        try
        {
            if (_randomizer == null)
            {
                _randomizer = new Randomizer();
            }

            return new MentalAttributes
            {
                Composure = _randomizer.Int(1, 20),
                Concentration = _randomizer.Int(1, 20),
                Leadership = _randomizer.Int(1, 20),
                Courage = _randomizer.Int(1, 20),
                Aggression = _randomizer.Int(1, 20),
                TacticalIntelligence = _randomizer.Int(1, 20),
                Resilience = _randomizer.Int(1, 20),
                Ambition = _randomizer.Int(1, 20),
                Discipline = _randomizer.Int(1, 20),
                Loyalty = _randomizer.Int(1, 20),
                PressureHandling = _randomizer.Int(1, 20)
            };
        }
        catch
        {
            return new MentalAttributes();
        }
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
