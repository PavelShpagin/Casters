Casters - Game Documentation

1. Overview
Casters is a 2-player fantasy Trading Card Game where heroes duel by summoning minions,
casting spells, and outsmarting each other with strategic moves. Each player builds a Main deck
(30 cards; only Minion and Spell types) and a Stage deck (5 cards, each granting 5 health). The
game emphasizes simultaneous play, coin-determined initiative, and engaging "mind game"
decision-making.

2. Gameplay
Deck Construction
● Main Deck:
○ 30 cards in total
○ Maximum 2 copies per card
○ Cards are either Minions or Spells
● Stage Deck:
○ 5 cards (only one copy per card)
○ Each Stage card represents 5 health
○ Total starting health is 25

Game Board Zones
● Main Deck Zone: Holds the 30-card deck.
● Stage Deck Zone: Displays Stage cards (serving as both health and play limit).

● Graveyard (GY): Where discarded or used cards go.
● Coin Indicator: Shows initiative (1 coin for first player, 2 coins for second).

Turn Structure
Pre-Game Setup
1. Decide Initiative: Flip a coin.
2. Deck Preparation: Each player shuffles their decks and reveals one Stage card.
3. Initial Draw: Each player draws 5 cards from their Main deck.

Turn Phases
1. Draw Phase:
○ Each player draws one card from their Main deck.
2. Set Phase:
○ Both players simultaneously set a number of cards face-down equal to the open
Stage cards on the board.

3. Reveal Phase:
○ Starting with the player holding the initiative coin, players reveal one face-down
card at a time.
○ Options:
■ Play the Card: Pay its cost; if it's a Minion, summon it (and trigger any
"On Play" effects). If it's a Spell, resolve its effect and move it to the
Graveyard.
■ Discard for Draw: Discard the card to draw 2 new cards.

4. Battle Phase:
○ The player with initiative may attack:

■ Choose a minion to attack either the opponent's Stage deck (hero) or a
tapped enemy minion.
■ Continue attacking until both players choose to skip.

5. End Phase:
○ The turn ends. The player with 2 coins passes one coin to the opponent, thereby
changing the initiative. Any damage dealt is resolved (note that Stage cards,
once lost, are permanent).

Win Condition
● A player loses when all their Stage cards (health) are removed.

3. User Interface & Scenes
Main Menu Scene
● Description:
The entry point of the game. Provides navigation to start a game, view your collection,
and learn more about Casters.
● Components:
○ Buttons:
■ Start a Game: Transitions to the Game Board scene.
■ View Collection: Opens the Decks scene.
■ About the Game: Displays a pop-up window with game information.
○ Background & Branding:
■ A thematic background that reflects the fantasy style of the game.
■ Title/logo of Casters prominently displayed.

Game Board Scene

● Description:
The main gameplay scene where matches take place. It includes all the gameplay
zones (Main Deck, Stage Deck, Graveyard, Coin Indicator) and manages turn phases.
● Components:
○ Board Layout:
■ Visual areas for the Main Deck, Stage Deck, Graveyard, and a
designated area for player hands.

○ Buttons & UI Elements:
■ End Phase Button: Allows players to signal the completion of their
phase.
■ Phase Indicators: Visual cues (or text) showing the current phase (Draw,
Set, Reveal, Battle, End).
■ Coin Indicator: Displays current initiative (1 coin or 2 coins).
○ Gameplay Elements:
■ Card animations for drawing, setting, revealing, and attacking.
■ Feedback for actions (e.g., damage numbers, card highlights).

Decks Scene (Collection)
● Description:
A screen where players can view and manage their card collections.
● Components:
○ Card Library:
■ A scrollable area or grid displaying all cards owned.
○ Navigation Buttons:
■ Options to view details of individual cards.

■ A back button to return to the Main Menu.

Deck Builder Scene (Deck)
● Description:
A scene dedicated to building and customizing decks for play.
● Components:
○ Deck Editor:
■ Drag-and-drop functionality to add cards from the collection to your Main
deck or Stage deck.

○ Deck Overview:
■ Display current deck statistics (number of cards, copies per card, total
health from Stage cards).

○ Save Options:
■ Buttons to save the deck.
○ Guidance & Tooltips:
■ Instructions or hints to help new players build valid decks (e.g., ensuring
only 30 cards in the Main deck).

4. Implementation Details
Technical Architecture
● Game Engine:
○ Built in Unity using the Universal Render Pipeline (URP) for 3D visuals.
● Project Structure:
○ Scripts Folder:

■ GameFlow: Controls turn phases and overall match management.
■ Cards: Contains definitions (ScriptableObjects) and CardView scripts.
■ Players: Manages player states, deck operations, and zone management.
■ UI: Handles all user interface elements (menus, buttons, phase
indicators).
○ Prefabs Folder:
■ Contains reusable assets such as Card Prefabs and UI components.
○ Scenes Folder:
■ Main Menu, Game Board, Decks, and Deck Builder scenes.
○ Art & Audio Folders:
■ Store 3D models, textures, icons, and sound effects.

Core Systems
● Game Manager:
○ Oversees game state transitions (pre-game setup, turn phases, win conditions).
● Deck & Hand Managers:
○ Control card drawing, shuffling, and hand organization.
● Board Manager:
○ Manages card placement on the board and resolves combat.
● UI Manager:
○ Updates interface elements according to game state (health, coins, current
phase).

5. Development Milestones
1. Prototype Development:
○ Build Menu and Game scenes.
○ Implement Game, Deck, Hand, and Board Manager classes.
○ Add core gameplay loop (setup, turn phases, win condition) using placeholder
assets.
2. Gameplay Refinement:
○ Integrate card mechanics and a more fine-grained gameplay.
3. Deck Builder Integration:
○ Build Decks, Deck Builder, and connect with the Main Menu and Game Board
scenes.

4. (Optional) Multiplayer or AI Integration:
○ Expand from local play to include networked matches or a more advanced AI
opponent. 