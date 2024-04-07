using System;
using System.Collections;
using System.Collections.Generic;
using TD.UServices.Lobbies;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Project_RunningFighter.ConnectionManagement
{
    class StartingHostState : OnlineState
    {
        [Inject]
        LobbyServiceController m_LobbyServiceFacade;
        [Inject]
        LocalLobby m_LocalLobby;
        ConnectionMethodBase m_ConnectionMethod;

        public StartingHostState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            m_ConnectionMethod = baseConnectionMethod;
            return this;
        }

        public override void Enter()
        {
            StartHost();
        }
        public override void Exit() { }

        public override void OnServerStarted()
        {
            m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Hosting);
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            Debug.Log("STARTING HOSTING STATE: connectionData.Length " + connectionData.Length);
            Debug.Log("STARTING HOSTING STATE: client ID " + clientId + " m_ConnectionManager.NetworkManager.LocalClientId "+ m_ConnectionManager.NetworkManager.LocalClientId);

            if (clientId == m_ConnectionManager.NetworkManager.LocalClientId)
            {
                var payload = System.Text.Encoding.UTF8.GetString(connectionData);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); 
                Debug.Log("STARTING HOSTING STATE: client ID " + clientId + " connectionPayload.playerId " + connectionPayload.playerId);

                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                    new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, 0, true));

                response.Approved = true;
                response.CreatePlayerObject = true;
            }
        }

        public override void OnServerStopped()
        {
            StartHostFailed();
        }

        async void StartHost()
        {
            try
            {
                await m_ConnectionMethod.SetupHostConnectionAsync();

                if (!m_ConnectionManager.NetworkManager.StartHost())
                {
                    StartHostFailed();
                }
            }
            catch (Exception)
            {
                StartHostFailed();
                throw;
            }
        }

        void StartHostFailed()
        {
            m_ConnectStatusPublisher.Publish(ConnectStatus.StartHostFailed);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }
}
