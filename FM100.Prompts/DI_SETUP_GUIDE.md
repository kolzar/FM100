# Dependency Injection Setup Guide

## Overview

The FM100 project uses **Microsoft.Extensions.DependencyInjection** to manage service dependencies. This guide explains how to set up and use DI in your application.

## Architecture

### Service Structure

- **FM100.Domain**: Contains pure data models with attributes only (no logic)
- **FM100.Core**: Contains calculation services (stateless calculators)
- **FM100.Core.Performance.Abstractions**: Defines service interfaces
- **FM100.Core.DependencyInjection**: DI registration extensions

### Registered Services

All performance calculation services are registered via the `AddPerformanceServices()` extension method:

#### Registered Calculators

| Service | Lifetime | Purpose |
|---------|----------|---------|
| `IEmotionalStabilityCalculator` | Singleton | Calculates emotional stability from emotional state variance |
| `IDominantEmotionCalculator` | Singleton | Identifies the dominant emotion from a player's state |
| `IMatchPerformanceCalculator` | Singleton | Calculates player and squad performance metrics |
| `ISquadStrengthEvaluator` | Scoped | Evaluates overall squad strength and performance |

**Lifetime Explanations:**
- **Singleton**: Stateless services that don't change between calls
- **Scoped**: Services that may contain instance-specific state

## Setup Example

### Step 1: Create Service Collection

```csharp
var services = new ServiceCollection();
services.AddPerformanceServices();
var serviceProvider = services.BuildServiceProvider();
```

### Step 2: Resolve Services

```csharp
// Resolve a single service
var calculator = serviceProvider.GetRequiredService<IMatchPerformanceCalculator>();

// Resolve a class with dependencies
var example = ActivatorUtilities.CreateInstance<MatchPerformanceExample>(serviceProvider);
```

### Step 3: Inject into Constructor

```csharp
public class MyService
{
	private readonly IMatchPerformanceCalculator _performanceCalculator;
	private readonly IEmotionalStabilityCalculator _stabilityCalculator;

	public MyService(
		IMatchPerformanceCalculator performanceCalculator,
		IEmotionalStabilityCalculator stabilityCalculator)
	{
		_performanceCalculator = performanceCalculator;
		_stabilityCalculator = stabilityCalculator;
	}

	public void AnalyzePerformance(MatchEmotionalState state)
	{
		var score = _performanceCalculator.CalculatePlayerPerformanceScore(15, state);
		var stability = _stabilityCalculator.Calculate(state);
		// ...
	}
}
```

## Complete Console Application Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using FM100.Core.DependencyInjection;
using FM100.Core.Performance.Abstractions;

class Program
{
	static void Main(string[] args)
	{
		// Setup DI Container
		var services = new ServiceCollection();
		services.AddPerformanceServices();
		var serviceProvider = services.BuildServiceProvider();

		// Create instance with automatic dependency injection
		var app = ActivatorUtilities.CreateInstance<Application>(serviceProvider);
		app.Run();
	}
}

class Application
{
	private readonly IMatchPerformanceCalculator _calculator;
	private readonly IEmotionalStabilityCalculator _stability;

	public Application(
		IMatchPerformanceCalculator calculator,
		IEmotionalStabilityCalculator stability)
	{
		_calculator = calculator;
		_stability = stability;
	}

	public void Run()
	{
		Console.WriteLine("FM100 Application with Dependency Injection");
		// Use services here...
	}
}
```

## ASP.NET Core Integration

If using in an ASP.NET Core application, add to `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddPerformanceServices();

var app = builder.Build();
// ...
```

Then inject into controllers:

```csharp
[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
	private readonly IMatchPerformanceCalculator _calculator;

	public PerformanceController(IMatchPerformanceCalculator calculator)
	{
		_calculator = calculator;
	}

	[HttpPost("calculate")]
	public IActionResult CalculatePerformance([FromBody] MatchEmotionalState state)
	{
		var score = _calculator.CalculatePlayerPerformanceScore(15, state);
		return Ok(new { performanceScore = score });
	}
}
```

## Service Interfaces

### IMatchPerformanceCalculator

```csharp
public interface IMatchPerformanceCalculator
{
	int CalculatePlayerPerformanceScore(int technicalAverage, MatchEmotionalState emotionalState);
	decimal CalculateEmotionalModifier(MatchEmotionalState emotionalState);
	void ApplyMatchEvent(MatchEmotionalState emotionalState, MatchEvent matchEvent, MentalAttributes mentalAttributes);
	int CalculateSquadEmotionalIndex(List<MatchEmotionalState> playerStates, int teamCohesion);
	int CalculateMoraleIndex(List<MatchEmotionalState> playerStates);
	decimal CalculateMatchImpactFactor(MatchEmotionalState emotionalState);
	int CalculateFatigueImpact(int fatigueLevel, int playedMinutes);
}
```

### IEmotionalStabilityCalculator

```csharp
public interface IEmotionalStabilityCalculator
{
	int Calculate(MatchEmotionalState emotionalState);
}
```

### IDominantEmotionCalculator

```csharp
public interface IDominantEmotionCalculator
{
	EmotionalState Calculate(MatchEmotionalState emotionalState);
}
```

### ISquadStrengthEvaluator

```csharp
public interface ISquadStrengthEvaluator
{
	int CalculateEmotionalStrength(List<MatchEmotionalState> playerStates);
	int CalculateOffensivePower(List<MatchEmotionalState> playerStates);
	int CalculateDefensiveSolidity(List<MatchEmotionalState> playerStates);
	SquadPerformanceSummary GetPerformanceSummary(List<MatchEmotionalState> playerStates);
	double CalculateExpectedWinProbability();
}
```

## Best Practices

1. **Depend on Abstractions**: Always inject interfaces, not concrete classes
2. **Avoid Service Locator**: Don't use `serviceProvider.GetService()` inside classes; use constructor injection
3. **Appropriate Lifetimes**: Use Singleton for stateless services, Scoped for state-aware services
4. **Clear Dependencies**: Make all dependencies explicit in constructor parameters
5. **Configuration**: Consider creating extension methods for related service groups

## Adding Custom Services

To register your own services:

```csharp
public static IServiceCollection AddMyCustomServices(this IServiceCollection services)
{
	services.AddSingleton<IMyService, MyService>();
	services.AddScoped<IAnotherService, AnotherService>();
	return services;
}

// Usage
services.AddPerformanceServices();
services.AddMyCustomServices();
```

## Testing with DI

For unit tests, mock the interfaces:

```csharp
[Fact]
public void TestWithMockedCalculator()
{
	var mockCalculator = new Mock<IMatchPerformanceCalculator>();
	mockCalculator
		.Setup(c => c.CalculatePlayerPerformanceScore(It.IsAny<int>(), It.IsAny<MatchEmotionalState>()))
		.Returns(15);

	var app = new Application(mockCalculator.Object);
	// Test logic...
}
```

## Troubleshooting

### "Unable to resolve service"

This means a service wasn't registered. Check:
- Service is registered via `AddPerformanceServices()` or custom registration
- Using the correct interface name
- Service collection is built before resolving

### "Service not in scope"

This typically happens with scoped services. Ensure:
- You're within a scope when resolving scoped services
- In console apps, create a new scope per logical operation
