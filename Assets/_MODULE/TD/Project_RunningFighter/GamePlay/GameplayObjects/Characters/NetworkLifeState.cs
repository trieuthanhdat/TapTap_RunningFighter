using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    
    public class NetworkLifeState : NetworkBehaviour
    {
        public enum CharacterLifeState
        {
            Alive,
            Fainted,
            Dead,
        }
        [SerializeField]
        NetworkVariable<CharacterLifeState> m_LifeState = new NetworkVariable<CharacterLifeState>(CharacterLifeState.Alive);

        public NetworkVariable<CharacterLifeState> LifeState => m_LifeState;
    }
}
