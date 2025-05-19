using UnityEngine;
using UnityEngine.UI; // Required for Button

public class MainMenuUI : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mainMenuCanvas;   // Assign the MainMenuCanvas itself
    public GameObject myDecksCanvas;    // Assign the MyDecksCanvas
    public GameObject deckBuilderCanvas; // Assign the DeckBuilderCanvas

    [Header("Main Menu Buttons")]
    public Button deckButton;         // Assign DeckButton_MainMenu
    public Button playButton;         // Assign PlayButton_MainMenu (Optional: Add functionality later)
    public Button soloButton;         // Assign SoloButton_MainMenu (Optional: Add functionality later)
    public Button shopButton;         // Assign ShopButton_MainMenu (Optional: Add functionality later)

    void Start()
    {
        if (!ValidateReferences())
        {
            this.enabled = false; // Disable script if references are missing
            return;
        }

        // Add listeners
        deckButton.onClick.AddListener(GoToMyDecks);
        // Add listeners for Play, Solo, Shop if needed
        // playButton.onClick.AddListener(OnPlayClicked); 

        // Initial state: Show main menu, hide others
        ShowMainMenu();
    }

    bool ValidateReferences()
    {
        if (mainMenuCanvas == null || myDecksCanvas == null || deckBuilderCanvas == null)
        {
            Debug.LogError("MainMenuUI: One or more Canvas references are missing!");
            return false;
        }
        if (deckButton == null) // Add checks for other required buttons
        {
            Debug.LogError("MainMenuUI: DeckButton reference is missing!");
            return false;
        }
        return true;
    }

    void GoToMyDecks()
    {        
        mainMenuCanvas.SetActive(false);
        deckBuilderCanvas.SetActive(false);
        myDecksCanvas.SetActive(true);
        // Optionally tell MyDecksUI to refresh its display
        MyDecksUI myDecksUIScript = myDecksCanvas.GetComponent<MyDecksUI>();
        if (myDecksUIScript != null) {
             myDecksUIScript.RefreshDisplay();
        }
    }

    public void ShowMainMenu()
    {
        deckBuilderCanvas.SetActive(false);
        myDecksCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }

    // Placeholder for other button actions
    // void OnPlayClicked() { Debug.Log("Play button clicked - Not implemented"); }
} 