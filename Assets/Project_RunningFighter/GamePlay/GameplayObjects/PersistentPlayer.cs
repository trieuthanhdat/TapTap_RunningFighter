using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.ConnectionManagement;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.Utils;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.GameplayObjects
{
    /// NetworkBehaviour that represents a player connection and is the "Default Player Prefab" inside Netcode for
    /// GameObjects' (Netcode) NetworkManager. This NetworkBehaviour will contain several other NetworkBehaviours that
    /// should persist throughout the duration of this connection, meaning it will persist between scenes.
    /// It is not necessary to explicitly mark this as a DontDestroyOnLoad object as Netcode will handle migrating this
    /// Player object between scene loads.
    [RequireComponent(typeof(NetworkObject))]
    public class PersistentPlayer : NetworkBehaviour
    {
        [SerializeField] protected PersistentPlayerRuntimeCollection m_PersistentPlayerRuntimeCollection;

        [SerializeField] protected NetworkNameState m_NetworkNameState;

        [SerializeField] protected NetworkAvatarGuidState m_NetworkAvatarGuidState;

        public NetworkNameState NetworkNameState => m_NetworkNameState;

        public NetworkAvatarGuidState NetworkAvatarGuidState => m_NetworkAvatarGuidState;

        public override void OnNetworkSpawn()
        {
            gameObject.name = "PersistentPlayer" + OwnerClientId;
            
            m_PersistentPlayerRuntimeCollection.Add(this);
            if (IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerData = sessionPlayerData.Value;
                    m_NetworkNameState.Name.Value = playerData.PlayerName;
                    if (playerData.HasCharacterSpawned)
                    {
                        m_NetworkAvatarGuidState.AvatarGuid.Value = playerData.AvatarNetworkGuid;
                    }
                    else
                    {
                        m_NetworkAvatarGuidState.SetRandomAvatar();
                        playerData.AvatarNetworkGuid = m_NetworkAvatarGuidState.AvatarGuid.Value;
                        SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                    }
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemovePersistentPlayer();
        }

        public override void OnNetworkDespawn()
        {
            RemovePersistentPlayer();
        }

        void RemovePersistentPlayer()
        {
            m_PersistentPlayerRuntimeCollection.Remove(this);
            if (IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerData = sessionPlayerData.Value;
                    playerData.PlayerName = m_NetworkNameState.Name.Value;
                    playerData.AvatarNetworkGuid = m_NetworkAvatarGuidState.AvatarGuid.Value;
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                }
            }
        }
    }
}

