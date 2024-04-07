using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Project_RunningFighter.Gameplay.Action
{
    [CreateAssetMenu(menuName = "Actions/Charged Shield Action")]
    public partial class ChargedShieldAction : GameAction
    {
       
        private float m_StoppedChargingUpTime = 0;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            if (m_Data.TargetIds != null && m_Data.TargetIds.Length > 0)
            {
                NetworkObject initialTarget = NetworkManager.Singleton.SpawnManager.SpawnedObjects[m_Data.TargetIds[0]];
                if (initialTarget)
                {
                    // face our target, if we had one
                    serverCharacter.physicsWrapper.Transform.LookAt(initialTarget.transform.position);
                }
            }

            serverCharacter.serverAnimationController.NetworkAnimator.ResetTrigger(Config.Anim2);
            serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.clientCharacter.ClientPlayActionRpc(Data);
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            m_ChargeGraphics = null;
            m_ShieldGraphics = null;
            m_StoppedChargingUpTime = 0;
        }

        private bool IsChargingUp()
        {
            return m_StoppedChargingUpTime == 0;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            if (m_StoppedChargingUpTime == 0)
            {
                // we haven't explicitly stopped charging up... but if we've reached max charge, that implicitly stops us
                if (TimeRunning >= Config.ExecTimeSeconds)
                {
                    StopChargingUp(clientCharacter);
                }
            }

            // we stop once the charge-up has ended and our effect duration has elapsed
            return m_StoppedChargingUpTime == 0 || Time.time < (m_StoppedChargingUpTime + Config.EffectDurationSeconds);
        }

        public override bool ShouldBecomeNonBlocking()
        {
            return m_StoppedChargingUpTime != 0;
        }

        private float GetPercentChargedUp()
        {
            return GameActionUtils.GetPercentChargedUp(m_StoppedChargingUpTime, TimeRunning, TimeStarted, Config.ExecTimeSeconds);
        }

        public override void BuffValue(BuffableValue buffType, ref float buffedValue)
        {
            if (buffType == BuffableValue.PercentDamageReceived)
            {
                float percentChargedUp = GetPercentChargedUp();

                float percentDamageReduction = 0.5f + ((percentChargedUp * percentChargedUp) / 2);

                buffedValue *= 1 - percentDamageReduction;
            }
            else if (buffType == BuffableValue.ChanceToStunTramplers)
            {
                if (GetPercentChargedUp() >= 1)
                {
                    buffedValue = 1;
                }
            }
        }

        public override void OnGameplayActivity(ServerCharacter serverCharacter, GameplayActivity activityType)
        {
            // for this particular type of Action, being attacked immediately causes you to stop charging up
            if (activityType == GameplayActivity.AttackedByEnemy || activityType == GameplayActivity.StoppedChargingUp)
            {
                StopChargingUp(serverCharacter);
            }
        }

        public override void Cancel(ServerCharacter serverCharacter)
        {
            StopChargingUp(serverCharacter);

            // if stepped into invincibility, decrement invincibility counter
            if (Mathf.Approximately(GetPercentChargedUp(), 1f))
            {
                serverCharacter.serverAnimationController.NetworkAnimator.Animator.SetInteger(Config.OtherAnimatorVariable,
                    serverCharacter.serverAnimationController.NetworkAnimator.Animator.GetInteger(Config.OtherAnimatorVariable) - 1);
            }
        }

        private void StopChargingUp(ServerCharacter parent)
        {
            if (IsChargingUp())
            {
                m_StoppedChargingUpTime = Time.time;
                parent.clientCharacter.ClientStopChargingUpRpc(GetPercentChargedUp());

                parent.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim2);

                parent.serverAnimationController.NetworkAnimator.ResetTrigger(Config.Anim);

                if (Mathf.Approximately(GetPercentChargedUp(), 1f))
                {
                    parent.serverAnimationController.NetworkAnimator.Animator.SetInteger(Config.OtherAnimatorVariable,
                        parent.serverAnimationController.NetworkAnimator.Animator.GetInteger(Config.OtherAnimatorVariable) + 1);
                }
            }
        }

        public override bool OnStartClient(ClientCharacter clientCharacter)
        {
            Assert.IsTrue(Config.Spawns.Length == 2, $"Found {Config.Spawns.Length} spawns for action {name}. Should be exactly 2: a charge-up particle and a fully-charged particle");

            base.OnStartClient(clientCharacter);
            m_ChargeGraphics = InstantiateSpecialFXGraphic(Config.Spawns[0], clientCharacter.transform, true);
            return true;
        }
    }
}
