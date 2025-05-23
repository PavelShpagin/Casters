using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;

public class DeckBuilderUI : MonoBehaviour
{
    [Header("Main Canvases/Panels")]
    public GameObject myDecksViewCanvas; // The main canvas for this UI
    public GameObject cardCollectionPanel; // The panel on the right holding the card list & search

    [Header("Deck List View (Left Panel)")]
    public Transform decksGridContainer; // Parent for deck boxes and add button
    public GameObject openedDeckViewPlaceholder; // Shows when a deck is "opened"

    [Header("Card Collection (Right Panel)")]
    public TMP_InputField searchInput; // Changed to TMP_InputField
    public Transform collectionContentParent; 

    [Header("Prefabs")]
    public GameObject cardItemPrefab;     
    public GameObject deckAddItemPrefab; // For the "+" button
    public GameObject deckBoxItemPrefab; // For individual deck representation

    // To keep track of instantiated card items in the collection for quick updates
    // We might not need CardItemUI if clicks on cards in collection do nothing for now
    private List<GameObject> instantiatedCollectionCards = new List<GameObject>();

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
        if (decksGridContainer == null) { Debug.LogError("DeckBuilderUI: DecksGridContainer not assigned!"); allRefsValid = false; }
        if (openedDeckViewPlaceholder == null) { Debug.LogError("DeckBuilderUI: OpenedDeckViewPlaceholder not assigned!"); allRefsValid = false; }
        if (searchInput == null) { Debug.LogError("DeckBuilderUI: SearchInput not assigned!"); allRefsValid = false; }
        if (collectionContentParent == null) { Debug.LogError("DeckBuilderUI: CollectionContentParent not assigned!"); allRefsValid = false; }
        if (cardItemPrefab == null) { Debug.LogError("DeckBuilderUI: CardItemPrefab not assigned!"); allRefsValid = false; }
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
        if (decksGridContainer != null) decksGridContainer.gameObject.SetActive(true);
        if (openedDeckViewPlaceholder != null) openedDeckViewPlaceholder.SetActive(false);
        PopulateDeckList();
    }

    void ShowOpenedDeckView(string deckNameContext)
    {
        if (decksGridContainer != null) decksGridContainer.gameObject.SetActive(false);
        if (openedDeckViewPlaceholder != null)
        {
            openedDeckViewPlaceholder.SetActive(true);
            // Optionally, display the deck name on the placeholder
            TextMeshProUGUI placeholderText = openedDeckViewPlaceholder.GetComponentInChildren<TextMeshProUGUI>();
            if (placeholderText != null)
            {
                placeholderText.text = $"Viewing: {deckNameContext}\n(Deck content area)";
            }
            else
            {
                 Text legacyText = openedDeckViewPlaceholder.GetComponentInChildren<Text>();
                 if(legacyText != null) legacyText.text = $"Viewing: {deckNameContext}\n(Deck content area)";
            }
        }
        // The card collection on the right remains visible and searchable
    }

    void PopulateDeckList()
    {
        if (DeckManager.Instance == null || decksGridContainer == null) return;

        ClearContainer(decksGridContainer);

        // 1. Instantiate the "Add New Deck" button
        if (deckAddItemPrefab != null)
        {
            GameObject addItemGO = Instantiate(deckAddItemPrefab, decksGridContainer);
            Button addBtn = addItemGO.GetComponent<Button>();
            if (addBtn != null)
            {
                addBtn.onClick.RemoveAllListeners(); // Clear previous before adding
                addBtn.onClick.AddListener(HandleAddNewDeckClicked);
            }
            else
            {
                Debug.LogWarning("DeckAddItemPrefab does not have a Button component.");
            }
        }

        // 2. Instantiate a DeckBox for each saved deck
        List<Deck> allDecks = DeckManager.Instance.GetAllDecks(); // Assuming DeckManager has such a method
        if (allDecks == null)
        {
            Debug.LogError("DeckManager.Instance.GetAllDecks() returned null.");
            return;
        }
        
        foreach (Deck savedDeckData in allDecks)
        {
            if (deckBoxItemPrefab == null) continue;

            GameObject deckBoxGO = Instantiate(deckBoxItemPrefab, decksGridContainer);
            deckBoxGO.name = "DeckBox_" + savedDeckData.deckName;

            // Set the deck name (Attempt TextMeshProUGUI first, then fallback to Text)
            TextMeshProUGUI tmpText = deckBoxGO.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = savedDeckData.deckName;
            }
            else
            {
                Text legacyText = deckBoxGO.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = savedDeckData.deckName;
                }
                else
                {
                    // Debug.LogWarning($"DeckBoxItemPrefab '{deckBoxGO.name}' has no TextMeshProUGUI or Text component for the name.");
                }
            }

            // Make the deck box clickable
            Button button = deckBoxGO.GetComponent<Button>();
            if (button == null) // If prefab doesn't have a button, add one
            {
                button = deckBoxGO.AddComponent<Button>();
                // Optional: If you added a button, you might want to assign its Target Graphic
                // Image img = deckBoxGO.GetComponent<Image>();
                // if (img != null) button.targetGraphic = img;
            }
            
            if (button != null)
            {
                string currentDeckID = savedDeckData.uniqueID; // Capture for the closure
                string currentDeckName = savedDeckData.deckName;
                button.onClick.RemoveAllListeners(); // Clear previous before adding
                button.onClick.AddListener(() => HandleDeckSelected(currentDeckID, currentDeckName));
            }
        }
    }

    void PopulateCardCollection(string filter = "")
    {
        if (DeckManager.Instance == null) { Debug.LogError("[DeckBuilderUI] DeckManager.Instance is null."); return; }
        if (cardItemPrefab == null) { Debug.LogError("[DeckBuilderUI] cardItemPrefab is null."); return; }
        if (collectionContentParent == null) { Debug.LogError("[DeckBuilderUI] collectionContentParent is null."); return; }

        // CRITICAL: Verify we're using the correct parent object
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

        // Configure GridLayoutGroup for 3-column layout with proper sizing
        GridLayoutGroup gridLayout = collectionContentParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = collectionContentParent.gameObject.AddComponent<GridLayoutGroup>();
        }
        
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3; // Exactly 3 columns
        gridLayout.cellSize = new Vector2(80f, 112f); // Card size that fits 3 columns nicely
        gridLayout.spacing = new Vector2(5f, 5f); // Small spacing between cards
        gridLayout.padding = new RectOffset(5, 5, 5, 5); // Small padding around edges
        gridLayout.childAlignment = TextAnchor.UpperLeft; // Align to upper left
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal; // Fill horizontally first

        // Configure ContentSizeFitter for proper scrolling
        ContentSizeFitter sizeFitter = collectionContentParent.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = collectionContentParent.gameObject.AddComponent<ContentSizeFitter>();
        }
        
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        
        // Configure scroll settings
        ScrollRect scrollRect = collectionContentParent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollRect.scrollSensitivity = 25f;
            
            if (scrollRect.viewport != null)
            {
                Image viewportImage = scrollRect.viewport.GetComponent<Image>();
                if (viewportImage == null)
                {
                    viewportImage = scrollRect.viewport.gameObject.AddComponent<Image>();
                    viewportImage.color = new Color(0, 0, 0, 0.01f);
                }
                viewportImage.raycastTarget = true;
            }
        }

        ClearContainer(collectionContentParent, instantiatedCollectionCards);

        if (DeckManager.Instance.allMasterCardsList == null)
        {
             Debug.LogError("[DeckBuilderUI] DeckManager.Instance.allMasterCardsList is null.");
             return;
        }

        string lowerFilter = string.IsNullOrEmpty(filter) ? "" : filter.ToLower();
        int cardsProcessed = 0;
        int cardsDisplayed = 0;
        
        foreach (CardData card in DeckManager.Instance.allMasterCardsList.OrderBy(c => c.title))
        {
            cardsProcessed++;

            // Filter cards based on search term
            if (!string.IsNullOrEmpty(lowerFilter) &&
                !card.title.ToLower().Contains(lowerFilter) &&
                !(card.description != null && card.description.ToLower().Contains(lowerFilter)) &&
                !card.cardType.ToString().ToLower().Contains(lowerFilter) &&
                !card.faction.ToString().ToLower().Contains(lowerFilter))
            {
                continue; // Skip filtered cards
            }

            GameObject cardGO = Instantiate(cardItemPrefab, collectionContentParent);
            if (cardGO == null)
            {
                Debug.LogError("[DeckBuilderUI] Failed to instantiate card: " + card.title);
                continue;
            }
            
            // Remove Layout Element if it exists to avoid conflicts with GridLayoutGroup
            LayoutElement layoutElement = cardGO.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                DestroyImmediate(layoutElement);
            }
            
            // DO NOT modify RectTransform - let GridLayoutGroup handle everything
            instantiatedCollectionCards.Add(cardGO);

            CardItemUI cardUI = cardGO.GetComponent<CardItemUI>();
            if (cardUI != null)
            {
                cardUI.SetupCard(card, 0, CardLocation.Collection, null);
            }
            else
            {
                // Fallback: Set image directly
                Image cardDisplayImage = cardGO.GetComponent<Image>();
                if (cardDisplayImage != null && card.cardImage != null)
                {
                    cardDisplayImage.sprite = card.cardImage;
                }
            }
            
            cardsDisplayed++;
        }
        
        // Only show summary in console
        Debug.Log($"[DeckBuilderUI] PopulateCardCollection completed. Processed: {cardsProcessed}, Displayed: {cardsDisplayed}, Filter: '{filter}'");
        
        // Force layout refresh after population (safely)
        StartCoroutine(RefreshLayoutNextFrame());
    }
    
    private System.Collections.IEnumerator RefreshLayoutNextFrame()
    {
        yield return null; // Wait one frame
        if (collectionContentParent != null)
        {
            Canvas.ForceUpdateCanvases();
        }
    }

    void FilterCardCollectionDisplay(string searchTerm)
    {
        PopulateCardCollection(searchTerm);
    }

    void HandleDeckSelected(string deckID, string deckName)
    {
        Debug.Log($"Deck selected: {deckName} (ID: {deckID})");
        // Here, you could use DeckManager to set this as a "currently viewed" deck
        // DeckManager.Instance.SetCurrentlyViewedDeck(deckID); // Hypothetical method
        ShowOpenedDeckView(deckName);
    }

    void HandleAddNewDeckClicked()
    {
        if (DeckManager.Instance == null) return;

        // Create a new deck (DeckManager should handle unique naming if needed)
        int nextDeckNumber = (DeckManager.Instance.GetAllDecks()?.Count ?? 0) + 1;
        Deck newDeck = DeckManager.Instance.CreateNewDeck($"Untitled Deck {nextDeckNumber}");

        if (newDeck != null)
        {
            Debug.Log($"New deck created: {newDeck.deckName} (ID: {newDeck.uniqueID})");
            PopulateDeckList(); // Refresh the list to show the new deck
            HandleDeckSelected(newDeck.uniqueID, newDeck.deckName); // Immediately "open" it
        }
        else
        {
            Debug.LogError("Failed to create a new deck via DeckManager.");
        }
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
} 