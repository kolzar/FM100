namespace FM100.Core.Management;

public sealed record HistoricalWorldGenerationResult(
    int YearsGenerated,
    int TablesGenerated,
    int ChampionsGenerated,
    int StartYear,
    int EndYear);
