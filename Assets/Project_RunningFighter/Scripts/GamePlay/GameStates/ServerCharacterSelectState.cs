using Project_RunningFighter.ConnectionManagement;
using Project_RunningFighter.Gameplay.GameplayObjects;
using Project_RunningFighter.Gameplay.GameStates;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Project_RunningFighter.Gameplay.GameStates
{
    [RequireComponent(typeof(NetcodeHooks), typeof(NetworkCharacterSelection))]
    public class ServerCharacterSelectState : GameStateBehaviour
    {
        [SerializeField]
        NetcodeHooks m_NetcodeHooks;

        public override GameState ActiveState => GameState.CharSelect;
        public NetworkCharacterSelection NetworkCharacterSelection { get; private set; }

        Coroutine m_WaitToEndLobbyCoroutine;

        [Inject]
        ConnectionManager m_ConnectionManager;

        protected override void Awake()
        {
            base.Awake();
            NetworkCharacterSelection = GetComponent<NetworkCharacterSelection>();

            m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_NetcodeHooks)
            {
                m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }
        }

        void OnClientChangedSeat(ulong clientId, int newSeatIdx, bool lockedIn)
        {
            int idx = FindLobbyPlayerIdx(clientId);
            if (idx == -1)
            {
                throw new Exception($"OnClientChangedSeat: client ID {clientId} is not a lobby player and cannot change seats! Shouldn't be here!");
            }

            if (NetworkCharacterSelection.IsLobbyClosed.Value)
            {
                // The user tried to change their class after everything was locked in... too late! Discard this choice
                return;
            }

            if (newSeatIdx == -1)
            {
                // we can't lock in with no seat
                lockedIn = false;
            }
            else
            {
                // see if someone has already locked-in that seat! If so, too late... discard this choice
                foreach (NetworkCharacterSelection.LobbyPlayerState playerInfo in NetworkCharacterSelection.LobbyPlayers)
                {
                    if (playerInfo.ClientId != clientId && playerInfo.SeatIdx == newSeatIdx && playerInfo.SeatState == NetworkCharacterSelection.SeatState.LockedIn)
                    {
                        // somebody already locked this choice in. Stop!
                        // Instead of granting lock request, change this player to Inactive state.
                        NetworkCharacterSelection.LobbyPlayers[idx] = new NetworkCharacterSelection.LobbyPlayerState(clientId,
                            NetworkCharacterSelection.LobbyPlayers[idx].PlayerName,
                            NetworkCharacterSelection.LobbyPlayers[idx].PlayerNumber,
                            NetworkCharacterSelection.SeatState.Inactive);

                        // then early out
                        return;
                    }
                }
            }

            NetworkCharacterSelection.LobbyPlayers[idx] = new NetworkCharacterSelection.LobbyPlayerState(clientId,
                NetworkCharacterSelection.LobbyPlayers[idx].PlayerName,
                NetworkCharacterSelection.LobbyPlayers[idx].PlayerNumber,
                lockedIn ? NetworkCharacterSelection.SeatState.LockedIn : NetworkCharacterSelection.SeatState.Active,
                newSeatIdx,
                Time.time);

            if (lockedIn)
            {
                for (int i = 0; i < NetworkCharacterSelection.LobbyPlayers.Count; ++i)
                {
                    if (NetworkCharacterSelection.LobbyPlayers[i].SeatIdx == newSeatIdx && i != idx)
                    {
                        // change this player to Inactive state.
                        NetworkCharacterSelection.LobbyPlayers[i] = new NetworkCharacterSelection.LobbyPlayerState(
                            NetworkCharacterSelection.LobbyPlayers[i].ClientId,
                            NetworkCharacterSelection.LobbyPlayers[i].PlayerName,
                            NetworkCharacterSelection.LobbyPlayers[i].PlayerNumber,
                            NetworkCharacterSelection.SeatState.Inactive);
                    }
                }
            }

            CloseLobbyIfReady();
        }

        int FindLobbyPlayerIdx(ulong clientId)
        {
            for (int i = 0; i < NetworkCharacterSelection.LobbyPlayers.Count; ++i)
            {
                if (NetworkCharacterSelection.LobbyPlayers[i].ClientId == clientId)
                    return i;
            }
            return -1;
        }

        void CloseLobbyIfReady()
        {
            foreach (NetworkCharacterSelection.LobbyPlayerState playerInfo in NetworkCharacterSelection.LobbyPlayers)
            {
                if (playerInfo.SeatState != NetworkCharacterSelection.SeatState.LockedIn)
                    return; // nope, at least one player isn't locked in yet!
            }

            // everybody's ready at the same time! Lock it down!
            NetworkCharacterSelection.IsLobbyClosed.Value = true;

            // remember our choices so the next scene can use the info
            SaveLobbyResults();

            // Delay a few seconds to give the UI time to react, then switch scenes
            m_WaitToEndLobbyCoroutine = StartCoroutine(WaitToEndLobby());
        }

        void CancelCloseLobby()
        {
            if (m_WaitToEndLobbyCoroutine != null)
            {
                StopCoroutine(m_WaitToEndLobbyCoroutine);
            }
            NetworkCharacterSelection.IsLobbyClosed.Value = false;
        }

        void SaveLobbyResults()
        {
            foreach (NetworkCharacterSelection.LobbyPlayerState playerInfo in NetworkCharacterSelection.LobbyPlayers)
            {
                var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerInfo.ClientId);

                if (playerNetworkObject && playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer))
                {
                    // pass avatar GUID to PersistentPlayer
                    // it'd be great to simplify this with something like a NetworkScriptableObjects :(
                    persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value =
                        NetworkCharacterSelection.AvatarConfiguration[playerInfo.SeatIdx].Guid.ToNetworkGuid();
                }
            }
        }

        IEnumerator WaitToEndLobby()
        {
            yield return new WaitForSeconds(3);
            SceneLoaderWrapper.Instance.LoadScene("ActionPhase", useNetworkSceneManager: true);
        }

        public void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            }
            if (NetworkCharacterSelection)
            {
                NetworkCharacterSelection.OnClientChangedSeat -= OnClientChangedSeat;
            }
        }

        public void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            else
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
                NetworkCharacterSelection.OnClientChangedSeat += OnClientChangedSeat;

                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
            }
        }

        void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
            Debug.Log("CHARACTER SELECTION STATE: OnSceneEvent "+sceneEvent.SceneName + " Client id "+ sceneEvent.ClientId);
            SeatNewPlayer(sceneEvent.ClientId);
        }

        int GetAvailablePlayerNumber()
        {
            for (int possiblePlayerNumber = 0; possiblePlayerNumber < m_ConnectionManager.MaxConnectedPlayers; ++possiblePlayerNumber)
            {
                if (IsPlayerNumberAvailable(possiblePlayerNumber))
                {
                    return possiblePlayerNumber;
                }
            }
            // we couldn't get a Player# for this person... which means the lobby is full!
            return -1;
        }

        bool IsPlayerNumberAvailable(int playerNumber)
        {
            bool found = false;
            foreach (NetworkCharacterSelection.LobbyPlayerState playerState in NetworkCharacterSelection.LobbyPlayers)
            {
                if (playerState.PlayerNumber == playerNumber)
                {
                    found = true;
                    break;
                }
            }

            return !found;
        }

        void SeatNewPlayer(ulong clientId)
        {
            // If lobby is closing and waiting to start the game, cancel to allow that new player to select a character
            if (NetworkCharacterSelection.IsLobbyClosed.Value)
            {
                CancelCloseLobby();
            }

            SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (sessionPlayerData.HasValue)
            {
                var playerData = sessionPlayerData.Value;
                if (playerData.PlayerNumber == -1 || !IsPlayerNumberAvailable(playerData.PlayerNumber))
                {
                    // If no player num already assigned or if player num is no longer available, get an available one.
                    playerData.PlayerNumber = GetAvailablePlayerNumber();
                }
                if (playerData.PlayerNumber == -1)
                {
                    // Sanity check. We ran out of seats... there was no room!
                    throw new Exception($"we shouldn't be here, connection approval should have refused this connection already for client ID {clientId} and player num {playerData.PlayerNumber}");
                }

                NetworkCharacterSelection.LobbyPlayers.Add(new NetworkCharacterSelection.LobbyPlayerState(clientId, playerData.PlayerName, playerData.PlayerNumber, NetworkCharacterSelection.SeatState.Inactive));
                SessionManager<SessionPlayerData>.Instance.SetPlayerData(clientId, playerData);
            }
        }

        void OnClientDisconnectCallback(ulong clientId)
        {
            // clear this client's PlayerNumber and any associated visuals (so other players know they're gone).
            for (int i = 0; i < NetworkCharacterSelection.LobbyPlayers.Count; ++i)
            {
                if (NetworkCharacterSelection.LobbyPlayers[i].ClientId == clientId)
                {
                    NetworkCharacterSelection.LobbyPlayers.RemoveAt(i);
                    break;
                }
            }

            if (!NetworkCharacterSelection.IsLobbyClosed.Value)
            {
                // If the lobby is not already closing, close if the remaining players are all ready
                CloseLobbyIfReady();
            }
        }
    }
}
