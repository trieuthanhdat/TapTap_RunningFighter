using Project_RunningFighter.ConnectionManagement;
using Project_RunningFighter.Gameplay.Action;
using System.Collections;
using System.Collections.Generic;
using TD.MonoAudioSFX;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Project_RunningFighter.Gameplay.GameStates
{
    [RequireComponent(typeof(NetcodeHooks))]
    public class ServerPostGameState : GameStateBehaviour
    {
        [SerializeField]
        NetcodeHooks m_NetcodeHooks;

        [FormerlySerializedAs("synchronizedStateData")]
        [SerializeField] NetworkPostGame networkPostGame;
        public NetworkPostGame NetworkPostGame => networkPostGame;

        public override GameState ActiveState { get { return GameState.PostGame; } }

        [Inject]
        ConnectionManager m_ConnectionManager;

        [Inject] PersistentGameState m_PersistentGameState;

        protected override void Awake()
        {
            base.Awake();

            m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
        }

        void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            else
            {
                SessionManager<SessionPlayerData>.Instance.OnSessionEnded();
                networkPostGame.WinState.Value = m_PersistentGameState.WinState;
            }
        }

        protected override void OnDestroy()
        {
            //clear actions pool
            GameActionFactory.PurgePooledActions();
            m_PersistentGameState.Reset();

            base.OnDestroy();

            m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
        }

        public void PlayAgain()
        {
            SceneLoaderWrapper.Instance.LoadScene("CharacterSelectScene", useNetworkSceneManager: true);
        }

        public void GoToMainMenu()
        {
            m_ConnectionManager.RequestShutdown();
        }
    }
    public enum WinState
    {
        Invalid,
        Win,
        Loss
    }

    /// Class containing some data that needs to be passed between ServerBossRoomState and PostGameState to represent the game session's win state.
    public class PersistentGameState
    {
        public WinState WinState { get; private set; }

        public void SetWinState(WinState winState)
        {
            WinState = winState;
        }

        public void Reset()
        {
            WinState = WinState.Invalid;
        }
    }
}
