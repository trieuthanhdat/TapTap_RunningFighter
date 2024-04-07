using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TD.Networks.Data
{
    public class NetworkedDataService : NetworkBehaviour
    {
        // Using a singleton here since we need spawned PlayerCursors to be able to find it, but we don't need the flexibility offered by the Locator.
        public static NetworkedDataService Instance;

        Dictionary<ulong, NetworkedPlayerData> m_playerData = new Dictionary<ulong, NetworkedPlayerData>();
        ulong m_localId;

        // Clients will need to retrieve the host's player data since it isn't synchronized. During that process, they will supply these callbacks.
        // Since we use RPC calls to retrieve data, these callbacks need to be retained (since the scope of the method that the client calls to request
        // data will be left in order to make the server RPC call).
        Action<NetworkedPlayerData> m_onGetCurrentCallback;
        UnityEvent<NetworkedPlayerData> m_onEachPlayerCallback;

        public void Awake()
        {
            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Instance == this)
                Instance = null;
        }

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        public void AddPlayer(ulong id, string name)
        {
            if (!IsServer)
                return;

            if (!m_playerData.ContainsKey(id))
                m_playerData.Add(id, new NetworkedPlayerData(name, id, 0));
            else
                m_playerData[id] = new NetworkedPlayerData(name, id, 0);
        }

        /// <returns>The updated score for the player matching the id after adding the delta, or int.MinValue otherwise.</returns>
        public int UpdateScore(ulong id, int delta)
        {
            if (!IsServer)
                return int.MinValue;

            if (m_playerData.ContainsKey(id))
            {
                m_playerData[id].score += delta;
                return m_playerData[id].score;
            }
            return int.MinValue;
        }

        /// <summary>
        /// Retrieve the data for all players in order from 1st to last place, calling onEachPlayer for each.
        /// </summary>
        public void GetAllPlayerData(UnityEvent<NetworkedPlayerData> onEachPlayer)
        {
            m_onEachPlayerCallback = onEachPlayer;
            GetAllPlayerData_ServerRpc(m_localId);
        }

        [ServerRpc(RequireOwnership = false)]
        void GetAllPlayerData_ServerRpc(ulong callerId)
        {
            var sortedData = m_playerData.Select(kvp => kvp.Value).OrderByDescending(data => data.score);
            GetAllPlayerData_ClientRpc(callerId, sortedData.ToArray());
        }

        [ClientRpc]
        void GetAllPlayerData_ClientRpc(ulong callerId, NetworkedPlayerData[] sortedData)
        {
            if (callerId != m_localId)
                return;

            int rank = 1;
            foreach (var data in sortedData)
            {
                m_onEachPlayerCallback.Invoke(data);
                rank++;
            }
            m_onEachPlayerCallback = null;
        }

        //Retreive the data for one player, passing it to the onGet callback.
        public void GetPlayerData(ulong targetId, Action<NetworkedPlayerData> onGet)
        {
            m_onGetCurrentCallback = onGet;
            GetPlayerData_ServerRpc(targetId, m_localId);
        }

        [ServerRpc(RequireOwnership = false)]
        void GetPlayerData_ServerRpc(ulong id, ulong callerId)
        {
            if (m_playerData.ContainsKey(id))
                GetPlayerData_ClientRpc(callerId, m_playerData[id]);
            else
                GetPlayerData_ClientRpc(callerId, new NetworkedPlayerData(null, 0));
        }

        [ClientRpc]
        public void GetPlayerData_ClientRpc(ulong callerId, NetworkedPlayerData data)
        {
            if (callerId == m_localId)
            {
                m_onGetCurrentCallback?.Invoke(data);
                m_onGetCurrentCallback = null;
            }
        }
    }
}
