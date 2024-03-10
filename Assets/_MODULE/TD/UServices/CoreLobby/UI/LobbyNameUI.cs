using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.UI;
using TMPro;
using UnityEngine;

namespace TD.UServices.CoreLobby.UI
{
    public class LobbyNameUI : UIPanelBase
    {
        [SerializeField]
        TMP_Text m_lobbyNameText;

        public override void Start()
        {
            base.Start();
            Manager.LocalLobby.LobbyName.onChanged += (s) => { m_lobbyNameText.SetText(s); };
        }
    }
}
