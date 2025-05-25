using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections; // Re-added for IEnumerator

public class MainMenuDeckController : MonoBehaviour
{
    [Header("Main Menu References")]
    [SerializeField] private GameObject deckBox;
    [SerializeField] private GameObject plusBox;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI deckLabel; // DeckLabel inside DeckBox (near play button)
    // deckIcon removed - it's just a static image, not for status display
    
    [Header("Deck Selection Popup")]
    [SerializeField] private GameObject deckSelectContent;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform deckGridContent; // DeckSelectContent/Panel/Scroll View/Viewport/DeckGridContent
    [SerializeField] private GameObject editMainmenuDeckBoxPrefab;
    
    [Header("Scene Management")]
    [SerializeField] private MainMenuController mainMenuController; // Reference to MainMenuController for scene switching
    [SerializeField] private DeckBuilderUI deckBuilderUI; // Reference to DeckBuilderUI component
    
    [Header("Background Panel (for closing on click)")]
    [SerializeField] private Button backgroundPanel; // The Panel inside DeckSelectContent
    
    // Data
    private string selectedDeckID = "";
    private List<GameObject> instantiatedDeckItems = new List<GameObject>();
    
    public static MainMenuDeckController Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        RefreshDeckDisplay();
        HideDeckSelectContent();
    }
    
    void SetupUI()
    {
        // Setup DeckBox click to open deck selection
        if (deckBox != null)
        {
            var deckBoxButton = deckBox.GetComponent<Button>();
            if (deckBoxButton == null)
                deckBoxButton = deckBox.AddComponent<Button>();
            deckBoxButton.onClick.AddListener(ShowDeckSelectContent);
            Debug.Log("[MainMenuDeckController] Setup DeckBox button");
        }
        else
        {
            Debug.LogError("[MainMenuDeckController] DeckBox is null!");
        }
        
        // Setup PlusBox click to create new deck
        if (plusBox != null)
        {
            var plusBoxButton = plusBox.GetComponent<Button>();
            if (plusBoxButton == null)
                plusBoxButton = plusBox.AddComponent<Button>();
            plusBoxButton.onClick.AddListener(CreateNewDeck);
            Debug.Log("[MainMenuDeckController] Setup PlusBox button");
        }
        else
        {
            Debug.LogWarning("[MainMenuDeckController] PlusBox is null - this is normal if you have decks");
        }
        
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideDeckSelectContent);
            Debug.Log("[MainMenuDeckController] Setup close button");
        }
        else
        {
            Debug.LogError("[MainMenuDeckController] Close button is null!");
        }
            
        // Setup background panel click to close
        if (backgroundPanel != null)
        {
            backgroundPanel.onClick.RemoveAllListeners(); // Clear any existing listeners
            backgroundPanel.onClick.AddListener(() => {
                Debug.Log("[MainMenuDeckController] Background panel clicked - closing deck selection");
                HideDeckSelectContent();
            });
            Debug.Log("[MainMenuDeckController] Setup background panel button");
        }
        else
        {
            Debug.LogError("[MainMenuDeckController] Background panel is null! Please assign the Panel inside DeckSelectContent");
        }
        
        // Setup play button
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGameWithSelectedDeck);
            Debug.Log("[MainMenuDeckController] Setup play button");
        }
        else
        {
            Debug.LogError("[MainMenuDeckController] Play button is null!");
        }
    }
    
    #region Public Methods
    
    /// <summary>
    /// Refreshes the main menu deck display based on available decks
    /// </summary>
    public void RefreshDeckDisplay()
    {
        if (DeckManager.Instance == null)
        {
            Debug.LogWarning("[MainMenuDeckController] DeckManager.Instance is null!");
            return;
        }
        
        List<Deck> allDecks = DeckManager.Instance.GetAllDecks();
        
        if (allDecks.Count == 0)
        {
            // No decks - show PlusBox, hide DeckBox
            ShowPlusBox();
            selectedDeckID = "";
        }
        else
        {
            // Has decks - show DeckBox, hide PlusBox
            ShowDeckBox();
            
            // If no deck selected or selected deck doesn't exist, select first deck
            if (string.IsNullOrEmpty(selectedDeckID) || !allDecks.Any(d => d.uniqueID == selectedDeckID))
            {
                selectedDeckID = allDecks[0].uniqueID;
            }
            
            UpdateDeckBoxDisplay();
        }
    }
    
    /// <summary>
    /// Selects a specific deck by ID
    /// </summary>
    /// <param name="deckID">The deck ID to select</param>
    public void SelectDeck(string deckID)
    {
        Debug.Log($"[MainMenuDeckController] SelectDeck called with ID: {deckID}");
        
        if (DeckManager.Instance == null)
        {
            Debug.LogError("[MainMenuDeckController] DeckManager.Instance is null!");
            return;
        }
        
        Deck deck = DeckManager.Instance.GetDeck(deckID);
        if (deck == null)
        {
            Debug.LogError($"[MainMenuDeckController] Deck with ID {deckID} not found!");
            return;
        }
        
        selectedDeckID = deckID;
        Debug.Log($"[MainMenuDeckController] Selected deck: {deck.deckName} (ID: {deckID})");
        
        UpdateDeckBoxDisplay();
        HideDeckSelectContent();
    }
    
    /// <summary>
    /// Gets the currently selected deck ID
    /// </summary>
    /// <returns>The selected deck ID, or empty string if none selected</returns>
    public string GetSelectedDeckID()
    {
        return selectedDeckID;
    }
    
    #endregion
    
    #region UI Management
    
    void ShowDeckBox()
    {
        if (deckBox != null) deckBox.SetActive(true);
        if (plusBox != null) plusBox.SetActive(false);
    }
    
    void ShowPlusBox()
    {
        if (deckBox != null) deckBox.SetActive(false);
        if (plusBox != null) plusBox.SetActive(true);
    }
    
    void UpdateDeckBoxDisplay()
    {
        Debug.Log($"[MainMenuDeckController] UpdateDeckBoxDisplay called with selectedDeckID: {selectedDeckID}");
        
        if (string.IsNullOrEmpty(selectedDeckID) || DeckManager.Instance == null)
        {
            Debug.LogWarning("[MainMenuDeckController] Cannot update deck box: selectedDeckID is empty or DeckManager is null");
            return;
        }
        
        Deck selectedDeck = DeckManager.Instance.GetDeck(selectedDeckID);
        if (selectedDeck == null)
        {
            Debug.LogWarning($"[MainMenuDeckController] Selected deck not found: {selectedDeckID}");
            return;
        }
        
        // Update deck label
        if (deckLabel != null)
        {
            deckLabel.text = selectedDeck.deckName;
            Debug.Log($"[MainMenuDeckController] Updated deck label to: {selectedDeck.deckName}");
        }
        else
        {
            Debug.LogError("[MainMenuDeckController] DeckLabel is null! Cannot update deck name display.");
        }
    }
    
    #endregion
    
    #region Deck Selection Content
    
    void ShowDeckSelectContent()
    {
        if (deckSelectContent != null)
        {
            deckSelectContent.SetActive(true);
            PopulateDeckGrid();
            Debug.Log("[MainMenuDeckController] Showing deck selection content");
        }
    }
    
    void HideDeckSelectContent()
    {
        if (deckSelectContent != null)
        {
            deckSelectContent.SetActive(false);
            ClearDeckGrid();
        }
    }
    
    void PopulateDeckGrid()
    {
        ClearDeckGrid();
        
        if (DeckManager.Instance == null || deckGridContent == null)
        {
            Debug.LogError("[MainMenuDeckController] DeckManager or DeckGridContent is null!");
            return;
        }
        
        List<Deck> allDecks = DeckManager.Instance.GetAllDecks();
        
        foreach (Deck deck in allDecks)
        {
            CreateDeckGridItem(deck);
        }
        
        Debug.Log($"[MainMenuDeckController] Populated {allDecks.Count} decks in grid");
    }
    
    void CreateDeckGridItem(Deck deck)
    {
        if (editMainmenuDeckBoxPrefab == null || deckGridContent == null)
        {
            Debug.LogError("[MainMenuDeckController] EditMainmenuDeckBoxPrefab or DeckGridContent is null!");
            return;
        }
        
        GameObject deckItem = Instantiate(editMainmenuDeckBoxPrefab, deckGridContent);
        
        // If the prefab has a DeckBox component, mark it as controlled by us
        DeckBox prefabDeckBoxComponent = deckItem.GetComponent<DeckBox>();
        if (prefabDeckBoxComponent != null)
        {
            prefabDeckBoxComponent.MarkAsControlledByMainMenuDeckController();
            Debug.Log($"[MainMenuDeckController] Marked DeckBox component on {deckItem.name} as controlled.");
        }
        
        // Setup the deck item components
        SetupDeckGridItem(deckItem, deck);
        
        instantiatedDeckItems.Add(deckItem);
    }
    
    void SetupDeckGridItem(GameObject deckItem, Deck deck)
    {
        Debug.Log($"[MainMenuDeckController] === SETTING UP DECK ITEM: {deck.deckName} ===");
        
        // Find and setup DeckLabel
        var deckLabelComponent = deckItem.GetComponentInChildren<TextMeshProUGUI>();
        if (deckLabelComponent != null)
        {
            deckLabelComponent.text = deck.deckName;
            Debug.Log($"[MainMenuDeckController] Set deck label to: {deck.deckName}");
        }
        else
        {
            Debug.LogWarning($"[MainMenuDeckController] No TextMeshProUGUI found for deck: {deck.deckName}");
        }
        
        // If the deck item has DeckItemUI component, disable it to prevent conflicts
        var deckItemUI = deckItem.GetComponent<DeckItemUI>();
        if (deckItemUI != null)
        {
            deckItemUI.enabled = false;
            Debug.Log($"[MainMenuDeckController] Disabled DeckItemUI component for: {deck.deckName} to prevent conflicts");
        }
        
        // First, find and setup the edit button
        Button editButton = null;
        Button[] allChildButtons = deckItem.GetComponentsInChildren<Button>(true); // Include inactive
        
        Debug.Log($"[MainMenuDeckController] Found {allChildButtons.Length} total buttons in {deckItem.name} for deck: {deck.deckName}");
        
        // Find edit button by name
        foreach (Button btn in allChildButtons)
        {
            string buttonName = btn.name.ToLower();
            string gameObjectName = btn.gameObject.name.ToLower();
            Debug.Log($"[MainMenuDeckController] Checking button: {btn.name} (GameObject: {btn.gameObject.name}, Active: {btn.gameObject.activeSelf}, Interactable: {btn.interactable})");
            
            if (buttonName.Contains("edit") || gameObjectName.Contains("edit"))
            {
                editButton = btn;
                Debug.Log($"[MainMenuDeckController] *** FOUND EDIT BUTTON CANDIDATE: {btn.name} ***");
                // Don't break, find the most specific one if multiple contain "edit"
            }
        }
        
        if (editButton == null)
        {
            Debug.LogError($"[MainMenuDeckController] CRITICAL: NO EDIT BUTTON FOUND for deck: {deck.deckName}! Check prefab {editMainmenuDeckBoxPrefab.name}.");
        }
        else
        {
            Debug.Log($"[MainMenuDeckController] FINAL EDIT BUTTON IS: {editButton.name}");
        }
        
        // Setup the edit button specifically
        if (editButton != null)
        {
            Debug.Log($"[MainMenuDeckController] 🔧 CONFIGURING EDIT BUTTON: {editButton.name}");
            
            editButton.gameObject.SetActive(true);
            editButton.enabled = true;
            editButton.interactable = true;
            
            editButton.onClick.RemoveAllListeners();
            editButton.onClick.AddListener(() => {
                Debug.Log($"[MainMenuDeckController] ⭐⭐⭐ EDIT BUTTON CLICKED ⭐⭐⭐ for deck: {deck.deckName} (ID: {deck.uniqueID})");
                EditDeck(deck.uniqueID);
            });
            
            Image btnImage = editButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = Color.white;
                btnImage.raycastTarget = true;
                editButton.targetGraphic = btnImage;
            }
            
            Debug.Log($"[MainMenuDeckController] ✅ Edit button configured for {deck.deckName}");
        }
        
        // Disable all other buttons except the edit button
        foreach (Button btn in allChildButtons)
        {
            if (btn != editButton)
            {
                Debug.Log($"[MainMenuDeckController] Disabling non-edit button: {btn.name}");
                btn.onClick.RemoveAllListeners();
                btn.interactable = false;
                var btnImage = btn.GetComponent<Image>();
                if (btnImage != null) 
                { 
                    //btnImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); 
                    btnImage.raycastTarget = false; 
                }
            }
        }
        
        // Now setup the main deck item for selection (this should be separate from child buttons)
        Image deckItemImage = deckItem.GetComponent<Image>();
        if (deckItemImage == null) 
        {
            deckItemImage = deckItem.AddComponent<Image>();
            //deckItemImage.color = new Color(1,1,1,0); // Transparent, but raycastable
            Debug.Log($"[MainMenuDeckController] Added transparent Image component to {deckItem.name} for clickability.");
        }
        deckItemImage.raycastTarget = true; 

        Button mainDeckItemButton = deckItem.GetComponent<Button>();
        if (mainDeckItemButton == null)
        {
            mainDeckItemButton = deckItem.AddComponent<Button>();
            Debug.Log($"[MainMenuDeckController] Added Button component to {deckItem.name} for deck selection.");
        }
        
        // Configure the main deck item button for selection
        mainDeckItemButton.interactable = true;
        mainDeckItemButton.onClick.RemoveAllListeners();
        mainDeckItemButton.onClick.AddListener(() => {
            Debug.Log($"[MainMenuDeckController] 🎯 DECK ITEM CLICKED: '{deck.deckName}' - Selecting deck for play.");
            SelectDeck(deck.uniqueID);
        });
        mainDeckItemButton.targetGraphic = deckItemImage;
        
        Debug.Log($"[MainMenuDeckController] ✅ Main deck item '{deckItem.name}' configured for selection of deck '{deck.deckName}'");
        Debug.Log($"[MainMenuDeckController] === FINISHED SETTING UP: {deck.deckName} ===");
    }
    
    void ClearDeckGrid()
    {
        foreach (GameObject item in instantiatedDeckItems)
        {
            if (item != null)
                Destroy(item);
        }
        instantiatedDeckItems.Clear();
    }
    
    #endregion
    
    #region Event Handlers
    
    void CreateNewDeck()
    {
        Debug.Log("[MainMenuDeckController] Creating new deck");
        
        // Hide deck selection if open
        HideDeckSelectContent();
        
        // Switch to Deck scene using MainMenuController
        if (mainMenuController != null)
        {
            mainMenuController.currentScene = MainMenuController.SceneType.Deck;
            mainMenuController.UpdateUI();
            Debug.Log("[MainMenuDeckController] Switched to Deck scene");
        }
        else
        {
            Debug.LogError("[MainMenuDeckController] MainMenuController is null! Please assign it in the inspector.");
        }
        
        // Create new deck via DeckBuilderUI
        if (deckBuilderUI != null)
        {
            deckBuilderUI.CreateNewDeck();
            Debug.Log("[MainMenuDeckController] Called CreateNewDeck on DeckBuilderUI");
        }
        else
        {
            Debug.LogError("[MainMenuDeckController] DeckBuilderUI is null! Please assign it in the inspector.");
        }
    }
    
    void EditDeck(string deckID)
    {
        Debug.Log($"[MainMenuDeckController] *** EDIT DECK CALLED *** with ID: {deckID}");
        
        if (string.IsNullOrEmpty(deckID))
        {
            Debug.LogError("[MainMenuDeckController] EditDeck: deckID is null or empty!");
            return;
        }
        
        if (DeckManager.Instance == null)
        {
            Debug.LogError("[MainMenuDeckController] EditDeck: DeckManager.Instance is null!");
            return;
        }
        
        Deck deck = DeckManager.Instance.GetDeck(deckID);
        if (deck == null)
        {
            Debug.LogError($"[MainMenuDeckController] EditDeck: Deck with ID {deckID} not found!");
            return;
        }
        string deckName = deck.deckName;
        Debug.Log($"[MainMenuDeckController] Found deck to edit: {deckName}");
        
        // Hide deck selection popup
        Debug.Log("[MainMenuDeckController] Hiding deck selection popup...");
        HideDeckSelectContent();
        
        // Check if deckBuilderUI is assigned
        if (deckBuilderUI == null)
        {
            Debug.LogError("[MainMenuDeckController] DeckBuilderUI is null! Please assign it in the inspector.");
            return;
        }

        // Switch to Deck scene using MainMenuController
        if (mainMenuController != null)
        {
            Debug.Log("[MainMenuDeckController] Switching to Deck scene (which will activate DeckBuilderUI)...");
            mainMenuController.currentScene = MainMenuController.SceneType.Deck;
            mainMenuController.UpdateUI(); // This activates deckScreenContent
            Debug.Log("[MainMenuDeckController] Switched to Deck scene.");
        }
        else
        {
            Debug.LogError("[MainMenuDeckController] MainMenuController is null! Cannot switch scenes.");
            return;
        }

        // Call DeckBuilderUI.ShowDeckEditingView after a frame delay to allow UI to initialize
        StartCoroutine(ShowDeckEditingViewAfterFrame(deckID, deckName));
    }

    IEnumerator ShowDeckEditingViewAfterFrame(string deckID, string deckName)
    {
        yield return new WaitForEndOfFrame(); // Wait for UI to activate and layout

        if (deckBuilderUI != null)
        {
            Debug.Log($"[MainMenuDeckController] Coroutine: Calling deckBuilderUI.ShowDeckEditingView for deck: {deckName} (ID: {deckID})");
            deckBuilderUI.ShowDeckEditingView(deckID, deckName);
            Debug.Log("[MainMenuDeckController] Coroutine: deckBuilderUI.ShowDeckEditingView call completed.");
        }
        else
        {
            Debug.LogError("[MainMenuDeckController] Coroutine: DeckBuilderUI became null before ShowDeckEditingViewAfterFrame could execute!");
        }
    }
    
    void StartGameWithSelectedDeck()
    {
        if (string.IsNullOrEmpty(selectedDeckID))
        {
            Debug.LogWarning("[MainMenuDeckController] No deck selected for play!");
            return;
        }
        
        Debug.Log($"[MainMenuDeckController] Starting game with deck: {selectedDeckID}");
        
        // Implement your game start logic here
        // For example:
        // GameManager.Instance.StartGame(selectedDeckID);
        // SceneManager.LoadScene("GameScene");
    }
    
    #endregion
    
    #region Callbacks for DeckBuilder
    
    /// <summary>
    /// Call this when a deck is saved/created in the deck builder
    /// </summary>
    /// <param name="deckID">The ID of the saved deck</param>
    public void OnDeckSaved(string deckID)
    {
        RefreshDeckDisplay();
        
        // If this was a new deck and we had no decks before, select it
        if (string.IsNullOrEmpty(selectedDeckID))
        {
            selectedDeckID = deckID;
            UpdateDeckBoxDisplay();
        }
    }
    
    /// <summary>
    /// Call this when a deck is deleted
    /// </summary>
    /// <param name="deckID">The ID of the deleted deck</param>
    public void OnDeckDeleted(string deckID)
    {
        // If the deleted deck was selected, clear selection
        if (selectedDeckID == deckID)
        {
            selectedDeckID = "";
        }
        
        RefreshDeckDisplay();
    }
    
    #endregion

    // Helper function to get full path of a GameObject for debugging
    string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }
} 