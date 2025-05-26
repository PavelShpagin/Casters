using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.Rendering;

public class GameBoardGenerator
{
    private const string CARD_BACK_PATH = "Sprites/CardBack/cardback-casters";

    [MenuItem("Tools/Generate GameBoard")]
    public static void GenerateGameBoard()
    {
        ClearExistingBoard();

        // Create main board plane
        GameObject boardPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        boardPlane.name = "GameBoardPlane";
        boardPlane.transform.localScale = new Vector3(20, 1, 30);
        boardPlane.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.2f, 0.3f, 0.2f));

        // Setup camera
        SetupCamera();

        // Create board root
        GameObject boardRoot = new GameObject("GameBoardRoot");

        // Create both player areas
        CreatePlayerArea(boardRoot.transform, "Player1", new Vector3(0, 0, -50), Quaternion.identity);
        CreatePlayerArea(boardRoot.transform, "Player2", new Vector3(0, 0, 50), Quaternion.Euler(0, 180, 0));

        // Create center elements
        CreateCenterLine(boardRoot.transform);
        CreateBigPhaseBanner(boardRoot.transform, "SET PHASE");
        CreateReadyButton(boardRoot.transform);
        CreateInitiativeIndicator(boardRoot.transform, "Player1");

        // Create card prefab
        CreateAndSaveCardPrefab();

        Debug.Log("🎯 COMPLETE GameBoard Generated!");
        Debug.Log("✅ Deck Layouts: Cards stack from bottom-up");
        Debug.Log("✅ Stage Rows: 5 centered slots with card holders");
        Debug.Log("✅ Minion Rows: 7 centered slots with card holders");
        Debug.Log("✅ Hand Arcs: Natural Yu-Gi-Oh! style positioning");
        Debug.Log("✅ Graveyard: Positioned above main decks");
        EditorUtility.DisplayDialog("GameBoard Complete!", "All card layouts, deck stacking, and hand arcs created successfully!", "Awesome!");
    }

    static void CreatePlayerArea(Transform parent, string playerName, Vector3 position, Quaternion rotation)
    {
        GameObject playerArea = new GameObject($"{playerName}_Area");
        playerArea.transform.SetParent(parent);
        playerArea.transform.localPosition = position;
        playerArea.transform.localRotation = rotation;

        float yOffset = 0.5f;

        // Create Health Display
        CreateHealthDisplay(playerArea.transform, playerName, new Vector3(45f, yOffset + 1f, -15f));

        // Create deck zones with card stacking
        CreateDeckZone(playerArea.transform, $"{playerName}_MainDeck", new Vector3(40f, yOffset, 0), 30, true);
        CreateDeckZone(playerArea.transform, $"{playerName}_StageDeck", new Vector3(-40f, yOffset, 0), 5, true);
        CreateGraveyardZone(playerArea.transform, $"{playerName}_Graveyard", new Vector3(40f, yOffset, 20f));

        // Create battle field card rows
        CreateCardRow(playerArea.transform, $"{playerName}_StageRow", new Vector3(0, yOffset, -15f), 5, 12f, "Stage");
        CreateCardRow(playerArea.transform, $"{playerName}_MinionRow", new Vector3(0, yOffset, -30f), 7, 10f, "Minion");

        // Create hand arc
        CreateHandArc(playerArea.transform, $"{playerName}_Hand", new Vector3(0, yOffset, 10f), 8);
    }

    static void CreateDeckZone(Transform parent, string name, Vector3 position, int cardCount, bool faceDown)
    {
        GameObject deckZone = new GameObject(name);
        deckZone.transform.SetParent(parent);
        deckZone.transform.localPosition = position;

        // Visual deck base (simple cube)
        GameObject deckBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deckBase.name = "DeckBase";
        deckBase.transform.SetParent(deckZone.transform);
        deckBase.transform.localPosition = Vector3.zero;
        deckBase.transform.localScale = new Vector3(7.5f, 1f, 10.5f); // Card width + buffer, thickness, card height + buffer
        deckBase.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.3f, 0.3f, 0.3f));
        Object.DestroyImmediate(deckBase.GetComponent<Collider>()); // Remove collider from base if not needed for interaction

        // Single card visual on top of the deck base
        GameObject topCardVisual = CreateCard($"TopCardVisual_{name}", faceDown); // True for faceDown (card back)
        topCardVisual.transform.SetParent(deckZone.transform); // Parent to deckZone for correct positioning relative to base
        // Position slightly above the deck base. CreateCard already creates a card with thickness.
        // The y position of cardBody inside CreateCard is 0, so we need to lift the whole card object.
        float cardThickness = 0.25f; // From CreateCard
        topCardVisual.transform.localPosition = new Vector3(0, (1f / 2f) + (cardThickness / 2f) + 0.01f, 0); // (base height/2) + (card thickness/2) + tiny offset
        topCardVisual.transform.localRotation = Quaternion.identity;

        // Text for card count
        GameObject textObj = new GameObject("DeckCountText");
        textObj.transform.SetParent(deckZone.transform);
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = cardCount.ToString();
        tmp.fontSize = 10;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        // Position text below the deck base or on its side, adjust as needed
        tmp.rectTransform.localPosition = new Vector3(0, 0, -6f); // Example: front face of the deck base
        tmp.rectTransform.localRotation = Quaternion.Euler(0, 0, 0); // Orient text if needed
        tmp.rectTransform.sizeDelta = new Vector2(7f, 1.5f);
        tmp.color = Color.white;

        Debug.Log($"Created {name} with top card visual and count: {cardCount}");
    }

    static void CreateGraveyardZone(Transform parent, string name, Vector3 position)
    {
        GameObject graveyardZone = new GameObject(name);
        graveyardZone.transform.SetParent(parent);
        graveyardZone.transform.localPosition = position;

        // Create very thin graveyard base
        GameObject graveyardBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        graveyardBase.name = "GraveyardBase";
        graveyardBase.transform.SetParent(graveyardZone.transform);
        graveyardBase.transform.localPosition = Vector3.zero;
        graveyardBase.transform.localScale = new Vector3(8f, 0.1f, 11f); // Slightly larger than card footprint, very thin
        graveyardBase.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.25f, 0.25f, 0.25f)); // Dark grey
        Object.DestroyImmediate(graveyardBase.GetComponent<Collider>());

        // Create a Quad for the graveyard image/visual on top of the base
        GameObject graveyardVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        graveyardVisual.name = "GraveyardVisualRepresentation";
        graveyardVisual.transform.SetParent(graveyardZone.transform); // Parent to graveyardZone for correct positioning
        // Position slightly above the base
        graveyardVisual.transform.localPosition = new Vector3(0, (0.1f / 2f) + 0.01f, 0); // (base height/2) + tiny offset
        graveyardVisual.transform.localRotation = Quaternion.Euler(90, 0, 0); // Lay flat
        // Make it slightly smaller than the base, to look like it's on top
        float cardWidth = 7f; // from CreateCard
        float cardHeight = cardWidth * 1.4f; // from CreateCard
        graveyardVisual.transform.localScale = new Vector3(cardWidth, cardHeight, 1f); 
        
        Material gyMat = CreateMaterial(new Color(0.5f, 0.5f, 0.5f)); // Medium Grey - placeholder
        // TODO: Load a specific texture for the graveyard image here if desired:
        // Texture2D gyTexture = Resources.Load<Texture2D>("Sprites/GraveyardPlaceholder");
        // if (gyTexture != null) gyMat.mainTexture = gyTexture;
        graveyardVisual.GetComponent<Renderer>().sharedMaterial = gyMat;
        Object.DestroyImmediate(graveyardVisual.GetComponent<Collider>());

        Debug.Log($"Created {name} with a thin base and a visual Quad.");
    }

    static void CreateCardRow(Transform parent, string name, Vector3 position, int slotCount, float spacing, string cardType)
    {
        GameObject cardRow = new GameObject(name);
        cardRow.transform.SetParent(parent);
        cardRow.transform.localPosition = position;

        // Create row base (invisible reference)
        GameObject rowBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rowBase.name = "RowBase";
        rowBase.transform.SetParent(cardRow.transform);
        rowBase.transform.localPosition = Vector3.zero;
        rowBase.transform.localScale = new Vector3(slotCount * spacing + 5f, 0.1f, 8f);
        rowBase.GetComponent<Renderer>().enabled = false; // Invisible
        rowBase.GetComponent<Collider>().isTrigger = true;

        // Calculate centered positions
        float startX = -(slotCount - 1) * spacing * 0.5f;

        // Create card slots
        for (int i = 0; i < slotCount; i++)
        {
            GameObject cardSlot = new GameObject($"CardSlot_{i:00}");
            cardSlot.transform.SetParent(cardRow.transform);
            cardSlot.transform.localPosition = new Vector3(startX + (i * spacing), 0.5f, 0);

            // Create slot indicator
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "SlotIndicator";
            indicator.transform.SetParent(cardSlot.transform);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = new Vector3(2f, 0.05f, 2f);
            
            Color slotColor = cardType == "Stage" ? 
                new Color(0.8f, 0.6f, 0.2f, 0.7f) : 
                new Color(0.2f, 0.6f, 0.8f, 0.7f);
            indicator.GetComponent<Renderer>().sharedMaterial = CreateMaterial(slotColor);
            Object.DestroyImmediate(indicator.GetComponent<Collider>());

            // Add placeholder cards to first few slots
            if (i < 3)
            {
                GameObject placeholderCard = CreateCard($"Placeholder_{cardType}_{i}", false);
                placeholderCard.transform.SetParent(cardSlot.transform);
                placeholderCard.transform.localPosition = new Vector3(0, 1f, 0);
            }
        }

        Debug.Log($"Created {name}: {slotCount} slots with {spacing} spacing (centered layout)");
    }

    static void CreateHandArc(Transform parent, string name, Vector3 position, int maxCards)
    {
        GameObject handArc = new GameObject(name);
        handArc.transform.SetParent(parent);
        handArc.transform.localPosition = position;

        float arcRadius = 60f;
        float arcAngle = 40f;
        float angleStep = arcAngle / (maxCards - 1);
        float startAngle = -arcAngle * 0.5f;

        // Create hand slots in arc formation
        for (int i = 0; i < maxCards; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            float radians = currentAngle * Mathf.Deg2Rad;

            Vector3 slotPosition = new Vector3(
                Mathf.Sin(radians) * arcRadius,
                0.5f,
                Mathf.Cos(radians) * arcRadius - arcRadius + 12f
            );

            GameObject handSlot = new GameObject($"HandSlot_{i:00}");
            handSlot.transform.SetParent(handArc.transform);
            handSlot.transform.localPosition = slotPosition;
            handSlot.transform.localRotation = Quaternion.Euler(0, -currentAngle, 0);

            // Add cards to first 5 slots for demonstration
            if (i < 5)
            {
                GameObject handCard = CreateCard($"HandCard_{i}", false);
                handCard.transform.SetParent(handSlot.transform);
                handCard.transform.localPosition = new Vector3(0, 1f, 0);
                handCard.transform.localRotation = Quaternion.Euler(-10, 0, 0); // Slight tilt
            }
        }

        Debug.Log($"Created {name}: {maxCards} hand slots in natural arc formation");
    }

    static GameObject CreateCard(string cardName, bool faceDown)
    {
        GameObject cardObj = new GameObject(cardName);

        // Card dimensions
        float cardWidth = 7f;
        float cardHeight = cardWidth * 1.4f; // Standard card ratio
        float cardThickness = 0.25f;

        // Create card body (thin box)
        GameObject cardBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cardBody.name = "CardBody";
        cardBody.transform.SetParent(cardObj.transform);
        cardBody.transform.localPosition = Vector3.zero;
        cardBody.transform.localScale = new Vector3(cardWidth, cardThickness, cardHeight);
        cardBody.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.1f, 0.1f, 0.1f));

        // Create front face (top)
        GameObject frontFace = GameObject.CreatePrimitive(PrimitiveType.Quad);
        frontFace.name = "FrontFace";
        frontFace.transform.SetParent(cardObj.transform);
        frontFace.transform.localPosition = new Vector3(0, cardThickness * 0.5f + 0.01f, 0);
        frontFace.transform.localRotation = Quaternion.Euler(90, 0, 0);
        frontFace.transform.localScale = new Vector3(cardWidth, cardHeight, 1);
        
        Material frontMat = CreateMaterial(Color.white);
        Texture2D cardTexture = Resources.Load<Texture2D>("Sprites/Cards/Annihilation");
        if (cardTexture != null) frontMat.mainTexture = cardTexture;
        frontFace.GetComponent<Renderer>().sharedMaterial = frontMat;
        frontFace.GetComponent<Renderer>().enabled = !faceDown;
        Object.DestroyImmediate(frontFace.GetComponent<Collider>());

        // Create back face (bottom)
        GameObject backFace = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backFace.name = "BackFace";
        backFace.transform.SetParent(cardObj.transform);
        backFace.transform.localPosition = new Vector3(0, -cardThickness * 0.5f - 0.01f, 0);
        backFace.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        backFace.transform.localScale = new Vector3(cardWidth, cardHeight, 1);
        
        Material backMat = CreateMaterial(new Color(0.2f, 0.2f, 0.5f));
        Texture2D backTexture = Resources.Load<Texture2D>(CARD_BACK_PATH);
        if (backTexture != null) backMat.mainTexture = backTexture;
        backFace.GetComponent<Renderer>().sharedMaterial = backMat;
        backFace.GetComponent<Renderer>().enabled = faceDown;
        Object.DestroyImmediate(backFace.GetComponent<Collider>());

        // Add collider for interaction
        BoxCollider cardCollider = cardObj.AddComponent<BoxCollider>();
        cardCollider.size = new Vector3(cardWidth, cardThickness * 2, cardHeight);
        cardCollider.isTrigger = true;

        return cardObj;
    }

    static void CreateCenterLine(Transform parent)
    {
        GameObject centerLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        centerLine.name = "CenterLine";
        centerLine.transform.SetParent(parent);
        centerLine.transform.localPosition = new Vector3(0, 0.5f, 0);
        centerLine.transform.localScale = new Vector3(100f, 0.2f, 0.8f);
        centerLine.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.8f, 0.8f, 0.2f));
        Object.DestroyImmediate(centerLine.GetComponent<Collider>());
    }

    static void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            mainCamera = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        mainCamera.transform.position = new Vector3(0, 100, -65);
        mainCamera.transform.rotation = Quaternion.Euler(65, 0, 0);
        mainCamera.fieldOfView = 60;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.45f, 0.45f, 0.45f);
    }

    static Material CreateMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null)
        {
            // Fallback or error handling if URP Lit is not found
            Debug.LogError("Universal Render Pipeline/Lit shader not found. Falling back to Standard.");
            mat = new Material(Shader.Find("Standard"));
        }
        mat.color = color;
        return mat;
    }

    static void CreateAndSaveCardPrefab()
    {
        string prefabFolder = "Assets/Prefabs/Generated";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Generated");
        }
        string prefabPath = prefabFolder + "/CardPrefab.prefab";

        GameObject cardPrefab = CreateCard("CardPrefab", false);
        PrefabUtility.SaveAsPrefabAssetAndConnect(cardPrefab, prefabPath, InteractionMode.UserAction);
        Object.DestroyImmediate(cardPrefab);
        Debug.Log("Card prefab saved to " + prefabPath);
    }

    static void ClearExistingBoard()
    {
        GameObject[] toDelete = {
            GameObject.Find("GameBoardRoot"),
            GameObject.Find("GameBoardPlane"),
            GameObject.Find("UICanvas_Generated")
        };

        foreach (GameObject obj in toDelete)
        {
            if (obj != null) Object.DestroyImmediate(obj);
        }
    }

    private static void CreateBigPhaseBanner(Transform parent, string text)
    {
        GameObject bannerObject = new GameObject("PhaseBanner");
        bannerObject.transform.SetParent(parent);
        bannerObject.transform.localPosition = new Vector3(0, 1f, 0); // Centered
        bannerObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // Create Banner Background (e.g., a styled Quad or multiple Quads)
        GameObject bannerBg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bannerBg.name = "BannerBackground";
        bannerBg.transform.SetParent(bannerObject.transform);
        bannerBg.transform.localPosition = Vector3.zero;
        bannerBg.transform.localRotation = Quaternion.Euler(90, 0, 0); // Lay flat, text will be on top
        bannerBg.transform.localScale = new Vector3(80f, 20f, 1f); // Wide banner
        Material bannerMat = CreateMaterial(new Color(0.1f, 0.5f, 0.8f, 0.9f)); // Blue, slightly transparent
        bannerBg.GetComponent<Renderer>().sharedMaterial = bannerMat;
        Object.DestroyImmediate(bannerBg.GetComponent<Collider>());

        // Add Text to Banner
        GameObject textObj = new GameObject("BannerText");
        textObj.transform.SetParent(bannerObject.transform); // Attach to bannerObject for correct orientation
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = text.ToUpper();
        tmp.fontSize = 30;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.rectTransform.localPosition = new Vector3(0, 0.1f, 0); // Slightly above the quad
        tmp.rectTransform.localRotation = Quaternion.Euler(90, 0, 0); // Orient text to be readable from camera view
        tmp.rectTransform.sizeDelta = new Vector2(75f, 18f);
        tmp.enableWordWrapping = false;
    }

    private static void CreateReadyButton(Transform parent) // Renamed and to be styled
    {
        GameObject readyButtonRoot = new GameObject("ReadyButtonRoot");
        readyButtonRoot.transform.SetParent(parent);
        // Position based on image: right side, near player 1's perspective
        readyButtonRoot.transform.localPosition = new Vector3(60f, 1f, -25f); 
        readyButtonRoot.transform.localRotation = Quaternion.identity;

        GameObject buttonMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        buttonMesh.name = "ReadyButtonMesh";
        buttonMesh.transform.SetParent(readyButtonRoot.transform);
        buttonMesh.transform.localPosition = Vector3.zero;
        buttonMesh.transform.localScale = new Vector3(15f, 2f, 7f); // Adjusted scale
        buttonMesh.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.2f, 0.8f, 0.2f)); // Green
        buttonMesh.GetComponent<Collider>().isTrigger = true;

        GameObject textObj = new GameObject("ReadyButtonText");
        textObj.transform.SetParent(readyButtonRoot.transform); 
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = "READY";
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.rectTransform.localPosition = new Vector3(0, 0.05f, 0); // Centered on the button face
        tmp.rectTransform.localRotation = Quaternion.Euler(90, 0, 0); // Lay flat on the button's top face
        tmp.rectTransform.sizeDelta = new Vector2(14f, 6f);
    }

    private static void CreateInitiativeIndicator(Transform parent, string owningPlayer)
    {
        GameObject initiativeRoot = new GameObject($"{owningPlayer}_InitiativeIndicator");
        initiativeRoot.transform.SetParent(parent);
        // Position near player 1's side, below ready button from image perspective
        initiativeRoot.transform.localPosition = new Vector3(45f, 0.6f, -35f); 

        GameObject indicatorSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicatorSphere.name = "IndicatorVisual";
        indicatorSphere.transform.SetParent(initiativeRoot.transform);
        indicatorSphere.transform.localPosition = Vector3.zero;
        indicatorSphere.transform.localScale = new Vector3(3f, 3f, 3f);
        indicatorSphere.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.3f, 0.7f, 0.3f)); // Greenish
        Object.DestroyImmediate(indicatorSphere.GetComponent<Collider>());

        GameObject textObj = new GameObject("InitiativeText");
        textObj.transform.SetParent(initiativeRoot.transform);
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = "Your Initiative";
        tmp.fontSize = 8;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        // Position text above the sphere, make it face the camera more directly if needed
        tmp.rectTransform.localPosition = new Vector3(0, 0, -5f); // In front of the sphere (towards camera for Player 1)
        tmp.rectTransform.localRotation = Quaternion.Euler(90, 0, 0);
        tmp.rectTransform.sizeDelta = new Vector2(15f, 5f);
    }

    private static void CreateHealthDisplay(Transform playerAreaParent, string playerName, Vector3 position)
    {
        GameObject healthRoot = new GameObject($"{playerName}_HealthDisplay");
        healthRoot.transform.SetParent(playerAreaParent); // Parent to player area for correct orientation
        healthRoot.transform.localPosition = position;
        healthRoot.transform.localRotation = Quaternion.identity; // Will be oriented with player area

        // Background for health (optional, could be a textured Quad)
        GameObject healthBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healthBg.name = "HealthBackground";
        healthBg.transform.SetParent(healthRoot.transform);
        healthBg.transform.localPosition = Vector3.zero;
        healthBg.transform.localScale = new Vector3(12f, 4f, 1.5f); // Adjust as needed
        healthBg.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.8f, 0.2f, 0.2f)); // Reddish
        Object.DestroyImmediate(healthBg.GetComponent<Collider>());

        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(healthRoot.transform);
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = "20 HEALTH"; // Initial Value
        tmp.fontSize = 15;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.rectTransform.localPosition = new Vector3(0, 0.05f, 0); // Slightly above BG
        tmp.rectTransform.localRotation = Quaternion.Euler(90,0,0); // Flat on the BG
        tmp.rectTransform.sizeDelta = new Vector2(11f, 3.5f);
    }
} 