using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Project_RunningFighter.Gameplay.UI
{
    public class GameModePanel : MonoBehaviour
    {
        [SerializeField] CanvasGroup m_CanvasGroup;

        public void Show()
        {
            if (m_CanvasGroup == null) return;
            m_CanvasGroup.alpha = 1;
            m_CanvasGroup.interactable = true;
            m_CanvasGroup.blocksRaycasts = true;    
        }
        public void Hide()
        {
            if (m_CanvasGroup == null) return;
            m_CanvasGroup.alpha = 0;
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.blocksRaycasts = false;
        }
    }

}
