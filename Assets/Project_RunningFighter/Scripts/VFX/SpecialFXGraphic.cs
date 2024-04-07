using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project_RunningFighter.VFX
{
    public class SpecialFXGraphic : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Particles that should be stopped on Shutdown")]
        public List<ParticleSystem> m_ParticleSystemsToTurnOffOnShutdown;

        [SerializeField]
        [Tooltip("If this graphic should automatically Shutdown after a certain time, set it here (in seconds). -1 means no auto-shutdown.")]
        private float m_AutoShutdownTime = -1;

        [SerializeField]
        [Tooltip("After Shutdown, how long before we self-destruct? 0 means no self destruct. -1 means self-destruct after ALL particles have disappeared")]
        private float m_PostShutdownSelfDestructTime = -1;

        [SerializeField]
        [Tooltip("If this graphic should keep its spawn rotation during its lifetime.")]
        bool m_StayAtSpawnRotation;

        private bool m_IsShutdown = false;

        private Coroutine coroWaitForSelfDestruct = null;

        Quaternion m_StartRotation;

        private void Start()
        {
            m_StartRotation = transform.rotation;

            if (m_AutoShutdownTime != -1)
            {
                coroWaitForSelfDestruct = StartCoroutine(CoroWaitForSelfDestruct());
            }
        }

        public void Shutdown()
        {
            if (!m_IsShutdown)
            {
                foreach (var particleSystem in m_ParticleSystemsToTurnOffOnShutdown)
                {
                    if (particleSystem)
                    {
                        particleSystem.Stop();
                    }
                }

                if (m_PostShutdownSelfDestructTime >= 0)
                {
                    Destroy(gameObject, m_PostShutdownSelfDestructTime);
                }
                else if (m_PostShutdownSelfDestructTime == -1)
                {
                    StartCoroutine(CoroWaitForParticlesToEnd());
                }

                m_IsShutdown = true;
            }
        }

        private IEnumerator CoroWaitForParticlesToEnd()
        {
            bool foundAliveParticles;
            do
            {
                yield return new WaitForEndOfFrame();
                foundAliveParticles = false;
                foreach (var particleSystem in m_ParticleSystemsToTurnOffOnShutdown)
                {
                    if (particleSystem.IsAlive())
                    {
                        foundAliveParticles = true;
                    }
                }
            } while (foundAliveParticles);

            if (coroWaitForSelfDestruct != null)
            {
                StopCoroutine(coroWaitForSelfDestruct);
            }

            Destroy(gameObject);
            yield break;
        }

        private IEnumerator CoroWaitForSelfDestruct()
        {
            yield return new WaitForSeconds(m_AutoShutdownTime);
            coroWaitForSelfDestruct = null;
            if (!m_IsShutdown)
            {
                Shutdown();
            }
        }

        void Update()
        {
            if (m_StayAtSpawnRotation)
            {
                transform.rotation = m_StartRotation;
            }
        }
    }


#if UNITY_EDITOR
    /// A custom editor that provides a button in the Inspector to auto-add all the
    /// particle systems in a SpecialFXGraphic (so we don't have to manually maintain the list).
    [CustomEditor(typeof(SpecialFXGraphic))]
    public class SpecialFXGraphicEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Auto-Add All Particle Systems"))
            {
                AddAllParticleSystems((SpecialFXGraphic)target);
            }
        }

        private void AddAllParticleSystems(SpecialFXGraphic specialFxGraphic)
        {
            if (specialFxGraphic.m_ParticleSystemsToTurnOffOnShutdown == null)
            {
                specialFxGraphic.m_ParticleSystemsToTurnOffOnShutdown = new List<ParticleSystem>();
            }

            specialFxGraphic.m_ParticleSystemsToTurnOffOnShutdown.Clear();
            foreach (var particleSystem in specialFxGraphic.GetComponentsInChildren<ParticleSystem>())
            {
                specialFxGraphic.m_ParticleSystemsToTurnOffOnShutdown.Add(particleSystem);
            }
        }
    }
#endif
}
