using Project_RunningFighter.Gameplay.GameplayObjects;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project_RunningFighter.Gameplay.Action
{
    [CreateAssetMenu(menuName = "Actions/Trample Action")]
    public partial class TrampleAction : GameAction
    {
        public StunnedAction StunnedActionPrototype;

    
        private enum ActionStage
        {
            Windup,     // performing animations prior to actually moving
            Charging,   // running across the screen and hitting characters
            Complete,   // ending action
        }

        private const float k_PhysicalTouchDistance = 1;

        private ActionStage m_PreviousStage;

        private HashSet<Collider> m_CollidedAlready = new HashSet<Collider>();
        private bool m_WasStunned;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            m_PreviousStage = ActionStage.Windup;

            if (m_Data.TargetIds != null && m_Data.TargetIds.Length > 0)
            {
                NetworkObject initialTarget = NetworkManager.Singleton.SpawnManager.SpawnedObjects[m_Data.TargetIds[0]];
                if (initialTarget)
                {
                    Vector3 lookAtPosition;
                    if (CharacterPhysicWrapper.TryGetPhysicsWrapper(initialTarget.NetworkObjectId, out var physicsWrapper))
                    {
                        lookAtPosition = physicsWrapper.Transform.position;
                    }
                    else
                    {
                        lookAtPosition = initialTarget.transform.position;
                    }

                    // snap to face our target! This is the direction we'll attack in
                    serverCharacter.physicsWrapper.Transform.LookAt(lookAtPosition);
                }
            }

            // reset our "stop" trigger (in case the previous run of the trample action was aborted due to e.g. being stunned)
            if (!string.IsNullOrEmpty(Config.Anim2))
            {
                serverCharacter.serverAnimationController.NetworkAnimator.ResetTrigger(Config.Anim2);
            }
            // start the animation sequence!
            if (!string.IsNullOrEmpty(Config.Anim))
            {
                serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim);
            }

            serverCharacter.clientCharacter.ClientPlayActionRpc(Data);
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            m_PreviousStage = default;
            m_CollidedAlready.Clear();
            m_SpawnedGraphics = null;
            m_WasStunned = false;
        }

        private ActionStage GetCurrentStage()
        {
            float timeSoFar = Time.time - TimeStarted;
            if (timeSoFar < Config.ExecTimeSeconds)
            {
                return ActionStage.Windup;
            }
            if (timeSoFar < Config.DurationSeconds)
            {
                return ActionStage.Charging;
            }
            return ActionStage.Complete;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            ActionStage newState = GetCurrentStage();
            if (newState != m_PreviousStage && newState == ActionStage.Charging)
            {
                // we've just started to charge across the screen! Anyone currently touching us gets hit
                SimulateCollisionWithNearbyFoes(clientCharacter);
                clientCharacter.Movement.StartForwardCharge(Config.MoveSpeed, Config.DurationSeconds - Config.ExecTimeSeconds);
            }

            m_PreviousStage = newState;
            return newState != ActionStage.Complete && !m_WasStunned;
        }

     
        private void CollideWithVictim(ServerCharacter parent, ServerCharacter victim)
        {
            if (victim == parent)
            {
                return;
            }

            if (m_WasStunned)
            {
                return;
            }

            if (parent.IsNpc != victim.IsNpc)
            {
                float chanceToStun = victim.GetBuffedValue(BuffableValue.ChanceToStunTramplers);
                if (chanceToStun > 0 && Random.Range(0, 1) < chanceToStun)
                {
                    StunSelf(parent);
                    return;
                }

                int damage;
                if (m_Data.TargetIds != null && m_Data.TargetIds.Length > 0 && m_Data.TargetIds[0] == victim.NetworkObjectId)
                {
                    damage = Config.Amount;
                }
                else
                {
                    damage = Config.SplashDamage;
                }

                if (victim.gameObject.TryGetComponent(out IDamageable damageable))
                {
                    damageable.ReceiveHP(parent, -damage);
                }
            }

            var victimMovement = victim.Movement;
            victimMovement.StartKnockback(parent.physicsWrapper.Transform.position, Config.KnockbackSpeed, Config.KnockbackDuration);
        }

        public override void CollisionEntered(ServerCharacter serverCharacter, Collision collision)
        {
            if (GetCurrentStage() != ActionStage.Charging)
                return;

            Collide(serverCharacter, collision.collider);
        }

        private void Collide(ServerCharacter parent, Collider collider)
        {
            if (m_CollidedAlready.Contains(collider))
                return; // already hit them!

            m_CollidedAlready.Add(collider);

            var victim = collider.gameObject.GetComponentInParent<ServerCharacter>();
            if (victim)
            {
                CollideWithVictim(parent, victim);
            }
            else if (!m_WasStunned)
            {
                var damageable = collider.gameObject.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.ReceiveHP(parent, -Config.SplashDamage);

                    if ((damageable.GetSpecialDamageFlags() & IDamageable.SpecialDamageFlags.StunOnTrample) == IDamageable.SpecialDamageFlags.StunOnTrample)
                    {
                        StunSelf(parent);
                    }
                }
            }
        }

        private void SimulateCollisionWithNearbyFoes(ServerCharacter parent)
        {
            RaycastHit[] results;
            int numResults = GameActionUtils.DetectNearbyEntities(true, true, parent.physicsWrapper.DamageCollider, k_PhysicalTouchDistance, out results);
            for (int i = 0; i < numResults; i++)
            {
                Collide(parent, results[i].collider);
            }
        }

        private void StunSelf(ServerCharacter parent)
        {
            if (!m_WasStunned)
            {
                parent.Movement.CancelMove();
                parent.clientCharacter.ClientCancelAllActionsRpc();
            }
            m_WasStunned = true;
        }

        public override bool ChainIntoNewAction(ref ActionRequestData newAction)
        {
            if (m_WasStunned)
            {
                newAction = ActionRequestData.Create(StunnedActionPrototype);
                newAction.ShouldQueue = false;
                return true;
            }
            return false;
        }

        public override void Cancel(ServerCharacter serverCharacter)
        {
            if (!string.IsNullOrEmpty(Config.Anim2))
            {
                serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim2);
            }
        }
    }
}
