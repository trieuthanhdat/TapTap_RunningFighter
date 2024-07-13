using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Utilities
{
    public class ActionFactory<T>
    {
        private Queue<Action<T>> m_ActionQueue;
        private Stack<Action<T>> m_ActionStack;
        private Action<T> m_CurrentAction;
        private T m_CurrentParameter;

        //This table helps track the important Action
        private Dictionary<int, Action<T>> m_TrackedActionTable;

        public void ExecuteTrackedActions()
        {
            var listExecutedActionKey = new List<int>();
            foreach (var kvp in m_TrackedActionTable)
            {
                kvp.Value?.Invoke(m_CurrentParameter);
                listExecutedActionKey.Add(kvp.Key);
            }
            foreach(var key in listExecutedActionKey)
            {
                RemoveExecutedTrackedAction(key);
                Debug.Log($"ACTION FACTORY: Execute Tracked Action! {key} - Number of Action left: {m_TrackedActionTable.Count}");
            }
        }

        public ActionFactory()
        {
            m_ActionQueue = new Queue<Action<T>>();
            m_ActionStack = new Stack<Action<T>>();
            m_TrackedActionTable = new Dictionary<int, Action<T>>();
        }

        public void Queue(Action<T> action, bool shouldTrack = false)
        {
            m_ActionQueue.Enqueue(action);
            if(shouldTrack)
            {
                var key = m_ActionQueue.Count - 1;
                m_TrackedActionTable.TryAdd(key, action);
            }
        }

        public void Stack(Action<T> action, bool shouldTrack = false)
        {
            m_ActionStack.Push(action);
            if (shouldTrack)
            {
                var key = m_ActionQueue.Count - 1;
                m_TrackedActionTable.TryAdd(key, action);
            }
        }

        public void StartExecution(T parameter = default)
        {
            m_CurrentParameter = parameter;
            ExecuteNextAction();
        }

        private void ExecuteNextAction()
        {
            if (m_CurrentAction != null)
            {
                // If there's a current action, wait for interaction before executing the next action
                return;
            }

            if (m_ActionQueue != null && m_ActionQueue.Count > 0)
            {
                m_CurrentAction = m_ActionQueue.Dequeue();
                RemoveExecutedTrackedAction(m_CurrentAction);
            }
            else if (m_ActionStack != null && m_ActionStack.Count > 0)
            {
                m_CurrentAction = m_ActionStack.Pop();
                RemoveExecutedTrackedAction(m_CurrentAction);
            }
            else
            {
                // No more actions to execute
                return;
            }
            // Execute the current action
            m_CurrentAction?.Invoke(m_CurrentParameter);
            
        }
        private void RemoveExecutedTrackedAction(int key)
        {
            if (m_TrackedActionTable.ContainsKey(key))
            {
                m_TrackedActionTable.Remove(key);
            }
            return;
        }
        private void RemoveExecutedTrackedAction(Action<T> action)
        {
            if (action == null) return;

            foreach (var kvp in m_TrackedActionTable)
            {
                if (kvp.Value == action)
                {
                    m_TrackedActionTable.Remove(kvp.Key);
                    Debug.Log("ACTION FACTORY: remove tracked action "+ kvp.Key);
                    break;
                }
            }
        }
        public bool IsAllTrackedActionCompleted()
        {
            return m_TrackedActionTable == null || m_TrackedActionTable.Count == 0;
        }
        public bool IsAllQueuedActionCompleted()
        {
            return m_CurrentAction == null || m_ActionQueue.Count == 0;
        }
        public bool IsAllStackedActionCompleted()
        {
            return m_CurrentAction == null || m_ActionStack.Count == 0;
        }
        public void CompleteAction()
        {
            // Interaction completed, proceed to the next action
            m_CurrentAction = null;
            ExecuteNextAction();
        }
        public void Reset()
        {
            if (m_ActionQueue != null)
            {
                m_ActionQueue.Clear();
                m_ActionQueue = null;
            }
            if (m_ActionStack != null)
            {
                m_ActionStack.Clear();
                m_ActionStack = null;
            }
        }
    }

}
