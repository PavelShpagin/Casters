using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuilderGenerator : EditorWindow
{
    // --- Style Constants ---
    private const float ELEMENT_HEIGHT_S = 30f; // Standard small height for buttons, inputs
    private const float ELEMENT_HEIGHT_M = 40f; // Medium height (no longer used for top bar)
    private const int PADDING_S = 5;
    private const int PADDING_M = 10;
    private const int SPACING_S = 5;
    private const int SPACING_M = 10;

    private static Color PANEL_BG_COLOR = new Color(0.15f, 0.15f, 0.18f, 0.9f); // Darker panel
    private static Color CANVAS_BG_COLOR = new Color(0.1f, 0.1f, 0.12f, 1f);   // Even darker canvas bg
    private static Color BUTTON_BG_COLOR = new Color(0.25f, 0.27f, 0.3f, 1f);
    private static Color INPUT_BG_COLOR = new Color(0.1f, 0.1f, 0.12f, 1f);
    private static Color TEXT_COLOR_LIGHT = Color.white;
    private static Color TEXT_COLOR_DARK = new Color(0.1f, 0.1f, 0.1f, 1f);
    private static Color SCROLL_VIEW_BG_COLOR = new Color(0.1f, 0.1f, 0.12f, 0.5f); // Slightly transparent
    private static Color DECK_AREA_BG_COLOR = new Color(0.1f, 0.1f, 0.12f, 0.5f); // BG for non-scrolling deck areas
    private static Color BORDER_COLOR = new Color(0.25f, 0.25f, 0.3f, 1f);
    // private const float CORNER_RADIUS = 8f;

    // Card display sizes (can be adjusted) 
    private const float COLLECTION_CARD_WIDTH = 80f;
    private const float COLLECTION_CARD_HEIGHT = 112f;
    private const float DECK_CARD_WIDTH = 70f; // Slightly smaller for deck lists
    private const float DECK_CARD_HEIGHT = 98f;

    // Estimate grid size needed based on max cards and cell size + spacing
    // This is approximate and might need tweaking based on actual appearance
    private const float MAIN_DECK_GRID_HEIGHT = (DECK_CARD_HEIGHT * 5) + (10 * 6); // ~5 rows
    private const float STAGE_DECK_GRID_HEIGHT = DECK_CARD_HEIGHT + 20; // ~1 row

    [MenuItem("Tools/Deck Building/SETUP - Create DeckBuilder Canvas Only (v3 Layout)")]
    public static void CreateDeckBuilderCanvasV3()
    {
        GameObject deckBuilderCanvasGO = CreateCanvas("DeckBuilderCanvas");
        deckBuilderCanvasGO.SetActive(false);
        CreatePanel(deckBuilderCanvasGO.transform, "BackgroundPanel_Builder", CANVAS_BG_COLOR, true, true);

        // --- TOP BAR (fixed height, anchored to top) ---
        GameObject topBar = CreatePanel(deckBuilderCanvasGO.transform, "TopBar", PANEL_BG_COLOR, true, false);
        RectTransform topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0, 1);
        topBarRT.anchorMax = new Vector2(1, 1);
        topBarRT.pivot = new Vector2(0.5f, 1);
        topBarRT.sizeDelta = new Vector2(0, 60); // Fixed height
        topBarRT.anchoredPosition = new Vector2(0, 0);

        HorizontalLayoutGroup topBarHLG = topBar.AddComponent<HorizontalLayoutGroup>();
        topBarHLG.padding = new RectOffset(PADDING_M, PADDING_M, 0, 0);
        topBarHLG.spacing = SPACING_M;
        topBarHLG.childAlignment = TextAnchor.MiddleCenter;
        topBarHLG.childControlWidth = true;
        topBarHLG.childControlHeight = true;
        topBarHLG.childForceExpandWidth = true;
        topBarHLG.childForceExpandHeight = true;

        // Back Arrow (left)
        GameObject backBtn = CreateButton(topBar.transform, "BackButton_Builder", "<", new Vector2(ELEMENT_HEIGHT_S, ELEMENT_HEIGHT_S));
        LayoutElement backBtnLE = backBtn.AddComponent<LayoutElement>();
        backBtnLE.flexibleWidth = 0;
        backBtnLE.minWidth = ELEMENT_HEIGHT_S;

        // Spacer (left of center)
        GameObject leftSpacer = new GameObject("LeftSpacer", typeof(RectTransform));
        leftSpacer.transform.SetParent(topBar.transform, false);
        LayoutElement leftSpacerLE = leftSpacer.AddComponent<LayoutElement>();
        leftSpacerLE.flexibleWidth = 1;

        // Deck Name Input (center)
        GameObject deckNameInput = CreateInputField(topBar.transform, "DeckNameInput_Builder", "Untitled", ELEMENT_HEIGHT_S);
        LayoutElement deckNameLE = deckNameInput.AddComponent<LayoutElement>();
        deckNameLE.flexibleWidth = 2;
        deckNameLE.minWidth = 200;

        // Spacer (right of center)
        GameObject rightSpacer = new GameObject("RightSpacer", typeof(RectTransform));
        rightSpacer.transform.SetParent(topBar.transform, false);
        LayoutElement rightSpacerLE = rightSpacer.AddComponent<LayoutElement>();
        rightSpacerLE.flexibleWidth = 1;

        // Save Button (right)
        GameObject saveBtn = CreateButton(topBar.transform, "SaveButton_Builder", "Save", new Vector2(90, ELEMENT_HEIGHT_S));
        LayoutElement saveBtnLE = saveBtn.AddComponent<LayoutElement>();
        saveBtnLE.flexibleWidth = 0;
        saveBtnLE.minWidth = 90;

        // --- MAIN HORIZONTAL LAYOUT (anchored below top bar) ---
        GameObject mainLayoutGO = CreatePanel(deckBuilderCanvasGO.transform, "MainLayout_Builder", Color.clear, false, false);
        RectTransform mainLayoutRT = mainLayoutGO.GetComponent<RectTransform>();
        mainLayoutRT.anchorMin = new Vector2(0, 0);
        mainLayoutRT.anchorMax = new Vector2(1, 1);
        mainLayoutRT.pivot = new Vector2(0.5f, 0.5f);
        mainLayoutRT.offsetMin = new Vector2(0, 0);
        mainLayoutRT.offsetMax = new Vector2(0, -60); // Leave space for top bar

        HorizontalLayoutGroup mainHLG = mainLayoutGO.AddComponent<HorizontalLayoutGroup>();
        mainHLG.padding = new RectOffset(PADDING_M, PADDING_M, PADDING_M, PADDING_M);
        mainHLG.spacing = SPACING_M;
        mainHLG.childControlWidth = true;
        mainHLG.childControlHeight = true;
        mainHLG.childForceExpandWidth = true;
        mainHLG.childForceExpandHeight = true;

        // --- LEFT PANEL (Search + Collection) ---
        GameObject leftPanel = CreatePanel(mainLayoutGO.transform, "CollectionPanel_Builder", PANEL_BG_COLOR, true, false);
        leftPanel.AddComponent<LayoutElement>().flexibleWidth = 0.8f;
        VerticalLayoutGroup leftVLG = leftPanel.AddComponent<VerticalLayoutGroup>();
        leftVLG.padding = new RectOffset(PADDING_S, PADDING_S, PADDING_S, PADDING_S);
        leftVLG.spacing = SPACING_S;
        leftVLG.childControlWidth = true;
        leftVLG.childControlHeight = true;
        leftVLG.childForceExpandHeight = false; // Allow scroll area to expand

        // Search Bar
        GameObject searchBar = CreateInputField(leftPanel.transform, "SearchBar_Collection", "Search cards...", ELEMENT_HEIGHT_S);
        LayoutElement searchBarLE = searchBar.AddComponent<LayoutElement>();
        searchBarLE.flexibleWidth = 1;
        searchBarLE.minHeight = ELEMENT_HEIGHT_S;
        searchBarLE.preferredHeight = ELEMENT_HEIGHT_S;
        searchBarLE.flexibleHeight = 0;

        // Collection ScrollView (fills remaining space)
        GameObject collectionScrollView = CreateScrollView(leftPanel.transform, "CollectionScrollView_Builder", SCROLL_VIEW_BG_COLOR);
        LayoutElement collectionScrollLE = collectionScrollView.AddComponent<LayoutElement>();
        collectionScrollLE.flexibleHeight = 1f; // Fills all available space under search bar
        collectionScrollLE.minHeight = 200f;    // Minimum height for usability
        collectionScrollLE.flexibleWidth = 1;

        // --- RIGHT PANEL (Deck Details) ---
        GameObject rightPanel = CreatePanel(mainLayoutGO.transform, "RightDeckPanel_Builder", PANEL_BG_COLOR, true, false);
        rightPanel.AddComponent<LayoutElement>().flexibleWidth = 1.2f;
        VerticalLayoutGroup rightVLG = rightPanel.AddComponent<VerticalLayoutGroup>();
        rightVLG.padding = new RectOffset(PADDING_S, PADDING_S, PADDING_S, PADDING_S);
        rightVLG.spacing = SPACING_M;
        rightVLG.childControlWidth = true;
        rightVLG.childControlHeight = true;
        rightVLG.childForceExpandHeight = true;

        // Deck Title (label only, not input)
        GameObject mainDeckTitle = CreateText(rightPanel.transform, "MainDeck_Title", "Main Deck (0/30)", Mathf.RoundToInt(ELEMENT_HEIGHT_S * 0.7f), TextAnchor.MiddleLeft, TEXT_COLOR_LIGHT);
        LayoutElement mainDeckTitleLE = mainDeckTitle.AddComponent<LayoutElement>();
        mainDeckTitleLE.minHeight = ELEMENT_HEIGHT_S * 0.9f;
        mainDeckTitleLE.preferredHeight = ELEMENT_HEIGHT_S * 0.9f;
        mainDeckTitleLE.flexibleHeight = 0;

        // Main Deck Grid (stretch: 70% of available space)
        GameObject mainDeckArea = CreateDeckAreaPanel(rightPanel.transform, "MainDeckArea", DECK_AREA_BG_COLOR, 1f);
        LayoutElement mainDeckAreaLE = mainDeckArea.AddComponent<LayoutElement>();
        mainDeckAreaLE.flexibleHeight = 7; // 70% of available space
        mainDeckAreaLE.minHeight = 0;
        mainDeckAreaLE.preferredHeight = -1;

        // Stage Deck Title
        GameObject stageDeckTitle = CreateText(rightPanel.transform, "StageDeck_Title", "Stage Deck (0/5)", Mathf.RoundToInt(ELEMENT_HEIGHT_S * 0.7f), TextAnchor.MiddleLeft, TEXT_COLOR_LIGHT);
        LayoutElement stageDeckTitleLE = stageDeckTitle.AddComponent<LayoutElement>();
        stageDeckTitleLE.minHeight = ELEMENT_HEIGHT_S * 0.9f;
        stageDeckTitleLE.preferredHeight = ELEMENT_HEIGHT_S * 0.9f;
        stageDeckTitleLE.flexibleHeight = 0;

        // Stage Deck Grid (stretch: 30% of available space)
        GameObject stageDeckArea = CreateDeckAreaPanel(rightPanel.transform, "StageDeckArea", DECK_AREA_BG_COLOR, 1f);
        LayoutElement stageDeckAreaLE = stageDeckArea.AddComponent<LayoutElement>();
        stageDeckAreaLE.flexibleHeight = 3; // 30% of available space
        stageDeckAreaLE.minHeight = 0;
        stageDeckAreaLE.preferredHeight = -1;

        Debug.Log("DeckBuilderCanvas (modern layout, fixed top bar, fixed scroll/deck heights) created successfully! Remember to SAVE YOUR SCENE.");
    }

    private static GameObject CreateDeckAreaPanel(Transform parent, string name, Color bgColor, float fillAmount)
    {
        GameObject panel = CreatePanel(parent, name, bgColor, true, false);
        // ... any additional setup for the deck area ...
        return panel;
    }

    private static void ConfigureCardGrid(GameObject contentPanel, Vector2 cellSize, Vector2 spacing, bool addContentSizeFitter)
    {
        GridLayoutGroup gridLayout = contentPanel.GetComponent<GridLayoutGroup>() ?? contentPanel.AddComponent<GridLayoutGroup>();
        gridLayout.padding = new RectOffset(PADDING_S, PADDING_S, PADDING_S, PADDING_S);
        gridLayout.spacing = spacing;
        gridLayout.cellSize = cellSize;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        
        if (addContentSizeFitter)
        {
            ContentSizeFitter contentFitter = contentPanel.GetComponent<ContentSizeFitter>() ?? contentPanel.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            // For horizontal, if you want cards to wrap and expand grid horizontally:
            // contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize; 
            // If you want it to scroll horizontally only when content exceeds view, leave horizontal as Unconstrained for Content and set ScrollRect to allow horizontal.
            // For typical card collections, vertical scroll with horizontal wrapping by fixed cell count or flexible is common.
            // Here, `constraint = Flexible` (default) means it will fill horizontally then wrap.
        }
        else
        {
             ContentSizeFitter fitter = contentPanel.GetComponent<ContentSizeFitter>();
             if (fitter != null && !addContentSizeFitter) { Object.DestroyImmediate(fitter); }
        }
    }
    
    private static GameObject CreateCanvas(string name)
    {
        GameObject canvasGO = new GameObject(name);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); // Standard HD
        canvasGO.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create " + name);
        return canvasGO;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color bgColor, bool addImageComponent, bool matchParentSize = false)
    {
        GameObject panelGO = new GameObject(name, typeof(RectTransform));
        panelGO.transform.SetParent(parent, false);
        if (addImageComponent)
        {
            Image image = panelGO.AddComponent<Image>();
            image.color = bgColor;
            image.raycastTarget = false; // Generally panels don't need to block rays unless interactive
        }
        RectTransform rt = panelGO.GetComponent<RectTransform>();
        if (matchParentSize) // Useful for background panels
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
        Undo.RegisterCreatedObjectUndo(panelGO, "Create " + name);
        return panelGO;
    }

    private static GameObject CreateText(Transform parent, string name, string textContent, int fontSize, TextAnchor alignment, Color textColor)
    {
        GameObject textGO = new GameObject(name, typeof(RectTransform));
        textGO.transform.SetParent(parent, false);
        Text textComp = textGO.AddComponent<Text>();
        textComp.text = textContent;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = fontSize;
        textComp.alignment = alignment;
        textComp.color = textColor;
        Undo.RegisterCreatedObjectUndo(textGO, "Create " + name);
        return textGO;
    }

    private static GameObject CreateButton(Transform parent, string name, string buttonText, Vector2 size)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);
        Image buttonImage = buttonGO.GetComponent<Image>();
        buttonImage.color = BUTTON_BG_COLOR;
        buttonImage.type = Image.Type.Sliced; // Good for scaling buttons
        Button buttonComp = buttonGO.GetComponent<Button>();
        Navigation nav = buttonComp.navigation;
        nav.mode = Navigation.Mode.None; // Simpler nav for now
        buttonComp.navigation = nav;
        RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
        buttonRT.sizeDelta = size;

        int fontSize = Mathf.Max(10, Mathf.RoundToInt(size.y * 0.6f)); // Slightly larger text ratio for smaller buttons
        GameObject textGO = CreateText(buttonGO.transform, "ButtonText", buttonText, fontSize, TextAnchor.MiddleCenter, TEXT_COLOR_LIGHT);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one; // Text fills button
        textRT.sizeDelta = Vector2.zero; textRT.anchoredPosition = Vector2.zero;
        
        Undo.RegisterCreatedObjectUndo(buttonGO, "Create " + name);
        return buttonGO;
    }
    
    private static GameObject CreateInputField(Transform parent, string name, string placeholderText, float height)
    {
        GameObject inputFieldGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        inputFieldGO.transform.SetParent(parent, false);
        inputFieldGO.GetComponent<RectTransform>().sizeDelta = new Vector2(200, height); 

        Image bgImage = inputFieldGO.GetComponent<Image>();
        bgImage.color = INPUT_BG_COLOR;
        bgImage.type = Image.Type.Sliced;

        InputField inputField = inputFieldGO.GetComponent<InputField>();
        
        int fontSize = Mathf.Max(10, Mathf.RoundToInt(height * 0.5f));

        GameObject placeholderGO = CreateText(inputFieldGO.transform, "Placeholder", placeholderText, fontSize, TextAnchor.MiddleLeft, new Color(0.6f,0.6f,0.6f,0.8f));
        Text placeholderTextComp = placeholderGO.GetComponent<Text>();
        placeholderTextComp.fontStyle = FontStyle.Italic;
        RectTransform placeholderRT = placeholderGO.GetComponent<RectTransform>();
        placeholderRT.anchorMin = Vector2.zero; placeholderRT.anchorMax = Vector2.one;
        placeholderRT.sizeDelta = new Vector2(-(PADDING_M), 0); // Only left padding for placeholder
        placeholderRT.anchoredPosition = new Vector2(PADDING_S,0); // Shift placeholder slightly right

        GameObject textCompGO = CreateText(inputFieldGO.transform, "Text", "", fontSize, TextAnchor.MiddleLeft, TEXT_COLOR_LIGHT);
        RectTransform textRT = textCompGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = new Vector2(-(PADDING_M), 0); 
        textRT.anchoredPosition = new Vector2(PADDING_S,0);
        inputField.textComponent = textCompGO.GetComponent<Text>();

        Navigation nav = inputField.navigation;
        nav.mode = Navigation.Mode.Automatic;
        inputField.navigation = nav;
        
        Undo.RegisterCreatedObjectUndo(inputFieldGO, "Create " + name);
        return inputFieldGO;
    }

    private static GameObject CreateScrollView(Transform parent, string name, Color bgColor)
    {
        GameObject scrollViewGO = new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
        scrollViewGO.transform.SetParent(parent, false);
        ScrollRect scrollRect = scrollViewGO.GetComponent<ScrollRect>();
        
        Image svBgImage = scrollViewGO.AddComponent<Image>(); 
        svBgImage.color = bgColor;
        svBgImage.type = Image.Type.Sliced;

        GameObject viewportGO = CreatePanel(scrollViewGO.transform, "Viewport", Color.clear, false); // Viewport doesn't need its own image if bg is on scrollview
        viewportGO.AddComponent<Mask>().showMaskGraphic = false; 
        RectTransform viewportRT = viewportGO.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero; viewportRT.anchorMax = Vector2.one;
        viewportRT.sizeDelta = new Vector2(-PADDING_S, -PADDING_S*2); // Padding inside the scroll view
        viewportRT.anchoredPosition = Vector2.zero;
        viewportRT.pivot = new Vector2(0, 1); 

        GameObject contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1); 
        contentRT.pivot = new Vector2(0.5f, 1);    
        contentRT.sizeDelta = new Vector2(0, 0); // Height controlled by fitter, width by viewport

        scrollRect.content = contentRT;
        scrollRect.viewport = viewportRT;
        scrollRect.horizontal = false; 
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.scrollSensitivity = 20; 
        
        Undo.RegisterCreatedObjectUndo(scrollViewGO, "Create " + name);
        return scrollViewGO;
    }
} 