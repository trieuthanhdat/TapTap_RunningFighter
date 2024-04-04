using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Data;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using static Project_RunningFighter.Gameplay.Action.GameActionFactory;
using static Project_RunningFighter.Gameplay.GameplayObjects.Characters.NetworkLifeState;

namespace Project_RunningFighter.Gameplay.Action.Input
{
    public enum SkillTriggerStyle
    {
        None,        //no skill was triggered.
        ScreenTouch, //skill was triggerd via touching screen
        MouseClick,  //skill was triggered via mouse-click implying you should do a raycast from the mouse position to find a target.
        Keyboard,    //skill was triggered via a Keyboard press, implying target should be taken from the active target.
        KeyboardRelease, //represents a released key.
        UI,          //skill was triggered from the UI, and similar to Keyboard, target should be inferred from the active target.
        UIRelease,   //represents letting go of the mouse-button on a UI button
    }

    [RequireComponent(typeof(ServerCharacter))]
    public class ClientInputSender : NetworkBehaviour
    {
        [SerializeField] ServerCharacter _ServerCharacter;
        [SerializeField] CharacterPhysicWrapper _PhysicsWrapper;
        #region ___EVENTS___
        public event Action<ActionRequestData> ActionInputEvent;
        public event Action<Vector3> ClientMoveEvent;
        public System.Action action1ModifiedCallback;


        #endregion

        #region ___PROPERTIES___
        public CharacterClass CharacterClass => _ServerCharacter.CharacterClass;

        protected bool   _MoveRequest;
        protected UnityEngine.Camera _MainCamera;

        //Number of ActionRequests that have been queued
        //since the last FixedUpdate.
        protected int   _ActionRequestCount;
        BaseActionInput _CurrentSkillInput;

        protected const float _MoveSendRateSeconds = 0.04f; //25 fps.
        protected float _LastSentMove;

        readonly RaycastHit[] _CachedHit = new RaycastHit[4];
        LayerMask m_GroundLayerMask;
        LayerMask m_ActionLayerMask;
        const float _MouseInputRaycastDistance = 100f;
        const float _MaxNavMeshDistance = 1f;
        RaycastHitComparer _RaycastHitComparer;

        private readonly ActionRequest[] m_ActionRequests = new ActionRequest[5];
        struct ActionRequest
        {
            public SkillTriggerStyle TriggerStyle;
            public GameActionID RequestedActionID;
            public ulong TargetId;
        }
        public ActionState actionState1 { get; private set; }
        public ActionState actionState2 { get; private set; }
        public ActionState actionState3 { get; private set; }

        ServerCharacter m_TargetServerCharacter;
        #endregion

        private void Awake()
        {
            _MainCamera = UnityEngine.Camera.main;
        }
        
        private void FixedUpdate()
        {
            OnUpdateWithMouseInput();
            OnUpdateWithTouchInput();
        }

        private void Update()
        {
            OnGetKeyboardInput();
        }

        private void OnGetKeyboardInput()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) && CharacterClass.Skill1)
            {
                RequestAction(actionState1.actionID, SkillTriggerStyle.Keyboard);
            }
            else if (UnityEngine.Input.GetKeyUp(KeyCode.Alpha1) && CharacterClass.Skill1)
            {
                RequestAction(actionState1.actionID, SkillTriggerStyle.KeyboardRelease);
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) && CharacterClass.Skill2)
            {
                RequestAction(actionState2.actionID, SkillTriggerStyle.Keyboard);
            }
            else if (UnityEngine.Input.GetKeyUp(KeyCode.Alpha2) && CharacterClass.Skill2)
            {
                RequestAction(actionState2.actionID, SkillTriggerStyle.KeyboardRelease);
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3) && CharacterClass.Skill3)
            {
                RequestAction(actionState3.actionID, SkillTriggerStyle.Keyboard);
            }
            else if (UnityEngine.Input.GetKeyUp(KeyCode.Alpha3) && CharacterClass.Skill3)
            {
                RequestAction(actionState3.actionID, SkillTriggerStyle.KeyboardRelease);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5))
            {
                RequestAction(GameDataSource.Instance.Emote1ActionPrototype.ActionID, SkillTriggerStyle.Keyboard);
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha6))
            {
                RequestAction(GameDataSource.Instance.Emote2ActionPrototype.ActionID, SkillTriggerStyle.Keyboard);
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha7))
            {
                RequestAction(GameDataSource.Instance.Emote3ActionPrototype.ActionID, SkillTriggerStyle.Keyboard);
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha8))
            {
                RequestAction(GameDataSource.Instance.Emote4ActionPrototype.ActionID, SkillTriggerStyle.Keyboard);
            }

            if (!EventSystem.current.IsPointerOverGameObject() &&
                _CurrentSkillInput == null)
            {
                //IsPointerOverGameObject() is a simple way to determine if the mouse is over a UI element. If it is, we don't perform mouse input logic,
                //to model the button "blocking" mouse clicks from falling through and interacting with the world.

                if (UnityEngine.Input.GetMouseButtonDown(1))
                {
                    RequestAction(CharacterClass.Skill1.ActionID, SkillTriggerStyle.MouseClick);
                }

                if (UnityEngine.Input.GetMouseButtonDown(0))
                {
                    RequestAction(GameDataSource.Instance.GeneralTargetActionPrototype.ActionID, SkillTriggerStyle.MouseClick);
                }
                else if (UnityEngine.Input.GetMouseButton(0))
                {
                    _MoveRequest = true;
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!IsClient || !IsOwner)
            {
                enabled = false;
                // dont need to do anything else if not the owner
                return;
            }
            if (_ServerCharacter)
            {
                _ServerCharacter.TargetId.OnValueChanged += OnTargetChanged;
                _ServerCharacter.HeldNetworkObject.OnValueChanged += OnHeldNetworkObjectChanged;
            }

            if (CharacterClass.Skill1 &&
                GameDataSource.Instance.TryGetActionPrototypeByID(CharacterClass.Skill1.ActionID, out var action1))
            {
                actionState1 = new ActionState() { actionID = action1.ActionID, selectable = true };
            }
            if (CharacterClass.Skill2 &&
                GameDataSource.Instance.TryGetActionPrototypeByID(CharacterClass.Skill2.ActionID, out var action2))
            {
                actionState2 = new ActionState() { actionID = action2.ActionID, selectable = true };
            }
            if (CharacterClass.Skill3 &&
                GameDataSource.Instance.TryGetActionPrototypeByID(CharacterClass.Skill3.ActionID, out var action3))
            {
                actionState3 = new ActionState() { actionID = action3.ActionID, selectable = true };
            }

            m_GroundLayerMask = LayerMask.GetMask(new[] { "Ground" });
            m_ActionLayerMask = LayerMask.GetMask(new[] { "PCs", "NPCs", "Ground" });
        }

        public override void OnNetworkDespawn()
        {
            if (_ServerCharacter)
            {
                _ServerCharacter.TargetId.OnValueChanged -= OnTargetChanged;
                _ServerCharacter.HeldNetworkObject.OnValueChanged -= OnHeldNetworkObjectChanged;
            }

            if (m_TargetServerCharacter)
            {
                m_TargetServerCharacter.NetCharacterLifeState.LifeState.OnValueChanged -= OnTargetLifeStateChanged;
            }
        }

        #region ___INPUT METHODS___

        private void OnUpdateWithTouchInput()
        {
            //play all ActionRequests, in FIFO order.
            for (int i = 0; i < _ActionRequestCount; ++i)
            {
                if (_CurrentSkillInput != null)
                {
                    //actions requested while input is active are discarded, except for "Release" requests, which go through.
                    if (IsReleaseStyle(m_ActionRequests[i].TriggerStyle))
                    {
                        _CurrentSkillInput.OnReleaseKey();
                    }
                }
                else if (!IsReleaseStyle(m_ActionRequests[i].TriggerStyle))
                {
                    var actionPrototype = GameDataSource.Instance.GetActionPrototypeByID(m_ActionRequests[i].RequestedActionID);
                    if (actionPrototype.Config.ActionInput != null)
                    {
                        var skillPlayer = Instantiate(actionPrototype.Config.ActionInput);
                        skillPlayer.Initiate(_ServerCharacter, _PhysicsWrapper.Transform.position, actionPrototype.ActionID, SendInput, FinishSkill);
                        _CurrentSkillInput = skillPlayer;
                    }
                    else
                    {
                        PerformSkill(actionPrototype.ActionID, m_ActionRequests[i].TriggerStyle, m_ActionRequests[i].TargetId);
                    }
                }
            }

            _ActionRequestCount = 0;

            if (EventSystem.current.currentSelectedGameObject != null)
            {
                return;
            }

            if (_MoveRequest)
            {
                _MoveRequest = false;
                if ((Time.time - _LastSentMove) > _MoveSendRateSeconds)
                {
                    _LastSentMove = Time.time;

                    // Check if there's a touch
                    if (UnityEngine.Input.touchCount > 0)
                    {
                        Touch touch = UnityEngine.Input.GetTouch(0); // Get the first touch

                        if (touch.phase == TouchPhase.Began) // Check if the touch just started
                        {
                            var ray = _MainCamera.ScreenPointToRay(touch.position);

                            var groundHits = Physics.RaycastNonAlloc(ray,
                                _CachedHit,
                                _MouseInputRaycastDistance,
                                m_GroundLayerMask);

                            if (groundHits > 0)
                            {
                                if (groundHits > 1)
                                {
                                    // sort hits by distance
                                    Array.Sort(_CachedHit, 0, groundHits, _RaycastHitComparer);
                                }

                                // verify point is indeed on navmesh surface
                                if (NavMesh.SamplePosition(_CachedHit[0].point,
                                        out var hit,
                                        _MaxNavMeshDistance,
                                        NavMesh.AllAreas))
                                {
                                    _ServerCharacter.ServerSendCharacterInputRpc(hit.position);
                                    //Send our client only click request
                                    ClientMoveEvent?.Invoke(hit.position);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnUpdateWithMouseInput()
        {
            //play all ActionRequests, in FIFO order.
            for (int i = 0; i < _ActionRequestCount; ++i)
            {
                if (_CurrentSkillInput != null)
                {
                    //actions requested while input is active are discarded, except for "Release" requests, which go through.
                    if (IsReleaseStyle(m_ActionRequests[i].TriggerStyle))
                    {
                        _CurrentSkillInput.OnReleaseKey();
                    }
                }
                else if (!IsReleaseStyle(m_ActionRequests[i].TriggerStyle))
                {
                    var actionPrototype = GameDataSource.Instance.GetActionPrototypeByID(m_ActionRequests[i].RequestedActionID);
                    if (actionPrototype.Config.ActionInput != null)
                    {
                        var skillPlayer = Instantiate(actionPrototype.Config.ActionInput);
                        skillPlayer.Initiate(_ServerCharacter, _PhysicsWrapper.Transform.position, actionPrototype.ActionID, SendInput, FinishSkill);
                        _CurrentSkillInput = skillPlayer;
                    }
                    else
                    {
                        PerformSkill(actionPrototype.ActionID, m_ActionRequests[i].TriggerStyle, m_ActionRequests[i].TargetId);
                    }
                }
            }

            _ActionRequestCount = 0;

            if (EventSystem.current.currentSelectedGameObject != null)
            {
                return;
            }

            if (_MoveRequest)
            {
                _MoveRequest = false;
                if ((Time.time - _LastSentMove) > _MoveSendRateSeconds)
                {
                    _LastSentMove = Time.time;
                    var ray = _MainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);

                    var groundHits = Physics.RaycastNonAlloc(ray,
                        _CachedHit,
                        _MouseInputRaycastDistance,
                        m_GroundLayerMask);

                    if (groundHits > 0)
                    {
                        if (groundHits > 1)
                        {
                            // sort hits by distance
                            Array.Sort(_CachedHit, 0, groundHits, _RaycastHitComparer);
                        }

                        // verify point is indeed on navmesh surface
                        if (NavMesh.SamplePosition(_CachedHit[0].point,
                                out var hit,
                                _MaxNavMeshDistance,
                                NavMesh.AllAreas))
                        {
                            _ServerCharacter.ServerSendCharacterInputRpc(hit.position);
                            //Send our client only click request
                            ClientMoveEvent?.Invoke(hit.position);
                        }
                    }
                }
            }
        }
        private void PerformSkill(GameActionID actionID, SkillTriggerStyle triggerStyle, ulong targetId = 0)
        {
            
        }

        private void FinishSkill()
        {
            _CurrentSkillInput = null;
        }

        void SendInput(ActionRequestData action)
        {
            ActionInputEvent?.Invoke(action);
            _ServerCharacter.ServerPlayActionRpc(action);
        }

        void OnTargetChanged(ulong previousValue, ulong newValue)
        {
            if (m_TargetServerCharacter)
            {
                m_TargetServerCharacter.NetCharacterLifeState.LifeState.OnValueChanged -= OnTargetLifeStateChanged;
            }

            m_TargetServerCharacter = null;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(newValue, out var selection) &&
                selection.TryGetComponent(out m_TargetServerCharacter))
            {
                m_TargetServerCharacter.NetCharacterLifeState.LifeState.OnValueChanged += OnTargetLifeStateChanged;
            }

            UpdateAction1();
        }
        private void OnHeldNetworkObjectChanged(ulong previousValue, ulong newValue)
        {
            UpdateAction1();
        }

        private void OnTargetLifeStateChanged(CharacterLifeState previousValue, CharacterLifeState newValue)
        {
            UpdateAction1();
        }
        void UpdateAction1()
        {
            var isHoldingNetworkObject =
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(_ServerCharacter.HeldNetworkObject.Value,
                    out var heldNetworkObject);

            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(_ServerCharacter.TargetId.Value,
                out var selection);

            var isSelectable = true;
            if (isHoldingNetworkObject)
            {
                // show drop!
                actionState1.actionID = GameDataSource.Instance.DropActionPrototype.ActionID;
            }
            else if ((_ServerCharacter.TargetId.Value != 0
                    && selection != null
                    && selection.TryGetComponent(out PickUpState pickUpState)))
            {
                // special case: targeting a pickup-able item or holding a pickup object
                actionState1.actionID = GameDataSource.Instance.PickUpActionPrototype.ActionID;
            }
            else if (_ServerCharacter.TargetId.Value != 0
                && selection != null
                && selection.NetworkObjectId != _ServerCharacter.NetworkObjectId
                && selection.TryGetComponent(out ServerCharacter charState)
                && !charState.IsNpc)
            {
                // special case: when we have a player selected, we change the meaning of the basic action
                // we have another player selected! In that case we want to reflect that our basic Action is a Revive, not an attack!
                // But we need to know if the player is alive... if so, the button should be disabled (for better player communication)
                actionState1.actionID = GameDataSource.Instance.ReviveActionPrototype.ActionID;
                isSelectable = charState.NetCharacterLifeState.LifeState.Value != CharacterLifeState.Alive;
            }
            else
            {
                actionState1.SetActionState(CharacterClass.Skill1.ActionID);
            }

            actionState1.selectable = isSelectable;

            action1ModifiedCallback?.Invoke();
        }
        private bool IsReleaseStyle(SkillTriggerStyle style)
        {
            return style == SkillTriggerStyle.KeyboardRelease || style == SkillTriggerStyle.UIRelease;
        }
        private bool GetActionRequestForTarget(Transform hit, GameActionID actionID, SkillTriggerStyle triggerStyle, out ActionRequestData resultData)
        {
            resultData = new ActionRequestData();

            var targetNetObj = hit != null ? hit.GetComponentInParent<NetworkObject>() : null;

            //if we can't get our target from the submitted hit transform, get it from our stateful target in our ServerCharacter.
            if (!targetNetObj && !GameDataSource.Instance.GetActionPrototypeByID(actionID).IsGeneralTargetAction)
            {
                ulong targetId = _ServerCharacter.TargetId.Value;
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out targetNetObj);
            }

            //sanity check that this is indeed a valid target.
            if (targetNetObj == null || !GameActionUtils.IsValidTarget(targetNetObj.NetworkObjectId))
            {
                return false;
            }

            if (targetNetObj.TryGetComponent<ServerCharacter>(out var serverCharacter))
            {
                //Skill1 may be contextually overridden if it was generated from a mouse-click.
                if (actionID == CharacterClass.Skill1.ActionID && triggerStyle == SkillTriggerStyle.MouseClick)
                {
                    if (!serverCharacter.IsNpc && serverCharacter.CharacterLifeState == CharacterLifeState.Fainted)
                    {
                        //right-clicked on a downed ally--change the skill play to Revive.
                        actionID = GameDataSource.Instance.ReviveActionPrototype.ActionID;
                    }
                }
            }

            Vector3 targetHitPoint;
            if (CharacterPhysicWrapper.TryGetPhysicsWrapper(targetNetObj.NetworkObjectId, out var movementContainer))
            {
                targetHitPoint = movementContainer.Transform.position;
            }
            else
            {
                targetHitPoint = targetNetObj.transform.position;
            }

            // record our target in case this action uses that info (non-targeted attacks will ignore this)
            resultData.ActionID = actionID;
            resultData.TargetIds = new ulong[] { targetNetObj.NetworkObjectId };
            PopulateSkillRequest(targetHitPoint, actionID, ref resultData);
            return true;
        }
        /// <summary>
        /// Populates the ActionRequestData with additional information. The TargetIds of the action should already be set before calling this.
        /// </summary>
        /// <param name="hitPoint">The point in world space where the click ray hit the target.</param>
        /// <param name="actionID">The action to perform (will be stamped on the resultData)</param>
        /// <param name="resultData">The ActionRequestData to be filled out with additional information.</param>
        void PopulateSkillRequest(Vector3 hitPoint, GameActionID actionID, ref ActionRequestData resultData)
        {
            resultData.ActionID = actionID;
            var actionConfig = GameDataSource.Instance.GetActionPrototypeByID(actionID).Config;
            //most skill types should implicitly close distance. The ones that don't are explicitly set to false in the following switch.
            resultData.ShouldClose = true;
            // figure out the Direction in case we want to send it
            Vector3 offset = hitPoint - _PhysicsWrapper.Transform.position;
            offset.y = 0;
            Vector3 direction = offset.normalized;

            switch (actionConfig.Logic)
            {
                //for projectile logic, infer the direction from the click position.
                case GameActionLogic.LaunchProjectile:
                    resultData.Direction = direction;
                    resultData.ShouldClose = false; //why? Because you could be lining up a shot, hoping to hit other people between you and your target. Moving you would be quite invasive.
                    return;
                case GameActionLogic.Target:
                    resultData.ShouldClose = false;
                    return;
                case GameActionLogic.Emote:
                    resultData.CancelMovement = true;
                    return;
                case GameActionLogic.RangedFXTargeted:
                    resultData.Position = hitPoint;
                    return;
                case GameActionLogic.PickUp:
                    resultData.CancelMovement = true;
                    resultData.ShouldQueue = false;
                    return;
            }
        }

        public void RequestAction(GameActionID actionID, SkillTriggerStyle triggerStyle, ulong targetId = 0)
        {
            Assert.IsNotNull(GameDataSource.Instance.GetActionPrototypeByID(actionID),
                $"Action with actionID {actionID} must be contained in the Action prototypes of GameDataSource!");

            if (_ActionRequestCount < m_ActionRequests.Length)
            {
                m_ActionRequests[_ActionRequestCount].RequestedActionID = actionID;
                m_ActionRequests[_ActionRequestCount].TriggerStyle = triggerStyle;
                m_ActionRequests[_ActionRequestCount].TargetId = targetId;
                _ActionRequestCount++;
            }
        }
        #endregion
    }
    public class ActionState
    {
        public GameActionID actionID { get; internal set; }

        public bool selectable { get; internal set; }

        internal void SetActionState(GameActionID newActionID, bool isSelectable = true)
        {
            actionID = newActionID;
            selectable = isSelectable;
        }
    }
}
