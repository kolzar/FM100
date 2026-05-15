# 👀 Views - FM100 User Interface

Questa cartella contiene tutti i **View XAML** che compongono l'interfaccia utente di FM100.

## 📁 Struttura

### Views Principali

#### 🎬 **SplashScreenView**
- **File**: `SplashScreenView.xaml` + `.xaml.cs`
- **Scopo**: Schermata di caricamento iniziale
- **Elementi**:
  - Logo dell'app
  - Titolo "Football Manager Master League"
  - Progress bar animata
  - Durata: ~3 secondi
- **Flusso**: App avvia → SplashScreenView → MainWindow carica → Menu

#### 🎮 **MenuView**
- **File**: `MenuView.xaml` + `.xaml.cs`
- **Scopo**: Menu principale dell'applicazione
- **Bottoni**:
  - 🎮 Nuova Partita → `CoachCustomizationView`
  - 📂 Carica Partita → (in sviluppo)
  - ⚙️ Impostazioni → (in sviluppo)
  - ❌ Esci → Chiudi app
- **Background**: Overlay scuro con titolo

#### 👔 **CoachCustomizationView**
- **File**: `CoachCustomizationView.xaml` + `.xaml.cs`
- **Scopo**: Personalizzazione allenatore e selezione squadra
- **Sezioni**:
  1. **Dati Allenatore**:
	 - Nome (TextBox)
	 - Nazionalità (ComboBox)
	 - Età (Slider 25-70)
	 - Stile Tattico (RadioButtons)
  2. **Selezione Squadra**:
	 - Divisione (ComboBox: A/B/C)
	 - Lista squadre (ListBox)
- **Bottoni**:
  - ← Indietro → Torna a `MenuView`
  - ✓ Continua → Vai a `GameView`

#### 🕹️ **GameView**
- **File**: `GameView.xaml` + `.xaml.cs`
- **Scopo**: Area principale di gioco
- **Layout**: Sidebar + Content Area
- **Sezioni**:
  - **Sidebar Sinistra** (200px):
	- Logo squadra
	- 10 bottoni di navigazione
	- Stile: Blu scuro con accenti azzurri
  - **Header Top** (60px):
	- Stagione e week
	- Prossima partita
	- Budget disponibile
  - **Content Area (Destra)**:
	- Ospita i diversi pannelli
	- Si aggiorna con i click della sidebar

## 🎨 Stile Visivo

### Tema Colori
```
Azzurro (Accent)     → #00d4ff   Bottoni attivi
Blu Scuro            → #1a1a2e   Background principale
Blu Più Scuro        → #0f0f1e   Sidebar
Grigio Scuro         → #3a3a6e   Input/Bottoni inattivi
Grigio               → #B0B0B0   Testo secondario
Bianco               → #FFFFFF   Testo principale
```

### Font
- **Family**: Segoe UI
- **Titoli**: 28-32px, Bold
- **Sottotitoli**: 16-24px, SemiBold
- **Testo normale**: 12-14px

## 🔄 Flusso di Navigazione

```
MainWindow.xaml
	  ↓
   OnContentRendered()
	  ↓
   ShowSplashScreen()  [3 sec]
	  ↓
   ShowMainMenu()
	  ├→ Nuova Partita
	  │   ↓
	  │   ShowCoachCustomization()
	  │   ├→ Indietro → ShowMainMenu()
	  │   └→ Continua → ShowGameArea()
	  │
	  ├→ Carica (in sviluppo)
	  ├→ Impostazioni (in sviluppo)
	  └→ Esci → Exit App

   ShowGameArea()
	  ├→ Sidebar Click → ShowGameContent()
	  ├→ Salva
	  └→ Esci → ShowMainMenu()
```

## 🔧 Come Aggiungere un Nuovo View

### 1. Creare il File XAML
```xaml
<!-- FM100/Views/MyNewView.xaml -->
<UserControl x:Class="FM100.Views.MyNewView"
			 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
			 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			 Background="#1a1a2e"
			 Foreground="White">
	<Grid>
		<!-- Contenuto qui -->
	</Grid>
</UserControl>
```

### 2. Creare il Code-Behind
```csharp
// FM100/Views/MyNewView.xaml.cs
using System.Windows.Controls;

namespace FM100.Views
{
	public partial class MyNewView : UserControl
	{
		public MyNewView()
		{
			InitializeComponent();
		}
	}
}
```

### 3. Registrare in MainWindow.xaml.cs
```csharp
private void ShowMyNewView()
{
	var myView = new MyNewView();
	ViewHost.Content = myView;
}
```

## 📦 Dipendenze

- **Framework**: .NET 10
- **Tecnologia**: WPF (Windows Presentation Foundation)
- **Linguaggio**: XAML + C#

## 🎯 Componenti Utilizzati

- `UserControl` → Per i view modulari
- `Grid` → Layout principale
- `StackPanel` → Layout lineare
- `Border` → Bordi e separatori
- `Button` → Interazione
- `TextBox`, `ComboBox`, `ListBox` → Input
- `Slider` → Slider per valori
- `RadioButton` → Scelta singola
- `ContentControl` → Host dinamico

## 🚀 Status

| View | Status | Note |
|------|--------|------|
| SplashScreenView | ✅ Completato | Funzionante |
| MenuView | ✅ Completato | Funzionante |
| CoachCustomizationView | ✅ Completato | Funzionante |
| GameView | ✅ Completato | Struttura pronta |
| Dashboard | 🔜 Prossimo | In sviluppo |
| Rosa | 🔜 Prossimo | In sviluppo |
| Tattica | 🔜 Prossimo | In sviluppo |
| Trasferimenti | 🔜 Prossimo | In sviluppo |

## 📋 Checklist Sviluppatore

Quando aggiungi un nuovo view:
- [ ] Creato file `XxxView.xaml`
- [ ] Creato file `XxxView.xaml.cs`
- [ ] Namespace corretto: `FM100.Views`
- [ ] Hereda da `UserControl` (se sotto-view) o `Window` (se principale)
- [ ] Aggiunto a `MainWindow.xaml.cs` per la navigazione
- [ ] Test di compilazione passato
- [ ] Test di runtime funzionante
- [ ] Documentato in questo file

## 🎓 Risorse

- **UI_ARCHITECTURE.md** - Architettura completa
- **QUICKSTART_UI.md** - Guida rapida
- **Styles/** - Risorse di styling
- **MainWindow.xaml.cs** - Logica di navigazione

## 📝 Note

- Tutti i colori sono definiti in `Styles/ColorPalette.xaml`
- Gli stili sono in `Styles/ButtonStyles.xaml`
- Mantenere la coerenza visiva tra i view
- Usare bindable controls quando possibile (per future implementazioni MVVM)

---

**Versione**: 1.0  
**Ultimo aggiornamento**: 2024  
**Responsabile**: FM100 UI Team
