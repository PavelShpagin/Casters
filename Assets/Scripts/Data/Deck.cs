using System.Collections.Generic;
using UnityEngine; // Required for [System.Serializable]

[System.Serializable] // Makes this class show up in the Inspector when part of another class (like UserDecks)
public class Deck
{
    public string deckName = "New Deck";
    public string uniqueID; // For uniquely identifying decks, e.g., for saving/loading

    // Using Dictionary to store CardData and its count in the deck
    public Dictionary<CardData, int> mainDeckCards = new Dictionary<CardData, int>();
    public Dictionary<CardData, int> stageDeckCards = new Dictionary<CardData, int>();

    public const int MAX_MAIN_DECK_SIZE = 30;
    public const int MAX_STAGE_DECK_SIZE = 5;
    public const int MAX_COPIES_PER_CARD = 2;

    public Deck(string name, string id)
    {
        deckName = name;
        uniqueID = id;
        mainDeckCards = new Dictionary<CardData, int>();
        stageDeckCards = new Dictionary<CardData, int>();
    }

    // Basic constructor if only name is provided initially
    public Deck(string name) : this(name, System.Guid.NewGuid().ToString()) {}

    // Parameterless constructor for serialization (e.g., if using JsonUtility later)
    public Deck()
    {
        uniqueID = System.Guid.NewGuid().ToString();
        mainDeckCards = new Dictionary<CardData, int>();
        stageDeckCards = new Dictionary<CardData, int>();
    }

    // Add more methods here as needed:
    // - AddCard(CardData card, bool toStageDeck)
    // - RemoveCard(CardData card, bool fromStageDeck)
    // - GetCardCount(CardData card)
    // - IsFull(bool checkStageDeck)
    // - IsValid() // Checks all deck rules
    // - GetTotalCardCount(bool mainDeckOnly)
} 