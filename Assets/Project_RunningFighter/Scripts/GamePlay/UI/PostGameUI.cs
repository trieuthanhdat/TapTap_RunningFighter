using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Project_RunningFighter.Gameplay.UI
{
    /// Provides backing logic for all of the UI that runs in the PostGame stage.
    public class PostGameUI : MonoBehaviour
    {
        [SerializeField]
        private Light m_SceneLight;

        [SerializeField]
        private TextMeshProUGUI m_WinEndMessage;

        [SerializeField]
        private TextMeshProUGUI m_LoseGameMessage;

        [SerializeField]
        private GameObject m_ReplayButton;

        [SerializeField]
        private GameObject m_WaitOnHostMsg;

        [SerializeField]
        private Color m_WinLightColor;

        [SerializeField]
        private Color m_LoseLightColor;

        GameStates.ServerPostGameState m_PostGameState;

        [Inject]
        void Inject(GameStates.ServerPostGameState postGameState)
        {
            m_PostGameState = postGameState;

            // only hosts can restart the game, other players see a wait message
            if (NetworkManager.Singleton.IsHost)
            {
                m_ReplayButton.SetActive(true);
                m_WaitOnHostMsg.SetActive(false);
            }
            else
            {
                m_ReplayButton.SetActive(false);
                m_WaitOnHostMsg.SetActive(true);
            }
        }

        void Start()
        {
            m_PostGameState.NetworkPostGame.WinState.OnValueChanged += OnWinStateChanged;
            SetPostGameUI(m_PostGameState.NetworkPostGame.WinState.Value);
        }

        void OnDestroy()
        {
            if (m_PostGameState != null)
            {
                m_PostGameState.NetworkPostGame.WinState.OnValueChanged -= OnWinStateChanged;
            }
        }

        void OnWinStateChanged(GameStates.WinState previousValue, GameStates.WinState newValue)
        {
            SetPostGameUI(newValue);
        }

        void SetPostGameUI(GameStates.WinState winState)
        {
            switch (winState)
            {
                // Set end message and background color based last game outcome
                case GameStates.WinState.Win:
                    m_SceneLight.color = m_WinLightColor;
                    m_WinEndMessage.gameObject.SetActive(true);
                    m_LoseGameMessage.gameObject.SetActive(false);
                    break;
                case GameStates.WinState.Loss:
                    m_SceneLight.color = m_LoseLightColor;
                    m_WinEndMessage.gameObject.SetActive(false);
                    m_LoseGameMessage.gameObject.SetActive(true);
                    break;
                case GameStates.WinState.Invalid:
                    Debug.LogWarning("PostGameUI encountered Invalid WinState");
                    break;
            }
        }

        public void OnPlayAgainClicked()
        {
            m_PostGameState.PlayAgain();
        }

        public void OnMainMenuClicked()
        {
            m_PostGameState.GoToMainMenu();
        }
    }
}

