using Project_RunningFighter.Gameplay.GameStates;
using Project_RunningFighter.Infrastruture;
using System;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Project_RunningFighter.Gameplay.Messages
{
    [RequireComponent(typeof(ServerActionPhaseState))]
    public class GameplayStateChangedMessagePublisher : NetworkBehaviour
    {
        /*private ServerActionPhaseState m_ServerActionPhaseState;
        [Inject]
        IPublisher<GameplayStateChangedEventMessage> m_Publisher;
        private void Awake()
        {
            m_ServerActionPhaseState = GetComponent<ServerActionPhaseState>();
        }
        public override void OnNetworkSpawn()
        {
            if(IsServer)
            {
                if(m_ServerActionPhaseState != null)
                {
                    m_ServerActionPhaseState.GameplayState.OnValueChanged += OnGameplayStateChanged;
                    m_ServerActionPhaseState.Container.Inject(this);
                }
            }
        }

        private void OnGameplayStateChanged(ActionPhaseState previousValue, ActionPhaseState newValue)
        {
            Debug.Log("GAMEPLAY STATE PUBLISHER: publish!!");
            m_Publisher.Publish(new GameplayStateChangedEventMessage()
            {
                newGameplayState = newValue,
            });
        }
*/
       
    }

}
