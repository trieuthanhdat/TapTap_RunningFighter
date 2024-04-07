using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TD.UServices.Authentication;
using TD.UServices.CoreLobby;
using TD.UServices.CoreLobby.Infrastructure;
using TD.UServices.CoreLobby.UI;
using TD.UServices.CoreLobby.Utilities;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Utils;

namespace TD.UServices.Core
{
    
    public class CoreGameManager : MonoSingleton<CoreGameManager>
    {
        [Flags]
        public enum GameState
        {
            Menu = 1,
            Lobby = 2,
            JoinMenu = 4,
        }
        [SerializeField] private bool m_AnonimouslySignInRefresh = false;
        [SerializeField] private LobbyCountDown m_countdown;
        [SerializeField] private LobbyInGameSetup m_setupInGame;

        [SerializeField] private Camera UICamera;
        [SerializeField] private Camera LobbyCamera;

        #region ____LOBBY EVENTS____
        public Action<GameState> onGameStateChanged;
        #endregion

        #region ____LOBBY PUBLIC VARIABLES____
        public LocalLobby LocalLobby => m_LocalLobby;
        public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();
        public GameState LocalGameState { get; private set; }
        public LobbyManager LobbyManager { get; private set; }
        #endregion

        protected LocalPlayer m_LocalUser;
        protected LocalLobby m_LocalLobby;

        #region ____LOBBY COLOR____
        protected LobbyColor m_lobbyColorFilter;
        public void SetLocalLobbyColor(int color)
        {
            if (m_LocalLobby.PlayerCount < 1)
                return;
            m_LocalLobby.LocalLobbyColor.Value = (LobbyColor)color;
            SendLocalLobbyData();
        }
        public void SetLobbyColorFilter(int color)
        {
            m_lobbyColorFilter = (LobbyColor)color;
        }
        private async void SendLocalLobbyData()
        {
            await LobbyManager.UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(m_LocalLobby));
        }
        private void OnLobbyStateChanged(LobbyState state)
        {
            if (state == LobbyState.Lobby)
                CancelCountDown();
            if (state == LobbyState.CountDown)
                BeginCountDown();
        }
        
        #endregion
        #region ____PLAYER STATUS____
        public void SetLocalUserStatus(PlayerStatus status)
        {
            m_LocalUser.UserStatus.Value = status;
            SendLocalUserData();
        }
        private void OnPlayersReady(int readyCount)
        {
            if (readyCount == m_LocalLobby.PlayerCount &&
                m_LocalLobby.LocalLobbyState.Value != LobbyState.CountDown)
            {
                m_LocalLobby.LocalLobbyState.Value = LobbyState.CountDown;
                SendLocalLobbyData();
            }
            else if (m_LocalLobby.LocalLobbyState.Value == LobbyState.CountDown)
            {
                m_LocalLobby.LocalLobbyState.Value = LobbyState.Lobby;
                SendLocalLobbyData();
            }
        }

        private async void SendLocalUserData()
        {
            await LobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(m_LocalUser));
        }
        #endregion
        #region ____COUNT DOWN____
        private void BeginCountDown()
        {
            Debug.Log("Beginning Countdown.");
            m_countdown.StartCountDown();
        }

        private void CancelCountDown()
        {
            Debug.Log("Countdown Cancelled.");
            m_countdown.CancelCountDown();
        }

        public void FinishedCountDown()
        {
            m_LocalUser.UserStatus.Value = PlayerStatus.InGame;
            m_LocalLobby.LocalLobbyState.Value = LobbyState.InGame;
            m_setupInGame.StartNetworkedGame(m_LocalLobby, m_LocalUser);
        }
        #endregion

        #region ____SET UP____
        public void ClientQuitGame()
        {
            EndGame();
            m_setupInGame?.OnGameEnd();
        }
        public void BeginGame()
        {
            if (m_LocalUser.IsHost.Value)
            {
                m_LocalLobby.LocalLobbyState.Value = LobbyState.InGame;
                m_LocalLobby.Locked.Value = true;
                SendLocalLobbyData();
            }
        }
        public void EndGame()
        {
            if (m_LocalUser.IsHost.Value)
            {
                m_LocalLobby.LocalLobbyState.Value = LobbyState.Lobby;
                m_LocalLobby.Locked.Value = false;
                SendLocalLobbyData();
            }

            SetLobbyView();
        }
        async void Awake()
        {
            Application.wantsToQuit += OnWantToQuit;
            m_LocalUser  = new LocalPlayer("", 0, false, "LocalPlayer");
            m_LocalLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };
            LobbyManager = new LobbyManager();

            await InitializeServices();
            AuthenticatePlayer();
            //StartVivoxLogin();
        }

        private async Task InitializeServices()
        {
            //IF HAVE'NT SIGNED IN
            /*if (!UnityAutenticationManager.instance.IsSignIn || 
                 m_AnonimouslySignInRefresh)
            {
                string serviceProfileName = "player";
#if UNITY_EDITOR
                serviceProfileName = $"{serviceProfileName}{LocalProfileTool.LocalProfileSuffix}";
#endif
                await UnityAutenticationManager.TrySignInAsync(serviceProfileName);
            }
*/
        }

        void AuthenticatePlayer()
        {
            var playerID   = !m_AnonimouslySignInRefresh ? 
                              UnityAutenticationManager.instance.PlayerID   :
                              AuthenticationService.Instance.PlayerId;
            var playerName = !m_AnonimouslySignInRefresh ? 
                              UnityAutenticationManager.instance.PlayerName :
                              NameGenerator.GetName(playerID);

            m_LocalUser.ID.Value = playerID;
            m_LocalUser.DisplayName.Value = playerName;
            Debug.Log($"CORE GAME MANAGER: Enter game : playerID {playerID} - player name {playerName}");
        }

        #endregion

        #region ____MAIN METHODS____
        public async Task<LocalPlayer> AwaitLocalUserInitialization()
        {
            while (m_LocalUser == null)
                await Task.Delay(100);
            return m_LocalUser;
        }
        public void HostSetRelayCode(string code)
        {
            m_LocalLobby.RelayCode.Value = code;
            SendLocalLobbyData();
        }
        public async void CreateLobby(string name, bool isPrivate, string password = null, int maxPlayers = 4)
        {
            try
            {
                var lobby = await LobbyManager.CreateLobbyAsync(
                    name,
                    maxPlayers,
                    isPrivate,
                    m_LocalUser,
                    password);

                LobbyConverters.RemoteToLocal(lobby, m_LocalLobby);
                await CreateLobby();
            }
            catch (LobbyServiceException exception)
            {
                SetGameState(GameState.JoinMenu);
                LogHandlerSettings.Instance.SpawnErrorPopup($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
            }
        }
        private async Task CreateLobby()
        {
            m_LocalUser.IsHost.Value = true;
            m_LocalLobby.onUserReadyChange = OnPlayersReady;
            try
            {
                await BindLobby();
            }
            catch (LobbyServiceException exception)
            {
                SetGameState(GameState.JoinMenu);
                LogHandlerSettings.Instance.SpawnErrorPopup($"Couldn't join Lobby : ({exception.ErrorCode}) {exception.Message}");
            }
        }
        public async void JoinLobby(string lobbyID, string lobbyCode, string password = null)
        {
            try
            {
                var lobby = await LobbyManager.JoinLobbyAsync(lobbyID, lobbyCode,
                    m_LocalUser, password: password);

                LobbyConverters.RemoteToLocal(lobby, m_LocalLobby);
                await JoinLobby();
            }
            catch (LobbyServiceException exception)
            {
                SetGameState(GameState.JoinMenu);
                LogHandlerSettings.Instance.SpawnErrorPopup($"Error joining lobby : ({exception.ErrorCode}) {exception.Message}");
            }
        }

        public async void QueryLobbies()
        {
            LobbyList.QueryState.Value = LobbyQueryState.Fetching;
            var qr = await LobbyManager.RetrieveLobbyListAsync(m_lobbyColorFilter);
            if (qr == null)
            {
                return;
            }

            SetCurrentLobbies(LobbyConverters.QueryToLocalList(qr));
        }
        protected IEnumerator RetryConnection(Action doConnection, string lobbyId)
        {
            yield return new WaitForSeconds(5);
            if (m_LocalLobby != null && 
                m_LocalLobby.LobbyID.Value == lobbyId && 
                !string.IsNullOrEmpty(lobbyId)) // Ensure we didn't leave the lobby during this waiting period.
                doConnection?.Invoke();
        }
        void SetLobbyView()
        {
            Debug.Log($"Setting Lobby user state {GameState.Lobby}");
            SetGameState(GameState.Lobby);
            SetLocalUserStatus(PlayerStatus.Lobby);
            ToggleLobbyCamera(true);
        }
        void ToggleLobbyCamera(bool isOn)
        {
            if (UICamera) UICamera.gameObject.SetActive(!isOn);
            if (LobbyCamera) LobbyCamera.gameObject.SetActive(isOn);
        }
        public async void QuickJoin()
        {
            var lobby = await LobbyManager.QuickJoinLobbyAsync(m_LocalUser, m_lobbyColorFilter);
            if (lobby != null)
            {
                LobbyConverters.RemoteToLocal(lobby, m_LocalLobby);
                await JoinLobby();
            }
            else
            {
                SetGameState(GameState.JoinMenu);
            }
        }
        void SetCurrentLobbies(IEnumerable<LocalLobby> lobbies)
        {
            var newLobbyDict = new Dictionary<string, LocalLobby>();
            foreach (var lobby in lobbies)
                newLobbyDict.Add(lobby.LobbyID.Value, lobby);

            LobbyList.CurrentLobbies = newLobbyDict;
            LobbyList.QueryState.Value = LobbyQueryState.Fetched;
        }

        protected async Task JoinLobby()
        {
            //Trigger UI Even when same value
            m_LocalUser.IsHost.ForceSet(false);
            await BindLobby();
        }
        async Task BindLobby()
        {
            await LobbyManager.BindLocalLobbyToRemote(m_LocalLobby.LobbyID.Value, m_LocalLobby);
            m_LocalLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;
            SetLobbyView();
            //StartVivoxJoin();
        }
        public void LeaveLobby()
        {
            m_LocalUser.ResetState();
#pragma warning disable 4014
            LobbyManager.LeaveLobbyAsync();
#pragma warning restore 4014
            ResetLocalLobby();
            //m_VivoxSetup.LeaveLobbyChannel();
            LobbyList.Clear();
            ToggleLobbyCamera(false);
        }
        void ResetLocalLobby()
        {
            m_LocalLobby.ResetLobby();
            m_LocalLobby.RelayServer = null;
        }
        void SetGameState(GameState state)
        {
            var isLeavingLobby = (state == GameState.Menu || state == GameState.JoinMenu) &&
                LocalGameState == GameState.Lobby;
            LocalGameState = state;

            if (isLeavingLobby)
                LeaveLobby();
            onGameStateChanged?.Invoke(LocalGameState);
        }
        #endregion

        #region ____UTITLIES____
        public void UIChangeMenuState(GameState state)
        {
            var isQuittingGame = LocalGameState == GameState.Lobby &&
                m_LocalLobby.LocalLobbyState.Value == LobbyState.InGame;

            if (isQuittingGame)
            {
                //If we were in-game, make sure we stop by the lobby first
                state = GameState.Lobby;
                ClientQuitGame();
            }
            SetGameState(state);
        }
        /// In builds, if we are in a lobby and try to send a Leave request on application quit, it won't go through if we're quitting on the same frame.
        /// So, we need to delay just briefly to let the request happen (though we don't need to wait for the result).
        protected IEnumerator LeaveBeforeQuit()
        {
            ForceLeaveAttempt();
            yield return null;
            Application.Quit();
        }

        protected bool OnWantToQuit()
        {
            bool canQuit = string.IsNullOrEmpty(m_LocalLobby?.LobbyID.Value);
            StartCoroutine(LeaveBeforeQuit());
            return canQuit;
        }

        protected override void OnDestroy()
        {
            ForceLeaveAttempt();
            LobbyManager.Dispose();
            base.OnDestroy();
        }

        protected void ForceLeaveAttempt()
        {
            if (!string.IsNullOrEmpty(m_LocalLobby?.LobbyID.Value))
            {
#pragma warning disable 4014
                LobbyManager.LeaveLobbyAsync();
#pragma warning restore 4014
                m_LocalLobby = null;
            }
        }
        #endregion
    }

}
