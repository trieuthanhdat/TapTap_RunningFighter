using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.VFX;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action
{
    public partial class MeleeAction
    {
        private bool m_ImpactPlayed;

        private const float k_RangePadding = 3f;

        private List<SpecialFXGraphic> m_SpawnedGraphics = null;

        public override bool OnStartClient(ClientCharacter clientCharacter)
        {
            base.OnStartClient(clientCharacter);

            if (Data.TargetIds != null
                && Data.TargetIds.Length > 0
                && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(Data.TargetIds[0], out var targetNetworkObj)
                && targetNetworkObj != null)
            {
                float padRange = Config.Range + k_RangePadding;

                Vector3 targetPosition;
                if (CharacterPhysicWrapper.TryGetPhysicsWrapper(Data.TargetIds[0], out var physicsWrapper))
                {
                    targetPosition = physicsWrapper.Transform.position;
                }
                else
                {
                    targetPosition = targetNetworkObj.transform.position;
                }

                if ((clientCharacter.transform.position - targetPosition).sqrMagnitude < (padRange * padRange))
                {
                    // target is in range! Play the graphics
                    m_SpawnedGraphics = InstantiateSpecialFXGraphics(physicsWrapper ? physicsWrapper.Transform : targetNetworkObj.transform, true);
                }
            }

            return true;
        }

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            return ActionConclusion.Continue;
        }

        public override void OnAnimEventClient(ClientCharacter clientCharacter, string id)
        {
            if (id == "impact" && !m_ImpactPlayed)
            {
                PlayHitReact(clientCharacter);
            }
        }

        public override void EndClient(ClientCharacter clientCharacter)
        {
            //if this didn't already happen, make sure it gets a chance to run. This could have failed to run because
            //our animationclip didn't have the "impact" event properly configured (as one possibility).
            PlayHitReact(clientCharacter);
            base.EndClient(clientCharacter);
        }

        public override void CancelClient(ClientCharacter clientCharacter)
        {
            // if we had any special target graphics, tell them we're done
            if (m_SpawnedGraphics != null)
            {
                foreach (var spawnedGraphic in m_SpawnedGraphics)
                {
                    if (spawnedGraphic)
                    {
                        spawnedGraphic.Shutdown();
                    }
                }
            }
        }

        void PlayHitReact(ClientCharacter parent)
        {
            if (m_ImpactPlayed) { return; }

            m_ImpactPlayed = true;

            if (NetworkManager.Singleton.IsServer)
            {
                return;
            }

            //Is my original target still in range? Then definitely get him!
            if (Data.TargetIds != null &&
                Data.TargetIds.Length > 0 &&
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(Data.TargetIds[0], out var targetNetworkObj)
                && targetNetworkObj != null)
            {
                float padRange = Config.Range + k_RangePadding;

                Vector3 targetPosition;
                if (CharacterPhysicWrapper.TryGetPhysicsWrapper(Data.TargetIds[0], out var movementContainer))
                {
                    targetPosition = movementContainer.Transform.position;
                }
                else
                {
                    targetPosition = targetNetworkObj.transform.position;
                }

                if ((parent.transform.position - targetPosition).sqrMagnitude < (padRange * padRange))
                {
                    if (targetNetworkObj.NetworkObjectId != parent.NetworkObjectId)
                    {
                        string hitAnim = Config.ReactAnim;
                        if (string.IsNullOrEmpty(hitAnim)) { hitAnim = _DefaultHitReact; }

                        if (targetNetworkObj.TryGetComponent<ServerCharacter>(out var serverCharacter)
                            && serverCharacter.clientCharacter != null
                            && serverCharacter.clientCharacter.PlayerAnimator)
                        {
                            serverCharacter.clientCharacter.PlayerAnimator.SetTrigger(hitAnim);
                        }
                    }
                }
            }

            //in the future we may do another physics check to handle the case where a target "ran under our weapon".
            //But for now, if the original target is no longer present, then we just don't play our hit react on anything.
        }
    }
}
