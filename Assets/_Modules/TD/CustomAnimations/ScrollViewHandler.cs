using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewHandler : MonoBehaviour
{
    public ScrollRect scrollRect;

    public bool enableHorizontal = false;
    public bool enableVertical = false;
    private void Start()
    {
        // Scroll to the top of the scroll view
        scrollRect.verticalNormalizedPosition = 1f;

        // Disable scrolling
        scrollRect.vertical = false;
        scrollRect.horizontal = false;

        // Enable scrolling again after a delay
        Invoke("EnableScrolling", 2f);
    }

    private void EnableScrolling()
    {
        scrollRect.vertical = enableVertical;
        scrollRect.horizontal = enableHorizontal;
    }

    public void ScrollToItem(RectTransform item)
    {
        // Calculate the normalized position of the item in the content
        float normalizedPosition = item.anchoredPosition.y / scrollRect.content.rect.height;

        // Scroll to the item
        scrollRect.verticalNormalizedPosition = 1f - normalizedPosition;
    }

    public void ScrollToTop()
    {
        // Scroll to the top of the scroll view
        scrollRect.verticalNormalizedPosition = 1f;
    }

    public void ScrollToBottom()
    {
        // Scroll to the bottom of the scroll view
        scrollRect.verticalNormalizedPosition = 0f;
    }

}
