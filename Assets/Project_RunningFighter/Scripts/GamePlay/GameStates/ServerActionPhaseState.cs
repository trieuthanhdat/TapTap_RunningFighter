using Project_RunningFighter.ConnectionManagement;
using Project_RunningFighter.Data;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.Gameplay.GameplayObjects;
using Project_RunningFighter.Gameplay.Messages;
using Project_RunningFighter.Infrastruture;
using Project_RunningFighter.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using VContainer;
using static Project_RunningFighter.Gameplay.GameplayObjects.Characters.NetworkLifeState;
using UnityEngine.UI;
using TMPro;
using Project_RunningFighter.Gameplay.UI;
using TD.MonoAudioSFX;

namespace Project_RunningFighter.Gameplay.GameStates
{
    
    [RequireComponent(typeof(NetcodeHooks))]
    public class ServerActionPhaseState : GameStateBehaviour
    {
        [FormerlySerializedAs("m_NetworkWinState")]
        [SerializeField] PersistentGameState persistentGameState;
        [SerializeField] NetcodeHooks m_NetcodeHooks;

        [SerializeField]
        [Tooltip("Make sure this is included in the NetworkManager's list of prefabs!")]
        private NetworkObject m_PlayerPrefab;
        [SerializeField]
        [Tooltip("A collection of locations for spawning players")]
        private Transform[] m_PlayerSpawnPoints;

        #region ____SINGLETON____
        private static ServerActionPhaseState instance = null;
        public static ServerActionPhaseState Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<ServerActionPhaseState>();

                return instance;
            }
        }
        #endregion
        #region ___PROPERTIES___

        private List<Transform> m_PlayerSpawnPointsList = new List<Transform>();
        private NetworkVariable<int> m_PlayerEnterteringWinBarrierCount = new NetworkVariable<int>(0);

        private ulong m_WinningPlayer = default;
        public ulong WinningPlayer => m_WinningPlayer;

        public override GameState ActiveState => GameState.ActionPhase;
        
        private const float k_WinDelay = 7.0f;
        private const float k_LoseDelay = 2.5f;
        public bool InitialSpawnDone { get; private set; }

        [Inject] ISubscriber<LifeStateChangedEventMessage> m_LifeStateChangedEventMessageSubscriber;

        [Inject] ConnectionManager m_ConnectionManager;
        [Inject] PersistentGameState m_PersistentGameState;

        public static event Action<PlayerSpawnedSetup> OnPlayerSpawned;
        public struct PlayerSpawnedSetup
        {
            public int spawnIndex;
            public Transform playerTrans;
            public Transform spawnPosTrans;
            public PlayerSpawnedSetup(int index, Transform player, Transform spawnPos)
            {
                spawnIndex = index;
                playerTrans = player;
                spawnPosTrans = spawnPos;
            }
        }
        #endregion

        #region UNITY METHODS___
        protected override void Awake()
        {
            base.Awake();
            m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;

            NetworkedActionPhaseState.OnGameplayStateChanged += OnGameplayStateChanged;
        }
        
        protected override void Start()
        {
            base.Start();
            MonoAudioManager.instance.StopSound("EntranceTheme");
            MonoAudioManager.instance.PlaySound("Music_1", true, true, 1f);
        }
        protected override void OnDestroy()
        {
            if (m_LifeStateChangedEventMessageSubscriber != null)
            {
                m_LifeStateChangedEventMessageSubscriber.Unsubscribe(OnLifeStateChangedEventMessage);
            }
            NetworkedActionPhaseState.OnGameplayStateChanged -= OnGameplayStateChanged;
            ClientCharacter.OnWinBarrierEntered -= ClientCharacter_OnWinBarrierEntered;
            if (m_NetcodeHooks)
            {
                m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }

            base.OnDestroy();
        }
        void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
                return;
            }
            m_PersistentGameState.Reset();
            m_LifeStateChangedEventMessageSubscriber.Subscribe(OnLifeStateChangedEventMessage);
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
            ClientCharacter.OnWinBarrierEntered += ClientCharacter_OnWinBarrierEntered;

            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();

            Debug.Log("SERVER ACTION PHASE STATE: start !!");
        }

       
        void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (m_LifeStateChangedEventMessageSubscriber != null)
                {
                    m_LifeStateChangedEventMessageSubscriber.Unsubscribe(OnLifeStateChangedEventMessage);
                }

                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
                NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
                ClientCharacter.OnWinBarrierEntered -= ClientCharacter_OnWinBarrierEntered;
            }
        }


        #endregion

        #region ____EVENT METHODS____
        private void ClientCharacter_OnWinBarrierEntered(ulong clientId)
        {
            if (m_PlayerEnterteringWinBarrierCount.Value == 0)
                m_WinningPlayer = clientId;

            m_PlayerEnterteringWinBarrierCount.Value += 1;
        }

        private void OnGameplayStateChanged(ActionPhaseState state)
        {
            if(state == ActionPhaseState.Finish)
            {
                //Set Win If Current Winninng player is LocalClient
                StartCoroutine(Co_GameOver(k_WinDelay, m_WinningPlayer == NetworkManager.Singleton.LocalClientId));
                MonoAudioManager.instance.StopSound("Music_1", true);
            }
        }

        void OnLifeStateChangedEventMessage(LifeStateChangedEventMessage message)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            switch (message.CharacterType)
            {
                case CharacterTypeEnum.SkyLife:
                case CharacterTypeEnum.EarthLife:
                case CharacterTypeEnum.WaterLife:
                    // Every time a player's life state changes to fainted we check to see if game is over
                    if (message.NewLifeState == CharacterLifeState.Dead)
                    {
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OnSynchronizeComplete(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            if (InitialSpawnDone && !PlayerServerCharacter.GetPlayerServerCharacter(clientId))
            {
                SpawnPlayer(clientId, true);
            }
        }

        void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            if (!InitialSpawnDone && loadSceneMode == LoadSceneMode.Single)
            {
                InitialSpawnDone = true;
                foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
                {
                    SpawnPlayer(kvp.Key, false);
                }
            }
        }

        void OnClientDisconnect(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                StartCoroutine(Co_GameOver(k_LoseDelay, false));
            }
        }
        #endregion

        #region ____PLAYERS UTILS____
        int count = 0;
        void SpawnPlayer(ulong clientId, bool lateJoin)
        {

            if (!NetworkManager.Singleton.IsServer) return;
            Transform spawnPoint = null;

            if (m_PlayerSpawnPointsList == null || m_PlayerSpawnPointsList.Count == 0)
            {
                m_PlayerSpawnPointsList = new List<Transform>(m_PlayerSpawnPoints);
            }

            Debug.Assert(m_PlayerSpawnPointsList.Count > 0,
                $"PlayerSpawnPoints array should have at least 1 spawn points.");

            int index = UnityEngine.Random.Range(0, m_PlayerSpawnPointsList.Count);
            spawnPoint = m_PlayerSpawnPointsList[index];
            m_PlayerSpawnPointsList.RemoveAt(index);

            var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);

            var newPlayer = Instantiate(m_PlayerPrefab, Vector3.zero, Quaternion.identity);

            var newPlayerCharacter = newPlayer.GetComponent<ServerCharacter>();

            var physicsTransform = newPlayerCharacter.physicsWrapper.Transform;

            if (spawnPoint != null)
            {
                physicsTransform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }

            var persistentPlayerExists = playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer);
            Assert.IsTrue(persistentPlayerExists,
                $"Matching persistent PersistentPlayer for client {clientId} not found!");

            // pass character type from persistent player to avatar
            var networkAvatarGuidStateExists =
                newPlayer.TryGetComponent(out NetworkAvatarGuidState networkAvatarGuidState);

            Assert.IsTrue(networkAvatarGuidStateExists,
                $"NetworkCharacterGuidState not found on player avatar!");

            // if reconnecting, set the player's position and rotation to its previous state
            if (lateJoin)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    physicsTransform.SetPositionAndRotation(sessionPlayerData.Value.PlayerPosition, sessionPlayerData.Value.PlayerRotation);
                }
            }

            // instantiate new NetworkVariables with a default value to ensure they're ready for use on OnNetworkSpawn
            networkAvatarGuidState.AvatarGuid = new NetworkVariable<NetworkGuid>(persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value);

            // pass name from persistent player to avatar
            if (newPlayer.TryGetComponent(out NetworkNameState networkNameState))
            {
                networkNameState.Name = new NetworkVariable<FixedPlayerName>(persistentPlayer.NetworkNameState.Name.Value);
            }

            // spawn players characters with destroyWithScene = true
            newPlayer.SpawnWithOwnership(clientId, true);
            OnPlayerSpawnedRpc(newPlayer.transform, spawnPoint);
            count++;
        }
        [Rpc(SendTo.ClientsAndHost)]
        private void OnPlayerSpawnedRpc(Transform playerTrans, Transform spawnPoint)
        {
            OnPlayerSpawned?.Invoke(new PlayerSpawnedSetup(count, playerTrans, spawnPoint));
        }
        #endregion

        #region ____GAME OVER METHODS____
        IEnumerator WaitToCheckForGameOver()
        {
            if (!NetworkManager.Singleton.IsServer) yield break;
            yield return null;
            CheckForGameOver();
        }
        void CheckForGameOver()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            foreach (var serverCharacter in PlayerServerCharacter.GetPlayerServerCharacters())
            {
                if (serverCharacter && serverCharacter.CharacterLifeState == CharacterLifeState.Alive)
                {
                    return;
                }
            }
            StartCoroutine(Co_GameOver(k_LoseDelay, false));
        }

        IEnumerator Co_GameOver(float wait, bool gameWon)
        {
            if (!NetworkManager.Singleton.IsServer) yield break;
            m_PersistentGameState.SetWinState(gameWon ? WinState.Win : WinState.Loss);

            // wait 5 seconds for game animations to finish
            yield return new WaitForSeconds(wait);

            SceneLoaderWrapper.Instance.LoadScene("PostGame", useNetworkSceneManager: true);
        }
        #endregion
    }

}
