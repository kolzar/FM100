using FM100.Core.GameState;

namespace FM100.Core.Management.Implementation;

public class MediaEventService : IMediaEventService
{
    public MediaEventRecord GetOrCreateCurrentEvent(GameState.GameState gameState)
    {
        var existing = gameState.MediaEvents
            .Where(mediaEvent => !mediaEvent.IsResolved)
            .OrderByDescending(mediaEvent => mediaEvent.CreatedAt)
            .FirstOrDefault();

        if (existing != null)
        {
            return existing;
        }

        var latestToday = gameState.MediaEvents
            .Where(mediaEvent => mediaEvent.Season == gameState.CurrentSeason && mediaEvent.Day == gameState.DaysElapsed)
            .OrderByDescending(mediaEvent => mediaEvent.CreatedAt)
            .FirstOrDefault();

        if (latestToday != null)
        {
            return latestToday;
        }

        var playerClub = gameState.GetPlayerClub();
        var mediaEvent = new MediaEventRecord
        {
            Season = gameState.CurrentSeason,
            Day = gameState.DaysElapsed,
            Headline = BuildHeadline(playerClub),
            Question = BuildQuestion(playerClub)
        };

        gameState.MediaEvents.Add(mediaEvent);
        gameState.LastSavedAt = DateTime.UtcNow;
        return mediaEvent;
    }

    public MediaResponseResult Respond(GameState.GameState gameState, Guid mediaEventId, MediaResponseStyle style)
    {
        var mediaEvent = gameState.MediaEvents.FirstOrDefault(item => item.Id == mediaEventId);
        if (mediaEvent == null)
        {
            return Failed("Media event is no longer available.");
        }

        if (mediaEvent.IsResolved)
        {
            return Failed("This media event has already been answered.");
        }

        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return Failed("No player club is available.");
        }

        var squad = playerClub.PlayerIds
            .Select(playerId => gameState.Players.TryGetValue(playerId, out var player) ? player : null)
            .Where(player => player != null)
            .Select(player => player!)
            .ToList();

        ApplyResponse(playerClub, squad, style);
        mediaEvent.IsResolved = true;
        mediaEvent.Response = style.ToString();
        mediaEvent.Outcome = BuildOutcome(style);
        mediaEvent.ResolvedAt = DateTime.UtcNow;
        gameState.LastSavedAt = DateTime.UtcNow;

        return new MediaResponseResult
        {
            Success = true,
            Message = mediaEvent.Outcome,
            Event = mediaEvent
        };
    }

    private static string BuildHeadline(FM100.Domain.Club.Club? club)
    {
        if (club == null)
        {
            return "The media is waiting for direction";
        }

        return club.GetMatchesPlayed() == 0
            ? $"{club.Name} face early-season questions"
            : $"{club.Name} under the spotlight";
    }

    private static string BuildQuestion(FM100.Domain.Club.Club? club)
    {
        if (club == null)
        {
            return "How do you want to address the pressure around the club?";
        }

        return club.SeasonLosses > club.SeasonWins
            ? "Supporters are worried by recent results. What message do you send?"
            : "The press wants to know how you will keep momentum and focus.";
    }

    private static void ApplyResponse(
        FM100.Domain.Club.Club club,
        IReadOnlyCollection<FM100.Domain.FootballPlayer.FootballPlayer> squad,
        MediaResponseStyle style)
    {
        switch (style)
        {
            case MediaResponseStyle.ChallengeSquad:
                club.FanSatisfaction = Clamp(club.FanSatisfaction + 1);
                foreach (var player in squad)
                {
                    player.CurrentState.Motivation = Clamp(player.CurrentState.Motivation + 2);
                    player.CurrentState.Stress = Clamp(player.CurrentState.Stress + 1);
                    player.CurrentState.Morale = Clamp(player.CurrentState.Morale - 1);
                    player.CurrentState.LastUpdated = DateTime.UtcNow;
                }

                break;
            case MediaResponseStyle.DeflectPressure:
                club.FanSatisfaction = Clamp(club.FanSatisfaction + 1);
                foreach (var player in squad)
                {
                    player.CurrentState.Stress = Clamp(player.CurrentState.Stress - 2);
                    player.CurrentState.Anxiety = Clamp(player.CurrentState.Anxiety - 1);
                    player.CurrentState.LastUpdated = DateTime.UtcNow;
                }

                break;
            default:
                foreach (var player in squad)
                {
                    player.CurrentState.Morale = Clamp(player.CurrentState.Morale + 1);
                    player.CurrentState.CoachRelationship = Clamp(player.CurrentState.CoachRelationship + 1);
                    player.CurrentState.Stress = Clamp(player.CurrentState.Stress - 1);
                    player.CurrentState.LastUpdated = DateTime.UtcNow;
                }

                break;
        }

        club.UpdatedAt = DateTime.UtcNow;
    }

    private static string BuildOutcome(MediaResponseStyle style)
    {
        return style switch
        {
            MediaResponseStyle.ChallengeSquad => "You challenged the squad publicly. Motivation rises, but pressure follows.",
            MediaResponseStyle.DeflectPressure => "You absorbed the pressure and calmed the room.",
            _ => "You protected the squad and strengthened trust inside the dressing room."
        };
    }

    private static int Clamp(int value)
    {
        return Math.Clamp(value, 1, 20);
    }

    private static MediaResponseResult Failed(string message)
    {
        return new MediaResponseResult { Success = false, Message = message };
    }
}
