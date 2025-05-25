# Complete Deck Editing System Implementation

This document describes the comprehensive deck editing system that has been implemented for the Unity card game project.

## Overview

The deck editing system allows players to:

- View their deck collection
- Create, edit, and delete decks
- Add/remove cards with right-click interactions
- Auto-save deck changes
- Maintain proper deck limits and validation
- See visual copy indicators (white for deck cards, gray for collection)

## Key Features Implemented

### 1. Dual View System

- **Deck Selection View**: Shows grid of saved decks with "Add New Deck" button
- **Deck Editing View**: Full deck editor with main deck, stage deck, and card collection

### 2. Deck Name Editing

- Editable input field in the Header GameObject (deck editing view only)
- Auto-save when name changes
- Auto-naming "Deck" if empty name but has cards
- Display-only names in deck selection view

### 3. Card Management

- **Right-click to add**: Collection cards → Deck
- **Right-click to remove**: Deck cards → Collection
- **Alphabetical sorting**: Cards maintain alphabetical order by title
- **Proper validation**: Respects deck limits (30 main, 5 stage, max 2 copies per main card, 1 per stage)

### 4. Copy Indicators

- **White circles**: Cards currently in the deck being edited
- **Gray circles**: Remaining copies in player collection
- **Transparency**: 70% for cards with all copies in deck, 35% for unowned cards

### 5. Auto-Save System

- Saves automatically when:
  - Deck name changes
  - Cards are added/removed
  - Navigating back from deck editor
- Conditions: Has cards OR non-empty name
- Auto-naming: "Deck" if empty name but has cards

### 6. Navigation

- **Back Button**: Returns to deck selection, auto-saves first
- **Trash Button**: Deletes current deck (no confirmation for now)

## Technical Implementation

### Updated Files

#### DeckBuilderUI.cs

- Added deck editing UI elements (mainDeckContainer, stageDeckContainer, deckNameInput, backButton, trashButton)
- Implemented dual view system with `ShowDeckListView()` and `ShowDeckEditingView()`
- Added deck loading/saving logic with `LoadDeckForEditing()` and `AutoSaveDeck()`
- Implemented card add/remove logic with proper validation
- Enhanced copy indicator system with color coding
- Added alphabetical sorting for deck card placement

#### CardItemUI.cs

- Added right-click event support with `onRightClick` Action
- Implemented custom copy indicator methods:
  - `ShowCopyIndicators(count, color)` - Clear and show new indicators
  - `ShowAdditionalCopyIndicators(count, color)` - Add without clearing
  - `SetTransparency(alpha)` - Control card transparency
- Enhanced `CreateCardUI()` with overloaded methods
- Added `CardData` property for easy access

### Deck Management Logic

#### Deck Limits

- **Main Deck**: 30 cards maximum, 2 copies per card maximum
- **Stage Deck**: 5 cards maximum, 1 copy per card maximum
- **Player Collection**: Tracks owned copies, prevents adding unowned cards

#### Sorting Algorithm

Cards are inserted maintaining alphabetical order using `string.Compare()` with case-insensitive comparison.

#### Copy Indicator Logic

```csharp
// Calculate indicators
int cardsInDeck = (main deck count) + (stage deck count)
int remainingCopies = ownedCopies - cardsInDeck

// Show white for deck cards, gray for remaining
if (cardsInDeck > 0) ShowCopyIndicators(cardsInDeck, Color.white)
if (remainingCopies > 0) ShowCopyIndicators(remainingCopies, Color.gray)
```

## Usage Instructions

### For Unity Setup

1. Assign the new UI elements in the DeckBuilderUI inspector:

   - `mainDeckContainer` - Container for main deck cards
   - `stageDeckContainer` - Container for stage deck cards
   - `deckNameInput` - TMP_InputField for deck name editing
   - `backButton` - Button to return to deck selection
   - `trashButton` - Button to delete current deck

2. Ensure proper UI hierarchy:
   - Deck editing elements should be hidden by default
   - Copy indicator containers are created automatically

### For Players

1. **Deck Selection**: Click any deck to enter editing mode
2. **Deck Editing**:
   - Edit name in the header input field
   - Right-click collection cards to add to deck
   - Right-click deck cards to remove from deck
   - Use back button to return (auto-saves)
   - Use trash button to delete deck

## Integration Notes

- Compatible with existing `DeckManager.cs` and `PlayerCollection` system
- Uses existing card data and prefab-free card creation
- Maintains backward compatibility with existing deck selection UI
- Integrates with existing search and filtering functionality

## Future Enhancements

Planned features mentioned by user but not yet implemented:

- Animation for card movement
- Confirmation dialogs for deck deletion
- Enhanced deck box visual styling
- Drag & drop card interaction
- Advanced deck validation rules

## Error Handling

The system includes comprehensive error handling:

- Validation for missing references
- Checks for deck limits before adding cards
- Handles null deck scenarios gracefully
- Provides debug logging for troubleshooting
- Falls back to safe defaults when possible
