using Project_RunningFighter.ConnectionManagement;
using Project_RunningFighter.Gameplay.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Avatar = Project_RunningFighter.Data.Avatar;

namespace Project_RunningFighter.Gameplay.GameStates
{
    [RequireComponent(typeof(NetcodeHooks))]
    public class ClientCharacterSelectState : GameStateBehaviour
    {
        /// Reference to the scene's state object so that UI can access state
        public static ClientCharacterSelectState Instance { get; private set; }

        [SerializeField]
        NetcodeHooks m_NetcodeHooks;
        public override GameState ActiveState { get { return GameState.CharSelect; } }

        [SerializeField]
        NetworkCharacterSelection m_NetworkCharacterSelection;

        [SerializeField]
        [Tooltip("This is triggered when the player chooses a character")]
        string m_AnimationTriggerOnCharSelect = "BeginRevive";

        [SerializeField]
        [Tooltip("This is triggered when the player presses the \"Ready\" button")]
        string m_AnimationTriggerOnCharChosen = "BeginRevive";

        [Header("Lobby Seats")]
        [SerializeField]
        [Tooltip("Collection of 4 portrait-boxes, one for each potential lobby member")]
        List<UICharSelectPlayerSeat> m_PlayerSeats;

        [System.Serializable]
        public class ColorAndIndicator
        {
            public Sprite Indicator;
            public Color Color;
        }
        [Tooltip("Representational information for each player")]
        public ColorAndIndicator[] m_IdentifiersForEachPlayerNumber;

        [SerializeField]
        [Tooltip("Text element containing player count which updates as players connect")]
        TextMeshProUGUI m_NumPlayersText;

        [SerializeField]
        [Tooltip("Text element for the Ready button")]
        TextMeshProUGUI m_ReadyButtonText;

        [Header("UI Elements for different lobby modes")]
        [SerializeField]
        [Tooltip("UI elements to turn on when the player hasn't chosen their seat yet. Turned off otherwise!")]
        List<GameObject> m_UIElementsForNoSeatChosen;

        [SerializeField]
        [Tooltip("UI elements to turn on when the player has locked in their seat choice (and is now waiting for other players to do the same). Turned off otherwise!")]
        List<GameObject> m_UIElementsForSeatChosen;

        [SerializeField]
        [Tooltip("UI elements to turn on when the lobby is closed (and game is about to start). Turned off otherwise!")]
        List<GameObject> m_UIElementsForLobbyEnding;

        [SerializeField]
        [Tooltip("UI elements to turn on when there's been a fatal error (and the client cannot proceed). Turned off otherwise!")]
        List<GameObject> m_UIElementsForFatalError;

        [Header("Misc")]
        [SerializeField]
        [Tooltip("The controller for the class-info box")]
        UICharacterSelectClassInfoBox m_ClassInfoBox;

        [SerializeField]
        Transform m_CharacterGraphicsParent;

        int m_LastSeatSelected = -1;
        bool m_HasLocalPlayerLockedIn = false;

        GameObject m_CurrentCharacterGraphics;

        Animator m_CurrentCharacterGraphicsAnimator;

        Dictionary<Guid, GameObject> m_SpawnedCharacterGraphics = new Dictionary<Guid, GameObject>();

        enum LobbyMode
        {
            ChooseSeat, // "Choose your seat!" stage
            SeatChosen, // "Waiting for other players!" stage
            LobbyEnding, // "Get ready! Game is starting!" stage
            FatalError, // "Fatal Error" stage
        }

        Dictionary<LobbyMode, List<GameObject>> m_LobbyUIElementsByMode;

        [Inject]
        ConnectionManager m_ConnectionManager;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;

            m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;

            m_LobbyUIElementsByMode = new Dictionary<LobbyMode, List<GameObject>>()
            {
                { LobbyMode.ChooseSeat, m_UIElementsForNoSeatChosen },
                { LobbyMode.SeatChosen, m_UIElementsForSeatChosen },
                { LobbyMode.LobbyEnding, m_UIElementsForLobbyEnding },
                { LobbyMode.FatalError, m_UIElementsForFatalError },
            };
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            base.OnDestroy();
        }

        protected override void Start()
        {
            base.Start();
            for (int i = 0; i < m_PlayerSeats.Count; ++i)
            {
                m_PlayerSeats[i].Initialize(i);
            }

            ConfigureUIForLobbyMode(LobbyMode.ChooseSeat);
            UpdateCharacterSelection(NetworkCharacterSelection.SeatState.Inactive);
        }

        void OnNetworkDespawn()
        {
            if (m_NetworkCharacterSelection)
            {
                m_NetworkCharacterSelection.IsLobbyClosed.OnValueChanged -= OnLobbyClosedChanged;
                m_NetworkCharacterSelection.LobbyPlayers.OnListChanged -= OnLobbyPlayerStateChanged;
            }
        }

        void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                enabled = false;
            }
            else
            {
                m_NetworkCharacterSelection.IsLobbyClosed.OnValueChanged += OnLobbyClosedChanged;
                m_NetworkCharacterSelection.LobbyPlayers.OnListChanged += OnLobbyPlayerStateChanged;
            }
        }

        /// Called when our PlayerNumber (e.g. P1, P2, etc.) has been assigned by the server
        /// <param name="playerNum"></param>
        void OnAssignedPlayerNumber(int playerNum)
        {
            m_ClassInfoBox.OnSetPlayerNumber(playerNum);
        }

        void UpdatePlayerCount()
        {
            int count = m_NetworkCharacterSelection.LobbyPlayers.Count;
            var pstr = (count > 1) ? "players" : "player";
            m_NumPlayersText.text = "<b>" + count + "</b> " + pstr + " connected";
        }

        void OnLobbyPlayerStateChanged(NetworkListEvent<NetworkCharacterSelection.LobbyPlayerState> changeEvent)
        {
            UpdateSeats();
            UpdatePlayerCount();

            int localPlayerIdx = -1;
            for (int i = 0; i < m_NetworkCharacterSelection.LobbyPlayers.Count; ++i)
            {
                if (m_NetworkCharacterSelection.LobbyPlayers[i].ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    localPlayerIdx = i;
                    break;
                }
            }

            if (localPlayerIdx == -1)
            {
                UpdateCharacterSelection(NetworkCharacterSelection.SeatState.Inactive);
            }
            else if (m_NetworkCharacterSelection.LobbyPlayers[localPlayerIdx].SeatState == NetworkCharacterSelection.SeatState.Inactive)
            {
                UpdateCharacterSelection(NetworkCharacterSelection.SeatState.Inactive);
                OnAssignedPlayerNumber(m_NetworkCharacterSelection.LobbyPlayers[localPlayerIdx].PlayerNumber);
            }
            else
            {
                UpdateCharacterSelection(m_NetworkCharacterSelection.LobbyPlayers[localPlayerIdx].SeatState, m_NetworkCharacterSelection.LobbyPlayers[localPlayerIdx].SeatIdx);
            }
        }

        /// Internal utility that sets the character-graphics and class-info box based on
        /// our chosen seat. It also triggers a LobbyMode change when it notices that our seat-state
        /// is LockedIn.
        /// <param name="state">Our current seat state</param>
        /// <param name="seatIdx">Which seat we're sitting in, or -1 if SeatState is Inactive</param>
        void UpdateCharacterSelection(NetworkCharacterSelection.SeatState state, int seatIdx = -1)
        {
            bool isNewSeat = m_LastSeatSelected != seatIdx;
            Debug.Log("CLIENT CHARACTER SELECT STATE: seatIdx "+seatIdx);
            m_LastSeatSelected = seatIdx;
            if (state == NetworkCharacterSelection.SeatState.Inactive)
            {
                if (m_CurrentCharacterGraphics)
                {
                    m_CurrentCharacterGraphics.SetActive(false);
                }

                m_ClassInfoBox.ConfigureForNoSelection();
            }
            else
            {
                if (seatIdx != -1)
                {
                    // change character preview when selecting a new seat
                    if (isNewSeat)
                    {
                        var selectedCharacterGraphics = GetCharacterGraphics(m_NetworkCharacterSelection.AvatarConfiguration[seatIdx]);

                        if (m_CurrentCharacterGraphics)
                        {
                            m_CurrentCharacterGraphics.SetActive(false);
                        }

                        selectedCharacterGraphics.SetActive(true);
                        m_CurrentCharacterGraphics = selectedCharacterGraphics;
                        m_CurrentCharacterGraphicsAnimator = m_CurrentCharacterGraphics.GetComponent<Animator>();

                        m_ClassInfoBox.ConfigureForClass(m_NetworkCharacterSelection.AvatarConfiguration[seatIdx].CharacterClass);
                    }
                }
                if (state == NetworkCharacterSelection.SeatState.LockedIn && !m_HasLocalPlayerLockedIn)
                {
                    //m_CurrentCharacterGraphicsAnimator.Play(m_AnimationTriggerOnCharChosen);
                   // m_CurrentCharacterGraphicsAnimator.SetTrigger(m_AnimationTriggerOnCharChosen);
                    ConfigureUIForLobbyMode(m_NetworkCharacterSelection.IsLobbyClosed.Value ? LobbyMode.LobbyEnding : LobbyMode.SeatChosen);
                    m_HasLocalPlayerLockedIn = true;
                }
                else if (m_HasLocalPlayerLockedIn && state == NetworkCharacterSelection.SeatState.Active)
                {
                    // reset character seats if locked in choice was unselected
                    if (m_HasLocalPlayerLockedIn)
                    {
                        ConfigureUIForLobbyMode(LobbyMode.ChooseSeat);
                        m_ClassInfoBox.SetLockedIn(false);
                        m_HasLocalPlayerLockedIn = false;
                    }
                }
                else if (state == NetworkCharacterSelection.SeatState.Active && isNewSeat)
                {
                    //m_CurrentCharacterGraphicsAnimator.SetTrigger(m_AnimationTriggerOnCharSelect);
                }
            }
        }

        /// Internal utility that sets the graphics for the eight lobby-seats (based on their current networked state)
        void UpdateSeats()
        {
            // Players can hop between seats -- and can even SHARE seats -- while they're choosing a class.
            // Once they have chosen their class (by "locking in" their seat), other players in that seat are kicked out.
            // But until a seat is locked in, we need to display each seat as being used by the latest player to choose it.
            // So we go through all players and figure out who should visually be shown as sitting in that seat.
            NetworkCharacterSelection.LobbyPlayerState[] curSeats = new NetworkCharacterSelection.LobbyPlayerState[m_PlayerSeats.Count];
            foreach (NetworkCharacterSelection.LobbyPlayerState playerState in m_NetworkCharacterSelection.LobbyPlayers)
            {
                if (playerState.SeatIdx == -1 || playerState.SeatState == NetworkCharacterSelection.SeatState.Inactive)
                    continue; // this player isn't seated at all!
                if (curSeats[playerState.SeatIdx].SeatState == NetworkCharacterSelection.SeatState.Inactive
                    || (curSeats[playerState.SeatIdx].SeatState == NetworkCharacterSelection.SeatState.Active && curSeats[playerState.SeatIdx].LastChangeTime < playerState.LastChangeTime))
                {
                    // this is the best candidate to be displayed in this seat (so far)
                    curSeats[playerState.SeatIdx] = playerState;
                }
            }

            // now actually update the seats in the UI
            for (int i = 0; i < m_PlayerSeats.Count; ++i)
            {
                m_PlayerSeats[i].SetState(curSeats[i].SeatState, curSeats[i].PlayerNumber, curSeats[i].PlayerName);
            }
        }

        /// Called by the server when the lobby closes (because all players are seated and locked in)
        void OnLobbyClosedChanged(bool wasLobbyClosed, bool isLobbyClosed)
        {
            if (isLobbyClosed)
            {
                ConfigureUIForLobbyMode(LobbyMode.LobbyEnding);
            }
            else
            {
                if (m_LastSeatSelected == -1)
                {
                    ConfigureUIForLobbyMode(LobbyMode.ChooseSeat);
                }
                else
                {
                    ConfigureUIForLobbyMode(LobbyMode.SeatChosen);
                    m_ClassInfoBox.ConfigureForClass(m_NetworkCharacterSelection.AvatarConfiguration[m_LastSeatSelected].CharacterClass);
                }
            }
        }

        /// Turns on the UI elements for a specified "lobby mode", and turns off UI elements for all other modes.
        /// It can also disable/enable the lobby seats and the "Ready" button if they are inappropriate for the
        /// given mode.
        void ConfigureUIForLobbyMode(LobbyMode mode)
        {
            // first the easy bit: turn off all the inappropriate ui elements, and turn the appropriate ones on!
            foreach (var list in m_LobbyUIElementsByMode.Values)
            {
                foreach (var uiElement in list)
                {
                    uiElement.SetActive(false);
                }
            }

            foreach (var uiElement in m_LobbyUIElementsByMode[mode])
            {
                uiElement.SetActive(true);
            }

            // that finishes the easy bit. Next, each lobby mode might also need to configure the lobby seats and class-info box.
            bool isSeatsDisabledInThisMode = false;
            switch (mode)
            {
                case LobbyMode.ChooseSeat:
                    if (m_LastSeatSelected == -1)
                    {
                        if (m_CurrentCharacterGraphics)
                        {
                            m_CurrentCharacterGraphics.gameObject.SetActive(false);
                        }
                        m_ClassInfoBox.ConfigureForNoSelection();
                    }
                    m_ReadyButtonText.text = "READY!";
                    break;
                case LobbyMode.SeatChosen:
                    isSeatsDisabledInThisMode = true;
                    m_ClassInfoBox.SetLockedIn(true);
                    m_ReadyButtonText.text = "UNREADY";
                    break;
                case LobbyMode.FatalError:
                    isSeatsDisabledInThisMode = true;
                    m_ClassInfoBox.ConfigureForNoSelection();
                    break;
                case LobbyMode.LobbyEnding:
                    isSeatsDisabledInThisMode = true;
                    m_ClassInfoBox.ConfigureForNoSelection();
                    break;
            }

            foreach (var seat in m_PlayerSeats)
            {
                seat.SetDisableInteraction(seat.IsLocked() || isSeatsDisabledInThisMode);
            }

        }

        /// Called directly by UI elements!
        /// <param name="seatIdx"></param>
        public void OnPlayerClickedSeat(int seatIdx)
        {
            if (m_NetworkCharacterSelection.IsSpawned)
            {
                m_NetworkCharacterSelection.ServerChangeSeatRpc(NetworkManager.Singleton.LocalClientId, seatIdx, false);
            }
        }

        public void OnPlayerClickedReady()
        {
            if (m_NetworkCharacterSelection.IsSpawned)
            {
                m_NetworkCharacterSelection.ServerChangeSeatRpc(NetworkManager.Singleton.LocalClientId, m_LastSeatSelected, !m_HasLocalPlayerLockedIn);
            }
        }

        GameObject GetCharacterGraphics(Avatar avatar)
        {
            if (!m_SpawnedCharacterGraphics.TryGetValue(avatar.Guid, out GameObject characterGraphics))
            {
                characterGraphics = Instantiate(avatar.GraphicsCharacterSelect, m_CharacterGraphicsParent);
                m_SpawnedCharacterGraphics.Add(avatar.Guid, characterGraphics);
            }

            return characterGraphics;
        }

    }
}
