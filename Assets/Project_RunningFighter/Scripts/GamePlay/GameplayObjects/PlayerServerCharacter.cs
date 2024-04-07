using Project_RunningFighter.ConnectionManagement;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.GameplayObjects
{
    [RequireComponent(typeof(ServerCharacter))]
    public class PlayerServerCharacter : NetworkBehaviour
    {
        static List<ServerCharacter> s_ActivePlayers = new List<ServerCharacter>();

        [SerializeField] ServerCharacter m_CachedServerCharacter;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                s_ActivePlayers.Add(m_CachedServerCharacter);
            }
            else
            {
                enabled = false;
            }

        }

        void OnDisable()
        {
            s_ActivePlayers.Remove(m_CachedServerCharacter);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                var movementTransform = m_CachedServerCharacter.Movement.transform;
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerData = sessionPlayerData.Value;
                    playerData.PlayerPosition = movementTransform.position;
                    playerData.PlayerRotation = movementTransform.rotation;
                    playerData.CurrentHitPoints = m_CachedServerCharacter.HitPoints;
                    playerData.CurrentManaPoints = m_CachedServerCharacter.ManaPoints;
                    playerData.HasCharacterSpawned = true;
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                }
            }
        }
        
        /// Returns a list of all active players' ServerCharacters. Treat the list as read-only!
        /// The list will be empty on the client.
        public static List<ServerCharacter> GetPlayerServerCharacters()
        {
            return s_ActivePlayers;
        }

        
        /// Returns the ServerCharacter owned by a specific client. Always returns null on the client.
        /// <param name="ownerClientId"></param>
        /// <returns>The ServerCharacter owned by the client, or null if no ServerCharacter is found</returns>
        public static ServerCharacter GetPlayerServerCharacter(ulong ownerClientId)
        {
            foreach (var playerServerCharacter in s_ActivePlayers)
            {
                if (playerServerCharacter.OwnerClientId == ownerClientId)
                {
                    return playerServerCharacter;
                }
            }
            return null;
        }
    }
}
