using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Project_RunningFighter.Gameplay.Action
{
    public static class GameActionFactory
    {

        [Serializable]
        public enum GameActionLogic
        {
            RangedTargeted,
            Revive,
            LaunchProjectile,
            Emote,
            RangedFXTargeted,
            AoE,
            Trample,
            Stunned,
            Target,
            ChargedLaunchProjectile,
            PickUp,
            Drop
        }
        [Serializable]
        public enum BlockingModeType
        {
            EntireDuration,
            OnlyDuringExecTime,
        }

        private static Dictionary<GameActionID, ObjectPool<GameAction>> s_ActionPools = new Dictionary<GameActionID, ObjectPool<GameAction>>();

        private static ObjectPool<GameAction> GetActionPool(GameActionID actionID)
        {
            if (!s_ActionPools.TryGetValue(actionID, out var actionPool))
            {
                actionPool = new ObjectPool<GameAction>(
                    createFunc: () => UnityEngine.Object.Instantiate(GameDataSource.Instance.GetActionPrototypeByID(actionID)),
                    actionOnRelease: action => action.Reset(),
                    actionOnDestroy: UnityEngine.Object.Destroy);

                s_ActionPools.Add(actionID, actionPool);
            }

            return actionPool;
        }
        /// Factory method that creates Actions from their request data.
        /// <param name="data">the data to instantiate this skill from. </param>
        /// <returns>the newly created action. </returns>
        public static GameAction CreateActionFromData(ref ActionRequestData data)
        {
            var ret = GetActionPool(data.ActionID).Get();
            ret.Initialize(ref data);
            return ret;
        }

        public static void ReturnAction(GameAction action)
        {
            var pool = GetActionPool(action.ActionID);
            pool.Release(action);
        }

        public static void PurgePooledActions()
        {
            foreach (var actionPool in s_ActionPools.Values)
            {
                actionPool.Clear();
            }
        }
    }
}
