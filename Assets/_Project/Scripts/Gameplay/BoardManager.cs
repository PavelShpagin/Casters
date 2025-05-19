using UnityEngine;

// Manages card placement on the board and resolves combat.
public class BoardManager : MonoBehaviour
{
    // References to player areas, zones, etc.
    public Transform player1BoardArea;
    public Transform player2BoardArea;

    public Transform player1Graveyard;
    public Transform player2Graveyard;

    // Add references to Stage card areas if needed for interaction

    void Start()
    {
        Debug.Log("BoardManager Start");
        // Setup board zones, potentially clear old cards if restarting
    }

    // TODO: Add methods for playing minions to the board
    // TODO: Add methods for handling spell effects on the board
    // TODO: Add methods for managing combat (selecting attackers/targets, resolving damage)
    // TODO: Add methods for moving cards to the Graveyard

    public void PlaceCardInGraveyard(GameObject card, int playerIndex) // playerIndex 0 or 1
    {
        Transform targetGraveyard = (playerIndex == 0) ? player1Graveyard : player2Graveyard;
        if (card != null && targetGraveyard != null)
        {
            card.transform.SetParent(targetGraveyard, false); // Parent to GY
            card.transform.position = targetGraveyard.position; // Move to GY position
            // TODO: Potentially stack or arrange cards in GY
            Debug.Log($"Card moved to Player {playerIndex + 1}'s Graveyard.");
        }
        else
        {
            Debug.LogWarning("Cannot move card to graveyard - card or graveyard transform is null.");
        }
    }
} 