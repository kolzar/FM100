namespace FM100.Core.Management;

public sealed record TeamTalkHistoryEntry(
    int Season,
    int Day,
    TeamTalkStyle Style,
    int Effectiveness,
    int AffectedPlayers,
    decimal MoraleBefore,
    decimal MoraleAfter,
    decimal MotivationBefore,
    decimal MotivationAfter,
    decimal TrustBefore,
    decimal TrustAfter,
    string Summary);
