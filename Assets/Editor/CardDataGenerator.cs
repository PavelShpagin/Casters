using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System; // Required for System.Enum

// Adjusted to match your JSON field names
[System.Serializable]
public class JsonCardInputData // Renamed for clarity
{
    // public int id; // Not present in your JSON, will be auto-generated or defaulted
    public string title;
    public string description;
    public string type; // Was cardType, matches your JSON
    public string @class; // Was faction, matches your JSON ("class" is a keyword, so @class)
    public string cost;   // Kept as string to read from JSON, will attempt to parse to int
    public int attack;
    public int health;
    public int level;
    public string card_img; // Was imageName, matches your JSON
}

// No longer need JsonCardList if the root is an array

public class CardDataGenerator : EditorWindow
{
    private string jsonFilePath = "Assets/Resources/Cards/cards.json";
    private string spritesResourcePath = "Sprites/Cards/";
    private string outputAssetPath = "Assets/Resources/CardsCollection/";

    [MenuItem("Tools/Card Data Generator from Array")]
    public static void ShowWindow()
    {
        GetWindow<CardDataGenerator>("Card Data Gen (Array)");
    }

    void OnGUI()
    {
        GUILayout.Label("Card Data Generation from JSON Array", EditorStyles.boldLabel);

        jsonFilePath = EditorGUILayout.TextField("JSON File Path:", jsonFilePath);
        spritesResourcePath = EditorGUILayout.TextField("Sprites Resource Path:", spritesResourcePath);
        outputAssetPath = EditorGUILayout.TextField("Output Asset Path:", outputAssetPath);

        if (GUILayout.Button("Generate CardData Assets"))
        {
            GenerateCards();
        }

        EditorGUILayout.HelpBox("Ensure paths are correct.\n" +
                                "JSON should be an ARRAY of card objects.\n" +
                                "Sprites in 'Resources/[SpritesResourcePath]'.", MessageType.Info);
    }

    private void GenerateCards()
    {
        if (string.IsNullOrEmpty(jsonFilePath) || string.IsNullOrEmpty(spritesResourcePath) || string.IsNullOrEmpty(outputAssetPath))
        {
            Debug.LogError("CardDataGenerator: Paths cannot be empty!");
            return;
        }

        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError($"CardDataGenerator: JSON file not found at {jsonFilePath}");
            return;
        }

        if (!Directory.Exists(outputAssetPath))
        {
            Directory.CreateDirectory(outputAssetPath);
            AssetDatabase.Refresh();
        }

        string jsonString = File.ReadAllText(jsonFilePath);
        List<JsonCardInputData> cardList;
        try
        {
            // Directly parse as a list since the root is an array
            // For more robust parsing of arrays, especially with JsonUtility, you might need a wrapper object
            // if JsonUtility.FromJson<List<T>> doesn't work directly. Newtonsoft.Json is better for this.
            // However, a common workaround for JsonUtility is to temporarily wrap the JSON string:
            string wrappedJsonString = "{\"cards\":" + jsonString + "}";
            JsonWrapper helper = JsonUtility.FromJson<JsonWrapper>(wrappedJsonString);
            cardList = helper.cards;
        }
        catch (Exception e)
        {
            Debug.LogError($"CardDataGenerator: Failed to parse JSON. Error: {e.Message}");
            return;
        }

        if (cardList == null)
        {
            Debug.LogError("CardDataGenerator: JSON data could not be parsed into a list of cards.");
            return;
        }

        int count = 0;
        int currentId = 1; // For auto-generating IDs
        foreach (JsonCardInputData jsonCard in cardList)
        {
            CardData cardDataInstance = ScriptableObject.CreateInstance<CardData>();

            cardDataInstance.id = currentId++; // Auto-generate ID
            cardDataInstance.title = jsonCard.title;
            cardDataInstance.description = jsonCard.description;
            cardDataInstance.attack = jsonCard.attack;
            cardDataInstance.health = jsonCard.health;
            cardDataInstance.level = jsonCard.level;

            // Assign Cost string directly
            cardDataInstance.cost = jsonCard.cost;

            // Parse Enums (case-insensitive)
            try
            {
                cardDataInstance.cardType = (CardType)System.Enum.Parse(typeof(CardType), jsonCard.type, true);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning($"CardDataGenerator: Could not parse CardType '{jsonCard.type}' for card '{jsonCard.title}'. Defaulting to {cardDataInstance.cardType}.");
            }

            try
            {
                // Assuming your CardFaction enum might have an entry like "Purple"
                cardDataInstance.faction = (CardFaction)System.Enum.Parse(typeof(CardFaction), jsonCard.@class, true);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning($"CardDataGenerator: Could not parse Faction (class) '{jsonCard.@class}' for card '{jsonCard.title}'. Defaulting to {cardDataInstance.faction}.");
            }

            // Load Sprite
            if (!string.IsNullOrEmpty(jsonCard.card_img))
            {
                string spriteFileName = Path.GetFileNameWithoutExtension(jsonCard.card_img);
                string fullSpritePath = spritesResourcePath + spriteFileName;
                Sprite loadedSprite = Resources.Load<Sprite>(fullSpritePath);
                if (loadedSprite != null)
                {
                    cardDataInstance.cardImage = loadedSprite;
                }
                else
                {
                    Debug.LogWarning($"CardDataGenerator: Sprite not found at 'Resources/{fullSpritePath}' (from imageName '{jsonCard.card_img}') for card '{jsonCard.title}'.");
                }
            }
            else
            {
                 Debug.LogWarning($"CardDataGenerator: No card_img provided for card '{jsonCard.title}'.");
            }

            string assetFileName = SanitizeFileName(jsonCard.title) + "_CardData.asset";
            string fullAssetPath = Path.Combine(outputAssetPath, assetFileName);
            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(fullAssetPath);

            AssetDatabase.CreateAsset(cardDataInstance, uniqueAssetPath);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"CardDataGenerator: Successfully generated {count} CardData assets in {outputAssetPath}");
    }

    private string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) name = "UnnamedCard";
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name;
    }

    // Helper class to wrap the array for JsonUtility
    [System.Serializable]
    private class JsonWrapper
    {
        public List<JsonCardInputData> cards;
    }
}
