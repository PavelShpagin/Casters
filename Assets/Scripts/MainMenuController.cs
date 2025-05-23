using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    public enum SceneType { Play, Deck, Solo, Shop }
    public SceneType currentScene = SceneType.Play;

    [Header("UI References")]
    public GameObject topBar;
    public GameObject line;
    public List<Button> menuButtons;
    public List<GameObject> underlineImages; // Underline images for each button

    [Header("Scene Content Panels")]
    public GameObject playScreenContent;      // Assign your MainMenuContent GameObject here
    public GameObject deckScreenContent;      // Assign your DeckBuilderContent GameObject here
    public List<GameObject> panels;           // For additional panels like Solo (index 2), Shop (index 3)

    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 1f, 1f, 0.8f);
    public Color activeColor = new Color(1f, 1f, 1f, 1f);

    [Header("Background Settings")]
    public Image backgroundToFitImage;   // Assign the Image component of "Background (1)"

    void Start()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            int idx = i; // Local copy for the closure
            if (menuButtons[i] != null)
            {
                var trigger = menuButtons[i].gameObject.AddComponent<EventTrigger>();
                AddEvent(trigger, EventTriggerType.PointerEnter, () => OnButtonHover(idx));
                AddEvent(trigger, EventTriggerType.PointerExit, () => OnButtonExit(idx));
                menuButtons[i].onClick.AddListener(() => OnMenuButtonClicked(idx));
            }
            else
            {
                Debug.LogWarning($"MainMenuController: Menu button at index {idx} is null in Start(). Skipping listener setup.");
            }
        }
        UpdateUI();
        StartCoroutine(FitImageToParentAfterLayout());
    }

    void AddEvent(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((eventData) => action());
        trigger.triggers.Add(entry);
    }

    void OnButtonHover(int idx)
    {
        if (idx < 0 || idx >= menuButtons.Count || menuButtons[idx] == null) return;
        if ((int)currentScene != idx)
        {
            Image btnImage = menuButtons[idx].GetComponent<Image>();
            if (btnImage != null) btnImage.color = hoverColor;
        }
    }

    void OnButtonExit(int idx)
    {
        if (idx < 0 || idx >= menuButtons.Count || menuButtons[idx] == null) return;
        if ((int)currentScene != idx)
        {
            Image btnImage = menuButtons[idx].GetComponent<Image>();
            if (btnImage != null) btnImage.color = normalColor;
        }
    }

    void OnMenuButtonClicked(int idx)
    {
        if (idx < 0 || idx >= System.Enum.GetValues(typeof(SceneType)).Length)
        {
            Debug.LogWarning($"MainMenuController: Invalid scene index {idx} clicked.");
            return;
        }
        currentScene = (SceneType)idx;
        UpdateUI();
    }

    void UpdateUI()
    {
        // Explicitly handle Play and Deck screens
        if (playScreenContent != null)
        {
            playScreenContent.SetActive(currentScene == SceneType.Play);
        }
        else if (currentScene == SceneType.Play)
        {
            Debug.LogWarning("MainMenuController: playScreenContent is not assigned!");
        }

        if (deckScreenContent != null)
        {
            deckScreenContent.SetActive(currentScene == SceneType.Deck);
        }
        else if (currentScene == SceneType.Deck)
        {
            Debug.LogWarning("MainMenuController: deckScreenContent is not assigned!");
        }

        // Handle other panels from the list (e.g., Solo, Shop)
        // Start from index 2 as Play (0) and Deck (1) are handled above
        for (int i = 2; i < System.Enum.GetValues(typeof(SceneType)).Length; i++)
        {
            bool isActivePanel = (i == (int)currentScene);
            // Adjust index for the 'panels' list if it's used for scenes beyond Play/Deck
            // E.g. if Solo is SceneType index 2, it might be panels[0] if panels only holds Solo, Shop etc.
            // For now, let's assume panels list aligns directly if populated for SceneType indices 2+
            int panelListIndex = i; // This assumes panels[0] is for Play, panels[1] for Deck etc. if those specific fields weren't used.
                                  // Or more robustly, if panels list is *only* for scenes *after* Play and Deck:
            // int panelListIndex = i - 2; // if panels[0] is Solo, panels[1] is Shop etc.

            // Let's make it simple: if panels list is used, it must correspond to SceneType index directly
            // but we skip 0 and 1 as they are handled by specific fields.
            if (i < panels.Count && panels[i] != null) // Check if a panel is assigned for this SceneType index
            {
                 panels[i].SetActive(isActivePanel);
            }
            else if (isActivePanel)
            {
                 Debug.LogWarning($"MainMenuController: Panel for scene {(SceneType)i} (index {i}) is not assigned in the 'panels' list (or specific field), or the list is too short.");
            }
        }

        // Update button visuals (same as before)
        for (int i = 0; i < menuButtons.Count; i++)
        {
            if (menuButtons[i] == null)
            {
                Debug.LogWarning($"MainMenuController: Menu button at index {i} is null.");
                continue;
            }

            Image btnImage = menuButtons[i].GetComponent<Image>();
            if (btnImage == null)
            {
                Debug.LogWarning($"MainMenuController: Menu button '{menuButtons[i].name}' at index {i} is missing an Image component.");
                continue;
            }

            bool isActiveButton = (i == (int)currentScene);
            menuButtons[i].transform.localScale = isActiveButton ? Vector3.one * 1.2f : Vector3.one;
            btnImage.color = isActiveButton ? activeColor : normalColor;

            if (i < underlineImages.Count && underlineImages[i] != null)
            {
                underlineImages[i].SetActive(isActiveButton);
            }
            else if (isActiveButton)
            {
                 Debug.LogWarning($"MainMenuController: Underline image for button at index {i} (scene {(SceneType)i}) is not assigned or 'underlineImages' list is too short.");
            }
        }
    }

    IEnumerator FitImageToParentAfterLayout()
    {
        yield return new WaitForEndOfFrame();
        FitImageToParent();
    }

    public void FitImageToParent()
    {
        if (backgroundToFitImage == null || backgroundToFitImage.sprite == null)
        {
            Debug.LogWarning("BackgroundToFit Image or its Sprite not assigned in MainMenuController, or sprite is missing.");
            return;
        }

        RectTransform backgroundRect = backgroundToFitImage.rectTransform;
        RectTransform parentRect = backgroundRect.parent as RectTransform;

        if (parentRect == null)
        {
            Debug.LogWarning("BackgroundToFit's parent is not a RectTransform. Cannot determine parent size.");
            return;
        }

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        Debug.Log($"[FitImage] Parent (Name: {parentRect.name}) Width: {parentWidth}, Height: {parentHeight}");

        if (parentWidth <= 0 || parentHeight <= 0)
        {
            Debug.LogWarning($"[FitImage] Parent RectTransform width ({parentWidth}) or height ({parentHeight}) is zero or invalid. Cannot fit image yet.");
            return;
        }

        float imageNativeWidth = backgroundToFitImage.sprite.rect.width;
        float imageNativeHeight = backgroundToFitImage.sprite.rect.height;

        Debug.Log($"[FitImage] Image Sprite (Name: {backgroundToFitImage.sprite.name}) Native Width: {imageNativeWidth}, Native Height: {imageNativeHeight}");

        if (imageNativeWidth <= 0 || imageNativeHeight <= 0)
        {
            Debug.LogWarning("[FitImage] BackgroundToFit sprite native width or height is zero or invalid.");
            return;
        }

        // Set anchors and pivot to center for consistent scaling and positioning
        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);

        // Set the RectTransform's size to the native size of the sprite
        backgroundRect.sizeDelta = new Vector2(imageNativeWidth, imageNativeHeight);
        Debug.Log($"[FitImage] Set background sizeDelta to Native: W={imageNativeWidth}, H={imageNativeHeight}");

        // Calculate scale factors required to fit width and height
        float scaleFactorWidth = parentWidth / imageNativeWidth;
        float scaleFactorHeight = parentHeight / imageNativeHeight;

        // Use the smaller scale factor to ensure the image fits entirely while maintaining aspect ratio
        float uniformScaleFactor = Mathf.Max(scaleFactorWidth, scaleFactorHeight);

        Debug.Log($"[FitImage] ScaleFactorWidth: {scaleFactorWidth}, ScaleFactorHeight: {scaleFactorHeight}, UniformScaleFactor: {uniformScaleFactor}");

        // Apply the uniform scale
        backgroundRect.localScale = new Vector3(uniformScaleFactor, uniformScaleFactor, 1f);

        // Ensure it's centered in the parent
        backgroundRect.anchoredPosition = Vector2.zero;

        Debug.Log($"[FitImage] Final LocalScale: {backgroundRect.localScale}, AnchoredPosition: {backgroundRect.anchoredPosition}");
    }
}
