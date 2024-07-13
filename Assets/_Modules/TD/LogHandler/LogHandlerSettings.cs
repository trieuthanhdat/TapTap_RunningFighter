using UnityEngine;
using TD.Utilities;
using System.Collections;
using System.Collections.Generic;

namespace LogHandler
{
    
    public class LogHandlerSettings : MonoSingleton<LogHandlerSettings>
    {
        [SerializeField]
        [Tooltip("Only logs of this level or higher will appear in the console.")]
        private LogMode m_editorLogVerbosity = LogMode.Critical;

        [SerializeField] private float delayInitTime = 1f;
        //[SerializeField]
        //private LobbyPopupUI m_popUp;

        private void OnEnable()
        {
            StartCoroutine(Co_Init());
        }
        IEnumerator Co_Init()
        {
            yield return new WaitForSeconds(delayInitTime);
            LogHandler.Get().mode = m_editorLogVerbosity;
            Debug.Log($"Starting project with Log Level : {m_editorLogVerbosity.ToString()}");
        }
        public void SetLogMode(string logMode)
        {
            if(EnumUtils.TryParse(logMode, out LogMode mode))
            {
                m_editorLogVerbosity = mode;
                OnValidate();
            }
        }
        /// For convenience while in the Editor, update the log verbosity when its value is changed in the Inspector.
        public void OnValidate()
        {
            LogHandler.Get().mode = m_editorLogVerbosity;
        }

        //public void SpawnErrorPopup(string errorMessage)
        //{
        //    m_popUp.ShowPopup(errorMessage);
        //}
    }
}
