using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Data;
using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using static Project_RunningFighter.Gameplay.GameplayObjects.Characters.NetworkLifeState;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    public class ServerAnimationController : NetworkBehaviour
    {
        [SerializeField] NetworkAnimator m_NetworkAnimator;
        [SerializeField] VisualizationConfig m_VisualizationConfiguration;
        [SerializeField] NetworkLifeState m_NetworkLifeState;

        public NetworkAnimator NetworkAnimator => m_NetworkAnimator;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Wait until next frame before registering on OnValueChanged
                // to make sure NetworkAnimator has spawned before.
                StartCoroutine(WaitToRegisterOnLifeStateChanged());
            }
        }

        IEnumerator WaitToRegisterOnLifeStateChanged()
        {
            yield return new WaitForEndOfFrame();
            m_NetworkLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
            if (m_NetworkLifeState.LifeState.Value != CharacterLifeState.Alive)
            {
                OnLifeStateChanged(CharacterLifeState.Alive, m_NetworkLifeState.LifeState.Value);
            }
        }

        void OnLifeStateChanged(CharacterLifeState previousValue, CharacterLifeState newValue)
        {
            switch (newValue)
            {
                case CharacterLifeState.Alive:
                    NetworkAnimator.SetTrigger(m_VisualizationConfiguration.AliveStateTriggerID);
                    break;
                case CharacterLifeState.Fainted:
                    NetworkAnimator.SetTrigger(m_VisualizationConfiguration.FaintedStateTriggerID);
                    break;
                case CharacterLifeState.Dead:
                    NetworkAnimator.SetTrigger(m_VisualizationConfiguration.DeadStateTriggerID);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                m_NetworkLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;
            }
        }
    }
}
