using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    public class NetworkHealthState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<int> HitPoints = new NetworkVariable<int>();

        public event System.Action HitPointsDepleted;

        public event System.Action HitPointsReplenished;

        void OnEnable()
        {
            HitPoints.OnValueChanged += HitPointsChanged;
        }

        void OnDisable()
        {
            HitPoints.OnValueChanged -= HitPointsChanged;
        }

        void HitPointsChanged(int previousValue, int newValue)
        {
            if (previousValue > 0 && newValue <= 0)
            {
                HitPointsDepleted?.Invoke();
            }
            else if (previousValue <= 0 && newValue > 0)
            {
                HitPointsReplenished?.Invoke();
            }
        }
    }
}

