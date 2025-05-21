using UnityEngine;
using UnityEngine.UI;
using System;

public class QuestBoxUI : MonoBehaviour
{
    public Image background;
    public Image border;
    public Text descriptionText;
    public Text amountText;
    public Image gemIcon;
    public Text gemAmountText;

    public Color normalBg = new Color(0.1f, 0.1f, 0.15f, 1f);
    public Color completeBg = new Color(0.2f, 0.7f, 0.2f, 1f); // Green
    public Color normalText = Color.white;
    public Color completeText = Color.white;

    public LayoutElement innerImageLayoutElement; // Assign in Inspector
    public RectTransform questDataRect;           // Assign in Inspector

    public void SetQuest(Quest quest)
    {
        descriptionText.text = quest.description.Replace("{amount}", quest.requiredAmount.ToString());
        amountText.text = $"({quest.currentAmount}/{quest.requiredAmount})";
        gemAmountText.text = quest.gemReward.ToString();

        if (quest.IsComplete)
        {
            background.color = completeBg;
            descriptionText.color = completeText;
            amountText.color = completeText;
            gemAmountText.color = completeText;
        }
        else
        {
            background.color = normalBg;
            descriptionText.color = normalText;
            amountText.color = normalText;
            gemAmountText.color = normalText;
        }
    }

    void LateUpdate()
    {
        // Get the preferred height of the content (QuestData)
        float contentHeight = LayoutUtility.GetPreferredHeight(questDataRect);

        // Set the height of InnerImage to match content
        if (innerImageLayoutElement != null)
        {
            innerImageLayoutElement.preferredHeight = contentHeight;
        }

        // Optionally, set the height of QuestBox itself (if needed)
        var questBoxLayout = GetComponent<LayoutElement>();
        if (questBoxLayout != null)
        {
            questBoxLayout.preferredHeight = contentHeight;
        }
    }
}
