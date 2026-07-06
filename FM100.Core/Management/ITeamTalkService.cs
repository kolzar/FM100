namespace FM100.Core.Management;

public interface ITeamTalkService
{
    TeamTalkResult ApplyTeamTalk(GameState.GameState gameState, TeamTalkStyle style);

    SquadDynamicsReport BuildSquadDynamicsReport(GameState.GameState gameState);
}
