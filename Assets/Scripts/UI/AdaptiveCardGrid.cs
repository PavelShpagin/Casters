using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AdaptiveGridLayout : MonoBehaviour
{
    [Header("Simple Settings")]
    [SerializeField] private int numberOfColumns = 3; // Number of columns in the grid
    [SerializeField] private float horizontalSpacing = 30f; // Margin on left and right sides
    [SerializeField] private float verticalSpacing = 30f; // Space between rows
    [SerializeField] private float aspectRatio = 1.4f; // Width/Height ratio of cards
    
    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;
    private float lastWidth = -1f;

    void Start()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        UpdateCellSize();
    }

    void Update()
    {
        float currentWidth = rectTransform.rect.width;
        if (Mathf.Abs(currentWidth - lastWidth) > 1f && currentWidth > 0)
        {
            lastWidth = currentWidth;
            UpdateCellSize();
        }
    }

    void UpdateCellSize()
    {
        if (gridLayout == null || rectTransform == null) return;
        if (numberOfColumns <= 0) numberOfColumns = 1; // Safety check

        // Formula: (parent_width - (numberOfColumns - 1) * horizontalSpacing) / numberOfColumns = card_width
        float parentWidth = rectTransform.rect.width;
        float cardWidth = (parentWidth - ((numberOfColumns - 1) * horizontalSpacing)) / numberOfColumns;
        float cardHeight = cardWidth * aspectRatio;
        
        // Apply to grid with proper spacing
        gridLayout.cellSize = new Vector2(cardWidth, cardHeight);
        gridLayout.spacing = new Vector2(horizontalSpacing, verticalSpacing);

        Debug.Log($"[AdaptiveGrid] {numberOfColumns} columns, Card size: {cardWidth:F1} x {cardHeight:F1}, Spacing: H={horizontalSpacing}, V={verticalSpacing}");
    }

    [ContextMenu("Update Now")]
    public void ForceUpdateCellSize()
    {
        UpdateCellSize();
    }
}
