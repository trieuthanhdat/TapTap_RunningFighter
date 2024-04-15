using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project_RunningFighter.Gameplay.UI
{
    public class UIPlayerProgressIconBehaviour : MonoBehaviour
    {
        [SerializeField] float m_HandleWidth = 55f;
        [SerializeField] Slider m_Slider;
        [SerializeField] RectTransform rectPlayerProgressIcon;
        [SerializeField] int m_AttachedPlayerIndex;

        private void Awake()
        {
            if(m_Slider == null) m_Slider = GetComponent<Slider>();
            SetActive(false);
        }
       
        public void SetActive(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
        
        public void Init(int playerIndex)
        {
            SetPlayerIndex(playerIndex);
            SetupRectTransform();
        }

        private void SetupRectTransform()
        {
            if (!rectPlayerProgressIcon) return;

            // Set anchorMin and anchorMax to (0, 0) to anchor the RectTransform to the bottom-left corner
            rectPlayerProgressIcon.anchorMin = new Vector2(0, 0);
            rectPlayerProgressIcon.anchorMax = new Vector2(0, 0);

            // Set the offsets to adjust the position
            rectPlayerProgressIcon.offsetMin = new Vector2(0, rectPlayerProgressIcon.offsetMin.y); // Keep the same bottom offset
            rectPlayerProgressIcon.offsetMax = new Vector2(m_HandleWidth, rectPlayerProgressIcon.offsetMax.y); 

            rectPlayerProgressIcon.anchoredPosition = new Vector2(0, rectPlayerProgressIcon.anchoredPosition.y); // Set PosX to 0
        }

        public void ChangeSliderProgress(float sliderValue)
        {
           m_Slider.value = sliderValue;
        }

        private void SetPlayerIndex(int playerIndex)
        {
            m_AttachedPlayerIndex = playerIndex;
            Debug.Log($"UI PLAYER PROGRESS BEHAVIOUR: set Player Trans source on networked index" + playerIndex);
        }

    }

}
