using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.Utils;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Project_RunningFighter.Gameplay.UI
{
    [DefaultExecutionOrder(300)]
    public class UIStateDisplayHandler : NetworkBehaviour
    {
        [SerializeField] bool m_DisplayHealth;
        [SerializeField] bool m_DisplayMana;
        [SerializeField] bool m_DisplayName;
        [SerializeField] UIStateDisplay m_UIStatePrefab;
        [SerializeField] NetworkHealthState m_NetworkHealthState;
        [SerializeField] NetworkManaState m_NetworkManaState;
        [SerializeField] NetworkNameState m_NetworkNameState;
        [SerializeField] int m_BaseHP;
        [SerializeField] int m_BaseMana;

        // spawned in world (only one instance of this)
        UIStateDisplay m_UIState;
        RectTransform m_UIStateRectTransform;
        bool m_UIStateActive;
        ServerCharacter m_ServerCharacter;
        ClientAvatarGuidController m_ClientAvatarGuidHandler;
        NetworkAvatarGuidState m_NetworkAvatarGuidState;


        [Tooltip("UI object(s) will appear positioned at this transforms position.")]
        [SerializeField] Transform m_TransformToTrack;

        UnityEngine.Camera m_Camera;
        Transform m_CanvasTransform;

        // as soon as any HP/MP goes to 0, we wait this long before removing health/Mana bar UI object
        const float k_DurationSeconds = 2f;

        [Tooltip("World space vertical offset for positioning.")]
        [SerializeField]
        float m_VerticalWorldOffset;

        [Tooltip("Screen space vertical offset for positioning.")]
        [SerializeField]
        float m_VerticalScreenOffset;

        Vector3 m_VerticalOffset;

        Vector3 m_WorldPos;

        void Awake()
        {
            m_ServerCharacter = GetComponent<ServerCharacter>();
        }

        public override void OnNetworkSpawn()
        {
            //NOT SURE YET
            if (!NetworkManager.Singleton.IsClient)
            {
                enabled = false;
                return;
            }

            var cameraGameObject = GameObject.FindWithTag("MainCamera");
            if (cameraGameObject)
            {
                m_Camera = cameraGameObject.GetComponent<UnityEngine.Camera>();
            }
            Assert.IsNotNull(m_Camera);

            var canvasGameObject = GameObject.FindWithTag("GameCanvas");
            if (canvasGameObject)
            {
                m_CanvasTransform = canvasGameObject.transform;
            }
            Assert.IsNotNull(m_CanvasTransform);

            Assert.IsTrue(m_DisplayHealth || m_DisplayName || m_DisplayMana, "Neither display fields are toggled on!");
            if (m_DisplayHealth)
            {
                Assert.IsNotNull(m_NetworkHealthState, "A NetworkHealthState component needs to be attached!");
            }
            if (m_DisplayMana)
            {
                Assert.IsNotNull(m_NetworkManaState, "A NetworkManaState component needs to be attached!");
            }

            m_VerticalOffset = new Vector3(0f, m_VerticalScreenOffset, 0f);

            // if PC, find our graphics transform and update health through callbacks, if displayed
            if (TryGetComponent(out m_ClientAvatarGuidHandler) && TryGetComponent(out m_NetworkAvatarGuidState))
            {
                m_BaseHP   = m_NetworkAvatarGuidState.RegisteredAvatar.CharacterClass.BaseHP;
                m_BaseMana = m_NetworkAvatarGuidState.RegisteredAvatar.CharacterClass.BaseMana;
                if (m_ServerCharacter.clientCharacter)
                {
                    TrackGraphicsTransform(m_ServerCharacter.clientCharacter.gameObject);
                }
                else
                {
                    m_ClientAvatarGuidHandler.AvatarGraphicsSpawned += TrackGraphicsTransform;
                }

                if (m_DisplayHealth)
                {
                    m_NetworkHealthState.HitPointsReplenished += DisplayUIHealth;
                    m_NetworkHealthState.HitPointsDepleted    += RemoveUIHealth;
                }
                if(m_DisplayMana)
                {
                    m_NetworkManaState.ManaPointsReplenished += DisplayUIMana;
                    m_NetworkManaState.ManaPointsDepleted    += RemoveUIMana;
                }
            }

            if (m_DisplayName)
            {
                DisplayUIName();
            }

            if (m_DisplayHealth)
            {
                DisplayUIHealth();
            }

            if(m_DisplayMana)
            {
                DisplayUIMana();
            }
        }

        

        void OnDisable()
        {
            if (!m_DisplayHealth)
            {
                return;
            }

            if (m_NetworkHealthState != null)
            {
                m_NetworkHealthState.HitPointsReplenished -= DisplayUIHealth;
                m_NetworkHealthState.HitPointsDepleted -= RemoveUIHealth;
            }

            if (m_ClientAvatarGuidHandler)
            {
                m_ClientAvatarGuidHandler.AvatarGraphicsSpawned -= TrackGraphicsTransform;
            }
        }
        private void DisplayUIMana()
        {
            if (m_NetworkManaState == null)
            {
                return;
            }

            if (m_UIState == null)
            {
                SpawnUIState();
            }

            m_UIState.DisplayMana(m_NetworkManaState.ManaPoints, m_BaseMana);
            m_UIStateActive = true;
        }
        void DisplayUIName()
        {
            if (m_NetworkNameState == null)
            {
                return;
            }

            if (m_UIState == null)
            {
                SpawnUIState();
            }

            m_UIState.DisplayName(m_NetworkNameState.Name);
            m_UIStateActive = true;
        }

        void DisplayUIHealth()
        {
            if (m_NetworkHealthState == null)
            {
                return;
            }

            if (m_UIState == null)
            {
                SpawnUIState();
            }

            m_UIState.DisplayHealth(m_NetworkHealthState.HitPoints, m_BaseHP);
            m_UIStateActive = true;
        }

        void SpawnUIState()
        {
            m_UIState = Instantiate(m_UIStatePrefab, m_CanvasTransform);
            // make in world UI state draw under other UI elements
            m_UIState.transform.SetAsFirstSibling();
            m_UIStateRectTransform = m_UIState.GetComponent<RectTransform>();
        }

        void RemoveUIHealth()
        {
            StartCoroutine(WaitToHideHealthBar());
        }
        void RemoveUIMana()
        {
            StartCoroutine(WaitToHideManaBar());
        }

        IEnumerator WaitToHideHealthBar()
        {
            yield return new WaitForSeconds(k_DurationSeconds);
            m_UIState.HideHealth();
        }
        IEnumerator WaitToHideManaBar()
        {
            yield return new WaitForSeconds(k_DurationSeconds);
            m_UIState.HideMana();
        }

        void TrackGraphicsTransform(GameObject graphicsGameObject)
        {
            m_TransformToTrack = graphicsGameObject.transform;
        }

        /// Moving UI objects on LateUpdate ensures that the game camera is at its final position pre-render.
        void LateUpdate()
        {
            if (m_UIStateActive && m_TransformToTrack)
            {
                // set world position with world offset added
                m_WorldPos.Set(m_TransformToTrack.position.x,
                    m_TransformToTrack.position.y + m_VerticalWorldOffset,
                    m_TransformToTrack.position.z);

                m_UIStateRectTransform.position = m_Camera.WorldToScreenPoint(m_WorldPos) + m_VerticalOffset;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (m_UIState != null)
            {
                Destroy(m_UIState.gameObject);
            }
        }
    }
}
