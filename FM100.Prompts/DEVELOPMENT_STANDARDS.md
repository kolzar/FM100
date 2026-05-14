# Development Standards & Best Practices

## FM100 Project Standards

This document outlines the development standards and best practices used in the FM100 project.

---

## Code Organization Standards

### Project Structure
```
FM100/
├── FM100.Domain/              → Data models only (no logic)
├── FM100.Core/                → Business logic only (no data models)
├── FM100.UnitTest/            → Tests (organized by component)
├── FM100.Prompts/             → Project documentation
└── Documentation/             → Architecture guides
```

### Folder Naming Conventions
- **PascalCase** for folder names
- **Semantic grouping** (Domain.Attribute, Core.Performance, etc.)
- **Logical hierarchy** reflecting class relationships

### File Naming Conventions
- **PascalCase** for all file names
- **One type per file** (one class, one enum, one interface)
- **File name matches type name** (MatchEmotionalState.cs for MatchEmotionalState class)
- **Plural for collections** (if containing multiple related types, use plural or suffix with "s")

---

## Code Style Standards

### C# Conventions
```csharp
// ✅ DO - Use meaningful names
public int Happiness { get; set; }

// ❌ DON'T - Use abbreviations
public int Hap { get; set; }

// ✅ DO - Use PascalCase for properties
public int EmotionalAttribute { get; set; }

// ❌ DON'T - Use camelCase for properties
public int emotionalAttribute { get; set; }

// ✅ DO - Use braces on new lines
if (condition)
{
	DoSomething();
}

// ❌ DON'T - K&R style
if (condition) {
	DoSomething();
}
```

### Whitespace & Formatting
- **4 spaces for indentation** (not tabs)
- **Line length**: Reasonable (typically 100-120 characters)
- **Blank lines**: Between logical sections
- **No trailing whitespace**

### XML Documentation
```csharp
// ✅ DO - Complete documentation
/// <summary>
/// Player's happiness level (1-20 scale).
/// Affects motivation and performance.
/// </summary>
public int Happiness { get; set; }

// ❌ DON'T - Incomplete or vague
/// <summary>
/// Some value
/// </summary>
public int Value { get; set; }
```

---

## Architectural Principles

### SOLID Principles

#### Single Responsibility
- Each class has **one reason to change**
- Domain classes: Only data
- Core services: Only calculations
- Tests: Only verification

```csharp
// ✅ DO - Single responsibility
public class MatchPerformanceCalculator
{
	public static int CalculatePlayerPerformanceScore(...) { }
}

// ❌ DON'T - Multiple responsibilities
public class MatchService
{
	public void StoreMatch() { }
	public void CalculatePerformance() { }
	public void SendNotification() { }
}
```

#### Open/Closed Principle
- **Open for extension**, closed for modification
- Use interfaces and inheritance
- Avoid changing existing code

```csharp
// ✅ DO - Use interfaces for extension
public interface IPerformanceCalculator
{
	int CalculateScore(...);
}

public class AdvancedCalculator : IPerformanceCalculator
{
	// New implementation without modifying original
}

// ❌ DON'T - Modify existing code
public class PerformanceCalculator
{
	if (useAdvanced)
	{
		// Branching logic added directly
	}
}
```

#### Liskov Substitution
- Derived classes must be substitutable for base
- Interface implementations must follow contract

```csharp
// ✅ DO - Proper implementation
public class MyCalculator : IPerformanceCalculator
{
	public int CalculateScore(int technical, MatchEmotionalState state)
	{
		// Implements contract correctly
		return CalculateScore(technical, state);
	}
}
```

#### Interface Segregation
- Many specific interfaces better than one fat interface
- Clients should depend only on what they use

```csharp
// ✅ DO - Segregated interfaces
public interface IEmotionalCalculator
{
	int Calculate(MatchEmotionalState state);
}

public interface IPerformanceCalculator
{
	int CalculateScore(int technical, MatchEmotionalState state);
}

// ❌ DON'T - Fat interface
public interface IAllCalculations
{
	int CalculateEmotion(...);
	int CalculatePerformance(...);
	int CalculateStability(...);
	// ... many more methods
}
```

#### Dependency Inversion
- Depend on abstractions, not concretions
- High-level modules independent of low-level details

```csharp
// ✅ DO - Depend on interface
public class Application
{
	private readonly IPerformanceCalculator _calculator;

	public Application(IPerformanceCalculator calculator)
	{
		_calculator = calculator;
	}
}

// ❌ DON'T - Depend on concrete class
public class Application
{
	private readonly PerformanceCalculator _calculator 
		= new PerformanceCalculator();
}
```

### Other Principles

#### DRY (Don't Repeat Yourself)
- Avoid code duplication
- Extract common logic

```csharp
// ✅ DO - Extract common logic
private static int ClampValue(int value, int min, int max)
{
	return Math.Max(min, Math.Min(max, value));
}

// ❌ DON'T - Repeat clamping everywhere
var score = Math.Max(1, Math.Min(20, score1));
var stability = Math.Max(1, Math.Min(20, stability1));
```

#### KISS (Keep It Simple, Stupid)
- Simple solutions are better
- Avoid over-engineering
- Readable code is better than clever code

#### YAGNI (You Aren't Gonna Need It)
- Don't add features "just in case"
- Implement only what's needed
- Avoid premature optimization

---

## Domain Layer Standards

### Domain Classes Requirements
- ✅ Properties only, no methods (except ToString for display)
- ✅ Public properties with getters and setters
- ✅ Default values appropriate to 1-20 scale
- ✅ Every property documented with XML
- ✅ Immutable where appropriate (use `init` for set-once properties)

### Example Domain Class
```csharp
namespace FM100.Domain.Base.Attribute;

/// <summary>
/// Represents a player's emotional state during a match.
/// Contains only data attributes with no calculation logic.
/// </summary>
public sealed class MatchEmotionalState
{
	/// <summary>
	/// Unique identifier for the player experiencing this emotional state.
	/// </summary>
	public int PlayerId { get; set; }

	/// <summary>
	/// Unique identifier for the match during which this state was recorded.
	/// </summary>
	public Guid MatchId { get; set; }

	/// <summary>
	/// Player's happiness level (1-20 scale).
	/// Range: 1-5 (Very unhappy), 6-10 (Normal), 11-15 (Happy), 16-20 (Very happy).
	/// </summary>
	public int Happiness { get; set; } = 10;

	// ... more properties
}
```

---

## Core Layer Standards

### Service Classes Requirements
- ✅ Calculation logic only, no data storage
- ✅ Stateless where possible (Singleton candidate)
- ✅ Implement service interfaces
- ✅ Static methods for calculation logic
- ✅ Pure functions (same input = same output)

### Service Interface Requirements
- ✅ Clear, well-defined contracts
- ✅ Method names clearly describe functionality
- ✅ Properly documented with XML
- ✅ Focused on specific responsibility

### Example Service Implementation
```csharp
namespace FM100.Core.Performance;

/// <summary>
/// Calculates player and squad performance metrics.
/// </summary>
public sealed class MatchPerformanceCalculator : IMatchPerformanceCalculator
{
	/// <summary>
	/// Calculates player performance score based on technical ability and emotional state.
	/// </summary>
	/// <param name="technicalAttributesAverage">Player's average technical skill (1-20).</param>
	/// <param name="emotionalState">Player's current emotional state.</param>
	/// <returns>Performance score (1-20).</returns>
	public static int CalculatePlayerPerformanceScore(
		int technicalAttributesAverage,
		MatchEmotionalState emotionalState)
	{
		var emotionalModifier = CalculateEmotionalModifier(emotionalState);
		var performanceScore = (technicalAttributesAverage + emotionalModifier) / 2;
		return ClampValue(performanceScore, 1, 20);
	}

	// ... more methods

	private static int ClampValue(int value, int min, int max)
	{
		return Math.Max(min, Math.Min(max, value));
	}
}
```

---

## Unit Testing Standards

### Test Class Organization
- One test file per domain/service class
- Test class name: `{ClassName}Tests`
- Test project mirrors production project structure

### Test Method Naming
```
{Method}_{Condition}_{ExpectedResult}

Example:
CalculatePlayerPerformanceScore_WithGoodEmotions_ReturnsHighScore
ApplyMatchEvent_GoalScored_IncreasesHappiness
```

### Test Structure (Arrange-Act-Assert)
```csharp
[Fact]
public void CalculateScore_WithHighHappiness_ReturnsHighScore()
{
	// Arrange - Setup test data
	var emotionalState = new MatchEmotionalState 
	{ 
		Happiness = 18, 
		Anxiety = 3 
	};
	var technicalAverage = 15;

	// Act - Perform the action
	var score = MatchPerformanceCalculator.CalculatePlayerPerformanceScore(
		technicalAverage, 
		emotionalState);

	// Assert - Verify the result
	Assert.True(score > 10);
}
```

### Test Requirements
- ✅ Meaningful test names
- ✅ Single assertion per test (or related assertions)
- ✅ Clear Arrange-Act-Assert structure
- ✅ Test both success and edge cases
- ✅ Use appropriate assertions

---

## Dependency Injection Standards

### Service Registration
```csharp
public static IServiceCollection AddPerformanceServices(this IServiceCollection services)
{
	// Singletons for stateless services
	services.AddSingleton<IEmotionalStabilityCalculator, EmotionalStabilityCalculator>();
	services.AddSingleton<IDominantEmotionCalculator, DominantEmotionCalculator>();
	services.AddSingleton<IMatchPerformanceCalculator, MatchPerformanceCalculator>();

	// Scoped for services with state
	services.AddScoped<ISquadStrengthEvaluator, SquadStrengthEvaluator>();

	return services;
}
```

### Lifetime Selection
- **Singleton**: Stateless, CPU-intensive, cached calculations
- **Scoped**: Per-request/per-operation state
- **Transient**: New instance per use (rare in this project)

### Injection Pattern
```csharp
public class MyService
{
	private readonly IPerformanceCalculator _calculator;

	// Constructor injection - always preferred
	public MyService(IPerformanceCalculator calculator)
	{
		_calculator = calculator;
	}
}
```

---

## Documentation Standards

### XML Documentation Requirements
- ✅ Every public member documented
- ✅ Clear, professional language
- ✅ Include value ranges where applicable
- ✅ Explain impact and usage
- ✅ Use proper XML tags

### Comment Types
- **Summary** (`<summary>`): What does this do?
- **Remarks** (`<remarks>`): Additional context
- **Param** (`<param>`): Parameter descriptions
- **Returns** (`<returns>`): Return value description
- **Example** (`<example>`): Usage example

---

## Error Handling Standards

### Exception Handling
```csharp
// ✅ DO - Specific exception handling
try
{
	var result = _calculator.Calculate(state);
}
catch (ArgumentNullException ex)
{
	Console.WriteLine($"Invalid state: {ex.Message}");
}

// ❌ DON'T - Swallow all exceptions
try
{
	var result = _calculator.Calculate(state);
}
catch { }

// ❌ DON'T - Generic catch-all
try
{
	var result = _calculator.Calculate(state);
}
catch (Exception ex)
{
	// What went wrong?
}
```

### Parameter Validation
```csharp
// ✅ DO - Validate parameters
public int Calculate(MatchEmotionalState state)
{
	if (state == null)
		throw new ArgumentNullException(nameof(state));

	// Calculation logic
}

// ❌ DON'T - Skip validation
public int Calculate(MatchEmotionalState state)
{
	return state.Happiness + state.Anger; // May crash if null
}
```

---

## Performance Standards

### Acceptable Performance
- Build time: < 5 seconds
- Test execution: < 1 second for full suite
- Memory usage: Minimal, no leaks
- No unnecessary allocations

### Optimization Strategy
- Measure first (use profiler)
- Don't optimize prematurely
- Focus on algorithmic efficiency
- Cache expensive calculations if needed

---

## Version Control Standards

### Commit Messages
```
Format: {Type}: {Description}

Type: feat (feature), fix (bug fix), docs (documentation), refactor, test, chore

Examples:
feat: Add emotional stability calculator
fix: Correct fatigue impact calculation
docs: Update architecture documentation
refactor: Simplify performance score calculation
test: Add edge case tests for emotions
```

### Branch Naming
```
{Type}/{Feature}

Examples:
feature/emotional-system
fix/fatigue-bug
docs/architecture-guide
```

---

## Security Standards

### Input Validation
- ✅ Validate all external input
- ✅ Use strong typing
- ✅ Check ranges and constraints

### Data Protection
- ✅ No sensitive data in logs
- ✅ Secure defaults
- ✅ Validate data integrity

---

## Refactoring Standards

When refactoring:
1. **Don't change behavior** - Refactoring preserves functionality
2. **Add tests first** - Ensure coverage before refactoring
3. **Make small changes** - Refactor incrementally
4. **Run tests often** - Verify after each change
5. **Document changes** - Update comments if logic changes

---

## Code Review Checklist

Before submitting code:
- ✅ Follows naming conventions
- ✅ Follows SOLID principles
- ✅ Contains proper documentation
- ✅ Tests are comprehensive
- ✅ Code is readable and maintainable
- ✅ No code duplication
- ✅ Error handling is appropriate
- ✅ Performance is acceptable
- ✅ Security concerns addressed

---

## Tools & Technologies

- **Language**: C# 13
- **Framework**: .NET 10
- **Testing**: xUnit
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **IDE**: Visual Studio 2026
- **Version Control**: Git

---

## Continuous Improvement

This standards document should be:
- Reviewed regularly
- Updated as standards evolve
- Discussed during code reviews
- Applied consistently across project

---

## Related Documents

- See **ARCHITECTURE.md** for system design
- See **DI_SETUP_GUIDE.md** for DI patterns
- See **COMPLETION_CHECKLIST.md** for requirements
