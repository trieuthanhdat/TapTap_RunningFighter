using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    public class NetworkManaState : NetworkBehaviour
    {
        public enum CharacterManaState
        {
            Consuming,
            Depleted,
            Recovering,
            Full
        }
        [SerializeField]
        NetworkVariable<CharacterManaState> m_ManaState = new NetworkVariable<CharacterManaState>(CharacterManaState.Full);
        public NetworkVariable<CharacterManaState> ManaState => m_ManaState;

        [HideInInspector]
        public NetworkVariable<int> ManaPoints = new NetworkVariable<int>();

        public event System.Action ManaPointsDepleted;
        public event System.Action ManaPointsReplenished;

        void OnEnable()
        {
            ManaPoints.OnValueChanged += ManaPointsChanged;
        }

        void OnDisable()
        {
            ManaPoints.OnValueChanged -= ManaPointsChanged;
        }

        private void ManaPointsChanged(int previousValue, int newValue)
        {
            if (previousValue > 0 && newValue <= 0)
            {
                ManaPointsDepleted?.Invoke();
            }
            else if (previousValue <= 0 && newValue > 0)
            {
                ManaPointsReplenished?.Invoke();
            }
        }
    }

}
