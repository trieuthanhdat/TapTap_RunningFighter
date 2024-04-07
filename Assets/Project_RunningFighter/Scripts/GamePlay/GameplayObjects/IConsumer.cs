using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Project_RunningFighter.Gameplay.GameplayObjects
{
    public interface IConsumer
    {
        public void ReceiveMP(ServerCharacter inflicter, int MP);
        public ulong NetworkObjectId { get; }

        [Flags]
        public enum SpecialConsumeFlags
        {
            None = 0,
            UnusedFlag = 1 << 0, // does nothing; see comments below
            ConsumeOnTrample = 1 << 1,
            NotComsumedByPlayers = 1 << 2,
        }
        public SpecialConsumeFlags GetSpecialDamageFlags();
        public bool IsConsumable();
    }

}
