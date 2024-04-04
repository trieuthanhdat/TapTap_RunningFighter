using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Unity.Netcode;
using UnityEngine;
using static Project_RunningFighter.Gameplay.GameplayObjects.Characters.NetworkLifeState;

namespace Project_RunningFighter.Gameplay.GameplayObjects
{
    public class DamageReceiver : NetworkBehaviour, IDamageable
    {
        [SerializeField] NetworkLifeState m_NetworkLifeState;

        #region ___EVENTS___
        public event Action<ServerCharacter, int> DamageReceived;
        public event Action<Collision> CollisionEntered;
        #endregion

        public void ReceiveHP(ServerCharacter inflicter, int HP)
        {
            if (IsDamageable())
            {
                DamageReceived?.Invoke(inflicter, HP);
            }
        }

        public IDamageable.SpecialDamageFlags GetSpecialDamageFlags()
        {
            return IDamageable.SpecialDamageFlags.None;
        }

        public bool IsDamageable()
        {
            return m_NetworkLifeState.LifeState.Value == CharacterLifeState.Alive;
        }

        void OnCollisionEnter(Collision other)
        {
            CollisionEntered?.Invoke(other);
        }
    }
}
