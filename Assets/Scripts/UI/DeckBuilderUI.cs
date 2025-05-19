using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class DeckBuilderUI : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject deckBuilderCanvas;
    public GameObject myDecksCanvas;    
    // public ConfirmationPopupUI confirmationPopup; // Assuming you have this

    [Header("DeckBuilder UI Elements")]
    public InputField deckNameInput;     
    public Button backButton;          
    public Button saveButton;          
    public InputField searchInput;       
    public Transform collectionContentParent; 
    public Transform mainDeckContentParent;   
    public Transform stageDeckContentParent;  
    public Text mainDeckTitleText;      
    public Text stageDeckTitleText;     

    [Header("Prefabs")]
    public GameObject cardItemPrefab;     

    // To keep track of instantiated card items in the collection for quick updates
    private Dictionary<CardData, CardItemUI> collectionCardItems = new Dictionary<CardData, CardItemUI>();

    private bool hasUnsavedChanges = false;

    void Start()
    {
        if (!ValidateReferences())
        {
            this.enabled = false;
            return;
        }

        backButton.onClick.AddListener(OnBackButtonClicked);
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        deckNameInput.onValueChanged.AddListener(MarkChanges); 
        searchInput.onValueChanged.AddListener(FilterCollectionDisplay); 

        if (deckBuilderCanvas.activeSelf) // If it starts active, ensure it's set up.
        {
             // This might happen if you are testing directly in this scene.
             // Consider loading a default or new deck if DeckManager.currentEditingDeck is null.
            if (DeckManager.Instance != null && DeckManager.Instance.currentEditingDeck == null)
            {
                // Create a dummy new deck for testing if no deck is loaded
                DeckManager.Instance.StartEditingDeck(DeckManager.Instance.CreateNewDeck("Test Edit Deck").uniqueID);
            }
            RefreshAllDisplays();
        }
        else
        {
            gameObject.SetActive(false); 
        }
    }

    bool ValidateReferences()
    {
        if (deckBuilderCanvas == null || myDecksCanvas == null || deckNameInput == null || backButton == null || 
            saveButton == null || searchInput == null || collectionContentParent == null || mainDeckContentParent == null || 
            stageDeckContentParent == null || cardItemPrefab == null || mainDeckTitleText == null || stageDeckTitleText == null)
        {
            Debug.LogError("DeckBuilderUI: One or more references are missing! Please assign them in the Inspector.");
            return false;
        }
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckBuilderUI: DeckManager.Instance is not found. Ensure DeckManager is in your scene and initialized.");
            return false;
        }
        return true;
    }

    public void LoadDeckForEditing(string deckID)
    {
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager instance not found when trying to load deck for editing.");
            GoToMyDecks(); // Fallback
            return;
        }

        DeckManager.Instance.StartEditingDeck(deckID); 

        if (DeckManager.Instance.currentEditingDeck == null)
        {
            Debug.LogError($"Failed to start editing deck with ID: {deckID}. The deck might not exist or an error occurred.");
            GoToMyDecks(); 
            return;
        }
        
        deckNameInput.text = DeckManager.Instance.currentEditingDeck.deckName;
        hasUnsavedChanges = false; 
        searchInput.text = ""; // Clear search on new deck load

        RefreshAllDisplays();
        deckBuilderCanvas.SetActive(true); // Ensure this canvas is active
    }

    void RefreshAllDisplays()
    {
        PopulateCollection();
        PopulateDeckDisplay();
    }

    void PopulateCollection()
    {
        if (DeckManager.Instance == null || cardItemPrefab == null) return;
        
        // Clear existing (but reuse GameObjects if you implement pooling later)
        foreach (CardItemUI item in collectionCardItems.Values)
        {
            if (item != null) Destroy(item.gameObject);
        }
        collectionCardItems.Clear();

        string filter = searchInput.text.ToLower();

        foreach (CardData card in DeckManager.Instance.allMasterCardsList.OrderBy(c => c.title))
        {
            if (!string.IsNullOrEmpty(filter) && 
                !card.title.ToLower().Contains(filter) && 
                !card.description.ToLower().Contains(filter) &&
                !card.cardType.ToString().ToLower().Contains(filter) &&
                !card.faction.ToString().ToLower().Contains(filter))
            {
                continue; 
            }

            GameObject cardGO = Instantiate(cardItemPrefab, collectionContentParent);
            CardItemUI cardUI = cardGO.GetComponent<CardItemUI>();
            if (cardUI != null)
            {
                int countInDeck = DeckManager.Instance.GetCardCountInEditingDeck(card);
                cardUI.SetupCard(card, countInDeck, CardLocation.Collection, AddCardToDeck);
                collectionCardItems[card] = cardUI;
            }
        }
    }

    void PopulateDeckDisplay()
    {
        if (DeckManager.Instance == null || DeckManager.Instance.currentEditingDeck == null || cardItemPrefab == null) return;

        ClearContainer(mainDeckContentParent);
        ClearContainer(stageDeckContentParent);

        // Populate Main Deck
        foreach (KeyValuePair<CardData, int> entry in DeckManager.Instance.currentEditingDeck.mainDeckCards.OrderBy(kv => kv.Key.title))
        {
            for (int i = 0; i < entry.Value; i++) // Instantiate one UI item per card copy
            {
                GameObject cardGO = Instantiate(cardItemPrefab, mainDeckContentParent);
                CardItemUI cardUI = cardGO.GetComponent<CardItemUI>();
                if (cardUI != null)
                {
                    // For deck display, count is implicitly 1 for this item, but SetupCard can take overall count for other uses
                    cardUI.SetupCard(entry.Key, entry.Value, CardLocation.MainDeck, RemoveCardFromDeck);
                }
            }
        }

        // Populate Stage Deck
        foreach (KeyValuePair<CardData, int> entry in DeckManager.Instance.currentEditingDeck.stageDeckCards.OrderBy(kv => kv.Key.title))
        {
             for (int i = 0; i < entry.Value; i++)
            {
                GameObject cardGO = Instantiate(cardItemPrefab, stageDeckContentParent);
                CardItemUI cardUI = cardGO.GetComponent<CardItemUI>();
                if (cardUI != null)
                {
                    cardUI.SetupCard(entry.Key, entry.Value, CardLocation.StageDeck, RemoveCardFromDeck);
                }
            }
        }
        UpdateDeckCountTitles();
    }

    void UpdateDeckCountTitles()
    {
        if (DeckManager.Instance == null || DeckManager.Instance.currentEditingDeck == null) return;
        int mainDeckCount = DeckManager.Instance.GetTotalCardCountInEditingDeck(true);
        int stageDeckCount = DeckManager.Instance.GetTotalCardCountInEditingDeck(false);
        mainDeckTitleText.text = $"Main Deck ({mainDeckCount}/{Deck.MAX_MAIN_DECK_SIZE})";
        stageDeckTitleText.text = $"Stage Deck ({stageDeckCount}/{Deck.MAX_STAGE_DECK_SIZE})";
    }


    void FilterCollectionDisplay(string searchTerm)
    {
        PopulateCollection(); 
    }

    void AddCardToDeck(CardData card)
    {
        if (DeckManager.Instance == null) return;

        bool added = DeckManager.Instance.AddCardToEditingDeck(card);
        if (added)
        {
            MarkChanges();
            PopulateDeckDisplay(); // Refresh deck view
            // Update only the specific card in collection view for efficiency
            if (collectionCardItems.TryGetValue(card, out CardItemUI cardUI))
            {
                cardUI.UpdateCollectionItemCount(DeckManager.Instance.GetCardCountInEditingDeck(card));
            }
            UpdateDeckCountTitles();
        }
        else
        {
            // Debug.Log($"Could not add card {card.title} (deck full or max copies reached).");
            // Optionally provide user feedback (e.g., UI indication, sound)
            // The CardItemUI itself should visually indicate if it can't be added.
        }
    }

    void RemoveCardFromDeck(CardData card)
    {
        if (DeckManager.Instance == null) return;

        bool removed = DeckManager.Instance.RemoveCardFromEditingDeck(card);
        if (removed)
        {
            MarkChanges();
            PopulateDeckDisplay(); // Refresh deck view
            if (collectionCardItems.TryGetValue(card, out CardItemUI cardUI))
            {
                cardUI.UpdateCollectionItemCount(DeckManager.Instance.GetCardCountInEditingDeck(card));
            }
            UpdateDeckCountTitles();
        }
    }

    void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    void MarkChanges(string _ = null) 
    {
        hasUnsavedChanges = true;
        // Optional: Visual cue for unsaved changes (e.g., asterisk on save button)
        if(saveButton != null) saveButton.GetComponentInChildren<Text>().text = "Save*";
    }

    void OnSaveButtonClicked()
    {
        if (DeckManager.Instance == null) return;

        string newName = deckNameInput.text;
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogError("Deck name cannot be empty!");
            // TODO: Show user feedback (e.g., highlight input field, popup)
            return;
        }

        bool saved = DeckManager.Instance.SaveEditedDeck(newName); 

        if (saved)
        {
            hasUnsavedChanges = false;
            if(saveButton != null) saveButton.GetComponentInChildren<Text>().text = "Save";
            Debug.Log("Deck saved successfully!");
            GoToMyDecks(); 
        }
        else
        {
            Debug.LogError("Failed to save deck. Name might be taken or another error occurred.");
            // TODO: Provide user feedback (e.g., popup)
        }
    }

    void OnBackButtonClicked()
    {
        if (hasUnsavedChanges)
        {
            // TODO: Implement a proper confirmation popup
            // For now, we'll use a simple Unity confirm dialog if in Editor, or just discard
            bool discard = true;
            #if UNITY_EDITOR
            discard = UnityEditor.EditorUtility.DisplayDialog(
                "Unsaved Changes",
                "You have unsaved changes. Are you sure you want to discard them and go back?",
                "Discard", "Cancel"
            );
            #endif

            if (discard)
            {
                ConfirmDiscardAndGoBack();
            }
        }
        else
        {
            GoToMyDecks(); 
        }
    }

    void ConfirmDiscardAndGoBack()
    {
        if(DeckManager.Instance != null)
        {
             DeckManager.Instance.DiscardDeckChanges();
        }
        hasUnsavedChanges = false;
        if(saveButton != null) saveButton.GetComponentInChildren<Text>().text = "Save";
        GoToMyDecks();
    }

    void GoToMyDecks()
    {
        if (deckBuilderCanvas != null) deckBuilderCanvas.SetActive(false);
        if (myDecksCanvas != null) 
        {
            myDecksCanvas.SetActive(true);
            MyDecksUI myDecksUIScript = myDecksCanvas.GetComponent<MyDecksUI>();
            if (myDecksUIScript != null) 
            {
                 myDecksUIScript.RefreshDisplay(); // Ensure deck list is up-to-date
            }
        }
    }
} 