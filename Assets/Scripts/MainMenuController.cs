using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    public enum SceneType { Play, Deck, Solo, Shop }
    public SceneType currentScene = SceneType.Play;

    [Header("UI References")]
    public GameObject topBar;
    public GameObject line;
    public GameObject mainMenuContent;
    public List<Button> menuButtons;
    public List<GameObject> underlineImages; // Underline images for each button
    public List<GameObject> panels; // PlayPanel, DeckPanel, etc.

    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 1f, 1f, 0.8f);
    public Color activeColor = new Color(1f, 1f, 1f, 1f);

    void Start()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            int idx = i;
            var trigger = menuButtons[i].gameObject.AddComponent<EventTrigger>();
            AddEvent(trigger, EventTriggerType.PointerEnter, () => OnButtonHover(idx));
            AddEvent(trigger, EventTriggerType.PointerExit, () => OnButtonExit(idx));
            menuButtons[i].onClick.AddListener(() => OnMenuButtonClicked(idx));
        }
        UpdateUI();
    }

    void AddEvent(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((eventData) => action());
        trigger.triggers.Add(entry);
    }

    void OnButtonHover(int idx)
    {
        if ((int)currentScene != idx)
            menuButtons[idx].GetComponent<Image>().color = hoverColor;
    }

    void OnButtonExit(int idx)
    {
        if ((int)currentScene != idx)
            menuButtons[idx].GetComponent<Image>().color = normalColor;
    }

    void OnMenuButtonClicked(int idx)
    {
        currentScene = (SceneType)idx;
        UpdateUI();
    }

    void UpdateUI()
    {
        // Show/hide main menu content
        mainMenuContent.SetActive(currentScene == SceneType.Play);

        // Show/hide panels
        for (int i = 0; i < panels.Count; i++)
            panels[i].SetActive((int)currentScene == i);

        // Update button visuals
        for (int i = 0; i < menuButtons.Count; i++)
        {
            var btn = menuButtons[i];
            var underline = underlineImages[i];
            if ((int)currentScene == i)
            {
                btn.transform.localScale = Vector3.one * 1.2f; // Make active button larger
                btn.GetComponent<Image>().color = activeColor;
                underline.SetActive(true);
            }
            else
            {
                btn.transform.localScale = Vector3.one;
                btn.GetComponent<Image>().color = normalColor;
                underline.SetActive(false);
            }
        }
    }
}
