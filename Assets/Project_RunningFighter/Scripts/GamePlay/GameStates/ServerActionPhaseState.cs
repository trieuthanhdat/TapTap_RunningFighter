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
using TD.SerializableDictionary;

namespace Project_RunningFighter.Gameplay.GameStates
{
    [System.Serializable]
    public enum ActionPhaseState
    {
        ReadyUp,        //Before Start Game
        StartCountDown, //Count Down Then Start Game
        Playing,        //Playing
        EndCountDown,   //Count Down Then End Game
        Finish          //After Count Down
    }
    [RequireComponent(typeof(NetcodeHooks))]
    public class ServerActionPhaseState : GameStateBehaviour
    {
        [Header("UI REFERENCES")]
        [SerializeField] TextMeshProUGUI txt_CountDown;
        [SerializeField] TextMeshProUGUI txt_PlayingTimer;
        [SerializeField] Slider m_ProgressSlider;
        [SerializeField] Transform m_GroupPlayingTimer;
        [SerializeField] List<UIPlayerProgressIconBehaviour> m_ListPlayerProgressIcon;
        [Header("OBJECTS REFERENCES")]
        [SerializeField] Transform m_endPosition;
        [Header("STATE SETTINGS")]
        [SerializeField] float m_StartCountDownDuration = 3f; //In seconds
        [SerializeField] float m_EndCountDownDuration   = 3f; //In seconds
        [SerializeField] float m_PlayTimeDuration = 60f;      //In seconds
        [FormerlySerializedAs("m_NetworkWinState")]
        [SerializeField] PersistentGameState persistentGameState;
        [SerializeField] NetcodeHooks m_NetcodeHooks;

        [SerializeField]
        [Tooltip("Make sure this is included in the NetworkManager's list of prefabs!")]
        private NetworkObject m_PlayerPrefab;
        [SerializeField]
        [Tooltip("A collection of locations for spawning players")]
        private Transform[] m_PlayerSpawnPoints;
        #region ___PROPERTIES___
        //ACTION PHASE STATE
        public static event Action<ActionPhaseState> OnGameplayStateChanged;
        [SerializeField]
        ActionPhaseState m_GameplayState = ActionPhaseState.ReadyUp;
        public ActionPhaseState GameplayState => m_GameplayState;
        [SerializeField]
        private Dictionary<Transform, Transform> m_SpanwedPlayerTable = new Dictionary<Transform, Transform>();
        private List<Transform> m_PlayerSpawnPointsList = new List<Transform>();

        private Transform m_StartPosition;

        private float m_CountDownTimer;

        private bool m_IsReadyUp;
        public bool IsReadyUp => m_IsReadyUp;

        private bool m_IsPlaying;
        public bool IsPlaying => m_IsPlaying;
        public override GameState ActiveState => GameState.ActionPhase;
        
        private const float k_WinDelay = 7.0f;
        private const float k_LoseDelay = 2.5f;
        public bool InitialSpawnDone { get; private set; }

        [Inject] ISubscriber<LifeStateChangedEventMessage> m_LifeStateChangedEventMessageSubscriber;

        [Inject] ConnectionManager m_ConnectionManager;
        [Inject] PersistentGameState m_PersistentGameState;
        #endregion

        #region UNITY METHODS___
        protected override void Awake()
        {
            base.Awake();
            m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }
        private void Update()
        {
            UpdateGameplayState();
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

            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();

            Debug.Log("SERVER ACTION PHASE STATE: start !!");
        }
        
        void OnNetworkDespawn()
        {
            if (m_LifeStateChangedEventMessageSubscriber != null)
            {
                m_LifeStateChangedEventMessageSubscriber.Unsubscribe(OnLifeStateChangedEventMessage);
            }

            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
        }

        protected override void OnDestroy()
        {
            if (m_LifeStateChangedEventMessageSubscriber != null)
            {
                m_LifeStateChangedEventMessageSubscriber.Unsubscribe(OnLifeStateChangedEventMessage);
            }


            if (m_NetcodeHooks)
            {
                m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }

            base.OnDestroy();
        }
        #endregion

        #region ____EVENT METHODS____
        void OnLifeStateChangedEventMessage(LifeStateChangedEventMessage message)
        {
            switch (message.CharacterType)
            {
                case CharacterTypeEnum.SkyLife:
                case CharacterTypeEnum.EarthLife:
                case CharacterTypeEnum.WaterLife:
                    // Every time a player's life state changes to fainted we check to see if game is over
                    if (message.NewLifeState == CharacterLifeState.Fainted)
                    {
                        CheckForGameOver();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OnSynchronizeComplete(ulong clientId)
        {
            if (InitialSpawnDone && !PlayerServerCharacter.GetPlayerServerCharacter(clientId))
            {
                SpawnPlayer(clientId, true);
                ChangeGameplayState(ActionPhaseState.ReadyUp);
            }
        }

        void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
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
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                StartCoroutine(WaitToCheckForGameOver());
            }
        }
        #endregion
       
        #region ____STATE MANAGEMENT____
        void ChangeGameplayState(ActionPhaseState newState)
        {
            m_GameplayState = newState;
            ChangeGameplayStateRpc(newState);
        }
        [Rpc(SendTo.Everyone)]
        public void ChangeGameplayStateRpc(ActionPhaseState newState)
        {
            OnGameplayStateChanged?.Invoke(newState);
        }
        void UpdateGameplayState()
        {
            switch(m_GameplayState)
            {
                case ActionPhaseState.ReadyUp:
                    if (m_IsReadyUp) return;

                    ToggleTextCountDown(false);
                    TogglePlayingTimer(false);
                    m_IsPlaying = false;
                    m_CountDownTimer = m_StartCountDownDuration;
                    m_IsReadyUp = true;
                    StartCoroutine(Co_WaitAlittle());
                    IEnumerator Co_WaitAlittle()
                    {
                        yield return new WaitForSeconds(0.5f);
                        ChangeGameplayState(ActionPhaseState.StartCountDown);
                    }
                    break;
                case ActionPhaseState.StartCountDown:
                    m_CountDownTimer -= Time.unscaledDeltaTime;

                    if (IsCountDownTimerFinished(ref m_CountDownTimer))
                    {
                        ChangeGameplayState(ActionPhaseState.Playing);
                    }
                    UpdateTextCountDown(m_CountDownTimer, m_CountDownTimer > 0);
                    break;
                case ActionPhaseState.Playing:
                    if (!m_IsPlaying)
                    {
                        m_IsPlaying = true;
                        m_CountDownTimer = m_PlayTimeDuration;
                    }
                    else
                    {
                        m_CountDownTimer -= Time.unscaledDeltaTime;
                        UpdatePlayersProgess();
                        //Will change To EndCount Down When There's only 3 seconds left
                        if (IsCountDownTimerFinished(ref m_CountDownTimer, 3f))
                        {
                            ChangeGameplayState(ActionPhaseState.EndCountDown);
                        }
                        UpdateTextTimePlaying(m_CountDownTimer);
                    }
                    break;
                case ActionPhaseState.EndCountDown:
                    m_CountDownTimer -= Time.unscaledDeltaTime;
                    UpdatePlayersProgess();
                    if (IsCountDownTimerFinished(ref m_CountDownTimer))
                    {
                        ChangeGameplayState(ActionPhaseState.Finish);
                    }
                    UpdateTextTimePlaying(m_CountDownTimer);
                    UpdateTextCountDown(m_CountDownTimer, m_CountDownTimer > 0);
                    break;
                case ActionPhaseState.Finish:
                    //FINISH GAME 
                    Debug.Log("FINISH GAME!!!");
                    m_IsPlaying = false;
                    break;
            }
        }
        bool IsCountDownTimerFinished(ref float Timer, float endTime = 0)
        {
            if (m_CountDownTimer <= endTime)
            {
                Timer = endTime;
                return true;
            }
            return false;
        }
        #endregion
        #region ____UI METHODS____
        /*private void SetupListPlayerProgressIcons()
        {
            for (int i = 0; i < m_ListPlayerProgressIcon.Count; i++) 
            { 
                var icon = m_ListPlayerProgressIcon[i];
                icon.Init();
            }
        }*/
        private string FormatTime(float timeInSeconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);

            if (timeInSeconds >= 60)
            {
                // Format as mm:ss
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                // Format as ss
                return $"{timeSpan.Seconds:D2}s";
            }
        }
        void UpdateTextTimePlaying(float time, bool isActive = true)
        {
            if (!m_GroupPlayingTimer) return;
            if (txt_PlayingTimer) txt_PlayingTimer.text = FormatTime((int)time);
            if (!isActive) StartCoroutine(Co_DeactivatePlayingTimer());
            else TogglePlayingTimer(true);

            IEnumerator Co_DeactivatePlayingTimer()
            {
                yield return new WaitForSeconds(0.5f);
                TogglePlayingTimer(false);
            }
        }
        
        void TogglePlayingTimer(bool isActive)
        {
            if (!m_GroupPlayingTimer) return;
            m_GroupPlayingTimer.gameObject.SetActive(isActive);
        }
        void UpdateTextCountDown(float time, bool isActive = true)
        {
            if (!txt_CountDown) return;
            int intTime = (int)time;
            txt_CountDown.text = intTime.ToString();

            if (!isActive) StartCoroutine(Co_DeactivateTextCountDown());
            else ToggleTextCountDown(true);

            IEnumerator Co_DeactivateTextCountDown()
            {
                yield return new WaitForSeconds(0.5f);
                ToggleTextCountDown(false);
            }
        }
        void ToggleTextCountDown(bool isActive)
        {
            if (!txt_CountDown) return;
            txt_CountDown.gameObject.SetActive(isActive);
        }

        #endregion
        #region ____PLAYERS UTILS____
        void UpdatePlayersProgess()
        {
            if (m_endPosition == null) return;
            if (m_SpanwedPlayerTable == null || m_SpanwedPlayerTable.Count == 0) return;

            // Calculate the width of the Fill Area of the slider
            RectTransform fillRect = m_ProgressSlider.fillRect.GetComponent<RectTransform>();
            float fillAreaWidth = fillRect.rect.width;
            int count = m_PlayerSpawnPoints.Length;
            
            for (int i = 0; i < count; i++)
            {
                Transform playerStartPos = m_PlayerSpawnPoints[i];

                m_SpanwedPlayerTable.TryGetValue(playerStartPos, out Transform player);
                if (player == null) continue;
                var playerIcon = m_ListPlayerProgressIcon[i];
                float totalDistance = m_endPosition.position.x - playerStartPos.position.x;

                float distanceToPlayer = Vector3.Distance(playerStartPos.position, player.transform.position);
                float sliderValue = distanceToPlayer / totalDistance;

                sliderValue = Mathf.Clamp01(sliderValue);
                Debug.Log($"UPDATE PLAYER PROGRESS: slider value {sliderValue} - distanceToPlayer {distanceToPlayer}");
                //The Slider's Handle Only for Host Player
                if (playerIcon.name.ToLower().Contains("server") || playerIcon.name.ToLower().Contains("host"))
                {
                    m_ProgressSlider.value = sliderValue;
                }else
                {
                    float iconXPosition = fillAreaWidth * sliderValue;
                    playerIcon.ChangePositionX(iconXPosition);
                }
                
            }
        }
        private int m_SpawnIndex = 0;
        void SpawnPlayer(ulong clientId, bool lateJoin)
        {
            Transform spawnPoint = null;

            if (m_PlayerSpawnPointsList == null || m_PlayerSpawnPointsList.Count == 0)
            {
                m_PlayerSpawnPointsList = new List<Transform>(m_PlayerSpawnPoints);
            }

            Debug.Assert(m_PlayerSpawnPointsList.Count > 0,
                $"PlayerSpawnPoints array should have at least 1 spawn points.");

            if (m_SpawnIndex < m_PlayerSpawnPointsList.Count)
            {
                spawnPoint = m_PlayerSpawnPointsList[m_SpawnIndex];
            }
            else
            {
                Debug.LogWarning("No more spawn points available for spawning players.");
                return;
            }

            var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);

            var newPlayer = Instantiate(m_PlayerPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log("SERVER ACTION PHASE STATE: spawning player " + newPlayer);
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

            networkAvatarGuidState.AvatarGuid = new NetworkVariable<NetworkGuid>(persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value);

            // pass name from persistent player to avatar
            if (newPlayer.TryGetComponent(out NetworkNameState networkNameState))
            {
                networkNameState.Name = new NetworkVariable<FixedPlayerName>(persistentPlayer.NetworkNameState.Name.Value);
            }

            newPlayer.SpawnWithOwnership(clientId, true);
            m_SpanwedPlayerTable.Add(spawnPoint, newPlayer.transform);
            m_ListPlayerProgressIcon[m_SpawnIndex].Init(newPlayer.transform);
            m_SpawnIndex++;

        }

        #endregion

        #region ____GAME OVER METHODS____
        IEnumerator WaitToCheckForGameOver()
        {
            yield return null;
            CheckForGameOver();
        }
        void CheckForGameOver()
        {
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
            m_PersistentGameState.SetWinState(gameWon ? WinState.Win : WinState.Loss);

            // wait 5 seconds for game animations to finish
            yield return new WaitForSeconds(wait);

            SceneLoaderWrapper.Instance.LoadScene("PostGame", useNetworkSceneManager: true);
        }
        #endregion
    }

}
