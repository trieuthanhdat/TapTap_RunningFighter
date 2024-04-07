using Project_RunningFighter.Gameplay.GameplayObjects;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action
{
    
    [CreateAssetMenu(menuName = "Actions/Dash Attack Action")]
    public class DashAttackAction : GameAction
    {
        private Vector3 m_TargetSpot;

        private bool m_Dashed;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            m_TargetSpot = GameActionUtils.GetDashDestination(serverCharacter.physicsWrapper.Transform, Data.Position, true, Config.Range, Config.Range);

            serverCharacter.physicsWrapper.Transform.LookAt(m_TargetSpot);
            serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.clientCharacter.ClientPlayActionRpc(Data);

            return ActionConclusion.Continue;
        }

        public override void Reset()
        {
            base.Reset();
            m_TargetSpot = default;
            m_Dashed = false;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            return ActionConclusion.Continue;
        }

        public override void End(ServerCharacter serverCharacter)
        {
            if (!string.IsNullOrEmpty(Config.Anim2))
            {
                serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim2);
            }

            serverCharacter.Movement.Teleport(m_TargetSpot);

            PerformMeleeAttack(serverCharacter);
        }

        public override void Cancel(ServerCharacter serverCharacter)
        {
            if (!string.IsNullOrEmpty(Config.OtherAnimatorVariable))
            {
                serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.OtherAnimatorVariable);
            }

            serverCharacter.clientCharacter.ClientCancelActionsByPrototypeIDRpc(ActionID);

        }

        public override void BuffValue(BuffableValue buffType, ref float buffedValue)
        {
            if (TimeRunning >= Config.ExecTimeSeconds && buffType == BuffableValue.PercentDamageReceived)
            {
                // we suffer no damage during the "dash" (client-side pretend movement)
                buffedValue = 0;
            }
        }

        private void PerformMeleeAttack(ServerCharacter parent)
        {
            IDamageable foe = MeleeAction.GetIdealMeleeFoe(Config.IsFriendly ^ parent.IsNpc,
                parent.physicsWrapper.DamageCollider,
                                                            Config.Radius,
                                                            (Data.TargetIds != null && Data.TargetIds.Length > 0 ? Data.TargetIds[0] : 0));

            if (foe != null)
            {
                foe.ReceiveHP(parent, -Config.Amount);
            }
        }

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            if (m_Dashed) { return ActionConclusion.Stop; } // we're done!

            return ActionConclusion.Continue;
        }
    }
}
