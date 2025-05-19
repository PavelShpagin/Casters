using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public enum CardLocation { Collection, MainDeck, StageDeck }

public class CardItemUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image cardImage;         // Assign the Image component for card art
    public Text countText;          // Optional: Assign a Text component to show count (e.g., "x2" in collection)
    public Image highlightBorder;   // Optional: Assign an Image for highlighting/selection
    
    private CardData currentCardData;
    private CardLocation currentLocation;
    private Action<CardData> onClickAction; // Action to perform on click (Add or Remove)

    // Used to know how many copies of this card are already in the current deck when displayed in the collection
    private int copiesInCurrentDeckForCollectionDisplay;

    public void SetupCard(CardData cardData, int currentCopiesInDeck, CardLocation location, Action<CardData> clickAction)
    {
        currentCardData = cardData;
        currentLocation = location;
        onClickAction = clickAction;
        copiesInCurrentDeckForCollectionDisplay = (location == CardLocation.Collection) ? currentCopiesInDeck : 0;

        if (cardImage != null) 
        {
            cardImage.sprite = cardData.cardImage; 
            cardImage.color = Color.white; 
            if (cardImage.sprite == null)
            {
                // Consider a placeholder color or default sprite if cardData.cardImage is null
                cardImage.color = Color.magenta; // Makes missing sprites obvious
                Debug.LogWarning($"CardItemUI: Card '{cardData.title}' is missing its sprite assignment in CardData.");
            }
        }
        
        if (countText != null) 
        { 
            if(location == CardLocation.Collection && currentCopiesInDeck > 0) 
            {
                countText.text = "x" + currentCopiesInDeck;
                countText.gameObject.SetActive(true);
            }
            else 
            {
                 countText.gameObject.SetActive(false);
            }
        }

        UpdateInteractabilityVisuals();

        if (highlightBorder != null) highlightBorder.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentCardData == null || onClickAction == null) return;

        // For simplicity, let's make left-click the primary action for adding/removing.
        // You can add more complex interactions (right-click for info, etc.) later.
        if (eventData.button == PointerEventData.InputButton.Left) 
        {
            // Before invoking, check if the action is valid (e.g., trying to add from collection when full)
            // This check is now primarily visual via UpdateInteractability, action will proceed.
            // DeckManager will make the final decision.
            onClickAction(currentCardData);
        }
        // Example: Right-click for card details (not implemented here)
        // else if (eventData.button == PointerEventData.InputButton.Right)
        // {
        //     Debug.Log($"Right-clicked on: {currentCardData.title}. Implement card preview/details here.");
        //     // Example: CardPreviewManager.Instance.ShowCardDetails(currentCardData);
        // }
    }

    // Call this if the card's state in the deck changes while it's visible in the collection
    public void UpdateCollectionItemCount(int newCountInDeck)
    {
        if (currentLocation == CardLocation.Collection)
        {
            copiesInCurrentDeckForCollectionDisplay = newCountInDeck;
            if (countText != null)
            {
                if (newCountInDeck > 0)
                {
                    countText.text = "x" + newCountInDeck;
                    countText.gameObject.SetActive(true);
                }
                else
                {
                    countText.gameObject.SetActive(false);
                }
            }
            UpdateInteractabilityVisuals();
        }
    }

    private void UpdateInteractabilityVisuals() 
    {
        bool canInteract = true;
        if (currentLocation == CardLocation.Collection)
        {
            if (DeckManager.Instance != null && DeckManager.Instance.currentEditingDeck != null && currentCardData != null)
            {
                bool isStageCard = currentCardData.cardType == CardType.Stage;
                canInteract = DeckManager.Instance.CanAddCardToEditingDeck(currentCardData, isStageCard);
            }
        }
        // For cards in deck lists, they are always "interactable" to be removed.

        if (cardImage != null)
        {
            cardImage.color = canInteract ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.9f); // Gray out if not interactable
        }
    }
} 