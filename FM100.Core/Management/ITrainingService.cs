using FM100.Core.GameState;

namespace FM100.Core.Management;

public interface ITrainingService
{
    TrainingReport BuildReport(GameState.GameState gameState);

    TrainingResult SetTrainingFocus(GameState.GameState gameState, TrainingFocus focus, int intensity = 2);
}
