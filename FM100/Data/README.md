# SQLite Database Integration Guide

## Overview

Il progetto FM100 ora include un'integrazione SQLite per la persistenza dei dati dei calciatori (FootballPlayer), con pattern Repository e seeding automatico usando Bogus.

## Architettura

### Struttura delle cartelle

```
FM100/
├── Data/
│   ├── DatabaseInitializer.cs           # Inizializza il database e schema
│   ├── DependencyInjection/
│   │   └── DataServiceCollectionExtensions.cs  # Registrazione DI
│   ├── Repositories/
│   │   ├── IFootballPlayerRepository.cs        # Interfaccia del repository
│   │   └── FootballPlayerRepository.cs         # Implementazione con Dapper
│   └── Seeders/
│       └── FootballPlayerSeeder.cs            # Generazione dati fake con Bogus
```

## Componenti

### 1. DatabaseInitializer
Gestisce:
- Creazione directory database in `%APPDATA%/FM100/`
- Creazione schema SQLite automatico
- Gestione percorso database

**Percorso database:** `C:\Users\<username>\AppData\Roaming\FM100\FM100.db`

### 2. Repository Pattern
**IFootballPlayerRepository** fornisce:
- `GetAllAsync()` - Recupera tutti i calciatori
- `GetByIdAsync(Guid id)` - Recupera un calciatore per ID
- `GetByShirtNumberAsync(int number)` - Recupera per numero di maglia
- `AddAsync(FootballPlayer player)` - Aggiunge un calciatore
- `AddManyAsync(IEnumerable<FootballPlayer>)` - Bulk insert
- `UpdateAsync(FootballPlayer player)` - Aggiorna un calciatore
- `DeleteAsync(Guid id)` - Elimina un calciatore
- `GetCountAsync()` - Conta totale calciatori
- `ClearAllAsync()` - Cancella tutti i dati (utile per testing)

### 3. Bogus DataSeeder
**FootballPlayerSeeder** genera dati fake:
- Crea squadre di 23 giocatori (customizzabile)
- Attributi realistici (età 17-52, nazionalità, peso, altezza)
- Valori Mental Attributes casuali (1-20)
- Valori Dynamic State casuali

## Come Usare

### Accesso al Database

```csharp
// Ottenere il service provider dall'App
var serviceProvider = Application.Current.MainWindow is MainWindow mw 
	? ((App)Application.Current).GetServiceProvider()
	: throw new InvalidOperationException();

// Usare il repository
var playerRepository = serviceProvider.GetRequiredService<IFootballPlayerRepository>();

// Esempio: ottenere tutti i giocatori
var allPlayers = await playerRepository.GetAllAsync();

// Esempio: aggiungere un giocatore
await playerRepository.AddAsync(newPlayer);

// Esempio: aggiornare
await playerRepository.UpdateAsync(player);
```

### Generazione Dati Fake

```csharp
var seeder = new FootballPlayerSeeder(playerRepository);

// Genera 11 giocatori fake
var players = seeder.GeneratePlayersForTeam(11);

// Salva nel database
await playerRepository.AddManyAsync(players);

// Seed automatico al primo avvio (solo se il database è vuoto)
await seeder.SeedIfEmptyAsync(23);
```

## Database Schema

**Tabella: FootballPlayers**

| Colonna | Tipo | Descrizione |
|---------|------|-------------|
| Id | TEXT (PK) | GUID unico del giocatore |
| FirstName | TEXT | Nome |
| LastName | TEXT | Cognome |
| BirthDate | TEXT | Data di nascita (ISO 8601) |
| Age | INTEGER | Età calcolata |
| Nationality | TEXT | Nazionalità |
| Description | TEXT | Descrizione |
| Height | INTEGER | Altezza (cm) |
| Weight | INTEGER | Peso (kg) |
| ShirtNumber | INTEGER | Numero di maglia |
| Potential | INTEGER | Potenziale (1-99) |
| Reputation | INTEGER | Reputazione (1-20) |
| MarketValue | INTEGER | Valore di mercato (milioni) |
| CurrentState | TEXT | JSON serializzato DynamicState |
| MentalAttributes | TEXT | JSON serializzato MentalAttributes |
| CreatedAt | TEXT | Timestamp creazione |
| UpdatedAt | TEXT | Timestamp ultimo aggiornamento |

**Indici:**
- `idx_player_name` on (FirstName, LastName)
- `idx_player_shirt` on (ShirtNumber)

## Dependency Injection

Il DI è configurato automaticamente in `App.xaml.cs`:

```csharp
services.AddDataServices();  // Registra IFootballPlayerRepository
services.AddPerformanceServices();  // Servizi core
```

## Tecnologie Utilizzate

- **Dapper**: ORM lightweight per accesso dati
- **System.Data.SQLite**: Provider SQLite
- **Bogus**: Libreria generazione dati fake
- **Microsoft.Extensions.DependencyInjection**: DI container

## Note Importanti

1. **Serializzazione:** DynamicState e MentalAttributes sono salvati come JSON nel database
2. **Async/Await:** Tutte le operazioni sono async
3. **Connection Management:** Dapper gestisce automaticamente le connessioni
4. **Database Location:** Il database si trova in AppData per non occupare lo spazio di sviluppo
5. **Seeding:** Al primo avvio, vengono generati 23 giocatori fake automaticamente

## Futuri Sviluppi

Quando vorrai aggiungere altre entità (Team, Staff, Match History):
1. Crea la nuova interfaccia repository `IXxxRepository`
2. Implementa con Dapper
3. Aggiungi il seeder corrispondente
4. Registra in `DataServiceCollectionExtensions.cs`
