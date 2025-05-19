using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DeckUISetup : EditorWindow
{
    private const float DECK_BOX_WIDTH = 160f;
    private const float DECK_BOX_HEIGHT = 200f;
    private const float DECK_BOX_NAME_HEIGHT = 30f;
    private const float CROSS_BUTTON_SIZE = 30f;

    // --- Style Constants ---
    private const float ELEMENT_HEIGHT_S = 32f; // Slightly larger for modern look
    private const int PADDING_S = 8;
    private const int PADDING_M = 16;
    private const int SPACING_S = 8;
    private const int SPACING_M = 16;

    // --- Modern Master Duel-inspired Colors ---
    private static readonly Color CANVAS_BG_COLOR      = new Color(0.06f, 0.06f, 0.08f, 1f);    // Darker black
    private static readonly Color PANEL_BG_COLOR       = new Color(0.12f, 0.12f, 0.15f, 0.98f); // Dark gray
    private static readonly Color PANEL_BORDER_COLOR   = new Color(0.2f, 0.2f, 0.25f, 1f);      // Slightly lighter for borders
    private static readonly Color BUTTON_BG_COLOR      = new Color(0.2f, 0.21f, 0.24f, 1f);     // Button dark
    private static readonly Color BUTTON_HIGHLIGHT     = new Color(0.28f, 0.3f, 0.35f, 1f);     // Button hover
    private static readonly Color INPUT_BG_COLOR       = new Color(0.1f, 0.1f, 0.12f, 1f);      // Input field
    private static readonly Color SCROLL_VIEW_BG_COLOR = new Color(0.09f, 0.09f, 0.11f, 0.7f);  // Slightly transparent
    private static readonly Color DECK_AREA_BG_COLOR   = new Color(0.11f, 0.11f, 0.14f, 0.9f);  // Deck area
    private static readonly Color TEXT_COLOR_LIGHT     = new Color(0.95f, 0.95f, 1f, 1f);       // Near white
    private static readonly Color TEXT_COLOR_SUBTLE    = new Color(0.65f, 0.65f, 0.75f, 1f);    // Subtle gray
    private static readonly Color PLACEHOLDER_COLOR    = new Color(0.45f, 0.45f, 0.55f, 0.7f);  // Placeholder

    // Add new style constants
    private const float BORDER_RADIUS = 8f;
    private const float BUTTON_BORDER_RADIUS = 6f;
    private const float INPUT_BORDER_RADIUS = 6f;
    private const float PANEL_BORDER_WIDTH = 1.5f;
    private const float BUTTON_BORDER_WIDTH = 1f;

    // Card display sizes
    private const float COLLECTION_CARD_WIDTH = 80f;
    private const float COLLECTION_CARD_HEIGHT = 112f;
    private const float DECK_CARD_WIDTH = 70f;
    private const float DECK_CARD_HEIGHT = 98f;

    [MenuItem("Tools/Deck Building/SETUP - Create ALL UI Canvases")]
    public static void CreateAllUICanvases()
    {
        CreateMainMenuCanvas();
        CreateMyDecksCanvas();
        CreateDeckBuilderCanvas();
        Debug.Log("All UI canvases created successfully! Remember to SAVE YOUR SCENE.");
    }

    private static void CreateMainMenuCanvas()
    {
        // Create basic canvas
        GameObject canvasGO = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Set up Canvas Scaler
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        // Background
        GameObject bgPanel = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgPanel.transform.SetParent(canvasGO.transform, false);
        Image bgImage = bgPanel.GetComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark blue-gray
        
        // Set background to fill screen
        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Title
        GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleGO.transform.SetParent(canvasGO.transform, false);
        Text titleText = titleGO.GetComponent<Text>();
        titleText.text = "Main Menu";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 36;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        
        // Position title at top
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(400, 80);
        titleRect.anchoredPosition = new Vector2(0, -50);
        
        // Create buttons container
        GameObject buttonsPanel = new GameObject("ButtonsPanel", typeof(RectTransform), typeof(VerticalLayoutGroup));
        buttonsPanel.transform.SetParent(canvasGO.transform, false);
        VerticalLayoutGroup vlg = buttonsPanel.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        
        // Position buttons in center
        RectTransform buttonsPanelRect = buttonsPanel.GetComponent<RectTransform>();
        buttonsPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonsPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonsPanelRect.pivot = new Vector2(0.5f, 0.5f);
        buttonsPanelRect.sizeDelta = new Vector2(300, 300);
        buttonsPanelRect.anchoredPosition = Vector2.zero;
        
        // Add buttons
        CreateSimpleButton(buttonsPanel.transform, "PlayButton", "Play");
        CreateSimpleButton(buttonsPanel.transform, "DeckButton", "Deck");
        CreateSimpleButton(buttonsPanel.transform, "SoloButton", "Solo");
        CreateSimpleButton(buttonsPanel.transform, "ShopButton", "Shop");
    }

    private static void CreateMyDecksCanvas()
    {
        // Create basic canvas
        GameObject canvasGO = new GameObject("MyDecksCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.SetActive(false); // Start inactive
        
        // Set up Canvas Scaler
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        // Background
        GameObject bgPanel = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgPanel.transform.SetParent(canvasGO.transform, false);
        Image bgImage = bgPanel.GetComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark blue-gray
        
        // Set background to fill screen
        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Top Bar
        GameObject topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
        topBar.transform.SetParent(canvasGO.transform, false);
        Image topBarImage = topBar.GetComponent<Image>();
        topBarImage.color = new Color(0.2f, 0.2f, 0.25f, 1f); // Slightly lighter than bg
        
        // Position top bar
        RectTransform topBarRect = topBar.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0, 1);
        topBarRect.anchorMax = new Vector2(1, 1);
        topBarRect.pivot = new Vector2(0.5f, 1);
        topBarRect.sizeDelta = new Vector2(0, 80);
        topBarRect.anchoredPosition = Vector2.zero;
        
        // Back Button
        GameObject backButton = CreateSimpleButton(topBar.transform, "BackButton", "<");
        RectTransform backButtonRect = backButton.GetComponent<RectTransform>();
        backButtonRect.anchorMin = new Vector2(0, 0.5f);
        backButtonRect.anchorMax = new Vector2(0, 0.5f);
        backButtonRect.pivot = new Vector2(0, 0.5f);
        backButtonRect.sizeDelta = new Vector2(50, 50);
        backButtonRect.anchoredPosition = new Vector2(20, 0);
        
        // Title
        GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleGO.transform.SetParent(topBar.transform, false);
        Text titleText = titleGO.GetComponent<Text>();
        titleText.text = "My Decks";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 32;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        
        // Position title
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = Vector2.zero;
        
        // Main Content Area
        GameObject contentArea = new GameObject("ContentArea", typeof(RectTransform));
        contentArea.transform.SetParent(canvasGO.transform, false);
        
        // Position content area below top bar
        RectTransform contentAreaRect = contentArea.GetComponent<RectTransform>();
        contentAreaRect.anchorMin = new Vector2(0, 0);
        contentAreaRect.anchorMax = new Vector2(1, 1);
        contentAreaRect.pivot = new Vector2(0.5f, 0.5f);
        contentAreaRect.offsetMin = new Vector2(20, 20);
        contentAreaRect.offsetMax = new Vector2(-20, -100);
        
        // Grid for decks
        GameObject gridGO = new GameObject("DecksGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGO.transform.SetParent(contentArea.transform, false);
        GridLayoutGroup grid = gridGO.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(160, 160); // Square boxes
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(10, 10, 10, 10);
        
        // Position grid
        RectTransform gridRect = gridGO.GetComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.sizeDelta = Vector2.zero;
        
        // Add "+" button
        GameObject addDeckButton = CreateDeckBox(gridGO.transform, "AddDeckButton", "+", true);
        
        // Add example deck if needed (would be replaced with real decks)
        CreateDeckBox(gridGO.transform, "ExampleDeck", "Example Deck", false);
    }

    private static GameObject CreateSimpleButton(Transform parent, string name, string text)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);
        
        // Set up button appearance
        Image buttonImage = buttonGO.GetComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        
        // Add text
        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(buttonGO.transform, false);
        Text buttonText = textGO.GetComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        
        // Position text to fill button
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        // Set button size
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 50);
        
        // Button colors
        Button button = buttonGO.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.35f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        button.colors = colors;
        
        return buttonGO;
    }

    private static GameObject CreateDeckBox(Transform parent, string name, string text, bool isAddButton)
    {
        GameObject deckBoxGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        deckBoxGO.transform.SetParent(parent, false);
        
        // Set up box appearance
        Image boxImage = deckBoxGO.GetComponent<Image>();
        boxImage.color = isAddButton ? new Color(0.3f, 0.5f, 0.3f, 1f) : new Color(0.3f, 0.3f, 0.4f, 1f);
        
        // Add text
        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(deckBoxGO.transform, false);
        Text boxText = textGO.GetComponent<Text>();
        boxText.text = text;
        boxText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        boxText.fontSize = isAddButton ? 48 : 20;
        boxText.alignment = TextAnchor.MiddleCenter;
        boxText.color = Color.white;
        
        if (!isAddButton)
        {
            // For normal deck boxes, move text to bottom
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 0.3f);
            textRect.sizeDelta = Vector2.zero;
        }
        else
        {
            // For add button, center the text
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        return deckBoxGO;
    }

    private static void CreateDeckBuilderCanvas()
    {
        GameObject deckBuilderCanvasGO = CreateCanvas("DeckBuilderCanvas");
        deckBuilderCanvasGO.SetActive(false);
        CreatePanel(deckBuilderCanvasGO.transform, "BackgroundPanel_Builder", CANVAS_BG_COLOR, true, true, true);

        GameObject mainLayoutGO = CreatePanel(deckBuilderCanvasGO.transform, "MainLayout_Builder", Color.clear, false, true, false);
        HorizontalLayoutGroup mainHLG = mainLayoutGO.AddComponent<HorizontalLayoutGroup>();
        mainHLG.padding = new RectOffset(PADDING_M, PADDING_M, PADDING_M, PADDING_M);
        mainHLG.spacing = SPACING_M;
        mainHLG.childControlWidth = true;
        mainHLG.childControlHeight = true;
        mainHLG.childForceExpandWidth = false;
        mainHLG.childForceExpandHeight = true;

        // --- Left Panel (Collection) ---
        GameObject leftPanel = CreatePanel(mainLayoutGO.transform, "CollectionPanel_Builder", PANEL_BG_COLOR, true, false, true);
        leftPanel.AddComponent<LayoutElement>().flexibleWidth = 0.8f;
        VerticalLayoutGroup leftVLG = leftPanel.AddComponent<VerticalLayoutGroup>();
        leftVLG.padding = new RectOffset(PADDING_S, PADDING_S, PADDING_S, PADDING_S);
        leftVLG.spacing = SPACING_S;
        leftVLG.childControlWidth = true;
        leftVLG.childControlHeight = false;
        leftVLG.childForceExpandHeight = false;

        // Top Bar for Back Button and Search
        GameObject leftTopBar = CreatePanel(leftPanel.transform, "CollectionTopBar", Color.clear, false);
        LayoutElement leftTopBarLE = leftTopBar.AddComponent<LayoutElement>();
        leftTopBarLE.minHeight = ELEMENT_HEIGHT_S;
        leftTopBarLE.preferredHeight = ELEMENT_HEIGHT_S;
        leftTopBarLE.flexibleHeight = 0;
        HorizontalLayoutGroup leftTopBarHLG = leftTopBar.AddComponent<HorizontalLayoutGroup>();
        leftTopBarHLG.spacing = SPACING_S;
        leftTopBarHLG.childControlHeight = false;
        leftTopBarHLG.childForceExpandWidth = false;

        CreateButton(leftTopBar.transform, "BackButton_Builder", "<", new Vector2(ELEMENT_HEIGHT_S, ELEMENT_HEIGHT_S));
        CreateInputField(leftTopBar.transform, "SearchBar_Collection", "Search cards...", ELEMENT_HEIGHT_S);

        GameObject collectionScrollView = CreateScrollView(leftPanel.transform, "CollectionScrollView_Builder", SCROLL_VIEW_BG_COLOR);
        LayoutElement collectionScrollLE = collectionScrollView.AddComponent<LayoutElement>();
        collectionScrollLE.flexibleHeight = 1f;
        collectionScrollLE.minHeight = 100f;

        // --- Right Panel (Deck Details) ---
        GameObject rightPanel = CreatePanel(mainLayoutGO.transform, "RightDeckPanel_Builder", PANEL_BG_COLOR, true, false, true);
        rightPanel.AddComponent<LayoutElement>().flexibleWidth = 1.2f;
        VerticalLayoutGroup rightVLG = rightPanel.AddComponent<VerticalLayoutGroup>();
        rightVLG.padding = new RectOffset(PADDING_S, PADDING_S, PADDING_S, PADDING_S);
        rightVLG.spacing = SPACING_M;
        rightVLG.childControlWidth = true;
        rightVLG.childControlHeight = false;
        rightVLG.childForceExpandHeight = false;

        // Top Controls (Deck Name Input, Save Button)
        GameObject rightTopBar = CreatePanel(rightPanel.transform, "DeckControlsBar", Color.clear, false);
        LayoutElement rightTopBarLE = rightTopBar.AddComponent<LayoutElement>();
        rightTopBarLE.minHeight = ELEMENT_HEIGHT_S;
        rightTopBarLE.preferredHeight = ELEMENT_HEIGHT_S;
        rightTopBarLE.flexibleHeight = 0;
        HorizontalLayoutGroup rightTopBarHLG = rightTopBar.AddComponent<HorizontalLayoutGroup>();
        rightTopBarHLG.spacing = SPACING_M;
        rightTopBarHLG.childControlHeight = false;
        rightTopBarHLG.childForceExpandWidth = false;

        CreateInputField(rightTopBar.transform, "DeckNameInput_Builder", "Deck name...", ELEMENT_HEIGHT_S);
        CreateButton(rightTopBar.transform, "SaveButton_Builder", "Save", new Vector2(90, ELEMENT_HEIGHT_S));

        // Main Deck Area
        GameObject mainDeckTitle = CreateText(rightPanel.transform, "MainDeck_Title", "Main Deck", 22, TextAnchor.MiddleLeft, TEXT_COLOR_LIGHT);
        LayoutElement mainDeckTitleLE = mainDeckTitle.AddComponent<LayoutElement>();
        mainDeckTitleLE.minHeight = ELEMENT_HEIGHT_S * 0.9f;
        mainDeckTitleLE.preferredHeight = ELEMENT_HEIGHT_S * 0.9f;
        mainDeckTitleLE.flexibleHeight = 0;

        CreateDeckArea(rightPanel.transform, "MainDeckArea", "Main Deck", 0.7f, new Vector2(DECK_CARD_WIDTH, DECK_CARD_HEIGHT));

        // Stage Deck Title
        GameObject stageDeckTitle = CreateText(rightPanel.transform, "StageDeck_Title", "Stage Deck", 22, TextAnchor.MiddleLeft, TEXT_COLOR_LIGHT);
        LayoutElement stageDeckTitleLE = stageDeckTitle.AddComponent<LayoutElement>();
        stageDeckTitleLE.minHeight = ELEMENT_HEIGHT_S * 0.9f;
        stageDeckTitleLE.preferredHeight = ELEMENT_HEIGHT_S * 0.9f;
        stageDeckTitleLE.flexibleHeight = 0;

        CreateDeckArea(rightPanel.transform, "StageDeckArea", "Stage Deck", 0.3f, new Vector2(DECK_CARD_WIDTH, DECK_CARD_HEIGHT));

        Debug.Log("DeckBuilderCanvas (Modern Master Duel Style) created successfully! Remember to SAVE YOUR SCENE.");
    }

    private static void CreateDeckArea(Transform parentForAreaElements, string areaNamePrefix, string titleText, float scrollViewFlexibleHeight, Vector2 cardSize)
    {
        GameObject titleTextGO = CreateText(parentForAreaElements, areaNamePrefix + "_Title", titleText, 22, TextAnchor.MiddleLeft, TEXT_COLOR_LIGHT, true);
        LayoutElement titleLE = titleTextGO.AddComponent<LayoutElement>();
        titleLE.minHeight = 35; titleLE.preferredHeight = 35; titleLE.flexibleHeight = 0;
        Text titleComp = titleTextGO.GetComponent<Text>();
        titleComp.color = Color.white; titleComp.fontStyle = FontStyle.Bold;
        GameObject deckScrollView = CreateScrollView(parentForAreaElements, areaNamePrefix + "_ScrollView", SCROLL_VIEW_BG_COLOR);
        LayoutElement svLE = deckScrollView.AddComponent<LayoutElement>(); svLE.flexibleHeight = scrollViewFlexibleHeight; 
        Image svBgImage = deckScrollView.GetComponent<Image>();
        if (svBgImage != null) { svBgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.35f); }
        ConfigureCardGrid(deckScrollView.transform.Find("Viewport/Content").gameObject, cardSize, new Vector2(8,8));
    }

    private static void ConfigureCardGrid(GameObject contentPanel, Vector2 cellSize, Vector2 spacing)
    {
        GridLayoutGroup gridLayout = contentPanel.GetComponent<GridLayoutGroup>() ?? contentPanel.AddComponent<GridLayoutGroup>();
        gridLayout.padding = new RectOffset(10, 10, 10, 10); gridLayout.spacing = spacing; gridLayout.cellSize = cellSize;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        ContentSizeFitter contentFitter = contentPanel.GetComponent<ContentSizeFitter>() ?? contentPanel.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static GameObject CreateCanvas(string name)
    {
        GameObject canvasGO = new GameObject(name);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1; // Ensure it renders on top
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        
        canvasGO.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create " + name);
        return canvasGO;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color bgColor, bool addImageComponent, bool matchParentSize = false, bool addBorder = false)
    {
        GameObject panelGO = new GameObject(name, typeof(RectTransform));
        panelGO.transform.SetParent(parent, false);
        
        RectTransform rt = panelGO.GetComponent<RectTransform>();
        if (matchParentSize)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
        
        if (addImageComponent)
        {
            Image image = panelGO.AddComponent<Image>();
            image.color = bgColor;
            image.raycastTarget = true; // Enable raycasting
            
            // Only use sliced if we have a sprite
            if (image.sprite != null)
            {
                image.type = Image.Type.Sliced;
            }
            
            // Add rounded corners if possible
            image.sprite = CreateRoundedCornerSprite(BORDER_RADIUS);
        }
        
        if (addBorder)
        {
            Outline outline = panelGO.AddComponent<Outline>();
            outline.effectColor = PANEL_BORDER_COLOR;
            outline.effectDistance = new Vector2(PANEL_BORDER_WIDTH, PANEL_BORDER_WIDTH);
        }
        
        Undo.RegisterCreatedObjectUndo(panelGO, "Create " + name);
        return panelGO;
    }

    private static GameObject CreateText(Transform parent, string name, string textContent, int fontSize, TextAnchor alignment, Color textColor, bool bold = false)
    {
        GameObject textGO = new GameObject(name, typeof(RectTransform));
        textGO.transform.SetParent(parent, false);
        Text textComp = textGO.AddComponent<Text>();
        textComp.text = textContent;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = fontSize;
        textComp.alignment = alignment;
        textComp.color = textColor;
        if (bold) textComp.fontStyle = FontStyle.Bold;
        Undo.RegisterCreatedObjectUndo(textGO, "Create " + name);
        return textGO;
    }

    private static GameObject CreateButton(Transform parent, string name, string buttonText, Vector2 size)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);
        
        // Ensure button is visible
        Image buttonImage = buttonGO.GetComponent<Image>();
        buttonImage.color = BUTTON_BG_COLOR;
        buttonImage.raycastTarget = true;
        
        // Add rounded corners if possible
        buttonImage.sprite = CreateRoundedCornerSprite(BUTTON_BORDER_RADIUS);
        
        // Add hover effect
        Button buttonComp = buttonGO.GetComponent<Button>();
        ColorBlock colors = buttonComp.colors;
        colors.normalColor = BUTTON_BG_COLOR;
        colors.highlightedColor = BUTTON_HIGHLIGHT;
        colors.pressedColor = new Color(BUTTON_HIGHLIGHT.r * 0.9f, BUTTON_HIGHLIGHT.g * 0.9f, BUTTON_HIGHLIGHT.b * 0.9f, 1f);
        colors.selectedColor = BUTTON_HIGHLIGHT;
        colors.fadeDuration = 0.1f;
        buttonComp.colors = colors;
        
        RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
        buttonRT.sizeDelta = size;
        
        // Make button text visible and properly sized
        int fontSize = Mathf.Max(16, Mathf.RoundToInt(size.y * 0.5f));
        GameObject textGO = CreateText(buttonGO.transform, "ButtonText", buttonText, fontSize, TextAnchor.MiddleCenter, TEXT_COLOR_LIGHT, true);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        textRT.anchoredPosition = Vector2.zero;
        
        // Ensure text is visible
        Text textComp = textGO.GetComponent<Text>();
        textComp.color = Color.white;
        
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
        // Add rounded corners
        bgImage.sprite = CreateRoundedCornerSprite(INPUT_BORDER_RADIUS);

        InputField inputField = inputFieldGO.GetComponent<InputField>();

        int fontSize = Mathf.Max(12, Mathf.RoundToInt(height * 0.5f));

        GameObject placeholderGO = CreateText(inputFieldGO.transform, "Placeholder", placeholderText, fontSize, TextAnchor.MiddleLeft, PLACEHOLDER_COLOR);
        Text placeholderTextComp = placeholderGO.GetComponent<Text>();
        placeholderTextComp.fontStyle = FontStyle.Italic;
        RectTransform placeholderRT = placeholderGO.GetComponent<RectTransform>();
        placeholderRT.anchorMin = Vector2.zero; placeholderRT.anchorMax = Vector2.one;
        placeholderRT.sizeDelta = new Vector2(-PADDING_M, 0);
        placeholderRT.anchoredPosition = new Vector2(PADDING_S, 0);

        GameObject textCompGO = CreateText(inputFieldGO.transform, "Text", "", fontSize, TextAnchor.MiddleLeft, TEXT_COLOR_LIGHT);
        RectTransform textRT = textCompGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = new Vector2(-PADDING_M, 0);
        textRT.anchoredPosition = new Vector2(PADDING_S, 0);
        inputField.textComponent = textCompGO.GetComponent<Text>();
        inputField.placeholder = placeholderTextComp;

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

        GameObject viewportGO = CreatePanel(scrollViewGO.transform, "Viewport", Color.clear, false);
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;
        RectTransform viewportRT = viewportGO.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero; viewportRT.anchorMax = Vector2.one;
        viewportRT.sizeDelta = new Vector2(-PADDING_S, -PADDING_S * 2);
        viewportRT.anchoredPosition = Vector2.zero;
        viewportRT.pivot = new Vector2(0, 1);

        GameObject contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);

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

    private static GameObject CreateScrollbar(Transform parent, string name, Scrollbar.Direction direction)
    {
        GameObject scrollbarGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        scrollbarGO.transform.SetParent(parent, false);
        Image sbImage = scrollbarGO.GetComponent<Image>(); sbImage.color = new Color(0.7f,0.7f,0.7f, 0.3f);
        Scrollbar scrollbar = scrollbarGO.GetComponent<Scrollbar>(); scrollbar.direction = direction;
        GameObject slidingAreaGO = new GameObject("SlidingArea", typeof(RectTransform));
        slidingAreaGO.transform.SetParent(scrollbarGO.transform, false);
        SetAnchorsAndPivots(slidingAreaGO.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
        GameObject handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGO.transform.SetParent(slidingAreaGO.transform, false);
        Image handleImage = handleGO.GetComponent<Image>(); handleImage.color = new Color(0.4f,0.4f,0.4f, 1f); // Darker handle
        scrollbar.targetGraphic = handleImage; scrollbar.handleRect = handleGO.GetComponent<RectTransform>();
        RectTransform handleRT = handleGO.GetComponent<RectTransform>();
        if (direction == Scrollbar.Direction.BottomToTop || direction == Scrollbar.Direction.TopToBottom)
        {
            SetAnchorsAndPivots(scrollbarGO.GetComponent<RectTransform>(), new Vector2(1,0), Vector2.one, new Vector2(1,0.5f)); 
            scrollbarGO.GetComponent<RectTransform>().sizeDelta = new Vector2(15,0);
            handleRT.sizeDelta = new Vector2(0,0); 
        }
        else 
        {
            SetAnchorsAndPivots(scrollbarGO.GetComponent<RectTransform>(), new Vector2(0,0), new Vector2(1,0), new Vector2(0.5f,0)); 
            scrollbarGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0,15);
            handleRT.sizeDelta = new Vector2(0,0); 
        }
        return scrollbarGO;
    }

    private static void SetAnchorsAndPivots(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition; rt.sizeDelta = sizeDelta;
    }
    private static void SetAnchorsAndPivots(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
    }

    // Add new helper method for creating rounded corner sprites
    private static Sprite CreateRoundedCornerSprite(float radius)
    {
        // Create a 9-sliced sprite with rounded corners
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dx = Mathf.Min(x, 31 - x);
                float dy = Mathf.Min(y, 31 - y);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distance < radius)
                {
                    colors[y * 32 + x] = Color.white;
                }
                else
                {
                    colors[y * 32 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
} 