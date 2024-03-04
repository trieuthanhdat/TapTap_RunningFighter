using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.UI;
using TMPro;
using UnityEngine;


namespace TD.UServices.CoreLobby.UI
{
    public class LobbyCountDownUI : UIPanelBase
    {
        [SerializeField]
        TMP_Text m_CountDownText;

        public void OnTimeChanged(float time)
        {
            if (time <= 0)
                m_CountDownText.SetText("Waiting for all players...");
            else
                m_CountDownText.SetText($"Starting in: {time:0}"); // Note that the ":0" formatting rounds, not truncates.
        }
    }
}
