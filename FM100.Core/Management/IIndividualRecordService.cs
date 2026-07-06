namespace FM100.Core.Management;

public interface IIndividualRecordService
{
    IndividualRecordReport UpdateSeasonRecords(GameState.GameState gameState);
}
