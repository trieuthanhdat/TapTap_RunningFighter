using TD.UServices.CoreLobby.Utilities;
using UnityEngine;

namespace TD.UServices.CoreLobby.Infrastructure
{
    /// <summary>
    /// Acts as a buffer between receiving requests to display error messages to the player and running the pop-up UI to do so.
    /// </summary>
    public class LogHandlerSettings : MonoSingleton<LogHandlerSettings>
    {
        [SerializeField]
        [Tooltip("Only logs of this level or higher will appear in the console.")]
        private LogMode m_editorLogVerbosity = LogMode.Critical;

        [SerializeField]
        private LobbyPopupUI m_popUp;

        private void Awake()
        {
            LogHandler.Get().mode = m_editorLogVerbosity;
            Debug.Log($"Starting project with Log Level : {m_editorLogVerbosity.ToString()}");
        }


        /// <summary>
        /// For convenience while in the Editor, update the log verbosity when its value is changed in the Inspector.
        /// </summary>
        public void OnValidate()
        {
            LogHandler.Get().mode = m_editorLogVerbosity;
        }


        public void SpawnErrorPopup(string errorMessage)
        {
            m_popUp.ShowPopup(errorMessage);
        }
    }
}
