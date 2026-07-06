using FM100.Core.GameState;
using FM100.Domain.Base.Attribute;
using FM100.Domain.Club;
using FM100.Domain.FootballPlayer;
using FM100.Domain.League;

namespace FM100.Core.Management.Implementation;

public sealed class InjuryService : IInjuryService
{
    public InjuryOutcome? EvaluateMatchInjury(
        GameState.GameState gameState,
        Club club,
        FootballPlayer player,
        Match match)
    {
        if (player.IsInjured)
        {
            return null;
        }

        var risk = GetRiskPercent(player);
        var roll = GetStableRoll(match.Id, player.Id, salt: 17);
        if (roll >= risk)
        {
            return null;
        }

        var severityRoll = GetStableRoll(match.Id, player.Id, salt: 53);
        var (injuryType, severity, baseDays) = GetInjury(player, severityRoll);
        var physioReduction = club.Id == gameState.PlayerClubId
            ? gameState.Staff.PhysioQuality switch
            {
                >= 18 => 0.7m,
                >= 15 => 0.85m,
                _ => 1m
            }
            : 1m;
        var days = Math.Max(2, (int)Math.Ceiling(baseDays * physioReduction));
        player.InjuryDaysRemaining = days;
        player.InjuryDescription = injuryType;
        player.CurrentState.Morale = Math.Clamp(player.CurrentState.Morale - 1, 1, 20);
        player.CurrentState.Anxiety = Math.Clamp(player.CurrentState.Anxiety + 2, 1, 20);

        var side = club.Id == match.AwayClubId ? "away" : "home";
        match.Events.Add(new MatchEvent
        {
            EventType = MatchEventType.InjuryIncident,
            Minute = 60 + severityRoll % 30,
            Description = $"Injury for {side} team: {player.FirstName} {player.LastName} ({injuryType})",
            EmotionalImpact = -6
        });
        match.Events = match.Events.OrderBy(matchEvent => matchEvent.Minute).ToList();
        gameState.InjuryHistory.Add(new InjuryHistoryRecord
        {
            Season = gameState.CurrentSeason,
            Day = gameState.DaysElapsed,
            PlayerId = player.Id,
            PlayerName = $"{player.FirstName} {player.LastName}".Trim(),
            ClubId = club.Id,
            ClubName = club.Name,
            InjuryType = injuryType,
            Severity = severity,
            InitialDays = days
        });

        return new InjuryOutcome(player.Id, injuryType, severity, days);
    }

    private static int GetRiskPercent(FootballPlayer player)
    {
        var fatigueRisk = player.CurrentState.Fatigue switch
        {
            >= 20 => 100,
            >= 19 => 75,
            >= 17 => 35,
            >= 15 => 15,
            >= 11 => 4,
            _ => 1
        };
        var ageRisk = player.Age switch
        {
            >= 34 => 8,
            >= 30 => 4,
            _ => 0
        };
        return Math.Min(100, fatigueRisk + ageRisk);
    }

    private static (string Type, string Severity, int Days) GetInjury(FootballPlayer player, int roll)
    {
        if (player.CurrentState.Fatigue >= 19 || roll >= 90)
        {
            return ("Hamstring injury", "Severe", 28);
        }

        if (player.CurrentState.Fatigue >= 17 || roll >= 65)
        {
            return ("Muscle strain", "Moderate", 14);
        }

        return ("Match knock", "Minor", 5);
    }

    private static int GetStableRoll(Guid matchId, Guid playerId, int salt)
    {
        return (int)((uint)HashCode.Combine(matchId, playerId, salt) % 100u);
    }
}
