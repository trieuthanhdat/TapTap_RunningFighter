using System;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action.Input
{
    public abstract class BaseActionInput : MonoBehaviour
    {
        #region ____PROPERTIES____
        protected ServerCharacter m_PlayerOwner;
        protected Vector3 m_Origin;
        protected GameActionID m_ActionPrototypeID;
        protected Action<ActionRequestData> m_SendInput;
        #endregion

        #region ____EVENTS____
        System.Action m_OnFinished;
        #endregion

        public void Initiate(ServerCharacter playerOwner, Vector3 origin, GameActionID actionPrototypeID, Action<ActionRequestData> onSendInput, System.Action onFinished)
        {
            m_PlayerOwner = playerOwner;
            m_Origin = origin;
            m_ActionPrototypeID = actionPrototypeID;
            m_SendInput = onSendInput;
            m_OnFinished = onFinished;
        }

        public void OnDestroy()
        {
            m_OnFinished();
        }

        public virtual void OnReleaseKey() { }
    }
}
