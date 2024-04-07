using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.Gameplay.GameplayObjects.Projectile;
using Project_RunningFighter.Infrastruture;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action
{
    /// Action responsible for creating a projectile object.
    [CreateAssetMenu(menuName = "Actions/Launch Projectile Action")]
    public class LaunchProjectileAction : GameAction
    {
        private bool m_Launched = false;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            //snap to face the direction we're firing, and then broadcast the animation, which we do immediately.
            serverCharacter.physicsWrapper.Transform.forward = Data.Direction;

            serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.clientCharacter.ClientPlayActionRpc(Data);
            return true;
        }

        public override void Reset()
        {
            m_Launched = false;
            base.Reset();
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            if (TimeRunning >= Config.ExecTimeSeconds && !m_Launched)
            {
                LaunchProjectile(clientCharacter);
            }

            return true;
        }

        protected virtual ProjectTileInfo GetProjectileInfo()
        {
            foreach (var projectileInfo in Config.Projectiles)
            {
                if (projectileInfo.ProjectilePrefab && projectileInfo.ProjectilePrefab.GetComponent<PhysicsProjecttile>())
                    return projectileInfo;
            }
            throw new System.Exception($"Action {name} has no usable Projectiles!");
        }

        protected void LaunchProjectile(ServerCharacter parent)
        {
            if (!m_Launched)
            {
                m_Launched = true;

                var projectileInfo = GetProjectileInfo();

                NetworkObject no = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.ProjectilePrefab, projectileInfo.ProjectilePrefab.transform.position, projectileInfo.ProjectilePrefab.transform.rotation);
                no.transform.forward = parent.physicsWrapper.Transform.forward;

                no.transform.position = parent.physicsWrapper.Transform.localToWorldMatrix.MultiplyPoint(no.transform.position);

                no.GetComponent<PhysicsProjecttile>().Initialize(parent.NetworkObjectId, projectileInfo);

                no.Spawn(true);
            }
        }

        public override void End(ServerCharacter serverCharacter)
        {
            //make sure this happens.
            LaunchProjectile(serverCharacter);
        }

        public override void Cancel(ServerCharacter serverCharacter)
        {
            if (!string.IsNullOrEmpty(Config.Anim2))
            {
                serverCharacter.serverAnimationController.NetworkAnimator.SetTrigger(Config.Anim2);
            }
        }

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            return ActionConclusion.Continue;
        }

    }
}
