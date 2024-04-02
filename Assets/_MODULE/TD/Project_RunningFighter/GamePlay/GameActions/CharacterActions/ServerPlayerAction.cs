using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using UnityEngine;
using UnityEngine.Pool;
using static Project_RunningFighter.Gameplay.Action.GameActionFactory;

namespace Project_RunningFighter.Gameplay.Action.CharacterActions
{
    public class ServerPlayerAction
    {
        #region ___PROPERTIES___
        private ServerCharacter         m_ServerCharacter;
        private ServerCharacterMovement m_Movement;
        private List<GameAction>        m_Queue;
        private List<GameAction>        m_NonBlockingActions;
        private Dictionary<GameActionID, float> m_LastUsedTimestamps;

        /// To prevent the action queue from growing without bound, we cap its play time to this number of seconds. We can only ever estimate
        /// the time-length of the queue, since actions are allowed to block indefinitely. But this is still a useful estimate that prevents
        /// us from piling up a large number of small actions.
        private const float k_MaxQueueTimeDepth = 1.6f;

        private ActionRequestData m_PendingSynthesizedAction = new ActionRequestData();
        private bool m_HasPendingSynthesizedAction;
        #endregion

        public ServerPlayerAction(ServerCharacter serverCharacter)
        {
            m_ServerCharacter = serverCharacter;
            m_Movement = serverCharacter.Movement;
            m_Queue = new List<GameAction>();
            m_NonBlockingActions = new List<GameAction>();
            m_LastUsedTimestamps = new Dictionary<GameActionID, float>();
        }

        public void PlayAction(ref ActionRequestData action)
        {
            if (!action.ShouldQueue && m_Queue.Count > 0 &&
                (m_Queue[0].Config.ActionInterruptible ||
                    m_Queue[0].Config.CanBeInterruptedBy(action.ActionID)))
            {
                ClearActions(false);
            }

            if (GetQueueTimeDepth() >= k_MaxQueueTimeDepth)
            {
                //the queue is too big (in execution seconds) to accommodate any more actions, so this action must be discarded.
                return;
            }

            var newAction = GameActionFactory.CreateActionFromData(ref action);
            m_Queue.Add(newAction);
            if (m_Queue.Count == 1) { StartAction(); }
        }

        public void ClearActions(bool cancelNonBlocking)
        {
            if (m_Queue.Count > 0)
            {
                // Since this action was canceled, we don't want the player to have to wait Description.ReuseTimeSeconds
                // to be able to start it again. It should be restartable immediately!
                m_LastUsedTimestamps.Remove(m_Queue[0].ActionID);
                m_Queue[0].Cancel(m_ServerCharacter);
            }

            //clear the action queue
            {
                var removedActions = ListPool<GameAction>.Get();

                foreach (var action in m_Queue)
                {
                    removedActions.Add(action);
                }

                m_Queue.Clear();

                foreach (var action in removedActions)
                {
                    TryReturnAction(action);
                }

                ListPool<GameAction>.Release(removedActions);
            }


            if (cancelNonBlocking)
            {
                var removedActions = ListPool<GameAction>.Get();

                foreach (var action in m_NonBlockingActions)
                {
                    action.Cancel(m_ServerCharacter);
                    removedActions.Add(action);
                }
                m_NonBlockingActions.Clear();

                foreach (var action in removedActions)
                {
                    TryReturnAction(action);
                }

                ListPool<GameAction>.Release(removedActions);
            }
        }
        /// If an Action is active, fills out 'data' param and returns true. If no Action is active, returns false.
        /// This only refers to the blocking action! (multiple non-blocking actions can be running in the background, and
        /// this will still return false).
        public bool GetActiveActionInfo(out ActionRequestData data)
        {
            if (m_Queue.Count > 0)
            {
                data = m_Queue[0].Data;
                return true;
            }
            else
            {
                data = new ActionRequestData();
                return false;
            }
        }

        /// Figures out if an action can be played now, or if it would automatically fail because it was
        /// used too recently. (Meaning that its ReuseTimeSeconds hasn't elapsed since the last use.)
        /// <param name="actionID">the action we want to run</param>
        /// <returns>true if the action can be run now, false if more time must elapse before this action can be run</returns>
        public bool IsReuseTimeElapsed(GameActionID actionID)
        {
            if (m_LastUsedTimestamps.TryGetValue(actionID, out float lastTimeUsed))
            {
                var abilityConfig = GameDataSource.Instance.GetActionPrototypeByID(actionID).Config;

                float reuseTime = abilityConfig.ReuseTimeSeconds;
                if (reuseTime > 0 && Time.time - lastTimeUsed < reuseTime)
                {
                    // still needs more time!
                    return false;
                }
            }
            return true;
        }

        /// Returns how many actions are actively running. This includes all non-blocking actions,
        /// and the one blocking action at the head of the queue (if present).
        public int RunningActionCount
        {
            get
            {
                return m_NonBlockingActions.Count + (m_Queue.Count > 0 ? 1 : 0);
            }
        }

        /// Starts the action at the head of the queue, if any.
        private void StartAction()
        {
            if (m_Queue.Count > 0)
            {
                float reuseTime = m_Queue[0].Config.ReuseTimeSeconds;
                if (reuseTime > 0
                    && m_LastUsedTimestamps.TryGetValue(m_Queue[0].ActionID, out float lastTimeUsed)
                    && Time.time - lastTimeUsed < reuseTime)
                {
                    // we've already started one of these too recently
                    AdvanceQueue(false); // note: this will call StartAction() recursively if there's more stuff in the queue ...
                    return;              // ... so it's important not to try to do anything more here
                }

                int index = SynthesizeTargetIfNecessary(0);
                SynthesizeChaseIfNecessary(index);

                m_Queue[0].TimeStarted = Time.time;
                bool play = m_Queue[0].OnStart(m_ServerCharacter);
                if (!play)
                {
                    //actions that exited out in the "Start" method will not have their End method called, by design.
                    AdvanceQueue(false); // note: this will call StartAction() recursively if there's more stuff in the queue ...
                    return;              // ... so it's important not to try to do anything more here
                }

                // if this Action is interruptible, that means movement should interrupt it... character needs to be stationary for this!
                // So stop any movement that's already happening before we begin
                if (m_Queue[0].Config.ActionInterruptible && !m_Movement.IsPerformingForcedMovement())
                {
                    m_Movement.CancelMove();
                }

                // remember the moment when we successfully used this Action!
                m_LastUsedTimestamps[m_Queue[0].ActionID] = Time.time;

                if (m_Queue[0].Config.ExecTimeSeconds == 0 && m_Queue[0].Config.BlockingMode == BlockingModeType.OnlyDuringExecTime)
                {
                    //this is a non-blocking action with no exec time. It should never be hanging out at the front of the queue (not even for a frame),
                    //because it could get cleared if a new Action came in in that interval.
                    m_NonBlockingActions.Add(m_Queue[0]);
                    AdvanceQueue(false); // note: this will call StartAction() recursively if there's more stuff in the queue ...
                    return;              // ... so it's important not to try to do anything more here
                }
            }
        }

        /// Synthesizes a Chase Action for the action at the Head of the queue, if necessary (the base action must have a target,
        /// and must have the ShouldClose flag set). This method must not be called when the queue is empty.
        /// <returns>The new index of the Action being operated on.</returns>
        private int SynthesizeChaseIfNecessary(int baseIndex)
        {
            GameAction baseAction = m_Queue[baseIndex];

            if (baseAction.Data.ShouldClose && baseAction.Data.TargetIds != null)
            {
                ActionRequestData data = new ActionRequestData
                {
                    ActionID = GameDataSource.Instance.GeneralChaseActionPrototype.ActionID,
                    TargetIds = baseAction.Data.TargetIds,
                    Amount = baseAction.Config.Range
                };
                baseAction.Data.ShouldClose = false; //you only get to do this once!
                GameAction chaseAction = GameActionFactory.CreateActionFromData(ref data);
                m_Queue.Insert(baseIndex, chaseAction);
                return baseIndex + 1;
            }
            return baseIndex;
        }

        /// Targeted skills should implicitly set the active target of the character, if not already set.
        /// <param name="baseIndex">The new index of the base action in m_Queue</param>
        /// <returns></returns>
        private int SynthesizeTargetIfNecessary(int baseIndex)
        {
            GameAction baseAction = m_Queue[baseIndex];
            var targets = baseAction.Data.TargetIds;

            if (targets != null &&
                targets.Length == 1 &&
                targets[0] != m_ServerCharacter.TargetId.Value)
            {
                //if this is a targeted skill (with a single requested target), and it is different from our
                //active target, then we synthesize a TargetAction to change  our target over.

                ActionRequestData data = new ActionRequestData
                {
                    ActionID = GameDataSource.Instance.GeneralTargetActionPrototype.ActionID,
                    TargetIds = baseAction.Data.TargetIds
                };

                //this shouldn't run redundantly, because the next time the base Action comes up to play, its Target
                //and the active target in our NetState should match.
                GameAction targetAction = GameActionFactory.CreateActionFromData(ref data);
                m_Queue.Insert(baseIndex, targetAction);
                return baseIndex + 1;
            }

            return baseIndex;
        }

        /// <summary>
        /// Optionally end the currently playing action, and advance to the next Action that wants to play.
        /// </summary>
        /// <param name="endRemoved">if true we call End on the removed element.</param>
        private void AdvanceQueue(bool endRemoved)
        {
            if (m_Queue.Count > 0)
            {
                if (endRemoved)
                {
                    m_Queue[0].End(m_ServerCharacter);
                    if (m_Queue[0].ChainIntoNewAction(ref m_PendingSynthesizedAction))
                    {
                        m_HasPendingSynthesizedAction = true;
                    }
                }
                var action = m_Queue[0];
                m_Queue.RemoveAt(0);
                TryReturnAction(action);
            }

            // now start the new Action! ... unless we now have a pending Action that will supercede it
            if (!m_HasPendingSynthesizedAction || m_PendingSynthesizedAction.ShouldQueue)
            {
                StartAction();
            }
        }

        private void TryReturnAction(GameAction action)
        {
            if (m_Queue.Contains(action))
            {
                return;
            }

            if (m_NonBlockingActions.Contains(action))
            {
                return;
            }

            GameActionFactory.ReturnAction(action);
        }

        public void OnUpdate()
        {
            if (m_HasPendingSynthesizedAction)
            {
                m_HasPendingSynthesizedAction = false;
                PlayAction(ref m_PendingSynthesizedAction);
            }

            if (m_Queue.Count > 0 && m_Queue[0].ShouldBecomeNonBlocking())
            {
                // the active action is no longer blocking, meaning it should be moved out of the blocking queue and into the
                // non-blocking one. (We use this for e.g. projectile attacks, so the projectiles can keep flying, but
                // the player can enqueue other actions in the meantime.)
                m_NonBlockingActions.Add(m_Queue[0]);
                AdvanceQueue(false);
            }

            // if there's a blocking action, update it
            if (m_Queue.Count > 0)
            {
                if (!UpdateAction(m_Queue[0]))
                {
                    AdvanceQueue(true);
                }
            }

            // if there's non-blocking actions, update them! We do this in reverse-order so we can easily remove expired actions.
            for (int i = m_NonBlockingActions.Count - 1; i >= 0; --i)
            {
                GameAction runningAction = m_NonBlockingActions[i];
                if (!UpdateAction(runningAction))
                {
                    // it's dead!
                    runningAction.End(m_ServerCharacter);
                    m_NonBlockingActions.RemoveAt(i);
                    TryReturnAction(runningAction);
                }
            }
        }

        /// Calls a given Action's Update() and decides if the action is still alive.
        /// <returns>true if the action is still active, false if it's dead</returns>
        private bool UpdateAction(GameAction action)
        {
            bool keepGoing = action.OnUpdate(m_ServerCharacter);
            bool expirable = action.Config.DurationSeconds > 0f; //non-positive value is a sentinel indicating the duration is indefinite.
            var timeElapsed = Time.time - action.TimeStarted;
            bool timeExpired = expirable && timeElapsed >= action.Config.DurationSeconds;
            return keepGoing && !timeExpired;
        }

        /// How much time will it take all remaining Actions in the queue to play out? This sums up all the time each Action is blocking,
        /// which is different from each Action's duration. Note that this is an ESTIMATE. An action may block the queue indefinitely if it wishes.
        /// <returns>The total "time depth" of the queue, or how long it would take to play in seconds, if no more actions were added. </returns>
        private float GetQueueTimeDepth()
        {
            if (m_Queue.Count == 0) { return 0; }

            float totalTime = 0;
            foreach (var action in m_Queue)
            {
                var info = action.Config;
                float actionTime = info.BlockingMode == BlockingModeType.OnlyDuringExecTime ? info.ExecTimeSeconds :
                                   info.BlockingMode == BlockingModeType.EntireDuration ? info.DurationSeconds :
                                   throw new System.Exception($"Unrecognized blocking mode: {info.BlockingMode}");
                totalTime += actionTime;
            }

            return totalTime - m_Queue[0].TimeRunning;
        }

        public void CollisionEntered(Collision collision)
        {
            if (m_Queue.Count > 0)
            {
                m_Queue[0].CollisionEntered(m_ServerCharacter, collision);
            }
        }

        /// Gives all active Actions a chance to alter a gameplay variable.
        /// Note that this handles both positive alterations (commonly called "buffs")
        /// AND negative ones ("debuffs").
        /// <param name="buffType">Which gameplay variable is being calculated</param>
        /// <returns>The final ("buffed") value of the variable</returns>
        public float GetBuffedValue(GameAction.BuffableValue buffType)
        {
            float buffedValue = GameAction.GetUnbuffedValue(buffType);
            if (m_Queue.Count > 0)
            {
                m_Queue[0].BuffValue(buffType, ref buffedValue);
            }
            foreach (var action in m_NonBlockingActions)
            {
                action.BuffValue(buffType, ref buffedValue);
            }
            return buffedValue;
        }

        /// Tells all active Actions that a particular gameplay event happened, such as being hit,
        /// getting healed, dying, etc. Actions can change their behavior as a result.
        /// <param name="activityThatOccurred">The type of event that has occurred</param>
        public virtual void OnGameplayActivity(GameAction.GameplayActivity activityThatOccurred)
        {
            if (m_Queue.Count > 0)
            {
                m_Queue[0].OnGameplayActivity(m_ServerCharacter, activityThatOccurred);
            }
            foreach (var action in m_NonBlockingActions)
            {
                action.OnGameplayActivity(m_ServerCharacter, activityThatOccurred);
            }
        }


        /// Cancels the first instance of the given ActionLogic that is currently running, or all instances if cancelAll is set to true.
        /// Searches actively running actions first, then looks at the head action in the queue.
        /// <param name="logic">The ActionLogic to cancel</param>
        /// <param name="cancelAll">If true will cancel all instances; if false will just cancel the first running instance.</param>
        /// <param name="exceptThis">If set, will skip this action (useful for actions canceling other instances of themselves).</param>
        public void CancelRunningActionsByLogic(GameActionLogic logic, bool cancelAll, GameAction exceptThis = null)
        {
            for (int i = m_NonBlockingActions.Count - 1; i >= 0; --i)
            {
                var action = m_NonBlockingActions[i];
                if (action.Config.Logic == logic && action != exceptThis)
                {
                    action.Cancel(m_ServerCharacter);
                    m_NonBlockingActions.RemoveAt(i);
                    TryReturnAction(action);
                    if (!cancelAll) { return; }
                }
            }

            if (m_Queue.Count > 0)
            {
                var action = m_Queue[0];
                if (action.Config.Logic == logic && action != exceptThis)
                {
                    action.Cancel(m_ServerCharacter);
                    m_Queue.RemoveAt(0);
                    TryReturnAction(action);
                }
            }
        }
    }
}
