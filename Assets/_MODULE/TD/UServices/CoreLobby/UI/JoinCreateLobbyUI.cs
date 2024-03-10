using System.Collections;
using System.Collections.Generic;
using TD.UServices.Core;
using UnityEngine;
using UnityEngine.Events;

namespace TD.UServices.CoreLobby.UI
{
    public enum JoinCreateTabs
    {
        Join,
        Create
    }
    /// <summary>
    /// The panel that holds the lobby joining and creation panels.
    /// </summary>
    public class JoinCreateLobbyUI : UIPanelBase
    {
        [HideInInspector]
        public UnityEvent<JoinCreateTabs> m_OnTabChanged;

        [SerializeField] //Serialized for Visisbility in Editor
        JoinCreateTabs m_CurrentTab;

        public JoinCreateTabs CurrentTab
        {
            get => m_CurrentTab;
            set
            {
                m_CurrentTab = value;
                m_OnTabChanged?.Invoke(m_CurrentTab);
            }
        }

        public void SetJoinTab()
        {
            CurrentTab = JoinCreateTabs.Join;
        }

        public void SetCreateTab()
        {
            CurrentTab = JoinCreateTabs.Create;
        }

        void GameStateChanged(CoreGameManager.GameState state)
        {
            if (state == CoreGameManager.GameState.JoinMenu)
            {
                m_OnTabChanged?.Invoke(m_CurrentTab);
                Show(false);
            }
            else
            {
                Hide();
            }
        }

        public override void Start()
        {
            base.Start();
            Manager.onGameStateChanged += GameStateChanged;
        }

        void OnDestroy()
        {
            if (Manager == null)
                return;
            Manager.onGameStateChanged -= GameStateChanged;
        }
    }
}


