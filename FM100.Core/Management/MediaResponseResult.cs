using FM100.Core.GameState;

namespace FM100.Core.Management;

public class MediaResponseResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public MediaEventRecord? Event { get; init; }
}
