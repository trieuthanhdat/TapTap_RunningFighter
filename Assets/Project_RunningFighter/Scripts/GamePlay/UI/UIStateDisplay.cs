using Project_RunningFighter.Utils;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.UI
{
    public class UIStateDisplay : MonoBehaviour
    {
        [SerializeField] UIName m_UIName;
        [SerializeField] UIHealth m_UIHealth;
        [SerializeField] UIMana m_UIMana;
        public void DisplayName(NetworkVariable<FixedPlayerName> networkedName)
        {
            m_UIName.gameObject.SetActive(true);
            m_UIName.Initialize(networkedName);
        }
        public void DisplayMana(NetworkVariable<int> networkedMana, int maxValue)
        {
            m_UIMana.gameObject.SetActive(true);
            m_UIMana.Initialize(networkedMana, maxValue);
        }
        public void DisplayHealth(NetworkVariable<int> networkedHealth, int maxValue)
        {
            m_UIHealth.gameObject.SetActive(true);
            m_UIHealth.Initialize(networkedHealth, maxValue);
        }

        public void HideHealth()
        {
            m_UIHealth.gameObject.SetActive(false);
        }
        public void HideMana()
        {
            m_UIMana.gameObject.SetActive(false);
        }
    }
}
