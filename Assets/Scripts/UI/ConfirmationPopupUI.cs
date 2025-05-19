using UnityEngine;
using UnityEngine.UI;
using System;

public class ConfirmationPopupUI : MonoBehaviour
{
    // Basic placeholder - Does nothing functional yet
    // TODO: Implement full popup logic with references to Text, Yes/No buttons etc.

    public GameObject popupPanel; // Assign the main panel of your popup UI
    public Text messageText;      // Assign the Text component for the message
    public Button yesButton;
    public Button noButton;

    private Action onConfirmAction;
    private Action onCancelAction;

    void Start()
    {
        // Ensure popup is hidden initially
        if(popupPanel != null) popupPanel.SetActive(false);

        // Hook up placeholder listeners 
        if(yesButton != null) yesButton.onClick.AddListener(Confirm);
        if(noButton != null) noButton.onClick.AddListener(Cancel);
    }

    public void ShowPopup(string message, Action onConfirm, Action onCancel = null)
    {
        Debug.Log($"Showing Popup (Not fully implemented): {message}");
        /* // Full implementation would look like:
        messageText.text = message;
        onConfirmAction = onConfirm;
        onCancelAction = onCancel;
        popupPanel.SetActive(true);
        */

         // --- TEMPORARY: Directly confirm for now ---
         if (onConfirm != null) onConfirm();
         // --- END TEMPORARY ---
    }

    void Confirm()
    {
        // onConfirmAction?.Invoke();
        // ClosePopup();
    }

    void Cancel()
    {
       // onCancelAction?.Invoke();
       // ClosePopup();
    }

    void ClosePopup()
    {
       // popupPanel.SetActive(false);
       // onConfirmAction = null;
       // onCancelAction = null;
    }
} 