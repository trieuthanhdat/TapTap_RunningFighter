using System;
using Project_RunningFighter.Infrastruture;
using UnityEngine;

namespace Project_RunningFighter.Data
{
    [CreateAssetMenu]
    [Serializable]
    public sealed class Avatar : GuidScriptableObject
    {
        public CharacterClass CharacterClass;

        public GameObject Graphics;

        public GameObject GraphicsCharacterSelect;

        public Sprite Portrait;
    }
}
