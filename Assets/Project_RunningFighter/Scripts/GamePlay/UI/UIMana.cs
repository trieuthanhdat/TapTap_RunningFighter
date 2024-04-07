using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project_RunningFighter.Gameplay.UI
{
    public class UIMana : MonoBehaviour
    {
        [SerializeField] bool m_DisableAtFullMana = false;
        [SerializeField] Slider m_ManaPointsSlider;

        NetworkVariable<int> m_NetworkedMana;

        public void Initialize(NetworkVariable<int> networkedMana, int maxValue)
        {
            m_NetworkedMana = networkedMana;
            m_ManaPointsSlider.minValue = 0;
            m_ManaPointsSlider.maxValue = maxValue;
            ManaChanged(maxValue, maxValue);

            m_NetworkedMana.OnValueChanged += ManaChanged;
        }

        void ManaChanged(int previousValue, int newValue)
        {
            m_ManaPointsSlider.value = newValue;
            m_ManaPointsSlider.gameObject.SetActive(m_ManaPointsSlider.value != m_ManaPointsSlider.maxValue && m_DisableAtFullMana);
        }


        void OnDestroy()
        {
            m_NetworkedMana.OnValueChanged -= ManaChanged;
        }
    }

}
