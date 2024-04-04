using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action
{
    public struct GameActionID : INetworkSerializeByMemcpy, IEquatable<GameActionID>
    {
        public int ID;
        public bool Equals(GameActionID other)
        {
            return ID == other.ID;
        }
        public override bool Equals(object obj)
        {
            return obj is GameActionID other && Equals(other);
        }
        public override int GetHashCode()
        {
            return ID;
        }
        public static bool operator ==(GameActionID x, GameActionID y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(GameActionID x, GameActionID y)
        {
            return !(x == y);
        }
        public override string ToString()
        {
            return $"GAME ACTION ID ({ID})";
        }
    }
}
