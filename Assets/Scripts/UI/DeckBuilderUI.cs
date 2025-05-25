using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

public class DeckBuilderUI : MonoBehaviour
{
    [Header("Main Canvases/Panels")]
    public GameObject myDecksViewCanvas; // The main canvas for this UI
    public GameObject cardCollectionPanel; // The panel on the right holding the card list & search
    public GameObject deckSelectionPanel; // The panel showing the deck list (left side)

    [Header("Deck List View (Left Panel)")]
    public Transform decksGridContainer; // Parent for deck boxes and add button
    public GameObject openedDeckViewPlaceholder; // Shows when a deck is "opened"

    [Header("Card Collection (Right Panel)")]
    public TMP_InputField searchInput; // Changed to TMP_InputField
    public Transform collectionContentParent; 

    [Header("Deck Editing UI Elements")]
    public GameObject editDeckPanel; // The main panel containing all deck editing UI
    public Transform mainDeckContainer; // Container for main deck cards
    public Transform stageDeckContainer; // Container for stage deck cards
    public TMP_InputField deckNameInput; // Input field for deck name editing
    public Button backButton; // Back button to return to deck selection
    public Button trashButton; // Trash button to delete current deck

    [Header("Prefabs")]
    // cardItemPrefab removed - we now create cards programmatically with CardItemUI.CreateCardUI()
    public GameObject deckAddItemPrefab; // For the "+" button
    public GameObject deckBoxItemPrefab; // For individual deck representation

    // To keep track of instantiated card items in the collection for quick updates
    private List<GameObject> instantiatedCollectionCards = new List<GameObject>();
    private List<GameObject> instantiatedMainDeckCards = new List<GameObject>();
    private List<GameObject> instantiatedStageDeckCards = new List<GameObject>();
    
    // Current editing state
    private string currentEditingDeckID = null;
    private bool isEditingDeck = false;

    void Start()
    {
        Debug.Log("[DeckBuilderUI] Start() called.");

        bool refsValid = ValidateCoreReferences();
        Debug.Log("[DeckBuilderUI] ValidateCoreReferences() returned: " + refsValid);

        if (!refsValid)
        {
            if (myDecksViewCanvas != null) myDecksViewCanvas.SetActive(false);
            this.enabled = false;
            Debug.LogError("[DeckBuilderUI] Core references invalid. Disabling script.");
            return;
        }

        if (searchInput != null)
        {
            searchInput.onValueChanged.AddListener(FilterCardCollectionDisplay);
            
            // Set placeholder text if it doesn't have one
            if (searchInput.placeholder != null)
            {
                var placeholderText = searchInput.placeholder.GetComponent<TMPro.TextMeshProUGUI>();
                if (placeholderText != null && string.IsNullOrEmpty(placeholderText.text))
                {
                    placeholderText.text = "Search cards by title...";
                    placeholderText.color = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent white
                }
            }
            
            Debug.Log("[DeckBuilderUI] Search input configured successfully");
        }
        else
        {
            Debug.LogWarning("[DeckBuilderUI] Search input not assigned!");
        }

        // Set up deck editing UI button listeners
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        if (trashButton != null)
        {
            trashButton.onClick.AddListener(OnTrashButtonClicked);
        }
        
        if (deckNameInput != null)
        {
            deckNameInput.onValueChanged.AddListener(OnDeckNameChanged);
        }

        ShowDeckListView();
        Debug.Log("[DeckBuilderUI] About to call PopulateCardCollection().");
        PopulateCardCollection();
    }

    bool ValidateCoreReferences()
    {
        Debug.Log("[DeckBuilderUI] Validating core references...");
        bool allRefsValid = true;
        if (myDecksViewCanvas == null) { Debug.LogError("DeckBuilderUI: MyDecksViewCanvas not assigned!"); allRefsValid = false; }
        if (cardCollectionPanel == null) { Debug.LogError("DeckBuilderUI: CardCollectionPanel not assigned!"); allRefsValid = false; }
        if (deckSelectionPanel == null) { Debug.LogWarning("DeckBuilderUI: DeckSelectionPanel not assigned - deck list may not show!"); }
        if (decksGridContainer == null) { Debug.LogError("DeckBuilderUI: DecksGridContainer not assigned!"); allRefsValid = false; }
        if (openedDeckViewPlaceholder == null) { Debug.LogError("DeckBuilderUI: OpenedDeckViewPlaceholder not assigned!"); allRefsValid = false; }
        if (searchInput == null) { Debug.LogError("DeckBuilderUI: SearchInput not assigned!"); allRefsValid = false; }
        if (collectionContentParent == null) { Debug.LogError("DeckBuilderUI: CollectionContentParent not assigned!"); allRefsValid = false; }
        
        // Deck editing UI validation
        if (editDeckPanel == null) { Debug.LogWarning("DeckBuilderUI: EditDeckPanel not assigned - deck editing may not work!"); }
        if (mainDeckContainer == null) { Debug.LogWarning("DeckBuilderUI: MainDeckContainer not assigned - deck editing may not work!"); }
        if (stageDeckContainer == null) { Debug.LogWarning("DeckBuilderUI: StageDeckContainer not assigned - deck editing may not work!"); }
        if (deckNameInput == null) { Debug.LogWarning("DeckBuilderUI: DeckNameInput not assigned - deck naming may not work!"); }
        if (backButton == null) { Debug.LogWarning("DeckBuilderUI: BackButton not assigned - navigation may not work!"); }
        if (trashButton == null) { Debug.LogWarning("DeckBuilderUI: TrashButton not assigned - deck deletion may not work!"); }
        
        // cardItemPrefab is no longer needed - we create cards programmatically
        if (deckAddItemPrefab == null) { Debug.LogError("DeckBuilderUI: DeckAddItemPrefab not assigned!"); allRefsValid = false; }
        if (deckBoxItemPrefab == null) { Debug.LogError("DeckBuilderUI: DeckBoxItemPrefab not assigned!"); allRefsValid = false; }

        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckBuilderUI: DeckManager.Instance is not found. Ensure DeckManager is in your scene and initialized.");
            allRefsValid = false;
        }
        else
        {
            Debug.Log($"DeckBuilderUI: DeckManager found with {DeckManager.Instance.allMasterCardsList?.Count ?? 0} cards loaded.");
        }
        
        Debug.Log($"DeckBuilderUI: Core references validation result: {allRefsValid}");
        return allRefsValid;
    }

    void ShowDeckListView()
    {
        Debug.Log("[DeckBuilderUI] Switching to Deck List View");
        
        isEditingDeck = false;
        currentEditingDeckID = null;
        
        // Show deck selection panel
        if (deckSelectionPanel != null)
        {
            deckSelectionPanel.SetActive(true);
            Debug.Log("[DeckBuilderUI] ✅ Activated DeckSelectionPanel");
        }
        else
        {
            Debug.LogWarning("[DeckBuilderUI] ⚠️ DeckSelectionPanel is null!");
        }
        
        if (decksGridContainer != null) 
        {
            decksGridContainer.gameObject.SetActive(true);
            Debug.Log("[DeckBuilderUI] ✅ Activated DecksGridContainer");
        }
        if (openedDeckViewPlaceholder != null) openedDeckViewPlaceholder.SetActive(false);
        
        // Hide deck editing UI elements
        if (editDeckPanel != null)
        {
            editDeckPanel.SetActive(false);
            Debug.Log("[DeckBuilderUI] ❌ Deactivated EditDeckPanel");
        }
        
        // Individual container deactivation (backup in case EditDeckPanel isn't assigned)
        if (mainDeckContainer != null) mainDeckContainer.gameObject.SetActive(false);
        if (stageDeckContainer != null) stageDeckContainer.gameObject.SetActive(false);
        if (deckNameInput != null) deckNameInput.gameObject.SetActive(false);
        if (backButton != null) backButton.gameObject.SetActive(false);
        if (trashButton != null) trashButton.gameObject.SetActive(false);
        
        // Refresh the deck list to show any changes made during editing
        Debug.Log("[DeckBuilderUI] Refreshing deck list after editing");
        PopulateDeckList();
        
        Debug.Log("[DeckBuilderUI] ✅ ShowDeckListView completed");
    }

    public void ShowDeckEditingView(string deckID, string deckName)
    {
        Debug.Log($"[DeckBuilderUI] ShowDeckEditingView called for deck: '{deckName}' (ID: {deckID})");
        
        isEditingDeck = true;
        currentEditingDeckID = deckID;
        
        // Hide deck selection panel
        if (deckSelectionPanel != null)
        {
            deckSelectionPanel.SetActive(false);
            Debug.Log("[DeckBuilderUI] ❌ Deactivated DeckSelectionPanel");
        }
        
        if (decksGridContainer != null) decksGridContainer.gameObject.SetActive(false);
        if (openedDeckViewPlaceholder != null) openedDeckViewPlaceholder.SetActive(false);
        
        // Show the main EditDeckPanel (this should show all deck editing UI)
        if (editDeckPanel != null)
        {
            editDeckPanel.SetActive(true);
            Debug.Log("[DeckBuilderUI] ✅ Activated EditDeckPanel");
        }
        else
        {
            Debug.LogError("[DeckBuilderUI] ❌ EditDeckPanel is null! Please assign it in the inspector.");
        }
        
        // Individual container activation (backup/additional verification)
        if (mainDeckContainer != null) 
            {
            mainDeckContainer.gameObject.SetActive(true);
            Debug.Log("[DeckBuilderUI] Activated main deck container");
            }
            else
            {
            Debug.LogWarning("[DeckBuilderUI] mainDeckContainer is null!");
        }
        
        if (stageDeckContainer != null) 
        {
            stageDeckContainer.gameObject.SetActive(true);
            Debug.Log("[DeckBuilderUI] Activated stage deck container");
        }
        else
        {
            Debug.LogWarning("[DeckBuilderUI] stageDeckContainer is null!");
        }
        
        if (deckNameInput != null) 
        {
            deckNameInput.gameObject.SetActive(true);
            deckNameInput.text = deckName; // Set the current deck name
            Debug.Log($"[DeckBuilderUI] Set deck name input to: '{deckName}'");
        }
        else
        {
            Debug.LogWarning("[DeckBuilderUI] deckNameInput is null!");
        }
        
        if (backButton != null) 
        {
            backButton.gameObject.SetActive(true);
            Debug.Log("[DeckBuilderUI] Activated back button");
        }
        else
        {
            Debug.LogWarning("[DeckBuilderUI] backButton is null!");
        }
        
        if (trashButton != null) 
        {
            trashButton.gameObject.SetActive(true);
            Debug.Log("[DeckBuilderUI] Activated trash button");
            }
        else
        {
            Debug.LogWarning("[DeckBuilderUI] trashButton is null!");
        }
        
        // Load and display the deck content
        Debug.Log("[DeckBuilderUI] Loading deck for editing...");
        LoadDeckForEditing(deckID);
        
        // Update collection display to show copy indicators for this deck
        Debug.Log("[DeckBuilderUI] Refreshing card collection display...");
        PopulateCardCollection();
        
        Debug.Log("[DeckBuilderUI] ShowDeckEditingView completed successfully");
    }

    void ShowOpenedDeckView(string deckNameContext)
    {
        // This method is being replaced by ShowDeckEditingView
        // Keeping for backwards compatibility but redirecting
        Debug.LogWarning("ShowOpenedDeckView is deprecated. Use ShowDeckEditingView instead.");
    }

    void PopulateDeckList()
    {
        Debug.Log("[DeckBuilderUI] === PopulateDeckList START ===");
        
        if (DeckManager.Instance == null || decksGridContainer == null) 
        {
            Debug.LogError("[DeckBuilderUI] Cannot populate deck list - DeckManager or decksGridContainer is null");
            return;
        }

        Debug.Log($"[DeckBuilderUI] DecksGridContainer: {decksGridContainer.name} (Active: {decksGridContainer.gameObject.activeInHierarchy})");

        ClearContainer(decksGridContainer);
        Debug.Log("[DeckBuilderUI] Cleared existing deck containers");

        // 1. Instantiate the "Add New Deck" button
        if (deckAddItemPrefab != null)
        {
            GameObject addItemGO = Instantiate(deckAddItemPrefab, decksGridContainer);
            addItemGO.SetActive(true); // Explicitly activate the add button
            Debug.Log($"[DeckBuilderUI] Created Add Deck button: {addItemGO.name}");
            Button addBtn = addItemGO.GetComponent<Button>();
            if (addBtn != null)
            {
                addBtn.onClick.RemoveAllListeners(); // Clear previous before adding
                addBtn.onClick.AddListener(HandleAddNewDeckClicked);
                Debug.Log("[DeckBuilderUI] Add button configured successfully");
            }
            else
            {
                Debug.LogWarning("DeckAddItemPrefab does not have a Button component.");
            }
        }
        else
        {
            Debug.LogError("[DeckBuilderUI] deckAddItemPrefab is null!");
        }

        // 2. Instantiate a DeckBox for each saved deck
        List<Deck> allDecks = DeckManager.Instance.GetAllDecks(); // Assuming DeckManager has such a method
        if (allDecks == null)
        {
            Debug.LogError("DeckManager.Instance.GetAllDecks() returned null.");
            return;
        }
        
        Debug.Log($"[DeckBuilderUI] Found {allDecks.Count} decks to display");
        
        foreach (Deck savedDeckData in allDecks)
        {
            if (deckBoxItemPrefab == null) 
            {
                Debug.LogError("[DeckBuilderUI] deckBoxItemPrefab is null!");
                continue;
            }

            GameObject deckBoxGO = Instantiate(deckBoxItemPrefab, decksGridContainer);
            deckBoxGO.name = "DeckBox_" + savedDeckData.deckName;
            deckBoxGO.SetActive(true); // Explicitly activate the deck box
            Debug.Log($"[DeckBuilderUI] Created deck box: {deckBoxGO.name} (Active: {deckBoxGO.activeInHierarchy})");

            // Set up DeckBox component if it exists
            DeckBox deckBox = deckBoxGO.GetComponent<DeckBox>();
            if (deckBox != null)
            {
                // Remove DeckBox component - we don't want popup functionality in deck builder
                DestroyImmediate(deckBox);
                Debug.Log($"[DeckBuilderUI] Removed DeckBox component from {savedDeckData.deckName} (deck builder doesn't need popup)");
            }
            
            // Always use fallback button setup for deck builder panel
            {
            // Set the deck name (Attempt TextMeshProUGUI first, then fallback to Text)
            TextMeshProUGUI tmpText = deckBoxGO.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = savedDeckData.deckName;
                    Debug.Log($"[DeckBuilderUI] Set deck name via TMP: {savedDeckData.deckName}");
            }
            else
            {
                Text legacyText = deckBoxGO.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = savedDeckData.deckName;
                        Debug.Log($"[DeckBuilderUI] Set deck name via Text: {savedDeckData.deckName}");
                }
                else
                {
                        Debug.LogWarning($"DeckBoxItemPrefab '{deckBoxGO.name}' has no TextMeshProUGUI or Text component for the name.");
                }
            }

                // Update deck info (card count, etc.)
                UpdateDeckBoxInfo(deckBoxGO, savedDeckData);

                // Make the deck box clickable to open editor (NOT popup)
            Button button = deckBoxGO.GetComponent<Button>();
            if (button == null) // If prefab doesn't have a button, add one
            {
                button = deckBoxGO.AddComponent<Button>();
                    Debug.Log($"[DeckBuilderUI] Added Button component to {deckBoxGO.name}");
            }
            
            if (button != null)
            {
                string currentDeckID = savedDeckData.uniqueID; // Capture for the closure
                string currentDeckName = savedDeckData.deckName;
                button.onClick.RemoveAllListeners(); // Clear previous before adding
                button.onClick.AddListener(() => HandleDeckSelected(currentDeckID, currentDeckName));
                    Debug.Log($"[DeckBuilderUI] Configured click handler for {deckBoxGO.name} to open EDITOR (not popup)");
                }
            }
        }
        
        Debug.Log($"[DeckBuilderUI] === PopulateDeckList END - Created {allDecks.Count} deck boxes ===");
        
        // Force layout rebuild and debug hierarchy
        if (decksGridContainer != null)
        {
            Debug.Log($"[DeckBuilderUI] DecksGridContainer active: {decksGridContainer.gameObject.activeInHierarchy}");
            Debug.Log($"[DeckBuilderUI] DecksGridContainer parent active: {decksGridContainer.parent?.gameObject.activeInHierarchy}");
            Debug.Log($"[DeckBuilderUI] DecksGridContainer child count: {decksGridContainer.childCount}");
            
            // Force layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(decksGridContainer as RectTransform);
            
            // Check if DeckSelectionPanel is visible
            if (deckSelectionPanel != null)
            {
                Debug.Log($"[DeckBuilderUI] DeckSelectionPanel active: {deckSelectionPanel.activeInHierarchy}");
                Debug.Log($"[DeckBuilderUI] DeckSelectionPanel local active: {deckSelectionPanel.activeSelf}");
            }
        }
    }

    private void UpdateDeckBoxInfo(GameObject deckBoxGO, Deck deckData)
    {
        // Try to find a component that can display additional deck info
        // Look for any text components that might show card count
        Text[] textComponents = deckBoxGO.GetComponentsInChildren<Text>();
        TextMeshProUGUI[] tmpComponents = deckBoxGO.GetComponentsInChildren<TextMeshProUGUI>();
        
        int totalCards = deckData.mainDeckCards.Values.Sum() + deckData.stageDeckCards.Values.Sum();
        string deckInfo = $"{totalCards} cards";
        
        // Try to set additional info in secondary text components
        if (tmpComponents.Length > 1)
        {
            tmpComponents[1].text = deckInfo;
        }
        else if (textComponents.Length > 1)
        {
            textComponents[1].text = deckInfo;
        }
    }

    void PopulateCardCollection(string filter = "")
    {
        Debug.Log("[DeckBuilderUI] === PopulateCardCollection START ===");
        
        if (DeckManager.Instance == null) { Debug.LogError("[DeckBuilderUI] DeckManager.Instance is null."); return; }
        if (collectionContentParent == null) { Debug.LogError("[DeckBuilderUI] collectionContentParent is null."); return; }

        Debug.Log($"[DeckBuilderUI] DeckManager found: {DeckManager.Instance != null}");
        Debug.Log($"[DeckBuilderUI] Player collection exists: {DeckManager.Instance?.playerCollection != null}");
        Debug.Log($"[DeckBuilderUI] Total cards in master list: {DeckManager.Instance?.allMasterCardsList?.Count ?? 0}");

        // Use the existing CardsGridContent object as configured by the user
        if (collectionContentParent.name != "CardsGridContent")
        {
            Debug.LogError("[DeckBuilderUI] Wrong parent assigned! Please assign CardsGridContent to Collection Content Parent field in inspector!");
            
            // Try to find CardsGridContent automatically
            Transform cardsGridContent = GameObject.Find("CardsGridContent")?.transform;
            if (cardsGridContent != null)
            {
                Debug.Log("[DeckBuilderUI] Found CardsGridContent automatically. Using it instead.");
                collectionContentParent = cardsGridContent;
            }
            else
            {
                Debug.LogError("[DeckBuilderUI] Could not find CardsGridContent object in scene!");
                return;
            }
        }

        // Clear existing cards first
        ClearContainer(collectionContentParent, instantiatedCollectionCards);

        // Keep the existing GridLayoutGroup settings (don't override user's configuration)
        GridLayoutGroup gridLayout = collectionContentParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            Debug.LogError("[DeckBuilderUI] No GridLayoutGroup found on CardsGridContent!");
            return;
        }

        // Ensure ContentSizeFitter is properly configured for scrolling
        ContentSizeFitter sizeFitter = collectionContentParent.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = collectionContentParent.gameObject.AddComponent<ContentSizeFitter>();
        }
        
        // Configure ContentSizeFitter for proper scrolling (this is essential)
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        if (DeckManager.Instance.allMasterCardsList == null)
        {
             Debug.LogError("[DeckBuilderUI] DeckManager.Instance.allMasterCardsList is null.");
             return;
        }

        string lowerFilter = string.IsNullOrEmpty(filter) ? "" : filter.ToLower().Trim();
        int cardsDisplayed = 0;
        int totalCards = DeckManager.Instance.allMasterCardsList.Count;
        
        Debug.Log($"[DeckBuilderUI] Searching through {totalCards} cards with filter: '{filter}'");
        
        foreach (CardData card in DeckManager.Instance.allMasterCardsList)
        {
            if (card == null) continue;

            // Apply search filter
            if (!string.IsNullOrEmpty(lowerFilter) && !CardMatchesSearchFilter(card, lowerFilter))
            {
                continue;
            }

            int ownedCopies = 0;
            if (DeckManager.Instance.playerCollection != null)
            {
                ownedCopies = DeckManager.Instance.playerCollection.GetCardCount(card);
            }

            // Create card UI programmatically
            GameObject cardObj = CardItemUI.CreateCardUI(card, collectionContentParent);
            if (cardObj != null)
            {
                instantiatedCollectionCards.Add(cardObj);
                
                // Add right-click functionality for adding to deck (only when editing)
                if (isEditingDeck)
                {
                    CardItemUI cardUI = cardObj.GetComponent<CardItemUI>();
            if (cardUI != null)
            {
                        cardUI.onRightClick += () => AddCardToDeck(card);
                    }
                }

                // Calculate copy indicators based on current deck state
                int cardsInDeck = 0;
                int remainingCopies = ownedCopies;
                
                if (isEditingDeck && currentEditingDeckID != null && DeckManager.Instance != null)
                {
                    Deck currentDeck = DeckManager.Instance.GetDeck(currentEditingDeckID);
                    if (currentDeck != null)
            {
                        // Count cards in current deck
                        if (currentDeck.mainDeckCards.ContainsKey(card))
                        {
                            cardsInDeck += currentDeck.mainDeckCards[card];
                        }
                        if (currentDeck.stageDeckCards.ContainsKey(card))
                        {
                            cardsInDeck += currentDeck.stageDeckCards[card];
                        }
                        
                        remainingCopies = ownedCopies - cardsInDeck;
                    }
                }

                // Add copy indicators with proper colors
                CardItemUI cardItemUI = cardObj.GetComponent<CardItemUI>();
                if (cardItemUI != null)
                {
                    // Simple logic: show white circles for deck cards, then gray for remaining
                    cardItemUI.ShowCopyIndicators(0, Color.clear); // Clear existing
                    
                    // First, show white circles for cards in deck
                    if (cardsInDeck > 0)
                    {
                        cardItemUI.ShowCopyIndicators(cardsInDeck, Color.white);
                    }
                    
                    // Then, add gray circles for remaining copies in collection
                    if (remainingCopies > 0)
                    {
                        for (int i = 0; i < remainingCopies; i++)
                {
                            cardItemUI.ShowAdditionalCopyIndicators(1, new Color(0.4f, 0.4f, 0.4f, 1f));
                        }
                    }
                    
                    // Set transparency based on card type and availability
                    bool isStageCard = card.cardType == CardType.Stage;
                    
                    if (ownedCopies <= 0)
                    {
                        // No copies owned - make very transparent
                        cardItemUI.SetTransparency(0.35f);
                    }
                    else if (isStageCard && cardsInDeck > 0)
                    {
                        // Stage card already in deck - make opaque (can't add more)
                        cardItemUI.SetTransparency(0.7f);
                    }
                    else if (!isStageCard && remainingCopies <= 0)
                    {
                        // Main deck card - all copies are in deck - make slightly transparent
                        cardItemUI.SetTransparency(0.7f);
                    }
                    else
                    {
                        // Has available copies - full opacity
                        cardItemUI.SetTransparency(1.0f);
                }
            }
            
            cardsDisplayed++;
            }
        }
        
        Debug.Log($"[DeckBuilderUI] PopulateCardCollection completed. Displayed: {cardsDisplayed} cards (out of {totalCards} total) with copy indicators, Filter: '{filter}'");
        Debug.Log("[DeckBuilderUI] === PopulateCardCollection END ===");
        
        // Force layout rebuild to ensure proper scrolling
        StartCoroutine(RefreshLayoutForScrolling());
    }
    
    private System.Collections.IEnumerator RefreshLayoutForScrolling()
    {
        yield return null; // Wait one frame
        
        if (collectionContentParent != null)
        {
            // Just force rebuild the layout - that's all we need for scrolling to work
            LayoutRebuilder.ForceRebuildLayoutImmediate(collectionContentParent as RectTransform);
            
            // Find the ScrollRect component and ensure it's properly configured
            ScrollRect scrollRect = collectionContentParent.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                // Simply ensure vertical scrolling is enabled
                scrollRect.vertical = true;
                // DON'T reset scroll position - let user maintain their current position
                // scrollRect.normalizedPosition = new Vector2(0, 1); // REMOVED - this was causing the jump
            }
        }
    }

    void FilterCardCollectionDisplay(string searchTerm)
    {
        Debug.Log($"[DeckBuilderUI] Filtering cards with search term: '{searchTerm}'");
        PopulateCardCollection(searchTerm);
    }

    // Enhanced search method with multiple search criteria
    private bool CardMatchesSearchFilter(CardData card, string filter)
    {
        if (string.IsNullOrEmpty(filter))
            return true; // Show all cards when no filter

        string lowerFilter = filter.ToLower().Trim();
        if (string.IsNullOrEmpty(lowerFilter))
            return true;

        // Primary search: Card title (most important)
        if (card.title.ToLower().Contains(lowerFilter))
            return true;

        // Secondary search: Card description
        if (!string.IsNullOrEmpty(card.description) && card.description.ToLower().Contains(lowerFilter))
            return true;

        // Tertiary search: Card type
        if (card.cardType.ToString().ToLower().Contains(lowerFilter))
            return true;

        // Quaternary search: Card faction
        if (card.faction.ToString().ToLower().Contains(lowerFilter))
            return true;

        // Cost search (if searching for numbers)
        if (!string.IsNullOrEmpty(card.cost) && card.cost.Contains(lowerFilter))
            return true;

        return false; // No match found
    }

    void HandleDeckSelected(string deckID, string deckName)
    {
        Debug.Log($"Deck selected: {deckName} (ID: {deckID})");
        // Here, you could use DeckManager to set this as a "currently viewed" deck
        // DeckManager.Instance.SetCurrentlyViewedDeck(deckID); // Hypothetical method
        ShowDeckEditingView(deckID, deckName);
    }

    void HandleAddNewDeckClicked()
    {
        Debug.Log("[DeckBuilderUI] === HandleAddNewDeckClicked START ===");
        
        if (DeckManager.Instance == null)
        {
            Debug.LogError("[DeckBuilderUI] DeckManager.Instance is null!");
            return;
        }

        // Create a new deck with unique naming
        List<Deck> existingDecks = DeckManager.Instance.GetAllDecks();
        Debug.Log($"[DeckBuilderUI] Found {existingDecks?.Count ?? 0} existing decks");
        
        int nextDeckNumber = (existingDecks?.Count ?? 0) + 1;
        string proposedName = $"Untitled Deck {nextDeckNumber}";
        
        // Ensure name is unique
        while (existingDecks != null && existingDecks.Any(d => d.deckName == proposedName))
        {
            nextDeckNumber++;
            proposedName = $"Untitled Deck {nextDeckNumber}";
        }
        
        Debug.Log($"[DeckBuilderUI] Creating new deck with name: '{proposedName}'");
        Deck newDeck = DeckManager.Instance.CreateNewDeck(proposedName);

        if (newDeck != null)
        {
            DeckManager.Instance.SaveDecks(); // Save immediately after creating
            Debug.Log($"[DeckBuilderUI] New deck created successfully: {newDeck.deckName} (ID: {newDeck.uniqueID})");
            
            // Refresh the deck list first to include the new deck
            Debug.Log("[DeckBuilderUI] About to call PopulateDeckList()");
            PopulateDeckList();
            Debug.Log("[DeckBuilderUI] PopulateDeckList() completed");
            
            // Immediately open the new deck in the editor
            Debug.Log($"[DeckBuilderUI] Opening new deck '{newDeck.deckName}' in editor");
            ShowDeckEditingView(newDeck.uniqueID, newDeck.deckName);
        }
        else
        {
            Debug.LogError("[DeckBuilderUI] Failed to create a new deck via DeckManager.");
        }
        
        Debug.Log("[DeckBuilderUI] === HandleAddNewDeckClicked END ===");
    }

    void ClearContainer(Transform container, List<GameObject> trackingList = null)
    {
        if (trackingList != null)
        {
            foreach (GameObject go in trackingList)
            {
                if (go != null) Destroy(go);
            }
            trackingList.Clear();
        }
        else
        {
            foreach (Transform child in container)
            {
                if (child != null) Destroy(child.gameObject);
            }
        }
    }

    public void DisplayDeckFromExternal(string deckID, string deckName)
    {
        // This method can be called by MyDecksUI or other scripts
        Debug.Log($"DeckBuilderUI: External request to display deck: {deckName} (ID: {deckID})");
        HandleDeckSelected(deckID, deckName); // Use existing internal logic
    }

    // Add this method to DeckBuilderUI class for debugging
    [ContextMenu("Debug Card Collection")]
    public void DebugCardCollection()
    {
        Debug.Log($"=== CARD COLLECTION DEBUG ===");
        Debug.Log($"Collection Content Parent: {collectionContentParent?.name}");
        Debug.Log($"Active: {collectionContentParent?.gameObject.activeInHierarchy}");
        Debug.Log($"Child Count: {collectionContentParent?.childCount}");
        
        if (collectionContentParent != null)
        {
            GridLayoutGroup grid = collectionContentParent.GetComponent<GridLayoutGroup>();
            Debug.Log($"GridLayout: {grid != null}, CellSize: {grid?.cellSize}, Spacing: {grid?.spacing}");
            
            RectTransform rect = collectionContentParent.GetComponent<RectTransform>();
            Debug.Log($"Content RectTransform Size: {rect.rect.size}, Position: {rect.anchoredPosition}");
            
            for (int i = 0; i < collectionContentParent.childCount; i++)
            {
                Transform child = collectionContentParent.GetChild(i);
                Debug.Log($"Child {i}: {child.name}, Active: {child.gameObject.activeInHierarchy}");
                
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    Debug.Log($"  - Image: Enabled={img.enabled}, Sprite={img.sprite?.name}, Color={img.color}");
                }

                // Check for copy indicators
                Transform copyIndicators = child.Find("CopyIndicators");
                if (copyIndicators != null)
                {
                    Debug.Log($"  - Copy Indicators: {copyIndicators.childCount} indicators found");
                    for (int j = 0; j < copyIndicators.childCount; j++)
                    {
                        Transform indicator = copyIndicators.GetChild(j);
                        Debug.Log($"    * Indicator {j}: {indicator.name}");
                    }
                }
                else
                {
                    Debug.Log($"  - No copy indicators found");
                }
            }
        }

        // Debug player collection
        if (DeckManager.Instance?.playerCollection != null)
        {
            Debug.Log($"Player Collection has {DeckManager.Instance.playerCollection.ownedCards.Count} different cards");
            foreach (var kvp in DeckManager.Instance.playerCollection.ownedCards.Take(5)) // Show first 5
            {
                Debug.Log($"  - {kvp.Key.title}: {kvp.Value} copies");
            }
        }
        
        Debug.Log($"=== END DEBUG ===");
    }

    [ContextMenu("Test Card Creation")]
    public void TestCardCreation()
    {
        if (DeckManager.Instance == null || collectionContentParent == null) return;
        
        var firstCard = DeckManager.Instance.allMasterCardsList.FirstOrDefault();
        if (firstCard != null)
        {
            Debug.Log($"Creating test card for: {firstCard.title}");
            int copies = DeckManager.Instance.GetPlayerCardCount(firstCard);
            Debug.Log($"Player has {copies} copies of {firstCard.title}");
            
            GameObject testCard = CardItemUI.CreateCardUI(collectionContentParent, firstCard, CardLocation.Collection, null);
            Debug.Log($"Test card created: {testCard?.name}");
        }
    }

    [ContextMenu("Force Refresh Collection")]
    public void ForceRefreshCollection()
    {
        Debug.Log("[DeckBuilderUI] === FORCE REFRESH COLLECTION ===");
        PopulateCardCollection();
    }

    [ContextMenu("Clear All Cards")]
    public void ClearAllCards()
    {
        if (collectionContentParent != null)
        {
            ClearContainer(collectionContentParent, instantiatedCollectionCards);
            Debug.Log("[DeckBuilderUI] Cleared all cards from collection");
        }
    }

    [ContextMenu("Diagnose Player Collection")]
    public void DiagnosePlayerCollection()
    {
        Debug.Log("=== PLAYER COLLECTION DIAGNOSIS ===");
        
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager.Instance is null!");
            return;
        }
        
        if (DeckManager.Instance.playerCollection == null)
        {
            Debug.LogError("Player collection is null!");
            return;
        }
        
        var collection = DeckManager.Instance.playerCollection;
        Debug.Log($"Player collection has {collection.ownedCards.Count} different card types");
        
        foreach (var kvp in collection.ownedCards.Take(5)) // Show first 5
        {
            Debug.Log($"  - {kvp.Key?.title ?? "NULL"}: {kvp.Value} copies");
        }
        
        if (DeckManager.Instance.allMasterCardsList != null)
        {
            Debug.Log($"Master cards list has {DeckManager.Instance.allMasterCardsList.Count} cards");
            var firstCard = DeckManager.Instance.allMasterCardsList.FirstOrDefault();
            if (firstCard != null)
            {
                int playerCopies = DeckManager.Instance.GetPlayerCardCount(firstCard);
                Debug.Log($"First card '{firstCard.title}' - player has {playerCopies} copies");
            }
        }
        
        Debug.Log("=== END DIAGNOSIS ===");
    }

    [ContextMenu("Test Search Functionality")]
    public void TestSearchFunctionality()
    {
        Debug.Log("=== TESTING SEARCH FUNCTIONALITY ===");
        
        if (DeckManager.Instance?.allMasterCardsList == null)
        {
            Debug.LogError("No cards available to test search!");
            return;
        }
        
        // Test with first card's title
        var firstCard = DeckManager.Instance.allMasterCardsList.FirstOrDefault();
        if (firstCard != null)
        {
            string testSearch = firstCard.title.Substring(0, Math.Min(3, firstCard.title.Length));
            Debug.Log($"Testing search with: '{testSearch}'");
            
            int matchCount = 0;
            foreach (var card in DeckManager.Instance.allMasterCardsList)
            {
                if (CardMatchesSearchFilter(card, testSearch))
                {
                    matchCount++;
                }
            }
            
            Debug.Log($"Search for '{testSearch}' found {matchCount} matches");
        }
        
        Debug.Log("=== END SEARCH TEST ===");
    }

    [ContextMenu("Clear Search")]
    public void ClearSearch()
    {
        if (searchInput != null)
        {
            searchInput.text = "";
            Debug.Log("[DeckBuilderUI] Search cleared");
        }
    }

    [ContextMenu("Debug Panel Assignments")]
    public void DebugPanelAssignments()
    {
        Debug.Log("=== PANEL ASSIGNMENTS DEBUG ===");
        Debug.Log($"MyDecksViewCanvas: {myDecksViewCanvas?.name} (Active: {myDecksViewCanvas?.activeInHierarchy})");
        Debug.Log($"CardCollectionPanel: {cardCollectionPanel?.name} (Active: {cardCollectionPanel?.activeInHierarchy})");
        Debug.Log($"DeckSelectionPanel: {deckSelectionPanel?.name} (Active: {deckSelectionPanel?.activeInHierarchy})");
        Debug.Log($"DecksGridContainer: {decksGridContainer?.name} (Active: {decksGridContainer?.gameObject.activeInHierarchy})");
        Debug.Log($"EditDeckPanel: {editDeckPanel?.name} (Active: {editDeckPanel?.activeInHierarchy})");
        
        if (deckSelectionPanel == null)
        {
            Debug.LogError("❌ DeckSelectionPanel is NOT ASSIGNED! Please assign it in the inspector.");
            
            // Try to find it automatically by name
            GameObject foundGO = GameObject.Find("DeckSelectionPanel");
            if (foundGO != null)
            {
                Debug.Log($"✅ Found DeckSelectionPanel GameObject: {foundGO.name}");
                Debug.Log("Please assign this to the DeckSelectionPanel field in DeckBuilderUI inspector!");
            }
            else
            {
                Debug.LogError("❌ Could not find DeckSelectionPanel in scene!");
            }
        }
        
        Debug.Log("=== END DEBUG ===");
    }

    [ContextMenu("Debug Persistence")]
    public void DebugPersistence()
    {
        Debug.Log("=== PERSISTENCE DEBUG ===");
        
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager.Instance is null!");
            return;
        }
        
        // Check save file paths
        string persistentPath = UnityEngine.Application.persistentDataPath;
        Debug.Log($"Persistent Data Path: {persistentPath}");
        
        string deckSavePath = System.IO.Path.Combine(persistentPath, "userDecks.json");
        string collectionSavePath = System.IO.Path.Combine(persistentPath, "playerCollection.json");
        
        Debug.Log($"Deck save file exists: {System.IO.File.Exists(deckSavePath)}");
        Debug.Log($"Collection save file exists: {System.IO.File.Exists(collectionSavePath)}");
        
        if (System.IO.File.Exists(deckSavePath))
        {
            var fileInfo = new System.IO.FileInfo(deckSavePath);
            Debug.Log($"Deck file last modified: {fileInfo.LastWriteTime}");
            Debug.Log($"Deck file size: {fileInfo.Length} bytes");
        }
        
        if (System.IO.File.Exists(collectionSavePath))
        {
            var fileInfo = new System.IO.FileInfo(collectionSavePath);
            Debug.Log($"Collection file last modified: {fileInfo.LastWriteTime}");
            Debug.Log($"Collection file size: {fileInfo.Length} bytes");
        }
        
        // Check current state
        var allDecks = DeckManager.Instance.GetAllDecks();
        Debug.Log($"Current decks in memory: {allDecks?.Count ?? 0}");
        
        if (DeckManager.Instance.playerCollection != null)
        {
            Debug.Log($"Player collection cards: {DeckManager.Instance.playerCollection.ownedCards?.Count ?? 0}");
        }
        
        Debug.Log("=== END PERSISTENCE DEBUG ===");
    }

    [ContextMenu("Force Save All")]
    public void ForceSaveAll()
    {
        Debug.Log("=== FORCE SAVE ALL ===");
        
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager.Instance is null!");
            return;
        }
        
        // Force save decks
        DeckManager.Instance.SaveDecks();
        Debug.Log("✅ Forced deck save");
        
        // Force save player collection
        DeckManager.Instance.SavePlayerCollection();
        Debug.Log("✅ Forced collection save");
        
        // Show current state
        var allDecks = DeckManager.Instance.GetAllDecks();
        Debug.Log($"📊 Current state: {allDecks?.Count ?? 0} decks in memory");
        
        foreach (var deck in allDecks.Take(3)) // Show first 3 decks
        {
            int mainCards = deck.mainDeckCards.Values.Sum();
            int stageCards = deck.stageDeckCards.Values.Sum();
            Debug.Log($"  - '{deck.deckName}': {mainCards} main + {stageCards} stage cards");
        }
        
        Debug.Log("=== FORCE SAVE COMPLETED ===");
    }

    [ContextMenu("Test Deck Selection Popup")]
    public void TestDeckSelectionPopup()
    {
        Debug.Log("=== TESTING DECK SELECTION POPUP ===");
        
        if (DeckSelectionPopup.Instance == null)
        {
            Debug.LogError("❌ DeckSelectionPopup.Instance is null!");
            
            // Try to find it in scene
            DeckSelectionPopup popup = FindFirstObjectByType<DeckSelectionPopup>();
            if (popup != null)
            {
                Debug.Log("✅ Found DeckSelectionPopup in scene (but Instance is null)");
            }
            else
            {
                Debug.LogError("❌ No DeckSelectionPopup found in scene at all!");
            }
            return;
        }
        
        Debug.Log("✅ DeckSelectionPopup.Instance exists, showing popup...");
        DeckSelectionPopup.Instance.ShowPopup(
            onDeckSelected: (deckID) => Debug.Log($"Test: Deck selected - {deckID}"),
            onDeckEdit: (deckID) => Debug.Log($"Test: Deck edit - {deckID}")
        );
        
        Debug.Log("=== END POPUP TEST ===");
    }

    // Public method to clear search (can be called from UI buttons)
    public void OnClearSearchButtonClicked()
    {
        ClearSearch();
    }

    void OnBackButtonClicked()
    {
        Debug.Log("[DeckBuilderUI] Back button clicked");
        
        // Auto-save before going back
        string savedDeckID = AutoSaveDeck();
        
        // Notify main menu if a deck was saved
        if (!string.IsNullOrEmpty(savedDeckID) && MainMenuDeckController.Instance != null)
        {
            MainMenuDeckController.Instance.OnDeckSaved(savedDeckID);
        }
        
        // Return to deck list view
        ShowDeckListView();
    }

    void OnTrashButtonClicked()
    {
        Debug.Log("[DeckBuilderUI] Trash button clicked");
        
        if (!isEditingDeck || string.IsNullOrEmpty(currentEditingDeckID) || DeckManager.Instance == null)
        {
            Debug.LogWarning("[DeckBuilderUI] Cannot delete deck: not currently editing a deck");
            return;
        }
        
        string deletedDeckID = currentEditingDeckID;
        
        // Delete the current deck
        bool deleted = DeckManager.Instance.DeleteDeck(currentEditingDeckID);
        if (deleted)
        {
            DeckManager.Instance.SaveDecks(); // Ensure deletion is saved immediately
            Debug.Log($"[DeckBuilderUI] Successfully deleted deck with ID: {currentEditingDeckID}");
            
            // Notify main menu
            if (MainMenuDeckController.Instance != null)
            {
                MainMenuDeckController.Instance.OnDeckDeleted(deletedDeckID);
            }
            
            // Return to deck list view
            ShowDeckListView();
        }
        else
        {
            Debug.LogError($"[DeckBuilderUI] Failed to delete deck with ID: {currentEditingDeckID}");
        }
    }

    void OnDeckNameChanged(string newName)
    {
        Debug.Log($"[DeckBuilderUI] Deck name changed to: {newName}");
        
        // Auto-save when name changes
        string savedDeckID = AutoSaveDeck();
        
        // Notify main menu if a deck was saved
        if (!string.IsNullOrEmpty(savedDeckID) && MainMenuDeckController.Instance != null)
        {
            MainMenuDeckController.Instance.OnDeckSaved(savedDeckID);
        }
    }

    public void LoadDeckForEditing(string deckID)
    {
        Debug.LogError("!!!!!! [DeckBuilderUI] LoadDeckForEditing CALLED with ID: " + deckID + " !!!!!!"); 
        
        if (DeckManager.Instance == null || string.IsNullOrEmpty(deckID))
        {
            Debug.LogError("[DeckBuilderUI] Cannot load deck: DeckManager missing or invalid deck ID");
            return;
        }
        
        Deck deck = DeckManager.Instance.GetDeck(deckID);
        if (deck == null)
        {
            Debug.LogError($"[DeckBuilderUI] Deck with ID {deckID} not found!");
            return;
        }
        
        // Set current editing state (if not already set by ShowDeckEditingView)
        currentEditingDeckID = deckID;
        isEditingDeck = true; 
        if (deckNameInput != null) deckNameInput.text = deck.deckName;

        // Clear existing deck displays
        ClearContainer(mainDeckContainer, instantiatedMainDeckCards);
        ClearContainer(stageDeckContainer, instantiatedStageDeckCards);
        
        // Load main deck cards (sorted alphabetically)
        var sortedMainCards = deck.mainDeckCards.Keys.OrderBy(card => card.title).ToList();
        foreach (CardData card in sortedMainCards)
        {
            int copies = deck.mainDeckCards[card];
            for (int i = 0; i < copies; i++)
            {
                CreateDeckCardUI(card, mainDeckContainer, instantiatedMainDeckCards, true);
            }
        }
        
        // Load stage deck cards (sorted alphabetically)
        var sortedStageCards = deck.stageDeckCards.Keys.OrderBy(card => card.title).ToList();
        foreach (CardData card in sortedStageCards)
        {
            int copies = deck.stageDeckCards[card];
            for (int i = 0; i < copies; i++)
            {
                CreateDeckCardUI(card, stageDeckContainer, instantiatedStageDeckCards, false);
            }
        }

        // Force layout rebuild for main deck container
        if (mainDeckContainer != null)
        {
            Debug.Log("[DeckBuilderUI] Forcing layout rebuild on mainDeckContainer...");
            LayoutRebuilder.ForceRebuildLayoutImmediate(mainDeckContainer as RectTransform);
            Debug.Log("[DeckBuilderUI] Main deck layout rebuild complete.");
        }
        else
        {
            Debug.LogWarning("[DeckBuilderUI] mainDeckContainer is null, cannot force layout rebuild!");
        }

        // Force layout rebuild for stage deck container
        if (stageDeckContainer != null)
        {
            Debug.Log("[DeckBuilderUI] Forcing layout rebuild on stageDeckContainer...");
            LayoutRebuilder.ForceRebuildLayoutImmediate(stageDeckContainer as RectTransform);
            Debug.Log("[DeckBuilderUI] Stage deck layout rebuild complete.");
        }
        else
        {
            Debug.LogWarning("[DeckBuilderUI] stageDeckContainer is null, cannot force layout rebuild!");
        }
        
        Debug.Log($"[DeckBuilderUI] Loaded deck '{deck.deckName}' with {sortedMainCards.Count} main card types and {sortedStageCards.Count} stage card types");
    }
    
    void CreateDeckCardUI(CardData card, Transform container, List<GameObject> trackingList, bool isMainDeck)
    {
        if (card == null || container == null) return;
        
        // Create card UI for deck (MainDeck or StageDeck location, no copy indicators)
        CardLocation location = isMainDeck ? CardLocation.MainDeck : CardLocation.StageDeck;
        GameObject cardObj = CardItemUI.CreateCardUI(container, card, location, null);
        if (cardObj != null)
        {
            trackingList.Add(cardObj);
            
            // Add right-click functionality for removal
            CardItemUI cardUI = cardObj.GetComponent<CardItemUI>();
            if (cardUI != null)
            {
                cardUI.onRightClick += () => RemoveCardFromDeck(card, isMainDeck);
            }
            
            Debug.Log($"[DeckBuilderUI] Created deck card UI for {card.title} (no copy indicators)");
        }
    }
    
    string AutoSaveDeck()
    {
        if (!isEditingDeck || string.IsNullOrEmpty(currentEditingDeckID) || DeckManager.Instance == null)
            return "";
            
        Deck currentDeck = DeckManager.Instance.GetDeck(currentEditingDeckID);
        if (currentDeck == null) return "";
        
        // Get current deck name from input field
        string deckName = "";
        if (deckNameInput != null)
        {
            deckName = deckNameInput.text.Trim();
        }
        
        // Check if deck has cards
        bool hasCards = currentDeck.mainDeckCards.Values.Sum() > 0 || currentDeck.stageDeckCards.Values.Sum() > 0;
        
        // Auto-save conditions: has cards or non-empty name
        if (hasCards || !string.IsNullOrEmpty(deckName))
        {
            // If name is empty but has cards, use default name "Deck" (but don't update input field)
            string finalDeckName = deckName;
            if (string.IsNullOrEmpty(finalDeckName) && hasCards)
            {
                finalDeckName = "Deck";
                // DON'T update the input field - let user see what they actually typed
                // if (deckNameInput != null)
                // {
                //     deckNameInput.text = finalDeckName;
                // }
            }
            
            currentDeck.deckName = finalDeckName;
            DeckManager.Instance.SaveDecks();
            Debug.Log($"[DeckBuilderUI] Auto-saved deck as '{finalDeckName}' (input shows: '{deckName}')");
            
            // Also save player collection in case cards were modified
            DeckManager.Instance.SavePlayerCollection();
            
            return currentEditingDeckID;
        }
        
        return "";
    }

    void AddCardToDeck(CardData card)
    {
        if (!isEditingDeck || card == null || DeckManager.Instance == null) return;
        
        Deck currentDeck = DeckManager.Instance.GetDeck(currentEditingDeckID);
        if (currentDeck == null) return;
        
        // Check player collection
        int playerCopies = DeckManager.Instance.playerCollection?.GetCardCount(card) ?? 0;
        if (playerCopies <= 0)
        {
            Debug.Log($"[DeckBuilderUI] Cannot add {card.title}: not owned by player");
            return;
        }
        
        // Determine if this should go to main deck or stage deck
        bool isStageCard = card.cardType == CardType.Stage;
        
        if (isStageCard)
        {
            // Stage deck validation: max 1 copy per card, max 5 cards total
            if (currentDeck.stageDeckCards.ContainsKey(card))
            {
                Debug.Log($"[DeckBuilderUI] Cannot add {card.title}: already in stage deck (max 1 copy)");
                return;
            }
            
            int totalStageCards = currentDeck.stageDeckCards.Values.Sum();
            if (totalStageCards >= 5)
            {
                Debug.Log($"[DeckBuilderUI] Cannot add {card.title}: stage deck full (max 5 cards)");
                return;
            }
            
            // Add to stage deck
            currentDeck.stageDeckCards[card] = 1;
            
            // Add card UI in alphabetically sorted position
            AddCardToContainer(card, stageDeckContainer, instantiatedStageDeckCards, false);
        }
        else
        {
            // Main deck validation: max 2 copies per card, max 30 cards total
            int currentCopies = currentDeck.mainDeckCards.ContainsKey(card) ? currentDeck.mainDeckCards[card] : 0;
            if (currentCopies >= 2)
            {
                Debug.Log($"[DeckBuilderUI] Cannot add {card.title}: already have 2 copies (max per card)");
                return;
            }
            
            int totalMainCards = currentDeck.mainDeckCards.Values.Sum();
            if (totalMainCards >= 30)
            {
                Debug.Log($"[DeckBuilderUI] Cannot add {card.title}: main deck full (max 30 cards)");
                return;
            }
            
            // Add to main deck
            if (!currentDeck.mainDeckCards.ContainsKey(card))
                currentDeck.mainDeckCards[card] = 0;
            currentDeck.mainDeckCards[card]++;
            
            // Add card UI in alphabetically sorted position
            AddCardToContainer(card, mainDeckContainer, instantiatedMainDeckCards, true);
        }
        
        AutoSaveDeck();
        
        // Refresh collection display to update copy indicators
        PopulateCardCollection();
        
        Debug.Log($"[DeckBuilderUI] Added {card.title} to {(isStageCard ? "stage" : "main")} deck");
    }
    
    void RemoveCardFromDeck(CardData card, bool isMainDeck)
    {
        if (!isEditingDeck || card == null || DeckManager.Instance == null) return;
        
        Deck currentDeck = DeckManager.Instance.GetDeck(currentEditingDeckID);
        if (currentDeck == null) return;
        
        if (isMainDeck)
        {
            if (!currentDeck.mainDeckCards.ContainsKey(card) || currentDeck.mainDeckCards[card] <= 0)
            {
                Debug.LogWarning($"[DeckBuilderUI] Cannot remove {card.title}: not in main deck");
                return;
            }
            
            currentDeck.mainDeckCards[card]--;
            if (currentDeck.mainDeckCards[card] <= 0)
            {
                currentDeck.mainDeckCards.Remove(card);
            }
            
            // Remove one card UI from main deck container
            RemoveCardFromContainer(card, mainDeckContainer, instantiatedMainDeckCards);
        }
        else
        {
            if (!currentDeck.stageDeckCards.ContainsKey(card))
            {
                Debug.LogWarning($"[DeckBuilderUI] Cannot remove {card.title}: not in stage deck");
                return;
            }
            
            currentDeck.stageDeckCards.Remove(card);
            
            // Remove card UI from stage deck container
            RemoveCardFromContainer(card, stageDeckContainer, instantiatedStageDeckCards);
        }
        
        AutoSaveDeck();
        
        // Refresh collection display to update copy indicators
        PopulateCardCollection();
        
        Debug.Log($"[DeckBuilderUI] Removed {card.title} from {(isMainDeck ? "main" : "stage")} deck");
    }
    
    void AddCardToContainer(CardData card, Transform container, List<GameObject> trackingList, bool isMainDeck)
    {
        // Create the new card UI
        GameObject newCardObj = CardItemUI.CreateCardUI(container, card, isMainDeck ? CardLocation.MainDeck : CardLocation.StageDeck, null);
        if (newCardObj == null) return;
        
        // Add right-click functionality
        CardItemUI cardUI = newCardObj.GetComponent<CardItemUI>();
        if (cardUI != null)
        {
            cardUI.onRightClick += () => RemoveCardFromDeck(card, isMainDeck);
        }
        
        // Find correct position to maintain alphabetical order
        int insertIndex = 0;
        for (int i = 0; i < trackingList.Count; i++)
        {
            CardItemUI existingCardUI = trackingList[i].GetComponent<CardItemUI>();
            if (existingCardUI != null && existingCardUI.CardData != null)
            {
                if (string.Compare(card.title, existingCardUI.CardData.title, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    insertIndex = i;
                    break;
                }
                insertIndex = i + 1;
            }
        }
        
        // Insert at correct position
        trackingList.Insert(insertIndex, newCardObj);
        newCardObj.transform.SetSiblingIndex(insertIndex);
    }
    
    void RemoveCardFromContainer(CardData card, Transform container, List<GameObject> trackingList)
    {
        // Find and remove the first instance of this card
        for (int i = trackingList.Count - 1; i >= 0; i--)
        {
            if (trackingList[i] != null)
            {
                CardItemUI cardUI = trackingList[i].GetComponent<CardItemUI>();
                if (cardUI != null && cardUI.CardData == card)
                {
                    GameObject cardToRemove = trackingList[i];
                    trackingList.RemoveAt(i);
                    DestroyImmediate(cardToRemove);
                    break;
                }
            }
        }
    }

    #region Public Methods for External Access
    
    /// <summary>
    /// Creates a new deck and opens it for editing
    /// </summary>
    public void CreateNewDeck()
    {
        if (DeckManager.Instance == null)
        {
            Debug.LogError("[DeckBuilderUI] Cannot create new deck: DeckManager.Instance is null!");
            return;
        }
        
        Deck newDeck = DeckManager.Instance.CreateNewDeck();
        if (newDeck != null)
        {
            Debug.Log($"[DeckBuilderUI] Created new deck with ID: {newDeck.uniqueID}");
            ShowDeckEditingView(newDeck.uniqueID, newDeck.deckName);
        }
        else
        {
            Debug.LogError("[DeckBuilderUI] Failed to create new deck!");
        }
    }
    
    #endregion
} 