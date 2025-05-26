# Casters - Game Documentation

## 1. Overview

Casters is a 2-player fantasy Trading Card Game where heroes duel by summoning minions, casting spells, and outsmarting each other with strategic moves. Each player builds a Main deck (30 cards; only Minion and Spell types) and a Stage deck (5 cards, each granting 5 health). The game emphasizes simultaneous play, coin-determined initiative, and engaging "mind game" decision-making.

## 2. Gameplay

### Deck Construction

- **Main Deck:**
  - 30 cards in total
  - Maximum 2 copies per card
  - Cards are either Minions or Spells
- **Stage Deck:**
  - 5 cards (only one copy per card)
  - Each Stage card represents 5 health
  - Total starting health is 25

### Game Board Zones

- **Main Deck Zone:** Holds the 30-card deck
- **Stage Deck Zone:** Displays Stage cards (serving as both health and play limit)
- **Graveyard (GY):** Where discarded or used cards go
- **Coin Indicator:** Shows initiative (1 coin for first player, 2 coins for second)

### Turn Structure

#### Pre-Game Setup

1. Decide Initiative: Flip a coin
2. Deck Preparation: Each player shuffles their decks and reveals one Stage card
3. Initial Draw: Each player draws 5 cards from their Main deck

#### Turn Phases

1. **Draw Phase:** Each player draws one card from their Main deck
2. **Set Phase:** Both players simultaneously set a number of cards face-down equal to the open Stage cards on the board
3. **Reveal Phase:** Starting with the player holding the initiative coin, players reveal one face-down card at a time
   - **Options:**
     - **Play the Card:** Pay its cost; if it's a Minion, summon it (and trigger any "On Play" effects). If it's a Spell, resolve its effect and move it to the Graveyard
     - **Discard for Draw:** Discard the card to draw 2 new cards
4. **Battle Phase:** The player with initiative may attack:
   - Choose a minion to attack either the opponent's Stage deck (hero) or a tapped enemy minion
   - Continue attacking until both players choose to skip
5. **End Phase:** The turn ends. The player with 2 coins passes one coin to the opponent, thereby changing the initiative. Any damage dealt is resolved (note that Stage cards, once lost, are permanent)

### Win Condition

A player loses when all their Stage cards (health) are removed.

## 3. Current Implementation - User Interface & Navigation

### Main Menu System

The game currently implements a sophisticated menu navigation system using a single scene with multiple content panels.

#### Main Menu Controller (`MainMenuController.cs`)

- **Scene Types:** Enum-based navigation system with four main sections:

  - `Play` (index 0): Main gameplay interface
  - `Deck` (index 1): Deck management and building
  - `Solo` (index 2): Single-player content (placeholder)
  - `Shop` (index 3): Card shop interface (placeholder)

- **Navigation Features:**

  - Tab-based navigation with visual feedback
  - Button hover effects with color transitions
  - Active tab highlighting with scale and underline indicators
  - Dynamic background image fitting system

- **UI Components:**
  - `menuButtons`: List of navigation buttons
  - `underlineImages`: Visual indicators for active tabs
  - `playScreenContent`: Main menu content panel
  - `deckScreenContent`: Deck builder content panel
  - `panels`: Additional content panels for Solo/Shop

#### Play Screen (Main Menu)

**Current Implementation:**

- **Deck Selection System:** Integrated deck management directly in main menu
- **Components:**
  - `MainMenuDeckController`: Manages deck selection and display
  - `DeckBox`: Shows currently selected deck with play button
  - `PlusBox`: Appears when no decks exist, allows creating first deck
  - Deck selection popup for choosing between multiple decks

**Features:**

- **Smart Deck Display:** Automatically shows DeckBox when decks exist, PlusBox when none
- **Deck Selection Popup:** Modal interface for browsing and selecting decks
- **Play Button Integration:** Direct gameplay launch with selected deck
- **Visual Feedback:** Deck name display and selection confirmation

#### Deck Management System

**Architecture:**

- **DeckManager:** Singleton pattern managing all deck operations
- **Persistent Storage:** JSON-based save/load system
- **Card Collection:** Player-owned card tracking system

**Navigation Flow:**

1. **Main Menu → My Decks:**

   - Click Deck tab or deck management button
   - Transitions to `MyDecksUI` interface
   - Shows grid of all player decks plus "Add New Deck" option

2. **My Decks Interface (`MyDecksUI.cs`):**

   - **Grid Layout:** Alphabetically sorted deck display
   - **Add Deck Button:** Always positioned first in grid
   - **Deck Actions:** Select to edit, delete with confirmation
   - **Back Navigation:** Return to main menu

3. **Deck Builder (`DeckBuilderUI.cs`):**
   - **Editing Modes:** Create new deck or edit existing
   - **Card Management:** Add/remove cards with validation
   - **Real-time Validation:** Deck size and card limit enforcement
   - **Save System:** Persistent deck storage with name validation

**Deck Builder Features:**

- **Dual-Deck System:** Separate main deck (30 cards) and stage deck (5 cards)
- **Card Filtering:** Search and filter by card type, cost, etc.
- **Collection Integration:** Only shows owned cards
- **Visual Feedback:** Card count displays, validation warnings
- **Auto-sorting:** Alphabetical card organization

#### Canvas Management System

**Multi-Canvas Architecture:**

- **MainMenuCanvas:** Primary navigation and play interface
- **MyDecksCanvas:** Deck collection management
- **DeckBuilderCanvas:** Deck editing interface

**Navigation Controllers:**

- `MainMenuUI`: Handles canvas transitions
- `MainMenuController`: Manages tab-based content switching
- `MainMenuDeckController`: Specialized deck selection logic

### Current Scene Structure

#### MainMenu Scene

**Description:** Single scene containing all menu functionality through canvas switching

**Components:**

- **Navigation System:**

  - Tab-based interface with Play/Deck/Solo/Shop sections
  - Visual feedback with hover effects and active indicators
  - Responsive background image system

- **Deck Management:**

  - Integrated deck selection in main menu
  - Popup-based deck browser
  - Direct deck editing access
  - New deck creation workflow

- **UI Elements:**
  - Dynamic content panels based on selected tab
  - Modal popups for deck selection and confirmations
  - Responsive grid layouts for deck collections
  - Search and filter systems in deck builder

#### GameBoard Scene

**Description:** Dedicated gameplay scene (referenced but not fully implemented)

**Planned Components:**

- Game board layout with all gameplay zones
- Turn phase management system
- Card interaction and animation systems
- Combat resolution interface

### Data Management

#### Deck System (`DeckManager.cs`)

**Features:**

- **Singleton Pattern:** Global access to deck operations
- **Persistent Storage:** Automatic save/load with JSON serialization
- **Deck Validation:** Enforces all deck construction rules
- **Collection Integration:** Tracks player-owned cards
- **Editing System:** Safe copy-based editing with rollback capability

**Deck Operations:**

- Create, delete, rename decks
- Add/remove cards with validation
- Duplicate deck detection
- Active deck selection for gameplay

#### Card Collection System

**Features:**

- **Master Card Database:** ScriptableObject-based card definitions
- **Player Collection:** Tracks owned card quantities
- **Collection Initialization:** Auto-populates with all cards (2 copies each)
- **Ownership Validation:** Prevents using unowned cards in decks

## 4. Implementation Details

### Technical Architecture

- **Game Engine:** Built in Unity using Universal Render Pipeline (URP)
- **Navigation Pattern:** Single scene with canvas switching
- **Data Persistence:** JSON-based save system with ScriptableObject card definitions
- **UI Framework:** Unity UI with custom controller scripts

### Project Structure

- **Scripts Folder:**

  - `MainMenuController.cs`: Primary navigation controller
  - `MainMenuDeckController.cs`: Deck selection logic
  - `UI/`: Menu interface controllers
  - `Managers/DeckManager.cs`: Deck data management
  - `Data/`: Deck and card data structures

- **Scenes Folder:**
  - `MainMenu.unity`: Complete menu system
  - `GameBoard.unity`: Gameplay scene (in development)

### Core Systems

- **Menu Manager:** Tab-based navigation with visual feedback
- **Deck Manager:** Comprehensive deck CRUD operations
- **UI Manager:** Canvas switching and modal popup management
- **Collection Manager:** Player card ownership tracking

## 5. Development Status

### Completed Features

1. **Menu Navigation System:**

   - Complete tab-based interface
   - Canvas switching architecture
   - Visual feedback and animations

2. **Deck Management:**

   - Full CRUD operations for decks
   - Deck builder with validation
   - Collection integration
   - Persistent storage system

3. **User Interface:**
   - Responsive deck grid layouts
   - Modal popup systems
   - Search and filter functionality
   - Confirmation dialogs

### In Development

1. **Gameplay Implementation:**

   - Game board scene development
   - Turn phase management
   - Card interaction systems

2. **Additional Features:**
   - Solo play modes
   - Shop system
   - Advanced deck statistics

### Development Milestones

1. ✅ **Menu System Development:** Complete navigation and deck management
2. 🔄 **Gameplay Core:** Implementing game board and turn system
3. ⏳ **Feature Expansion:** Solo modes and shop integration
4. ⏳ **Multiplayer Integration:** Network play or advanced AI

## 6. Navigation Flow Summary
