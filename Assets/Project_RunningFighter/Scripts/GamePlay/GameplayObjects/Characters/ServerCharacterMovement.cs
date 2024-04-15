using System;
using System.Collections;
using DG.Tweening;
using Project_RunningFighter.Data;
using Project_RunningFighter.Gameplay.GameStates;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    [Serializable]
    public enum MovementState
    {
        Idle,
        Moving,
        Knockback,
        Charging,
        Jumping
    }
    [Serializable]
    public enum MovementStatus
    {
        Idle,
        Normal,
        Uncontrolled,
        Hasted,
        Slowed,
        Walking,
        Running
    }
    public class ServerCharacterMovement : NetworkBehaviour
    {
        [SerializeField] private ClientCharacterMovement m_ClientMovement;
        [SerializeField] private ServerCharacter m_CharLogic;
        [SerializeField] private Vector3 m_moveDirection = Vector3.forward;
        [SerializeField] Rigidbody m_rigidbody;
        [SerializeField] float m_FallMultiplier = 1f;
        [SerializeField] float m_JumpPowerMultiplier = 1f;
        [SerializeField] float m_JumpTime = 1.2f;
        [SerializeField] ForceMode m_Forcemode = ForceMode.Impulse;
        [SerializeField] private LayerMask m_CollisionLayerMask;
        [SerializeField] float m_CapsuleHeight = 1f;
        [SerializeField] float m_CapsuleRadius = 0.07f;

        #region ___PROPERTIES___
        private NetworkVariable<bool> m_IsGrounded  = new NetworkVariable<bool>(true);
        private NetworkVariable<bool> m_WasGrounded = new NetworkVariable<bool>(true);
        [SerializeField]
        private MovementState  m_MovementState;
        private MovementStatus m_PreviousState;
        private Vector3 m_VectorGravity;
        private Vector3 m_TargetPosition;
        private Vector3 m_knockbackDirection;
        // when we are in charging and knockback mode, we use these additional variables
        float _jumpDeltaTime = 0;
        private float m_ForcedSpeed;
        private float m_SpecialModeDurationRemaining;
        private NetworkVariable<bool> m_IsJumping = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> IsJumping => m_IsJumping;

        [SerializeField]
        private NetworkVariable<bool> m_CanMoveByTouchScreen = new NetworkVariable<bool>(true);
        public NetworkVariable<bool> CanMoveByTouchScreen => m_CanMoveByTouchScreen;

        public bool IsWining 
        {
            get
            {
                bool hasWon = ServerActionPhaseState.Instance.WinningPlayer == OwnerClientId;
                Debug.Log($"SERVER CHARACTER MOVEMENT: has won ! {hasWon}");
                return hasWon;
            }
        }

        #endregion

        private void Awake()
        {
            // disable this NetworkBehavior until it is spawned
            if (m_rigidbody == null)
                m_rigidbody = GetComponent<Rigidbody>();
            enabled = false;

        }
        private void Start()
        {
            m_VectorGravity = new Vector3(0, Physics.gravity.y, 0);
        }
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                enabled = true;
                m_ClientMovement.SetupJumpVariables(m_JumpTime, m_JumpPowerMultiplier, m_Forcemode);
            }

        }

        
        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                // Disable server components when despawning
                enabled = false;
            }
        }
        private void Update()
        {
            //if (IsWining) return;
            m_IsGrounded.Value = IsGrounded();
            
            PerformMovement();
            m_WasGrounded = m_IsGrounded;
            var currentState = GetMovementStatus(m_MovementState);
            if (m_PreviousState != currentState)
            {
                m_CharLogic.MovementStatus.Value = currentState;
                m_PreviousState = currentState;
            }
        }

        public void FollowTransform(Transform followTransform)
        {
            //todo
        }
        private bool m_IsMoving;
        
        private void PerformMovement()
        {
            if (!IsServer) return;
            //if (IsWining) return;
            if (m_MovementState == MovementState.Idle)
                return;
            Vector3 movingVector = Vector3.zero;

            if (m_MovementState == MovementState.Charging || m_MovementState == MovementState.Knockback)
            {
               //todo
            }
            else if(m_MovementState == MovementState.Moving) //MOVE STATE
            {
                var speed = GetBaseMovementSpeed();
                movingVector = transform.forward;
                var desiredMovementAmount = speed * Time.deltaTime;
                m_ClientMovement.MoveClientCharacteALongTargetPositionrRpc(desiredMovementAmount);
                if (Vector3.Distance(transform.position, m_TargetPosition) <= 0.1f)
                {
                    ResetMovementState();
                    return;
                }
                
                /*m_CanMoveByTouchScreen.Value = false;
                
                var physicsTransform = m_CharLogic.physicsWrapper.Transform;
                var speed = GetBaseMovementSpeed();
                movingVector = transform.forward;
                var desiredMovementAmount = speed * Time.deltaTime;
                Transform tmpPos = transform;
                tmpPos.position += movingVector * desiredMovementAmount;
                physicsTransform.SetPositionAndRotation(tmpPos.position, tmpPos.rotation);

                //Smooth lerp Rotation
                float rotateSpeed = 10f;
                transform.forward = Vector3.Slerp(movingVector, Vector3.forward, Time.deltaTime * rotateSpeed);
                if (Vector3.Distance(transform.position, m_TargetPosition) <= 0.1f)
                {
                    ResetMovementState();
                    return;
                }*/
            }
            else //JUMP STATE
            {
                _jumpDeltaTime += Time.deltaTime;
                if (_jumpDeltaTime > m_JumpTime)
                {
                    m_IsJumping.Value = false;
                    _jumpDeltaTime = 0;
                    ResetMovementState();
                    return;
                }
                m_ClientMovement.JumpClientCharacterRpc(_jumpDeltaTime, m_IsJumping.Value);
            }

            transform.rotation = Quaternion.LookRotation(movingVector.normalized);
            // Update the position of the dynamic rigidbody
            m_rigidbody.position = transform.position;
            m_rigidbody.rotation = transform.rotation;
        }
        private void ResetMovementState()
        {
            m_MovementState = MovementState.Idle;
            m_CanMoveByTouchScreen.Value = true;
        }
        private bool IsGrounded()
        {
            Vector3 startPoint = transform.position;
            bool isGrounded = Physics.CheckSphere(startPoint, m_CapsuleRadius, m_CollisionLayerMask);
            Debug.DrawLine(startPoint, startPoint - Vector3.up * m_CapsuleRadius, isGrounded ? Color.green : Color.red);
            Debug.Log($"PLAYER : {name} is grounded {isGrounded}");
            return isGrounded;
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw a sphere at the ground check position
            Gizmos.color = Color.yellow;
            Vector3 startPoint = transform.position;
            Gizmos.DrawWireSphere(startPoint, m_CapsuleRadius);
        }
#endif
        //Retrieves the speed for this character's class.
        private float GetBaseMovementSpeed()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_CharLogic.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_CharLogic.CharacterType}");
            return characterClass.Speed;
        }
        
        private float GetBaseJumpForwardForce()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_CharLogic.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_CharLogic.CharacterType}");
            return characterClass.JumpFwdForce;
        }
        private float GetBaseJumpUpForce()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_CharLogic.CharacterType];
            Debug.Log($"CHARACTER MOVEMENT: base jump up Force {characterClass.JumpUpForce}");
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_CharLogic.CharacterType}");
            return characterClass.JumpUpForce;
        }
        //Retrieves the speed for this character's class.
        private float GetBaseMovementDistance()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_CharLogic.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_CharLogic.CharacterType}");
            return characterClass.MoveDistance;
        }

        private float GetBaseMovementCoolDown()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_CharLogic.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_CharLogic.CharacterType}");
            return characterClass.MoveCoolDown;
        }


        #region ___MOVEMENT METHODS___
        //Determines the appropriate MovementStatus for the character. The
        //MovementStatus is used by the client code when animating the character.
        private MovementStatus GetMovementStatus(MovementState movementState)
        {
            switch (movementState)
            {
                case MovementState.Idle:
                    return MovementStatus.Idle;
                case MovementState.Knockback:
                    return MovementStatus.Uncontrolled;
                default:
                    return MovementStatus.Normal;
            }
        }
        
        public void Teleport(Vector3 newPosition)
        {
            CancelMove();

            m_rigidbody.position = transform.position;
            m_rigidbody.rotation = transform.rotation;
        }

        public void StartForwardCharge(float speed, float duration)
        {
            m_MovementState = MovementState.Charging;
            m_ForcedSpeed = speed;
            m_SpecialModeDurationRemaining = duration;
        }

        public void StartKnockback(Vector3 knocker, float speed, float duration)
        {
            m_MovementState = MovementState.Knockback;
            m_knockbackDirection = transform.position - knocker;
            m_ForcedSpeed = speed;
            m_SpecialModeDurationRemaining = duration;
        }
        public void SetMovementTarget(Vector3 position)
        {
            m_MovementState  = MovementState.Moving;
            //m_NavPath.SetTargetPosition(position);
        }
        public void SetJumpState()
        {
            if(m_IsGrounded.Value)
            {
                m_MovementState = MovementState.Jumping;
                m_IsJumping.Value = true;
                _jumpDeltaTime = 0;
            }
        }
        public void SetNextMovementTarget()
        {
            m_CanMoveByTouchScreen.Value = false;
            m_MovementState = MovementState.Moving;
            Vector3 nextPosition = transform.position + transform.forward * GetBaseMovementDistance();
            m_TargetPosition = nextPosition;
            if (m_ClientMovement)
            {
                m_ClientMovement.SetTargetPosition(m_TargetPosition);
            }
            Debug.Log("SERVER CHARACTER MOVEMENT: next target pos "+ m_TargetPosition +" on player "+name);
            //m_NavPath.SetTargetPosition(position);
        }

        public bool IsPerformingForcedMovement()
        {
            return m_MovementState == MovementState.Knockback ||
                   m_MovementState == MovementState.Charging;
        }

        public bool IsMoving()
        {
            return m_MovementState != MovementState.Idle;
        }

        public void CancelMove()
        {
            m_MovementState = MovementState.Idle;
        }


        #endregion

        #region ____EVENT METHODS____
       
        #endregion
    }

}
