using FM100.Core.GameState;

namespace FM100.Core.Management;

public interface IMediaEventService
{
    MediaEventRecord GetOrCreateCurrentEvent(GameState.GameState gameState);

    MediaResponseResult Respond(GameState.GameState gameState, Guid mediaEventId, MediaResponseStyle style);
}
