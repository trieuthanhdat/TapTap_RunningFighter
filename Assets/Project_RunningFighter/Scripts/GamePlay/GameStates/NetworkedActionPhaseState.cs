using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.Gameplay.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static Project_RunningFighter.Gameplay.GameStates.ServerActionPhaseState;

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
    [RequireComponent(typeof(ServerActionPhaseState))]
    public class NetworkedActionPhaseState : NetworkBehaviour
    {
        [Header("UI REFERENCES")]
        [SerializeField] Slider m_ProgressSlider;
        [SerializeField] List<UIPlayerProgressIconBehaviour> m_ListPlayerProgressIcon;
        [Header("OBJECTS REFERENCES")]
        [SerializeField] Transform m_endPosition;
        [SerializeField] Transform m_PlayerProgressParent;
        [Header("STATE SETTINGS")]
        [SerializeField] float m_StartCountDownDuration = 3f; //In seconds
        [SerializeField] float m_EndCountDownDuration = 3f;   //In seconds
        [SerializeField] float m_PlayTimeDuration = 60f;      //In seconds

        #region ____PROPETIES____
        #region NETWORKED PROPETIES

        private NetworkVariable<float> m_CountDownTimer = new NetworkVariable<float>(3f);

        private NetworkVariable<bool> m_IsReadyUp = new NetworkVariable<bool>();
        public  NetworkVariable<bool> IsReadyUp => m_IsReadyUp;

        private NetworkVariable<bool> m_IsPlaying = new NetworkVariable<bool>();
        public  NetworkVariable<bool> IsPlaying => m_IsPlaying;
        [SerializeField]
        private NetworkVariable<ActionPhaseState> m_GameplayState = new NetworkVariable<ActionPhaseState>(ActionPhaseState.ReadyUp);
        public  NetworkVariable<ActionPhaseState> GameplayState => m_GameplayState;
        #endregion
        private Dictionary<int, Transform> m_PlayerStartPositionTable = new Dictionary<int, Transform>();
        private Dictionary<Transform, Transform> m_SpanwedPlayerTable = new Dictionary<Transform, Transform>();
        //ACTION PHASE STATE
        public static event Action<ActionPhaseState> OnGameplayStateChanged;

        #region ____SINGLETON____
        private static NetworkedActionPhaseState instance = null;
        public static NetworkedActionPhaseState Instance
        {
            get
            {
                if(instance == null) 
                   instance = FindObjectOfType<NetworkedActionPhaseState>(); 
                
                return instance;
            }
        }
        #endregion
        #endregion
        private void Awake()
        {
            OnPlayerSpawned += ServerActionPhaseState_OnPlayerSpawned;
        }
        private void Update()
        {
            UpdateGameplayState();
        }
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
            ClientCharacter.OnWinBarrierEntered += ClientCharacter_OnWinBarrierEntered;
        }
        public override void OnNetworkDespawn()
        {
            OnPlayerSpawned -= ServerActionPhaseState_OnPlayerSpawned;
            ClientCharacter.OnWinBarrierEntered -= ClientCharacter_OnWinBarrierEntered;
            base.OnNetworkDespawn();
        }
        public override void OnDestroy()
        {
            OnPlayerSpawned -= ServerActionPhaseState_OnPlayerSpawned;
            ClientCharacter.OnWinBarrierEntered -= ClientCharacter_OnWinBarrierEntered;
            base.OnDestroy();
        }
        #region ____EVENT METHODS____
        private void ClientCharacter_OnWinBarrierEntered(ulong clientId)
        {
            if (m_GameplayState.Value != ActionPhaseState.EndCountDown &&
               m_GameplayState.Value != ActionPhaseState.Finish)
            {
                m_CountDownTimer.Value = m_EndCountDownDuration;
                ChangeGameplayState(ActionPhaseState.EndCountDown);
            }

        }

        private void ServerActionPhaseState_OnPlayerSpawned(ServerActionPhaseState.PlayerSpawnedSetup setup)
        {
            try
            {
                m_SpanwedPlayerTable.Add(setup.spawnPosTrans, setup.playerTrans);
                m_PlayerStartPositionTable.Add(setup.spawnIndex, setup.spawnPosTrans);
                
                SetActivePlayerProgressRpc(setup.spawnIndex);
            }
            catch (Exception e)
            {
                Debug.LogError($"[NETWORKED ACTION PHASE STATE]: error at ServerActionPhaseState_OnPlayerSpawned => "+ e);
            }
           
        }
        private void OnSynchronizeComplete(ulong clientId)
        {
            ChangeGameplayState(ActionPhaseState.ReadyUp);
        }
        #endregion

        #region ____PLAYERS UTILS____
        void UpdatePlayersProgress()
        {
            if (m_endPosition == null) return;
            if (m_SpanwedPlayerTable == null || m_SpanwedPlayerTable.Count == 0) return;

            // Create a list to store the spawn positions in order
            List<Transform> orderedSpawnPositions = new List<Transform>(m_PlayerStartPositionTable.Values);
            List<int> listPlayerNumerKey = new List<int>(m_PlayerStartPositionTable.Keys);

            // Calculate the width of the Fill Area of the slider
            RectTransform fillRect = m_ProgressSlider.fillRect.GetComponent<RectTransform>();
            float fillAreaWidth = fillRect.rect.width;

            for (int i = 0; i < orderedSpawnPositions.Count; i++)
            {
                Transform playerStartPos = orderedSpawnPositions[i];
                int playerNumberKey = listPlayerNumerKey[i];

                if (playerStartPos == null) continue;

                if (m_SpanwedPlayerTable.TryGetValue(playerStartPos, out Transform player))
                {
                    if (player == null) continue;
                    float totalDistance = Vector3.Distance(playerStartPos.position, m_endPosition.position);
                    float distanceToPlayer = Vector3.Distance(playerStartPos.position, player.position);
                    float sliderValue = distanceToPlayer / totalDistance;
                    sliderValue = Mathf.Clamp01(sliderValue);

                    Debug.Log($"UPDATE PLAYER PROGRESS: slider value {sliderValue} " +
                              $"- distanceToPlayer {distanceToPlayer} " +
                              $"- Total Distance {totalDistance} " +
                              $"- fillAreaWidth {fillAreaWidth}");
                    SetPlayerProgressRpc(playerNumberKey, sliderValue);

                }
            }
        }


        #endregion

        #region ____STATE MANAGEMENT____
        public
        void ChangeGameplayState(ActionPhaseState newState)
        {
            if (!IsServer) return;
            m_GameplayState.Value = newState;
            ChangeGameplayStateRpc(newState);
        }
        [Rpc(SendTo.Everyone)]
        public void ChangeGameplayStateRpc(ActionPhaseState newState)
        {
            OnGameplayStateChanged?.Invoke(newState);
            Debug.Log("[SERVER]: RPC state change to every One !");
        }
        void UpdateGameplayState()
        {
            if (!IsServer) return;
            switch (m_GameplayState.Value)
            {
                case ActionPhaseState.ReadyUp:
                    if (m_IsReadyUp.Value) return;
                    m_IsPlaying.Value = false;
                    m_IsReadyUp.Value = true;
                    m_CountDownTimer.Value = m_StartCountDownDuration;
                    StartCoroutine(Co_WaitAlittle());
                    IEnumerator Co_WaitAlittle()
                    {
                        yield return new WaitForSeconds(1f);
                        ChangeGameplayState(ActionPhaseState.StartCountDown);
                    }
                    break;
                case ActionPhaseState.StartCountDown:
                    m_CountDownTimer.Value -= Time.unscaledDeltaTime;

                    if (IsCountDownTimerFinished(ref m_CountDownTimer))
                    {
                        ChangeGameplayState(ActionPhaseState.Playing);
                    }
                    break;
                case ActionPhaseState.Playing:
                    if (!m_IsPlaying.Value)
                    {
                        m_IsPlaying.Value = true;
                        m_CountDownTimer.Value = m_PlayTimeDuration;
                    }
                    else
                    {
                        m_CountDownTimer.Value -= Time.unscaledDeltaTime;
                        UpdatePlayersProgress();
                        //Will change To EndCount Down When There's only 3 seconds left
                        if (IsCountDownTimerFinished(ref m_CountDownTimer, m_EndCountDownDuration))
                        {
                            ChangeGameplayState(ActionPhaseState.EndCountDown);
                        }
                    }
                    break;
                case ActionPhaseState.EndCountDown:
                    m_CountDownTimer.Value -= Time.unscaledDeltaTime;
                    UpdatePlayersProgress();
                    if (IsCountDownTimerFinished(ref m_CountDownTimer))
                    {
                        ChangeGameplayState(ActionPhaseState.Finish);
                    }
                    break;
                case ActionPhaseState.Finish:
                    //FINISH GAME 
                    Debug.Log("FINISH GAME!!!");
                    m_IsPlaying.Value = false;
                    break;
            }
        }
        bool IsCountDownTimerFinished(ref NetworkVariable<float> Timer, float endTime = 0)
        {
            if (m_CountDownTimer.Value <= endTime)
            {
                Timer.Value = endTime;
                return true;
            }
            return false;
        }
        #endregion

        #region ____RPC METHODS____

        [Rpc(SendTo.ClientsAndHost)]
        public void SetPlayerProgressRpc(int playerNumberKey, float sliderValue)
        {
            var playerIcon = m_ListPlayerProgressIcon[playerNumberKey];

            sliderValue = Mathf.Clamp01(sliderValue);

            Debug.Log($"UPDATE PLAYER PROGRESS: RPC slider value {sliderValue}");
            playerIcon.ChangeSliderProgress(sliderValue);
        }
        [Rpc(SendTo.ClientsAndHost)]
        public void SetActivePlayerProgressRpc(int spawnIndex)
        {
            var progressIcon = m_ListPlayerProgressIcon[spawnIndex];
            progressIcon.SetActive(true);
            progressIcon.Init(spawnIndex);
        }
        #endregion

        #region ____UI METHODS____
        public float GetCountDownTimer()
        {
            return m_CountDownTimer.Value;
        }
       
        #endregion

    }

}
