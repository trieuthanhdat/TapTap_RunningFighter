using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.VFX;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action
{
    
    [CreateAssetMenu(menuName = "Actions/Stealth Mode Action")]
    public class StealthModeAction : GameAction
    {
        private bool m_IsStealthStarted = false;
        private bool m_IsStealthEnded = false;

        private List<SpecialFXGraphic> m_SpawnedGraphics = null;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim);

            serverCharacter.clientCharacter.ClientPlayActionRpc(Data);

            return true;
        }

        public override void Reset()
        {
            base.Reset();
            m_IsStealthEnded = false;
            m_IsStealthStarted = false;
            m_SpawnedGraphics = null;
        }

        public override bool ShouldBecomeNonBlocking()
        {
            return TimeRunning >= Config.ExecTimeSeconds;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            if (TimeRunning >= Config.ExecTimeSeconds && !m_IsStealthStarted && !m_IsStealthEnded)
            {
                // start actual stealth-mode... NOW!
                /*m_IsStealthStarted = true;*/
                /*clientCharacter.IsStealthy.Value = true;*/
            }
            return !m_IsStealthEnded;
        }

        public override void Cancel(ServerCharacter serverCharacter)
        {
            if (!string.IsNullOrEmpty(Config.Anim2))
            {
                serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim2);
            }

            EndStealth(serverCharacter);
        }

        public override void OnGameplayActivity(ServerCharacter serverCharacter, GameplayActivity activityType)
        {
            // we break stealth after using an attack. (Or after being hit, which could happen during exec time before we're stealthed, or even afterwards, such as from an AoE attack)
            if (activityType == GameplayActivity.UsingAttackAction || activityType == GameplayActivity.AttackedByEnemy)
            {
                EndStealth(serverCharacter);
            }
        }

        private void EndStealth(ServerCharacter parent)
        {
            /*i*f (!m_IsStealthEnded)
            {
                m_IsStealthEnded = true;
               *//* if (m_IsStealthStarted)
                {
                    parent.IsStealthy.Value = false;
                }*//*

                parent.clientCharacter.ClientCancelActionsByPrototypeIDRpc(ActionID);
            }*/
        }

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            if (TimeRunning >= Config.ExecTimeSeconds && m_SpawnedGraphics == null && clientCharacter.IsOwner)
            {
                m_SpawnedGraphics = InstantiateSpecialFXGraphics(clientCharacter.transform, true);
            }

            return ActionConclusion.Continue;
        }

        public override void CancelClient(ClientCharacter clientCharacter)
        {
            if (m_SpawnedGraphics != null)
            {
                foreach (var graphic in m_SpawnedGraphics)
                {
                    if (graphic)
                    {
                        graphic.transform.SetParent(null);
                        graphic.Shutdown();
                    }
                }
            }
        }

    }
}
