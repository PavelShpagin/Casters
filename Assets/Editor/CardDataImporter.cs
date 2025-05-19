using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic; // Required for List<T>

// Helper class to match the structure of your JSON card objects
[System.Serializable]
public class JsonCard
{
    public string title;
    public string description;
    public int health;
    public int attack;
    public int level;
    public string cost; // Keep as string for flexibility like "Free Summon"
    public string type; // "Minion", "Spell", "Stage"
    public string faction; // "Purple", "Red", etc. (Matches CardFaction enum names)
    public string card_img; // Filename or path part
    // Add any other fields that are in your JSON and you want to import
}

public class CardDataImporter : EditorWindow
{
    [MenuItem("Tools/Deck Building/Import Cards from JSON")]
    public static void ImportCards()
    {
        string jsonFilePath = Path.Combine(Application.dataPath, "Resources", "Cards", "cards.json");
        string outputFolderPath = "Assets/Resources/CardsCollection"; // SOs will be created here

        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError($"JSON file not found at: {jsonFilePath}");
            return;
        }

        if (!Directory.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
        }

        string jsonString = File.ReadAllText(jsonFilePath);
        
        // JsonUtility expects an object to deserialize into, not a root array directly.
        // So we wrap our array in a simple object.
        jsonString = "{\"cards\":" + jsonString + "}";
        JsonCardArrayWrapper wrapper = JsonUtility.FromJson<JsonCardArrayWrapper>(jsonString);

        if (wrapper == null || wrapper.cards == null)
        {
            Debug.LogError("Failed to parse JSON. Make sure it is a valid JSON array of card objects.");
            Debug.LogError("Parsed JSON string was: " + jsonString); // Log what was parsed
            return;
        }

        int cardsImported = 0;
        int cardsSkipped = 0;

        foreach (JsonCard jsonCard in wrapper.cards)
        {
            string assetPath = Path.Combine(outputFolderPath, $"{SanitizeFileName(jsonCard.title)}.asset");

            if (AssetDatabase.LoadAssetAtPath<CardData>(assetPath) != null)
            {
                Debug.LogWarning($"Card asset already exists for '{jsonCard.title}' at '{assetPath}'. Skipping.");
                cardsSkipped++;
                continue;
            }

            CardData cardDataSO = ScriptableObject.CreateInstance<CardData>();

            cardDataSO.title = jsonCard.title;
            cardDataSO.description = jsonCard.description;
            cardDataSO.health = jsonCard.health;
            cardDataSO.attack = jsonCard.attack;
            cardDataSO.level = jsonCard.level;
            
            // Handle cost: For now, let's try to parse it as int, default to 0 if not.
            // You might want a more sophisticated system if "Free Summon" implies 0 or needs special handling.
            if (int.TryParse(jsonCard.cost, out int costValue))
            {
                cardDataSO.cost = costValue;
            }
            else if (jsonCard.cost.ToLower() == "free summon")
            {
                cardDataSO.cost = 0; // Or another specific value for free
            }
            else
            {
                cardDataSO.cost = -1; // Indicate unparsed or special cost
                Debug.LogWarning($"Could not parse cost '{jsonCard.cost}' for card '{jsonCard.title}'. Set to -1.");
            }

            // Convert string type to CardType enum
            if (System.Enum.TryParse<CardType>(jsonCard.type, true, out CardType cardTypeResult))
            {
                cardDataSO.cardType = cardTypeResult;
            }
            else
            {
                Debug.LogWarning($"Could not parse CardType '{jsonCard.type}' for card '{jsonCard.title}'. Defaulting to Minion.");
                cardDataSO.cardType = CardType.Minion; // Default
            }

            // Convert string faction to CardFaction enum
            if (System.Enum.TryParse<CardFaction>(jsonCard.faction, true, out CardFaction factionResult))
            {
                cardDataSO.faction = factionResult;
            }
            else
            { 
                Debug.LogWarning($"Could not parse CardFaction '{jsonCard.faction}' for card '{jsonCard.title}'. Defaulting to Neutral.");
                cardDataSO.faction = CardFaction.Neutral; // Default
            }
            
            // For card_img, this assumes you have sprites named similarly in a loadable path (e.g. Resources/CardArt/)
            // or you will manually assign them. For now, we just store the name/path part.
            // cardDataSO.cardImage = Resources.Load<Sprite>("CardArt/" + jsonCard.card_img); 
            // If card_img is just a filename like "myimage.webp", and it's in "Resources/CardArt/myimage.webp"
            // If loading fails, cardDataSO.cardImage will be null. You'd then assign it manually.

            // Create a unique ID for the card (e.g., based on title or a hash)
            // For simplicity, let's use a basic hash for now, but a sequential or GUID system is better for uniqueness.
            cardDataSO.id = jsonCard.title.GetHashCode(); // Simple ID generation, ensure it's unique enough or improve

            AssetDatabase.CreateAsset(cardDataSO, assetPath);
            cardsImported++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Card import complete. Imported: {cardsImported}, Skipped (already exist): {cardsSkipped}");
    }

    // Helper to create a valid filename from a string
    private static string SanitizeFileName(string name)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c.ToString(), "");
        }
        return name;
    }
}

// Wrapper class for JsonUtility to deserialize a root array
[System.Serializable]
public class JsonCardArrayWrapper
{
    public List<JsonCard> cards;
} 