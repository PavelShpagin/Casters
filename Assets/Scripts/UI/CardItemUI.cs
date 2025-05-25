using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public enum CardLocation { Collection, MainDeck, StageDeck }

public class CardItemUI : MonoBehaviour, IPointerClickHandler
{
    // Main card components
    private Image cardImage;
    private Transform copyIndicatorsContainer;
    private List<Image> copyIndicators = new List<Image>();
    
    private CardData currentCardData;
    private CardLocation currentLocation;
    private Action<CardData> onClickAction;
    
    // New: Right-click support
    public Action onRightClick;
    
    // Public property for accessing card data
    public CardData CardData => currentCardData;

    // Updated CreateCardUI method to match DeckBuilderUI calls
    public static GameObject CreateCardUI(CardData cardData, Transform parent)
    {
        return CreateCardUI(parent, cardData, CardLocation.Collection, null);
    }

    public static GameObject CreateCardUI(Transform parent, CardData cardData, CardLocation location, Action<CardData> clickAction = null)
    {
        // Create main card GameObject
        GameObject cardGO = new GameObject($"Card_{cardData.title}");
        cardGO.transform.SetParent(parent, false);

        // Add RectTransform
        RectTransform cardRect = cardGO.AddComponent<RectTransform>();
        cardRect.anchorMin = Vector2.zero;
        cardRect.anchorMax = Vector2.one;
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;

        // Add the main card image
        Image cardImage = cardGO.AddComponent<Image>();
        cardImage.sprite = cardData.cardImage;
        cardImage.preserveAspect = true;
        
        // Add CardItemUI component and set it up
        CardItemUI cardUI = cardGO.AddComponent<CardItemUI>();
        cardUI.cardImage = cardImage;
        cardUI.currentCardData = cardData;
        cardUI.currentLocation = location;
        cardUI.onClickAction = clickAction;

        // Create copy indicators only for collection cards
        if (location == CardLocation.Collection)
        {
            cardUI.CreateCopyIndicators();
            // Don't call UpdateVisuals - let DeckBuilderUI control copy indicators manually
        }
        else
        {
            // For deck cards (MainDeck/StageDeck), update visuals but no copy indicators
            cardUI.UpdateVisuals();
        }

        return cardGO;
    }

    private void CreateCopyIndicators()
    {
        // Create container for copy indicators
        GameObject containerGO = new GameObject("CopyIndicators");
        containerGO.transform.SetParent(transform, false);

        RectTransform containerRect = containerGO.AddComponent<RectTransform>();
        // Position OUTSIDE the card, below the bottom edge
        containerRect.anchorMin = new Vector2(0f, 0f);
        containerRect.anchorMax = new Vector2(1f, 0f);
        containerRect.anchoredPosition = new Vector2(0f, -9f); // NEGATIVE value to go below the card
        containerRect.sizeDelta = new Vector2(0f, 16f); // 16 pixels high

        // Add layout group with better spacing
        HorizontalLayoutGroup layout = containerGO.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 3f; // Tight spacing for small circles
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        copyIndicatorsContainer = containerGO.transform;
        
        Debug.Log($"[CardItemUI] Created copy indicators container BELOW card");
        
        // Create the actual indicators
        UpdateCopyIndicators();
    }

    private void UpdateCopyIndicators()
    {
        if (copyIndicatorsContainer == null) return;

        // Clear existing indicators
        foreach (Image indicator in copyIndicators)
        {
            if (indicator != null)
                DestroyImmediate(indicator.gameObject);
        }
        copyIndicators.Clear();

        // Only show copy indicators for collection cards
        if (currentLocation != CardLocation.Collection || currentCardData == null)
            return;

        // Get number of copies owned by player
        int ownedCopies = 0;
        if (DeckManager.Instance != null)
        {
            ownedCopies = DeckManager.Instance.GetPlayerCardCount(currentCardData);
        }

        Debug.Log($"[CardItemUI] Creating {ownedCopies} copy indicators for {currentCardData.title}");

        // Create copy indicators (circles using programmatic sprite)
        for (int i = 0; i < ownedCopies; i++)
        {
            GameObject indicatorGO = new GameObject($"CopyCircle_{i}");
            indicatorGO.transform.SetParent(copyIndicatorsContainer, false);

            // Add RectTransform
            RectTransform indicatorRect = indicatorGO.AddComponent<RectTransform>();
            indicatorRect.sizeDelta = new Vector2(10f, 10f); // Small circles

            // Add Image component with programmatic circle sprite
            Image indicatorImage = indicatorGO.AddComponent<Image>();
            
            // Always use programmatic circle sprite (more reliable than built-in resources)
            indicatorImage.sprite = CreatePerfectCircleSprite();
            indicatorImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Darker gray circles
            indicatorImage.type = Image.Type.Simple;

            copyIndicators.Add(indicatorImage);
            
            Debug.Log($"[CardItemUI] Created copy indicator {i} for {currentCardData.title}");
        }
        
        Debug.Log($"[CardItemUI] Total indicators created: {copyIndicators.Count}");
    }

    private Sprite CreatePerfectCircleSprite()
    {
        // Create a perfect circle sprite as fallback
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = (size / 2f) - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public void UpdateVisuals()
    {
        UpdateCopyIndicators();
        UpdateTransparency();
    }

    private void UpdateTransparency()
    {
        if (currentLocation == CardLocation.Collection && currentCardData != null && cardImage != null)
        {
            // Get number of copies owned by player
            int ownedCopies = 0;
            if (DeckManager.Instance != null)
            {
                ownedCopies = DeckManager.Instance.GetPlayerCardCount(currentCardData);
            }

            if (ownedCopies == 0)
            {
                // 35% opacity for unowned cards
                cardImage.color = new Color(1f, 1f, 1f, 0.35f);
            }
            else
            {
                // Check if we can add to deck
                bool canInteract = true;
                if (DeckManager.Instance != null && DeckManager.Instance.currentEditingDeck != null)
                {
                    bool isStageCard = currentCardData.cardType == CardType.Stage;
                    canInteract = DeckManager.Instance.CanAddCardToEditingDeck(currentCardData, isStageCard);
            }

                cardImage.color = canInteract ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.9f);
            }
        }
        else if (cardImage != null)
        {
            // For deck cards, normal transparency
            cardImage.color = Color.white;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left click
            if (onClickAction != null && currentCardData != null)
            {
                onClickAction(currentCardData);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right click
            onRightClick?.Invoke();
        }
    }

    // Legacy method for compatibility
    public void SetupCard(CardData cardData, int currentCopiesInDeck, CardLocation location, Action<CardData> clickAction)
    {
        currentCardData = cardData;
        currentLocation = location;
        onClickAction = clickAction;

        if (cardImage != null && cardData != null) 
        {
            cardImage.sprite = cardData.cardImage; 
        }

        UpdateVisuals();
    }

    // Public method to refresh the card display
    public void RefreshCard()
        {
        UpdateVisuals();
    }

    // New methods for custom copy indicator control
    public void ShowCopyIndicators(int count, Color color)
    {
        if (copyIndicatorsContainer == null) return;

        // Clear existing indicators
        ClearCopyIndicators();

        // Create new indicators with specified color
        for (int i = 0; i < count; i++)
        {
            CreateSingleCopyIndicator(color);
        }
    }

    public void ShowAdditionalCopyIndicators(int count, Color color)
    {
        if (copyIndicatorsContainer == null) return;

        // Add additional indicators without clearing existing ones
        for (int i = 0; i < count; i++)
        {
            CreateSingleCopyIndicator(color);
        }
    }

    private void CreateSingleCopyIndicator(Color color)
    {
        GameObject indicatorGO = new GameObject($"CopyCircle_{copyIndicators.Count}");
        indicatorGO.transform.SetParent(copyIndicatorsContainer, false);

        // Add RectTransform
        RectTransform indicatorRect = indicatorGO.AddComponent<RectTransform>();
        indicatorRect.sizeDelta = new Vector2(10f, 10f); // Small circles

        // Add Image component with programmatic circle sprite
        Image indicatorImage = indicatorGO.AddComponent<Image>();
        
        // Always use programmatic circle sprite (more reliable)
        indicatorImage.sprite = CreatePerfectCircleSprite();
        indicatorImage.color = color;
        indicatorImage.type = Image.Type.Simple;

        copyIndicators.Add(indicatorImage);
    }
    
    private void ClearCopyIndicators()
    {
        foreach (Image indicator in copyIndicators)
        {
            if (indicator != null)
                DestroyImmediate(indicator.gameObject);
        }
        copyIndicators.Clear();
            }
    
    public void SetTransparency(float alpha)
    {
        if (cardImage != null)
        {
            Color color = cardImage.color;
            color.a = alpha;
            cardImage.color = color;
        }
    }
} 