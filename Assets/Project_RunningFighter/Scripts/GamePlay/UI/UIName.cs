using Project_RunningFighter.Utils;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.UI
{
    public class UIName : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI m_UINameText;

        NetworkVariable<FixedPlayerName> m_NetworkedNameTag;

        public void Initialize(NetworkVariable<FixedPlayerName> networkedName)
        {
            m_NetworkedNameTag = networkedName;

            m_UINameText.text = networkedName.Value.ToString();
            networkedName.OnValueChanged += NameUpdated;
        }

        void NameUpdated(FixedPlayerName previousValue, FixedPlayerName newValue)
        {
            m_UINameText.text = newValue.ToString();
        }

        void OnDestroy()
        {
            m_NetworkedNameTag.OnValueChanged -= NameUpdated;
        }
    }
}
