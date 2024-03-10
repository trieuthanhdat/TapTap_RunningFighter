using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TD.UServices.Core;
using TD.UServices.CoreLobby.Infrastructure;
using TD.UServices.CoreLobby.UI;
using TMPro;
using UnityEngine;

namespace TD.UServices.CoreLobby.UI
{
    public class RelayAddressUI : UIPanelBase
    {
        [SerializeField]
        TMP_Text m_IPAddressText;

        public override void Start()
        {
            base.Start();
            CoreGameManager.Instance.LocalLobby.RelayServer.onChanged += GotRelayAddress;
        }

        void GotRelayAddress(ServerAddress address)
        {
            m_IPAddressText.SetText(address.ToString());
        }
    }
}
