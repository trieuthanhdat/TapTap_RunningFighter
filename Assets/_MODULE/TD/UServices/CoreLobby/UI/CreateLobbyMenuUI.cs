using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TD.UServices.CoreLobby.UI
{
    public class CreateLobbyMenuUI : UIPanelBase
    {
        [Header("REFERENCES")]
        [SerializeField] protected Button m_CreateButton;
        [SerializeField] protected TMP_InputField m_PasswordInputField;
        [SerializeField] protected TMP_InputField m_RoomNameInputField;
        [SerializeField] protected JoinCreateLobbyUI m_JoinCreateLobbyUI;
        
        protected string m_ServerName;
        protected string m_ServerPassword;
        protected bool   m_IsServerPrivate;

        public override void Start()
        {
            base.Start();
            if (m_CreateButton)
            {
                m_CreateButton.interactable = false;
                m_CreateButton.onClick.AddListener(OnClick_CreateRoom);
            }
            if(m_JoinCreateLobbyUI) m_JoinCreateLobbyUI.m_OnTabChanged.AddListener(OnTabChanged);
            if (m_PasswordInputField) m_PasswordInputField.onValueChanged.AddListener(SetServerPassword);
            if (m_RoomNameInputField) m_RoomNameInputField.onValueChanged.AddListener(SetServerName);
        }
        protected void OnTabChanged(JoinCreateTabs tabState)
        {
            if (tabState == JoinCreateTabs.Create)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void SetServerName(string serverName)
        {
            m_ServerName = serverName;
            m_CreateButton.interactable = ValidateServerName(m_ServerName) && ValidatePassword(m_ServerPassword);
        }

        public void SetServerPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                password = null;
            m_ServerPassword = password;
            m_CreateButton.interactable = ValidatePassword(m_ServerPassword) && ValidateServerName(m_ServerName);
        }

        public void SetServerPrivate(bool priv)
        {
            m_IsServerPrivate = priv;
        }

        public void OnClick_CreateRoom()
        {
            Manager.CreateLobby(m_ServerName, m_IsServerPrivate, m_ServerPassword);
        }

        /// Lobby Service only allows passwords greater than 6 and less than 64 characters
        /// Null is also an option, meaning No password.
        protected bool ValidatePassword(string password)
        {
            if (password == null)
                return true;
            var passwordLength = password.Length;
            if (passwordLength < 1)
                return true;
            return passwordLength is >= 6 and <= 64;
        }

        protected bool ValidateServerName(string serverName)
        {
            var serverNameLength = serverName.Length;

            return serverNameLength is > 0 and <= 64;
        }
    }

}
