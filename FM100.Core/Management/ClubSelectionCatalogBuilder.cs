using FM100.Domain.Club;

namespace FM100.Core.Management;

public static class ClubSelectionCatalogBuilder
{
    public static IReadOnlyList<Club> Build(IEnumerable<Club> clubs, Division division)
    {
        ArgumentNullException.ThrowIfNull(clubs);

        return clubs
            .Where(club => club.Division == division)
            .GroupBy(club => club.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(club => club.UpdatedAt)
                .ThenByDescending(club => club.CreatedAt)
                .ThenByDescending(club => club.Reputation)
                .First())
            .OrderBy(club => club.Name)
            .ToList();
    }
}
