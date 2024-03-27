using System.Collections;
using System.Collections.Generic;
using TD.UServices.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TD.UServices.CoreLobby.UI
{
    public class GameStateVisibilityManager : MonoSingleton<GameStateVisibilityManager>   
    {
        [SerializeField] private JoinCreateLobbyUI joinCreateLobbyUI;
        public void ToGameStatePanel(CoreGameManager.GameState state)
        {
            CoreGameManager.instance.UIChangeMenuState(state);
        }

        public void ToLobbySingleMode()
        {
            SceneManager.LoadScene("GameplayScene");
        }

        public void ToLobbyJoinTab()
        {
            ToJoinMenu();
            if (joinCreateLobbyUI) joinCreateLobbyUI.SetJoinTab();
        }
        public void ToLobbyCreateTab()
        {
            ToJoinMenu();
            if (joinCreateLobbyUI) joinCreateLobbyUI.SetCreateTab();
        }
        public void ToJoinMenu()
        {
            ToGameStatePanel(CoreGameManager.GameState.JoinMenu);
        }

        public void ToMenu()
        {
            ToGameStatePanel(CoreGameManager.GameState.Menu);
        }
    }

}

