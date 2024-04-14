using Project_RunningFighter.Gameplay.UI;
using System;
using System.Collections;
using System.Collections.Generic;
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
        }
        public override void OnNetworkDespawn()
        {
            OnPlayerSpawned -= ServerActionPhaseState_OnPlayerSpawned;
            base.OnNetworkDespawn();
        }
        #region ____EVENT METHODS____
        private void ServerActionPhaseState_OnPlayerSpawned(ServerActionPhaseState.PlayerSpawnedSetup setup)
        {
            try
            {
                m_SpanwedPlayerTable.Add(setup.spawnPosTrans, setup.playerTrans);
                m_PlayerStartPositionTable.Add(setup.spawnIndex, setup.spawnPosTrans);
                m_ListPlayerProgressIcon[setup.spawnIndex].Init(setup.playerTrans);
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
        void UpdatePlayersProgess()
        {
            if (m_endPosition == null) return;
            if (m_SpanwedPlayerTable == null || m_SpanwedPlayerTable.Count == 0) return;

            // Calculate the width of the Fill Area of the slider
            RectTransform fillRect = m_ProgressSlider.fillRect.GetComponent<RectTransform>();
            float fillAreaWidth = fillRect.rect.width;
            int count = m_PlayerStartPositionTable.Count;

            for (int i = 0; i < count; i++)
            {
                Transform playerStartPos = m_PlayerStartPositionTable[i];
                if (playerStartPos == null) continue;
                m_SpanwedPlayerTable.TryGetValue(playerStartPos, out Transform player);
                if (player == null) continue;
                var playerIcon = m_ListPlayerProgressIcon[i];
                float totalDistance = Vector3.Distance(playerStartPos.position, m_endPosition.position);

                float distanceToPlayer = Vector3.Distance(playerStartPos.position, player.transform.position);
                float sliderValue = distanceToPlayer / totalDistance;

                sliderValue = Mathf.Clamp01(sliderValue);
                Debug.Log($"UPDATE PLAYER PROGRESS: slider value {sliderValue} - distanceToPlayer {distanceToPlayer} - Total Distance {totalDistance}");
                //The Slider's Handle Only for Host Player
                if (playerIcon.name.ToLower().Contains("server") || playerIcon.name.ToLower().Contains("host"))
                {
                    m_ProgressSlider.value = sliderValue;
                }
                else
                {
                    float iconXPosition = fillAreaWidth * sliderValue;
                    playerIcon.ChangePositionX(iconXPosition);
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
                        UpdatePlayersProgess();
                        //Will change To EndCount Down When There's only 3 seconds left
                        if (IsCountDownTimerFinished(ref m_CountDownTimer, 3f))
                        {
                            ChangeGameplayState(ActionPhaseState.EndCountDown);
                        }
                    }
                    break;
                case ActionPhaseState.EndCountDown:
                    m_CountDownTimer.Value -= Time.unscaledDeltaTime;
                    UpdatePlayersProgess();
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
        #region ____UI METHODS____
        public float GetCountDownTimer()
        {
            return m_CountDownTimer.Value;
        }
       
        #endregion

    }

}
