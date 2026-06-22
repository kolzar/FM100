namespace FM100.Core.Management.Implementation;

public sealed class HistoryService : IHistoryService
{
    public IReadOnlyList<HistoryTitleEntry> GetTitleHistory(GameState.GameState gameState)
    {
        return gameState.HallOfFame.TitlesByClub
            .Select(entry =>
            {
                var clubName = gameState.Clubs.TryGetValue(entry.Key, out var club)
                    ? club.Name
                    : "Unknown Club";
                var division = gameState.Clubs.TryGetValue(entry.Key, out var knownClub)
                    ? knownClub.Division
                    : Domain.Club.Division.SerieC;

                return new HistoryTitleEntry(clubName, division, entry.Value);
            })
            .Where(entry => entry.Titles > 0)
            .OrderByDescending(entry => entry.Titles)
            .ThenBy(entry => entry.ClubName)
            .ToList();
    }

    public IReadOnlyList<MediaStoryEntry> GetMediaHistory(GameState.GameState gameState, int take = 8)
    {
        return gameState.MediaEvents
            .OrderByDescending(mediaEvent => mediaEvent.CreatedAt)
            .ThenByDescending(mediaEvent => mediaEvent.Season)
            .ThenByDescending(mediaEvent => mediaEvent.Day)
            .Take(Math.Max(0, take))
            .Select(mediaEvent => new MediaStoryEntry(
                mediaEvent.Headline,
                mediaEvent.IsResolved ? mediaEvent.Response : "Awaiting response",
                mediaEvent.IsResolved ? mediaEvent.Outcome : mediaEvent.Question,
                mediaEvent.Season,
                mediaEvent.Day))
            .ToList();
    }
}
