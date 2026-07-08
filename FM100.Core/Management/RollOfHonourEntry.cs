namespace FM100.Core.Management;

public sealed record RollOfHonourEntry(
    int Season,
    string SerieAChampion,
    string SerieBChampion,
    string SerieCChampion);

public sealed record CupRollOfHonourEntry(
    int Season,
    string SerieACupWinner,
    string SerieBCupWinner,
    string SerieCCupWinner,
    string MasterCupWinner);
