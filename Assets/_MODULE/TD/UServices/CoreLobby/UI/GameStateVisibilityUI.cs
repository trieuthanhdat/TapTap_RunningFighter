using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.UI;
using UnityEngine;

namespace TD.UServices.CoreLobby.UI
{
    public class GameStateVisibilityUI : UIPanelBase
    {
        [SerializeField]
        protected Core.CoreGameManager.GameState ShowThisWhen;

        void GameStateChanged(Core.CoreGameManager.GameState state)
        {
            if (!ShowThisWhen.HasFlag(state))
                Hide();
            else
                Show();
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
