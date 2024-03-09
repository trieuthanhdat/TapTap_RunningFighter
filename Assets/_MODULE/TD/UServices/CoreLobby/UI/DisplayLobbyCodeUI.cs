using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.UI;
using TMPro;
using UnityEngine;

namespace TD.UServices.CoreLobby.UI
{
    public class DisplayLobbyCodeUI : UIPanelBase
    {
        public enum CodeType 
        { 
            Lobby = 0, 
            Relay = 1 
        }

        [SerializeField] protected TMP_InputField m_outputText;
        [SerializeField] protected CodeType m_codeType;

        protected void LobbyCodeChanged(string newCode)
        {
            if (!string.IsNullOrEmpty(newCode))
            {
                m_outputText.text = newCode;
                Show();
            }
            else
            {
                Hide();
            }
        }

        public override void Start()
        {
            base.Start();
            if (m_codeType == CodeType.Lobby)
                Manager.LocalLobby.LobbyCode.onChanged += LobbyCodeChanged;
            if (m_codeType == CodeType.Relay)
                Manager.LocalLobby.RelayCode.onChanged += LobbyCodeChanged;
        }
    }
}
