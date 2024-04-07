using UnityEngine;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    public class PickUpState : MonoBehaviour, ITargetable
    {
        public bool IsNpc => true;
        public bool IsValidTarget => true;
    }
}
