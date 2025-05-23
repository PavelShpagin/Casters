using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public enum CardLocation { Collection, MainDeck, StageDeck }

public class CardItemUI : MonoBehaviour, IPointerClickHandler
{
    // Automatically find the Image component - no manual assignment needed
    private Image cardImage;
    
    private CardData currentCardData;
    private CardLocation currentLocation;
    private Action<CardData> onClickAction; // Action to perform on click (Add or Remove)

    private void Awake()
    {
        // Automatically find the Image component on this GameObject
        cardImage = GetComponent<Image>();
        
        if (cardImage == null)
        {
            Debug.LogError($"CardItemUI on '{gameObject.name}': No Image component found! Please ensure this GameObject has an Image component.");
        }
    }

    public void SetupCard(CardData cardData, int currentCopiesInDeck, CardLocation location, Action<CardData> clickAction)
    {
        currentCardData = cardData;
        currentLocation = location;
        onClickAction = clickAction;

        if (cardImage != null && cardData != null) 
        {
            // Use the sprite directly from the CardData ScriptableObject
            cardImage.sprite = cardData.cardImage; 
            cardImage.color = Color.white; 
            
            if (cardImage.sprite == null)
            {
                // Set a fallback color if no sprite is assigned to the CardData
                cardImage.color = Color.magenta; 
                Debug.LogWarning($"CardItemUI: Card '{cardData.title}' has no cardImage assigned in its CardData ScriptableObject.");
            }
        }
        else if (cardImage == null)
        {
            Debug.LogError($"CardItemUI: No Image component found on '{gameObject.name}'.");
        }

        UpdateInteractabilityVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentCardData == null || onClickAction == null) return;

        if (eventData.button == PointerEventData.InputButton.Left) 
        {
            onClickAction(currentCardData);
        }
    }

    public void UpdateCollectionItemCount(int newCountInDeck)
    {
        if (currentLocation == CardLocation.Collection)
        {
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

        if (cardImage != null)
        {
            cardImage.color = canInteract ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.9f); 
        }
    }
} 