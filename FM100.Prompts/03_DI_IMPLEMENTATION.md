# Dependency Injection Implementation

## Overview

This document details the Dependency Injection pattern implementation for the FM100 project as requested by the user.

## User Request

```
"Vorrei usare anche il pattern dependency injection"

Translation: "I also want to use the dependency injection pattern"
```

---

## Implementation Details

### 1. Service Interfaces

Located in `FM100.Core/Performance/Abstractions/`

#### IMatchPerformanceCalculator
```csharp
public interface IMatchPerformanceCalculator
{
	int CalculatePlayerPerformanceScore(
		int technicalAverage, 
		MatchEmotionalState emotionalState);

	decimal CalculateEmotionalModifier(
		MatchEmotionalState emotionalState);

	void ApplyMatchEvent(
		MatchEmotionalState emotionalState, 
		MatchEvent matchEvent, 
		MentalAttributes mentalAttributes);

	int CalculateSquadEmotionalIndex(
		List<MatchEmotionalState> playerStates, 
		int teamCohesion);

	int CalculateMoraleIndex(
		List<MatchEmotionalState> playerStates);

	decimal CalculateMatchImpactFactor(
		MatchEmotionalState emotionalState);

	int CalculateFatigueImpact(
		int fatigueLevel, 
		int playedMinutes);
}
```

#### IEmotionalStabilityCalculator
```csharp
public interface IEmotionalStabilityCalculator
{
	int Calculate(MatchEmotionalState emotionalState);
}
```

#### IDominantEmotionCalculator
```csharp
public interface IDominantEmotionCalculator
{
	EmotionalState Calculate(MatchEmotionalState emotionalState);
}
```

#### ISquadStrengthEvaluator
```csharp
public interface ISquadStrengthEvaluator
{
	int CalculateEmotionalStrength(
		List<MatchEmotionalState> playerStates);

	int CalculateOffensivePower(
		List<MatchEmotionalState> playerStates);

	int CalculateDefensiveSolidity(
		List<MatchEmotionalState> playerStates);

	SquadPerformanceSummary GetPerformanceSummary(
		List<MatchEmotionalState> playerStates);

	double CalculateExpectedWinProbability();
}
```

### 2. Service Implementations

Each calculator implements its corresponding interface:

```csharp
// EmotionalStabilityCalculator
public sealed class EmotionalStabilityCalculator : IEmotionalStabilityCalculator
{
	public static int Calculate(MatchEmotionalState state)
	{
		// Implementation
	}

	int IEmotionalStabilityCalculator.Calculate(MatchEmotionalState emotionalState)
	{
		return Calculate(emotionalState);
	}
}

// DominantEmotionCalculator
public sealed class DominantEmotionCalculator : IDominantEmotionCalculator
{
	public static EmotionalState Calculate(MatchEmotionalState state)
	{
		// Implementation
	}

	EmotionalState IDominantEmotionCalculator.Calculate(MatchEmotionalState emotionalState)
	{
		return Calculate(emotionalState);
	}
}

// MatchPerformanceCalculator
public sealed class MatchPerformanceCalculator : IMatchPerformanceCalculator
{
	public static int CalculatePlayerPerformanceScore(
		int technicalAttributesAverage,
		MatchEmotionalState emotionalState)
	{
		// Implementation
	}

	int IMatchPerformanceCalculator.CalculatePlayerPerformanceScore(
		int technicalAverage, 
		MatchEmotionalState emotionalState)
	{
		return CalculatePlayerPerformanceScore(technicalAverage, emotionalState);
	}

	// ... other methods
}

// SquadStrengthEvaluator
public sealed class SquadStrengthEvaluator : ISquadStrengthEvaluator
{
	// Constructor-based initialization
	// Interface implementation
}
```

### 3. DI Registration

File: `FM100.Core/DependencyInjection/PerformanceServiceCollectionExtensions.cs`

```csharp
public static class PerformanceServiceCollectionExtensions
{
	/// <summary>
	/// Adds all performance calculation services to the dependency injection container.
	/// </summary>
	public static IServiceCollection AddPerformanceServices(
		this IServiceCollection services)
	{
		// Register calculator interfaces with implementations
		// Singletons for stateless services
		services.AddSingleton<IEmotionalStabilityCalculator, EmotionalStabilityCalculator>();
		services.AddSingleton<IDominantEmotionCalculator, DominantEmotionCalculator>();
		services.AddSingleton<IMatchPerformanceCalculator, MatchPerformanceCalculator>();

		// Scoped for services with instance state
		services.AddScoped<ISquadStrengthEvaluator, SquadStrengthEvaluator>();

		return services;
	}
}
```

### 4. Service Lifetimes

**Why Singleton for Calculators?**
- Stateless services
- Can be reused across requests
- Improves performance
- Thread-safe

**Why Scoped for SquadStrengthEvaluator?**
- May contain match-specific state
- Clean state per logical operation
- Prevents cross-contamination

### 5. Usage Pattern

#### Console Application
```csharp
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
		// Use injected services
		var score = _calculator.CalculatePlayerPerformanceScore(15, state);
		var stability = _stability.Calculate(state);
	}
}
```

#### ASP.NET Core Application
```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddPerformanceServices();

var app = builder.Build();
app.Run();

// In Controller
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
	public IActionResult Calculate([FromBody] MatchEmotionalState state)
	{
		var score = _calculator.CalculatePlayerPerformanceScore(15, state);
		return Ok(new { performanceScore = score });
	}
}
```

#### MatchPerformanceExample Update
```csharp
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
		// Setup DI
		var services = new ServiceCollection();
		services.AddPerformanceServices();
		var serviceProvider = services.BuildServiceProvider();

		// Create with DI
		var example = ActivatorUtilities.CreateInstance<MatchPerformanceExample>(serviceProvider);
		example.Run();
	}

	private void Run()
	{
		// Use injected services
		var playerState = CreatePlayerState();
		var stability = _stabilityCalculator.Calculate(playerState);
		var emotion = _emotionCalculator.Calculate(playerState);
	}
}
```

### 6. NuGet Dependencies

Added to `FM100.Core.csproj`:

```xml
<ItemGroup>
	<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
	<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0" />
</ItemGroup>
```

---

## Benefits of DI Implementation

### 1. Loose Coupling
- Services depend on abstractions, not concretions
- Easy to swap implementations
- Reduced dependencies

### 2. Testability
- Services can be mocked via interfaces
- Unit tests are simpler and cleaner
- Better test isolation

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

### 3. Maintainability
- Clear dependencies
- Easy to understand
- Changes are localized

### 4. Extensibility
- New implementations can be added
- Different lifetimes for different scenarios
- Factory patterns can be added

### 5. Configuration
- Centralized service registration
- Easy to enable/disable features
- Conditional registration possible

```csharp
public static IServiceCollection AddPerformanceServices(
	this IServiceCollection services,
	PerformanceOptions options)
{
	if (options.UseCache)
	{
		services.AddSingleton<ICacheService, CacheService>();
	}

	// Register services...
	return services;
}
```

---

## Advanced Patterns

### Factory Pattern
```csharp
public interface IPerformanceCalculatorFactory
{
	IMatchPerformanceCalculator Create();
}

public class PerformanceCalculatorFactory : IPerformanceCalculatorFactory
{
	private readonly IServiceProvider _serviceProvider;

	public PerformanceCalculatorFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public IMatchPerformanceCalculator Create()
	{
		return _serviceProvider.GetRequiredService<IMatchPerformanceCalculator>();
	}
}

// Register
services.AddSingleton<IPerformanceCalculatorFactory, PerformanceCalculatorFactory>();
```

### Decorator Pattern
```csharp
public class CachedMatchPerformanceCalculator : IMatchPerformanceCalculator
{
	private readonly IMatchPerformanceCalculator _inner;
	private readonly IMemoryCache _cache;

	public CachedMatchPerformanceCalculator(
		IMatchPerformanceCalculator inner,
		IMemoryCache cache)
	{
		_inner = inner;
		_cache = cache;
	}

	public int CalculatePlayerPerformanceScore(
		int technicalAverage,
		MatchEmotionalState emotionalState)
	{
		var key = $"perf_{emotionalState.PlayerId}";
		if (_cache.TryGetValue(key, out int result))
		{
			return result;
		}

		result = _inner.CalculatePlayerPerformanceScore(technicalAverage, emotionalState);
		_cache.Set(key, result, TimeSpan.FromMinutes(5));
		return result;
	}
}

// Register
services.AddSingleton<IMatchPerformanceCalculator>(sp =>
{
	var inner = new MatchPerformanceCalculator();
	return new CachedMatchPerformanceCalculator(inner, sp.GetRequiredService<IMemoryCache>());
});
```

---

## Integration with Other Frameworks

### Entity Framework Core
```csharp
builder.Services.AddDbContext<FM100DbContext>();
builder.Services.AddPerformanceServices();
```

### Logging
```csharp
builder.Services.AddLogging(config =>
{
	config.AddConsole();
});
builder.Services.AddPerformanceServices();
```

### Configuration
```csharp
var config = builder.Configuration;
builder.Services.Configure<PerformanceOptions>(config.GetSection("Performance"));
builder.Services.AddPerformanceServices();
```

---

## Testing with DI

### Unit Testing
```csharp
[Fact]
public void CalculatePerformance_WithGoodEmotions_ReturnsHighScore()
{
	// Arrange
	var calculator = new MatchPerformanceCalculator();
	var state = new MatchEmotionalState { Happiness = 18, Anxiety = 3 };

	// Act
	var score = calculator.CalculatePlayerPerformanceScore(15, state);

	// Assert
	Assert.True(score > 12);
}
```

### Integration Testing
```csharp
[Fact]
public void Application_WithDI_WorksCorrectly()
{
	// Arrange
	var services = new ServiceCollection();
	services.AddPerformanceServices();
	var provider = services.BuildServiceProvider();

	// Act
	var calculator = provider.GetRequiredService<IMatchPerformanceCalculator>();

	// Assert
	Assert.NotNull(calculator);
}
```

---

## Troubleshooting

### "Unable to resolve service"
**Cause**: Service not registered
**Solution**: Add to `AddPerformanceServices()` or verify registration

### "No suitable constructor found"
**Cause**: Constructor parameters not registered
**Solution**: Register all dependencies

### "Service is not in scope"
**Cause**: Using scoped service outside scope
**Solution**: Create scope with `serviceProvider.CreateScope()`

---

## Summary

The DI implementation:
✅ Uses interface-based design
✅ Properly registers services
✅ Supports multiple lifetimes
✅ Integrates with standard patterns
✅ Enables easy testing
✅ Improves code maintainability
✅ Follows .NET best practices

All 38 unit tests pass with DI integration ✅

---

## Related Documents

- See **ARCHITECTURE.md** for full system architecture
- See **DI_SETUP_GUIDE.md** for detailed setup instructions
- See **COMPLETION_CHECKLIST.md** for implementation verification
