using FM100.Core.GameState;
using FM100.Domain.Base;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.Personnel;

namespace FM100.Core.Management.Implementation;

public sealed class PersonDirectoryService : IPersonDirectoryService
{
    private static readonly PersonnelRole[] Roles = Enum.GetValues<PersonnelRole>();
    private static readonly string[] FirstNames =
    [
        "Alessandro", "Andrea", "Carlo", "Davide", "Diego", "Enrico", "Fabio", "Gianni",
        "Lorenzo", "Marco", "Matteo", "Paolo", "Roberto", "Simone", "Stefano", "Vincenzo"
    ];
    private static readonly string[] LastNames =
    [
        "Bianchi", "Conti", "De Luca", "Ferrari", "Fontana", "Galli", "Greco", "Lombardi",
        "Mancini", "Marino", "Moretti", "Ricci", "Rinaldi", "Romano", "Rossi", "Serra"
    ];
    private static readonly string[] Nationalities =
    [
        "Italian", "Spanish", "French", "German", "Portuguese", "Dutch", "English", "Belgian"
    ];

    public int EnsureDirectory(GameState.GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        var created = 0;
        foreach (var club in gameState.Clubs.Values.OrderBy(club => club.Name))
        {
            foreach (var role in Roles)
            {
                if (gameState.Personnel.Values.Any(person => person.ClubId == club.Id && person.Role == role))
                {
                    continue;
                }

                var person = CreatePerson(gameState, club, role);
                gameState.Personnel[person.Id] = person;
                if (!club.StaffIds.Contains(person.Id))
                {
                    club.StaffIds.Add(person.Id);
                }
                created++;
            }
        }

        if (created > 0)
        {
            gameState.LastSavedAt = DateTime.UtcNow;
        }
        return created;
    }

    public IReadOnlyList<PersonSearchEntry> Search(
        GameState.GameState gameState,
        string? searchText = null,
        PersonCategory category = PersonCategory.All,
        Guid? clubId = null,
        int take = 2000)
    {
        EnsureDirectory(gameState);
        var query = (searchText ?? string.Empty).Trim();
        var players = gameState.Players.Values.Select(player => BuildPlayerEntry(gameState, player));
        var personnel = gameState.Personnel.Values.Select(person => BuildPersonnelEntry(gameState, person));
        return players.Concat(personnel)
            .Where(entry => category == PersonCategory.All || entry.Category == category)
            .Where(entry => !clubId.HasValue || ResolveClubId(gameState, entry.PersonId) == clubId)
            .Where(entry => string.IsNullOrWhiteSpace(query) || Matches(entry, query))
            .OrderBy(entry => entry.FullName)
            .ThenBy(entry => entry.ClubName)
            .ThenBy(entry => entry.Role)
            .Take(Math.Max(0, take))
            .ToList();
    }

    public PersonDetail? GetDetail(GameState.GameState gameState, Guid personId)
    {
        if (gameState.Players.TryGetValue(personId, out var player))
        {
            return BuildPlayerDetail(gameState, player);
        }
        return gameState.Personnel.TryGetValue(personId, out var person)
            ? BuildPersonnelDetail(gameState, person)
            : null;
    }

    private static ClubPerson CreatePerson(GameState.GameState gameState, Club club, PersonnelRole role)
    {
        var random = new Random(GetStableSeed(club.Name, role));
        var isHumanManager = club.Id == gameState.PlayerClubId && role == PersonnelRole.HeadCoach;
        var managerName = SplitName(gameState.Manager.Name);
        var age = role switch
        {
            PersonnelRole.Owner or PersonnelRole.President => random.Next(48, 76),
            PersonnelRole.ChiefExecutive or PersonnelRole.SportingDirector or PersonnelRole.TechnicalDirector => random.Next(38, 66),
            _ => random.Next(30, 61)
        };
        var baseAbility = Math.Clamp(club.Reputation + random.Next(-3, 5), 4, 20);
        if (club.Id == gameState.PlayerClubId)
        {
            baseAbility = role switch
            {
                PersonnelRole.Physio => gameState.Staff.PhysioQuality,
                PersonnelRole.ChiefScout or PersonnelRole.Scout => gameState.Staff.ScoutQuality,
                PersonnelRole.HeadCoach or PersonnelRole.AssistantCoach or PersonnelRole.GoalkeepingCoach or PersonnelRole.FitnessCoach => gameState.Staff.CoachQuality,
                _ => baseAbility
            };
        }

        var firstName = isHumanManager ? managerName.FirstName : FirstNames[random.Next(FirstNames.Length)];
        var lastName = isHumanManager ? managerName.LastName : LastNames[random.Next(LastNames.Length)];
        var nationality = isHumanManager ? gameState.Manager.Nationality : Nationalities[random.Next(Nationalities.Length)];
        return new ClubPerson
        {
            Id = Guid.NewGuid(),
            ClubId = club.Id,
            Role = role,
            FirstName = firstName,
            LastName = lastName,
            Age = age,
            BirthDate = DateTime.UtcNow.Date.AddYears(-age).AddDays(-random.Next(1, 365)),
            Nationality = nationality,
            Description = $"{FormatRole(role)} at {club.Name}",
            Height = random.Next(165, 198),
            Weight = random.Next(62, 98),
            Ability = baseAbility,
            Potential = Math.Clamp(baseAbility + random.Next(0, 5), 1, 20),
            Reputation = Math.Clamp(baseAbility + random.Next(-2, 3), 1, 20),
            Leadership = Attribute(random, baseAbility),
            Negotiation = Attribute(random, baseAbility),
            TacticalKnowledge = Attribute(random, role is PersonnelRole.HeadCoach or PersonnelRole.AssistantCoach ? baseAbility + 2 : baseAbility),
            JudgingPlayers = Attribute(random, role is PersonnelRole.ChiefScout or PersonnelRole.Scout or PersonnelRole.SportingDirector ? baseAbility + 2 : baseAbility),
            JudgingPotential = Attribute(random, role is PersonnelRole.ChiefScout or PersonnelRole.Scout ? baseAbility + 2 : baseAbility),
            YouthDevelopment = Attribute(random, baseAbility),
            MedicalKnowledge = Attribute(random, role == PersonnelRole.Physio ? baseAbility + 3 : baseAbility - 3),
            FitnessKnowledge = Attribute(random, role == PersonnelRole.FitnessCoach ? baseAbility + 3 : baseAbility - 2),
            WageInMillions = role is PersonnelRole.Owner or PersonnelRole.President ? 0 : Math.Max(1, baseAbility / 4),
            ContractExpiresSeason = gameState.CurrentSeason + random.Next(1, 5),
            IsHumanManager = isHumanManager,
            CurrentState = new DynamicState
            {
                Happiness = random.Next(8, 18),
                Morale = random.Next(8, 18),
                Motivation = random.Next(8, 18),
                Confidence = random.Next(8, 18),
                Stress = random.Next(4, 15),
                TeamCohesion = random.Next(8, 18),
                CoachRelationship = random.Next(8, 18)
            },
            MentalAttributes = new MentalAttributes
            {
                Composure = Attribute(random, baseAbility),
                Concentration = Attribute(random, baseAbility),
                Leadership = Attribute(random, baseAbility),
                Courage = Attribute(random, baseAbility),
                Aggression = Attribute(random, 10),
                TacticalIntelligence = Attribute(random, baseAbility),
                Resilience = Attribute(random, baseAbility),
                Ambition = Attribute(random, baseAbility),
                Discipline = Attribute(random, baseAbility),
                Loyalty = Attribute(random, 12),
                PressureHandling = Attribute(random, baseAbility)
            }
        };
    }

    private static PersonSearchEntry BuildPlayerEntry(GameState.GameState gameState, FootballPlayer player)
    {
        var club = FindPlayerClub(gameState, player.Id);
        return new PersonSearchEntry(
            player.Id,
            PersonCategory.Players,
            "Player",
            FullName(player),
            FormatRole(player.Position),
            club?.Name ?? "Free Agent",
            club == null ? "-" : FormatDivision(club.Division),
            player.Age,
            player.Nationality,
            player.Reputation,
            player.IsInjured ? $"Injured {player.InjuryDaysRemaining}d" : "Available");
    }

    private static PersonSearchEntry BuildPersonnelEntry(GameState.GameState gameState, ClubPerson person)
    {
        gameState.Clubs.TryGetValue(person.ClubId, out var club);
        return new PersonSearchEntry(
            person.Id,
            IsExecutive(person.Role) ? PersonCategory.Executives : PersonCategory.Staff,
            IsExecutive(person.Role) ? "Executive" : "Staff",
            FullName(person),
            FormatRole(person.Role),
            club?.Name ?? "Unattached",
            club == null ? "-" : FormatDivision(club.Division),
            person.Age,
            person.Nationality,
            person.Reputation,
            person.IsHumanManager ? "Human manager" : $"Contract S{person.ContractExpiresSeason}");
    }

    private static PersonDetail BuildPlayerDetail(GameState.GameState gameState, FootballPlayer player)
    {
        var club = FindPlayerClub(gameState, player.Id);
        var properties = BaseProperties(player, club);
        Add(properties, "Career", "Person type", "Player");
        Add(properties, "Career", "Position", FormatRole(player.Position));
        Add(properties, "Career", "Shirt number", player.ShirtNumber.ToString());
        Add(properties, "Career", "Reputation", Score(player.Reputation));
        Add(properties, "Career", "Potential", Score(player.Potential));
        Add(properties, "Career", "Market value", $"EUR {player.MarketValue}M");
        Add(properties, "Career", "Annual wage", $"EUR {player.WageInMillions}M");
        Add(properties, "Career", "Contract expires", $"Season {player.ContractExpiresSeason}");
        Add(properties, "Availability", "Status", player.IsInjured ? "Injured" : "Available");
        Add(properties, "Availability", "Injury", string.IsNullOrWhiteSpace(player.InjuryDescription) ? "None" : player.InjuryDescription);
        Add(properties, "Availability", "Days remaining", player.InjuryDaysRemaining.ToString());
        Add(properties, "Season statistics", "Appearances", player.SeasonStats.Appearances.ToString());
        Add(properties, "Season statistics", "Minutes", player.SeasonStats.MinutesPlayed.ToString());
        Add(properties, "Season statistics", "Goals", player.SeasonStats.Goals.ToString());
        Add(properties, "Season statistics", "Assists", player.SeasonStats.Assists.ToString());
        Add(properties, "Season statistics", "Yellow cards", player.SeasonStats.YellowCards.ToString());
        Add(properties, "Season statistics", "Red cards", (player.SeasonStats.RedCards + player.SeasonStats.SecondYellowRedCards).ToString());
        Add(properties, "Season statistics", "Average rating", player.SeasonStats.GetAverageRating().ToString());
        AddStateProperties(properties, player.CurrentState);
        AddMentalProperties(properties, player.MentalAttributes);
        return new PersonDetail(player.Id, FullName(player), $"Player | {FormatRole(player.Position)}", club?.Name ?? "Free Agent", properties);
    }

    private static PersonDetail BuildPersonnelDetail(GameState.GameState gameState, ClubPerson person)
    {
        gameState.Clubs.TryGetValue(person.ClubId, out var club);
        var properties = BaseProperties(person, club);
        Add(properties, "Career", "Person type", IsExecutive(person.Role) ? "Executive" : "Staff");
        Add(properties, "Career", "Role", FormatRole(person.Role));
        Add(properties, "Career", "Current ability", Score(person.Ability));
        Add(properties, "Career", "Potential", Score(person.Potential));
        Add(properties, "Career", "Reputation", Score(person.Reputation));
        Add(properties, "Career", "Annual wage", person.WageInMillions == 0 ? "Not applicable" : $"EUR {person.WageInMillions}M");
        Add(properties, "Career", "Contract expires", $"Season {person.ContractExpiresSeason}");
        Add(properties, "Professional attributes", "Leadership", Score(person.Leadership));
        Add(properties, "Professional attributes", "Negotiation", Score(person.Negotiation));
        Add(properties, "Professional attributes", "Tactical knowledge", Score(person.TacticalKnowledge));
        Add(properties, "Professional attributes", "Judging players", Score(person.JudgingPlayers));
        Add(properties, "Professional attributes", "Judging potential", Score(person.JudgingPotential));
        Add(properties, "Professional attributes", "Youth development", Score(person.YouthDevelopment));
        Add(properties, "Professional attributes", "Medical knowledge", Score(person.MedicalKnowledge));
        Add(properties, "Professional attributes", "Fitness knowledge", Score(person.FitnessKnowledge));
        Add(properties, "Professional attributes", "Human manager", person.IsHumanManager ? "Yes" : "No");
        AddStateProperties(properties, person.CurrentState);
        AddMentalProperties(properties, person.MentalAttributes);
        return new PersonDetail(person.Id, FullName(person), $"{(IsExecutive(person.Role) ? "Executive" : "Staff")} | {FormatRole(person.Role)}", club?.Name ?? "Unattached", properties);
    }

    private static List<PersonPropertyEntry> BaseProperties(Person person, Club? club)
    {
        var properties = new List<PersonPropertyEntry>();
        Add(properties, "Identity", "Person ID", person.Id.ToString());
        Add(properties, "Identity", "First name", person.FirstName);
        Add(properties, "Identity", "Last name", person.LastName);
        Add(properties, "Identity", "Age", person.Age.ToString());
        Add(properties, "Identity", "Birth date", person.BirthDate == default ? "-" : person.BirthDate.ToString("dd/MM/yyyy"));
        Add(properties, "Identity", "Nationality", person.Nationality);
        Add(properties, "Identity", "Height", person.Height > 0 ? $"{person.Height} cm" : "-");
        Add(properties, "Identity", "Weight", person.Weight > 0 ? $"{person.Weight} kg" : "-");
        Add(properties, "Identity", "Description", person.Description);
        Add(properties, "Club", "Club", club?.Name ?? "Unattached");
        Add(properties, "Club", "Division", club == null ? "-" : FormatDivision(club.Division));
        Add(properties, "Club", "City", club?.City ?? "-");
        return properties;
    }

    private static void AddStateProperties(ICollection<PersonPropertyEntry> properties, DynamicState state)
    {
        Add(properties, "Current state", "Happiness", Score(state.Happiness));
        Add(properties, "Current state", "Morale", Score(state.Morale));
        Add(properties, "Current state", "Motivation", Score(state.Motivation));
        Add(properties, "Current state", "Confidence", Score(state.Confidence));
        Add(properties, "Current state", "Stress", Score(state.Stress));
        Add(properties, "Current state", "Anxiety", Score(state.Anxiety));
        Add(properties, "Current state", "Fatigue", Score(state.Fatigue));
        Add(properties, "Current state", "Team cohesion", Score(state.TeamCohesion));
        Add(properties, "Current state", "Coach relationship", Score(state.CoachRelationship));
    }

    private static void AddMentalProperties(ICollection<PersonPropertyEntry> properties, MentalAttributes attributes)
    {
        Add(properties, "Mental attributes", "Composure", Score(attributes.Composure));
        Add(properties, "Mental attributes", "Concentration", Score(attributes.Concentration));
        Add(properties, "Mental attributes", "Leadership", Score(attributes.Leadership));
        Add(properties, "Mental attributes", "Courage", Score(attributes.Courage));
        Add(properties, "Mental attributes", "Aggression", Score(attributes.Aggression));
        Add(properties, "Mental attributes", "Tactical intelligence", Score(attributes.TacticalIntelligence));
        Add(properties, "Mental attributes", "Resilience", Score(attributes.Resilience));
        Add(properties, "Mental attributes", "Ambition", Score(attributes.Ambition));
        Add(properties, "Mental attributes", "Discipline", Score(attributes.Discipline));
        Add(properties, "Mental attributes", "Loyalty", Score(attributes.Loyalty));
        Add(properties, "Mental attributes", "Pressure handling", Score(attributes.PressureHandling));
    }

    private static Guid? ResolveClubId(GameState.GameState gameState, Guid personId)
    {
        if (gameState.Personnel.TryGetValue(personId, out var person)) return person.ClubId;
        return FindPlayerClub(gameState, personId)?.Id;
    }

    private static Club? FindPlayerClub(GameState.GameState gameState, Guid playerId) =>
        gameState.Clubs.Values.FirstOrDefault(club => club.PlayerIds.Contains(playerId));

    private static bool Matches(PersonSearchEntry entry, string query) =>
        entry.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        entry.Role.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        entry.ClubName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        entry.Nationality.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        entry.PersonType.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static bool IsExecutive(PersonnelRole role) => role is
        PersonnelRole.SportingDirector or PersonnelRole.TechnicalDirector or
        PersonnelRole.ChiefExecutive or PersonnelRole.President or PersonnelRole.Owner;

    private static string FullName(Person person) => $"{person.FirstName} {person.LastName}".Trim();
    private static string Score(int value) => $"{value}/20";
    private static void Add(ICollection<PersonPropertyEntry> properties, string group, string name, string value) =>
        properties.Add(new PersonPropertyEntry(group, name, value));

    private static int Attribute(Random random, int baseline) => Math.Clamp(baseline + random.Next(-4, 5), 1, 20);

    private static (string FirstName, string LastName) SplitName(string name)
    {
        var parts = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length switch
        {
            0 => ("Human", "Manager"),
            1 => (parts[0], "Manager"),
            _ => (parts[0], string.Join(' ', parts.Skip(1)))
        };
    }

    private static int GetStableSeed(string clubName, PersonnelRole role)
    {
        unchecked
        {
            var hash = 23;
            foreach (var character in $"{clubName}|{role}") hash = hash * 31 + character;
            return hash;
        }
    }

    private static string FormatDivision(Division division) => division switch
    {
        Division.SerieA => "Serie A",
        Division.SerieB => "Serie B",
        _ => "Serie C"
    };

    private static string FormatRole(PlayerPosition position) => position switch
    {
        PlayerPosition.Goalkeeper => "Goalkeeper",
        PlayerPosition.Defender => "Defender",
        PlayerPosition.Midfielder => "Midfielder",
        _ => "Forward"
    };

    private static string FormatRole(PersonnelRole role) => role switch
    {
        PersonnelRole.HeadCoach => "Head Coach",
        PersonnelRole.AssistantCoach => "Assistant Coach",
        PersonnelRole.GoalkeepingCoach => "Goalkeeping Coach",
        PersonnelRole.FitnessCoach => "Fitness Coach",
        PersonnelRole.Physio => "Physiotherapist",
        PersonnelRole.ChiefScout => "Chief Scout",
        PersonnelRole.Scout => "Scout",
        PersonnelRole.SportingDirector => "Sporting Director",
        PersonnelRole.TechnicalDirector => "Technical Director",
        PersonnelRole.ChiefExecutive => "Chief Executive",
        PersonnelRole.President => "President",
        _ => "Owner"
    };
}
