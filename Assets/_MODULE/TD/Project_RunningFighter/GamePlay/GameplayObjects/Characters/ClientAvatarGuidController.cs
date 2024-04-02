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
            if (m_GraphicsAnimator.transform.childCount > 0)
            {
                // we may receive a NetworkVariable's OnValueChanged callback more than once as a client
                // this makes sure we don't spawn a duplicate graphics GameObject
                return;
            }

            // spawn avatar graphics GameObject
            Instantiate(m_NetworkAvatarGuidState.RegisteredAvatar.Graphics, m_GraphicsAnimator.transform);

            m_GraphicsAnimator.Rebind();
            m_GraphicsAnimator.Update(0f);

            AvatarGraphicsSpawned?.Invoke(m_GraphicsAnimator.gameObject);
        }
    }
}
