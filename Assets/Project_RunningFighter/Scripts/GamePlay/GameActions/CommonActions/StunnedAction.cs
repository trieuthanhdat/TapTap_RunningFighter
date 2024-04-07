using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action
{
    
    [CreateAssetMenu(menuName = "Actions/Stunned Action")]
    public class StunnedAction : GameAction
    {
        public override bool OnStart(ServerCharacter serverCharacter)
        {
            serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim);
            return true;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            return true;
        }

        public override void BuffValue(BuffableValue buffType, ref float buffedValue)
        {
            if (buffType == BuffableValue.PercentDamageReceived)
            {
                buffedValue *= Config.Amount;
            }
        }

        public override void Cancel(ServerCharacter serverCharacter)
        {
            if (!string.IsNullOrEmpty(Config.Anim2))
            {
                serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim2);
            }
        }

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            return ActionConclusion.Continue;
        }
    }
}
