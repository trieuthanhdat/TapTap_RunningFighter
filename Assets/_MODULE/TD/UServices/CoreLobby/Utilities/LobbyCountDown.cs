using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.Infrastructure;
using UnityEngine;

namespace TD.UServices.CoreLobby.Utilities
{
    [RequireComponent(typeof(UI.LobbyCountDownUI))]
    public class LobbyCountDown : MonoBehaviour
    {
        CallbackValue<float> TimeLeft = new CallbackValue<float>();

        private UI.LobbyCountDownUI m_ui;
        private const int k_countdownTime = 4;

        public void OnEnable()
        {
            if (m_ui == null)
                m_ui = GetComponent<UI.LobbyCountDownUI>();
            TimeLeft.onChanged += m_ui.OnTimeChanged;
            TimeLeft.Value = -1;
        }

        public void StartCountDown()
        {
            TimeLeft.Value = k_countdownTime;
        }

        public void CancelCountDown()
        {
            TimeLeft.Value = -1;
        }

        public void Update()
        {
            if (TimeLeft.Value < 0)
                return;
            TimeLeft.Value -= Time.deltaTime;
            //if (TimeLeft.Value < 0)
               // GameManager.Instance.FinishedCountDown();
        }
    }
}

