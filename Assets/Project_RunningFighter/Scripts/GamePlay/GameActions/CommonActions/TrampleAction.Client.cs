using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.VFX;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action
{
    public partial class TrampleAction
    {
        /// We spawn the "visual cue" graphics a moment after we begin our action.
        /// (A little extra delay helps ensure we have the correct orientation for the
        /// character, so the graphics are oriented in the right direction!)
        private const float k_GraphicsSpawnDelay = 0.3f;

        private List<SpecialFXGraphic> m_SpawnedGraphics = null;

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            float age = Time.time - TimeStarted;
            if (age > k_GraphicsSpawnDelay && m_SpawnedGraphics == null)
            {
                m_SpawnedGraphics = InstantiateSpecialFXGraphics(clientCharacter.transform, false);
            }

            return true;
        }

        public override void CancelClient(ClientCharacter clientCharacter)
        {
            // we've been aborted -- destroy the "cue graphics"
            if (m_SpawnedGraphics != null)
            {
                foreach (var fx in m_SpawnedGraphics)
                {
                    if (fx)
                    {
                        fx.Shutdown();
                    }
                }
            }

            m_SpawnedGraphics = null;
        }
    }
}
