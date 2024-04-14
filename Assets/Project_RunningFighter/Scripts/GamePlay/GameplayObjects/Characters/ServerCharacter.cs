using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.ConnectionManagement;
using Project_RunningFighter.Data;
using Project_RunningFighter.Gameplay.Action;
using Project_RunningFighter.Gameplay.Action.CharacterActions;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using static Project_RunningFighter.Gameplay.Action.GameActionFactory;
using static Project_RunningFighter.Gameplay.GameplayObjects.Characters.NetworkLifeState;
using static Project_RunningFighter.Gameplay.GameplayObjects.Characters.NetworkManaState;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    public class ServerCharacter : NetworkBehaviour, ITargetable
    {
        [FormerlySerializedAs("m_ClientVisualization")]
        [SerializeField] private ClientCharacter m_ClientCharacter;
        [SerializeField] private CharacterClass m_CharacterClass;

        [SerializeField]
        [Tooltip("If set to false, an NPC character will be denied its brain (won't attack or chase players)")]
        private bool m_BrainEnabled = true;

        [SerializeField]
        [Tooltip("Setting negative value disables destroying object after it is killed.")]
        private float m_KilledDestroyDelaySeconds = 3.0f;

        [SerializeField]
        [Tooltip("If set, the ServerCharacter will automatically play the StartingAction when it is created. ")]
        private GameAction m_StartingAction;

        [SerializeField] DamageReceiver m_DamageReceiver;
        [SerializeField] ManaConsumer m_ManaConsumer;
        [SerializeField] ServerCharacterMovement m_Movement;
        [SerializeField] CharacterPhysicWrapper m_PhysicsWrapper;
        [SerializeField] ServerAnimationController m_ServerAnimationController;

        public ServerCharacterMovement   Movement => m_Movement;
        public CharacterPhysicWrapper    physicsWrapper => m_PhysicsWrapper;
        public ServerAnimationController serverAnimationController => m_ServerAnimationController;

        public ClientCharacter clientCharacter => m_ClientCharacter;
        public CharacterClass CharacterClass
        {
            get
            {
                if (m_CharacterClass == null)
                {
                    m_CharacterClass = m_State.RegisteredAvatar.CharacterClass;
                }

                return m_CharacterClass;
            }
            set => m_CharacterClass = value;
        }

        public NetworkVariable<MovementStatus> MovementStatus { get; } = new NetworkVariable<MovementStatus>();
        public NetworkVariable<ulong> HeldNetworkObject { get; } = new NetworkVariable<ulong>();
        public NetworkVariable<ulong> TargetId { get; } = new NetworkVariable<ulong>();
        public NetworkManaState NetManaState { get; private set; }
        public int ManaPoints
        {
            get => NetManaState.ManaPoints.Value;
            private set => NetManaState.ManaPoints.Value = value;
        }
        public CharacterManaState CharacterManaState
        {
            get => NetManaState.ManaState.Value;
            private set => NetManaState.ManaState.Value = value;
        }
        public NetworkHealthState NetHealthState { get; private set; }
        public int HitPoints
        {
            get => NetHealthState.HitPoints.Value;
            private set => NetHealthState.HitPoints.Value = value;
        }

        public NetworkLifeState NetCharacterLifeState { get; private set; }

        //Current CharacterLifeState. Only Players should enter the FAINTED state.
        public CharacterLifeState CharacterLifeState
        {
            get => NetCharacterLifeState.LifeState.Value;
            private set => NetCharacterLifeState.LifeState.Value = value;
        }
        public bool IsNpc => CharacterClass.IsNpc;

        public bool IsValidTarget => CharacterLifeState != CharacterLifeState.Dead;

        //Returns true if the Character is currently in a state where it can play actions, false otherwise.
        public bool CanPerformActions => CharacterLifeState == CharacterLifeState.Alive;
        public CharacterTypeEnum CharacterType => CharacterClass.CharacterType;

        private ServerPlayerAction m_ServerPlayerAction;
        public  ServerPlayerAction ActionPlayer => m_ServerPlayerAction;

        //private AIBrain m_AIBrain;
        NetworkAvatarGuidState m_State;

        void Awake()
        {
            m_ServerPlayerAction  = new ServerPlayerAction(this);
            NetCharacterLifeState = GetComponent<NetworkLifeState>();
            NetHealthState = GetComponent<NetworkHealthState>();
            NetManaState   = GetComponent<NetworkManaState>();
            m_State        = GetComponent<NetworkAvatarGuidState>();
        }

        private void Update()
        {
            m_ServerPlayerAction.OnUpdate();
            //if (m_AIBrain != null && CharacterLifeState == CharacterLifeState.Alive && m_BrainEnabled)
            //{
            //    m_AIBrain.Update();
            //}
        }

        void CollisionEntered(Collision collision)
        {
            if (m_ServerPlayerAction != null)
            {
                m_ServerPlayerAction.CollisionEntered(collision);
            }
        }


        public override void OnNetworkSpawn()
        {
            if (!IsServer) { enabled = false; }
            else
            {
                NetCharacterLifeState.LifeState.OnValueChanged += OnCharacterLifeStateChanged;

                NetManaState.ManaState.OnValueChanged += OnCharacterManaStateChanged;
                m_DamageReceiver.DamageReceived += ReceiveHP;
                m_DamageReceiver.CollisionEntered += CollisionEntered;

                m_ManaConsumer.OnConsumeMP += ReceiveMP;
                m_ManaConsumer.CollisionEntered += CollisionEntered;
                if (IsNpc)
                {
                    //TODO with AI behaviour
                    //m_AIBrain = new AIBrain(this, m_ServerPlayerAction);
                }

                if (m_StartingAction != null)
                {
                    var startingAction = new ActionRequestData() { ActionID = m_StartingAction.ActionID };
                    PlayAction(ref startingAction);
                }
                InitializeHitPoints();
                InitializeManaPoints();
            }
        }

       
        public override void OnNetworkDespawn()
        {
            NetCharacterLifeState.LifeState.OnValueChanged -= OnCharacterLifeStateChanged;
            NetManaState.ManaState.OnValueChanged -= OnCharacterManaStateChanged;
            if (m_DamageReceiver)
            {
                m_DamageReceiver.DamageReceived   -= ReceiveHP;
                m_DamageReceiver.CollisionEntered -= CollisionEntered;
            }
            if(m_ManaConsumer)
            {
                m_ManaConsumer.OnConsumeMP      -= ReceiveMP;
                m_ManaConsumer.CollisionEntered -= CollisionEntered;
            }
        }


        [Rpc(SendTo.Server)]
        public void ServerSendCharacterInputRpc(Vector3 movementTarget)
        {
            if (CharacterLifeState == CharacterLifeState.Alive && !m_Movement.IsPerformingForcedMovement())
            {
                // if we're currently playing an interruptible action, interrupt it!
                if (m_ServerPlayerAction.GetActiveActionInfo(out ActionRequestData data))
                {
                    if (GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).Config.ActionInterruptible)
                    {
                        m_ServerPlayerAction.ClearActions(false);
                    }
                }
                Debug.Log("SERVER CHARACTER: Send input rpc => move target " + movementTarget);
                m_ServerPlayerAction.CancelRunningActionsByLogic(GameActionLogic.Target, true); //clear target on move.
                m_Movement.SetMovementTarget(movementTarget);
            }
        }
        [Rpc(SendTo.Server)]
        public void ServerSendCharacterInputRpc()
        {
            if (!m_Movement.CanMoveByTouchScreen) return;
            if (CharacterLifeState == CharacterLifeState.Alive && !m_Movement.IsPerformingForcedMovement())
            {
                // if we're currently playing an interruptible action, interrupt it!
                if (m_ServerPlayerAction.GetActiveActionInfo(out ActionRequestData data))
                {
                    if (GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).Config.ActionInterruptible)
                    {
                        m_ServerPlayerAction.ClearActions(false);
                    }
                }
                m_ManaConsumer.ReceiveMP(this, -10);
                m_Movement.SetNextMovementTarget();
            }
        }

        // ACTION SYSTEM
        #region ___ACTION SYSTEM___
        [Rpc(SendTo.Server)]
        public void ServerPlayActionRpc(ActionRequestData data)
        {
            ActionRequestData data1 = data;
            if (!GameDataSource.Instance.GetActionPrototypeByID(data1.ActionID).Config.IsFriendly)
            {
                // notify running actions that we're using a new attack. (e.g. so Stealth can cancel itself)
                ActionPlayer.OnGameplayActivity(GameAction.GameplayActivity.UsingAttackAction);
            }

            PlayAction(ref data1);
        }

        // UTILITY AND SPECIAL-PURPOSE RPCs
        //Called on server when the character's client decides they have stopped "charging up" an attack.
        [Rpc(SendTo.Server)]
        public void ServerStopChargingUpRpc()
        {
            m_ServerPlayerAction.OnGameplayActivity(GameAction.GameplayActivity.StoppedChargingUp);
        }

        void InitializeHitPoints()
        {
            HitPoints = CharacterClass.BaseHP;

            if (!IsNpc)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    HitPoints = sessionPlayerData.Value.CurrentHitPoints;
                    if (HitPoints <= 0)
                    {
                        CharacterLifeState = CharacterLifeState.Fainted;
                    }
                }
            }
        }
        void InitializeManaPoints()
        {
            ManaPoints = CharacterClass.BaseMana;

            if (!IsNpc)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    ManaPoints = sessionPlayerData.Value.CurrentManaPoints;
                    if (ManaPoints <= 0)
                    {
                        
                    }
                }
            }
        }

        //Play a sequence of actions!
        public void PlayAction(ref ActionRequestData action)
        {
            //the character needs to be alive in order to be able to play actions
            if (CharacterLifeState == CharacterLifeState.Alive && !m_Movement.IsPerformingForcedMovement())
            {
                if (action.CancelMovement)
                {
                    m_Movement.CancelMove();
                }
                m_ServerPlayerAction.PlayAction(ref action);
            }
        }
        private void OnCharacterManaStateChanged(CharacterManaState previousManaState, CharacterManaState newManaState)
        {
            switch(newManaState)
            {
                case CharacterManaState.Consuming:
                    break;
                case CharacterManaState.Depleted:
                    m_Movement.CancelMove();
                    break;
                case CharacterManaState.Full:
                    break;
                case CharacterManaState.Recovering:
                    break;
            }
        }

        void OnCharacterLifeStateChanged(CharacterLifeState prevCharacterLifeState, CharacterLifeState CharacterLifeState)
        {
            if (CharacterLifeState != CharacterLifeState.Alive)
            {
                m_ServerPlayerAction.ClearActions(true);
                m_Movement.CancelMove();
            }
        }

        IEnumerator KilledDestroyProcess()
        {
            yield return new WaitForSeconds(m_KilledDestroyDelaySeconds);

            if (NetworkObject != null)
            {
                NetworkObject.Despawn(true);
            }
        }

        void ReceiveHP(ServerCharacter inflicter, int HP)
        {
            if (HP > 0)
            {
                m_ServerPlayerAction.OnGameplayActivity(GameAction.GameplayActivity.Healed);
                float healingMod = m_ServerPlayerAction.GetBuffedValue(GameAction.BuffableValue.PercentHealingReceived);
                HP = (int)(HP * healingMod);
            }
            else
            {

                m_ServerPlayerAction.OnGameplayActivity(GameAction.GameplayActivity.AttackedByEnemy);
                float damageMod = m_ServerPlayerAction.GetBuffedValue(GameAction.BuffableValue.PercentDamageReceived);
                HP = (int)(HP * damageMod);

                serverAnimationController.NetworkAnimator.SetTrigger("HitReact1");
            }

            HitPoints = Mathf.Clamp(HitPoints + HP, 0, CharacterClass.BaseHP);

            //if (m_AIBrain != null)
            //{
            //    //let the brain know about the modified amount of damage we received.
            //    m_AIBrain.ReceiveHP(inflicter, HP);
            //}

            //we can't currently heal a dead character back to Alive state.
            //that's handled by a separate function.
            if (HitPoints <= 0)
            {
                if (IsNpc)
                {
                    if (m_KilledDestroyDelaySeconds >= 0.0f && CharacterLifeState != CharacterLifeState.Dead)
                    {
                        StartCoroutine(KilledDestroyProcess());
                    }

                    CharacterLifeState = CharacterLifeState.Dead;
                }
                else
                {
                    CharacterLifeState = CharacterLifeState.Fainted;
                }

                m_ServerPlayerAction.ClearActions(false);
            }
        }
        void ReceiveMP(ServerCharacter inflicter, int MP)
        {
            if (MP > 0)
            {
                CharacterManaState = CharacterManaState.Recovering;
            }
            else
            {
                CharacterManaState = CharacterManaState.Consuming;
            }

            ManaPoints = Mathf.Clamp(ManaPoints + MP, 0, CharacterClass.BaseMana);
            if (ManaPoints <= 0)
            {
                if (IsNpc)
                {
                    //TODO for NPC Chars
                }
                else
                {
                    
                }

                m_ServerPlayerAction.ClearActions(false);
            }
        }

        public float GetBuffedValue(GameAction.BuffableValue buffType)
        {
            return m_ServerPlayerAction.GetBuffedValue(buffType);
        }
        public void Revive(ServerCharacter inflicter, int HP)
        {
            if (CharacterLifeState == CharacterLifeState.Fainted)
            {
                HitPoints = Mathf.Clamp(HP, 0, CharacterClass.BaseHP);
                NetCharacterLifeState.LifeState.Value = CharacterLifeState.Alive;
            }
        }
        #endregion

        ////This character's AIBrain. Will be null if this is not an NPC.
        //public AIBrain AIBrain { get { return m_AIBrain; } }

    }
}
