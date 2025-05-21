using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestBoxUI : MonoBehaviour
{
    public Image innerImageBackground;
    public TextMeshProUGUI descriptionText;
    // public TextMeshProUGUI amountText; // REMOVED
    public Image gemIcon;
    public TextMeshProUGUI gemAmountText;

    public Color normalBgColor = new Color(0.1f, 0.1f, 0.15f, 1f);
    public Color completeBgColor = new Color(0.2f, 0.7f, 0.2f, 1f); // Green
    public Color normalTextColor = Color.white;
    public Color completeTextColor = Color.white;

    [HideInInspector] public LayoutElement innerImageLayoutElement;
    [HideInInspector] public RectTransform questDataRect;

    public void SetQuest(Quest quest)
    {
        if (descriptionText != null)
        {
            // Use the base description and remove the "{amount}" placeholder if it exists,
            // then append the progress.
            string baseDescription = quest.description.Replace("({amount})", "").Trim(); // Remove placeholder and trim whitespace
            string fullDescription = $"{baseDescription} ({quest.currentAmount}/{quest.requiredAmount})"; // Append progress
            descriptionText.text = fullDescription;
        }
        else
        {
            Debug.LogError("descriptionText not assigned in QuestBoxUI on " + gameObject.name, this);
        }
        
        if (gemAmountText != null)
            gemAmountText.text = quest.gemReward.ToString();
        else
            Debug.LogError("gemAmountText not assigned in QuestBoxUI on " + gameObject.name, this);

        if (innerImageBackground != null)
        {
            innerImageBackground.color = quest.IsComplete ? completeBgColor : normalBgColor;
        }
        else
        {
            Debug.LogError("innerImageBackground not assigned in QuestBoxUI on " + gameObject.name, this);
        }

        Color currentTextColor = quest.IsComplete ? completeTextColor : normalTextColor;
        if (descriptionText != null) descriptionText.color = currentTextColor;
        if (gemAmountText != null) gemAmountText.color = currentTextColor;
    }

    void LateUpdate()
    {
        if (questDataRect != null && innerImageLayoutElement != null)
        {
            float contentHeight = LayoutUtility.GetPreferredHeight(questDataRect);

            innerImageLayoutElement.preferredHeight = contentHeight;

            LayoutElement rootQuestBoxLayout = GetComponent<LayoutElement>();
            if (rootQuestBoxLayout != null)
            {
                rootQuestBoxLayout.preferredHeight = contentHeight;
            }
        }
    }
}
