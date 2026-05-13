using FM100.Domain.Base.Attribute;
using FM100.Domain.FootballPlayer;
using FM100.Core.Performance;
using FM100.Core.Performance.Abstractions;
using FM100.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace FM100.Examples;

/// <summary>
/// Esempio pratico di utilizzo del sistema emotivo e di valutazione delle prestazioni in match.
/// Dimostra l'uso del pattern Dependency Injection per accedere ai servizi di calcolo.
/// </summary>
public class MatchPerformanceExample
{
    private readonly IMatchPerformanceCalculator _performanceCalculator;
    private readonly IEmotionalStabilityCalculator _stabilityCalculator;
    private readonly IDominantEmotionCalculator _emotionCalculator;
    private readonly ISquadStrengthEvaluator _squadEvaluator;

    public MatchPerformanceExample(
        IMatchPerformanceCalculator performanceCalculator,
        IEmotionalStabilityCalculator stabilityCalculator,
        IDominantEmotionCalculator emotionCalculator,
        ISquadStrengthEvaluator squadEvaluator)
    {
        _performanceCalculator = performanceCalculator;
        _stabilityCalculator = stabilityCalculator;
        _emotionCalculator = emotionCalculator;
        _squadEvaluator = squadEvaluator;
    }

    public static void Main()
    {
        // Setup Dependency Injection
        var services = new ServiceCollection();
        services.AddPerformanceServices();
        var serviceProvider = services.BuildServiceProvider();

        // Resolve the example from DI container
        var example = ActivatorUtilities.CreateInstance<MatchPerformanceExample>(serviceProvider);
        example.Run();
    }

    private void Run()
    {
        Console.WriteLine("===== FM100: Sistema Emotivo e Performance Match (con Dependency Injection) =====\n");

        // Scenario: Una partita tra due squadre
        var matchId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        // 1. Setup squadra
        Console.WriteLine("📋 SETUP SQUADRA");
        var teamDynamicState = CreateTeamDynamicState();
        var playerEmotionalStates = CreatePlayerEmotionalStates(matchId, 11);
        var mentalAttributesAverage = CreateAverageMentalAttributes();
        var technicalAttributesAverage = 13;

        Console.WriteLine($"Squadra creata con {playerEmotionalStates.Count} giocatori");
        Console.WriteLine($"Team Cohesion: {teamDynamicState.TeamCohesion}/20");
        Console.WriteLine($"Technical Average: {technicalAttributesAverage}/20\n");

        // 2. Primo tempo - Event 1: Goal
        Console.WriteLine("⚽ EVENTO 1: GOAL (Minuto 25)");
        var goalEvent = new MatchEvent
        {
            EventType = MatchEventType.Goal,
            Minute = 25,
            Description = "Goal bellissimo da fuori area"
        };

        ApplyEventToAllPlayers(playerEmotionalStates, goalEvent, mentalAttributesAverage);
        PrintEmotionalSnapshot(playerEmotionalStates, "Dopo il Goal");

        // 3. Event 2: Goal Conceded
        Console.WriteLine("\n❌ EVENTO 2: GOAL SUBITO (Minuto 35)");
        var goalConcededEvent = new MatchEvent
        {
            EventType = MatchEventType.GoalConceded,
            Minute = 35,
            Description = "Goal con errore di marcatura"
        };

        ApplyEventToAllPlayers(playerEmotionalStates, goalConcededEvent, mentalAttributesAverage);
        PrintEmotionalSnapshot(playerEmotionalStates, "Dopo il Goal Subito");

        // 4. Event 3: Foul Received
        Console.WriteLine("\n🤕 EVENTO 3: FALLO SUBITO (Minuto 42)");
        var foulEvent = new MatchEvent
        {
            EventType = MatchEventType.FoulReceived,
            Minute = 42,
            Description = "Fallo commesso dall'avversario"
        };

        ApplyEventToAllPlayers(playerEmotionalStates, foulEvent, mentalAttributesAverage);
        PrintEmotionalSnapshot(playerEmotionalStates, "Dopo il Fallo");

        // 5. Calcolare performance individuali
        Console.WriteLine("\n👥 PERFORMANCE INDIVIDUALI (Primo Tempo - 45 min)");
        PrintIndividualPerformances(playerEmotionalStates, technicalAttributesAverage, 5);

        // 6. Calcolare indici squadra
        Console.WriteLine("\n📊 INDICI EMOTIVI SQUADRA");
        PrintSquadEmotionalIndices(playerEmotionalStates, teamDynamicState);

        // 7. Valutare forza complessiva
        Console.WriteLine("\n🏆 VALUTAZIONE FORZA SQUADRA");
        PrintSquadStrengthEvaluation(
            playerEmotionalStates,
            teamDynamicState,
            mentalAttributesAverage,
            technicalAttributesAverage,
            45);

        // 8. Secondo Tempo - Event 4: Yellow Card
        Console.WriteLine("\n🟨 EVENTO 4: CARTELLINO GIALLO (Minuto 62)");
        var yellowCardEvent = new MatchEvent
        {
            EventType = MatchEventType.YellowCard,
            Minute = 62,
            Description = "Cartellino giallo per protesta"
        };

        ApplyEventToAllPlayers(playerEmotionalStates, yellowCardEvent, mentalAttributesAverage);
        PrintEmotionalSnapshot(playerEmotionalStates, "Dopo Cartellino Giallo");

        // 9. Event 5: Another Goal (We score)
        Console.WriteLine("\n⚽ EVENTO 5: GOAL DI PAREGGIO (Minuto 70)");
        var equalizer = new MatchEvent
        {
            EventType = MatchEventType.Goal,
            Minute = 70,
            Description = "Pareggio meritato"
        };

        ApplyEventToAllPlayers(playerEmotionalStates, equalizer, mentalAttributesAverage);
        PrintEmotionalSnapshot(playerEmotionalStates, "Dopo il Pareggio");

        // 10. Final evaluation
        Console.WriteLine("\n🎯 VALUTAZIONE FINALE (Fine Match - 90 min)");
        PrintSquadStrengthEvaluation(
            playerEmotionalStates,
            teamDynamicState,
            mentalAttributesAverage,
            technicalAttributesAverage,
            90);

        // 11. Match statistics
        Console.WriteLine("\n📈 STATISTICHE MATCH");
        PrintMatchStatistics(playerEmotionalStates, mentalAttributesAverage);

        Console.WriteLine("\n===== Fine Simulazione =====");
    }

    private static DynamicState CreateTeamDynamicState()
    {
        return new DynamicState
        {
            Happiness = 10,
            Anger = 9,
            Fear = 8,
            Sadness = 8,
            Anxiety = 9,
            Morale = 12,
            Confidence = 13,
            Stress = 8,
            Fatigue = 2,
            TeamCohesion = 14,
            CoachRelationship = 13
        };
    }

    private static List<MatchEmotionalState> CreatePlayerEmotionalStates(Guid matchId, int playerCount)
    {
        var states = new List<MatchEmotionalState>();
        for (int i = 0; i < playerCount; i++)
        {
            states.Add(new MatchEmotionalState
            {
                PlayerId = Guid.NewGuid(),
                MatchId = matchId,
                Happiness = 10,
                Anger = 9,
                Fear = 8,
                Sadness = 8,
                Anxiety = 9,
                Focus = 12,
                Determination = 11,
                Motivation = 12,
                Confidence = 11
            });
        }
        return states;
    }

    private static MentalAttributes CreateAverageMentalAttributes()
    {
        return new MentalAttributes
        {
            Composure = 12,
            Concentration = 12,
            Leadership = 13,
            Courage = 13,
            Aggression = 10,
            TacticalIntelligence = 14,
            Resilience = 12,
            Ambition = 13,
            Discipline = 12,
            Loyalty = 11,
            PressureHandling = 12
        };
    }

    private void ApplyEventToAllPlayers(
        List<MatchEmotionalState> playerStates,
        MatchEvent matchEvent,
        MentalAttributes mentalAttributes)
    {
        foreach (var player in playerStates)
        {
            _performanceCalculator.ApplyMatchEvent(player, matchEvent, mentalAttributes);
        }
    }

    private void PrintEmotionalSnapshot(
        List<MatchEmotionalState> playerStates,
        string label)
    {
        Console.WriteLine($"\n  {label}:");
        var firstPlayer = playerStates.First();
        Console.WriteLine($"    Happiness:   {firstPlayer.Happiness}/20");
        Console.WriteLine($"    Anger:       {firstPlayer.Anger}/20");
        Console.WriteLine($"    Fear:        {firstPlayer.Fear}/20");
        Console.WriteLine($"    Sadness:     {firstPlayer.Sadness}/20");
        Console.WriteLine($"    Anxiety:     {firstPlayer.Anxiety}/20");
        Console.WriteLine($"    Stability:   {_stabilityCalculator.Calculate(firstPlayer)}/20");
        Console.WriteLine($"    Motivation:  {firstPlayer.Motivation}/20");
    }

    private static void PrintIndividualPerformances(
        List<MatchEmotionalState> playerStates,
        int technicalAverage,
        int playersToShow)
    {
        Console.WriteLine("\n  Top Performers:");
        var performances = playerStates
            .Take(playersToShow)
            .Select((p, i) => new
            {
                Number = i + 1,
                Performance = MatchPerformanceCalculator.CalculatePlayerPerformanceScore(
                    technicalAverage,
                    p),
                Emotion = DominantEmotionCalculator.Calculate(p),
                Stability = EmotionalStabilityCalculator.Calculate(p)
            })
            .OrderByDescending(x => x.Performance)
            .ToList();

        foreach (var perf in performances)
        {
            Console.WriteLine(
                $"    Giocatore #{perf.Number}: {perf.Performance}/20 " +
                $"(Emotion: {perf.Emotion}, Stability: {perf.Stability}/20)");
        }
    }

    private static void PrintSquadEmotionalIndices(
        List<MatchEmotionalState> playerStates,
        DynamicState teamDynamicState)
    {
        var moraleIndex = MatchPerformanceCalculator.CalculateMoraleIndex(playerStates);
        var squadEmotionalIndex = MatchPerformanceCalculator.CalculateSquadEmotionalIndex(
            playerStates,
            teamDynamicState.TeamCohesion);

        Console.WriteLine($"\n  Morale Index:             {moraleIndex}/20");
        Console.WriteLine($"  Squad Emotional Index:    {squadEmotionalIndex}/20");
        Console.WriteLine($"  Average Happiness:        {playerStates.Average(p => p.Happiness):F1}/20");
        Console.WriteLine($"  Average Fear:             {playerStates.Average(p => p.Fear):F1}/20");
        Console.WriteLine($"  Average Anxiety:          {playerStates.Average(p => p.Anxiety):F1}/20");
    }

    private static void PrintSquadStrengthEvaluation(
        List<MatchEmotionalState> playerStates,
        DynamicState teamDynamicState,
        MentalAttributes mentalAttributes,
        int technicalAverage,
        int matchMinutes)
    {
        var evaluator = new SquadStrengthEvaluator(
            playerStates,
            teamDynamicState,
            mentalAttributes,
            technicalAverage);

        var summary = evaluator.GetPerformanceSummary(matchMinutes);

        Console.WriteLine($"\n  {summary}");
        Console.WriteLine($"\n  Weaknesses:");
        var weaknesses = evaluator.IdentifyWeaknesses();
        if (weaknesses.Count == 0)
        {
            Console.WriteLine("    ✅ No weaknesses identified");
        }
        else
        {
            foreach (var weakness in weaknesses)
            {
                Console.WriteLine($"    ⚠️ {weakness}");
            }
        }

        Console.WriteLine($"\n  Strengths:");
        var strengths = evaluator.IdentifyStrengths();
        if (strengths.Count == 0)
        {
            Console.WriteLine("    No significant strengths at this time");
        }
        else
        {
            foreach (var strength in strengths)
            {
                Console.WriteLine($"    ✅ {strength}");
            }
        }

        var winProb = evaluator.CalculateExpectedWinProbability();
        Console.WriteLine($"\n  Win Probability:          {winProb * 100:F1}%");
    }

    private static void PrintMatchStatistics(
        List<MatchEmotionalState> playerStates,
        MentalAttributes mentalAttributes)
    {
        var avgHappiness = playerStates.Average(p => p.Happiness);
        var avgStability = playerStates.Average(p => EmotionalStabilityCalculator.Calculate(p));
        var maxFear = playerStates.Max(p => p.Fear);
        var minFear = playerStates.Min(p => p.Fear);

        Console.WriteLine($"\n  Average Squad Happiness:  {avgHappiness:F1}/20");
        Console.WriteLine($"  Average Stability:        {avgStability:F1}/20");
        Console.WriteLine($"  Fear Range:               {minFear}-{maxFear}/20");

        var emotionalRanges = playerStates.Select(p => new
        {
            Dominant = DominantEmotionCalculator.Calculate(p),
            ImpactFactor = MatchPerformanceCalculator.CalculateMatchImpactFactor(p)
        }).ToList();

        var mostCommon = emotionalRanges
            .GroupBy(x => x.Dominant)
            .OrderByDescending(g => g.Count())
            .First();

        Console.WriteLine($"  Most Common Emotion:      {mostCommon.Key} ({mostCommon.Count()} players)");
        Console.WriteLine($"  Average Impact Factor:    {emotionalRanges.Average(x => x.ImpactFactor):F1}/20");
    }
}
