using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Required for List
using System.Linq;

public class MyDecksUI : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject myDecksCanvas;    // Assign MyDecksCanvas itself
    public GameObject mainMenuCanvas;   // Assign MainMenuCanvas
    public GameObject deckBuilderCanvas;// Assign DeckBuilderCanvas

    [Header("UI Elements")]
    public Button backButton;       // Assign BackButton_MyDecks
    public Transform deckGridContent; // Assign Content object under DecksGridScrollView_MyDecks > Viewport
    public GameObject deckBoxPrefab;  // Assign the DeckBoxItemPrefab you created
    // Optional: Confirmation Popup reference
    // public ConfirmationPopupUI confirmationPopup;

    private Button addDeckButtonInstance; // Reference to the instantiated Add button

    void Start()
    {
        if (!ValidateReferences())
        {
            this.enabled = false;
            return;
        }
        backButton.onClick.AddListener(GoToMainMenu);
        // Initial refresh if canvas starts active
        if (myDecksCanvas.activeSelf)
        {
            RefreshDisplay();
        }
    }

    void OnEnable()
    {
        // This is a good place to refresh because it's called every time the object becomes active
        RefreshDisplay();
    }

    bool ValidateReferences()
    {
        if (myDecksCanvas == null || mainMenuCanvas == null || deckBuilderCanvas == null)
        {
            Debug.LogError("MyDecksUI: One or more Canvas references are missing!");
            return false;
        }
        if (backButton == null || deckGridContent == null || deckBoxPrefab == null)
        {
            Debug.LogError("MyDecksUI: Button, Grid Content, or Prefab references are missing!");
            return false;
        }
        if (DeckManager.Instance == null)
        {
             Debug.LogError("MyDecksUI: DeckManager.Instance not found!");
            return false;
        }
        return true;
    }

    public void RefreshDisplay()
    {
        if (DeckManager.Instance == null) return; // Guard clause

        // Clear existing deck boxes (except the persisted Add button)
        for (int i = deckGridContent.childCount - 1; i >= 0; i--)
        {
            Transform child = deckGridContent.GetChild(i);
            if (addDeckButtonInstance != null && child == addDeckButtonInstance.transform)
                continue; 
            Destroy(child.gameObject);
        }

        // Instantiate Add Deck Button (if not already done or if destroyed somehow)
        if (addDeckButtonInstance == null)
        {
            GameObject addBoxGO = Instantiate(deckBoxPrefab, deckGridContent);
            addDeckButtonInstance = addBoxGO.GetComponent<Button>(); // Get the button component
            DeckBoxItemUI addBoxUI = addBoxGO.GetComponent<DeckBoxItemUI>();
            if (addBoxUI != null) 
            {
                addBoxUI.SetupAsAddButton(OnAddDeckClicked);
            }
            else
            {
                 Debug.LogError("DeckBoxItemPrefab is missing DeckBoxItemUI script!");
                 if(addBoxGO != null) Destroy(addBoxGO); // Clean up
                 return;
            }
        }
         // Ensure Add button is always first for consistent UI
        if(addDeckButtonInstance != null) addDeckButtonInstance.transform.SetAsFirstSibling();


        List<Deck> userDecks = DeckManager.Instance.currentUserDecks.allDecks;
        foreach (Deck deck in userDecks.OrderBy(d => d.deckName)) // Display alphabetically
        {
            GameObject deckBoxGO = Instantiate(deckBoxPrefab, deckGridContent);
            DeckBoxItemUI deckBoxUI = deckBoxGO.GetComponent<DeckBoxItemUI>();
            if (deckBoxUI != null)
            {
                deckBoxUI.SetupDeckBox(deck, OnDeckSelected, OnDeleteDeckClicked);
            }
        }
    }

    void OnAddDeckClicked()
    {        
        Deck newDeck = DeckManager.Instance.CreateNewDeck(); 
        if (newDeck != null)
        {            
            GoToDeckBuilder(newDeck.uniqueID); // Go to edit the new deck immediately
        }
        else
        {
            Debug.LogError("Failed to create a new deck.");
            // Potentially show an error to the user
        }
        // RefreshDisplay(); // No need, GoToDeckBuilder will hide this canvas. Refresh happens on OnEnable.
    }

    void OnDeckSelected(string deckID)
    {
        Debug.Log($"Deck selected: {deckID}");
        GoToDeckBuilder(deckID);
    }

    void OnDeleteDeckClicked(string deckID, string deckName)
    {
        Debug.Log($"Request delete for deck: {deckName} ({deckID})");
        
        bool confirmDelete = true; // Default to direct delete
        #if UNITY_EDITOR
        confirmDelete = UnityEditor.EditorUtility.DisplayDialog(
            "Delete Deck",
            $"Are you sure you want to delete the deck '{deckName}'?",
            "Delete", "Cancel"
        );
        #endif
        // TODO: Replace with your ConfirmationPopupUI.ShowPopup($"Delete '{deckName}'?", () => ConfirmDeleteDeck(deckID));
        
        if (confirmDelete)
        {
            ConfirmDeleteDeck(deckID);
        }
    }

    void ConfirmDeleteDeck(string deckID)
    {
        bool deleted = DeckManager.Instance.DeleteDeck(deckID);
        if (deleted)
        {
            DeckManager.Instance.SaveDecks(); 
            RefreshDisplay(); 
        }
    }

    void GoToMainMenu()
    {
        myDecksCanvas.SetActive(false);
        // deckBuilderCanvas.SetActive(false); // Should already be false if coming from MyDecks
        mainMenuCanvas.SetActive(true);
    }

    void GoToDeckBuilder(string deckID)
    {
        myDecksCanvas.SetActive(false);
        // mainMenuCanvas.SetActive(false); // Should already be false
        
        DeckBuilderUI builderScript = deckBuilderCanvas.GetComponent<DeckBuilderUI>();
        if (builderScript != null)
        {
            deckBuilderCanvas.SetActive(true); // Activate before loading
            Deck selectedDeck = DeckManager.Instance.GetDeck(deckID);
            if (selectedDeck != null)
            {
                builderScript.DisplayDeckFromExternal(selectedDeck.uniqueID, selectedDeck.deckName);
            }
            else
            {
                Debug.LogError("Could not find the selected deck in DeckManager!");
                myDecksCanvas.SetActive(true); // Go back to safety
            }
        }
        else
        {
            Debug.LogError("Could not find DeckBuilderUI script on DeckBuilderCanvas! Cannot open deck editor.");
            myDecksCanvas.SetActive(true); // Go back to safety
        }
    }
} 