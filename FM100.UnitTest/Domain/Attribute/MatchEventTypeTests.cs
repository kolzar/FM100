using Xunit;
using FM100.Domain.Base.Attribute;

namespace FM100.UnitTest.Domain.Attribute;

/// <summary>
/// Unit tests for MatchEventType enum.
/// </summary>
public class MatchEventTypeTests
{
    [Fact]
    public void MatchEventType_HasAllValues()
    {
        // Assert
        Assert.Equal(1, (int)MatchEventType.Goal);
        Assert.Equal(2, (int)MatchEventType.GoalConceded);
        Assert.Equal(3, (int)MatchEventType.FoulCommitted);
        Assert.Equal(4, (int)MatchEventType.FoulReceived);
        Assert.Equal(5, (int)MatchEventType.YellowCard);
        Assert.Equal(6, (int)MatchEventType.RedCard);
        Assert.Equal(7, (int)MatchEventType.Save);
        Assert.Equal(8, (int)MatchEventType.Tackle);
        Assert.Equal(9, (int)MatchEventType.Pass);
        Assert.Equal(10, (int)MatchEventType.Dribble);
        Assert.Equal(11, (int)MatchEventType.Shot);
        Assert.Equal(12, (int)MatchEventType.Interception);
        Assert.Equal(13, (int)MatchEventType.Corner);
        Assert.Equal(14, (int)MatchEventType.Substitution);
        Assert.Equal(15, (int)MatchEventType.InjuryIncident);
        Assert.Equal(16, (int)MatchEventType.ControversialDecision);
    }
}
