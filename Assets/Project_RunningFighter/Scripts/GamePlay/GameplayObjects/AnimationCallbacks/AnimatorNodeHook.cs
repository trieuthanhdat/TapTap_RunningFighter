using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.GameplayObjects.AnimationCallbacks
{
    public class AnimatorNodeHook : StateMachineBehaviour
    {
        private AnimatorTriggeredSpecialFX[] m_CachedTriggerRefs;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_CachedTriggerRefs == null)
                m_CachedTriggerRefs = animator.GetComponentsInChildren<AnimatorTriggeredSpecialFX>();
            foreach (var fxController in m_CachedTriggerRefs)
            {
                if (fxController && fxController.enabled)
                {
                    fxController.OnStateEnter(animator, stateInfo, layerIndex);
                }
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_CachedTriggerRefs == null)
                m_CachedTriggerRefs = animator.GetComponentsInChildren<AnimatorTriggeredSpecialFX>();
            foreach (var fxController in m_CachedTriggerRefs)
            {
                if (fxController && fxController.enabled)
                {
                    fxController.OnStateExit(animator, stateInfo, layerIndex);
                }
            }
        }
    }
}
