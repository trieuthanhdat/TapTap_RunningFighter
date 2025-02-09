using System;
using TD.UServices.CoreLobby.Infrastructure;
using UnityEngine;


namespace TD.UServices.CoreLobby.UI
{
    /// <summary>
    /// User permission type. It's a flag enum to allow for the Inspector to select multiples for various UI features.
    /// </summary>
    [Flags]
    public enum UserPermission
    {
        Client = 1,
        Host = 2
    }

    /// <summary>
    /// Shows the UI when the LocalPlayer matches some conditions, including having the target permissions.
    /// </summary>
    public class LobbyPlayerVisibilityStateUI : UIPanelBase
    {
        public PlayerStatus ShowThisWhen;
        public UserPermission Permissions;
        bool m_HasStatusFlags = false;
        bool m_HasPermissions;

        public override async void Start()
        {
            base.Start();
            var localUser = await Manager.AwaitLocalUserInitialization();

            localUser.IsHost.onChanged += OnUserHostChanged;

            localUser.UserStatus.onChanged += OnUserStatusChanged;
        }

        void OnUserStatusChanged(PlayerStatus observedStatus)
        {
            m_HasStatusFlags = ShowThisWhen.HasFlag(observedStatus);
            CheckVisibility();
        }

        void OnUserHostChanged(bool isHost)
        {
            m_HasPermissions = false;
            if (Permissions.HasFlag(UserPermission.Host) && isHost)
            {
                m_HasPermissions = true;
            }

            if (Permissions.HasFlag(UserPermission.Client) && !isHost)
            {
                m_HasPermissions = true;
            }

            CheckVisibility();
        }

        void CheckVisibility()
        {
            if (m_HasStatusFlags && m_HasPermissions)
                Show();
            else
                Hide();
        }
    }
}
