# 🎮 FM100 UI Architecture

## Panoramica

L'interfaccia utente di FM100 è organizzata seguendo un modello di **View-based Navigation** dove la finestra principale (`MainWindow`) ospita diversi view che vengono caricati dinamicamente.

## Struttura dei View

### 1. **SplashScreenView** (Schermata di Caricamento)
- **Percorso**: `FM100/Views/SplashScreenView.xaml`
- **Scopo**: Splash screen con logo e loading bar
- **Durata**: ~3 secondi, poi transizione al Menu
- **Elementi**:
  - Logo dell'applicazione
  - Titolo "Football Manager Master League"
  - Progress bar indeterminata
  - Background con overlay scuro

### 2. **MenuView** (Menu Principale)
- **Percorso**: `FM100/Views/MenuView.xaml`
- **Scopo**: Menu principale con opzioni di gioco
- **Bottoni Disponibili**:
  - 🎮 **Nuova Partita** → Apre `CoachCustomizationView`
  - 📂 **Carica Partita** → Sistema di salvataggio (in sviluppo)
  - ⚙️ **Impostazioni** → Impostazioni (in sviluppo)
  - ❌ **Esci** → Chiude l'applicazione

### 3. **CoachCustomizationView** (Personalizzazione Allenatore)
- **Percorso**: `FM100/Views/CoachCustomizationView.xaml`
- **Scopo**: Creazione profilo allenatore e selezione squadra
- **Sezioni**:

  #### A. Dati Allenatore
  - **Nome**: TextBox per inserire il nome
  - **Nazionalità**: ComboBox con list di paesi
  - **Età**: Slider (25-70 anni)
  - **Stile Tattico**: RadioButtons (Offensivo, Equilibrato, Difensivo)

  #### B. Selezione Squadra
  - **Divisione**: ComboBox per scegliere Serie A, B o C
  - **Lista Squadre**: ListBox con squadre disponibili nella divisione
  - **Bottoni**:
	- ← Indietro → Ritorna al Menu
	- ✓ Continua → Accede a `GameView`

### 4. **GameView** (Area di Gioco - Layout Principale)
- **Percorso**: `FM100/Views/GameView.xaml`
- **Scopo**: Layout principale con sidebar e area contenuti
- **Layout**:
  ```
  ┌─────────────────────────────────────────┐
  │  Header Bar (Season, Budget, Next Match)│
  ├─────────────┬─────────────────────────────┤
  │             │                             │
  │  SIDEBAR    │   CONTENT AREA              │
  │  (Menu)     │   (Dynamic Content)         │
  │             │                             │
  │  - Dashboard│   Carica qui i diversi     │
  │  - Rosa     │   pannelli a seconda del   │
  │  - Tattica  │   bottone cliccato         │
  │  - Transfer │                             │
  │  - Calendario                             │
  │  - Classifica                             │
  │  - Finanze  │                             │
  │  - Settings │                             │
  │  - Salva    │                             │
  │  - Esci     │                             │
  └─────────────┴─────────────────────────────┘
  ```

#### Sidebar Navigation (Sinistra - 200px)
Bottoni di navigazione:
- 📊 **Dashboard** → Overview del team (Colore evidenziato: #00d4ff)
- 👥 **Rosa** → Gestione giocatori
- 🎯 **Tattica** → Configurazione formazione e tattica
- 🔄 **Trasferimenti** → Mercato e scambi
- 📅 **Calendario** → Partite e fixture
- 📈 **Classifica** → Standings e risultati
- 💰 **Finanze** → Budget e statistiche economiche
- ⚙️ **Impostazioni** → Impostazioni partita
- 💾 **Salva** → Salva la partita
- ❌ **Esci al Menu** → Torna al menu principale

#### Header Bar (Top)
- **Stagione e Week**: "Stagione 1 - Week 1"
- **Prossima Partita**: Mostra il match successivo
- **Budget**: Visualizza budget disponibile

#### Content Area (Destra - Dinamico)
- Ospita i diversi pannelli a seconda della sezione selezionata
- Viene popolato dinamicamente dai click sulla sidebar

---

## Palette Colori

| Colore | Hex Code | Utilizzo |
|--------|----------|----------|
| Background principale | #1a1a2e | Background dell'app |
| Sfondo scuro | #0f0f1e | Sidebar e header |
| Accent primario | #00d4ff | Bottoni attivi, elementi evidenziati |
| Sfondo input | #2a2a4e | TextBox, ComboBox, ListBox |
| Testo secondario | #B0B0B0 | Etichette, didascalie |
| Bianco | #FFFFFF | Testo principale |
| Nero | #000000 | Overlay |

---

## Stili XAML

### ButtonStyles.xaml
Definisce due stili principali di bottoni:

#### **MenuButtonStyle**
- Bottoni del menu principale
- Hover: ingrandimento e cambio colore
- Press: effetto di pressione

#### **SidebarButtonStyle**
- Bottoni della sidebar sinistra
- Hover: sfondo azzurro con testo nero
- Press: effetto più scuro

---

## Flusso di Navigazione

```
Avvio App
	↓
[SplashScreenView] (3 secondi)
	↓
[MenuView]
	├→ Nuova Partita ──→ [CoachCustomizationView]
	│                        ├→ Indietro ──→ [MenuView]
	│                        └→ Continua ──→ [GameView]
	├→ Carica ──→ (in sviluppo)
	├→ Impostazioni ──→ (in sviluppo)
	└→ Esci ──→ Chiudi app

[GameView]
	├→ Sidebar buttons → Content Area
	└→ Esci ──→ [MenuView]
```

---

## Implementazione

### MainWindow.xaml.cs
Gestisce la logica di navigazione tra i view:

```csharp
// Metodi principali:
- ShowSplashScreen()         // Mostra splash per 3 sec
- ShowMainMenu()             // Menu principale
- ShowCoachCustomization()   // Personalizzazione coach
- ShowGameArea()             // Area di gioco
- ShowGameContent(string)    // Carica contenuto dinamico
```

### Code-Behind di Ogni View
Ogni view ha il suo file `.xaml.cs` per gestire interazioni locali.

---

## Da Implementare

### Sezioni Gioco Mancanti
1. **Dashboard** - Overview del team
2. **Rosa** - Gestione giocatori
3. **Tattica** - Formazione e tattica
4. **Trasferimenti** - Mercato calciatori
5. **Calendario** - Fixture e risultati
6. **Classifica** - Standings
7. **Finanze** - Budget e economia

### Funzionalità Future
- [ ] Sistema di salvataggio partite
- [ ] Impostazioni avanzate
- [ ] Sistema audio/musica
- [ ] Notifiche in-game
- [ ] Chat e comunicazioni

---

## Asset Necessari

Crea una cartella `FM100/Assets/` e aggiungi:

```
Assets/
├── SplashBackground.png     (1600x1200)
├── MenuBackground.png       (1920x1080)
├── TeamLogo.png             (100x100)
├── AppIcon.ico              (64x64)
└── Football_Banner.png      (2000x500)
```

### Linee Guida per le Immagini
- **Formato**: PNG per trasparenza, JPG per foto
- **Colori**: Tonalità blu/azzurro coerenti con il tema
- **Qualità**: 300 DPI per immagini di sfondo

---

## Responsive Design

L'app è ottimizzata per:
- **Risoluzione minima**: 1280x720
- **Risoluzione consigliata**: 1920x1080
- **Scalabilità**: Segue il DPI del sistema

---

## Styling Future

Considerare l'aggiunta di:
- [ ] Temi (Light/Dark)
- [ ] Animazioni di transizione tra view
- [ ] Toast notifications
- [ ] Dialog box personalizzate
- [ ] Tooltips informativi

---

**Versione**: 1.0  
**Ultima modifica**: 2024  
**Responsabile**: Architettura UI FM100
