using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.UServices.CoreLobby.UI
{
    public class CreateLobbyButtonUI : UIPanelBase
    {
        public void OnClick_StartLobby()
        {
            Manager.UIChangeMenuState(Core.CoreGameManager.GameState.JoinMenu);
            
        }
    }

}
