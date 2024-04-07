using System;
using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.VFX;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static Project_RunningFighter.Gameplay.Action.GameActionFactory;

namespace Project_RunningFighter.Gameplay.Action
{
    public abstract class GameAction : ScriptableObject
    {
        //An index into the GameDataSource array of action prototypes. Set at runtime by GameDataSource class.  If action is not itself a prototype - will contain the action id of the prototype reference.
        //This field is used to identify actions in a way that can be sent over the network.
        [NonSerialized]
        public GameActionID ActionID;

        //The default hit react animation; several different ActionFXs make use of this.
        public const string _DefaultHitReact = "HitReact1";
        protected ActionRequestData m_Data;
        //Time when this Action was started (from Time.time) in seconds. Set by the ActionPlayer or ActionVisualization.
        public float TimeStarted { get; set; }
        //How long the Action has been running (since its Start was called)--in seconds, measured via Time.time.
        public float TimeRunning { get { return (Time.time - TimeStarted); } }
        public ref ActionRequestData Data => ref m_Data;
        public GameActionConfig Config;

        public bool IsChaseAction => ActionID == GameDataSource.Instance.GeneralChaseActionPrototype.ActionID;
        public bool IsStunAction => ActionID == GameDataSource.Instance.StunnedActionPrototype.ActionID;
        public bool IsGeneralTargetAction => ActionID == GameDataSource.Instance.GeneralTargetActionPrototype.ActionID;

        public void Initialize(ref ActionRequestData data)
        {
            m_Data = data;
            ActionID = data.ActionID;
        }

        public virtual void Reset()
        {
            m_Data = default;
            ActionID = default;
            TimeStarted = 0;
        }

        //Called when the Action starts actually playing (which may be after it is created, because of queueing).
        //<returns>false if the action decided it doesn't want to run after all, true otherwise. </returns>
        public abstract bool OnStart(ServerCharacter serverCharacter);


        //Called each frame while the action is running.
        //<returns>true to keep running, false to stop. The Action will stop by default when its duration expires, if it has a duration set. </returns>
        public abstract bool OnUpdate(ServerCharacter clientCharacter);

        
        //Called each frame (before OnUpdate()) for the active ("blocking") Action, asking if it should become a background Action.
        
        //<returns>true to become a non-blocking Action, false to remain a blocking Action</returns>
        public virtual bool ShouldBecomeNonBlocking()
        {
            return Config.BlockingMode == BlockingModeType.OnlyDuringExecTime ? TimeRunning >= Config.ExecTimeSeconds : false;
        }

        //Called when the Action ends naturally. By default just calls Cancel()
        public virtual void End(ServerCharacter serverCharacter)
        {
            Cancel(serverCharacter);
        }

        //This will get called when the Action gets canceled. The Action should clean up any ongoing effects at this point.
        //(e.g. an Action that involves moving should cancel the current active move).
        public virtual void Cancel(ServerCharacter serverCharacter) { }
        
        //Called *AFTER* End(). At this point, the Action has ended, meaning its Update() etc. functions will never be
        //called again. If the Action wants to immediately segue into a different Action, it can do so here. The new
        //Action will take effect in the next Update().
        ///
        //Note that this is not called on prematurely cancelled Actions, only on ones that have their End() called.
        //<param name="newAction">the new Action to immediately transition to</param>
        //<returns>true if there's a new action, false otherwise</returns>
        public virtual bool ChainIntoNewAction(ref ActionRequestData newAction) { return false; }
        //Called on the active ("blocking") Action when this character collides with another.
        //<param name="serverCharacter"></param>
        //<param name="collision"></param>
        public virtual void CollisionEntered(ServerCharacter serverCharacter, Collision collision) { }

        public enum BuffableValue
        {
            PercentHealingReceived, // unbuffed value is 1.0. Reducing to 0 would mean "no healing". 2 would mean "double healing"
            PercentDamageReceived,  // unbuffed value is 1.0. Reducing to 0 would mean "no damage". 2 would mean "double damage"
            ChanceToStunTramplers,  // unbuffed value is 0. If > 0, is the chance that someone trampling this character becomes stunned
            PercentManaRecovered, // unbuffed value is 1.0. Reducing to 0 would mean "no healing". 2 would mean "double healing"
            PercentManaDepleted
        }

        
        //Called on all active Actions to give them a chance to alter the outcome of a gameplay calculation. Note
        //that this is used for both "buffs" (positive gameplay benefits) and "debuffs" (gameplay penalties).
        //In a more complex game with lots of buffs and debuffs, this function might be replaced by a separate
        //BuffRegistry component. This would let you add fancier features, such as defining which effects
        //"stack" with other ones, and could provide a UI that lists which are affecting each character
        //and for how long.
        //<param name="buffType">Which gameplay variable being calculated</param>
        //<param name="orgValue">The original ("un-buffed") value</param>
        //<param name="buffedValue">The final ("buffed") value</param>
        public virtual void BuffValue(BuffableValue buffType, ref float buffedValue) { }

        
        //Static utility function that returns the default ("un-buffed") value for a BuffableValue.
        //(This just ensures that there's one place for all these constants.)
        public static float GetUnbuffedValue(GameAction.BuffableValue buffType)
        {
            switch (buffType)
            {
                case BuffableValue.PercentDamageReceived: return 1;
                case BuffableValue.PercentHealingReceived: return 1;
                case BuffableValue.ChanceToStunTramplers: return 0;
                default: throw new System.Exception($"Unknown buff type {buffType}");
            }
        }

        public enum GameplayActivity
        {
            AttackedByEnemy,
            Healed,
            StoppedChargingUp,
            UsingAttackAction,
            RechargedMana,
            ConsumeMana
        }

        //Called on active Actions to let them know when a notable gameplay event happens.
        //When a GameplayActivity of AttackedByEnemy or Healed happens, OnGameplayAction() is called BEFORE BuffValue() is called.
        //<param name="serverCharacter"></param>
        //<param name="activityType"></param>
        public virtual void OnGameplayActivity(ServerCharacter serverCharacter, GameplayActivity activityType) { }



        //True if this actionFX began running immediately, prior to getting a confirmation from the server.
        public bool AnticipatedClient { get; protected set; }

        //Starts the ActionFX. Derived classes may return false if they wish to end immediately without their Update being called.
        //Derived class should be sure to call base.OnStart() in their implementation, but note that this resets "Anticipated" to false.
        //<returns>true to play, false to be immediately cleaned up.</returns>
        public virtual bool OnStartClient(ClientCharacter clientCharacter)
        {
            AnticipatedClient = false; //once you start for real you are no longer an anticipated action.
            TimeStarted = UnityEngine.Time.time;
            return true;
        }

        public virtual bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            return ActionConclusion.Continue;
        }
        //End is always called when the ActionFX finishes playing. This is a good place for derived classes to put
        //wrap-up logic (perhaps playing the "puff of smoke" that rises when a persistent fire AOE goes away). Derived
        //classes should aren't required to call base.End(); by default, the method just calls 'Cancel', to handle the
        //common case where Cancel and End do the same thing.
        public virtual void EndClient(ClientCharacter clientCharacter)
        {
            CancelClient(clientCharacter);
        }

        //Cancel is called when an ActionFX is interrupted prematurely. It is kept logically distinct from End to allow
        //for the possibility that an Action might want to play something different if it is interrupted, rather than
        //completing. For example, a "ChargeShot" action might want to emit a projectile object in its End method, but
        //instead play a "Stagger" animation in its Cancel method.
        public virtual void CancelClient(ClientCharacter clientCharacter) { }

        //Should this ActionFX be created anticipatively on the owning client?
        //<param name="clientCharacter">The ActionVisualization that would be playing this ActionFX.</param>
        //<param name="data">The request being sent to the server</param>
        //<returns>If true ActionVisualization should pre-emptively create the ActionFX on the owning client, before hearing back from the server.</returns>
        public static bool ShouldClientAnticipate(ClientCharacter clientCharacter, ref ActionRequestData data)
        {
            if (!clientCharacter.CanPerformActions) { return false; }

            var actionDescription = GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).Config;

            //for actions with ShouldClose set, we check our range locally. If we are out of range, we shouldn't anticipate, as we will
            //need to execute a ChaseAction (synthesized on the server) prior to actually playing the skill.
            bool isTargetEligible = true;
            if (data.ShouldClose == true)
            {
                ulong targetId = (data.TargetIds != null && data.TargetIds.Length > 0) ? data.TargetIds[0] : 0;
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject networkObject))
                {
                    float rangeSquared = actionDescription.Range * actionDescription.Range;
                    isTargetEligible = (networkObject.transform.position - clientCharacter.transform.position).sqrMagnitude < rangeSquared;
                }
            }

            //at present all Actionts anticipate except for the Target action, which runs a single instance on the client and is
            //responsible for action anticipation on its own.
            return isTargetEligible && actionDescription.Logic != GameActionLogic.Target;
        }

        //Called when the visualization receives an animation event.
        public virtual void OnAnimEventClient(ClientCharacter clientCharacter, string id) { }

        //Called when this action has finished "charging up". (Which is only meaningful for a
        //few types of actions -- it is not called for other actions.)
        public virtual void OnStoppedChargingUpClient(ClientCharacter clientCharacter, float finalChargeUpPercentage) { }

        //Utility function that instantiates all the graphics in the Spawns list.
        //If parentToOrigin is true, the new graphics are parented to the origin Transform.
        //If false, they are positioned/oriented the same way but are not parented.
        protected List<SpecialFXGraphic> InstantiateSpecialFXGraphics(Transform origin, bool parentToOrigin)
        {
            var returnList = new List<SpecialFXGraphic>();
            foreach (var prefab in Config.Spawns)
            {
                if (!prefab) { continue; } // skip blank entries in our prefab list
                returnList.Add(InstantiateSpecialFXGraphic(prefab, origin, parentToOrigin));
            }
            return returnList;
        }

        /// Utility function that instantiates one of the graphics from the Spawns list.
        /// If parentToOrigin is true, the new graphics are parented to the origin Transform.
        /// If false, they are positioned/oriented the same way but are not parented.
        protected SpecialFXGraphic InstantiateSpecialFXGraphic(GameObject prefab, Transform origin, bool parentToOrigin)
        {
            if (prefab.GetComponent<SpecialFXGraphic>() == null)
            {
                throw new System.Exception($"One of the Spawns on action {this.name} does not have a SpecialFXGraphic component and can't be instantiated!");
            }
            var graphicsGO = GameObject.Instantiate(prefab, origin.transform.position, origin.transform.rotation, (parentToOrigin ? origin.transform : null));
            return graphicsGO.GetComponent<SpecialFXGraphic>();
        }

        //Called when the action is being "anticipated" on the client. For example, if you are the owner of a tank and you swing your hammer,
        //you get this call immediately on the client, before the server round-trip.
        //Overriders should always call the base class in their implementation!
        public virtual void AnticipateActionClient(ClientCharacter clientCharacter)
        {
            AnticipatedClient = true;
            TimeStarted = UnityEngine.Time.time;

            if (!string.IsNullOrEmpty(Config.AnimAnticipation))
            {
                clientCharacter.PlayerAnimator.SetTrigger(Config.AnimAnticipation);
            }
        }

    }
}
