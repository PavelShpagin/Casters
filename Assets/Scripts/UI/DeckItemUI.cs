using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class DeckItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI deckNameText;
    [SerializeField] private TextMeshProUGUI deckStatsText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Image deckBackground;
    [SerializeField] private Image deckThumbnail; // Optional: for deck preview image
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.25f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.3f, 0.3f, 0.35f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.4f, 0.4f, 0.45f, 1f);
    
    // Events
    private Action<string> onPlayClicked;
    private Action<string> onEditClicked;
    
    // Data
    private Deck currentDeck;
    private bool isSelected = false;
    
    void Awake()
    {
        SetupButtons();
        SetupHoverEffects();
    }
    
    void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => {
                if (currentDeck != null)
                {
                    onPlayClicked?.Invoke(currentDeck.uniqueID);
                }
            });
        }
        
        if (editButton != null)
        {
            editButton.onClick.AddListener(() => {
                if (currentDeck != null)
                {
                    onEditClicked?.Invoke(currentDeck.uniqueID);
                }
            });
        }
    }
    
    void SetupHoverEffects()
    {
        if (deckBackground != null)
        {
            // Add hover effect to background
            var button = deckBackground.GetComponent<Button>();
            if (button == null)
            {
                button = deckBackground.gameObject.AddComponent<Button>();
            }
            
            button.targetGraphic = deckBackground;
            button.transition = Selectable.Transition.ColorTint;
            
            var colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = hoverColor;
            colors.selectedColor = selectedColor;
            colors.pressedColor = selectedColor;
            button.colors = colors;
            
            // Make background clickable for selection
            button.onClick.AddListener(() => {
                if (currentDeck != null)
                {
                    onPlayClicked?.Invoke(currentDeck.uniqueID);
                }
            });
        }
    }
    
    /// <summary>
    /// Sets up the deck item with deck data and callbacks
    /// </summary>
    /// <param name="deck">The deck to display</param>
    /// <param name="playCallback">Callback when play is clicked</param>
    /// <param name="editCallback">Callback when edit is clicked</param>
    public void SetupDeckItem(Deck deck, Action<string> playCallback, Action<string> editCallback)
    {
        currentDeck = deck;
        onPlayClicked = playCallback;
        onEditClicked = editCallback;
        
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        if (currentDeck == null) return;
        
        // Update deck name
        if (deckNameText != null)
        {
            deckNameText.text = currentDeck.deckName;
        }
        
        // Update deck stats
        if (deckStatsText != null)
        {
            int mainCards = currentDeck.mainDeckCards.Values.Sum();
            int stageCards = currentDeck.stageDeckCards.Values.Sum();
            deckStatsText.text = $"Main: {mainCards}/30\nStage: {stageCards}/5";
        }
        
        // Update deck validity visual indicator
        UpdateDeckValidityIndicator();
        
        // Update thumbnail if needed
        UpdateThumbnail();
    }
    
    void UpdateDeckValidityIndicator()
    {
        if (deckBackground == null) return;
        
        bool isValid = IsDeckValid();
        
        if (isValid)
        {
            deckBackground.color = normalColor;
        }
        else
        {
            // Slightly red tint for invalid decks
            deckBackground.color = new Color(normalColor.r + 0.1f, normalColor.g, normalColor.b, normalColor.a);
        }
    }
    
    bool IsDeckValid()
    {
        if (currentDeck == null) return false;
        
        int mainCards = currentDeck.mainDeckCards.Values.Sum();
        int stageCards = currentDeck.stageDeckCards.Values.Sum();
        
        // Check if deck meets minimum requirements
        return mainCards >= 30 && stageCards >= 1 && stageCards <= 5;
    }
    
    void UpdateThumbnail()
    {
        if (deckThumbnail == null) return;
        
        // You can implement custom thumbnail logic here
        // For example, show the most expensive card, or a faction symbol
        
        // Placeholder: Set thumbnail based on deck validity
        if (IsDeckValid())
        {
            deckThumbnail.color = Color.white;
        }
        else
        {
            deckThumbnail.color = Color.gray;
        }
    }
    
    /// <summary>
    /// Sets the selected state of this deck item
    /// </summary>
    /// <param name="selected">Whether this item is selected</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (deckBackground != null)
        {
            deckBackground.color = selected ? selectedColor : normalColor;
        }
    }
    
    /// <summary>
    /// Refreshes the deck display (call after deck data changes)
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }
} 