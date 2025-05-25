using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class DeckSelectionPopupController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupCanvas;
    [SerializeField] private GameObject deckItemPrefab;
    [SerializeField] private Transform deckContainer;
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect scrollRect;
    
    [Header("Buttons")]
    [SerializeField] private Button playButtonPrefab;
    [SerializeField] private Button editButtonPrefab;
    
    [Header("Panel References")]
    [SerializeField] private GameObject deckEditPanel; // Reference to deck builder panel
    
    [Header("Settings")]
    [SerializeField] private bool closeOnSelection = true;
    [SerializeField] private bool showEditButtons = true;
    [SerializeField] private bool showPlayButtons = true;
    
    // Events
    public static event Action<string> OnDeckSelected;
    public static event Action<string> OnDeckEditRequested;
    public static event Action OnPopupClosed;
    
    // Internal
    private List<GameObject> instantiatedDeckItems = new List<GameObject>();
    private Action<string> currentDeckSelectedCallback;
    private Action<string> currentDeckEditCallback;
    
    public static DeckSelectionPopupController Instance { get; private set; }
    
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
        HidePopup();
    }
    
    void SetupUI()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(HidePopup);
            
        if (popupCanvas == null)
        {
            Debug.LogError("[DeckSelectionPopup] PopupCanvas is not assigned!");
        }
    }
    
    #region Public Methods
    
    /// <summary>
    /// Shows the deck selection popup
    /// </summary>
    /// <param name="onDeckSelected">Callback when a deck is selected for play</param>
    /// <param name="onDeckEdit">Callback when a deck is selected for editing</param>
    public void ShowPopup(Action<string> onDeckSelected = null, Action<string> onDeckEdit = null)
    {
        currentDeckSelectedCallback = onDeckSelected;
        currentDeckEditCallback = onDeckEdit;
        
        if (popupCanvas != null)
        {
            popupCanvas.SetActive(true);
            PopulateDecks();
            Debug.Log("[DeckSelectionPopup] Popup shown");
        }
    }
    
    /// <summary>
    /// Hides the deck selection popup
    /// </summary>
    public void HidePopup()
    {
        if (popupCanvas != null)
        {
            popupCanvas.SetActive(false);
            ClearDeckItems();
            OnPopupClosed?.Invoke();
        }
    }
    
    /// <summary>
    /// Opens the deck edit panel for the specified deck
    /// </summary>
    /// <param name="deckID">The unique ID of the deck to edit</param>
    public void OpenDeckEditPanel(string deckID)
    {
        if (deckEditPanel != null)
        {
            // Hide popup and show deck edit panel
            HidePopup();
            deckEditPanel.SetActive(true);
            
            // If you have a DeckBuilderUI component, load the deck
            var deckBuilder = deckEditPanel.GetComponent<DeckBuilderUI>();
            if (deckBuilder != null)
            {
                deckBuilder.LoadDeckForEditing(deckID);
            }
        }
        
        // Call the edit callback
        currentDeckEditCallback?.Invoke(deckID);
        OnDeckEditRequested?.Invoke(deckID);
    }
    
    #endregion
    
    #region Deck Population
    
    void PopulateDecks()
    {
        ClearDeckItems();
        
        if (DeckManager.Instance == null)
        {
            Debug.LogError("[DeckSelectionPopup] DeckManager.Instance is null!");
            return;
        }
        
        List<Deck> allDecks = DeckManager.Instance.GetAllDecks();
        
        foreach (Deck deck in allDecks)
        {
            CreateDeckItem(deck);
        }
        
        Debug.Log($"[DeckSelectionPopup] Populated {allDecks.Count} decks");
    }
    
    void CreateDeckItem(Deck deck)
    {
        if (deckItemPrefab == null || deckContainer == null)
        {
            Debug.LogError("[DeckSelectionPopup] DeckItemPrefab or DeckContainer is not assigned!");
            return;
        }
        
        GameObject deckItem = Instantiate(deckItemPrefab, deckContainer);
        
        // Configure the deck item
        var deckItemUI = deckItem.GetComponent<DeckItemUI>();
        if (deckItemUI != null)
        {
            deckItemUI.SetupDeckItem(deck, OnDeckPlayClicked, OnDeckEditClicked);
        }
        else
        {
            // Fallback: try to find components manually
            SetupDeckItemFallback(deckItem, deck);
        }
        
        instantiatedDeckItems.Add(deckItem);
    }
    
    void SetupDeckItemFallback(GameObject deckItem, Deck deck)
    {
        // Try to find and setup text components
        var nameText = deckItem.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = deck.deckName;
        }
        
        // Setup buttons based on settings
        var buttons = deckItem.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            if (button.name.ToLower().Contains("play") && showPlayButtons)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnDeckPlayClicked(deck.uniqueID));
                button.gameObject.SetActive(true);
            }
            else if (button.name.ToLower().Contains("edit") && showEditButtons)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnDeckEditClicked(deck.uniqueID));
                button.gameObject.SetActive(true);
            }
            else if (button.name.ToLower().Contains("play") && !showPlayButtons)
            {
                button.gameObject.SetActive(false);
            }
            else if (button.name.ToLower().Contains("edit") && !showEditButtons)
            {
                button.gameObject.SetActive(false);
            }
        }
    }
    
    void ClearDeckItems()
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
    
    void OnDeckPlayClicked(string deckID)
    {
        Debug.Log($"[DeckSelectionPopup] Play deck: {deckID}");
        
        currentDeckSelectedCallback?.Invoke(deckID);
        OnDeckSelected?.Invoke(deckID);
        
        if (closeOnSelection)
        {
            HidePopup();
        }
    }
    
    void OnDeckEditClicked(string deckID)
    {
        Debug.Log($"[DeckSelectionPopup] Edit deck: {deckID}");
        OpenDeckEditPanel(deckID);
    }
    
    #endregion
} 