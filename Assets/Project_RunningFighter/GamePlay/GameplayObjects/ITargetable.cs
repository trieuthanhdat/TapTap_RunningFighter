using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.GameplayObjects
{
    public interface ITargetable
    {
        bool IsNpc { get; }
        bool IsValidTarget { get; }
    }
}
