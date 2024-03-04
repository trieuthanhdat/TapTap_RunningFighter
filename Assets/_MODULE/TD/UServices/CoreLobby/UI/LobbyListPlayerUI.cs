using System.Collections.Generic;
using TD.UServices.CoreLobby.Infrastructure;
using UnityEngine;

namespace TD.UServices.CoreLobby.UI
{
    public class LobbyListPlayerUI : UIPanelBase
    {
        [SerializeField]
        List<InLobbyPlayerUI> m_UserUIObjects = new List<InLobbyPlayerUI>();

        LocalLobby m_LocalLobby;

        public override void Start()
        {
            base.Start();
            //m_LocalLobby = GameManager.Instance.LocalLobby;
            m_LocalLobby.onUserJoined += OnUserJoined;
            m_LocalLobby.onUserLeft += OnUserLeft;
        }

        void OnUserJoined(LocalPlayer localPlayer)
        {
            SynchPlayerUI();
        }

        void OnUserLeft(int i)
        {
            SynchPlayerUI();
        }

        void SynchPlayerUI()
        {
            foreach (var ui in m_UserUIObjects)
                ui.ResetUI();
            for (int i = 0; i < m_LocalLobby.PlayerCount; i++)
            {
                var lobbySlot = m_UserUIObjects[i];
                var player = m_LocalLobby.GetLocalPlayer(i);
                if (player == null)
                    continue;
                lobbySlot.SetUser(player);
            }
        }
    }
}

