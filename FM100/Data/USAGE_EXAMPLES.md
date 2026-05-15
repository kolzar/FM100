# Usage Examples - Football Player Repository

## Quick Start - Using PlayerManagementService (Recommended)

For most game logic, use the high-level `PlayerManagementService` instead of the repository directly:

### 1. Load All Players

```csharp
var playerService = serviceProvider.GetRequiredService<PlayerManagementService>();
var allPlayers = await playerService.LoadAllPlayersAsync();
```

### 2. Load Players by Shirt Number

```csharp
var playersByShirtNumber = await playerService.LoadPlayersByShirtNumberAsync();
var striker = playersByShirtNumber[9];
```

### 3. Get Player by ID

```csharp
var player = await playerService.GetPlayerAsync(playerId);
```

### 4. Get Player by Shirt Number

```csharp
var leftWing = await playerService.GetPlayerByShirtNumberAsync(11);
```

### 5. Save Player Changes

```csharp
player.Reputation = 20;
await playerService.SavePlayerAsync(player);
```

### 6. Add New Player

```csharp
var newPlayer = new FootballPlayer
{
	FirstName = "Carlos",
	LastName = "Tevez",
	// ... other properties
};
await playerService.AddPlayerAsync(newPlayer);
```

### 7. Remove Player

```csharp
await playerService.RemovePlayerAsync(playerId);
```

### 8. Get Squad Count

```csharp
var squadSize = await playerService.GetSquadCountAsync();
```

### 9. Check Shirt Number Availability

```csharp
bool isTaken = await playerService.IsShirtNumberTakenAsync(10);
int nextAvailable = await playerService.GetNextAvailableShirtNumberAsync();
```

---

## Direct Repository Usage (Advanced)

If you need more control, use the repository directly:

### 1. Load All Players from Database

```csharp
var serviceProvider = ((App)Application.Current).GetServiceProvider();
var playerRepository = serviceProvider.GetRequiredService<IFootballPlayerRepository>();

var allPlayers = await playerRepository.GetAllAsync();
foreach (var player in allPlayers)
{
	Console.WriteLine($"{player.FirstName} {player.LastName} - Shirt: {player.ShirtNumber}");
}
```

### 2. Get a Specific Player by ID

```csharp
var playerId = Guid.Parse("your-player-id");
var player = await playerRepository.GetByIdAsync(playerId);

if (player != null)
{
	Console.WriteLine($"Found: {player.FirstName} {player.LastName}");
}
```

### 3. Get Player by Shirt Number

```csharp
var player = await playerRepository.GetByShirtNumberAsync(7);
if (player != null)
{
	Console.WriteLine($"Player #{player.ShirtNumber}: {player.FirstName} {player.LastName}");
}
```

### 4. Add a New Player

```csharp
var newPlayer = new FootballPlayer
{
	Id = Guid.NewGuid(),
	FirstName = "Cristiano",
	LastName = "Ronaldo",
	BirthDate = new DateTime(1985, 2, 5),
	Age = 39,
	Nationality = "Portugal",
	Height = 187,
	Weight = 84,
	ShirtNumber = 7,
	Potential = 95,
	Reputation = 20,
	MarketValue = 50,
	CurrentState = new DynamicState(),
	MentalAttributes = new MentalAttributes()
};

await playerRepository.AddAsync(newPlayer);
```

### 5. Add Multiple Players (Bulk Insert)

```csharp
var seeder = new FootballPlayerSeeder(playerRepository);
var players = seeder.GeneratePlayersForTeam(11);
await playerRepository.AddManyAsync(players);
```

### 6. Update Player Data

```csharp
var player = await playerRepository.GetByIdAsync(playerId);
if (player != null)
{
	player.Reputation = 20;
	player.MarketValue = 120;
	await playerRepository.UpdateAsync(player);
}
```

### 7. Delete a Player

```csharp
await playerRepository.DeleteAsync(playerId);
```

### 8. Get Total Player Count

```csharp
var totalPlayers = await playerRepository.GetCountAsync();
Console.WriteLine($"Total players in database: {totalPlayers}");
```

### 9. Generate Fake Players with Bogus

```csharp
var seeder = new FootballPlayerSeeder(playerRepository);

// Generate 11 random players
var squad = seeder.GeneratePlayersForTeam(11);

// Or generate and save directly
await seeder.SeedIfEmptyAsync(23);
```

---

## ViewModel Example

```csharp
using FM100.Data.Repositories;
using FM100.Services;
using FM100.Domain.FootballPlayer;

public class GameViewModel
{
	private readonly PlayerManagementService _playerService;
	private List<FootballPlayer> _players;

	public GameViewModel(PlayerManagementService playerService)
	{
		_playerService = playerService;
	}

	public async Task InitializeAsync()
	{
		_players = await _playerService.LoadAllPlayersAsync();
	}

	public async Task UpdatePlayerPerformance(Guid playerId, int newReputation)
	{
		var player = _players.FirstOrDefault(p => p.Id == playerId);
		if (player != null)
		{
			player.Reputation = newReputation;
			await _playerService.SavePlayerAsync(player);
		}
	}

	public FootballPlayer? GetPlayerByShirtNumber(int shirtNumber)
	{
		return _players.FirstOrDefault(p => p.ShirtNumber == shirtNumber);
	}
}
```

## View Example (WPF UserControl)

```csharp
public partial class GameView : UserControl
{
	private readonly PlayerManagementService _playerService;
	private readonly GameViewModel _viewModel;

	public GameView()
	{
		InitializeComponent();

		var serviceProvider = ((App)Application.Current).GetServiceProvider();
		_playerService = serviceProvider.GetRequiredService<PlayerManagementService>();
		_viewModel = new GameViewModel(_playerService);

		DataContext = _viewModel;
	}

	private async void OnGameStarted(object sender, RoutedEventArgs e)
	{
		await _viewModel.InitializeAsync();
	}
}
```

## Integration in App.xaml.cs

The App.xaml.cs already handles DI registration and seeding:

```csharp
public partial class App : Application
{
	private ServiceProvider? _serviceProvider;

	public App()
	{
		InitializeServices();
	}

	private void InitializeServices()
	{
		var services = new ServiceCollection();
		services.AddDataServices();        // Database + Repository + Services
		services.AddPerformanceServices(); // Match calculation services
		_serviceProvider = services.BuildServiceProvider();
	}

	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		// Auto-seed with fake players on first run
		if (_serviceProvider != null)
		{
			var playerRepository = _serviceProvider.GetRequiredService<IFootballPlayerRepository>();
			var seeder = new FootballPlayerSeeder(playerRepository);
			await seeder.SeedIfEmptyAsync(23);
		}
	}

	public ServiceProvider GetServiceProvider()
	{
		return _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized");
	}
}
```

## Testing with Clear & Reseed

```csharp
public class GameSetupService
{
	private readonly IFootballPlayerRepository _playerRepository;

	public GameSetupService(IFootballPlayerRepository playerRepository)
	{
		_playerRepository = playerRepository;
	}

	public async Task ResetAndSeedDefaultSquad()
	{
		await _playerRepository.ClearAllAsync();
		var seeder = new FootballPlayerSeeder(_playerRepository);
		await seeder.SeedIfEmptyAsync(23);
	}
}
```

## Database Location

The SQLite database is stored at:
```
C:\Users\<YourUsername>\AppData\Roaming\FM100\FM100.db
```

To reset the database, simply delete this file and restart the application.

