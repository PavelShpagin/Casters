using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBox : MonoBehaviour
{
    [Header("UI References")]
    public Button editButton;
    public TextMeshProUGUI deckNameText;
    
    private string deckID;
    private string deckName;
    
    private bool controlledByMainMenuDeckController = false;

    public void SetDeckData(string id, string name)
    {
        deckID = id;
        deckName = name;
        
        if (deckNameText != null)
        {
            deckNameText.text = name;
        }
        
        // COMPLETELY DISABLED: All button functionality handled by MainMenuDeckController
        /*
        // DISABLED: Make the entire deck box clickable to open popup
        // This conflicts with MainMenuDeckController
        Button deckBoxButton = GetComponent<Button>();
        if (deckBoxButton == null)
        {
            deckBoxButton = gameObject.AddComponent<Button>();
            Debug.Log($"[DeckBox] Added Button component to {gameObject.name}");
        }
        
        deckBoxButton.onClick.RemoveAllListeners();
        deckBoxButton.onClick.AddListener(OnDeckBoxClicked);
        Debug.Log($"[DeckBox] Set up deck box button for {name}");
        
        // Set up edit button listener (optional)
        if (editButton != null)
        {
            editButton.onClick.RemoveAllListeners();
            editButton.onClick.AddListener(OnEditButtonClicked);
            Debug.Log($"[DeckBox] Set up edit button for {name}");
        }
        else
        {
            Debug.Log($"[DeckBox] No separate edit button - using popup edit instead");
        }
        */
        
        Debug.Log($"[DeckBox] SetDeckData called for {name} - all button handling disabled, using MainMenuDeckController");
    }
    
    void OnEditButtonClicked()
    {
        Debug.Log($"[DeckBox] Edit button clicked for deck: {deckName}");
        
        // Open deck in edit mode
        DeckBuilderUI deckBuilder = FindFirstObjectByType<DeckBuilderUI>();
        if (deckBuilder != null)
        {
            deckBuilder.DisplayDeckFromExternal(deckID, deckName);
        }
        else
        {
            Debug.LogError("[DeckBox] Could not find DeckBuilderUI!");
        }
    }
    
    void OnDeckSelected(string selectedDeckID)
    {
        Debug.Log($"[DeckBox] Deck selected for play: {deckName}");
        
        // For now, just log. You can implement play functionality here.
        // Example: GameManager.Instance.StartGameWithDeck(selectedDeckID);
    }
    
    void OnDeckEditFromPopup(string editDeckID)
    {
        Debug.Log($"[DeckBox] Deck edit requested from popup: {editDeckID}");
        
        // Open deck in edit mode
        DeckBuilderUI deckBuilder = FindFirstObjectByType<DeckBuilderUI>();
        if (deckBuilder != null)
        {
            Deck deck = DeckManager.Instance?.GetDeck(editDeckID);
            if (deck != null)
            {
                deckBuilder.DisplayDeckFromExternal(editDeckID, deck.deckName);
            }
        }
    }
    
    void UpdateCurrentDeckDisplay()
    {
        // Visual feedback for current selected deck
        if (DeckManager.Instance != null)
        {
            string currentDeckID = DeckManager.Instance.GetCurrentSelectedDeckID();
            bool isCurrentDeck = currentDeckID == deckID;
            
            // Change visual appearance based on selection
            Image bgImage = GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = isCurrentDeck ? 
                    new Color(0.3f, 0.6f, 0.3f, 1f) : // Green tint for selected
                    new Color(1f, 1f, 1f, 1f);        // Normal color
            }
            
            if (isCurrentDeck && deckNameText != null)
            {
                deckNameText.color = Color.yellow; // Highlight current deck name
            }
            else if (deckNameText != null)
            {
                deckNameText.color = Color.white;
            }
        }
    }
    
    void OnDeckBoxClicked()
    {
        Debug.Log($"[DeckBox] Deck box clicked for deck: {deckName}");
        
        // Show popup with deck options
        if (DeckSelectionPopup.Instance != null)
        {
            DeckSelectionPopup.Instance.ShowPopup(
                onDeckSelected: OnDeckSelected,
                onDeckEdit: (editDeckID) => {
                    Debug.Log($"[DeckBox] Edit requested for deck: {deckName}");
                    DeckBuilderUI deckBuilder = FindFirstObjectByType<DeckBuilderUI>();
                    if (deckBuilder != null)
                    {
                        deckBuilder.DisplayDeckFromExternal(editDeckID, deckName);
                    }
                }
            );
        }
        else
        {
            Debug.LogError("[DeckBox] Still couldn't access DeckSelectionPopup.Instance!");
        }
    }
    
    public void MarkAsControlledByMainMenuDeckController()
    {
        controlledByMainMenuDeckController = true;
    }

    void Start()
    {
        if (controlledByMainMenuDeckController) return;
        UpdateCurrentDeckDisplay();
    }
    
    void OnEnable()
    {
        if (controlledByMainMenuDeckController) return;
        UpdateCurrentDeckDisplay();
    }
} 