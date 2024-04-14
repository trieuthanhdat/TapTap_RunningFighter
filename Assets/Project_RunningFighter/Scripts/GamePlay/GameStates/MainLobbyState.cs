using Project_RunningFighter.Data;
using Project_RunningFighter.Gameplay.UI;
using Project_RunningFighter.Utils;
using System;
using TD.UServices.Authentication;
using TD.UServices.Lobbies;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Project_RunningFighter.Gameplay.GameStates
{
    public class MainLobbyState : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.MainLobby;

        [SerializeField] NameGenerationData m_NameGenerationData;
        [SerializeField] Button             m_LobbyButton;
        [SerializeField] GameObject         m_SignInSpinner;
        [SerializeField] UIProfileSelector  m_UIProfileSelector;
        [SerializeField] LobbyUIMediator    m_LobbyUIMediator;
        [SerializeField] GameModePanel      m_GameModePanel;
        [SerializeField] IPUIMediator       m_IPUIMediator;

        [Inject] ProfileManager m_ProfileManager;
        [Inject] LocalLobby m_LocalLobby;
        [Inject] LocalLobbyUser m_LocalUser;
        [Inject] AuthenticationServiceFacade m_AuthServiceFacade;
        protected override void Awake()
        {
            base.Awake();

            if (m_LobbyButton) m_LobbyButton.interactable = false;
            if (m_LobbyUIMediator) m_LobbyUIMediator.Hide();
            if (m_GameModePanel) m_GameModePanel.Show();
            Debug.Log("MAIN LOBBY STATE: cloud id "+ Application.cloudProjectId);
            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                OnSignInFailed();
                return;
            }
            TrySignIn();
        }
        private async void TrySignIn()
        {
            try
            {
                var unityAuthenticationInitOptions = 
                    m_AuthServiceFacade.GenerateAuthenticationOptions(m_ProfileManager.Profile);

                await m_AuthServiceFacade.InitializeAndSignInAsync(unityAuthenticationInitOptions);
                OnAuthSignIn();
                m_ProfileManager.onProfileChanged += OnProfileChanged;
            }
            catch (Exception)
            {
                OnSignInFailed();
            }
        }

        private void OnAuthSignIn()
        {
            if(m_LobbyButton) m_LobbyButton.interactable = true;
            //m_UGSSetupTooltipDetector.enabled = false;
            if (m_SignInSpinner) m_SignInSpinner.SetActive(false);

            Debug.Log($"MAIN LOBBY STATE:Signed in. Unity Player ID {AuthenticationService.Instance.PlayerId}");

            m_LocalUser.ID = AuthenticationService.Instance.PlayerId;

            m_LocalLobby.AddUser(m_LocalUser);
        }

        private void OnSignInFailed()
        {
            if (m_LobbyButton)
            {
                m_LobbyButton.interactable = false;
                //m_UGSSetupTooltipDetector.enabled = true;
            }

            if (m_SignInSpinner)
            {
                m_SignInSpinner.SetActive(false);
            }
        }
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(m_NameGenerationData);
            builder.RegisterComponent(m_LobbyUIMediator);
            builder.RegisterComponent(m_IPUIMediator);
        }
        protected override void OnDestroy()
        {
            m_ProfileManager.onProfileChanged -= OnProfileChanged;
            base.OnDestroy();
        }
        async void OnProfileChanged()
        {
            if(m_LobbyButton) m_LobbyButton.interactable = false;
            if(m_SignInSpinner) m_SignInSpinner.SetActive(true);
            await m_AuthServiceFacade.SwitchProfileAndReSignInAsync(m_ProfileManager.Profile);

            if (m_LobbyButton) m_LobbyButton.interactable = true;
            if (m_SignInSpinner) m_SignInSpinner.SetActive(false);

            Debug.Log($"ON PROFILE CHANGED: Signed in. Unity Player ID {AuthenticationService.Instance.PlayerId}");

            // Updating LocalUser and LocalLobby
            m_LocalLobby.RemoveUser(m_LocalUser);
            m_LocalUser.ID = AuthenticationService.Instance.PlayerId;
            m_LocalLobby.AddUser(m_LocalUser);
        }
        public void OnBackBtnClicked()
        {
            if (m_GameModePanel) m_GameModePanel.Show();
            if (m_LobbyUIMediator) m_LobbyUIMediator.Hide();
        }
        public void OnCreateLobbyClicked()
        {
            if (m_GameModePanel) m_GameModePanel.Hide();
            if (m_LobbyUIMediator)
            {
                m_LobbyUIMediator.ToggleCreateLobbyUI();
                m_LobbyUIMediator.Show();
            }
            
        }
        public void OnJoinLobbyClicked()
        {
            if (m_GameModePanel) m_GameModePanel.Hide();
            if (m_LobbyUIMediator)
            {
                m_LobbyUIMediator.ToggleJoinLobbyUI();
                m_LobbyUIMediator.Show();
            }
              
        }

        public void OnDirectIPClicked()
        {
            if (m_LobbyUIMediator) m_LobbyUIMediator.Hide();
            if (m_IPUIMediator) m_IPUIMediator.Show();
        }

        public void OnChangeProfileClicked()
        {
            if(m_UIProfileSelector) m_UIProfileSelector.Show();
        }
    }

}
