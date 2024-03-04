using System;
using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.Infrastructure;
using TD.UServices.GamePlay;
using Unity.Netcode;
using UnityEngine;

namespace TD.UServices.Core
{
    public class InGameNetworkRunner : NetworkBehaviour
    {
        /*[SerializeField]
        private PlayerCursor m_playerCursorPrefab = default;
        [SerializeField]
        private SymbolContainer m_symbolContainerPrefab = default;
        [SerializeField]
        private SymbolObject m_symbolObjectPrefab = default;
        [SerializeField]
        private SequenceSelector m_sequenceSelector = default;*/

        /*[SerializeField]
        private SymbolKillVolume m_killVolume = default;
        [SerializeField]
        private IntroOutroRunner m_introOutroRunner = default;*/
        [SerializeField] private NetworkedScorer m_scorer = default;
        [SerializeField] private NetworkedDataService m_dataStore = default;
        [SerializeField] private BoxCollider m_collider;

        public Action onGameBeginning;
        Action m_onConnectionVerified, m_onGameEnd;
        // Used by the host, but we can't call the RPC until the network connection completes.
        private int m_expectedPlayerCount; 
        private bool? m_canSpawnInGameObjects;
        //private Queue<Vector2> m_pendingSymbolPositions = new Queue<Vector2>();
        // Initial time buffer to ensure connectivity before loading objects.
        //private float m_symbolSpawnTimer = 0.5f;
        // Only used by the host.
        //private int m_remainingSymbolCount = 0; 
        private float m_timeout = 10;
        private bool m_hasConnected = false;

        //[SerializeField]
        //private SymbolContainer m_symbolContainerInstance;
        private NetworkedPlayerData m_localUserData; // This has an ID that's not necessarily the OwnerClientId, since all clients will see all spawned objects regardless of ownership.

        public static InGameNetworkRunner Instance
        {
            get
            {
                if (s_Instance!) return s_Instance;
                return s_Instance = FindObjectOfType<InGameNetworkRunner>();
            }
        }

        static InGameNetworkRunner s_Instance;

        public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameBegin,
            Action onGameEnd,
            LocalPlayer localUser)
        {
            m_onConnectionVerified = onConnectionVerified;
            m_expectedPlayerCount = expectedPlayerCount;
            onGameBeginning = onGameBegin;
            m_onGameEnd = onGameEnd;
            m_canSpawnInGameObjects = null;
            m_localUserData = new NetworkedPlayerData(localUser.DisplayName.Value, 0);
        }

        public override void OnNetworkSpawn()
        {
            if (IsHost)
                FinishInitialize();
            m_localUserData = new NetworkedPlayerData(m_localUserData.name, NetworkManager.Singleton.LocalClientId);
            VerifyConnection_ServerRpc(m_localUserData.id);
        }

        public override void OnNetworkDespawn()
        {
            m_onGameEnd(); // As a backup to ensure in-game objects get cleaned up, if this is disconnected unexpectedly.
        }

        private void FinishInitialize()
        {
        //    m_symbolContainerInstance = Instantiate(m_symbolContainerPrefab);
        //    m_symbolContainerInstance.NetworkObject.Spawn();
        //    ResetPendingSymbolPositions();
        //    m_killVolume.Initialize(OnSymbolDeactivated);
        }

        // To verify the connection,
        // To verify the connection, 
        //invoke a server RPC call that then invokes a client RPC call. After this, the actual setup occurs.
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnection_ServerRpc(ulong clientId)
        {
            VerifyConnection_ClientRpc(clientId);
            // While we could start pooling symbol objects now, incoming clients would be flooded with the Spawn calls.
            // This could lead to dropped packets such that the InGameRunner's Spawn call fails to occur, so we'll wait until all players join.
            // (Besides, we will need to display instructions, which has downtime during which symbol objects can be spawned.)
        }

        [ClientRpc]
        private void VerifyConnection_ClientRpc(ulong clientId)
        {
            if (clientId == m_localUserData.id)
                VerifyConnectionConfirm_ServerRpc(m_localUserData);
        }

        /// Once the connection is confirmed, spawn a player cursor and check if all players have connected.
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnectionConfirm_ServerRpc(NetworkedPlayerData clientData)
        {
            m_dataStore.AddPlayer(clientData.id, clientData.name);
            bool areAllPlayersConnected = NetworkManager.Singleton.ConnectedClients.Count >= m_expectedPlayerCount;
            VerifyConnectionConfirm_ClientRpc(clientData.id, areAllPlayersConnected);
        }

        [ClientRpc]
        private void VerifyConnectionConfirm_ClientRpc(ulong clientId, bool canBeginGame)
        {
            if (clientId == m_localUserData.id)
            {
                m_onConnectionVerified?.Invoke();
                m_hasConnected = true;
            }

            if (canBeginGame && m_hasConnected)
            {
                m_timeout = -1;
                BeginGame();
            }
        }

        /// The game will begin either when all players have connected successfully or after a timeout.
        void BeginGame()
        {
            m_canSpawnInGameObjects = true;
            CoreGameManager.Instance.BeginGame();
            onGameBeginning?.Invoke();
        }

        public void Update()
        {
            if (m_timeout >= 0)
            {
                m_timeout -= Time.deltaTime;
                if (m_timeout < 0)
                    BeginGame();
            }
        }

        /// Called while on the host to determine if incoming input has scored or not.
        public void OnPlayerInput(ulong playerId)
        {
            
        }


        private void EndGame()
        {
            if (IsHost)
                StartCoroutine(EndGame_ClientsFirst());
        }

        private IEnumerator EndGame_ClientsFirst()
        {
            EndGame_ClientRpc();
            yield return null;
            SendLocalEndGameSignal();
        }

        [ClientRpc]
        private void EndGame_ClientRpc()
        {
            if (IsHost)
                return;
            SendLocalEndGameSignal();
        }

        private void SendLocalEndGameSignal()
        {
            m_onGameEnd();
        }
    }

}
