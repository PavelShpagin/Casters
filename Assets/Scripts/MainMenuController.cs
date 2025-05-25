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

    public void UpdateUI()
    {
        Debug.Log($"[MainMenuController] UpdateUI called for scene: {currentScene}");

        // Deactivate all primary content panels first
        if (playScreenContent != null) playScreenContent.SetActive(false);
        if (deckScreenContent != null) deckScreenContent.SetActive(false);
        foreach (GameObject panel in panels)
        {
            if (panel != null) panel.SetActive(false);
        }

        // Activate the correct panel based on currentScene
        switch (currentScene)
        {
            case SceneType.Play:
                if (playScreenContent != null) {
                    Debug.Log("[MainMenuController] Activating playScreenContent.");
                    playScreenContent.SetActive(true);
                }
                else Debug.LogWarning("MainMenuController: playScreenContent is not assigned!");
                break;
            case SceneType.Deck:
                if (deckScreenContent != null) {
                    Debug.Log("[MainMenuController] Activating deckScreenContent.");
                    deckScreenContent.SetActive(true);
                }
                else Debug.LogWarning("MainMenuController: deckScreenContent is not assigned!");
                break;
            case SceneType.Solo:
                // Assuming Solo corresponds to panels[0] if panels is used for scenes beyond Play/Deck
                // or panels[2] if panels list aligns directly with SceneType enum indices starting from 0.
                // For safety, let's use an index that would correspond to SceneType.Solo (index 2).
                // This part needs to align with how 'panels' list is intended to be used.
                // If 'panels' strictly holds Solo, Shop, etc., then index might be (int)currentScene - 2.
                // For now, using direct mapping for SceneType indices 2 and 3 if panels has them.
                if ((int)currentScene < panels.Count && panels[(int)currentScene] != null)
                {
                    Debug.Log($"[MainMenuController] Activating panel for {currentScene}: {panels[(int)currentScene].name}");
                    panels[(int)currentScene].SetActive(true);
                }
                else if (panels.Count > 0 && (int)currentScene == 2 && panels.Count > (int)SceneType.Solo - 2 && panels[(int)SceneType.Solo -2] != null ) // A common pattern: panels[0] = Solo, panels[1] = Shop
                {
                     Debug.Log($"[MainMenuController] Activating panel for {currentScene} (index {(int)SceneType.Solo -2}): {panels[(int)SceneType.Solo -2].name}");
                     panels[(int)SceneType.Solo -2].SetActive(true); // Example: Solo is SceneType index 2, panels[0]
                }
                else
                {
                    Debug.LogWarning($"MainMenuController: Panel for scene {currentScene} (index {(int)currentScene}) is not assigned or panels list misconfigured.");
                }
                break;
            case SceneType.Shop:
                 if ((int)currentScene < panels.Count && panels[(int)currentScene] != null)
                {
                    Debug.Log($"[MainMenuController] Activating panel for {currentScene}: {panels[(int)currentScene].name}");
                    panels[(int)currentScene].SetActive(true);
                }
                // Example: Shop is SceneType index 3, panels[1]
                else if (panels.Count > 1 && (int)currentScene == 3 && panels.Count > (int)SceneType.Shop - 2 && panels[(int)SceneType.Shop -2] != null )
                {
                     Debug.Log($"[MainMenuController] Activating panel for {currentScene} (index {(int)SceneType.Shop -2}): {panels[(int)SceneType.Shop -2].name}");
                     panels[(int)SceneType.Shop -2].SetActive(true); 
                }
                else
                {
                    Debug.LogWarning($"MainMenuController: Panel for scene {currentScene} (index {(int)currentScene}) is not assigned or panels list misconfigured.");
                }
                break;
            default:
                Debug.LogWarning($"MainMenuController: Unhandled scene type: {currentScene}");
                break;
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
