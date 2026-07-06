namespace FM100.Core.Management;

public sealed record LineupRecommendationResult(
    bool Success,
    int ChangedPlayers,
    int AvailablePlayers,
    string Message);
