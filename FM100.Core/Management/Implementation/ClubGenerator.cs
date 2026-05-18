using FM100.Domain.Club;

namespace FM100.Core.Management.Implementation;

/// <summary>
/// Generates realistic football clubs with diverse attributes.
/// </summary>
public class ClubGenerator
{
    private readonly Random _random = new();

    private readonly string[] _stadiumNames = new[]
    {
        "Stadio Olimpico", "San Siro", "Allianz Stadium", "Artemio Franchi",
        "Stadio Tardini", "Stadio Dall'Ara", "Diego Armando Maradona", "Stadio Coliseum",
        "Stadio Friuli", "Stadio Dorico", "Penzo", "Arechi"
    };

    private readonly string[] _italianCities = new[]
    {
        "Rome", "Milan", "Turin", "Florence", "Naples", "Venice", "Bologna",
        "Palermo", "Genoa", "Sampdoria", "Perugia", "Parma", "Piacenza", "Lecce"
    };

    private readonly string[] _clubNames = new[]
    {
        "AS Roma", "AC Milan", "Juventus", "Inter Milano", "Fiorentina", "Lazio",
        "Napoli", "Torino", "Atalanta", "Parma", "Bologna", "Genoa", "Sampdoria",
        "Lecce", "Hellas Verona", "Empoli"
    };

    private readonly Dictionary<Division, (int, int)> _budgetRanges = new()
    {
        { Division.SerieA, (100, 500) },      // 100-500 million
        { Division.SerieB, (30, 150) },       // 30-150 million
        { Division.SerieC, (10, 50) }         // 10-50 million
    };

    /// <summary>
    /// Generates multiple clubs for a division.
    /// </summary>
    public List<Club> GenerateClubsForDivision(Division division, int count = 16)
    {
        var clubs = new List<Club>();
        var (minBudget, maxBudget) = _budgetRanges[division];
        var usedCities = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            string city;
            // Ensure no duplicate cities
            do
            {
                city = _italianCities[_random.Next(_italianCities.Length)];
            } while (usedCities.Contains(city) && usedCities.Count < _italianCities.Length);

            usedCities.Add(city);

            var club = new Club
            {
                Id = Guid.NewGuid(),
                Name = _clubNames[_random.Next(_clubNames.Length)] + $" {i + 1}",
                Abbreviation = GenerateAbbreviation(),
                Division = division,
                City = city,
                BudgetInMillions = _random.Next(minBudget, maxBudget + 1),
                Stadium = GenerateStadium(),
                Reputation = _random.Next(1, 21),
                FanSatisfaction = _random.Next(8, 16),
                Formation = "4-3-3",
                TitlesWon = _random.Next(0, 5),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            clubs.Add(club);
        }

        return clubs;
    }

    /// <summary>
    /// Generates a stadium for a club.
    /// </summary>
    private Stadium GenerateStadium()
    {
        return new Stadium
        {
            Name = _stadiumNames[_random.Next(_stadiumNames.Length)],
            Capacity = (_random.Next(15, 81) * 1000), // 15k-80k capacity
            Condition = _random.Next(12, 20),
            AverageAttendancePercent = _random.Next(45, 96)
        };
    }

    /// <summary>
    /// Generates a random 3-letter abbreviation.
    /// </summary>
    private string GenerateAbbreviation()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Range(0, 3)
            .Select(_ => chars[_random.Next(chars.Length)])
            .ToArray());
    }
}
