using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUIGenerator : EditorWindow
{
    private static readonly Vector2 REFERENCE_RESOLUTION = new Vector2(1920, 1080);
    private static readonly Color BG_COLOR = new Color(0.12f, 0.08f, 0.13f, 1f);
    private static readonly Color PANEL_COLOR = new Color(0.18f, 0.13f, 0.18f, 0.95f);
    private static readonly Color BUTTON_COLOR = new Color(0.25f, 0.21f, 0.28f, 1f);
    private static readonly Color TEXT_COLOR = Color.white;
    private static readonly Color SUBTLE_TEXT_COLOR = new Color(0.7f, 0.7f, 0.8f, 1f);

    [MenuItem("Tools/Deck Building/SETUP - Create Main Menu UI Canvas")]
    public static void CreateMainMenuUICanvas()
    {
        // --- Canvas Setup ---
        GameObject canvasGO = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;

        // --- Background ---
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bg.GetComponent<Image>();
        bgImg.color = BG_COLOR;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;

        // --- Top Bar ---
        GameObject topBar = CreatePanel(canvasGO.transform, "TopBar", PANEL_COLOR, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -0), new Vector2(0, 100));
        HorizontalLayoutGroup topBarHLG = topBar.AddComponent<HorizontalLayoutGroup>();
        topBarHLG.padding = new RectOffset(32, 32, 0, 0);
        topBarHLG.spacing = 32;
        topBarHLG.childAlignment = TextAnchor.MiddleCenter;
        topBarHLG.childForceExpandWidth = true;
        topBarHLG.childForceExpandHeight = true;

        // --- Settings Icon (Top Left) ---
        GameObject settingsBtn = CreateIconButton(topBar.transform, "SettingsButton", 48, "Settings Icon SVG here");
        // TODO: Assign SVG to Image component

        // --- Menu Buttons (Play, Deck, Solo, Shop) ---
        GameObject menuBtnsPanel = new GameObject("MenuButtonsPanel", typeof(RectTransform));
        menuBtnsPanel.transform.SetParent(topBar.transform, false);
        HorizontalLayoutGroup menuBtnsHLG = menuBtnsPanel.AddComponent<HorizontalLayoutGroup>();
        menuBtnsHLG.spacing = 24;
        menuBtnsHLG.childAlignment = TextAnchor.MiddleCenter;
        menuBtnsHLG.childForceExpandWidth = false;
        menuBtnsHLG.childForceExpandHeight = false;
        CreateMenuButton(menuBtnsPanel.transform, "Play");
        CreateMenuButton(menuBtnsPanel.transform, "Deck");
        CreateMenuButton(menuBtnsPanel.transform, "Solo");
        CreateMenuButton(menuBtnsPanel.transform, "Shop");

        // --- Spacer ---
        GameObject spacer = new GameObject("TopBarSpacer", typeof(RectTransform));
        spacer.transform.SetParent(topBar.transform, false);
        LayoutElement spacerLE = spacer.AddComponent<LayoutElement>();
        spacerLE.flexibleWidth = 1;

        // --- Currency Panel (Top Right) ---
        GameObject currencyPanel = new GameObject("CurrencyPanel", typeof(RectTransform));
        currencyPanel.transform.SetParent(topBar.transform, false);
        HorizontalLayoutGroup currencyHLG = currencyPanel.AddComponent<HorizontalLayoutGroup>();
        currencyHLG.spacing = 16;
        currencyHLG.childAlignment = TextAnchor.MiddleRight;
        currencyHLG.childForceExpandWidth = false;
        currencyHLG.childForceExpandHeight = false;
        CreateCurrencyDisplay(currencyPanel.transform, "Gems", "100");
        CreateCurrencyDisplay(currencyPanel.transform, "Souls", "100");

        // --- Main Layout (Below Top Bar) ---
        GameObject mainLayout = new GameObject("MainLayout", typeof(RectTransform));
        mainLayout.transform.SetParent(canvasGO.transform, false);
        RectTransform mainLayoutRT = mainLayout.GetComponent<RectTransform>();
        mainLayoutRT.anchorMin = new Vector2(0, 0);
        mainLayoutRT.anchorMax = new Vector2(1, 1);
        mainLayoutRT.offsetMin = new Vector2(0, 0);
        mainLayoutRT.offsetMax = new Vector2(0, -100); // below top bar
        mainLayoutRT.pivot = new Vector2(0.5f, 0.5f);
        HorizontalLayoutGroup mainHLG = mainLayout.AddComponent<HorizontalLayoutGroup>();
        mainHLG.padding = new RectOffset(32, 32, 32, 32);
        mainHLG.spacing = 32;
        mainHLG.childAlignment = TextAnchor.MiddleCenter;
        mainHLG.childForceExpandWidth = true;
        mainHLG.childForceExpandHeight = true;

        // --- SideQuests (Left) ---
        GameObject sideQuestsPanel = CreatePanel(mainLayout.transform, "SideQuestsPanel", PANEL_COLOR, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(32, 0), new Vector2(340, 280));
        VerticalLayoutGroup questsVLG = sideQuestsPanel.AddComponent<VerticalLayoutGroup>();
        questsVLG.spacing = 16;
        questsVLG.padding = new RectOffset(16, 16, 16, 16);
        questsVLG.childAlignment = TextAnchor.UpperLeft;
        questsVLG.childForceExpandWidth = true;
        questsVLG.childForceExpandHeight = false;
        for (int i = 0; i < 4; i++)
            CreateSideQuest(sideQuestsPanel.transform, i == 2); // Mark 3rd as completed for demo

        // --- Bottom Bar (DeckBox, Play, Rank) ---
        GameObject bottomBarPanel = CreatePanel(canvasGO.transform, "BottomBarPanel", Color.clear, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(600, 160));
        HorizontalLayoutGroup bottomBarHLG = bottomBarPanel.AddComponent<HorizontalLayoutGroup>();
        bottomBarHLG.spacing = 32;
        bottomBarHLG.childAlignment = TextAnchor.LowerCenter;
        bottomBarHLG.childForceExpandWidth = false;
        bottomBarHLG.childForceExpandHeight = false;
        CreateDeckBox(bottomBarPanel.transform);
        CreatePlayButton(bottomBarPanel.transform);
        CreateRankBox(bottomBarPanel.transform);

        // --- Latest News/Announcement (Right, Top) ---
        GameObject newsPanel = CreatePanel(canvasGO.transform, "NewsPanel", PANEL_COLOR, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -140), new Vector2(340, 220));
        VerticalLayoutGroup newsVLG = newsPanel.AddComponent<VerticalLayoutGroup>();
        newsVLG.spacing = 16;
        newsVLG.padding = new RectOffset(16, 16, 16, 16);
        newsVLG.childAlignment = TextAnchor.UpperCenter;
        CreateAnnouncementBox(newsPanel.transform);

        Debug.Log("Main Menu UI Canvas created! Assign SVGs and TMP font assets as needed.");
    }

    // --- Helper Methods ---

    private static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image img = panel.GetComponent<Image>();
        img.color = color;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
        return panel;
    }

    private static GameObject CreateIconButton(Transform parent, string name, float size, string tooltip)
    {
        GameObject btn = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent, false);
        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);
        // TODO: Assign SVG to Image component here
        // Tooltip: tooltip
        return btn;
    }

    private static GameObject CreateMenuButton(Transform parent, string label)
    {
        GameObject btn = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent, false);
        Image img = btn.GetComponent<Image>();
        img.color = BUTTON_COLOR;
        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 48);
        // TextMeshPro label
        GameObject textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(btn.transform, false);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label.ToUpper();
        tmp.fontSize = 32;
        tmp.color = TEXT_COLOR;
        tmp.alignment = TextAlignmentOptions.Center;
        // TODO: Assign custom TMP font asset if needed
        return btn;
    }

    private static void CreateCurrencyDisplay(Transform parent, string type, string amount)
    {
        GameObject panel = new GameObject(type + "Display", typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = panel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 4;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        // Icon
        GameObject icon = new GameObject(type + "Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(panel.transform, false);
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(32, 32);
        // TODO: Assign SVG to Image component
        // Amount
        GameObject textGO = new GameObject("Amount", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(panel.transform, false);
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = amount;
        tmp.fontSize = 28;
        tmp.color = TEXT_COLOR;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        // TODO: Assign custom TMP font asset if needed
    }

    private static void CreateSideQuest(Transform parent, bool completed = false)
    {
        GameObject quest = new GameObject("SideQuest", typeof(RectTransform), typeof(Image), typeof(Button));
        quest.transform.SetParent(parent, false);
        Image img = quest.GetComponent<Image>();
        img.color = completed ? new Color(0.18f, 0.35f, 0.18f, 1f) : BUTTON_COLOR;
        RectTransform rt = quest.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(340, 48);
        HorizontalLayoutGroup questHLG = quest.AddComponent<HorizontalLayoutGroup>();
        questHLG.childAlignment = TextAnchor.MiddleLeft;
        questHLG.childForceExpandWidth = false;
        questHLG.childForceExpandHeight = false;
        questHLG.spacing = 12;
        questHLG.padding = new RectOffset(8, 8, 4, 4);
        // Icon (optional: add a checkmark or X for completed)
        GameObject icon = new GameObject("QuestIcon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(quest.transform, false);
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(24, 24);
        // TODO: Assign SVG for quest icon (e.g., checkmark or X)
        // Text
        GameObject textGO = new GameObject("QuestText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(quest.transform, false);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = completed ? "Kill minions (5/5)" : "Kill minions (0/5)";
        tmp.fontSize = 22;
        tmp.color = completed ? SUBTLE_TEXT_COLOR : TEXT_COLOR;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        LayoutElement textLE = textGO.AddComponent<LayoutElement>();
        textLE.flexibleWidth = 1;
        // TODO: Assign custom TMP font asset if needed
    }

    private static void CreateDeckBox(Transform parent)
    {
        GameObject deckBox = new GameObject("DeckBox", typeof(RectTransform), typeof(Image), typeof(Button));
        deckBox.transform.SetParent(parent, false);
        Image img = deckBox.GetComponent<Image>();
        img.color = BUTTON_COLOR;
        RectTransform rt = deckBox.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 160);
        // TODO: Assign SVG for deck box image
        // Label
        GameObject textGO = new GameObject("DeckLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(deckBox.transform, false);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 0.3f);
        textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "Untitled 1";
        tmp.fontSize = 20;
        tmp.color = TEXT_COLOR;
        tmp.alignment = TextAlignmentOptions.Bottom;
        // TODO: Assign custom TMP font asset if needed
    }

    private static void CreatePlayButton(Transform parent)
    {
        GameObject playBtn = new GameObject("PlayButton", typeof(RectTransform), typeof(Image), typeof(Button));
        playBtn.transform.SetParent(parent, false);
        Image img = playBtn.GetComponent<Image>();
        img.color = new Color(0.8f, 0.65f, 0.3f, 1f);
        RectTransform rt = playBtn.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 80);
        // Label
        GameObject textGO = new GameObject("PlayLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(playBtn.transform, false);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "PLAY";
        tmp.fontSize = 40;
        tmp.color = TEXT_COLOR;
        tmp.alignment = TextAlignmentOptions.Center;
        // TODO: Assign custom TMP font asset if needed
    }

    private static void CreateRankBox(Transform parent)
    {
        GameObject rankBox = new GameObject("RankBox", typeof(RectTransform), typeof(Image));
        rankBox.transform.SetParent(parent, false);
        Image img = rankBox.GetComponent<Image>();
        img.color = BUTTON_COLOR;
        RectTransform rt = rankBox.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 60);
        // TODO: Assign SVG for rank icon
        // Label
        GameObject textGO = new GameObject("RankLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(rankBox.transform, false);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "LVL 1\nNovice";
        tmp.fontSize = 22;
        tmp.color = TEXT_COLOR;
        tmp.alignment = TextAlignmentOptions.Center;
        // TODO: Assign custom TMP font asset if needed
    }

    private static void CreateAnnouncementBox(Transform parent)
    {
        GameObject annBtn = new GameObject("AnnouncementButton", typeof(RectTransform), typeof(Image), typeof(Button));
        annBtn.transform.SetParent(parent, false);
        Image img = annBtn.GetComponent<Image>();
        img.color = new Color(0.18f, 0.25f, 0.35f, 1f);
        RectTransform rt = annBtn.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 200);
        // TODO: Assign SVG for announcement image
        // Label
        GameObject textGO = new GameObject("AnnouncementLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(annBtn.transform, false);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 0.3f);
        textRT.offsetMin = new Vector2(8, 8); textRT.offsetMax = new Vector2(-8, 0);
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "Announcement!\nJoin Our Discord — stay in touch with the updates and community!";
        tmp.fontSize = 18;
        tmp.color = TEXT_COLOR;
        tmp.alignment = TextAlignmentOptions.BottomLeft;
        // TODO: Assign custom TMP font asset if needed
        // Add click event to open Discord link
        Button btn = annBtn.GetComponent<Button>();
#if UNITY_EDITOR
        btn.onClick.AddListener(() => Application.OpenURL("https://discord.com/invite/WGjZa3CXAw"));
#endif
    }
}

// --- HOW TO USE CUSTOM FONTS WITH TEXTMESHPRO ---
// 1. Import your .ttf or .otf font file into Assets/Fonts/.
// 2. Right-click the font file in Unity and select "Create > TextMeshPro > Font Asset".
// 3. Assign the created TMP Font Asset to the TextMeshProUGUI components in the inspector or via script.
//    (In this script, you can assign it by getting the TextMeshProUGUI component and setting its .font property.)
// 4. For SVGs, assign your SVG sprite to the Image component where marked. 