using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserDecks
{
    public List<Deck> allDecks = new List<Deck>();

    // We can add a reference to the currently selected/active deck ID if needed
    // public string currentlySelectedDeckID;

    public UserDecks()
    {
        allDecks = new List<Deck>();
    }

    // Add methods like:
    // - AddNewDeck(string name)
    // - DeleteDeck(string uniqueID)
    // - GetDeck(string uniqueID)
    // - GetDeckByName(string name)
    // - IsDeckNameTaken(string name)
} 