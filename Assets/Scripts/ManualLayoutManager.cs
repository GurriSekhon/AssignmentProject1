using UnityEngine;
using System.Collections.Generic;

public class ManualLayoutManager : MonoBehaviour
{
    [Header("Aspect Ratio")]
    [Tooltip("Desired aspect ratio for cards (width / height). E.g., 0.7 for portrait cards.")]
    public float cardAspectRatio = 0.7f;

    [Header("Padding")]
    [Tooltip("Space added on all sides inside the container.")]
    public float horizontalPadding = 20f;
    public float verticalPadding = 20f;

    [Header("Alignment")]
    public ChildAlignment childAlignment = ChildAlignment.UpperLeft;

    /// <summary>
    /// Alignments similar to GridLayoutGroup's Child Alignment
    /// </summary>
    public enum ChildAlignment
    {
        UpperLeft,
        UpperCenter,
        UpperRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        LowerLeft,
        LowerCenter,
        LowerRight
    }

    /// <summary>
    /// Arranges the cards in the given rows and columns, preserving their aspect ratio
    /// and aligning them within this container based on the selected ChildAlignment.
    /// </summary>
    public void ArrangeCards(int rows, int cols, List<GameObject> cards)
    {
        // 1. Get container size
        RectTransform containerRect = GetComponent<RectTransform>();
        float containerWidth = containerRect.rect.width;
        float containerHeight = containerRect.rect.height;

        // 2. Compute the workable area by subtracting padding
        float workableWidth = containerWidth - (horizontalPadding * 2f);
        float workableHeight = containerHeight - (verticalPadding * 2f);

        // 3. Compute raw cell size (no aspect ratio considered yet)
        float cellWidth = workableWidth / cols;
        float cellHeight = workableHeight / rows;

        // 4. Adjust cell size to maintain card's aspect ratio
        //    We'll pick the limiting dimension so cards don’t exceed the cell 
        //    while preserving aspect ratio.
        float cellAspect = cellWidth / cellHeight;

        if (cellAspect > cardAspectRatio)
        {
            // Cell is too wide relative to the card's aspect -> limit width
            cellWidth = cellHeight * cardAspectRatio;
        }
        else
        {
            // Cell is too tall relative to the card's aspect -> limit height
            cellHeight = cellWidth / cardAspectRatio;
        }

        // 5. Compute total width/height occupied by the arranged cards
        float totalWidth = cellWidth * cols;
        float totalHeight = cellHeight * rows;

        // 6. Determine alignment offsets based on ChildAlignment
        Vector2 offset = CalculateAlignmentOffset(workableWidth, workableHeight, totalWidth, totalHeight);

        // 7. Position each card
        for (int i = 0; i < cards.Count; i++)
        {
            int rowIndex = i / cols;
            int colIndex = i % cols;

            // By default, we treat rowIndex 0 as the top row. So let's invert rowIndex for "top" alignment:
            // e.g., row 0 will be at the top, row (rows-1) at the bottom
            int invertedRow = (rows - 1) - rowIndex;

            // Calculate local position within workable area:
            // Left edge plus offset plus half a cell to center the card
            float xPos = (colIndex * cellWidth) + (cellWidth * 0.5f);
            float yPos = (invertedRow * cellHeight) + (cellHeight * 0.5f);

            // Apply alignment offsets + overall padding
            xPos += horizontalPadding + offset.x;
            yPos += verticalPadding + offset.y;

            // Assign the card's transform
            RectTransform cardRect = cards[i].GetComponent<RectTransform>();

            // Make sure the card is parented to this container
            cardRect.SetParent(containerRect, false);

            // Pivot at 0.5, 0.5 for easy positioning
            cardRect.pivot = new Vector2(0.5f, 0.5f);

            // Position the card in the container
            cardRect.anchoredPosition = new Vector2(xPos, yPos);

            // Update the size to match the cell
            cardRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellWidth);
            cardRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellHeight);
        }
    }

    /// <summary>
    /// Calculates an offset for card placement based on the selected ChildAlignment.
    /// Returns a Vector2 (xOffset, yOffset) within the workable area.
    /// </summary>
    private Vector2 CalculateAlignmentOffset(float workableWidth, float workableHeight, float totalWidth, float totalHeight)
    {
        float xOffset = 0f;
        float yOffset = 0f;

        // X offset
        switch (childAlignment)
        {
            case ChildAlignment.UpperLeft:
            case ChildAlignment.MiddleLeft:
            case ChildAlignment.LowerLeft:
                xOffset = 0f; // left alignment
                break;

            case ChildAlignment.UpperCenter:
            case ChildAlignment.MiddleCenter:
            case ChildAlignment.LowerCenter:
                xOffset = (workableWidth - totalWidth) * 0.5f; // center
                break;

            case ChildAlignment.UpperRight:
            case ChildAlignment.MiddleRight:
            case ChildAlignment.LowerRight:
                xOffset = workableWidth - totalWidth; // right
                break;
        }

        // Y offset
        switch (childAlignment)
        {
            case ChildAlignment.UpperLeft:
            case ChildAlignment.UpperCenter:
            case ChildAlignment.UpperRight:
                yOffset = workableHeight - totalHeight; // top
                break;

            case ChildAlignment.MiddleLeft:
            case ChildAlignment.MiddleCenter:
            case ChildAlignment.MiddleRight:
                yOffset = (workableHeight - totalHeight) * 0.5f; // middle
                break;

            case ChildAlignment.LowerLeft:
            case ChildAlignment.LowerCenter:
            case ChildAlignment.LowerRight:
                yOffset = 0f; // bottom
                break;
        }

        return new Vector2(xOffset, yOffset);
    }
}
