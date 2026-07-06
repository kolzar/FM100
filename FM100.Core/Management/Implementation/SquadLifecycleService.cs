using FM100.Core.GameState;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;

namespace FM100.Core.Management.Implementation;

public sealed class SquadLifecycleService : ISquadLifecycleService
{
    private static readonly string[] FirstNames =
        ["Marco", "Luca", "Andrea", "Matteo", "Nico", "Leo", "Gabriel", "Daniel", "Rafael", "Samuel"];
    private static readonly string[] LastNames =
        ["Rossi", "Bianchi", "Costa", "Marino", "Silva", "Moretti", "Romano", "Greco", "Ferri", "Ricci"];

    public SquadLifecycleReport ApplySeasonRollover(GameState.GameState gameState)
    {
        var agedPlayerIds = gameState.Players.Keys.ToList();
        foreach (var playerId in agedPlayerIds)
        {
            var player = gameState.Players[playerId];
            player.Age++;
            if (player.BirthDate != default)
            {
                player.BirthDate = player.BirthDate.AddYears(-1);
            }
        }

        var retirementCount = 0;
        var promotionCount = 0;
        foreach (var club in gameState.Clubs.Values)
        {
            var squad = club.PlayerIds
                .Select(playerId => gameState.Players.GetValueOrDefault(playerId))
                .Where(player => player != null)
                .Select(player => player!)
                .ToList();
            var retiring = squad.Where(ShouldRetire).ToList();
            var replacementPositions = retiring.Select(player => player.Position).ToList();

            foreach (var player in retiring)
            {
                squad.Remove(player);
                club.PlayerIds.Remove(player.Id);
                gameState.Players.Remove(player.Id);
                gameState.TransferMarket.RemoveAll(listing => listing.PlayerId == player.Id);
                gameState.PlayerCareerEvents.Add(new PlayerCareerEventRecord
                {
                    Season = gameState.CurrentSeason,
                    PlayerId = player.Id,
                    ClubId = club.Id,
                    PlayerName = $"{player.FirstName} {player.LastName}".Trim(),
                    ClubName = club.Name,
                    EventType = "Retirement",
                    Age = player.Age,
                    Summary = $"Retired from {club.Name} at age {player.Age} after {player.PlayedMinutes} season minutes."
                });
                retirementCount++;
            }

            while (squad.Count > 23)
            {
                var released = squad
                    .OrderBy(player => player.Reputation + player.Potential)
                    .ThenByDescending(player => player.Age)
                    .First();
                squad.Remove(released);
                club.PlayerIds.Remove(released.Id);
                gameState.Players.Remove(released.Id);
                gameState.TransferMarket.RemoveAll(listing => listing.PlayerId == released.Id);
                gameState.PlayerCareerEvents.Add(new PlayerCareerEventRecord
                {
                    Season = gameState.CurrentSeason,
                    PlayerId = released.Id,
                    ClubId = club.Id,
                    PlayerName = $"{released.FirstName} {released.LastName}".Trim(),
                    ClubName = club.Name,
                    EventType = "Released",
                    Age = released.Age,
                    Summary = $"Released by {club.Name} during the season squad rebuild."
                });
            }

            while (squad.Count < 23)
            {
                var position = replacementPositions.Count > 0
                    ? replacementPositions[0]
                    : GetNeededPosition(squad);
                if (replacementPositions.Count > 0)
                {
                    replacementPositions.RemoveAt(0);
                }

                var youth = CreateAcademyPlayer(club, position, gameState.CurrentSeason, squad.Count);
                var usedShirtNumbers = squad.Select(player => player.ShirtNumber).ToHashSet();
                youth.ShirtNumber = Enumerable.Range(1, 99).First(number => !usedShirtNumbers.Contains(number));
                squad.Add(youth);
                gameState.Players[youth.Id] = youth;
                gameState.PlayerCareerEvents.Add(new PlayerCareerEventRecord
                {
                    Season = gameState.CurrentSeason,
                    PlayerId = youth.Id,
                    ClubId = club.Id,
                    PlayerName = $"{youth.FirstName} {youth.LastName}".Trim(),
                    ClubName = club.Name,
                    EventType = "AcademyPromotion",
                    Age = youth.Age,
                    Summary = $"Promoted from the {club.Name} academy at age {youth.Age}."
                });
                promotionCount++;
            }

            club.PlayerIds = squad.Select(player => player.Id).ToList();
            gameState.Lineups[club.Id] = BuildLineup(club, squad);
        }

        var rosteredPlayerIds = gameState.Clubs.Values
            .SelectMany(club => club.PlayerIds)
            .ToHashSet();
        foreach (var listing in gameState.TransferMarket
                     .Where(listing => listing.IsFreeAgent &&
                                       !rosteredPlayerIds.Contains(listing.PlayerId) &&
                                       gameState.Players.TryGetValue(listing.PlayerId, out var player) &&
                                       ShouldRetire(player))
                     .ToList())
        {
            var player = gameState.Players[listing.PlayerId];
            gameState.TransferMarket.Remove(listing);
            gameState.Players.Remove(player.Id);
            gameState.PlayerCareerEvents.Add(new PlayerCareerEventRecord
            {
                Season = gameState.CurrentSeason,
                PlayerId = player.Id,
                ClubId = Guid.Empty,
                PlayerName = $"{player.FirstName} {player.LastName}".Trim(),
                ClubName = "Free Agents",
                EventType = "Retirement",
                Age = player.Age,
                Summary = $"Retired as a free agent at age {player.Age}."
            });
            retirementCount++;
        }

        return new SquadLifecycleReport(agedPlayerIds.Count, retirementCount, promotionCount);
    }

    private static bool ShouldRetire(FootballPlayer player)
    {
        return player.Age >= 39 ||
               player.Age >= 36 && player.PlayedMinutes < 900 ||
               player.Age >= 34 && player.Reputation <= 4;
    }

    private static FootballPlayer CreateAcademyPlayer(Club club, PlayerPosition position, int season, int slot)
    {
        var random = new Random(HashCode.Combine(club.Id, season, slot, position));
        var age = random.Next(17, 20);
        var reputation = Math.Clamp(club.Reputation + random.Next(-7, -2), 1, 14);
        return new FootballPlayer
        {
            Id = Guid.NewGuid(),
            FirstName = FirstNames[random.Next(FirstNames.Length)],
            LastName = LastNames[random.Next(LastNames.Length)],
            Age = age,
            BirthDate = DateTime.UtcNow.AddYears(-age).AddDays(-random.Next(1, 365)),
            Nationality = "Italian",
            Description = $"Academy graduate at {club.Name}",
            Height = random.Next(170, 198),
            Weight = random.Next(65, 90),
            Position = position,
            Reputation = reputation,
            Potential = Math.Clamp(reputation + random.Next(3, 9), reputation, 20),
            MarketValue = Math.Max(1, reputation * 2),
            WageInMillions = Math.Max(1, reputation / 5),
            ContractExpiresSeason = season + 4,
            CurrentState = new DynamicState
            {
                Happiness = 14,
                Morale = 14,
                Motivation = 16,
                Confidence = 11,
                Fatigue = 2,
                TeamCohesion = 8,
                CoachRelationship = 12
            },
            MentalAttributes = new MentalAttributes
            {
                Composure = random.Next(6, 15),
                TacticalIntelligence = random.Next(6, 15)
            }
        };
    }

    private static PlayerPosition GetNeededPosition(IReadOnlyCollection<FootballPlayer> squad)
    {
        if (squad.Count(player => player.Position == PlayerPosition.Goalkeeper) < 3) return PlayerPosition.Goalkeeper;
        if (squad.Count(player => player.Position == PlayerPosition.Defender) < 7) return PlayerPosition.Defender;
        if (squad.Count(player => player.Position == PlayerPosition.Midfielder) < 7) return PlayerPosition.Midfielder;
        return PlayerPosition.Forward;
    }

    private static TeamLineup BuildLineup(Club club, IReadOnlyCollection<FootballPlayer> squad)
    {
        var starters = new List<FootballPlayer>();
        AddBest(starters, squad, PlayerPosition.Goalkeeper, 1);
        AddBest(starters, squad, PlayerPosition.Defender, 4);
        AddBest(starters, squad, PlayerPosition.Midfielder, 3);
        AddBest(starters, squad, PlayerPosition.Forward, 3);
        var ordered = squad.OrderByDescending(player => player.Reputation)
            .ThenByDescending(player => player.Potential)
            .ToList();
        starters.AddRange(ordered.Where(player => !starters.Contains(player)).Take(11 - starters.Count));

        return new TeamLineup
        {
            ClubId = club.Id,
            Formation = club.Formation,
            StartingPlayerIds = starters.Select(player => player.Id).ToList(),
            SubstitutePlayerIds = ordered.Where(player => !starters.Contains(player)).Take(12).Select(player => player.Id).ToList()
        };
    }

    private static void AddBest(
        ICollection<FootballPlayer> starters,
        IEnumerable<FootballPlayer> squad,
        PlayerPosition position,
        int count)
    {
        foreach (var player in squad.Where(player => player.Position == position)
                     .OrderByDescending(player => player.Reputation)
                     .ThenByDescending(player => player.Potential)
                     .Take(count))
        {
            starters.Add(player);
        }
    }
}
