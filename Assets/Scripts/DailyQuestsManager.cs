using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Quest
{
    public string description;
    public int requiredAmount;
    public int currentAmount;
    public int gemReward;
    public bool IsComplete => currentAmount >= requiredAmount;
}

public class DailyQuestsManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform questsParent; // Assign: DailyQuests (Vertical Layout Group)
    public GameObject questBoxPrefab; // Assign: QuestBox prefab

    [Header("Quest Data")]
    public int dailyQuestCount = 5;

    // Sample quest data
    private List<Quest> allPossibleQuests = new List<Quest>
    {
        new Quest { description = "Kill minions ({amount})", requiredAmount = 5, currentAmount = 0, gemReward = 15 },
        new Quest { description = "Sacrifice minions ({amount})", requiredAmount = 5, currentAmount = 0, gemReward = 15 },
        new Quest { description = "Win a game ({amount})", requiredAmount = 1, currentAmount = 0, gemReward = 25 },
        new Quest { description = "Play cards ({amount})", requiredAmount = 20, currentAmount = 0, gemReward = 10 },
        new Quest { description = "Deal damage ({amount})", requiredAmount = 50, currentAmount = 0, gemReward = 20 },
        new Quest { description = "Draw cards ({amount})", requiredAmount = 15, currentAmount = 0, gemReward = 10 }
    };

    private List<Quest> activeQuests = new List<Quest>();

    void Start()
    {
        PickNewDailyQuests();
        DisplayQuests();
    }

    void PickNewDailyQuests()
    {
        activeQuests.Clear();
        var usedIndices = new HashSet<int>();
        var rand = new System.Random();
        while (activeQuests.Count < dailyQuestCount && usedIndices.Count < allPossibleQuests.Count)
        {
            int idx = rand.Next(allPossibleQuests.Count);
            if (!usedIndices.Contains(idx))
            {
                usedIndices.Add(idx);
                // Deep copy to avoid reference issues
                Quest q = new Quest
                {
                    description = allPossibleQuests[idx].description,
                    requiredAmount = allPossibleQuests[idx].requiredAmount,
                    currentAmount = 0,
                    gemReward = allPossibleQuests[idx].gemReward
                };
                activeQuests.Add(q);
            }
        }
    }

    void DisplayQuests()
    {
        // Remove old boxes
        foreach (Transform child in questsParent)
            Destroy(child.gameObject);

        Debug.Log($"Starting to display {activeQuests.Count} quests.");

        List<GameObject> newBoxes = new List<GameObject>(); // Keep track of new boxes

        // Add new boxes
        foreach (var quest in activeQuests)
        {
            GameObject boxInstance = Instantiate(questBoxPrefab, questsParent);
            boxInstance.SetActive(false); // Start disabled
            Debug.Log($"Instantiated QuestBox: {boxInstance.name}");

            QuestBoxUI questBoxUIScript = boxInstance.GetComponent<QuestBoxUI>();
            if (questBoxUIScript == null)
            {
                Debug.LogError($"QuestBoxUI component not found on instantiated prefab: {boxInstance.name}. Ensure the script is attached to the root of your QuestBox prefab.");
                continue; 
            }

            // --- For QuestBoxUI's LateUpdate dynamic sizing logic ---
            Transform questDataTransform = boxInstance.transform.Find("QuestData");
            if (questDataTransform != null)
            {
                questBoxUIScript.questDataRect = questDataTransform.GetComponent<RectTransform>();
                if (questBoxUIScript.questDataRect == null)
                {
                    Debug.LogError($"RectTransform component not found on 'QuestData' child of {boxInstance.name} for dynamic sizing.");
                }
            }
            else
            {
                Debug.LogError($"Child GameObject named 'QuestData' not found in {boxInstance.name} for dynamic sizing. Check prefab hierarchy.");
            }

            Transform innerImageTransform = boxInstance.transform.Find("InnerImage");
            if (innerImageTransform != null)
            {
                questBoxUIScript.innerImageLayoutElement = innerImageTransform.GetComponent<UnityEngine.UI.LayoutElement>();
                if (questBoxUIScript.innerImageLayoutElement == null)
                {
                    Debug.LogError($"LayoutElement component not found on 'InnerImage' child of {boxInstance.name} for dynamic sizing.");
                }
            }
            else
            {
                Debug.LogError($"Child GameObject named 'InnerImage' not found in {boxInstance.name} for dynamic sizing. Check prefab hierarchy.");
            }
            // --- End of dynamic sizing references ---
            
            questBoxUIScript.SetQuest(quest); // This method in QuestBoxUI uses its own Text/Image references
            newBoxes.Add(boxInstance); // Add to list
        }

        // Force layout rebuild on the parent
        if (questsParent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(questsParent.GetComponent<RectTransform>());
        }

        // Enable them after a very short delay or next frame
        StartCoroutine(EnableQuestBoxesAfterLayout(newBoxes));
    }

    System.Collections.IEnumerator EnableQuestBoxesAfterLayout(List<GameObject> boxesToEnable)
    {
        // Wait for the end of the frame, allowing layout to calculate at least once
        yield return new WaitForEndOfFrame();
        // Potentially wait one more frame if needed, or force rebuild again
        // if (questsParent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(questsParent.GetComponent<RectTransform>());
        // yield return new WaitForEndOfFrame();


        foreach (var box in boxesToEnable)
        {
            if (box != null) box.SetActive(true);
        }
    }
}
