using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Project_RunningFighter.Gameplay.Action
{
    [CreateAssetMenu(menuName = "Actions/Revive Action")]
    public class ReviveAction : GameAction
    {
        private bool m_ExecFired;
        private ServerCharacter m_TargetCharacter;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            if (m_Data.TargetIds == null || m_Data.TargetIds.Length == 0 || !NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(m_Data.TargetIds[0]))
            {
                Debug.Log("Failed to start ReviveAction. The target entity  wasn't submitted or doesn't exist anymore");
                return false;
            }

            var targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[m_Data.TargetIds[0]];
            m_TargetCharacter = targetNetworkObject.GetComponent<ServerCharacter>();

            serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim);

            return true;
        }

        public override void Reset()
        {
            base.Reset();
            m_ExecFired = false;
            m_TargetCharacter = null;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            if (!m_ExecFired && Time.time - TimeStarted >= Config.ExecTimeSeconds)
            {
                m_ExecFired = true;

                if (m_TargetCharacter.CharacterLifeState == NetworkLifeState.CharacterLifeState.Fainted)
                {
                    Assert.IsTrue(Config.Amount > 0, "Revive amount must be greater than 0.");
                    m_TargetCharacter.Revive(clientCharacter, Config.Amount);
                }
                else
                {
                    //cancel the action if the target is alive!
                    Cancel(clientCharacter);
                    return false;
                }
            }

            return true;
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
