namespace FM100.Core.Management.Implementation;

public class TeamTalkService : ITeamTalkService
{
    public TeamTalkResult ApplyTeamTalk(GameState.GameState gameState, TeamTalkStyle style)
    {
        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return Failed("No player club is available.");
        }

        var players = playerClub.PlayerIds
            .Select(playerId => gameState.Players.TryGetValue(playerId, out var player) ? player : null)
            .Where(player => player != null)
            .Select(player => player!)
            .ToList();

        if (players.Count == 0)
        {
            return Failed("No squad players are available.");
        }

        foreach (var player in players)
        {
            ApplyStyle(player.CurrentState, style);
        }

        gameState.LastSavedAt = DateTime.UtcNow;

        return new TeamTalkResult
        {
            Success = true,
            Message = BuildMessage(style, players.Count),
            AffectedPlayers = players.Count,
            AverageMorale = Math.Round((decimal)players.Average(player => player.CurrentState.Morale), 1),
            AverageMotivation = Math.Round((decimal)players.Average(player => player.CurrentState.Motivation), 1)
        };
    }

    private static void ApplyStyle(FM100.Domain.Base.Attribute.DynamicState state, TeamTalkStyle style)
    {
        switch (style)
        {
            case TeamTalkStyle.Calm:
                state.Morale = Clamp(state.Morale + 1);
                state.Motivation = Clamp(state.Motivation + 1);
                state.Anxiety = Clamp(state.Anxiety - 2);
                state.Stress = Clamp(state.Stress - 2);
                state.Fear = Clamp(state.Fear - 1);
                state.CoachRelationship = Clamp(state.CoachRelationship + 1);
                break;
            case TeamTalkStyle.FireUp:
                state.Morale = Clamp(state.Morale + 1);
                state.Motivation = Clamp(state.Motivation + 3);
                state.Confidence = Clamp(state.Confidence + 2);
                state.Anger = Clamp(state.Anger + 1);
                state.Stress = Clamp(state.Stress + 1);
                break;
            default:
                state.Morale = Clamp(state.Morale + 2);
                state.Motivation = Clamp(state.Motivation + 2);
                state.Confidence = Clamp(state.Confidence + 1);
                state.CoachRelationship = Clamp(state.CoachRelationship + 1);
                break;
        }

        state.LastUpdated = DateTime.UtcNow;
    }

    private static string BuildMessage(TeamTalkStyle style, int affectedPlayers)
    {
        var label = style switch
        {
            TeamTalkStyle.Calm => "Calm talk",
            TeamTalkStyle.FireUp => "Fire-up talk",
            _ => "Balanced talk"
        };

        return $"{label} affected {affectedPlayers} players.";
    }

    private static int Clamp(int value)
    {
        return Math.Clamp(value, 1, 20);
    }

    private static TeamTalkResult Failed(string message)
    {
        return new TeamTalkResult { Success = false, Message = message };
    }
}
