using FM100.Core.Management;
using FM100.Domain.Club;

namespace FM100.UnitTest.Core.Management;

public class ClubSelectionCatalogBuilderTests
{
    [Fact]
    public void Build_RemovesDuplicateClubNamesWithinDivision()
    {
        var older = CreateClub("Juventus", Division.SerieA, reputation: 14, updatedAt: new DateTime(2026, 1, 1));
        var newer = CreateClub("Juventus", Division.SerieA, reputation: 18, updatedAt: new DateTime(2026, 2, 1));
        var differentDivision = CreateClub("Juventus", Division.SerieB, reputation: 10, updatedAt: new DateTime(2026, 3, 1));
        var roma = CreateClub("Roma", Division.SerieA, reputation: 16, updatedAt: new DateTime(2026, 1, 15));

        var result = ClubSelectionCatalogBuilder.Build([older, newer, differentDivision, roma], Division.SerieA);

        Assert.Equal(2, result.Count);
        Assert.Equal(["Juventus", "Roma"], result.Select(club => club.Name));
        Assert.Equal(18, result[0].Reputation);
    }

    private static Club CreateClub(string name, Division division, int reputation, DateTime updatedAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..Math.Min(3, name.Length)].ToUpperInvariant(),
            Division = division,
            City = name,
            Reputation = reputation,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 20000 },
            CreatedAt = updatedAt.AddDays(-1),
            UpdatedAt = updatedAt
        };
}
