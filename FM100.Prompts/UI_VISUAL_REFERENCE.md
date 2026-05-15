# 🎬 FM100 UI - Visual Reference Guide

## 📐 Layout Architecture

### Full Application Flow

```
╔══════════════════════════════════════════════════════════════════════╗
║                         FM100 Application                            ║
║                   (1400x900 MainWindow)                              ║
╠══════════════════════════════════════════════════════════════════════╣
║                                                                      ║
║              ┌─────────────────────────────────────────┐             ║
║              │  DYNAMIC VIEW CONTAINER (ContentControl)│             ║
║              │                                         │             ║
║              │  Hosts:                                 │             ║
║              │  • SplashScreenView (initial)           │             ║
║              │  • MenuView (main menu)                 │             ║
║              │  • CoachCustomizationView (setup)       │             ║
║              │  • GameView (main gameplay)             │             ║
║              │                                         │             ║
║              └─────────────────────────────────────────┘             ║
║                                                                      ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## 🎬 SplashScreenView (3 seconds)

```
┌──────────────────────────────────────────────────┐
│                                                  │
│              🎮 FOOTBALL MANAGER                │
│           MASTER LEAGUE 100 YEARS               │
│                                                  │
│                                                  │
│           ╔══════════════════════════╗           │
│           ║ ▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░░░ ║           │
│           ╚══════════════════════════╝           │
│                  Loading...                      │
│                                                  │
│         [Background Image - Team Action]        │
│                                                  │
└──────────────────────────────────────────────────┘

Colore: Sfondo scuro (#1a1a2e)
Overlay: Nero 40% trasparenza
Durata: ~3 secondi
```

---

## 🎮 MenuView

```
┌──────────────────────────────────────────────────┐
│                                                  │
│          FOOTBALL MANAGER MASTER LEAGUE         │
│                                                  │
│                                                  │
│     ┌────────────────────────────────────┐      │
│     │  🎮 NUOVA PARTITA                   │      │
│     └────────────────────────────────────┘      │
│     (Azzurro #00d4ff - Testo nero)              │
│                                                  │
│     ┌────────────────────────────────────┐      │
│     │  📂 CARICA PARTITA                  │      │
│     └────────────────────────────────────┘      │
│     (Blu #3a3a6e - Bordo azzurro)               │
│                                                  │
│     ┌────────────────────────────────────┐      │
│     │  ⚙️ IMPOSTAZIONI                    │      │
│     └────────────────────────────────────┘      │
│     (Blu #3a3a6e - Bordo azzurro)               │
│                                                  │
│     ┌────────────────────────────────────┐      │
│     │  ❌ ESCI                            │      │
│     └────────────────────────────────────┘      │
│     (Blu #3a3a6e - Bordo grigio)                │
│                                                  │
│         [Background Image - Stadium]            │
│                                                  │
└──────────────────────────────────────────────────┘

Bottoni: 50px height, 16px font
Margin: 10px tra bottoni
Hover: +5% zoom, colore più chiaro
```

---

## 👔 CoachCustomizationView

```
┌─────────────────────────────────────────────────┐
│                                                 │
│    PERSONALIZZA IL TUO ALLENATORE               │
│                                                 │
│  ┌────────────────────────────────────────┐   │
│  │ Nome Allenatore                         │   │
│  │ [________________________]              │   │
│  └────────────────────────────────────────┘   │
│                                                 │
│  ┌────────────────────────────────────────┐   │
│  │ Nazionalità                             │   │
│  │ [▼ Italia            ]                  │   │
│  └────────────────────────────────────────┘   │
│                                                 │
│  ┌────────────────────────────────────────┐   │
│  │ Età: [════════●───────] 45 anni        │   │
│  └────────────────────────────────────────┘   │
│                                                 │
│  ┌────────────────────────────────────────┐   │
│  │ Stile Tattico                           │   │
│  │ ◉ Offensivo  ○ Equilibrato  ○ Difensivo│   │
│  └────────────────────────────────────────┘   │
│                                                 │
│  ───────────────────────────────────────────   │
│                                                 │
│  SCEGLI LA TUA SQUADRA                         │
│                                                 │
│  ┌────────────────────────────────────────┐   │
│  │ Divisione: [▼ Serie A       ]          │   │
│  └────────────────────────────────────────┘   │
│                                                 │
│  ┌────────────────────────────────────────┐   │
│  │ • Team A                                │   │
│  │ • Team B                                │   │
│  │ • Team C                                │   │
│  │ • Team D (selected)                     │   │
│  │ • Team E                                │   │
│  │ • Team F                                │   │
│  └────────────────────────────────────────┘   │
│                                                 │
│  [Indietro]                    [Continua ▶]   │
│                                                 │
└─────────────────────────────────────────────────┘

Scroll area per lista squadre
InputBox: 45px height, padding 15px
ComboBox: 45px height, padding 15px
Font: Segoe UI, 14px
```

---

## 🕹️ GameView - Layout Principale

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                          HEADER BAR (60px)                                    ║
║  📊 Stagione 1 - Week 1  |  🎯 Prossima: Team A vs Team B  |  💰 €5.000.000  ║
╠═════════════════════════════╦═════════════════════════════════════════════════╣
║                             ║                                                 ║
║        SIDEBAR (200px)      ║            CONTENT AREA (Dinamico)             ║
║   (Background #0f0f1e)      ║                                                 ║
║                             ║                                                 ║
║  ┌─────────────────────┐   ║  ╔════════════════════════════════════╗         ║
║  │   TEAM LOGO (80x80) │   ║  ║  [Contenuto di Dashboard/Rosa/     ║         ║
║  │       My Team       │   ║  ║   Tattica/Trasferimenti/ecc]      ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║                             ║  ║  Aggiornato dinamicamente         ║         ║
║  ┌─────────────────────┐   ║  ║  in base al bottone cliccato      ║         ║
║  │ 📊 Dashboard  ■◄─┐ │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║  ┌─────────────────────┐   ║  ║                                    ║         ║
║  │ 👥 Rosa            │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║  ┌─────────────────────┐   ║  ║                                    ║         ║
║  │ 🎯 Tattica         │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║  ┌─────────────────────┐   ║  ║                                    ║         ║
║  │ 🔄 Trasferimenti   │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║  ┌─────────────────────┐   ║  ║                                    ║         ║
║  │ 📅 Calendario      │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║  ┌─────────────────────┐   ║  ║                                    ║         ║
║  │ 📈 Classifica      │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║  ┌─────────────────────┐   ║  ║                                    ║         ║
║  │ 💰 Finanze         │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║                             ║  ║                                    ║         ║
║  ───────────────────────── ║  ║                                    ║         ║
║                             ║  ║                                    ║         ║
║  ┌─────────────────────┐   ║  ║                                    ║         ║
║  │ ⚙️ Impostazioni    │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║  ┌─────────────────────┐   ║  ║                                    ║         ║
║  │ 💾 Salva           │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ║                                    ║         ║
║  ┌─────────────────────┐   ║  ║                                    ║         ║
║  │ ❌ Esci al Menu    │   ║  ║                                    ║         ║
║  └─────────────────────┘   ║  ╚════════════════════════════════════╝         ║
║                             ║                                                 ║
╚═════════════════════════════╩═════════════════════════════════════════════════╝

Sidebar:
- Width: 200px (fisso)
- Background: #0f0f1e
- Border-Right: 2px #00d4ff
- Button Height: 50px
- Margin: 10px

Header:
- Height: 60px
- Background: #0f0f1e
- Border-Bottom: 1px #00d4ff
- Padding: 20px

Content:
- Flex (remainder)
- Margin: 20px
- Background: #1a1a2e
```

---

## 🔄 Navigation Flow Diagram

```
					┌─ APPLICAZIONE AVVIA
					│
					▼
		   ╔════════════════════╗
		   ║  SPLASH SCREEN     ║
		   ║  (3 secondi)       ║
		   ╚════════════════════╝
					│
					▼ (auto)
		   ╔════════════════════╗
		   ║   MENU PRINCIPALE  ║
		   ╚════════════════════╝
					│
		┌───────────┼───────────┬──────────┐
		│           │           │          │
		▼           ▼           ▼          ▼
	NUOVA PT    CARICA     IMPOSTAZIONI   ESCI
		│       (TODO)      (TODO)        │
		│                                 ▼
		│                         EXIT APPLICATION
		│
		▼
   ╔═════════════════════════╗
   ║ COACH CUSTOMIZATION     ║
   ╠═════════════════════════╣
   ║ • Nome Allenatore       ║
   ║ • Nazionalità           ║
   ║ • Età                   ║
   ║ • Stile Tattico         ║
   ║ • Selezione Squadra     ║
   ╚═════════════════════════╝
		│          │
		▼ Back     ▼ Continue
   MENU PRINCIPALE  GAME AREA
						│
						▼
				   ╔════════════════╗
				   ║    GAME VIEW   ║
				   ╠════════════════╣
				   ║ SIDEBAR        ║
				   ║ • Dashboard    ║
				   ║ • Rosa         ║
				   ║ • Tattica      ║
				   ║ • Transfer     ║ ─────► CONTENT AREA
				   ║ • Calendario   ║       (si aggiorna)
				   ║ • Classifica   ║
				   ║ • Finanze      ║
				   ║ • Settings     ║
				   ║ • Salva        ║
				   ║ • Esci         ◄─────┐
				   ╚════════════════╝      │
						│                 │
						└─ MENU PRINCIPALE─┘
```

---

## 🎨 Color Palette Reference

```
╔══════════════════════════════════════════╗
║        FM100 COLOR PALETTE               ║
╠══════════════════════════════════════════╣
║                                          ║
║  PRIMARY ACCENT                          ║
║  ■ #00d4ff  Azzurro Bright               ║
║    Uso: Bottoni attivi, highlight       ║
║                                          ║
║  PRIMARY BACKGROUND                      ║
║  ■ #1a1a2e  Blu Scuro                   ║
║    Uso: Background principale app       ║
║                                          ║
║  SECONDARY BACKGROUND                    ║
║  ■ #0f0f1e  Blu Più Scuro               ║
║    Uso: Sidebar, header, navbar         ║
║                                          ║
║  TERTIARY BACKGROUND                     ║
║  ■ #3a3a6e  Grigio-Blu Scuro            ║
║    Uso: Input fields, inattivi         ║
║                                          ║
║  TEXT SECONDARY                          ║
║  ■ #B0B0B0  Grigio Chiaro               ║
║    Uso: Etichette, testo secondario    ║
║                                          ║
║  TEXT PRIMARY                            ║
║  ■ #FFFFFF  Bianco                      ║
║    Uso: Testo principale                ║
║                                          ║
╚══════════════════════════════════════════╝

Contrasto verificato: AAA compliant
Accessibilità: ✅ Conforme WCAG
```

---

## 🔘 Button States

```
┌─────────────────────────────────────────────────────┐
│ MENU BUTTON STYLE                                   │
├─────────────────────────────────────────────────────┤
│                                                     │
│ DEFAULT STATE                                      │
│ ┌─────────────────────────────────────────┐        │
│ │  🎮 NUOVA PARTITA                       │        │
│ │  Background: #00d4ff (Azzurro)         │        │
│ │  Text: Nero                             │        │
│ │  Font: SemiBold, 16px                   │        │
│ └─────────────────────────────────────────┘        │
│                                                     │
│ HOVER STATE                                        │
│ ┌─────────────────────────────────────────┐        │
│ │  🎮 NUOVA PARTITA  ◄─ Zoom 1.05x       │        │
│ │  Background: #00e6ff (Più chiaro)      │        │
│ │  Text: Nero                             │        │
│ │  Cursor: Mano                           │        │
│ └─────────────────────────────────────────┘        │
│                                                     │
│ PRESSED STATE                                      │
│ ┌─────────────────────────────────────────┐        │
│ │  🎮 NUOVA PARTITA                       │        │
│ │  Background: #00b3d4 (Più scuro)       │        │
│ │  Text: Nero                             │        │
│ │  Effetto: Pressione                     │        │
│ └─────────────────────────────────────────┘        │
│                                                     │
├─────────────────────────────────────────────────────┤
│ SIDEBAR BUTTON STYLE                                │
├─────────────────────────────────────────────────────┤
│                                                     │
│ DEFAULT STATE                                      │
│ ┌─────────────────────────────────────────┐        │
│ │  📊 Dashboard                           │        │
│ │  Background: #3a3a6e (Grigio-Blu)      │        │
│ │  Border: 1px #00d4ff                    │        │
│ │  Text: Bianco                           │        │
│ │  Font: Regular, 12px                    │        │
│ └─────────────────────────────────────────┘        │
│                                                     │
│ HOVER STATE                                        │
│ ┌─────────────────────────────────────────┐        │
│ │  📊 Dashboard                           │        │
│ │  Background: #00d4ff (Azzurro)         │        │
│ │  Border: 1px #00d4ff                    │        │
│ │  Text: Nero ◄─ Cambio colore           │        │
│ │  Cursor: Mano                           │        │
│ └─────────────────────────────────────────┘        │
│                                                     │
│ PRESSED STATE                                      │
│ ┌─────────────────────────────────────────┐        │
│ │  📊 Dashboard                           │        │
│ │  Background: #0099aa (Più scuro)       │        │
│ │  Border: 1px #00d4ff                    │        │
│ │  Text: Nero                             │        │
│ │  Effetto: Pressione                     │        │
│ └─────────────────────────────────────────┘        │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## 📏 Typography

```
╔════════════════════════════════════════════════════╗
║ FONT FAMILY: Segoe UI (System Font)               ║
╠════════════════════════════════════════════════════╣
║                                                    ║
║ HEADING 1 (Page Title)                           ║
║ Size: 32px | Weight: Bold | Color: #00d4ff      ║
║ "Personalizza il Tuo Allenatore"                 ║
║                                                    ║
║ HEADING 2 (Section Title)                        ║
║ Size: 24px | Weight: Bold | Color: #00d4ff      ║
║ "Scegli la Tua Squadra"                          ║
║                                                    ║
║ BUTTON TEXT                                       ║
║ Size: 14-16px | Weight: SemiBold | Color: Var   ║
║ "Continua" | "Indietro" | "Salva"               ║
║                                                    ║
║ BODY TEXT                                         ║
║ Size: 14px | Weight: Regular | Color: #FFFFFF   ║
║ "Seleziona la tua squadra..."                     ║
║                                                    ║
║ LABEL TEXT                                        ║
║ Size: 12px | Weight: Regular | Color: #B0B0B0   ║
║ "Nome Allenatore" | "Nazionalità"                ║
║                                                    ║
╚════════════════════════════════════════════════════╝
```

---

## 🎯 Responsive Layout

```
╔════════════════════════════════════════════════╗
║ BREAKPOINTS                                    ║
╠════════════════════════════════════════════════╣
║                                                ║
║ MINIMUM (1280x720)                             ║
║  • Sidebar: 200px (still visible)              ║
║  • Content: scales dynamically                 ║
║  • Fonts: 12-14px (readable)                   ║
║                                                ║
║ RECOMMENDED (1920x1080)                        ║
║  • Sidebar: 200px                              ║
║  • Content: full width                         ║
║  • Fonts: 14-16px (optimal)                    ║
║                                                ║
║ MAXIMUM (2560x1440)                            ║
║  • Sidebar: 200px (fixed)                      ║
║  • Content: max-width applied                  ║
║  • Fonts: 14-16px (maintained)                 ║
║                                                ║
╚════════════════════════════════════════════════╝
```

---

## 🚀 Performance

```
╔═══════════════════════════════════════════════╗
║ LOAD TIMES (Estimated)                        ║
╠═══════════════════════════════════════════════╣
║                                               ║
║ App Start → Splash    : ~500ms                ║
║ Splash → Menu         : ~100ms                ║
║ Menu → Customization  : ~150ms                ║
║ Customization → Game  : ~200ms                ║
║ View Transitions      : ~50ms                 ║
║                                               ║
║ TOTAL STARTUP TIME    : ~2 seconds            ║
║ (Plus splash delay)   : +3 seconds            ║
║ Total Time to Game    : ~5 seconds            ║
║                                               ║
╚═══════════════════════════════════════════════╝
```

---

**Visual Guide v1.0 - FM100 UI**  
Complete reference for all UI layouts, colors, and interactions.
