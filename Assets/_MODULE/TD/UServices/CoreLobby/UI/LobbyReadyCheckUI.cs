using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.Infrastructure;
using TD.UServices.CoreLobby.UI;
using UnityEngine;

namespace TD.UServices.CoreLobby.UI
{
    public class LobbyReadyCheckUI : UIPanelBase
    {
        public void OnClick_Ready()
        {
            ChangeState(PlayerStatus.Ready);
        }
        public void OnClick_Cancel()
        {
            ChangeState(PlayerStatus.Lobby);
        }
        void ChangeState(PlayerStatus status)
        {
            Manager.SetLocalUserStatus(status);
        }
    }

}
