# 🎮 Guida Rapida: Navigazione UI FM100

## Come Funziona l'Interfaccia

### ⚡ Avvio Applicazione

1. **Splash Screen** (3 secondi)
   - Logo di FM100
   - Loading bar
   - Transizione automatica al Menu

2. **Menu Principale**
   - 🎮 **Nuova Partita** → Personalizza allenatore
   - 📂 **Carica Partita** → Riprendi una partita salvata
   - ⚙️ **Impostazioni** → Regola le impostazioni
   - ❌ **Esci** → Chiudi l'app

---

## 📝 Personalizzazione Allenatore

Quando clicchi **Nuova Partita**, devi:

### 1. Configurare l'Allenatore
- **Nome**: Inserisci il nome del tuo allenatore
- **Nazionalità**: Scegli da una dropdown
- **Età**: Usa lo slider (25-70 anni)
- **Stile Tattico**: Scegli uno dei tre:
  - 🔴 Offensivo
  - 🟡 Equilibrato (default)
  - 🔵 Difensivo

### 2. Scegliere la Squadra
- **Divisione**: Seleziona Serie A, B o C
- **Lista**: Appare una lista di squadre disponibili
- Clicca su una squadra per selezionarla

### 3. Iniziare la Partita
- Clicca **Continua** per accedere all'area di gioco

---

## 🎮 Area di Gioco

### Layout Principale

```
┌─────────────────────────────────────────┐
│  📊 Stagione 1 - Week 1 | 🎯 Next Match │
├─────────────┬─────────────────────────────┤
│             │                             │
│  SIDEBAR    │   CONTENUTO PRINCIPALE      │
│  (Menu)     │   (Dettagli, Tabelle, Ecc.)│
│             │                             │
└─────────────┴─────────────────────────────┘
```

### Sidebar Sinistra

Usa i bottoni per navigare tra le sezioni:

| Bottone | Funzione |
|---------|----------|
| 📊 Dashboard | Panoramica del team |
| 👥 Rosa | Gestione giocatori |
| 🎯 Tattica | Formazione e tattica |
| 🔄 Trasferimenti | Mercato calciatori |
| 📅 Calendario | Fixture e partite |
| 📈 Classifica | Standings |
| 💰 Finanze | Budget e economia |
| ⚙️ Impostazioni | Impostazioni gioco |
| 💾 Salva | Salva la partita |
| ❌ Esci | Torna al menu |

### Header Superiore

Mostra informazioni importanti:
- 🏆 **Stagione e Week**: Avanzamento nel gioco
- 🎯 **Prossima Partita**: Chi gioca dopo
- 💰 **Budget**: Soldi disponibili

### Content Area (Destra)

Cambia contenuto in base al bottone cliccato:
- **Dashboard** → Overview team
- **Rosa** → Lista giocatori
- **Tattica** → Formazione
- **Trasferimenti** → Mercato
- **Calendario** → Partite
- **Classifica** → Standings
- **Finanze** → Budget

---

## 🎨 Colori e Design

### Tema Colori
- 🟦 **Azzurro** (#00d4ff) → Elementi attivi
- 🟦 **Blu scuro** (#1a1a2e) → Background
- 🟦 **Blu più scuro** (#0f0f1e) → Sidebar
- ⚪ **Grigio** (#B0B0B0) → Testo secondario

### Pulsanti
- **Hover**: Cambiano colore e si ingrandiscono leggermente
- **Click**: Effetto di pressione

---

## 📱 Scorciatoie Tastiera (Future)

Verranno aggiunte:
- `Ctrl+S` → Salva
- `Esc` → Torna al menu
- `F1` → Aiuto
- `F5` → Aggiorna dati

---

## 🔧 Troubleshooting

### L'app non si avvia
- Controlla che .NET 10 sia installato
- Esegui: `dotnet build` e `dotnet run`

### I bottoni non rispondono
- Prova a cliccare di nuovo
- Controlla la console per messaggi di errore

### Le immagini di sfondo non appaiono
- Aggiungi i file PNG nella cartella `Assets/`
- Rebuild il progetto

---

## 🚀 Prossimamente

- [ ] Salvataggio automatico
- [ ] Sistema di notifiche
- [ ] Effetti sonori
- [ ] Temi (Light/Dark)
- [ ] Localizzazione (English, Spanish, etc.)
- [ ] Tutorial interattivo
- [ ] Hotkeys
- [ ] Screenshot e replay

---

**Versione**: 1.0  
**Piattaforma**: .NET 10 WPF  
**Sviluppatore**: FM100 Team
