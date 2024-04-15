using DG.Tweening;
using Project_RunningFighter.Data;
using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.Infrastructure;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    [RequireComponent(typeof(ServerCharacter))]
    public class ClientCharacterMovement : NetworkBehaviour
    {
        private ServerCharacter m_ServerCharacter;
        private Rigidbody m_rigidbody;

        private Vector3 m_PrevTargetPos;
        private Vector3 m_TargetPosition;

        private NetworkVariable<float> m_JumpTime = new NetworkVariable<float>(1f);
        private NetworkVariable<float> m_JumpPowerMultiplier = new NetworkVariable<float>(1f);
        private NetworkVariable<ForceMode> m_Forcemode = new NetworkVariable<ForceMode>(default);
        #region ____UNITY METHOD____
        private void Awake()
        {
            m_ServerCharacter = GetComponent<ServerCharacter>();
            m_rigidbody = GetComponent<Rigidbody>();
        }
        public override void OnNetworkSpawn()
        {
            m_PrevTargetPos = m_TargetPosition = default;
        }
        #endregion
        public void SetupJumpVariables(float m_JumpTime,float m_JumpPowerMultiplier, ForceMode m_Forcemode)
        {
            this.m_Forcemode.Value = m_Forcemode;
            this.m_JumpTime.Value = m_JumpTime;
            this.m_JumpPowerMultiplier.Value = m_JumpPowerMultiplier;
        }
        // Called on the client when setting the target position
        public void SetTargetPosition(Vector3 position)
        {
            m_TargetPosition = position;
        }
        
        public void MoveClientCharacterWithDOTween()
        {
            var speed = GetBaseMovementSpeed();
            var duration = GetBaseMovementDistance() / speed;
            transform.DOMove(m_TargetPosition, duration)
                     .OnUpdate(()   => { UpdatePositionClientCharacterRpc(transform.position); })
                     .OnComplete(() => { m_PrevTargetPos = transform.position; });
        }
       

        #region ____RPC METHOD____
        [Rpc(SendTo.ClientsAndHost)]
        public void JumpClientCharacterRpc(float _jumpDeltaTime, bool m_IsJumping)
        {
            if (m_rigidbody.velocity.y <= 0 && m_IsJumping)
            {
                float t = _jumpDeltaTime / m_JumpTime.Value;
                float currentJumpM = m_JumpPowerMultiplier.Value;

                if (t > 0.5f)
                {
                    currentJumpM = m_JumpPowerMultiplier.Value * (1 - t);
                }
                Vector3 jumpForce = transform.forward * GetBaseJumpForwardForce();
                jumpForce.y = GetBaseJumpUpForce();
                jumpForce *= currentJumpM;
                m_rigidbody.AddForce(jumpForce, m_Forcemode.Value);
                Debug.Log($"CLIENT CHARACTER MOVEMENT: Add force + {jumpForce} - _jumpDeltaTime {_jumpDeltaTime}");
            }
        }
        [Rpc(SendTo.ClientsAndHost)]
        public void MoveClientCharacteALongTargetPositionrRpc(float desiredMovementAmount)
        {
            Vector3 movingVector = transform.forward;
            var physicsTransform = m_ServerCharacter.physicsWrapper.Transform;
            Transform tmpPos = transform;
            tmpPos.position += movingVector * desiredMovementAmount;
            physicsTransform.SetPositionAndRotation(tmpPos.position, tmpPos.rotation);
            Debug.Log("CLIENT CHARACTER MOVEMENT: moving client char");
            //Smooth lerp Rotation
            float rotateSpeed = 10f;
            transform.forward = Vector3.Slerp(movingVector, Vector3.forward, Time.deltaTime * rotateSpeed);
            /*if (Vector3.Distance(transform.position, m_TargetPosition) <= 0.1f)
            {
                ResetMovementStatus();
                return;
            }*/
        }
        [Rpc(SendTo.ClientsAndHost)]
        private void UpdatePositionClientCharacterRpc(Vector3 position)
        {
            transform.position = position;
        }
        #endregion

        #region ____CHARACTER STATS____
        private float GetBaseMovementSpeed()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_ServerCharacter.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_ServerCharacter.CharacterType}");
            return characterClass.Speed;
        }
        //Retrieves the speed for this character's class.
        private float GetBaseMovementDistance()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_ServerCharacter.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_ServerCharacter.CharacterType}");
            return characterClass.MoveDistance;
        }

        private float GetBaseMovementCoolDown()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_ServerCharacter.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_ServerCharacter.CharacterType}");
            return characterClass.MoveCoolDown;
        }

        private float GetBaseJumpForwardForce()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_ServerCharacter.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_ServerCharacter.CharacterType}");
            return characterClass.JumpFwdForce;
        }
        private float GetBaseJumpUpForce()
        {
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_ServerCharacter.CharacterType];
            Debug.Log($"CHARACTER MOVEMENT: base jump up Force {characterClass.JumpUpForce}");
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_ServerCharacter.CharacterType}");
            return characterClass.JumpUpForce;
        }
        #endregion
    }

}
