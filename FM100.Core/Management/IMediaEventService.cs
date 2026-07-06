using FM100.Core.GameState;

namespace FM100.Core.Management;

public interface IMediaEventService
{
    MediaEventRecord GetOrCreateCurrentEvent(GameState.GameState gameState);

    MediaBrief BuildBrief(GameState.GameState gameState, MediaEventRecord mediaEvent);

    MediaResponseResult Respond(GameState.GameState gameState, Guid mediaEventId, MediaResponseStyle style);
}
