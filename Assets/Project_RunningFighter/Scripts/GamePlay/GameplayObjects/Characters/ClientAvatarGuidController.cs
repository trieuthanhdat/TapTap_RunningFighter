using System;
using Unity.Netcode;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.GameplayObjects.Characters
{
    [RequireComponent(typeof(NetworkAvatarGuidState))]
    public class ClientAvatarGuidController : NetworkBehaviour
    {
        [SerializeField]
        Animator m_GraphicsAnimator;

        [SerializeField]
        NetworkAvatarGuidState m_NetworkAvatarGuidState;

        public Animator graphicsAnimator => m_GraphicsAnimator;

        public event Action<GameObject> AvatarGraphicsSpawned;

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                InstantiateAvatar();
            }
        }

        void InstantiateAvatar()
        {
            // spawn avatar graphics GameObject
            Instantiate(m_NetworkAvatarGuidState.RegisteredAvatar.Graphics, m_GraphicsAnimator.transform);
            Debug.Log("SPAWN CHAR GRAPHICS: "+ m_NetworkAvatarGuidState.RegisteredAvatar.Graphics.name);
            m_GraphicsAnimator.Rebind();
            m_GraphicsAnimator.Update(0f);

            AvatarGraphicsSpawned?.Invoke(m_GraphicsAnimator.gameObject);
        }
        private bool IsValidToSpawnAvatarGraphics()
        {
            foreach(Transform child in m_GraphicsAnimator.transform)
            {
                if (child.gameObject.activeSelf)
                    return false;
            }
            return true;
        }
    }
}
