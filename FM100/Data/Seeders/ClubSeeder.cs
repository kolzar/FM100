using FM100.Core.Management.Implementation;
using FM100.Core.Repositories;
using FM100.Domain.Club;

namespace FM100.Data.Seeders;

/// <summary>
/// Seeds the database with clubs if empty.
/// </summary>
public class ClubSeeder
{
    private readonly IClubRepository _repository;
    private readonly ClubGenerator _clubGenerator;

    public ClubSeeder(IClubRepository repository, ClubGenerator clubGenerator)
    {
        _repository = repository;
        _clubGenerator = clubGenerator;
    }

    /// <summary>
    /// Seeds the database with clubs if empty.
    /// </summary>
    public async Task SeedIfEmptyAsync()
    {
        var existingClubs = await _repository.GetAllAsync();

        if (!existingClubs.Any())
        {
            var clubs = new List<Club>();
            foreach (Division division in Enum.GetValues(typeof(Division)))
            {
                var generatedClubs = _clubGenerator.GenerateClubsForDivision(division);
                clubs.AddRange(generatedClubs);
            }

            await _repository.AddManyAsync(clubs);
        }
    }
}
