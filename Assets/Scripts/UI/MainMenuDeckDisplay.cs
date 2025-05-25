using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MainMenuDeckDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Transform deckContainer; // Container for main menu deck boxes
    public GameObject mainMenuDeckBoxPrefab; // MainMenuDeckBox_Prefab
    public Button playButton; // The main PLAY button
    
    private List<GameObject> instantiatedDeckBoxes = new List<GameObject>();
    private string selectedDeckID = "";
    
    void Start()
    {
        // DISABLED: This conflicts with MainMenuDeckController
        // Use MainMenuDeckController instead for deck management
        /*
        PopulateMainMenuDecks();
        
        // Set up play button
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        
        // Initialize popup if needed
        if (DeckSelectionPopup.Instance == null)
        {
            CreateDeckSelectionPopup();
        }
        */
        
        Debug.Log("[MainMenuDeckDisplay] DISABLED - Using MainMenuDeckController instead");
    }
    
    void CreateDeckSelectionPopup()
    {
        // Find the main canvas
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("[MainMenuDeckDisplay] No Canvas found in scene!");
            return;
        }
        
        GameObject popupGO = new GameObject("DeckSelectionPopup");
        popupGO.transform.SetParent(mainCanvas.transform, false);
        
        // Set up RectTransform to fill the canvas
        RectTransform popupRect = popupGO.AddComponent<RectTransform>();
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.offsetMin = Vector2.zero;
        popupRect.offsetMax = Vector2.zero;
        
        // Add the DeckSelectionPopup component
        popupGO.AddComponent<DeckSelectionPopup>();
        
        Debug.Log("[MainMenuDeckDisplay] Created DeckSelectionPopup for main menu");
    }
    
    void PopulateMainMenuDecks()
    {
        if (DeckManager.Instance == null || deckContainer == null)
        {
            Debug.LogError("[MainMenuDeckDisplay] DeckManager or deck container is null!");
            return;
        }
        
        // Clear existing deck boxes
        ClearDeckBoxes();
        
        List<Deck> allDecks = DeckManager.Instance.GetAllDecks();
        
        // Get currently selected deck
        selectedDeckID = DeckManager.Instance.GetCurrentSelectedDeckID();
        
        // Show first few decks in main menu (limit to 3-5)
        int maxDecks = Mathf.Min(5, allDecks.Count);
        
        for (int i = 0; i < maxDecks; i++)
        {
            CreateMainMenuDeckBox(allDecks[i]);
        }
        
        Debug.Log($"[MainMenuDeckDisplay] Populated {maxDecks} deck boxes in main menu");
    }
    
    void CreateMainMenuDeckBox(Deck deck)
    {
        if (mainMenuDeckBoxPrefab == null)
        {
            Debug.LogError("[MainMenuDeckDisplay] MainMenuDeckBoxPrefab is not assigned!");
            return;
        }
        
        GameObject deckBoxGO = Instantiate(mainMenuDeckBoxPrefab, deckContainer);
        deckBoxGO.SetActive(true);
        
        // Set up DeckBox component
        DeckBox deckBox = deckBoxGO.GetComponent<DeckBox>();
        if (deckBox != null)
        {
            deckBox.SetDeckData(deck.uniqueID, deck.deckName);
            Debug.Log($"[MainMenuDeckDisplay] Created main menu deck box for {deck.deckName}");
        }
        else
        {
            Debug.LogError($"[MainMenuDeckDisplay] MainMenuDeckBoxPrefab doesn't have DeckBox component!");
        }
        
        instantiatedDeckBoxes.Add(deckBoxGO);
    }
    
    void ClearDeckBoxes()
    {
        foreach (GameObject deckBox in instantiatedDeckBoxes)
        {
            if (deckBox != null)
                Destroy(deckBox);
        }
        instantiatedDeckBoxes.Clear();
    }
    
    void OnPlayButtonClicked()
    {
        Debug.Log("[MainMenuDeckDisplay] Play button clicked");
        
        // Check if a deck is selected
        if (string.IsNullOrEmpty(selectedDeckID))
        {
            Debug.Log("[MainMenuDeckDisplay] No deck selected, opening deck selection popup");
            
            // Open deck selection popup if no deck selected
            if (DeckSelectionPopup.Instance != null)
            {
                DeckSelectionPopup.Instance.ShowPopup(
                    onDeckSelected: OnDeckSelectedForPlay,
                    onDeckEdit: OnDeckEditFromPlay
                );
            }
        }
        else
        {
            Debug.Log($"[MainMenuDeckDisplay] Starting game with selected deck: {selectedDeckID}");
            // Start game with selected deck
            StartGameWithDeck(selectedDeckID);
        }
    }
    
    void OnDeckSelectedForPlay(string deckID)
    {
        Debug.Log($"[MainMenuDeckDisplay] Deck selected for play: {deckID}");
        
        // Set as current deck
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.SetCurrentSelectedDeck(deckID);
            selectedDeckID = deckID;
        }
        
        // Start game
        StartGameWithDeck(deckID);
    }
    
    void OnDeckEditFromPlay(string deckID)
    {
        Debug.Log($"[MainMenuDeckDisplay] Opening deck editor from main menu: {deckID}");
        
        // Find DeckBuilderUI and open deck
        DeckBuilderUI deckBuilder = FindFirstObjectByType<DeckBuilderUI>();
        if (deckBuilder != null)
        {
            Deck deck = DeckManager.Instance?.GetDeck(deckID);
            if (deck != null)
            {
                deckBuilder.DisplayDeckFromExternal(deckID, deck.deckName);
            }
        }
        else
        {
            Debug.LogError("[MainMenuDeckDisplay] Could not find DeckBuilderUI!");
        }
    }
    
    void StartGameWithDeck(string deckID)
    {
        Debug.Log($"[MainMenuDeckDisplay] Starting game with deck: {deckID}");
        
        // Set the active gameplay deck
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.SetActiveGameplayDeck(deckID);
        }
        
        // TODO: Load game scene or start gameplay
        // SceneManager.LoadScene("GameplayScene");
    }
    
    public void RefreshDeckDisplay()
    {
        PopulateMainMenuDecks();
    }
} 