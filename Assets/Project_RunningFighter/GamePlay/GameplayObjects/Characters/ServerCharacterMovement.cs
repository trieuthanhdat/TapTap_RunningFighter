using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    [Serializable]
    public enum MovementState
    {
        Idle,
        Moving,
        Knockback,
        Charging
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
        [SerializeField] private ServerCharacter m_CharLogic;
        [SerializeField] private Vector3 m_moveDirection = Vector3.forward;
        [SerializeField] Rigidbody m_rigidbody;

        #region ___PROPERTIES___
        public Vector3 MoveDirection => m_moveDirection;

        private MovementState  m_MovementState;
        private MovementStatus m_PreviousState;
        private Vector3 m_knockbackDirection;
        // when we are in charging and knockback mode, we use these additional variables
        private float m_ForcedSpeed;
        private float m_SpecialModeDurationRemaining;
        #endregion

        private void Awake()
        {
            // disable this NetworkBehavior until it is spawned
            if (m_rigidbody == null)
                m_rigidbody = GetComponent<Rigidbody>();
            enabled = false;

        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                enabled = true;
            }

        }
        private void FixedUpdate()
        {
            PerformMovement();

            var currentState = GetMovementStatus(m_MovementState);
            if (m_PreviousState != currentState)
            {
                m_CharLogic.MovementStatus.Value = currentState;
                m_PreviousState = currentState;
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
        private void PerformMovement()
        {
            if (m_MovementState == MovementState.Idle)
                return;
            Vector3 movementVector;

            if (m_MovementState == MovementState.Charging)
            {
                // if we're done charging, stop moving
                m_SpecialModeDurationRemaining -= Time.fixedDeltaTime;
                if (m_SpecialModeDurationRemaining <= 0)
                {
                    m_MovementState = MovementState.Idle;
                    return;
                }

                var desiredMovementAmount = m_ForcedSpeed * Time.fixedDeltaTime;
                movementVector = transform.forward * desiredMovementAmount;
            }
            else if (m_MovementState == MovementState.Knockback)
            {
                m_SpecialModeDurationRemaining -= Time.fixedDeltaTime;
                if (m_SpecialModeDurationRemaining <= 0)
                {
                    m_MovementState = MovementState.Idle;
                    return;
                }

                var desiredMovementAmount = m_ForcedSpeed * Time.fixedDeltaTime;
                movementVector = m_knockbackDirection * desiredMovementAmount;
            }
            else
            {
                var desiredMovementAmount = GetBaseMovementSpeed() * Time.fixedDeltaTime;
                movementVector   = transform.forward * desiredMovementAmount;
                m_rigidbody.MovePosition(movementVector);
                // If we didn't move stop moving.
                if (movementVector == Vector3.zero) //FIX HERE
                {
                    m_MovementState = MovementState.Idle;
                    return;
                }
            }

            transform.rotation = Quaternion.LookRotation(movementVector);

            // After moving adjust the position of the dynamic rigidbody.
            m_rigidbody.position = transform.position;
            m_rigidbody.rotation = transform.rotation;
        }


        //Retrieves the speed for this character's class.
        private float GetBaseMovementSpeed()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_CharLogic.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_CharLogic.CharacterType}");
            return characterClass.Speed;
        }
        //Retrieves the speed for this character's class.
        private float GetBaseMovementDistance()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_CharLogic.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_CharLogic.CharacterType}");
            return characterClass.MoveDistance;
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
            m_MovementState = MovementState.Moving;
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
    }

}
