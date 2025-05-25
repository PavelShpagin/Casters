using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class DynamicDeckSelectionPopup : MonoBehaviour
{
    private GameObject popupPanel;
    private GameObject backgroundOverlay;
    private Transform deckGridContainer;
    private ScrollRect scrollRect;
    
    private List<GameObject> instantiatedDeckItems = new List<GameObject>();
    private System.Action<string> onDeckSelected;
    private System.Action<string> onDeckEdit;
    
    // Sprites loaded from Resources
    private Sprite deckSelectionBgSprite;
    private Sprite closeButtonSprite;
    private Sprite editButtonSprite;
    
    public static DynamicDeckSelectionPopup Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadSprites();
            CreatePopupUI();
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
    
    void LoadSprites()
    {
        // Load sprites from Art/Sprites/UI/Misc folder
        deckSelectionBgSprite = Resources.Load<Sprite>("Art/Sprites/UI/Misc/deck_sel");
        closeButtonSprite = Resources.Load<Sprite>("Art/Sprites/UI/Misc/close_btn");
        editButtonSprite = Resources.Load<Sprite>("Art/Sprites/UI/Misc/edit");
        
        Debug.Log($"[DynamicDeckSelectionPopup] Loaded sprites - Deck: {deckSelectionBgSprite != null}, Close: {closeButtonSprite != null}, Edit: {editButtonSprite != null}");
        
        // Create fallback sprites if loading failed
        if (deckSelectionBgSprite == null)
        {
            deckSelectionBgSprite = CreateRoundedRectSprite(800, 600, 20, new Color(0.1f, 0.1f, 0.15f, 0.95f));
            Debug.LogWarning("[DynamicDeckSelectionPopup] deck_sel.png not found, using generated sprite");
        }
        
        if (closeButtonSprite == null)
        {
            closeButtonSprite = CreateCloseButtonSprite();
            Debug.LogWarning("[DynamicDeckSelectionPopup] close_btn.png not found, using generated sprite");
        }
        
        if (editButtonSprite == null)
        {
            editButtonSprite = CreateEditButtonSprite();
            Debug.LogWarning("[DynamicDeckSelectionPopup] edit.png not found, using generated sprite");
        }
    }
    
    void CreatePopupUI()
    {
        // Find or create main canvas
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("[DynamicDeckSelectionPopup] No Canvas found in scene!");
            return;
        }
        
        // Create main popup container that fills the canvas
        popupPanel = new GameObject("DynamicDeckSelectionPopup");
        popupPanel.transform.SetParent(mainCanvas.transform, false);
        
        // Add Canvas component to ensure it renders on top
        Canvas popupCanvas = popupPanel.AddComponent<Canvas>();
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 100; // High value to appear on top
        
        // Add GraphicRaycaster for UI interactions
        popupPanel.AddComponent<GraphicRaycaster>();
        
        RectTransform popupRect = popupPanel.AddComponent<RectTransform>();
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.offsetMin = Vector2.zero;
        popupRect.offsetMax = Vector2.zero;
        
        // Create all UI elements
        CreateBackgroundOverlay();
        CreateMainWindow();
        CreateCloseButton();
        CreateScrollArea();
        
        Debug.Log("[DynamicDeckSelectionPopup] UI created successfully - fully dynamic!");
    }
    
    void CreateBackgroundOverlay()
    {
        backgroundOverlay = new GameObject("BackgroundOverlay");
        backgroundOverlay.transform.SetParent(popupPanel.transform, false);
        
        RectTransform bgRect = backgroundOverlay.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Image bgImage = backgroundOverlay.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f); // Transparent black
        
        // Make background clickable to close popup
        Button bgButton = backgroundOverlay.AddComponent<Button>();
        bgButton.targetGraphic = bgImage;
        bgButton.onClick.AddListener(HidePopup);
    }
    
    void CreateMainWindow()
    {
        GameObject mainWindow = new GameObject("MainWindow");
        mainWindow.transform.SetParent(popupPanel.transform, false);
        
        RectTransform windowRect = mainWindow.AddComponent<RectTransform>();
        windowRect.anchoredPosition = Vector2.zero;
        windowRect.sizeDelta = new Vector2(800, 600);
        
        Image windowImage = mainWindow.AddComponent<Image>();
        windowImage.sprite = deckSelectionBgSprite;
        windowImage.type = Image.Type.Sliced;
        
        // Add title
        CreateTitle(mainWindow.transform);
    }
    
    void CreateTitle(Transform parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(0, 60);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SELECT DECK";
        titleText.fontSize = 24;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
    }
    
    void CreateCloseButton()
    {
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(popupPanel.transform, false);
        
        RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-30, -30);
        closeRect.sizeDelta = new Vector2(40, 40);
        
        Image closeImage = closeButtonObj.AddComponent<Image>();
        closeImage.sprite = closeButtonSprite;
        
        Button closeButton = closeButtonObj.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        closeButton.onClick.AddListener(HidePopup);
    }
    
    void CreateScrollArea()
    {
        // Create scroll view (matches your existing structure)
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(popupPanel.transform, false);
        
        RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
        scrollViewRect.anchoredPosition = Vector2.zero;
        scrollViewRect.sizeDelta = new Vector2(760, 480);
        
        // Add ScrollRect component
        ScrollRect scrollRectComponent = scrollViewObj.AddComponent<ScrollRect>();
        scrollRectComponent.horizontal = false;
        scrollRectComponent.vertical = true;
        
        // Create viewport (matches your structure)
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollViewObj.transform, false);
        
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.clear;
        
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        // Create content container (like your DeckGridContent)
        GameObject content = new GameObject("DeckGridContent");
        content.transform.SetParent(viewport.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        
        // Add GridLayoutGroup (matching your settings)
        GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(235, 235); // Match your deck box size
        gridLayout.spacing = new Vector2(25, 25);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 4; // 4 columns to match your layout
        
        // Add ContentSizeFitter
        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Connect ScrollRect
        this.scrollRect = scrollRectComponent;
        scrollRectComponent.content = contentRect;
        scrollRectComponent.viewport = viewportRect;
        
        deckGridContainer = content.transform;
        
        Debug.Log("[DynamicDeckSelectionPopup] Scroll area created - fully dynamic grid container ready");
    }
    
    public void ShowPopup(System.Action<string> onDeckSelected = null, System.Action<string> onDeckEdit = null)
    {
        this.onDeckSelected = onDeckSelected;
        this.onDeckEdit = onDeckEdit;
        
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            PopulateDecks();
            Debug.Log("[DynamicDeckSelectionPopup] Popup shown");
        }
    }
    
    public void HidePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
    
    void PopulateDecks()
    {
        // Clear existing items
        ClearDeckItems();
        
        if (DeckManager.Instance == null)
        {
            Debug.LogError("[DynamicDeckSelectionPopup] DeckManager.Instance is null!");
            return;
        }
        
        List<Deck> allDecks = DeckManager.Instance.GetAllDecks();
        
        foreach (Deck deck in allDecks)
        {
            CreateDeckItem(deck);
        }
        
        Debug.Log($"[DynamicDeckSelectionPopup] Populated {allDecks.Count} decks dynamically");
    }
    
    void CreateDeckItem(Deck deck)
    {
        GameObject deckItem = new GameObject($"DeckItem_{deck.deckName}");
        deckItem.transform.SetParent(deckGridContainer, false);
        
        // Main deck item background
        Image deckImage = deckItem.AddComponent<Image>();
        deckImage.sprite = CreateDeckItemSprite();
        deckImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        
        // Make deck item clickable
        Button deckButton = deckItem.AddComponent<Button>();
        deckButton.targetGraphic = deckImage;
        deckButton.onClick.AddListener(() => {
            onDeckSelected?.Invoke(deck.uniqueID);
            HidePopup();
        });
        
        // Add deck name
        CreateDeckNameText(deckItem.transform, deck.deckName);
        
        // Add deck stats
        CreateDeckStatsText(deckItem.transform, deck);
        
        // Add edit button
        CreateEditButton(deckItem.transform, deck.uniqueID);
        
        instantiatedDeckItems.Add(deckItem);
    }
    
    void CreateDeckNameText(Transform parent, string deckName)
    {
        GameObject nameObj = new GameObject("DeckName");
        nameObj.transform.SetParent(parent, false);
        
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.7f);
        nameRect.anchorMax = new Vector2(1, 0.9f);
        nameRect.offsetMin = new Vector2(10, 0);
        nameRect.offsetMax = new Vector2(-10, 0);
        
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = deckName;
        nameText.fontSize = 16;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontStyle = FontStyles.Bold;
    }
    
    void CreateDeckStatsText(Transform parent, Deck deck)
    {
        GameObject statsObj = new GameObject("DeckStats");
        statsObj.transform.SetParent(parent, false);
        
        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0.5f);
        statsRect.anchorMax = new Vector2(1, 0.7f);
        statsRect.offsetMin = new Vector2(10, 0);
        statsRect.offsetMax = new Vector2(-10, 0);
        
        int mainCards = deck.mainDeckCards.Values.Sum();
        int stageCards = deck.stageDeckCards.Values.Sum();
        
        TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = $"Main: {mainCards}\nStage: {stageCards}";
        statsText.fontSize = 12;
        statsText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        statsText.alignment = TextAlignmentOptions.Center;
    }
    
    void CreateEditButton(Transform parent, string deckID)
    {
        GameObject editButtonObj = new GameObject("EditButton");
        editButtonObj.transform.SetParent(parent, false);
        
        RectTransform editRect = editButtonObj.AddComponent<RectTransform>();
        editRect.anchorMin = new Vector2(1, 1);
        editRect.anchorMax = new Vector2(1, 1);
        editRect.anchoredPosition = new Vector2(-15, -15);
        editRect.sizeDelta = new Vector2(30, 30);
        
        Image editImage = editButtonObj.AddComponent<Image>();
        editImage.sprite = editButtonSprite;
        
        Button editButton = editButtonObj.AddComponent<Button>();
        editButton.targetGraphic = editImage;
        editButton.onClick.AddListener(() => {
            onDeckEdit?.Invoke(deckID);
            HidePopup();
        });
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
    
    // Sprite creation methods (fallbacks)
    Sprite CreateRoundedRectSprite(int width, int height, int cornerRadius, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isInside = IsInsideRoundedRect(x, y, width, height, cornerRadius);
                pixels[y * width + x] = isInside ? color : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
    }
    
    bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
    {
        if (x >= radius && x < width - radius) return true;
        if (y >= radius && y < height - radius) return true;
        
        Vector2[] corners = {
            new Vector2(radius, radius),
            new Vector2(width - radius, radius),
            new Vector2(radius, height - radius),
            new Vector2(width - radius, height - radius)
        };
        
        foreach (Vector2 corner in corners)
        {
            if (Vector2.Distance(new Vector2(x, y), corner) <= radius)
                return true;
        }
        
        return false;
    }
    
    Sprite CreateCloseButtonSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isX = (Mathf.Abs(x - y) <= 2) || (Mathf.Abs(x - (size - 1 - y)) <= 2);
                bool isInCircle = Vector2.Distance(new Vector2(x, y), new Vector2(size/2, size/2)) <= size/2 - 2;
                
                if (isX && isInCircle)
                    pixels[y * size + x] = Color.white;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    Sprite CreateEditButtonSprite()
    {
        int size = 24;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isPencil = (x >= 2 && x <= 6 && y >= size - 10) || 
                               (x >= 4 && x <= 8 && y >= size - 20 && y < size - 10);
                
                if (isPencil)
                    pixels[y * size + x] = Color.white;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    Sprite CreateDeckItemSprite()
    {
        return CreateRoundedRectSprite(235, 235, 10, new Color(0.15f, 0.15f, 0.2f, 1f));
    }
} 