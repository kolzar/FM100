# Sistema Emotivo e Performance Match - FM100

## 📋 Panoramica

Questo sistema gestisce e valuta l'**aspetto emotivo dei giocatori durante una partita di calcio**, con scale da 1-20 e formule matematiche per calcolare prestazioni individuali e della squadra.

## 🎯 Componenti Principali

### 1. **DynamicState** (Stato Dinamico)
Traccia stati emotivi che cambiano durante la partita:
- **Happiness** (1-20): Felicità/soddisfazione
- **Anger** (1-20): Rabbia/frustrazione
- **Fear** (1-20): Paura/ansia
- **Sadness** (1-20): Tristezza
- **Anxiety** (1-20): Ansia/nervosismo
- **Morale** (1-20): Morale generale
- **Confidence** (1-20): Fiducia
- **Stress** (1-20): Livello di stress
- **Fatigue** (1-20): Affaticamento
- **TeamCohesion** (1-20): Coesione di squadra
- **CoachRelationship** (1-20): Relazione con allenatore

### 2. **MatchEmotionalState** (Stato Emotivo di Match)
Traccia le emozioni di un singolo giocatore durante un match:

```csharp
public sealed class MatchEmotionalState
{
	public Guid PlayerId { get; set; }
	public Guid MatchId { get; set; }

	// Primary emotions (1-20)
	public int Happiness { get; set; } = 10;
	public int Anger { get; set; } = 10;
	public int Fear { get; set; } = 10;
	public int Sadness { get; set; } = 10;
	public int Anxiety { get; set; } = 10;

	// Performance modifiers
	public int Focus { get; set; } = 10;
	public int Determination { get; set; } = 10;
	public int Motivation { get; set; } = 10;
	public int Confidence { get; set; } = 10;

	// Methods
	public int CalculateEmotionalStability() // 1-20
	public EmotionalState GetDominantEmotion()
}
```

**Emozioni possibili:**
- `Happy` - Felice
- `Angry` - Arrabbiato
- `Afraid` - Spaventato
- `Sad` - Triste
- `Anxious` - Ansioso
- `Neutral` - Neutrale

### 3. **MatchEvent** (Evento di Partita)
Rappresenta eventi che influenzano le emozioni:

```csharp
public enum MatchEventType
{
	Goal = 1,
	GoalConceded = 2,
	FoulCommitted = 3,
	FoulReceived = 4,
	YellowCard = 5,
	RedCard = 6,
	Save = 7,
	Tackle = 8,
	Pass = 9,
	Dribble = 10,
	Shot = 11,
	Interception = 12,
	Corner = 13,
	Substitution = 14,
	InjuryIncident = 15,
	ControversialDecision = 16
}
```

### 4. **MatchPerformanceCalculator** (Calcolatore Performance Match)
Calcola prestazioni individuali e di squadra con formule matematiche:

#### Metodi Principali:

```csharp
// Performance Score di un singolo giocatore
static int CalculatePlayerPerformanceScore(
	int technicalAttributesAverage,
	MatchEmotionalState emotionalState)
// Scala: 1-20
// Formula: (Technical Average + Emotional Modifier) / 2

// Indice Emotivo della Squadra
static int CalculateSquadEmotionalIndex(
	List<MatchEmotionalState> playerEmotionalStates,
	int teamCohesion)
// Scala: 1-20
// Formula: (Positive Emotions + Team Cohesion) - (Negative Emotions)

// Indice di Morale
static int CalculateMoraleIndex(
	List<MatchEmotionalState> playerEmotionalStates)
// Scala: 1-20
// Formula: 10 + (Happiness - 10) * 0.5 - (Fear - 10) * 0.25 - (Sadness - 10) * 0.25

// Indice di Resistenza alla Pressione
static int CalculatePressureResistanceIndex(
	List<MatchEmotionalState> playerEmotionalStates,
	MentalAttributes mentalAttributesAverage)
// Scala: 1-20
// Formula: (Courage + Resilience + PressureHandling) weighted by Fear/Anxiety

// Match Impact Factor
static int CalculateMatchImpactFactor(MatchEmotionalState state)
// Scala: 1-20
// Formula: (Stability * 0.3) + (Focus * 0.4) + (Motivation * 0.3)

// Applicare evento di match
static void ApplyMatchEvent(
	MatchEmotionalState state,
	MatchEvent matchEvent,
	MentalAttributes mentalAttributes)
```

**Impatto degli eventi sulle emozioni:**

| Evento | Happiness | Anger | Fear | Sadness | Anxiety | Motivation |
|--------|-----------|-------|------|---------|---------|------------|
| Goal | +5 | - | -2 | -3 | - | +3 |
| Goal Conceded | -4 | - | - | +4 | +3 | -2 |
| Foul Received | - | +3 | - | - | - | - |
| Yellow Card | - | - | +3 | - | +5 | -2 |
| Red Card | - | - | +8 | +6 | - | -5 |
| Save | +3 | - | - | - | - | - |
| Tackle | - | - | -1 (se Courage > 12) | - | - | +2 |
| Controversial Decision | - | +4 | - | - | +2 | - |
| Substitution | -3 | - | - | - | +2 | -5 |
| Injury Incident | - | - | +4 | - | +3 | -3 |

### 5. **SquadStrengthEvaluator** (Valutatore Forza Squadra)
Valuta la forza complessiva della squadra durante una partita:

```csharp
public sealed class SquadStrengthEvaluator
{
	// Calcola forza complessiva squadra (1-20)
	public int CalculateOverallSquadStrength()
	// Formula: (Technical + Emotional + Tactical) / 3

	// Forza tecnica (1-20)
	public int CalculateTechnicalStrength()

	// Forza emotiva (1-20)
	public int CalculateEmotionalStrength()
	// Formula: (Morale + Emotional Index + Pressure Resistance) / 3

	// Forza tattica (1-20)
	public int CalculateTacticalStrength()
	// Formula: (Team Cohesion + Leadership + Tactical Intelligence) / 3

	// Potenza offensiva (1-20)
	public int CalculateOffensivePower()

	// Solidità difensiva (1-20)
	public int CalculateDefensiveSolidity()

	// Resilienza mentale (1-20)
	public int CalculateMentalResilience()

	// Affaticamento mentale (1-20)
	public int CalculateMentalFatigue(int matchMinutesElapsed)

	// Identifica debolezze (<8)
	public List<string> IdentifyWeaknesses()

	// Identifica punti di forza (>15)
	public List<string> IdentifyStrengths()

	// Probabilità di vittoria (0.0 - 1.0)
	public double CalculateExpectedWinProbability()

	// Resoconto prestazioni completo
	public SquadPerformanceSummary GetPerformanceSummary(int matchMinutesElapsed = 0)
}
```

## 📊 Esempio di Utilizzo

```csharp
using FM100.Domain.Base.Attribute;
using FM100.Domain.FootballPlayer;

// Creare stato dinamico squadra
var teamDynamicState = new DynamicState
{
	Morale = 14,
	Confidence = 13,
	Stress = 8,
	Fatigue = 3,
	TeamCohesion = 14,
	CoachRelationship = 12
};

// Creare stati emotivi dei giocatori (11 giocatori)
var playerEmotionalStates = new List<MatchEmotionalState>();
for (int i = 0; i < 11; i++)
{
	playerEmotionalStates.Add(new MatchEmotionalState
	{
		PlayerId = Guid.NewGuid(),
		MatchId = matchId,
		Happiness = 12,
		Anger = 9,
		Fear = 8,
		Sadness = 8,
		Anxiety = 9,
		Focus = 13,
		Determination = 12,
		Motivation = 12
	});
}

// Applicare un evento di match (Goal!)
var goalEvent = new MatchEvent
{
	EventType = MatchEventType.Goal,
	Minute = 25,
	Description = "Goal segnato da Kane"
};

var mentalAttributes = new MentalAttributes { Resilience = 12, Courage = 13 };

foreach (var player in playerEmotionalStates)
{
	MatchPerformanceCalculator.ApplyMatchEvent(player, goalEvent, mentalAttributes);
}

// Calcolare performance dei giocatori
var technicalAverage = 13;
foreach (var player in playerEmotionalStates)
{
	var performanceScore = MatchPerformanceCalculator.CalculatePlayerPerformanceScore(
		technicalAverage,
		player);
	Console.WriteLine($"Performance Score: {performanceScore}/20");
}

// Calcolare indici della squadra
var moraleIndex = MatchPerformanceCalculator.CalculateMoraleIndex(playerEmotionalStates);
var squadEmotionalIndex = MatchPerformanceCalculator.CalculateSquadEmotionalIndex(
	playerEmotionalStates,
	teamDynamicState.TeamCohesion);

Console.WriteLine($"Morale Index: {moraleIndex}/20");
Console.WriteLine($"Squad Emotional Index: {squadEmotionalIndex}/20");

// Valutare forza squadra complessiva
var mentalAttributesAverage = new MentalAttributes
{
	Courage = 13,
	Resilience = 12,
	Leadership = 13,
	TacticalIntelligence = 14,
	Discipline = 12,
	Concentration = 12,
	Composure = 12,
	Aggression = 10,
	Ambition = 13,
	Loyalty = 11,
	PressureHandling = 12
};

var evaluator = new SquadStrengthEvaluator(
	playerEmotionalStates,
	teamDynamicState,
	mentalAttributesAverage,
	technicalAverage);

var overallStrength = evaluator.CalculateOverallSquadStrength();
var offensivePower = evaluator.CalculateOffensivePower();
var defensiveSolidity = evaluator.CalculateDefensiveSolidity();

Console.WriteLine($"Overall Strength: {overallStrength}/20");
Console.WriteLine($"Offensive Power: {offensivePower}/20");
Console.WriteLine($"Defensive Solidity: {defensiveSolidity}/20");

// Ottenere resoconto completo
var summary = evaluator.GetPerformanceSummary(45); // Dopo 45 minuti
Console.WriteLine(summary.ToString());

// Identificare debolezze
var weaknesses = evaluator.IdentifyWeaknesses();
foreach (var weakness in weaknesses)
{
	Console.WriteLine($"⚠️ {weakness}");
}

// Probabilità di vittoria
var winProbability = evaluator.CalculateExpectedWinProbability();
Console.WriteLine($"Win Probability: {winProbability * 100:F1}%");
```

## 🧮 Formule Matematiche

### Performance Score Individuale
```
Performance = (Technical Average + Emotional Modifier) / 2

Emotional Modifier = 10
				   + (Happiness - 10) × 0.5  [felicità bonus]
				   + Anger Modifier (±3)      [rabbia]
				   - (Max(0, Fear - 10) × 0.33) [paura penalità]
				   - (Max(0, Anxiety - 10) × 0.33) [ansia penalità]
				   - (Sadness - 10) × 0.3    [tristezza penalità]
				   + Max(0, Focus - 10) × 0.3 [focus bonus]
				   + Max(0, Determination - 10) × 0.2 [determinazione]
```

### Squad Emotional Index
```
Positive Score = (Avg Happiness × 0.4) 
			   + (Avg Determination × 0.3)
			   + (Team Cohesion × 0.1)

Negative Penalty = ((20 - Avg Fear) × 0.2)
				 + ((20 - Avg Anxiety) × 0.2)
				 + ((20 - Avg Sadness) × 0.1)

Index = (Positive Score + Negative Penalty + Anger Impact) / 2
```

### Morale Index
```
Morale = 10 
	   + (Avg Happiness - 10) × 0.5
	   + (20 - Avg Fear) × 0.25
	   + (20 - Avg Sadness) × 0.25
```

### Emotional Stability
```
Average = (Happiness + Anger + Fear + Sadness + Anxiety) / 5
Variance = Σ(Emotion - Average)² / 5

Stability = 20 - (Variance / 66.67) × 19  [clamped 1-20]
```

## 📈 Scale di Valutazione

### Indici Generali (1-20)
- **1-5**: Pessimo
- **6-8**: Debole
- **9-12**: Medio
- **13-15**: Buono
- **16-18**: Eccellente
- **19-20**: Straordinario

### Probabilità di Vittoria
- **0.0 - 0.3**: Molto bassa
- **0.3 - 0.45**: Bassa
- **0.45 - 0.55**: Equilibrata
- **0.55 - 0.7**: Alta
- **0.7 - 1.0**: Molto alta

## 🔄 Ciclo di Vita degli Emozioni

1. **Inizializzazione**: Tutti i giocatori iniziano con emozioni a 10 (neutrale)
2. **Evento Match**: Accade un evento (goal, fallo, cartellino, ecc.)
3. **Applicazione**: L'evento modifica le emozioni secondo le tabelle
4. **Calcolo**: Vengono calcolati i nuovi score di performance
5. **Valutazione**: Si valuta la forza complessiva della squadra
6. **Feedback**: I dati vengono usati per predizioni future

## ⚙️ Integrazione con FootballPlayer

```csharp
public class FootballPlayer : Person
{
	// Stato emotivo attuale in match
	public MatchEmotionalState? CurrentMatchEmotionalState { get; set; }

	// Minuti giocati in questa partita
	public int PlayedMinutes { get; set; }

	// Calcolare performance attuale
	public int GetCurrentMatchPerformanceScore(int technicalAttributesAverage)

	// Emozione dominante
	public EmotionalState GetDominantEmotion()

	// Stabilità emotiva
	public int GetEmotionalStability()
}
```

## 📝 Unit Tests

Sono inclusi 28 unit test completi che verificano:
- ✅ Inizializzazione stati emotivi
- ✅ Calcolo stabilità emotiva
- ✅ Applicazione eventi di match
- ✅ Calcoli performance individuale
- ✅ Indici emotivi squadra
- ✅ Valutazione forza squadra
- ✅ Identificazione debolezze/punti di forza
- ✅ Probabilità di vittoria

Eseguire i test:
```bash
dotnet test FM100.UnitTest
```

## 📚 Riferimenti

- **Scale**: 1-20 (Football Manager style)
- **Emozioni Primarie**: 5 (Happiness, Anger, Fear, Sadness, Anxiety)
- **Modificatori Performance**: 4 (Focus, Determination, Motivation, Confidence)
- **Tipi di Evento**: 16 diversi eventi di match
- **Componenti Squadra**: 3 (Technical, Emotional, Tactical)

---

**Sviluppato per**: FM100 - Simulatore Calcio  
**Versione**: 1.0  
**.NET Target**: 10.0
