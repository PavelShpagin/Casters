using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class UICoverBackground : MonoBehaviour
{
    void Start()
    {
        // Set anchors to stretch in both directions
        var rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f); // Center pivot
        rt.anchoredPosition = Vector2.zero; // Center position
    }

    void LateUpdate()
    {
        var rt = GetComponent<RectTransform>();
        var img = GetComponent<Image>();
        if (img.sprite == null) return;

        // Get the actual screen dimensions
        float screenW = Screen.width;
        float screenH = Screen.height;
        float imgW = img.sprite.rect.width;
        float imgH = img.sprite.rect.height;

        float screenRatio = screenW / screenH;
        float imgRatio = imgW / imgH;

        float scale;
        if (screenRatio > imgRatio)
        {
            // Screen is wider than image: scale to match width
            scale = screenW / imgW;
        }
        else
        {
            // Screen is taller than image: scale to match height
            scale = screenH / imgH;
        }

        // Apply the scale to both dimensions to maintain aspect ratio
        rt.sizeDelta = new Vector2(imgW * scale, imgH * scale);
    }
}
