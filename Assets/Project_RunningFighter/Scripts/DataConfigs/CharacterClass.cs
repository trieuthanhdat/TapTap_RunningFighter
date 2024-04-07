using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Gameplay.Action;
using UnityEngine;

namespace Project_RunningFighter.Data
{
    public enum CharacterTypeEnum
    {
        EarthLife,
        SkyLife,
        WaterLife,
    }
    [CreateAssetMenu(menuName = "GameData/CharacterClass", order = 1)]
    public class CharacterClass : ScriptableObject
    {
        public CharacterTypeEnum CharacterType;
        public GameAction Skill1;
        public GameAction Skill2;
        public GameAction Skill3;
        public int BaseHP;
        public int BaseMana;

        [Tooltip("Base movement speed of this character class (in meters/sec)")]
        public float Speed;

        [Tooltip("Base movement Distance of this character class (in meters)")]
        public float MoveDistance;
        [Tooltip("Base movement CoolDown of this character class (in Seconds)")]
        public float MoveCoolDown = 3f;

        [Tooltip("Set to true If this is an NPC/AI")]
        public bool IsNpc;

        [Tooltip("For NPCs, this will be used as the aggro radius at which enemies start to attack player")]
        public float DetectRange;

        [Tooltip("For players, this is the displayed \"class name\".")]
        public string DisplayedName;

        [Tooltip("For players, this is the class banner (when active).")]
        public Sprite ClassBannerSprite;

    }
}
