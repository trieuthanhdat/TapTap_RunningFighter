using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using Unity.Netcode;
using UnityEngine;
using static Project_RunningFighter.Gameplay.Action.GameActionFactory;

namespace Project_RunningFighter.Gameplay.Action
{
 
    [CreateAssetMenu(menuName = "Actions/Target Action")]
    public partial class TargetAction : GameAction
    {
        public override bool OnStart(ServerCharacter serverCharacter)
        {
            serverCharacter.TargetId.Value = 0;
            serverCharacter.ActionPlayer.CancelRunningActionsByLogic(GameActionLogic.Target, true, this);

            if (Data.TargetIds == null || Data.TargetIds.Length == 0) { return false; }

            serverCharacter.TargetId.Value = TargetId;

            FaceTarget(serverCharacter, TargetId);

            return true;
        }

        public override void Reset()
        {
            base.Reset();
            m_TargetReticule = null;
            m_CurrentTarget = 0;
            m_NewTarget = 0;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            bool isValid = GameActionUtils.IsValidTarget(TargetId);

            if (clientCharacter.ActionPlayer.RunningActionCount == 1 && !clientCharacter.Movement.IsMoving() && isValid)
            {
                //we're the only action running, and we're not moving, so let's swivel to face our target, just to be cool!
                FaceTarget(clientCharacter, TargetId);
            }

            return isValid;
        }

        public override void Cancel(ServerCharacter serverCharacter)
        {
            if (serverCharacter.TargetId.Value == TargetId)
            {
                serverCharacter.TargetId.Value = 0;
            }
        }

        private ulong TargetId { get { return Data.TargetIds[0]; } }

        /// Only call this after validating the target via IsValidTarget.
        private void FaceTarget(ServerCharacter parent, ulong targetId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetObject))
            {
                Vector3 targetObjectPosition;

                if (targetObject.TryGetComponent(out ServerCharacter serverCharacter))
                {
                    targetObjectPosition = serverCharacter.physicsWrapper.Transform.position;
                }
                else
                {
                    targetObjectPosition = targetObject.transform.position;
                }

                Vector3 diff = targetObjectPosition - parent.physicsWrapper.Transform.position;

                diff.y = 0;
                if (diff != Vector3.zero)
                {
                    parent.physicsWrapper.Transform.forward = diff;
                }
            }
        }
    }
}

