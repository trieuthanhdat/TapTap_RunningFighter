using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using UnityEngine;


namespace Project_RunningFighter.Gameplay.GameplayObjects
{
    public interface IDamageable
    {
        public void ReceiveHP(ServerCharacter inflicter, int HP);
        public ulong NetworkObjectId { get; }
        public Transform transform { get; }

        [Flags]
        public enum SpecialDamageFlags
        {
            None = 0,
            UnusedFlag = 1 << 0, // does nothing; see comments below
            StunOnTrample = 1 << 1,
            NotDamagedByPlayers = 1 << 2,
            // The "UnusedFlag" flag does nothing. It exists to work around a Unity editor quirk involving [Flags] enums:
            // if you enable all the flags, Unity stores the value as 0xffffffff (labeled "Everything"), meaning that not
            // only are all the currently-existing flags enabled, but any future flags you added later would also be enabled!
            // This is not future-proof and can cause hard-to-track-down problems, when prefabs magically inherit a new flag
            // you just added. So we have the Unused flag, which should NOT do anything, and shouldn't be selected on prefabs.
            // It's just there so that we can select all the "real" flags and not get it turned into "Everything" in the editor.
        }
        public SpecialDamageFlags GetSpecialDamageFlags();

        public bool IsDamageable();
    }

}
