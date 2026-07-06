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
        var storyline = BuildStoryline(gameState, playerClub);
        var mediaEvent = new MediaEventRecord
        {
            Season = gameState.CurrentSeason,
            Day = gameState.DaysElapsed,
            StorylineKey = storyline.Key,
            StorylineStage = storyline.Stage,
            PressureLevel = storyline.PressureLevel,
            Headline = storyline.Headline,
            Question = storyline.Question
        };
        var brief = BuildBrief(gameState, mediaEvent);
        mediaEvent.RecommendedResponse = brief.RecommendedStyle.ToString();
        mediaEvent.RiskLabel = brief.Risk;

        gameState.MediaEvents.Add(mediaEvent);
        gameState.LastSavedAt = DateTime.UtcNow;
        return mediaEvent;
    }

    public MediaBrief BuildBrief(GameState.GameState gameState, MediaEventRecord mediaEvent)
    {
        var recommended = GetRecommendedStyle(mediaEvent.StorylineKey);
        var risk = mediaEvent.PressureLevel switch
        {
            >= 8 => "High",
            >= 5 => "Elevated",
            _ => "Managed"
        };
        return new MediaBrief(
            recommended,
            risk,
            Math.Clamp(mediaEvent.PressureLevel, 1, 10),
            gameState.Manager.MediaReputation,
            gameState.Manager.BoardConfidence,
            $"Recommended {recommended} | {risk} risk | Media reputation {gameState.Manager.MediaReputation}/20 | Board {gameState.Manager.BoardConfidence}/20");
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

        var effectiveness = CalculateResponseEffectiveness(gameState, mediaEvent, style);
        var reputationBefore = gameState.Manager.MediaReputation;
        var fanBefore = playerClub.FanSatisfaction;
        ApplyResponse(playerClub, squad, style, effectiveness);
        var reputationChange = effectiveness >= 115 ? 1 : effectiveness < 80 ? -1 : 0;
        gameState.Manager.MediaReputation = Clamp(gameState.Manager.MediaReputation + reputationChange);
        var recommended = GetRecommendedStyle(mediaEvent.StorylineKey);
        gameState.Manager.BoardConfidence = Clamp(gameState.Manager.BoardConfidence + (style == recommended ? 1 : effectiveness < 80 ? -1 : 0));
        mediaEvent.IsResolved = true;
        mediaEvent.Response = style.ToString();
        mediaEvent.ResponseEffectiveness = effectiveness;
        mediaEvent.MediaReputationBefore = reputationBefore;
        mediaEvent.MediaReputationAfter = gameState.Manager.MediaReputation;
        mediaEvent.FanSatisfactionBefore = fanBefore;
        mediaEvent.FanSatisfactionAfter = playerClub.FanSatisfaction;
        mediaEvent.RecommendedResponse = recommended.ToString();
        mediaEvent.RiskLabel = BuildBrief(gameState, mediaEvent).Risk;
        mediaEvent.Outcome = BuildOutcome(style, effectiveness, reputationChange);
        mediaEvent.ResolvedAt = DateTime.UtcNow;
        gameState.LastSavedAt = DateTime.UtcNow;

        return new MediaResponseResult
        {
            Success = true,
            Message = mediaEvent.Outcome,
            Event = mediaEvent,
            Effectiveness = effectiveness,
            MediaReputation = gameState.Manager.MediaReputation,
            FanSatisfaction = playerClub.FanSatisfaction
        };
    }

    private static int CalculateResponseEffectiveness(
        GameState.GameState gameState,
        MediaEventRecord mediaEvent,
        MediaResponseStyle style)
    {
        var effectiveness = 100;
        var recommended = GetRecommendedStyle(mediaEvent.StorylineKey);
        effectiveness += style == recommended ? 20 : -15;
        if (gameState.Manager.MediaReputation >= 15) effectiveness += 10;
        if (gameState.Manager.MediaReputation <= 7) effectiveness -= 15;
        if (style == MediaResponseStyle.ChallengeSquad && mediaEvent.PressureLevel >= 7) effectiveness -= 10;
        if (style == MediaResponseStyle.ProtectSquad && mediaEvent.PressureLevel >= 7) effectiveness += 5;
        return Math.Clamp(effectiveness, 50, 130);
    }

    private static MediaResponseStyle GetRecommendedStyle(string storylineKey)
    {
        return storylineKey switch
        {
            "poor-form" => MediaResponseStyle.ChallengeSquad,
            "momentum" => MediaResponseStyle.DeflectPressure,
            _ => MediaResponseStyle.ProtectSquad
        };
    }

    private static MediaStoryline BuildStoryline(GameState.GameState gameState, FM100.Domain.Club.Club? club)
    {
        if (club == null)
        {
            return new MediaStoryline(
                "general",
                1,
                1,
                "The media is waiting for direction",
                "How do you want to address the pressure around the club?");
        }

        var key = DetermineStorylineKey(gameState, club);
        var stage = gameState.MediaEvents
            .Where(mediaEvent => mediaEvent.StorylineKey == key)
            .Select(mediaEvent => mediaEvent.StorylineStage)
            .DefaultIfEmpty(0)
            .Max() + 1;
        var pressureLevel = CalculatePressureLevel(gameState, club, key);
        var continued = stage > 1 ? " continues" : string.Empty;

        return key switch
        {
            "injury-crisis" => new MediaStoryline(
                key,
                stage,
                pressureLevel,
                $"{club.Name} injury crisis{continued}",
                "Unavailable players are shaping the week. How do you keep the squad together?"),
            "contract-tension" => new MediaStoryline(
                key,
                stage,
                pressureLevel,
                $"{club.Name} contract tension{continued}",
                "Several contracts need attention. How do you reassure the dressing room?"),
            "poor-form" => new MediaStoryline(
                key,
                stage,
                pressureLevel,
                $"{club.Name} under pressure{continued}",
                "Supporters are worried by recent results. What message do you send?"),
            "momentum" => new MediaStoryline(
                key,
                stage,
                pressureLevel,
                $"{club.Name} momentum story{continued}",
                "The press wants to know how you will keep standards high."),
            _ => new MediaStoryline(
                key,
                stage,
                pressureLevel,
                club.GetMatchesPlayed() == 0
                    ? $"{club.Name} face early-season questions"
                    : $"{club.Name} under the spotlight",
                "The press wants to know how you will keep momentum and focus.")
        };
    }

    private static string DetermineStorylineKey(GameState.GameState gameState, FM100.Domain.Club.Club club)
    {
        var squad = club.PlayerIds
            .Select(playerId => gameState.Players.TryGetValue(playerId, out var player) ? player : null)
            .Where(player => player != null)
            .Select(player => player!)
            .ToList();
        var injuredCount = squad.Count(player => player.IsInjured);
        var expiringContracts = squad.Count(player => player.ContractExpiresSeason <= gameState.CurrentSeason + 1);

        if (injuredCount >= 3)
        {
            return "injury-crisis";
        }

        if (expiringContracts >= 3)
        {
            return "contract-tension";
        }

        if (club.SeasonLosses > club.SeasonWins)
        {
            return "poor-form";
        }

        return club.SeasonWins >= 3 && club.SeasonLosses == 0
            ? "momentum"
            : "general";
    }

    private static int CalculatePressureLevel(GameState.GameState gameState, FM100.Domain.Club.Club club, string key)
    {
        var squad = club.PlayerIds
            .Select(playerId => gameState.Players.TryGetValue(playerId, out var player) ? player : null)
            .Where(player => player != null)
            .Select(player => player!)
            .ToList();
        var baseline = key switch
        {
            "poor-form" => 4 + Math.Max(0, club.SeasonLosses - club.SeasonWins),
            "injury-crisis" => 3 + squad.Count(player => player.IsInjured),
            "contract-tension" => 3 + squad.Count(player => player.ContractExpiresSeason <= gameState.CurrentSeason + 1),
            "momentum" => 2,
            _ => 1
        };

        return Math.Clamp(baseline, 1, 10);
    }

    private sealed record MediaStoryline(
        string Key,
        int Stage,
        int PressureLevel,
        string Headline,
        string Question);

    private static void ApplyResponse(
        FM100.Domain.Club.Club club,
        IReadOnlyCollection<FM100.Domain.FootballPlayer.FootballPlayer> squad,
        MediaResponseStyle style,
        int effectiveness)
    {
        switch (style)
        {
            case MediaResponseStyle.ChallengeSquad:
                club.FanSatisfaction = Clamp(club.FanSatisfaction + Scale(1, effectiveness));
                foreach (var player in squad)
                {
                    player.CurrentState.Motivation = Clamp(player.CurrentState.Motivation + Scale(2, effectiveness));
                    player.CurrentState.Stress = Clamp(player.CurrentState.Stress + Scale(1, effectiveness));
                    player.CurrentState.Morale = Clamp(player.CurrentState.Morale + Scale(-1, effectiveness));
                    player.CurrentState.LastUpdated = DateTime.UtcNow;
                }

                break;
            case MediaResponseStyle.DeflectPressure:
                club.FanSatisfaction = Clamp(club.FanSatisfaction + Scale(1, effectiveness));
                foreach (var player in squad)
                {
                    player.CurrentState.Stress = Clamp(player.CurrentState.Stress + Scale(-2, effectiveness));
                    player.CurrentState.Anxiety = Clamp(player.CurrentState.Anxiety + Scale(-1, effectiveness));
                    player.CurrentState.LastUpdated = DateTime.UtcNow;
                }

                break;
            default:
                foreach (var player in squad)
                {
                    player.CurrentState.Morale = Clamp(player.CurrentState.Morale + Scale(1, effectiveness));
                    player.CurrentState.CoachRelationship = Clamp(player.CurrentState.CoachRelationship + Scale(1, effectiveness));
                    player.CurrentState.Stress = Clamp(player.CurrentState.Stress + Scale(-1, effectiveness));
                    player.CurrentState.LastUpdated = DateTime.UtcNow;
                }

                break;
        }

        club.UpdatedAt = DateTime.UtcNow;
    }

    private static string BuildOutcome(MediaResponseStyle style, int effectiveness, int reputationChange)
    {
        var response = style switch
        {
            MediaResponseStyle.ChallengeSquad => "You challenged the squad publicly. Motivation rises, but pressure follows.",
            MediaResponseStyle.DeflectPressure => "You absorbed the pressure and calmed the room.",
            _ => "You protected the squad and strengthened trust inside the dressing room."
        };
        var reputation = reputationChange switch
        {
            > 0 => " Media reputation improved.",
            < 0 => " Media reputation declined.",
            _ => string.Empty
        };
        return $"{response} Effectiveness {effectiveness}%.{reputation}";
    }

    private static int Scale(int value, int effectiveness) =>
        value == 0 ? 0 : (int)Math.Round(value * effectiveness / 100m, MidpointRounding.AwayFromZero);

    private static int Clamp(int value)
    {
        return Math.Clamp(value, 1, 20);
    }

    private static MediaResponseResult Failed(string message)
    {
        return new MediaResponseResult { Success = false, Message = message };
    }
}
