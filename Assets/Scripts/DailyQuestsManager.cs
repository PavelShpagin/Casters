using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
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
        new Quest { description = "Summon creatures ({amount})", requiredAmount = 10, currentAmount = 0, gemReward = 12 },
        new Quest { description = "Draw cards ({amount})", requiredAmount = 15, currentAmount = 0, gemReward = 10 }
    };

    private List<Quest> activeQuests = new List<Quest>();

    private const string LastLoginKey = "LastLoginDate";
    private const string QuestsKey = "ActiveQuests";

    void Start()
    {
        if (IsNewDay())
        {
            PickNewDailyQuests();
            SaveLastLogin();
        }
        else
        {
            LoadActiveQuests();
        }
        DisplayQuests();
    }

    bool IsNewDay()
    {
        string lastLogin = PlayerPrefs.GetString(LastLoginKey, "");
        if (string.IsNullOrEmpty(lastLogin)) return true;
        DateTime last = DateTime.Parse(lastLogin);
        return last.Date != DateTime.Now.Date;
    }

    void SaveLastLogin()
    {
        PlayerPrefs.SetString(LastLoginKey, DateTime.Now.ToString());
        PlayerPrefs.Save();
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
        SaveActiveQuests();
    }

    void SaveActiveQuests()
    {
        // You can implement serialization for persistence if needed
        // For now, just store in memory
    }

    void LoadActiveQuests()
    {
        // You can implement deserialization for persistence if needed
        // For now, just use in-memory list
    }

    void DisplayQuests()
    {
        // Remove old boxes
        foreach (Transform child in questsParent)
            Destroy(child.gameObject);

        // Add new boxes
        foreach (var quest in activeQuests)
        {
            GameObject box = Instantiate(questBoxPrefab, questsParent);
            var questBox = box.GetComponent<QuestBoxUI>();
            // Assign references for dynamic sizing
            questBox.innerImageLayoutElement = box.transform.Find("InnerImage").GetComponent<UnityEngine.UI.LayoutElement>();
            questBox.questDataRect = box.transform.Find("QuestData").GetComponent<RectTransform>();
            questBox.SetQuest(quest);
        }
    }
}
