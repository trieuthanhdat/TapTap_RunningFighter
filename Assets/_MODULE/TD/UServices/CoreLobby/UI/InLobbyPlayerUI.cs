using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TD.UServices.CoreLobby.UI
{
    public class InLobbyPlayerUI : UIPanelBase
    {
        [SerializeField]
        TMP_Text m_DisplayNameText;

        [SerializeField]
        TMP_Text m_StatusText;

        [SerializeField]
        Image m_HostIcon;

        /*[SerializeField]
        vivox.VivoxUserHandler m_VivoxUserHandler;*/

        public bool IsAssigned => UserId != null;
        public string UserId { get; set; }
        LocalPlayer m_LocalPlayer;

        public virtual void SetUser(LocalPlayer localPlayer)
        {
            Show();
            m_LocalPlayer = localPlayer;
            UserId = localPlayer.ID.Value;
            SetIsHost(localPlayer.IsHost.Value);
            SetUserStatus(localPlayer.UserStatus.Value);
            SetDisplayName(m_LocalPlayer.DisplayName.Value);
            SubscribeToPlayerUpdates();

            //m_VivoxUserHandler.SetId(UserId);
        }

        protected virtual void SubscribeToPlayerUpdates()
        {
            m_LocalPlayer.DisplayName.onChanged += SetDisplayName;
            m_LocalPlayer.UserStatus.onChanged  += SetUserStatus;
            m_LocalPlayer.IsHost.onChanged      += SetIsHost;
        }

        protected virtual void UnsubscribeToPlayerUpdates()
        {
            if (m_LocalPlayer == null)
                return;
            if (m_LocalPlayer.DisplayName?.onChanged != null)
                m_LocalPlayer.DisplayName.onChanged  -= SetDisplayName;
            if (m_LocalPlayer.UserStatus?.onChanged  != null)
                m_LocalPlayer.UserStatus.onChanged   -= SetUserStatus;
            if (m_LocalPlayer.IsHost?.onChanged      != null)
                m_LocalPlayer.IsHost.onChanged       -= SetIsHost;
        }

        public virtual void ResetUI()
        {
            if (m_LocalPlayer == null)
                return;
            UserId = null;
            SetUserStatus(PlayerStatus.Lobby);
            Hide();
            UnsubscribeToPlayerUpdates();
            m_LocalPlayer = null;
        }

        protected virtual void SetDisplayName(string displayName)
        {
            if(m_DisplayNameText) m_DisplayNameText.SetText(displayName);
        }

        protected virtual void SetUserStatus(PlayerStatus statusText)
        {
            if(m_StatusText) m_StatusText.SetText(SetStatusFancy(statusText));
        }

        protected virtual void SetIsHost(bool isHost)
        {
            if (m_HostIcon) m_HostIcon.enabled = isHost;
        }

        protected virtual string SetStatusFancy(PlayerStatus status)
        {
            switch (status)
            {
                case PlayerStatus.Lobby:
                    return "<color=#56B4E9>In Lobby</color>"; // Light Blue
                case PlayerStatus.Ready:
                    return "<color=#009E73>Ready</color>"; // Light Mint
                case PlayerStatus.Connecting:
                    return "<color=#F0E442>Connecting...</color>"; // Bright Yellow
                case PlayerStatus.InGame:
                    return "<color=#005500>In Game</color>"; // Green
                default:
                    return "";
            }
        }
    }
}


