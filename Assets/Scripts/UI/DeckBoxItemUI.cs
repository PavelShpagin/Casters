using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for hover events
using System;

public class DeckBoxItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Text deckNameText;       // Assign Text object under NameLabelPanel
    public Button mainButton;       // Assign the root Button component of the prefab
    public Button deleteButton;     // Assign DeleteButton_MyDecks (child object)
    public GameObject plusSignText;   // Assign PlusSignText (child object, only relevant for Add button)
    public GameObject nameLabelPanel; // Assign NameLabelPanel (child object)

    private string currentDeckID;
    private string currentDeckName;
    private bool isAddButton = false;

    // Actions to call back to the main MyDecksUI manager
    private Action<string> onSelectAction; 
    private Action<string, string> onDeleteAction;
    private Action onAddAction;

    public void SetupDeckBox(Deck deck, Action<string> selectAction, Action<string, string> deleteAction)
    {
        isAddButton = false;
        currentDeckID = deck.uniqueID;
        currentDeckName = deck.deckName;
        onSelectAction = selectAction;
        onDeleteAction = deleteAction;

        if (deckNameText != null) deckNameText.text = currentDeckName;
        if (plusSignText != null) plusSignText.SetActive(false); // Hide plus sign
        if (nameLabelPanel != null) nameLabelPanel.SetActive(true);
        if (deleteButton != null) deleteButton.gameObject.SetActive(false); // Start with delete hidden

        // Configure buttons
        if (mainButton != null) 
        {
            mainButton.onClick.RemoveAllListeners(); // Clear previous listeners
            mainButton.onClick.AddListener(() => onSelectAction?.Invoke(currentDeckID));
        }
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => onDeleteAction?.Invoke(currentDeckID, currentDeckName));
        }
    }

    public void SetupAsAddButton(Action addAction)
    {
        isAddButton = true;
        onAddAction = addAction;
        currentDeckID = null;
        currentDeckName = "Add Deck";

        if (deckNameText != null) deckNameText.text = ""; // Clear name text
        if (plusSignText != null) plusSignText.SetActive(true); // Show plus sign
        if (nameLabelPanel != null) nameLabelPanel.SetActive(false); // Hide name panel
        if (deleteButton != null) deleteButton.gameObject.SetActive(false); // Hide delete button

        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => onAddAction?.Invoke());
        }
    }

    // Show delete button on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isAddButton && deleteButton != null)
        {
            deleteButton.gameObject.SetActive(true);
        }
    }

    // Hide delete button when not hovering
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isAddButton && deleteButton != null)
        {
            deleteButton.gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        // Ensure delete button is hidden if the object is disabled while hovered
        if (!isAddButton && deleteButton != null && deleteButton.gameObject.activeSelf)
        {
            deleteButton.gameObject.SetActive(false);
        }
    }
} 