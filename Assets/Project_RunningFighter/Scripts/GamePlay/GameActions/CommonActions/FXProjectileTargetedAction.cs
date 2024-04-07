using Project_RunningFighter.Gameplay.GameplayObjects;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action
{
    
    [CreateAssetMenu(menuName = "Actions/FX Projectile Targeted Action")]
    public partial class FXProjectileTargetedAction : GameAction
    {
        private bool m_ImpactedTarget;
        private float m_TimeUntilImpact;
        private IDamageable m_DamageableTarget;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            m_DamageableTarget = GetDamageableTarget(serverCharacter);

            Vector3 targetPos = m_DamageableTarget != null ? m_DamageableTarget.transform.position : m_Data.Position;

            if (!GameActionUtils.HasLineOfSight(serverCharacter.physicsWrapper.Transform.position, targetPos, out Vector3 collidePos))
            {
                m_DamageableTarget = null;
                targetPos = collidePos;

                Data.TargetIds = new ulong[0];
                Data.Position = targetPos;
            }

            serverCharacter.physicsWrapper.Transform.LookAt(targetPos);

            float distanceToTargetPos = Vector3.Distance(targetPos, serverCharacter.physicsWrapper.Transform.position);
            m_TimeUntilImpact = Config.ExecTimeSeconds + (distanceToTargetPos / Config.Projectiles[0].Speed_m_s);

            serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.clientCharacter.ClientPlayActionRpc(Data);
            return true;
        }

        public override void Reset()
        {
            base.Reset();

            m_ImpactedTarget = false;
            m_TimeUntilImpact = 0;
            m_DamageableTarget = null;
            m_ImpactPlayed = false;
            m_ProjectileDuration = 0;
            m_Projectile = null;
            m_Target = null;
            m_TargetTransform = null;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            if (!m_ImpactedTarget && m_TimeUntilImpact <= TimeRunning)
            {
                m_ImpactedTarget = true;
                if (m_DamageableTarget != null)
                {
                    m_DamageableTarget.ReceiveHP(clientCharacter, -Config.Projectiles[0].Damage);
                }
            }
            return true;
        }

        public override void Cancel(ServerCharacter serverCharacter)
        {
            if (!m_ImpactedTarget)
            {
                serverCharacter.clientCharacter.ClientCancelActionsByPrototypeIDRpc(ActionID);
            }
        }

        private IDamageable GetDamageableTarget(ServerCharacter parent)
        {
            if (Data.TargetIds == null || Data.TargetIds.Length == 0)
            {
                return null;
            }

            NetworkObject obj;
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(Data.TargetIds[0], out obj) && obj != null)
            {
                var serverChar = obj.GetComponent<ServerCharacter>();
                if (serverChar && serverChar.IsNpc == (Config.IsFriendly ^ parent.IsNpc))
                {
                    return null;
                }

                if (CharacterPhysicWrapper.TryGetPhysicsWrapper(Data.TargetIds[0], out var physicsWrapper))
                {
                    return physicsWrapper.DamageCollider.GetComponent<IDamageable>();
                }
                else
                {
                    return obj.GetComponent<IDamageable>();
                }
            }
            else
            {
                Debug.Log($"FXProjectileTargetedAction was targeted at ID {Data.TargetIds[0]}, but that target can't be found in spawned object list! (May have just been deleted?)");
                return null;
            }
        }
    }
}
