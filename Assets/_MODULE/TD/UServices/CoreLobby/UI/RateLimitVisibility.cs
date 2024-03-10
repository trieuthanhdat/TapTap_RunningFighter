using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.UI;
using TD.UServices.CoreLobby;
using UnityEngine;
using TD.UServices.Core;

namespace TD.UServices.CoreLobby.UI
{
    public class RateLimitVisibility : MonoBehaviour
    {
        [SerializeField] private UIPanelBase m_target;
        [SerializeField] private float m_alphaWhenHidden = 0.5f;
        [SerializeField] private LobbyManager.RequestType m_requestType;

        void Start()
        {
            CoreGameManager.Instance.LobbyManager.GetRateLimit(m_requestType).onCooldownChange += UpdateVisibility;
        }

        void OnDestroy()
        {
            if (CoreGameManager.Instance == null || CoreGameManager.Instance.LobbyManager == null)
                return;
            CoreGameManager.Instance.LobbyManager.GetRateLimit(m_requestType).onCooldownChange -= UpdateVisibility;
        }

        void UpdateVisibility(bool isCoolingDown)
        {
            if (isCoolingDown)
                m_target.Hide(m_alphaWhenHidden);
            else
                m_target.Show();
        }
    }
}
