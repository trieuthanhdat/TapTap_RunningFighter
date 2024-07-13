using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollbarHandler : MonoBehaviour
{

    [Tooltip("Reference to the Scroll Rect component")]
    public ScrollRect scrollRect;
    private Scrollbar scrollbar;

    public void Awake()
    {
        scrollbar = GetComponent<Scrollbar>();
        scrollbar.onValueChanged.AddListener(OnScrollbarValueChangedV2);
    }

    public void OnScrollbarValueChanged(float value)
    {
        // Calculate the normalized position of the scrollbar value
        float normalizedPosition = 1 - value;

        // Set the verticalNormalizedPosition of the scroll rect
        scrollRect.verticalNormalizedPosition = normalizedPosition;
    }
    public void OnScrollbarValueChangedV2(float value)
    {
        // Calculate the normalized position of the scrollbar value
        float normalizedPosition = 1 - scrollbar.size;

        // Calculate the bottom-most position of the content
        float bottomPosition = scrollRect.content.rect.height - scrollRect.viewport.rect.height;

        // Calculate the vertical position of the bottom-most item
        float bottomItemPosition = scrollRect.content.GetChild(scrollRect.content.childCount - 1).transform.localPosition.y;

        // Calculate the normalized position of the bottom-most item
        float bottomItemNormalizedPosition = 1 - (bottomItemPosition / bottomPosition);

        // Set the verticalNormalizedPosition of the scroll rect to the bottom-most position
        scrollRect.verticalNormalizedPosition = bottomItemNormalizedPosition;
    }


}
