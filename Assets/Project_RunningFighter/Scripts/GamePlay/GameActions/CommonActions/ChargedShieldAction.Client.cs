using Project_RunningFighter.Gameplay.GameplayObjects.Characters;
using Project_RunningFighter.VFX;
using System;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.Action
{
    public partial class ChargedShieldAction
    {
        
        SpecialFXGraphic m_ChargeGraphics;

        SpecialFXGraphic m_ShieldGraphics;

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            return IsChargingUp() || (Time.time - m_StoppedChargingUpTime) < Config.EffectDurationSeconds;
        }

        public override void CancelClient(ClientCharacter clientCharacter)
        {
            if (IsChargingUp())
            {
                if (m_ChargeGraphics)
                {
                    m_ChargeGraphics.Shutdown();
                }
            }

            if (m_ShieldGraphics)
            {
                m_ShieldGraphics.Shutdown();
            }
        }

        public override void OnStoppedChargingUpClient(ClientCharacter clientCharacter, float finalChargeUpPercentage)
        {
            if (!IsChargingUp()) { return; }

            m_StoppedChargingUpTime = Time.time;
            if (m_ChargeGraphics)
            {
                m_ChargeGraphics.Shutdown();
                m_ChargeGraphics = null;
            }

            // if fully charged, we show a special graphic
            if (Mathf.Approximately(finalChargeUpPercentage, 1))
            {
                m_ShieldGraphics = InstantiateSpecialFXGraphic(Config.Spawns[1], clientCharacter.transform, true);
            }
        }

        public override void AnticipateActionClient(ClientCharacter clientCharacter)
        {
            clientCharacter.PlayerAnimator.ResetTrigger(Config.Anim2);
            base.AnticipateActionClient(clientCharacter);
        }
    }
}
