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
        "Stadio Friuli", "Stadio Dorico", "Penzo", "Arechi", "Etihad Stadium",
        "Allianz Arena", "Anfield", "Parc des Princes", "San Siro", "Wanda Metropolitano",
        "Estádio da Luz", "Stadio Giuseppe Meazza", "Stamford Bridge", "Signal Iduna Park"
    };

    // Serie A clubs (Top tier - 20 clubs)
    private readonly string[] _serieAClubs = new[]
    {
        "Real Madrid", "Manchester City", "Bayern Monaco", "Liverpool",
        "Paris Saint-Germain", "Inter", "Chelsea", "Borussia Dortmund",
        "Roma", "Barcellona", "Manchester United", "Arsenal",
        "Benfica", "Atalanta", "Atlético Madrid", "Porto",
        "RB Lipsia", "Milan", "Siviglia", "Juventus"
    };

    // Serie B clubs (Second tier - 16 clubs)
    private readonly string[] _serieBClubs = new[]
    {
        "Napoli", "Lazio", "Ajax", "Sporting CP",
        "Fiorentina", "Club Brugge", "Villarreal", "Real Sociedad",
        "Eintracht Francoforte", "PSV Eindhoven", "Shakhtar Donetsk", "Salisburgo",
        "Dinamo Zagabria", "Lille", "Galatasaray", "Olympique Marsiglia"
    };

    // Serie C clubs (Third tier - 12 clubs)
    private readonly string[] _serieCClubs = new[]
    {
        "Rangers", "Fenerbahçe", "Braga", "Celtic",
        "Union Saint-Gilloise", "PAOK", "Olympiacos", "Basilea",
        "Young Boys", "Copenaghen", "Bodo/Glimt", "Aston Villa"
    };

    private readonly string[] _europeanCities = new[]
    {
        "Madrid", "Manchester", "Munich", "Liverpool", "Paris", "Milan", "London",
        "Dortmund", "Rome", "Barcelona", "Amsterdam", "Porto", "Leipzig", "Seville",
        "Lisbon", "Brussels", "Bilbao", "Naples", "Turin", "Venice", "Athens",
        "Frankfurt", "Eindhoven", "Kyiv", "Salzburg", "Zagreb", "Lille", "Istanbul",
        "Marseille", "Glasgow", "Prague", "Prague", "Warsaw", "Basel", "Bern",
        "Copenhagen", "Gliwice", "Birmingham"
    };

    private readonly Dictionary<Division, (int, int)> _budgetRanges = new()
    {
        { Division.SerieA, (200, 800) },      // 200-800 million (top clubs)
        { Division.SerieB, (80, 250) },       // 80-250 million (mid clubs)
        { Division.SerieC, (30, 100) }        // 30-100 million (lower clubs)
    };

    /// <summary>
    /// Generates multiple clubs for a division.
    /// </summary>
    public List<Club> GenerateClubsForDivision(Division division, int count = 16)
    {
        var clubs = new List<Club>();
        var (minBudget, maxBudget) = _budgetRanges[division];
        var usedCities = new HashSet<string>();

        // Get the club names for this division
        var clubNames = division switch
        {
            Division.SerieA => _serieAClubs,
            Division.SerieB => _serieBClubs,
            Division.SerieC => _serieCClubs,
            _ => _serieAClubs
        };

        // Generate clubs from the specific division pool
        var clubsToGenerate = clubNames.Take(count).ToList();

        foreach (var clubName in clubsToGenerate)
        {
            string city;
            // Ensure no duplicate cities
            do
            {
                city = _europeanCities[_random.Next(_europeanCities.Length)];
            } while (usedCities.Contains(city) && usedCities.Count < _europeanCities.Length);

            usedCities.Add(city);

            var club = new Club
            {
                Id = Guid.NewGuid(),
                Name = clubName,
                Abbreviation = GenerateAbbreviation(clubName),
                Division = division,
                City = city,
                BudgetInMillions = _random.Next(minBudget, maxBudget + 1),
                Stadium = GenerateStadium(),
                Reputation = division switch
                {
                    Division.SerieA => _random.Next(12, 20),  // Top clubs: higher reputation
                    Division.SerieB => _random.Next(8, 15),   // Mid clubs
                    Division.SerieC => _random.Next(4, 12),   // Lower clubs
                    _ => _random.Next(1, 20)
                },
                FanSatisfaction = _random.Next(8, 16),
                Formation = "4-3-3",
                TitlesWon = division switch
                {
                    Division.SerieA => _random.Next(0, 8),
                    Division.SerieB => _random.Next(0, 3),
                    Division.SerieC => _random.Next(0, 1),
                    _ => 0
                },
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
    /// Generates a 2-3 letter abbreviation from club name.
    /// </summary>
    private string GenerateAbbreviation(string clubName)
    {
        // Extract first letters from the club name
        var parts = clubName.Split(' ');
        var abbr = string.Concat(parts.Select(p => p.FirstOrDefault()));

        // Ensure it's 2-3 characters
        if (abbr.Length > 3)
            abbr = abbr.Substring(0, 3);
        else if (abbr.Length < 2)
            abbr = clubName.Substring(0, Math.Min(2, clubName.Length));

        return abbr.ToUpper();
    }
}
