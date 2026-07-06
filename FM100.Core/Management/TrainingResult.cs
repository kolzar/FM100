using FM100.Core.GameState;

namespace FM100.Core.Management;

public sealed class TrainingResult
{
    public bool Success { get; init; }
    public TrainingFocus Focus { get; init; }
    public int Intensity { get; init; }
    public string Message { get; init; } = string.Empty;
}
