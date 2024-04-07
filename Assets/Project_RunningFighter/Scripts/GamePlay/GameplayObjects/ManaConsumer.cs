using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace Project_RunningFighter.Gameplay.GameplayObjects
{
    public class ManaConsumer : NetworkBehaviour, IConsumer
    {
        #region ___EVENTS___
        public event Action<ServerCharacter, int> OnConsumeMP;
        public event Action<Collision> CollisionEntered; //This field is for Mana Depletion Event
        #endregion
        
        public IConsumer.SpecialConsumeFlags GetSpecialDamageFlags()
        {
            return IConsumer.SpecialConsumeFlags.None;
        }

        public bool IsConsumable()
        {
            return true;
        }

        public void ReceiveMP(ServerCharacter inflicter, int MP)
        {
            if (IsConsumable())
            {
                OnConsumeMP?.Invoke(inflicter, MP);
            }
        }
    }
}