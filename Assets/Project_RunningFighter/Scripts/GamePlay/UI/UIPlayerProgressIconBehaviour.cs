using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.UI
{
    public class UIPlayerProgressIconBehaviour : MonoBehaviour
    {
        [SerializeField] RectTransform rectTransform;
        [SerializeField] private Transform m_AttachedPlayerTf;
        public void Init(Transform playerTrans)
        {
            m_AttachedPlayerTf = playerTrans;
            RefreshRectTransform();
        }

        private void RefreshRectTransform()
        {
            if (!rectTransform) return;
            // Set anchorMin and anchorMax to (0, 0) to anchor the RectTransform to the bottom-left corner
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            // Set the offsets to adjust the position
            rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y); // Keep the same bottom offset
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, rectTransform.offsetMax.y); // Keep the same top offset

            // Set the new PosX
            rectTransform.anchoredPosition = new Vector2(0, rectTransform.anchoredPosition.y); // Set PosX to 0
        }

        public void ChangePositionX(float newX)
        {
            RectTransform iconRect = rectTransform;
            Vector3 iconPosition = iconRect.localPosition;
            iconPosition.x = newX;
            iconRect.localPosition = iconPosition;
        }
    }

}
