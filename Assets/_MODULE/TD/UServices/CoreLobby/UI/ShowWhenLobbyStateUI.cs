using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.UI;
using TD.UServices.CoreLobby;
using UnityEngine;

namespace TD.UServices.CoreLobby.UI
{
    public class ShowWhenLobbyStateUI : UIPanelBase
    {
        [SerializeField] protected LobbyState m_ShowThisWhen;

        public void LobbyChanged(LobbyState lobbyState)
        {
            if (m_ShowThisWhen.HasFlag(lobbyState))
                Show();
            else
                Hide();
        }

        public override void Start()
        {
            base.Start();
            Manager.LocalLobby.LocalLobbyState.onChanged += LobbyChanged;
        }

    }

}
