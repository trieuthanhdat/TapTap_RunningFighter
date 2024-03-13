using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;


namespace TD.UServices.CoreLobby.UI
{
    public class LobbyEntryUI : MonoBehaviour
    {
        [SerializeField] protected ColorLobbyUI m_ColorLobbyUI;
        [SerializeField] protected TMP_Text lobbyNameText;
        [SerializeField] protected TMP_Text lobbyCountText;
      
        public UnityEvent<LocalLobby> onLobbyPressed;
        LocalLobby m_Lobby;

        public void OnLobbyClicked()
        {
            onLobbyPressed.Invoke(m_Lobby);
        }

        public void SetLobby(LocalLobby lobby)
        {
            m_Lobby = lobby;
            SetLobbyname(m_Lobby.LobbyName.Value);
            SetLobbyCount(m_Lobby.PlayerCount);
            SetLobbyColor(lobby);

            m_Lobby.LobbyName.onChanged += SetLobbyname;
            m_Lobby.onUserJoined += (_) =>
            {
                SetLobbyCount(m_Lobby.PlayerCount);
            };
            m_Lobby.onUserLeft += (_) =>
            {
                SetLobbyCount(m_Lobby.PlayerCount);
            };
        }
        void SetLobbyColor(LocalLobby lobby)
        {
            if (m_ColorLobbyUI) m_ColorLobbyUI.SetLobby(lobby);
        }
        void SetLobbyname(string lobbyName)
        {
            if (lobbyNameText) lobbyNameText.SetText(m_Lobby.LobbyName.Value);
        }

        void SetLobbyCount(int count)
        {
            if (lobbyCountText) lobbyCountText.SetText($"{count}/{m_Lobby.MaxPlayerCount.Value}");
        }
    }
}
