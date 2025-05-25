using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq; // For LINQ queries like FirstOrDefault, Any

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    public UserDecks currentUserDecks = new UserDecks();
    public PlayerCollection playerCollection = new PlayerCollection();
    
    // Current deck management
    public string currentSelectedDeckID = "";
    
    private string saveFileName = "userDecks.json";
    private string collectionSaveFileName = "playerCollection.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
    private string CollectionSavePath => Path.Combine(Application.persistentDataPath, collectionSaveFileName);

    // This would hold all available cards in the game, loaded from ScriptableObjects
    public List<CardData> allMasterCardsList = new List<CardData>();

    // Currently being edited deck (a temporary copy)
    public Deck currentEditingDeck;
    public string originalEditingDeckName; // To check if name changed

    public Deck ActiveGameplayDeck { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make it a persistent singleton
            LoadAllMasterCards();
            LoadDecks();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadAllMasterCards()
    {
        // Load all CardData ScriptableObjects from a specific folder in Resources
        // Ensure your CardData .asset files are in a folder named "Resources/CardsCollection" (or any subfolder within Resources)
        var loadedCards = Resources.LoadAll<CardData>("CardsCollection");
        allMasterCardsList = new List<CardData>(loadedCards);
        Debug.Log($"Loaded {allMasterCardsList.Count} cards from Resources/CardsCollection.");
        if (allMasterCardsList.Count == 0)
        {
            Debug.LogWarning("No CardData assets found in Resources/CardsCollection. Make sure they are there and correctly configured.");
        }
        
        // Initialize player collection with all cards (2 copies each for now)
        LoadPlayerCollection();
        if (playerCollection.ownedCards.Count == 0)
        {
            playerCollection.InitializeWithAllCards(allMasterCardsList, 2);
            SavePlayerCollection();
        }
    }

    #region Deck Management Methods

    public List<Deck> GetAllDecks()
    {
        if (currentUserDecks != null && currentUserDecks.allDecks != null)
        {
            return new List<Deck>(currentUserDecks.allDecks); // Return a copy
        }
        return new List<Deck>(); // Return an empty list if null
    }

    public Deck CreateNewDeck(string deckName = "")
    {
        string nameToUse = string.IsNullOrEmpty(deckName) ? GetUniqueUntitledDeckName() : deckName;
        if (IsDeckNameTaken(nameToUse))
        {
            Debug.LogWarning($"Deck name '{nameToUse}' is already taken. Cannot create new deck with this name.");
            return null; // Or handle with a numbered suffix if trying to use an existing name
        }

        Deck newDeck = new Deck(nameToUse);
        currentUserDecks.allDecks.Add(newDeck);
        // SaveDecks(); // Optional: save immediately or wait for explicit save action
        Debug.Log($"Created new deck: {newDeck.deckName} with ID: {newDeck.uniqueID}");
        return newDeck;
    }

    public bool DeleteDeck(string uniqueID)
    {
        Deck deckToRemove = currentUserDecks.allDecks.FirstOrDefault(d => d.uniqueID == uniqueID);
        if (deckToRemove != null)
        {
            currentUserDecks.allDecks.Remove(deckToRemove);
            // SaveDecks(); // Optional: save immediately
            Debug.Log($"Deleted deck: {deckToRemove.deckName}");
            return true;
        }
        Debug.LogWarning($"Could not find deck with ID: {uniqueID} to delete.");
        return false;
    }

    public Deck GetDeck(string uniqueID)
    {
        return currentUserDecks.allDecks.FirstOrDefault(d => d.uniqueID == uniqueID);
    }

    public bool IsDeckNameTaken(string name, string excludeDeckID = null)
    {
        return currentUserDecks.allDecks.Any(d => d.deckName == name && (excludeDeckID == null || d.uniqueID != excludeDeckID));
    }

    public string GetUniqueUntitledDeckName()
    {
        int count = 1;
        string baseName = "Untitled Deck";
        string currentName = $"{baseName} {count}";
        while (IsDeckNameTaken(currentName))
        {
            count++;
            currentName = $"{baseName} {count}";
        }
        return currentName;
    }

    // Call this when starting to edit a deck to work on a copy
    public void StartEditingDeck(string deckID)
    {
        Deck sourceDeck = GetDeck(deckID);
        if (sourceDeck != null)
        {
            // Deep copy the deck to avoid modifying the original until save
            currentEditingDeck = new Deck(sourceDeck.deckName, sourceDeck.uniqueID);
            currentEditingDeck.mainDeckCards = new Dictionary<CardData, int>(sourceDeck.mainDeckCards);
            currentEditingDeck.stageDeckCards = new Dictionary<CardData, int>(sourceDeck.stageDeckCards);
            originalEditingDeckName = sourceDeck.deckName;
            Debug.Log($"Started editing deck: {currentEditingDeck.deckName}");
        }
        else
        {
            Debug.LogError($"Cannot start editing. Deck with ID {deckID} not found.");
            currentEditingDeck = null;
        }
    }

    // Call this to save changes from currentEditingDeck back to the main list
    public bool SaveEditedDeck(string newDeckName)
    {
        if (currentEditingDeck == null)
        {
            Debug.LogError("No deck is currently being edited.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(newDeckName))
        {
            Debug.LogError("Deck name cannot be empty.");
            return false; // Or provide a default name
        }

        // Check if new name is taken by another deck
        if (newDeckName != originalEditingDeckName && IsDeckNameTaken(newDeckName, currentEditingDeck.uniqueID))
        {
            Debug.LogError($"Deck name '{newDeckName}' is already taken by another deck.");
            return false;
        }

        Deck deckToUpdate = GetDeck(currentEditingDeck.uniqueID);
        if (deckToUpdate != null)
        {
            deckToUpdate.deckName = newDeckName;
            deckToUpdate.mainDeckCards = new Dictionary<CardData, int>(currentEditingDeck.mainDeckCards);
            deckToUpdate.stageDeckCards = new Dictionary<CardData, int>(currentEditingDeck.stageDeckCards);
            
            SaveDecks(); // Persist changes to file
            Debug.Log($"Saved changes to deck: {deckToUpdate.deckName}");
            currentEditingDeck = null; // Clear editing state
            originalEditingDeckName = string.Empty;
            return true;
        }
        else
        {
            Debug.LogError($"Could not find original deck with ID {currentEditingDeck.uniqueID} to save changes.");
            return false;
        }
    }

    public void DiscardDeckChanges()
    {
        currentEditingDeck = null;
        originalEditingDeckName = string.Empty;
        Debug.Log("Discarded changes to the currently edited deck.");
    }

    public void SetActiveGameplayDeck(string deckID)
    {
        Deck deckToPlay = GetDeck(deckID);
        if (deckToPlay != null)
        {
            // Create a deep copy of the deck for gameplay
            ActiveGameplayDeck = new Deck(deckToPlay.deckName, deckToPlay.uniqueID); 
            ActiveGameplayDeck.mainDeckCards = new Dictionary<CardData, int>(deckToPlay.mainDeckCards);
            ActiveGameplayDeck.stageDeckCards = new Dictionary<CardData, int>(deckToPlay.stageDeckCards);
            Debug.Log($"Active gameplay deck set to: {ActiveGameplayDeck.deckName}");
        }
        else
        {
            Debug.LogError($"Could not set active gameplay deck. Deck with ID {deckID} not found.");
            ActiveGameplayDeck = null;
        }
    }

    public CardData DrawCardFromMainDeck()
    {
        if (ActiveGameplayDeck == null)
        {
            Debug.LogError("No active gameplay deck set in DeckManager.");
            return null;
        }

        if (ActiveGameplayDeck.mainDeckCards.Sum(kvp => kvp.Value) == 0) // Check total cards in main deck
        {
            Debug.Log("Main deck is empty. Cannot draw card.");
            return null;
        }

        // Create a temporary list of all individual cards in the main deck to draw from
        List<CardData> actualDeckList = new List<CardData>();
        foreach (KeyValuePair<CardData, int> pair in ActiveGameplayDeck.mainDeckCards)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                actualDeckList.Add(pair.Key);
            }
        }

        if (actualDeckList.Count == 0) // Should be redundant due to earlier check, but safe
        {
             Debug.Log("Main deck is effectively empty (after expansion). Cannot draw card.");
            return null;
        }

        // Draw a random card from this list
        CardData drawnCard = actualDeckList[Random.Range(0, actualDeckList.Count)];

        // Decrement count in the ActiveGameplayDeck's dictionary
        if (ActiveGameplayDeck.mainDeckCards.TryGetValue(drawnCard, out int count))
        {
            if (count > 1)
            {
                ActiveGameplayDeck.mainDeckCards[drawnCard] = count - 1;
            }
            else
            {
                ActiveGameplayDeck.mainDeckCards.Remove(drawnCard);
            }
            Debug.Log($"Drew card: {drawnCard.title} from main deck. Remaining cards in main deck: {ActiveGameplayDeck.mainDeckCards.Sum(kvp => kvp.Value)}");
            return drawnCard;
        }
        
        Debug.LogError($"Card '{drawnCard.title}' was selected for drawing but not found in deck dictionary. This should not happen.");
        return null; // Should ideally not be reached if logic is sound
    }

    public void SetCurrentSelectedDeck(string deckID)
    {
        if (GetDeck(deckID) != null)
        {
            currentSelectedDeckID = deckID;
            Debug.Log($"Current selected deck set to: {GetDeck(deckID).deckName} (ID: {deckID})");
            SaveDecks(); // Save the current selection
        }
        else
        {
            Debug.LogError($"Cannot set current deck - deck with ID {deckID} not found");
        }
    }

    public Deck GetCurrentSelectedDeck()
    {
        if (!string.IsNullOrEmpty(currentSelectedDeckID))
        {
            return GetDeck(currentSelectedDeckID);
        }
        return null;
    }

    public string GetCurrentSelectedDeckID()
    {
        return currentSelectedDeckID;
    }

    #endregion

    #region Card Management in Editing Deck

    public bool CanAddCardToEditingDeck(CardData card, bool toStageDeck)
    {
        if (currentEditingDeck == null) return false;

        Dictionary<CardData, int> targetDeckPart = toStageDeck ? currentEditingDeck.stageDeckCards : currentEditingDeck.mainDeckCards;
        int currentDeckSize = targetDeckPart.Sum(kvp => kvp.Value);
        int maxDeckSize = toStageDeck ? Deck.MAX_STAGE_DECK_SIZE : Deck.MAX_MAIN_DECK_SIZE;

        if (currentDeckSize >= maxDeckSize) return false; // Deck full

        targetDeckPart.TryGetValue(card, out int currentCopies);
        if (currentCopies >= Deck.MAX_COPIES_PER_CARD) return false; // Max copies reached

        return true;
    }

    public bool AddCardToEditingDeck(CardData card)
    {
        if (currentEditingDeck == null) return false;
        if (card == null) {
            Debug.LogWarning("Attempted to add a null card.");
            return false;
        }

        bool toStage = card.cardType == CardType.Stage;
        Dictionary<CardData, int> targetDeckPart = toStage ? currentEditingDeck.stageDeckCards : currentEditingDeck.mainDeckCards;

        if (!CanAddCardToEditingDeck(card, toStage)) 
        {
            Debug.LogWarning($"Cannot add card '{card.title}' to {(toStage ? "stage" : "main")} deck part. Rules violated.");
            return false;
        }

        targetDeckPart.TryGetValue(card, out int count);
        targetDeckPart[card] = count + 1;
        Debug.Log($"Added '{card.title}' to editing deck ({(toStage ? "stage" : "main")}). Count: {targetDeckPart[card]}");
        return true;
    }

    public bool RemoveCardFromEditingDeck(CardData card)
    {
        if (currentEditingDeck == null) return false;
        if (card == null) {
            Debug.LogWarning("Attempted to remove a null card.");
            return false;
        }

        bool fromStage = card.cardType == CardType.Stage;
        Dictionary<CardData, int> targetDeckPart = fromStage ? currentEditingDeck.stageDeckCards : currentEditingDeck.mainDeckCards;

        if (targetDeckPart.TryGetValue(card, out int count) && count > 0)
        {
            targetDeckPart[card] = count - 1;
            if (targetDeckPart[card] == 0)
            {
                targetDeckPart.Remove(card);
            }
            Debug.Log($"Removed '{card.title}' from editing deck ({(fromStage ? "stage" : "main")}). Remaining: {(targetDeckPart.ContainsKey(card) ? targetDeckPart[card] : 0)}");
            return true;
        }
        Debug.LogWarning($"Card '{card.title}' not found in specified part of editing deck or count is zero.");
        return false;
    }

    public int GetCardCountInEditingDeck(CardData card)
    {
        if (currentEditingDeck == null || card == null) return 0;

        bool isStageCard = card.cardType == CardType.Stage;
        Dictionary<CardData, int> deckPart = isStageCard ? currentEditingDeck.stageDeckCards : currentEditingDeck.mainDeckCards;

        deckPart.TryGetValue(card, out int count);
        return count;
    }

    public int GetTotalCardCountInEditingDeck(bool mainDeck)
    {
        if (currentEditingDeck == null) return 0;
        Dictionary<CardData, int> deckPart = mainDeck ? currentEditingDeck.mainDeckCards : currentEditingDeck.stageDeckCards;
        return deckPart.Sum(kvp => kvp.Value);
    }


    #endregion

    #region Save/Load Logic

    public void SaveDecks()
    {
        try
        {
            // Convert to serializable format
            var serializableDecks = new SerializableUserDecks();
            serializableDecks.FromUserDecks(currentUserDecks, allMasterCardsList);
            
            string json = JsonUtility.ToJson(serializableDecks, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("Decks saved successfully to: " + SavePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save decks: " + e.Message);
        }
    }

    public void LoadDecks()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                var serializableDecks = JsonUtility.FromJson<SerializableUserDecks>(json);
                if (serializableDecks != null)
                {
                    currentUserDecks = serializableDecks.ToUserDecks(allMasterCardsList);
                }
                else
                {
                    currentUserDecks = new UserDecks();
                }
                Debug.Log("Decks loaded successfully from: " + SavePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load decks: " + e.Message + ". Starting with empty deck list.");
                currentUserDecks = new UserDecks();
            }
        }
        else
        {
            Debug.Log("No saved decks file found. Starting with empty deck list.");
            currentUserDecks = new UserDecks();
        }
    }
    #endregion

    #region Player Collection Management
    
    public int GetPlayerCardCount(CardData card)
    {
        return playerCollection.GetCardCount(card);
    }
    
    public void SavePlayerCollection()
    {
        try
        {
            // Since Unity's JsonUtility doesn't serialize Dictionaries directly,
            // we need a serializable wrapper
            var serializableCollection = new SerializablePlayerCollection();
            serializableCollection.FromPlayerCollection(playerCollection, allMasterCardsList);
            
            string json = JsonUtility.ToJson(serializableCollection, true);
            File.WriteAllText(CollectionSavePath, json);
            Debug.Log($"Player collection saved to: {CollectionSavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving player collection: {e.Message}");
        }
    }
    
    public void LoadPlayerCollection()
    {
        try
        {
            if (File.Exists(CollectionSavePath))
            {
                string json = File.ReadAllText(CollectionSavePath);
                var serializableCollection = JsonUtility.FromJson<SerializablePlayerCollection>(json);
                playerCollection = serializableCollection.ToPlayerCollection(allMasterCardsList);
                Debug.Log($"Player collection loaded from: {CollectionSavePath}");
            }
            else
            {
                Debug.Log("No saved player collection found. Will initialize with defaults.");
                playerCollection = new PlayerCollection();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading player collection: {e.Message}");
            playerCollection = new PlayerCollection();
        }
    }
    
    #endregion
}

// Helper class for serializing player collection
[System.Serializable]
public class SerializablePlayerCollection
{
    public List<int> cardIds = new List<int>();
    public List<int> cardCounts = new List<int>();
    
    public void FromPlayerCollection(PlayerCollection collection, List<CardData> allCards)
    {
        cardIds.Clear();
        cardCounts.Clear();
        
        foreach (var kvp in collection.ownedCards)
        {
            // Find the index of this card in the master list
            int cardIndex = allCards.IndexOf(kvp.Key);
            if (cardIndex >= 0)
            {
                cardIds.Add(kvp.Key.id); // Use the card's ID instead of index
                cardCounts.Add(kvp.Value);
            }
        }
    }
    
    public PlayerCollection ToPlayerCollection(List<CardData> allCards)
    {
        var collection = new PlayerCollection();
        
        for (int i = 0; i < cardIds.Count && i < cardCounts.Count; i++)
        {
            // Find card by ID
            CardData card = allCards.FirstOrDefault(c => c.id == cardIds[i]);
            if (card != null)
            {
                collection.SetCardCount(card, cardCounts[i]);
            }
        }
        
        return collection;
    }
} 

// Helper classes for serializing decks with card IDs instead of CardData references
[System.Serializable]
public class SerializableUserDecks
{
    public List<SerializableDeck> allDecks = new List<SerializableDeck>();
    public string currentSelectedDeckID = "";
    
    public void FromUserDecks(UserDecks userDecks, List<CardData> allCards)
    {
        allDecks.Clear();
        
        foreach (var deck in userDecks.allDecks)
        {
            var serializableDeck = new SerializableDeck();
            serializableDeck.FromDeck(deck, allCards);
            allDecks.Add(serializableDeck);
        }
        
        // Include current selected deck ID
        if (DeckManager.Instance != null)
        {
            currentSelectedDeckID = DeckManager.Instance.currentSelectedDeckID;
        }
    }
    
    public UserDecks ToUserDecks(List<CardData> allCards)
    {
        var userDecks = new UserDecks();
        
        foreach (var serializableDeck in allDecks)
        {
            var deck = serializableDeck.ToDeck(allCards);
            if (deck != null)
            {
                userDecks.allDecks.Add(deck);
            }
        }
        
        // Restore current selected deck ID
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.currentSelectedDeckID = currentSelectedDeckID;
        }
        
        return userDecks;
    }
}

[System.Serializable]
public class SerializableDeck
{
    public string deckName;
    public string uniqueID;
    public List<int> mainDeckCardIds = new List<int>();
    public List<int> mainDeckCardCounts = new List<int>();
    public List<int> stageDeckCardIds = new List<int>();
    public List<int> stageDeckCardCounts = new List<int>();
    
    public void FromDeck(Deck deck, List<CardData> allCards)
    {
        deckName = deck.deckName;
        uniqueID = deck.uniqueID;
        
        // Clear existing data
        mainDeckCardIds.Clear();
        mainDeckCardCounts.Clear();
        stageDeckCardIds.Clear();
        stageDeckCardCounts.Clear();
        
        // Serialize main deck cards
        foreach (var kvp in deck.mainDeckCards)
        {
            if (kvp.Key != null)
            {
                mainDeckCardIds.Add(kvp.Key.id);
                mainDeckCardCounts.Add(kvp.Value);
            }
        }
        
        // Serialize stage deck cards
        foreach (var kvp in deck.stageDeckCards)
        {
            if (kvp.Key != null)
            {
                stageDeckCardIds.Add(kvp.Key.id);
                stageDeckCardCounts.Add(kvp.Value);
            }
        }
    }
    
    public Deck ToDeck(List<CardData> allCards)
    {
        var deck = new Deck(deckName, uniqueID);
        
        // Deserialize main deck cards
        for (int i = 0; i < mainDeckCardIds.Count && i < mainDeckCardCounts.Count; i++)
        {
            CardData card = allCards.FirstOrDefault(c => c.id == mainDeckCardIds[i]);
            if (card != null)
            {
                deck.mainDeckCards[card] = mainDeckCardCounts[i];
            }
        }
        
        // Deserialize stage deck cards
        for (int i = 0; i < stageDeckCardIds.Count && i < stageDeckCardCounts.Count; i++)
        {
            CardData card = allCards.FirstOrDefault(c => c.id == stageDeckCardIds[i]);
            if (card != null)
            {
                deck.stageDeckCards[card] = stageDeckCardCounts[i];
            }
        }
        
        return deck;
    }
} 