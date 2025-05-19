using UnityEngine;

// This allows you to create instances of CardData from the Assets/Create menu.
[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card Data", order = 1)]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public int id; // Unique identifier
    public string title = "New Card";
    [TextArea(3, 5)]
    public string description = "Card description.";
    public CardType cardType = CardType.Minion;
    public CardFaction faction = CardFaction.Neutral;

    [Header("Stats")]
    public int cost; // Could be "Free Summon" or a numeric value. Consider how to handle this.
                     // For simplicity now, let's assume int. You might need a string or a more complex type later.
    public int attack = 0; // Only relevant for Minions
    public int health = 0; // Relevant for Minions and potentially some Stages
    public int level = 0;  // Or "Rank", "Tier" etc.

    [Header("Visuals")]
    public Sprite cardImage; // Reference to the card's artwork
    // public string card_img_path; // If loading from path, use this and load Sprite at runtime

    // You can add more properties here later, e.g.:
    // public bool isUnique; // If only one copy allowed in a deck regardless of normal limits
    // public Rarity rarity;
    // public List<Effect> effects;
} 