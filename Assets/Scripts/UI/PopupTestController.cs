using UnityEngine;

public class PopupTestController : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode testPopupKey = KeyCode.P;
    [SerializeField] private KeyCode hidePopupKey = KeyCode.Escape;
    
    [Header("Test Callbacks")]
    [SerializeField] private bool logCallbacks = true;
    
    void Update()
    {
        // Press P to test the popup
        if (Input.GetKeyDown(testPopupKey))
        {
            ShowDeckSelectionPopup();
        }
        
        // Press Escape to hide popup
        if (Input.GetKeyDown(hidePopupKey))
        {
            HideDeckSelectionPopup();
        }
    }
    
    void OnEnable()
    {
        // Subscribe to popup events
        DeckSelectionPopupController.OnDeckSelected += OnDeckSelected;
        DeckSelectionPopupController.OnDeckEditRequested += OnDeckEditRequested;
        DeckSelectionPopupController.OnPopupClosed += OnPopupClosed;
    }
    
    void OnDisable()
    {
        // Unsubscribe from popup events
        DeckSelectionPopupController.OnDeckSelected -= OnDeckSelected;
        DeckSelectionPopupController.OnDeckEditRequested -= OnDeckEditRequested;
        DeckSelectionPopupController.OnPopupClosed -= OnPopupClosed;
    }
    
    public void ShowDeckSelectionPopup()
    {
        if (DeckSelectionPopupController.Instance != null)
        {
            DeckSelectionPopupController.Instance.ShowPopup(
                onDeckSelected: OnDeckSelectedForPlay,
                onDeckEdit: OnDeckSelectedForEdit
            );
            
            if (logCallbacks)
                Debug.Log("[PopupTestController] Showing deck selection popup");
        }
        else
        {
            Debug.LogError("[PopupTestController] DeckSelectionPopupController.Instance is null!");
        }
    }
    
    public void HideDeckSelectionPopup()
    {
        if (DeckSelectionPopupController.Instance != null)
        {
            DeckSelectionPopupController.Instance.HidePopup();
            
            if (logCallbacks)
                Debug.Log("[PopupTestController] Hiding deck selection popup");
        }
    }
    
    #region Event Callbacks
    
    private void OnDeckSelectedForPlay(string deckID)
    {
        if (logCallbacks)
            Debug.Log($"[PopupTestController] Deck selected for play: {deckID}");
        
        // Here you would start the game with the selected deck
        StartGameWithDeck(deckID);
    }
    
    private void OnDeckSelectedForEdit(string deckID)
    {
        if (logCallbacks)
            Debug.Log($"[PopupTestController] Deck selected for edit: {deckID}");
        
        // Here you would open the deck editor
        // Note: The popup controller already handles opening the edit panel
    }
    
    private void OnDeckSelected(string deckID)
    {
        if (logCallbacks)
            Debug.Log($"[PopupTestController] Deck selected (global event): {deckID}");
    }
    
    private void OnDeckEditRequested(string deckID)
    {
        if (logCallbacks)
            Debug.Log($"[PopupTestController] Deck edit requested (global event): {deckID}");
    }
    
    private void OnPopupClosed()
    {
        if (logCallbacks)
            Debug.Log("[PopupTestController] Popup closed (global event)");
    }
    
    #endregion
    
    #region Game Logic
    
    private void StartGameWithDeck(string deckID)
    {
        // Implement your game start logic here
        Debug.Log($"[PopupTestController] Starting game with deck: {deckID}");
        
        // Example: Load game scene with selected deck
        // SceneManager.LoadScene("GameScene");
        // GameManager.Instance.SetPlayerDeck(deckID);
    }
    
    #endregion
} 