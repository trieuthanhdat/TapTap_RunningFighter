using Project_RunningFighter.Gameplay.GameStates;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Messages
{
    public struct GameplayStateChangedEventMessage : INetworkSerializeByMemcpy
    {
        public ActionPhaseState newGameplayState;
    }

}
