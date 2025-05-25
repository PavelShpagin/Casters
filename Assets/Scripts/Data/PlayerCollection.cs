using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCollection
{
    // Dictionary mapping CardData to the number of copies owned
    public Dictionary<CardData, int> ownedCards = new Dictionary<CardData, int>();

    public PlayerCollection()
    {
        ownedCards = new Dictionary<CardData, int>();
    }

    // Get the number of copies of a specific card the player owns
    public int GetCardCount(CardData card)
    {
        if (card == null) return 0;
        return ownedCards.ContainsKey(card) ? ownedCards[card] : 0;
    }

    // Set the number of copies of a specific card
    public void SetCardCount(CardData card, int count)
    {
        if (card == null) return;
        
        if (count <= 0)
        {
            if (ownedCards.ContainsKey(card))
                ownedCards.Remove(card);
        }
        else
        {
            ownedCards[card] = count;
        }
    }

    // Add copies of a card
    public void AddCards(CardData card, int count)
    {
        if (card == null || count <= 0) return;
        
        int currentCount = GetCardCount(card);
        SetCardCount(card, currentCount + count);
    }

    // Remove copies of a card
    public bool RemoveCards(CardData card, int count)
    {
        if (card == null || count <= 0) return false;
        
        int currentCount = GetCardCount(card);
        if (currentCount < count) return false;
        
        SetCardCount(card, currentCount - count);
        return true;
    }

    // Initialize collection with default cards (2 copies each for now)
    public void InitializeWithAllCards(List<CardData> allCards, int copiesPerCard = 2)
    {
        ownedCards.Clear();
        
        foreach (CardData card in allCards)
        {
            if (card != null)
            {
                ownedCards[card] = copiesPerCard;
            }
        }
        
        Debug.Log($"Initialized player collection with {ownedCards.Count} unique cards, {copiesPerCard} copies each.");
    }
} 