using FM100.Core.GameState;

namespace FM100.Core.Management;

public sealed record TrainingReport(
    TrainingFocus Focus,
    int Intensity,
    string Benefit,
    string Risk,
    string Load,
    int SessionsThisSeason,
    string Summary);
