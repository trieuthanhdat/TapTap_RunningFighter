using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Data;
using Project_RunningFighter.Gameplay.Action;
using Project_RunningFighter.Gameplay.Action.CharacterActions;
using Project_RunningFighter.Gameplay.Action.Input;
using Project_RunningFighter.Gameplay.Camera;
using Project_RunningFighter.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    public class ClientCharacter : NetworkBehaviour
    {
        [SerializeField] private Animator m_ClientVisualsAnimator;
        [SerializeField] private VisualizationConfig m_VisualizationConfiguration;

        public Animator PlayerAnimator => m_ClientVisualsAnimator;
        public GameObject TargetReticulePrefab => m_VisualizationConfiguration.TargetReticule;
        public Material ReticuleHostileMat => m_VisualizationConfiguration.ReticuleHostileMat;
        public Material ReticuleFriendlyMat => m_VisualizationConfiguration.ReticuleFriendlyMat;

        private CharacterSwap m_CharacterSwapper;
        public CharacterSwap CharacterSwap => m_CharacterSwapper;
        public bool CanPerformActions => m_ServerCharacter.CanPerformActions;

        ServerCharacter m_ServerCharacter;

        public ServerCharacter serverCharacter => m_ServerCharacter;

        ClientPlayerAction m_ClientActionViz;

        PositionLerper m_PositionLerper;

        RotationLerper m_RotationLerper;

        // this value suffices for both positional and rotational interpolations; one may have a constant value for each
        const float k_LerpTime = 0.08f;
        Vector3 m_LerpedPosition;
        Quaternion m_LerpedRotation;
        float m_CurrentSpeed;

        [Rpc(SendTo.ClientsAndHost)]
        public void ClientPlayActionRpc(ActionRequestData data)
        {
            ActionRequestData data1 = data;
            m_ClientActionViz.PlayAction(ref data1);
        }
        //This RPC is invoked on the client when the active action FXs n
        //need to be cancelled (e.g. when the character has been stunned)
        [Rpc(SendTo.ClientsAndHost)]
        public void ClientCancelAllActionsRpc()
        {
            m_ClientActionViz.CancelAllActions();
        }

        //This RPC is invoked on the client
        //when active action FXs of a certain type need to be cancelled (e.g. when the Stealth action ends)
        [Rpc(SendTo.ClientsAndHost)]
        public void ClientCancelActionsByPrototypeIDRpc(GameActionID actionPrototypeID)
        {
            m_ClientActionViz.CancelAllActionsWithSamePrototypeID(actionPrototypeID);
        }

        //Called on all clients when this character has stopped "charging up" an attack.
        //Provides a value between 0 and 1 inclusive which indicates how "charged up" the attack ended up being.
        [Rpc(SendTo.ClientsAndHost)]
        public void ClientStopChargingUpRpc(float percentCharged)
        {
            m_ClientActionViz.OnStoppedChargingUp(percentCharged);
        }

        void Awake()
        {
            enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsClient || transform.parent == null)
            {
                return;
            }

            enabled = true;

            m_ClientActionViz = new ClientPlayerAction(this);

            m_ServerCharacter = GetComponentInParent<ServerCharacter>();

            //m_ServerCharacter.IsStealthy.OnValueChanged += OnStealthyChanged;
            m_ServerCharacter.MovementStatus.OnValueChanged += OnMovementStatusChanged;
            OnMovementStatusChanged(MovementStatus.Normal, m_ServerCharacter.MovementStatus.Value);

            // sync our visualization position & rotation to the most up to date version received from server
            transform.SetPositionAndRotation(serverCharacter.physicsWrapper.Transform.position,
                serverCharacter.physicsWrapper.Transform.rotation);
            m_LerpedPosition = transform.position;
            m_LerpedRotation = transform.rotation;

            // similarly, initialize start position and rotation for smooth lerping purposes
            m_PositionLerper = new PositionLerper(serverCharacter.physicsWrapper.Transform.position, k_LerpTime);
            m_RotationLerper = new RotationLerper(serverCharacter.physicsWrapper.Transform.rotation, k_LerpTime);

            if (!m_ServerCharacter.IsNpc)
            {
                name = "AvatarGraphics" + m_ServerCharacter.OwnerClientId;

                if (m_ServerCharacter.TryGetComponent(out ClientAvatarGuidController clientAvatarGuidHandler))
                {
                    m_ClientVisualsAnimator = clientAvatarGuidHandler.graphicsAnimator;
                }

                m_CharacterSwapper = GetComponentInChildren<CharacterSwap>();

                // ...and visualize the current char-select value that we know about
                SetAppearanceSwap();

                if (m_ServerCharacter.IsOwner)
                {
                    ActionRequestData data = new ActionRequestData
                                             {
                                                ActionID = GameDataSource.Instance.GeneralTargetActionPrototype.ActionID
                                             };

                    m_ClientActionViz.PlayAction(ref data);
                    gameObject.AddComponent<GameplayCameraController>();

                    if (m_ServerCharacter.TryGetComponent(out ClientInputSender inputSender))
                    {
                        // anticipated actions will only be played on non-host, owning clients
                        if (!IsServer)
                        {
                            inputSender.ActionInputEvent += OnActionInput;
                        }
                        inputSender.ClientMoveEvent += OnMoveInput;
                        inputSender.ClientTouchMoveEvent += OnMoveInput;
                    }
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (m_ServerCharacter)
            {
                //m_ServerCharacter.IsStealthy.OnValueChanged -= OnStealthyChanged;

                if (m_ServerCharacter.TryGetComponent(out ClientInputSender sender))
                {
                    sender.ActionInputEvent -= OnActionInput;
                    sender.ClientMoveEvent -= OnMoveInput;
                    sender.ClientTouchMoveEvent -= OnMoveInput;
                }
            }

            enabled = false;
        }

        private void OnActionInput(ActionRequestData data)
        {
            m_ClientActionViz.AnticipateAction(ref data);
        }

        private void OnMoveInput(Vector3 position)
        {
            if (!IsAnimating())
            {
                PlayerAnimator.SetBool(m_VisualizationConfiguration.StaticTypeBooleanID, true);
            }
        }
        private void OnMoveInput()
        {
            if (!IsAnimating())
            {
                PlayerAnimator.SetBool(m_VisualizationConfiguration.StaticTypeBooleanID, true);
            }
        }

        private void OnStealthyChanged(bool oldValue, bool newValue)
        {
            SetAppearanceSwap();
        }

        private void SetAppearanceSwap()
        {
            if (m_CharacterSwapper)
            {
                var specialMaterialMode = CharacterSwap.SpecialMaterialMode.None;
                //if (m_ServerCharacter.IsStealthy.Value)
                //{
                //    if (m_ServerCharacter.IsOwner)
                //    {
                //        specialMaterialMode = CharacterSwap.SpecialMaterialMode.StealthySelf;
                //    }
                //    else
                //    {
                //        specialMaterialMode = CharacterSwap.SpecialMaterialMode.StealthyOther;
                //    }
                //}

                m_CharacterSwapper.SwapToModel(specialMaterialMode);
            }
        }

        // Returns the value we should set the Animator's "Speed" variable, given current gameplay conditions.
        private float GetVisualMovementSpeed(MovementStatus movementStatus)
        {
            if (m_ServerCharacter.NetCharacterLifeState.LifeState.Value != NetworkLifeState.CharacterLifeState.Alive)
            {
                return m_VisualizationConfiguration.SpeedDead;
            }

            switch (movementStatus)
            {
                case MovementStatus.Idle:
                    return m_VisualizationConfiguration.SpeedIdle;
                case MovementStatus.Running:
                    return m_VisualizationConfiguration.SpeedRunning;
                case MovementStatus.Normal:
                    return m_VisualizationConfiguration.SpeedNormal;
                case MovementStatus.Uncontrolled:
                    return m_VisualizationConfiguration.SpeedUncontrolled;
                case MovementStatus.Slowed:
                    return m_VisualizationConfiguration.SpeedSlowed;
                case MovementStatus.Hasted:
                    return m_VisualizationConfiguration.SpeedHasted;
                case MovementStatus.Walking:
                    return m_VisualizationConfiguration.SpeedWalking;
                default:
                    throw new Exception($"Unknown MovementStatus {movementStatus}");
            }
        }

        private void OnMovementStatusChanged(MovementStatus previousValue, MovementStatus newValue)
        {
            m_CurrentSpeed = GetVisualMovementSpeed(newValue);
        }

        private void Update()
        {
            if (IsHost)
            {
                m_LerpedPosition = m_PositionLerper.LerpPosition(m_LerpedPosition,
                    serverCharacter.physicsWrapper.Transform.position);
                m_LerpedRotation = m_RotationLerper.LerpRotation(m_LerpedRotation,
                    serverCharacter.physicsWrapper.Transform.rotation);
                transform.SetPositionAndRotation(m_LerpedPosition, m_LerpedRotation);
            }

            if (m_ClientVisualsAnimator)
            {
                // set Animator variables here
                PlayerAnimator.SetFloat(m_VisualizationConfiguration.SpeedVariableID, m_CurrentSpeed);
            }

            m_ClientActionViz.OnUpdate();
        }

        private void OnAnimEvent(string id)
        {
            m_ClientActionViz.OnAnimEvent(id);
        }
        public bool IsAnimating()
        {
            if (PlayerAnimator.GetFloat(m_VisualizationConfiguration.SpeedVariableID) > 0.0) { return true; }

           /* for (int i = 0; i < PlayerAnimator.layerCount; i++)
            {
                if (PlayerAnimator.GetCurrentAnimatorStateInfo(i).tagHash != 
                    m_VisualizationConfiguration.BaseNodeTagID)
                {
                    //we are in an active node, not the default "nothing" node.
                    Debug.Log("NOTHING NODE!!!");
                    return true;
                }
            }*/

            return false;
        }
    }
}
