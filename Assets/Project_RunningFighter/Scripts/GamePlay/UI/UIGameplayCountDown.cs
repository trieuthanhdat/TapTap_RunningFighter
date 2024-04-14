using Project_RunningFighter.Gameplay.GameStates;
using System;
using System.Collections;
using System.Collections.Generic;
using TD.MonoAudioSFX;
using TMPro;
using UnityEngine;
using static Project_RunningFighter.Gameplay.UI.UIGameplayCountDown;

namespace Project_RunningFighter.Gameplay.UI
{
    public class UIGameplayCountDown : MonoBehaviour
    {
        public enum CountDownTimerType
        {
            Warning,
            PlayTime
        }
        private const string NUMBER_POPUP = "NumberPopup";

        [SerializeField] private TextMeshProUGUI m_CountdownText;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private CountDownTimerType m_CountDownTimerType;
        [SerializeField] private Animator animator;
        private int previousCountdownNumber;
        private bool _isVisible;
        #region ____UNITY METHODS____
        private void OnEnable()
        {
            NetworkedActionPhaseState.OnGameplayStateChanged += NetworkedActionPhaseState_OnGameplayStateChanged;
        }
        private void OnDisable()
        {
            NetworkedActionPhaseState.OnGameplayStateChanged -= NetworkedActionPhaseState_OnGameplayStateChanged;
        }
        private void Start()
        {
            Hide();
        }

        private void NetworkedActionPhaseState_OnGameplayStateChanged(ActionPhaseState newState)
        {
            switch (m_CountDownTimerType)
            {
                case CountDownTimerType.Warning:
                    SetVisibility(newState == ActionPhaseState.StartCountDown ||
                                  newState == ActionPhaseState.EndCountDown);
                    Debug.Log("UI GAMEPLAY COUNT DOWN: Warning => new state " + newState + " is visible "+_isVisible);
                    break;
                case CountDownTimerType.PlayTime:
                    SetVisibility(newState == ActionPhaseState.Playing ||
                                  newState == ActionPhaseState.EndCountDown);
                    Debug.Log("UI GAMEPLAY COUNT DOWN: PlayTime => new state " + newState + " is visible " + _isVisible);
                    break;
            }
        }

        private void Update()
        {
            if (!_isVisible)
            {
                Hide();
                return;
            }
            int countdownNumber = Mathf.CeilToInt(NetworkedActionPhaseState.Instance.GetCountDownTimer());
            m_CountdownText.text = FormatTime(countdownNumber);

            if (previousCountdownNumber != countdownNumber)
            {
                previousCountdownNumber = countdownNumber;
                if(m_CountDownTimerType == CountDownTimerType.Warning)
                {
                    if(animator) animator.SetTrigger(NUMBER_POPUP);
                    MonoAudioManager.Instance.PlaySound("Warning_1");
                }
            }
        }
        #endregion

        #region ____UTILITIES METHODS____
        private string FormatTime(float timeInSeconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);

            if (timeInSeconds >= 60)
            {
                // Format as mm:ss
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                // Format as ss
                return $"{timeSpan.Seconds:D1}";
            }
        }
        public void SetVisibility(bool isVisible)
        {
            _isVisible = isVisible;
            if (isVisible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
        private void Show()
        {
            if(!m_CanvasGroup)
            {
                gameObject.SetActive(true);
            }else
            {
                m_CanvasGroup.alpha = 1.0f;
            }
        }

        private void Hide()
        {
            if (!m_CanvasGroup)
            {
                gameObject.SetActive(false);
            }
            else
            {
                m_CanvasGroup.alpha = 0f;
            }
        }
        #endregion
    }

}
