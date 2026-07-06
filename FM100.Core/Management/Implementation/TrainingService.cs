using FM100.Core.GameState;

namespace FM100.Core.Management.Implementation;

public sealed class TrainingService : ITrainingService
{
    public TrainingReport BuildReport(GameState.GameState gameState)
    {
        var training = gameState.Training;
        var benefit = training.Focus switch
        {
            TrainingFocus.Fitness => "Motivation and match sharpness",
            TrainingFocus.Tactical => "Confidence and tactical execution",
            TrainingFocus.Recovery => "Lower fatigue and stress",
            TrainingFocus.Youth => "Young player development",
            _ => "Balanced squad preparation"
        };
        var risk = training.Intensity switch
        {
            >= 3 when training.Focus != TrainingFocus.Recovery => "High fatigue load",
            >= 3 => "Low development push",
            1 => "Slow progress",
            _ => "Managed load"
        };
        var load = training.Intensity switch
        {
            1 => "Light",
            3 => "Intense",
            _ => "Standard"
        };
        var sessionsThisSeason = gameState.TrainingHistory.Count(record => record.Season == gameState.CurrentSeason);

        return new TrainingReport(
            training.Focus,
            training.Intensity,
            benefit,
            risk,
            load,
            sessionsThisSeason,
            $"{training.Focus} | {load} load | {sessionsThisSeason} sessions | Benefit: {benefit} | Risk: {risk}");
    }

    public TrainingResult SetTrainingFocus(GameState.GameState gameState, TrainingFocus focus, int intensity = 2)
    {
        var normalizedIntensity = Math.Clamp(intensity, 1, 3);
        gameState.Training.Focus = focus;
        gameState.Training.Intensity = normalizedIntensity;
        gameState.Training.UpdatedAt = DateTime.UtcNow;
        gameState.LastSavedAt = DateTime.UtcNow;

        return new TrainingResult
        {
            Success = true,
            Focus = focus,
            Intensity = normalizedIntensity,
            Message = $"Training set to {focus} intensity {normalizedIntensity}/3."
        };
    }
}
